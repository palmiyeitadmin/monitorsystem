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

# 2. Create Status Page (Minimal)
$statusPageUrl = "http://localhost:5000/api/StatusPages"
$slug = "test-page-$(Get-Date -Format 'HHmmss')"
$statusPageBody = @{
    name     = "Test Status Page"
    slug     = $slug
    isPublic = $true
    isActive = $true
} | ConvertTo-Json

Write-Host "`nCreating Status Page..."
try {
    $response = Invoke-WebRequest -Method Post -Uri $statusPageUrl -Headers $headers -Body $statusPageBody
    $statusPage = $response.Content | ConvertFrom-Json
    Write-Host "Status Page Created: $($statusPage.id) - $($statusPage.name)"
}
catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = [System.IO.StreamReader]::new($stream)
    $err = $reader.ReadToEnd()
    $err | Out-File -Encoding utf8 error.json
    Write-Error "Create Status Page Failed: $($_.Exception.Message) - Check error.json"
    exit 1
}

# 3. Add Component
$componentUrl = "$statusPageUrl/$($statusPage.id)/components"
$componentBody = @{
    name       = "API Server"
    type       = 0
    showUptime = $true
    order      = 1
} | ConvertTo-Json

Write-Host "`nAdding Component..."
try {
    $response = Invoke-WebRequest -Method Post -Uri $componentUrl -Headers $headers -Body $componentBody
    $component = $response.Content | ConvertFrom-Json
    Write-Host "Component Added: $($component.id) - $($component.name)"
}
catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = [System.IO.StreamReader]::new($stream)
    $err = $reader.ReadToEnd()
    Write-Error "Add Component Failed: $($_.Exception.Message) - $err"
    exit 1
}

# 4. Verify Public Access
$publicUrl = "http://localhost:5000/api/public/status/$slug"
Write-Host "`nVerifying Public Access ($publicUrl)..."
try {
    $response = Invoke-WebRequest -Method Get -Uri $publicUrl
    $publicPage = $response.Content | ConvertFrom-Json
    Write-Host "SUCCESS: Public Status Page retrieved. Name: $($publicPage.name)"
}
catch {
    Write-Error "Public Access Failed: $($_.Exception.Message)"
    exit 1
}
