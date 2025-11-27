# ERA Monitor - Docker Deployment Script (Windows)

Write-Host "=== ERA Monitor - Docker Deployment Script ===" -ForegroundColor Cyan
Write-Host ""

# Build all images
Write-Host "Step 1: Building Docker images..." -ForegroundColor Yellow
docker-compose build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build completed successfully!" -ForegroundColor Green
Write-Host ""

# Start services
Write-Host "Step 2: Starting services..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start services!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Services started!" -ForegroundColor Green
Write-Host ""

# Wait for health check
Write-Host "Step 3: Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Show status
Write-Host ""
Write-Host "=== Service Status ===" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "=== Deployment Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Dashboard:        http://localhost:3000" -ForegroundColor White
Write-Host "🔌 API:              http://localhost:5000" -ForegroundColor White
Write-Host "📚 Swagger:          http://localhost:5000/swagger" -ForegroundColor White
Write-Host "⚙️  Hangfire:         http://localhost:5000/hangfire" -ForegroundColor White
Write-Host "🗄️  PostgreSQL:       localhost:5432" -ForegroundColor White
Write-Host ""
Write-Host "Default Admin Credentials:" -ForegroundColor Yellow
Write-Host "  Email:    admin@eramonitor.com" -ForegroundColor White
Write-Host "  Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "To view logs:  docker-compose logs -f" -ForegroundColor Cyan
Write-Host "To stop:       docker-compose down" -ForegroundColor Cyan
Write-Host ""
