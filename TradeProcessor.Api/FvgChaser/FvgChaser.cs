using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Hangfire.Server;
using OneOf;
using System.ComponentModel;
using TradeProcessor.Api.Contracts;
using TradeProcessor.Api.Domain;
using TradeProcessor.Api.Domain.Stoploss;
using TradeProcessor.Api.Domain.TakeProfit;
using TradeProcessor.Api.Logging;

namespace TradeProcessor.Api.FvgChaser;

public class FvgChaser
{
    private BybitSocketClient _socketClient;
    private BybitRestClient _restclient;

    private ILogger<FvgChaser> _logger;

    private PerformContextLogger<FvgChaser> _performContextLogger;

    public FvgChaser(ILogger<FvgChaser> logger, IConfiguration configuration)
    {
        _logger = logger;

        var apiCredentials = new ApiCredentials(
            configuration["Bybit:Key"],
            configuration["Bybit:Secret"]);

        _socketClient = new BybitSocketClient(opts =>
        {
            opts.DerivativesPublicOptions.ApiCredentials = apiCredentials;
        });

        _restclient = new BybitRestClient(otps =>
        {
            otps.DerivativesOptions.ApiCredentials = apiCredentials;
        });
    }

    [DisplayName("{6} {0} {1}")]
    public async Task DoWork(string symbol,
        string interval,
        decimal riskPerTrade,
        decimal? maxNumberOfTrades,
        decimal stoploss,
        string? takeProfit,
        ImbalanceType bias,
        PerformContext context)
    {
        // note: The PerformContext object is inject automatically by the Hangfire library

        _performContextLogger = new PerformContextLogger<FvgChaser>(context, _logger);

        await DoWork(
            new FvgChaserRequest(
                symbol, interval, riskPerTrade, maxNumberOfTrades, stoploss, takeProfit, bias), context);
    }

    private async Task DoWork(FvgChaserRequest request, PerformContext context)
    {
        _logger.LogInformation("Received request: {request}", request);

        Candle current = null, previous = null, previousPrevious = null;
        int candleCount = 0;

        var interval = MapToKlineInterval(request.Interval);

        while (true)
        {
            await _socketClient
                .DerivativesApi
                .SubscribeToKlineUpdatesAsync(StreamDerivativesCategory.USDTPerp, request.Symbol, interval,
                    async updates =>
                    {
                        var data = updates.Data.First();

                        var candle = new Candle(data.OpenPrice, data.HighPrice, data.LowPrice, data.ClosePrice);

                        if (data.Confirm)
                        {
                            if (candleCount != 3)
                            {
                                // Initialise the first 3 candles

                                //third pass
                                if (previousPrevious is null && candleCount == 2)
                                {
                                    previousPrevious = previous with { };
                                    previous = current with { };
                                    current = candle with { };
                                    candleCount = 3;

                                    _performContextLogger.LogInformation("Loaded all 3 candles...");
                                    _performContextLogger.LogInformation("FVG strategy started");
                                }

                                // second pass
                                if (previous is null && candleCount == 1)
                                {
                                    previous = current with { };
                                    current = candle with { };
                                    candleCount = 2;


                                    _performContextLogger.LogInformation("Loaded second candle...");
                                }

                                // first pass
                                if (current is null)
                                {
                                    current = candle;
                                    candleCount = 1;


                                    _performContextLogger.LogInformation("Loaded first candle...");
                                }
                            }
                            else
                            {
                                previousPrevious = previous with { };
                                previous = current with { };
                                current = candle with { };
                            }

                            if (previousPrevious is not null &&
                                previous is not null &&
                                current is not null)
                            {
                                Imbalance imbalance;
                                if (TryFindImbalance(current, previous, previousPrevious, out imbalance))
                                {
                                    _performContextLogger.LogInformation($"Found imbalance: [{imbalance}]");

                                    if (imbalance.ImbalanceType == request.Bias)
                                    {
                                        var limitPrice = imbalance.ImbalanceType == ImbalanceType.Bullish
                                            ? imbalance.High
                                            : imbalance.Low;

                                        await PlaceLimitOrder(request, limitPrice);
                                    }
                                    else
                                    {
                                        _performContextLogger.LogInformation("Imbalance is not in the direction of bias - Ignored.");
                                    }
                                }
                            }
                        }
                    });
        }
    }

    private KlineInterval MapToKlineInterval(string requestInterval)
    {
        if (requestInterval.Contains("m"))
        {
            requestInterval = requestInterval.Replace("m", "");
            var integer = Int32.Parse(requestInterval);

            var integerInSeconds = integer * 60;

            return (KlineInterval)integerInSeconds;
        }

        if (requestInterval.Contains("H", StringComparison.InvariantCultureIgnoreCase))
        {
            requestInterval = requestInterval.Replace("H", "");
            var integer = Int32.Parse(requestInterval);

            var integerInSeconds = integer * 60 * 60;

            return (KlineInterval)integerInSeconds;
        }

        if (requestInterval.Contains("D"))
        {
            requestInterval = requestInterval.Replace("D", "");
            var integer = Int32.Parse(requestInterval);

            var integerInSeconds = integer * 60 * 60 * 24;

            return (KlineInterval)integerInSeconds;
        }

        throw new ArgumentException($"Cannot parse {requestInterval}", nameof(requestInterval));
    }

