$baseUri = [Uri]'http://localhost/CRMApi/'
$relativeUri = '/api/v1/ClientData/12345'
$fullUri = [Uri]::new($baseUri, $relativeUri)
Write-Host "Base URL: $($baseUri.AbsoluteUri)" -ForegroundColor Cyan
Write-Host "Relative: $relativeUri" -ForegroundColor Cyan
Write-Host "Full URL: $($fullUri.AbsoluteUri)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Testing the constructed URL..." -ForegroundColor Green
try {
    $result = Invoke-RestMethod -Uri $fullUri -Headers @{'Authorization'='Bearer 123'}
    Write-Host "SUCCESS!" -ForegroundColor Green
    $result | ConvertTo-Json
} catch {
    Write-Host "FAILED!" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
