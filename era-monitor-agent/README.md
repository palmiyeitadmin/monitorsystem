# ERA Monitor Agent

ERA Monitor sistemi için Go tabanlı cross-platform monitoring agent'ı.

## Özellikler

- ✅ **Cross-Platform**: Windows, Linux ve macOS desteği
- ✅ **Sistem İzleme**: CPU, RAM, Disk kullanımı
- ✅ **Servis İzleme**: 
  - Windows Services
  - Systemd Units (Linux)
  - Docker Containers
  - IIS Sites ve App Pools
- ✅ **GUI Arayüz**: Fyne framework ile modern masaüstü arayüzü
- ✅ **Otomatik Login**: Kullanıcı adı/şifre ile API key alma
- ✅ **Gerçek Zamanlı**: 60 saniyede bir otomatik heartbeat gönderimi
- ✅ **System Tray**: Arka planda çalışma desteği

## Kurulum

### Gereksinimler

- Go 1.24+
- GCC (Windows için MinGW-w64)
- CGO_ENABLED=1

### Windows için Build

```powershell
$env:Path += ";C:\mingw64\bin"
$env:CGO_ENABLED="1"
go build -o era-agent-gui.exe ./cmd/agent-gui
```

### Linux için Build

```bash
export CGO_ENABLED=1
go build -o era-agent ./cmd/agent
```

## Kullanım

### GUI Modu (Windows)

```powershell
.\era-agent-gui.exe
```

### CLI Modu (Linux/Server)

```bash
./era-agent --config config.yaml
```

## Konfigürasyon

`config.yaml` dosyası örneği:

```yaml
server:
  apiEndpoint: http://localhost:5000/api
  apiKey: YOUR_API_KEY_HERE
  timeout: 30
  retryCount: 3
  retryDelay: 5

host:
  displayName: MyServer-01
  location: Istanbul
  tags:
    - production
    - web-server

collectors:
  intervalSeconds: 60
  system:
    enabled: true
    cpu: true
    ram: true
    disk: true
    network: false

services:
  windows:
    enabled: true
    services: []
  systemd:
    enabled: false
    units: []
  docker:
    enabled: true
    containers: []

gui:
  enabled: true
  startMinimized: true
  showNotifications: true

logging:
  level: info
  logPath: logs/agent.log
  logToFile: true
  maxSizeMB: 10
  maxBackups: 3
  maxAgeDays: 7
```

## GUI Özellikleri

### Ana Ekran
- Gerçek zamanlı CPU ve RAM kullanımı
- Son heartbeat zamanı
- İzlenen servis sayısı
- Bağlantı durumu

### Settings Dialog
- **Connection Sekmesi**:
  - Otomatik login (kullanıcı adı/şifre ile API key alma)
  - Manuel API key girişi
  - API endpoint ayarları
  
- **Host Info Sekmesi**:
  - Hostname (otomatik doldurma özelliği)
  - Location bilgisi

### Butonlar
- **Send Heartbeat**: Manuel heartbeat gönderimi
- **Settings**: Ayarlar dialogu
- **View Logs**: Log dosyasını görüntüleme
- **Restart Agent**: Agent'ı yeniden başlatma (manuel)

## API Entegrasyonu

Agent, ERA Monitor API'sine aşağıdaki endpoint'ler üzerinden bağlanır:

- `POST /api/auth/login` - Kullanıcı girişi ve API key alma
- `POST /api/agent/heartbeat` - Sistem metrikleri gönderimi

### Heartbeat Payload

```json
{
  "systemInfo": {
    "hostname": "MyServer-01",
    "osType": "windows",
    "osVersion": "10.0.19045",
    "cpuPercent": 25.5,
    "ramPercent": 60.2,
    "ramUsedMB": 8192,
    "ramTotalMB": 16384,
    "uptimeSeconds": 86400
  },
  "disks": [
    {
      "name": "C:",
      "mountPoint": "C:\\",
      "totalGB": 500,
      "usedGB": 250,
      "usedPercent": 50.0
    }
  ],
  "services": [
    {
      "name": "nginx",
      "displayName": "Nginx Web Server",
      "status": "running",
      "type": "docker"
    }
  ]
}
```

## Geliştirme

### Proje Yapısı

```
era-monitor-agent/
├── cmd/
│   ├── agent/          # CLI agent
│   └── agent-gui/      # GUI agent
├── internal/
│   ├── agent/          # Agent core logic
│   ├── api/            # API models
│   ├── collectors/     # Metric collectors
│   │   ├── service/    # Service monitors
│   │   └── system/     # System metrics
│   ├── config/         # Configuration
│   ├── gui/            # Fyne GUI
│   └── logger/         # Logging
├── config.yaml         # Default config
└── go.mod
```

### Yeni Collector Ekleme

1. `internal/collectors` altında yeni collector oluştur
2. `Collector` interface'ini implement et
3. `agent.go` içinde collector'ı initialize et

## Lisans

Bu proje ERA Monitor sisteminin bir parçasıdır.

## Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## Sorun Giderme

### Windows'ta Build Hatası

Eğer "build constraints exclude all Go files" hatası alıyorsanız:

```powershell
$env:CGO_ENABLED="1"
$env:Path += ";C:\mingw64\bin"
```

### 401 Unauthorized Hatası

- Settings > Connection'dan login yapın
- Yeni API key alın
- Save edin ve agent'ı yeniden başlatın

### Servis İzleme "Access Denied" Hatası

Windows Services'ı izlemek için agent'ı Administrator olarak çalıştırın.

## İletişim

ERA Monitor - [@eracloud](https://github.com/eracloud)

Proje Linki: [https://github.com/eracloud/era-monitor-agent](https://github.com/eracloud/era-monitor-agent)