    async Task PlaceLimitOrder(FvgChaserRequest request, decimal limitPrice)
    {

        var quantity = //0.025m;
            Math.Round(request.RiskPerTrade / Math.Abs(limitPrice - request.Stoploss), 3);

        _performContextLogger.LogInformation("Setting limit order at: {limitPrice}", limitPrice);

        var takeProfitStrategy = GetTakeProfit(limitPrice, request);
        _performContextLogger.LogInformation("Using TakeProfitStrategy: {takeProfitStrategy}", takeProfitStrategy?.GetType().ToString());
        var takeProfit = takeProfitStrategy?.Result() ?? null;

        var stoplossStrategy = GetStoploss(limitPrice, request);
        _performContextLogger.LogInformation("Using StoplossStrategy: {takeProfitStrategy}", stoplossStrategy?.GetType().ToString());
        var stoploss = stoplossStrategy?.Result() ?? null;


        var orderResult = await _restclient.DerivativesApi.ContractApi.Trading.PlaceOrderAsync(
            request.Symbol,
            request.Bias == ImbalanceType.Bullish ? OrderSide.Buy : OrderSide.Sell,
            OrderType.Limit,
            quantity,
            TimeInForce.GoodTillCanceled,
            price: limitPrice,
            positionMode: request.Bias == ImbalanceType.Bullish ? PositionMode.BothSideBuy : PositionMode.BothSideSell,
            takeProfitPrice: takeProfit
        );

        if (!orderResult.Success)
        {

            _performContextLogger.LogError("Order failed with {error}", orderResult.Error.Message);
        }
        else
        {
            _performContextLogger.LogInformation("Order submitted successfully");
        }
    }

    bool TryFindImbalance(Candle current, Candle previous, Candle previousPrevious, out Imbalance imbalance)
    {

        if (//previous.IsBearishCandle())
            new[] { current, previous, previousPrevious }.All(x => x.IsBearishCandle()))  // all 3 bearish
                                                                                          //        || previousPrevious.IsBearishCandle() && previous.IsBearishCandle() && current.IsBullishCandle()) // first 2 bearish, last is bullish
        {
            if (current.High < previousPrevious.Low)
            {
                imbalance = new Imbalance(previousPrevious.Low, current.High, ImbalanceType.Bearish);
                return true;
            }
        }

        // all 3 bullish
        if (
                //previous.IsBullishCandle())
                new[] { current, previous, previousPrevious }.All(x => x.IsBullishCandle()))
        // || previousPrevious.IsBullishCandle() && previous.IsBullishCandle() && current.IsBearishCandle())) // first 2 bullish, last is bearish
        {
            if (current.Low > previousPrevious.High)
            {
                imbalance = new Imbalance(current.Low, previousPrevious.High, ImbalanceType.Bullish);
                return true;
            }
        }

        imbalance = null;
        return false;
    }



    private ITakeProfit? GetTakeProfit(decimal entryPrice, FvgChaserRequest request)
    {
        var takeProfit = request.TakeProfit;

        if (takeProfit is null)
            return null;

        if (takeProfit.Contains("+") || takeProfit.Contains("-"))
        {
            var tpString = takeProfit
                .Replace("+", "")
                .Replace("-", "");

            return new RelativeTakeProfit(entryPrice, decimal.Parse(tpString), request.Bias == ImbalanceType.Bullish);
        }

        if (takeProfit.Contains("R", StringComparison.InvariantCultureIgnoreCase))
        {
            var tpString = takeProfit
                .Replace("R", "");

            return new RiskRewardTakeProfit(entryPrice, request.Stoploss, decimal.Parse(tpString), request.Bias == ImbalanceType.Bullish);
        }

        if (takeProfit.Contains("%"))
        {
            var tpString = takeProfit
                .Replace("%", "");

            return new PercentageTakeProfit(decimal.Parse(tpString), entryPrice, request.Bias == ImbalanceType.Bullish);
        }

        return new StaticTakeProfit(decimal.Parse(takeProfit));
    }


    private IStoploss? GetStoploss(decimal entryPrice, FvgChaserRequest request)
    {
        var stoploss = request.TakeProfit;

        if (stoploss is null)
            return null;

        if (stoploss.Contains("+") || stoploss.Contains("-"))
        {
            var tpString = stoploss
                .Replace("+", "")
                .Replace("-", "");

            return new RelativeStoploss(entryPrice, decimal.Parse(tpString), request.Bias == ImbalanceType.Bullish);
        }

        if (stoploss.Contains("%"))
        {
            var tpString = stoploss
                .Replace("%", "");

            return new PercentageStoploss(decimal.Parse(tpString), entryPrice, request.Bias == ImbalanceType.Bullish);
        }

        return new StaticStoploss(decimal.Parse(stoploss));
    }




}
