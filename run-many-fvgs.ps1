$path = "C:\Users\willi\Projects\tradingview-alerts-home directory\BinanceSpotWithOkxPerp.csv"
$apiKey = "123"

$localUrl = "https://localhost:7071";
$externalUrl = "https://tradeprocessor-api.azurewebsites.net";
$baseUrl = "$externalUrl/trade?apiKey=$apiKey"  # Replace with your actual API endpoint

$csvData = Import-Csv $path 

foreach ($line in $csvData) {

    $symbol = $line.symbol -Replace("BINANCE:" ,"")
    
    $body = @{
        apiKey       = "123";
        symbol       = "$symbol";
        interval     = "1D";
        riskPerTrade = "10";
        stoploss     = "fvg low";
        takeProfit   = "0.5R";
        bias         = "Bullish";
    } | ConvertTo-Json
    
    Write-Host "Sending Bullish request for '$symbol' ..."
    
    Invoke-RestMethod -Method Post -Uri $baseUrl -Body $body -ContentType 'application/json'
    
    Write-Host "Sleeping for 1 seconds ..."
    Start-Sleep 1;
    
    
    $body = @{
        apiKey       = "123";
        symbol       = "$symbol";
        interval     = "1D";
        riskPerTrade = "10";
        stoploss     = "fvg high";
        takeProfit   = "0.5R";
        bias         = "Bearish";
    } | ConvertTo-Json
    
    Write-Host "Sending Bearish request for '$symbol' ..."
    
    Invoke-RestMethod -Method Post -Uri $baseUrl -Body $body -ContentType 'application/json'
    
    Write-Host "Sleeping for 1 seconds ..."
    Start-Sleep 1;
}