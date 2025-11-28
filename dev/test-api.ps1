# Test CRM Backend API
Write-Host "Testing API at http://localhost:5018/api/customer/info/12345" -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5018/api/customer/info/12345" -Headers @{"Authorization"="Bearer 123"} -Method Get
    Write-Host "SUCCESS! Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Yellow
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} catch {
    Write-Host "ERROR: Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details:" -ForegroundColor Yellow
        Write-Host $_.ErrorDetails.Message
    }
}
