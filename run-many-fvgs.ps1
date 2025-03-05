$path = "C:\Users\willi\Projects\tradingview-alerts-home directory\BinanceSpotWithOkxPerp.csv"
$apiKey = "123"

$localUrl = "https://localhost:7071";
$externalUrl = "https://tradeprocessor-api.azurewebsites.net";
$baseUrl = "$localUrl/trade?apiKey=$apiKey"  # Replace with your actual API endpoint


 $csvData = Import-Csv $path 

$data = @(
    'SOLUSDT',
    'AVAXUSDT',
    'JSTUSDT',
    'BONKUSDT',
    'AUCTIONUSDT',
    'BCHUSDT',
    'BANDUSDT',
    'API3USDT',
    'WOOUSDT',
    'YGGUSDT'
)

foreach ($line in $data) {

    $symbol = $line; # when running from list
    #$symbol = $line.symbol -Replace("BINANCE:" ,"") # when running from csv
    
    $body = @{
        apiKey       = "123";
        symbol       = "$symbol";
        interval     = "1H";
        riskPerTrade = "10";
        stoploss     = "fvg high";
        setStoploss  =  $false;
        takeProfit   = "3R";
        bias         = "Bullish";
    } | ConvertTo-Json
    
    Write-Host "Sending $($body.bias) request for '$symbol' ..."
    
    Invoke-RestMethod -Method Post -Uri $baseUrl -Body $body -ContentType 'application/json'
    sleep 1;
}