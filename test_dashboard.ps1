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
    Write-Host "Authentication Successful. Token obtained."
}
catch {
    Write-Error "Authentication Failed: $($_.Exception.Message)"
    exit 1
}

$headers = @{
    Authorization  = "Bearer $token"
    "Content-Type" = "application/json"
}

# 2. Create Dashboard
$dashboardUrl = "http://localhost:5000/api/Dashboard"
$dashboardBody = @{
    title       = "Test Dashboard $(Get-Date -Format 'HH:mm:ss')"
    description = "Automated test dashboard"
    isDefault   = $false
    visibility  = 0 # Private
} | ConvertTo-Json

Write-Host "`nCreating Dashboard..."
try {
    $dashboard = Invoke-RestMethod -Method Post -Uri $dashboardUrl -Headers $headers -Body $dashboardBody
    Write-Host "Dashboard Created: $($dashboard.id) - $($dashboard.title)"
}
catch {
    $err = $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() }
    Write-Error "Create Dashboard Failed: $($_.Exception.Message) - $err"
    exit 1
}

# 3. Add Widget
$widgetUrl = "$dashboardUrl/$($dashboard.id)/widgets"
$widgetBody = @{
    title         = "Uptime Chart"
    type          = 0 # UptimeChart
    positionX     = 0
    positionY     = 0
    width         = 6
    height        = 4
    configuration = "{}"
} | ConvertTo-Json

Write-Host "`nAdding Widget..."
try {
    $widget = Invoke-RestMethod -Method Post -Uri $widgetUrl -Headers $headers -Body $widgetBody
    Write-Host "Widget Added: $($widget.id) - $($widget.title)"
}
catch {
    $err = $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() }
    Write-Error "Add Widget Failed: $($_.Exception.Message) - $err"
    exit 1
}

# 4. Verify Dashboard Details
Write-Host "`nVerifying Dashboard Details..."
try {
    $verifyDashboard = Invoke-RestMethod -Method Get -Uri "$dashboardUrl/$($dashboard.id)" -Headers $headers
    
    if ($verifyDashboard.widgets.Count -gt 0) {
        Write-Host "SUCCESS: Dashboard retrieved with $($verifyDashboard.widgets.Count) widget(s)."
    }
    else {
        Write-Error "FAILURE: Dashboard retrieved but has no widgets."
    }
}
catch {
    Write-Error "Get Dashboard Failed: $($_.Exception.Message)"
    exit 1
}
