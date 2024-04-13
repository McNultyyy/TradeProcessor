$path = "C:\Users\willi\Projects\tradingview-alerts-home directory\BinanceSpotWithOkxPerp.csv"
$apiKey = "123"

$localUrl = "https://localhost:7071";
$externalUrl = "https://tradeprocessor-api.azurewebsites.net";
$baseUrl = "$localUrl/trade?apiKey=$apiKey"  # Replace with your actual API endpoint


#$csvData = Import-Csv $path 


#'SHIBUSDT', 'AIDOGEUSDT', 'KISHUUSDT', 'MEMEUSDT', 'ORDIUSDT', 'PEOPLEUSDT', 'TURBOUSDT','PEPEUSDT', 'DOGEUSDT',
#'BTCUSDT', 'ETHUSDT', 'MATICUSDT', 'SOLUSDT', 'XTZUSDT', 'LINKUSDT', 'AVAXUSDT', 'ADAUSDT', 'ALGOUSDT', 'ATOMUSDT'
#'RSRUSDT',

$symbol = 'NEOUSDT'
$timeframeConfigs = @(
    @{ "interval" =  "1m"; "riskPerTrade" =  "10"; "stoploss" = "1%";  },
    @{ "interval" =  "5m"; "riskPerTrade" =  "20"; "stoploss" = "2%";  },
    @{ "interval" =  "15m"; "riskPerTrade" =  "40"; "stoploss" = "3%";  },
    @{ "interval" =  "1H"; "riskPerTrade" =  "80"; "stoploss" = "6%";  }
)


foreach ($config in $timeframeConfigs) {
    
    #$symbol = $line.symbol -Replace("BINANCE:" ,"") # when running from csv
    
    $body = @{
        apiKey       = "123";
        symbol       = "$symbol";
        interval     = $($config.interval);
        riskPerTrade = $($config.riskPerTrade);
        stoploss     = $($config.stoploss);
        setStoploss  =  $false;
        takeProfit   = "1R";
        bias         = "Bullish";
        gaps         = @("Price", "Volume")
    } | ConvertTo-Json
    
    Write-Host "Sending $($body.bias) request for '$symbol' ..."
    
    Invoke-RestMethod -Method Post -Uri $baseUrl -Body $body -ContentType 'application/json'
    sleep 1;
}