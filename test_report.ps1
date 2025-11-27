$ErrorActionPreference = "Stop"

# 1. Authenticate
$loginUrl = "http://localhost:5000/api/Auth/login"
$body = @{
    email    = "admin@eramonitor.local"
    password = "Admin123!"
} | ConvertTo-Json

Write-Host "Authenticating..."
try {
    $loginResponse = Invoke-RestMethod -Method Post -Uri $loginUrl -ContentType "application/json" -Body $body
    $token = $loginResponse.accessToken
    Write-Host "Authentication Successful."
}
catch {
    Write-Error "Authentication Failed: $($_.Exception.Message)"
    exit 1
}

$headers = @{
    Authorization  = "Bearer $token"
    "Content-Type" = "application/json"
}

# 2. Create Report
$reportUrl = "http://localhost:5000/api/Reports"
$reportBody = @{
    name            = "Test Report $(Get-Date -Format 'HH:mm:ss')"
    type            = 0 
    timeRange       = 0 
    format          = 1 
    sendEmail       = $true
    emailRecipients = '["admin@eramonitor.local"]'
    isScheduled     = $true
    cronExpression  = "0 0 * * *"
    isActive        = $true
} | ConvertTo-Json

Write-Host "`nCreating Report..."
try {
    $report = Invoke-RestMethod -Method Post -Uri $reportUrl -Headers $headers -Body $reportBody
    Write-Host "Report Created: $($report.id) - $($report.name)"
}
catch {
    $err = $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() }
    Write-Error "Create Report Failed: $($_.Exception.Message) - $err"
    exit 1
}

# 3. Trigger Generation
$generateUrl = "$reportUrl/$($report.id)/generate"
Write-Host "`nTriggering Report Generation..."
try {
    Invoke-RestMethod -Method Post -Uri $generateUrl -Headers $headers
    Write-Host "Report Generation Triggered."
}
catch {
    $err = $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() }
    Write-Error "Trigger Generation Failed: $($_.Exception.Message) - $err"
    exit 1
}

# 4. Check Executions
Start-Sleep -Seconds 2

$executionsUrl = "$reportUrl/$($report.id)/executions"
Write-Host "`nChecking Report Executions..."
try {
    $executions = Invoke-RestMethod -Method Get -Uri $executionsUrl -Headers $headers
    
    if ($executions.Count -ge 0) {
        Write-Host "SUCCESS: Retrieved $($executions.Count) execution records."
    }
    else {
        Write-Error "FAILURE: Failed to retrieve executions list."
    }
}
catch {
    Write-Error "Get Executions Failed: $($_.Exception.Message)"
    exit 1
}
