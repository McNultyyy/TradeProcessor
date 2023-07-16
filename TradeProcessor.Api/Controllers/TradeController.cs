using Hangfire;
using Microsoft.AspNetCore.Mvc;
using TradeProcessor.Api.Contracts;

namespace TradeProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradeController : ControllerBase
    {

        private readonly ILogger<TradeController> _logger;

        private readonly FvgChaser.FvgChaser _fvgChaser;

        public TradeController(ILogger<TradeController> logger, FvgChaser.FvgChaser fvgChaser)
        {
            _logger = logger;
            _fvgChaser = fvgChaser;
        }

        [HttpPost(Name = "FvgChaser")]
        public IActionResult FvgChase(FvgChaserRequest request)
        {
            BackgroundJob
                .Enqueue(() =>
                    _fvgChaser.DoWork(
                        request.Symbol,
                        request.Interval,
                        request.RiskPerTrade,
                        request.MaxNumberOfTrades,
                        request.Stoploss,
                        request.TakeProfit,
                        request.Bias,
                        null
                        ));

            return Ok();
        }
    }
}
