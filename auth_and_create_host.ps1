$loginUrl = "http://localhost:5000/api/auth/login"
$createHostUrl = "http://localhost:5000/api/hosts"

$loginBody = @{
    email    = "admin@eramonitor.local"
    password = "Admin123!"
} | ConvertTo-Json

try {
    Write-Host "Authenticating..."
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -ContentType "application/json" -Body $loginBody
    
    # Write-Host "Response Type: $($loginResponse.GetType().Name)"
    # Write-Host "Full Response: $($loginResponse | ConvertTo-Json -Depth 5)"

    $token = $loginResponse.accessToken
    
    if (-not $token) {
        # Maybe it's directly returning the token string or different property?
        if ($loginResponse -is [string]) {
            $token = $loginResponse
        }
    }

    if (-not $token) {
        Write-Error "Failed to get token"
        exit 1
    }
    Write-Host "Authentication successful."

    $headers = @{
        Authorization = "Bearer $token"
    }

    $createHostBody = @{
        name                 = "Agent Test 2"
        hostname             = "localhost"
        osType               = 1 # Windows
        checkIntervalSeconds = 60
        monitoringEnabled    = $true
    } | ConvertTo-Json

    Write-Host "Creating host..."
    $hostResponse = Invoke-RestMethod -Uri $createHostUrl -Method Post -ContentType "application/json" -Headers $headers -Body $createHostBody
    
    Write-Host "Host Created Successfully"
    Write-Host "HOST_API_KEY: $($hostResponse.apiKey)"
    Write-Host "HOST_ID: $($hostResponse.id)"
    
    # Save token to file for easy reading
    $hostResponse.apiKey | Out-File -FilePath host_token.txt -Encoding ASCII
}
catch {
    Write-Error "Error occurred:"
    Write-Error $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Error $reader.ReadToEnd()
    }
}
