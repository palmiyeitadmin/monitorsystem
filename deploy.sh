#!/bin/bash

echo "=== ERA Monitor - Docker Deployment Script ==="
echo ""

# Build all images
echo "Step 1: Building Docker images..."
docker-compose build

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✅ Build completed successfully!"
echo ""

# Start services
echo "Step 2: Starting services..."
docker-compose up -d

if [ $? -ne 0 ]; then
    echo "❌ Failed to start services!"
    exit 1
fi

echo "✅ Services started!"
echo ""

# Wait for health check
echo "Step 3: Waiting for database to be ready..."
sleep 5

# Show status
echo ""
echo "=== Service Status ==="
docker-compose ps

echo ""
echo "=== Deployment Complete! ==="
echo ""
echo "📊 Dashboard:        http://localhost:3000"
echo "🔌 API:              http://localhost:5000"
echo "📚 Swagger:          http://localhost:5000/swagger"
echo "⚙️  Hangfire:         http://localhost:5000/hangfire"
echo "🗄️  PostgreSQL:       localhost:5432"
echo ""
echo "Default Admin Credentials:"
echo "  Email:    admin@eramonitor.com"
echo "  Password: Admin123!"
echo ""
echo "To view logs:  docker-compose logs -f"
echo "To stop:       docker-compose down"
echo ""
