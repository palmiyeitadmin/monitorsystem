$loginUrl = "http://localhost:5000/api/auth/login"
$hostId = "67f88f08-3537-4d97-9144-f68cbc8e58a7"
$hostUrl = "http://localhost:5000/api/hosts/$hostId"

# 1. Authenticate
$loginBody = @{
    email    = "admin@eramonitor.local"
    password = "Admin123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.accessToken
    Write-Host "Authenticated. Token obtained."
}
catch {
    Write-Error "Authentication failed: $_"
    exit 1
}

# 2. Get Host Details
$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $hostResponse = Invoke-RestMethod -Uri $hostUrl -Method Get -Headers $headers
    Write-Host "Host Details:"
    $hostResponse | ConvertTo-Json -Depth 10
}
catch {
    Write-Error "Failed to get host details: $_"
    exit 1
}
