# ERA Monitor - Kapsamlı Sunucu İzleme Sistemi

Modern, ölçeklenebilir ve kullanıcı dostu sunucu izleme ve yönetim platformu.

## 🚀 Özellikler

### Backend (ASP.NET Core)
- ✅ RESTful API
- ✅ JWT Authentication
- ✅ PostgreSQL Database
- ✅ Entity Framework Core
- ✅ Background Jobs (Hangfire)
- ✅ Multi-tenant Architecture

### Frontend (Next.js)
- ✅ Modern React UI
- ✅ Tailwind CSS v4
- ✅ Server Components
- ✅ Responsive Design
- ✅ Dark Mode

### Monitoring Agent (Go)
- ✅ Cross-platform (Windows/Linux/macOS)
- ✅ GUI (Fyne) ve CLI modları
- ✅ Sistem metrikleri (CPU, RAM, Disk)
- ✅ Servis izleme (Windows Services, Systemd, Docker, IIS)
- ✅ Otomatik heartbeat

## 📋 Sistem Gereksinimleri

### Backend
- .NET 8.0 SDK
- PostgreSQL 14+
- Redis (opsiyonel, caching için)

### Frontend
- Node.js 18+
- npm veya yarn

### Agent
- Go 1.24+
- GCC (Windows için MinGW-w64)

## 🛠️ Kurulum

### 1. Database Setup

```bash
# PostgreSQL'de database oluştur
createdb era_monitor

# Connection string'i appsettings.json'a ekle
```

### 2. Backend

```bash
cd src/ERAMonitor.API
dotnet restore
dotnet ef database update
dotnet run
```

### 3. Frontend

```bash
cd dashboard
npm install
npm run dev
```

### 4. Agent

```bash
cd era-monitor-agent

# Windows
$env:CGO_ENABLED="1"
go build -o era-agent-gui.exe ./cmd/agent-gui

# Linux
export CGO_ENABLED=1
go build -o era-agent ./cmd/agent
```

## 📁 Proje Yapısı

```
monitorsystem/
├── src/
│   ├── ERAMonitor.API/              # ASP.NET Core Web API
│   ├── ERAMonitor.Core/             # Domain Models & Interfaces
│   ├── ERAMonitor.Infrastructure/   # Data Access & Services
│   └── ERAMonitor.BackgroundJobs/   # Hangfire Background Jobs
├── dashboard/                        # Next.js Frontend
└── era-monitor-agent/               # Go Monitoring Agent
    ├── cmd/
    │   ├── agent/                   # CLI Agent
    │   └── agent-gui/               # GUI Agent
    └── internal/
        ├── agent/                   # Core Logic
        ├── collectors/              # Metric Collectors
        ├── config/                  # Configuration
        └── gui/                     # Fyne GUI
```

## 🔧 Konfigürasyon

### API (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=era_monitor;Username=postgres;Password=***"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-here",
    "Issuer": "ERAMonitor",
    "Audience": "ERAMonitor",
    "ExpirationMinutes": 60
  }
}
```

### Agent (config.yaml)

```yaml
server:
  apiEndpoint: http://localhost:5000/api
  apiKey: YOUR_API_KEY

host:
  displayName: MyServer-01
  location: Istanbul

collectors:
  intervalSeconds: 60
```

## 🎯 Kullanım

### İlk Kullanıcı Oluşturma

```sql
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FullName", "Role", "OrganizationId", "IsActive", "CreatedAt")
VALUES (
  gen_random_uuid(),
  'admin@eramonitor.local',
  'hashed_password',
  'System Administrator',
  2, -- Admin
  'org-id',
  true,
  NOW()
);
```

### Agent'ı Başlatma

```bash
# GUI Mode (Windows)
.\era-agent-gui.exe

# CLI Mode (Linux)
./era-agent --config config.yaml
```

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/refresh-token` - Token yenileme
- `GET /api/auth/me` - Kullanıcı bilgileri

### Monitoring
- `POST /api/agent/heartbeat` - Agent heartbeat
- `GET /api/servers` - Sunucu listesi
- `GET /api/servers/{id}` - Sunucu detayları

### Status Pages
- `GET /api/statuspages` - Status page listesi
- `POST /api/statuspages` - Yeni status page
- `GET /api/statuspages/{slug}` - Public status page

## 🔐 Güvenlik

- JWT token authentication
- Role-based access control (SuperAdmin, Admin, Operator, Viewer)
- Multi-tenant data isolation
- API key authentication for agents
- HTTPS zorunlu (production)

## 🧪 Test

```bash
# Backend tests
cd src/ERAMonitor.API
dotnet test

# Frontend tests
cd dashboard
npm test

# Agent tests
cd era-monitor-agent
go test ./...
```

## 📦 Deployment

### Docker Compose

```bash
docker-compose up -d
```

### Manual Deployment

1. Backend'i publish et: `dotnet publish -c Release`
2. Frontend'i build et: `npm run build`
3. Agent'ı compile et: `go build -o era-agent`
4. Reverse proxy kur (nginx/caddy)
5. SSL sertifikası ekle

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📝 Lisans

Bu proje özel lisans altındadır.

## 📧 İletişim

ERA Monitor Team - support@eramonitor.com

## 🙏 Teşekkürler

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [Next.js](https://nextjs.org/)
- [Fyne](https://fyne.io/)
- [PostgreSQL](https://www.postgresql.org/)
- [Tailwind CSS](https://tailwindcss.com/)
