$path = "C:\Users\willi\Projects\tradingview-alerts-home directory\BinanceSpotWithOkxPerp.csv"
$apiKey = "123"

$localUrl = "https://localhost:7071";
$externalUrl = "https://tradeprocessor-api.azurewebsites.net";
$baseUrl = "$localUrl/trade?apiKey=$apiKey"  # Replace with your actual API endpoint


# $csvData = Import-Csv $path 

$data = @(
    'SOLUSDT', 'AVAXUSDT', 'LINKUSDT', 'BCHUSDT', 'BNBUSDT', 'DOGEUSDT',
    'FTMUSDT', 'CFXUSDT', 'OPUSDT', 'APTUSDT', 'DGBUSDT', 'RVNUSDT', 'APEUSDT',
        'CHZUSDT', 'HBARUSDT', 'DOTUSDT', 'RENUSDT'
)


foreach ($line in $data) {

    $symbol = $line; # when running from list
    #$symbol = $line.symbol -Replace("BINANCE:" ,"") # when running from csv
    
    $body = @{
        apiKey       = "123";
        symbol       = "$symbol";
        interval     = "1H";
        riskPerTrade = "20";
        stoploss     = "fvg high";
        setStoploss  =  $true;
        #takeProfit   = "1R";
        bias         = "Bearish";
    } | ConvertTo-Json
    
    Write-Host "Sending $($body.bias) request for '$symbol' ..."
    
    Invoke-RestMethod -Method Post -Uri $baseUrl -Body $body -ContentType 'application/json'
    sleep 1;
}