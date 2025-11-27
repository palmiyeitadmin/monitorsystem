using ERAMonitor.Core.Entities;
using ERAMonitor.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.Infrastructure.Data;

public static class SeedDataTurkish
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var random = new Random();

        // Organizasyon
        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null)
        {
            organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = "ERA Monitor Turkiye",
                Slug = "era-monitor-tr",
                IsActive = true,
            };
            context.Organizations.Add(organization);
            await context.SaveChangesAsync();
        }

        // Ana yonetici
        if (!await context.Users.AnyAsync(u => u.Email == "admin@eramonitor.com.tr"))
        {
            context.Users.Add(new User
            {
                OrganizationId = organization.Id,
                Email = "admin@eramonitor.com.tr",
                FullName = "Sistem Yoneticisi",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Lokasyonlar (10-15 arasi)
        if (!await context.Locations.AnyAsync())
        {
            var locations = new[]
            {
                new Location { OrganizationId = organization.Id, Name = "Istanbul Veri Merkezi", Category = LocationCategory.HostingProvider, City = "Istanbul", Country = "Turkiye", Latitude = 41.0082m, Longitude = 28.9784m, Notes = "Birincil veri merkezi" },
                new Location { OrganizationId = organization.Id, Name = "Ankara Bulut Bolgesi", Category = LocationCategory.CloudProvider, City = "Ankara", Country = "Turkiye", Latitude = 39.9334m, Longitude = 32.8597m, Notes = "Yedekleme ve DR" },
                new Location { OrganizationId = organization.Id, Name = "Izmir Kenar Noktasi", Category = LocationCategory.Colocation, City = "Izmir", Country = "Turkiye", Latitude = 38.4192m, Longitude = 27.1287m, Notes = "Kenar dagitim" },
                new Location { OrganizationId = organization.Id, Name = "Bursa Yerel Ofis", Category = LocationCategory.OnPremise, City = "Bursa", Country = "Turkiye", Latitude = 40.1826m, Longitude = 29.0669m },
                new Location { OrganizationId = organization.Id, Name = "Antalya CDN Noktasi", Category = LocationCategory.HostingProvider, City = "Antalya", Country = "Turkiye", Latitude = 36.8969m, Longitude = 30.7133m },
            };
            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();
        }

        var locationIds = await context.Locations.Select(l => l.Id).ToListAsync();

        // Musteriler (10-15 arasi)
        if (!await context.Customers.AnyAsync())
        {
            var customers = new[]
            {
                new Customer { OrganizationId = organization.Id, Name = "TechnoSoft A.S.", Slug = "technosoft", ContactEmail = "info@technosoft.com.tr", ContactName = "Ahmet Yilmaz", ContactPhone = "+90 212 555 0101", City = "Istanbul", Country = "Turkiye", Industry = "Yazilim" },
                new Customer { OrganizationId = organization.Id, Name = "DataCore Teknoloji", Slug = "datacore", ContactEmail = "destek@datacore.com.tr", ContactName = "Mehmet Kaya", ContactPhone = "+90 312 555 0202", City = "Ankara", Country = "Turkiye", Industry = "Danismanlik" },
                new Customer { OrganizationId = organization.Id, Name = "CloudNet Solutions", Slug = "cloudnet", ContactEmail = "iletisim@cloudnet.com.tr", ContactName = "Ayse Demir", ContactPhone = "+90 216 555 0303", City = "Istanbul", Country = "Turkiye", Industry = "Bulut" },
                new Customer { OrganizationId = organization.Id, Name = "DigitalHub Ltd.", Slug = "digitalhub", ContactEmail = "info@digitalhub.com.tr", ContactName = "Fatma Sahin", ContactPhone = "+90 232 555 0404", City = "Izmir", Country = "Turkiye", Industry = "E-ticaret" },
                new Customer { OrganizationId = organization.Id, Name = "SynthAI Yazilim", Slug = "synthai", ContactEmail = "iletisim@synthai.com.tr", ContactName = "Ali Celik", ContactPhone = "+90 242 555 0505", City = "Antalya", Country = "Turkiye", Industry = "Yapay Zeka" },
                new Customer { OrganizationId = organization.Id, Name = "MegaData Systems", Slug = "megadata", ContactEmail = "info@megadata.com.tr", ContactName = "Zeynep Arslan", ContactPhone = "+90 224 555 0606", City = "Bursa", Country = "Turkiye", Industry = "Veri Merkezi" },
                new Customer { OrganizationId = organization.Id, Name = "Inovasyon Teknoloji", Slug = "inovasyon", ContactEmail = "destek@inovasyon.com.tr", ContactName = "Emre Yildiz", ContactPhone = "+90 212 555 0707", City = "Istanbul", Country = "Turkiye", Industry = "IoT" },
                new Customer { OrganizationId = organization.Id, Name = "NetWork Pro A.S.", Slug = "networkpro", ContactEmail = "info@networkpro.com.tr", ContactName = "Selin Aydin", ContactPhone = "+90 312 555 0808", City = "Ankara", Country = "Turkiye", Industry = "Ag" },
                new Customer { OrganizationId = organization.Id, Name = "CyberGuard Siber Guvenlik", Slug = "cyberguard", ContactEmail = "iletisim@cyberguard.com.tr", ContactName = "Burak Ozdemir", ContactPhone = "+90 216 555 0909", City = "Istanbul", Country = "Turkiye", Industry = "Siber Guvenlik" },
                new Customer { OrganizationId = organization.Id, Name = "AppDev Yazilim", Slug = "appdev", ContactEmail = "info@appdev.com.tr", ContactName = "Deniz Kara", ContactPhone = "+90 232 555 1010", City = "Izmir", Country = "Turkiye", Industry = "Mobil Uygulama" },
                new Customer { OrganizationId = organization.Id, Name = "LogiTrans Lojistik", Slug = "logitrans", ContactEmail = "destek@logitrans.com.tr", ContactName = "Cem Ayhan", ContactPhone = "+90 212 555 1111", City = "Istanbul", Country = "Turkiye", Industry = "Lojistik" },
                new Customer { OrganizationId = organization.Id, Name = "Finera Finans", Slug = "finera", ContactEmail = "iletisim@finera.com.tr", ContactName = "Elif Guler", ContactPhone = "+90 312 555 1212", City = "Ankara", Country = "Turkiye", Industry = "Finans" },
            };
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
        }

        var customerIds = await context.Customers.Select(c => c.Id).ToListAsync();

        // Hostlar (15 adet)
        if (!await context.Hosts.AnyAsync())
        {
            var hostTemplates = new[]
            {
                ("web-istanbul-01", OsType.Linux, HostCategory.Website, "Ubuntu 22.04 LTS"),
                ("web-istanbul-02", OsType.Linux, HostCategory.Website, "Ubuntu 22.04 LTS"),
                ("api-ankara-01", OsType.Linux, HostCategory.CloudInstance, "Debian 12"),
                ("db-ankara-01", OsType.Linux, HostCategory.CloudInstance, "PostgreSQL Ubuntu"),
                ("cache-istanbul-01", OsType.Linux, HostCategory.VirtualMachine, "AlmaLinux 9"),
                ("queue-istanbul-01", OsType.Linux, HostCategory.VirtualMachine, "Rocky Linux 9"),
                ("files-izmir-01", OsType.Linux, HostCategory.VirtualMachine, "Ubuntu 20.04"),
                ("mail-ankara-01", OsType.Linux, HostCategory.VirtualMachine, "Debian 11"),
                ("monitor-izmir-01", OsType.Linux, HostCategory.VirtualMachine, "Ubuntu 22.04"),
                ("backup-bursa-01", OsType.Linux, HostCategory.PhysicalServer, "Ubuntu 22.04"),
                ("k8s-antalya-01", OsType.Linux, HostCategory.CloudInstance, "Ubuntu 22.04"),
                ("k8s-antalya-02", OsType.Linux, HostCategory.CloudInstance, "Ubuntu 22.04"),
                ("win-edge-01", OsType.Windows, HostCategory.VirtualMachine, "Windows Server 2022"),
                ("win-iis-01", OsType.Windows, HostCategory.VirtualMachine, "Windows Server 2019"),
                ("vpn-ankara-01", OsType.Linux, HostCategory.VPS, "Debian 12"),
            };

            var hostStatuses = new[] { StatusType.Up, StatusType.Up, StatusType.Up, StatusType.Warning, StatusType.Down, StatusType.Degraded };

            var hosts = hostTemplates.Select(template =>
            {
                var status = hostStatuses[random.Next(hostStatuses.Length)];
                return new Host
                {
                    OrganizationId = organization.Id,
                    LocationId = locationIds[random.Next(locationIds.Count)],
                    CustomerId = customerIds[random.Next(customerIds.Count)],
                    Name = template.Item1,
                    Hostname = $"{template.Item1}.local",
                    Description = "Dashboard icin ornek host",
                    OsType = template.Item2,
                    OsVersion = template.Item4,
                    Category = template.Item3,
                    Tags = new[] { "ornek", "demo" },
                    PrimaryIp = $"10.0.{random.Next(10, 40)}.{random.Next(2, 200)}",
                    PublicIp = $"185.{random.Next(50, 80)}.{random.Next(10, 200)}.{random.Next(2, 240)}",
                    AgentVersion = "2.2.0",
                    AgentInstalledAt = DateTime.UtcNow.AddDays(-random.Next(5, 120)),
                    CheckIntervalSeconds = 60,
                    CurrentStatus = status,
                    LastSeenAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 120)),
                    StatusChangedAt = DateTime.UtcNow.AddHours(-random.Next(1, 72)),
                    PreviousStatus = StatusType.Up,
                    UptimeSeconds = random.Next(5, 90) * 3600L,
                    CpuPercent = random.Next(5, 90),
                    RamPercent = random.Next(20, 95),
                    RamUsedMb = random.Next(2048, 16384),
                    RamTotalMb = 16384,
                    ProcessCount = random.Next(80, 220),
                    Notes = "Ornek veri",
                    MaintenanceMode = false,
                    MonitoringEnabled = true
                };
            }).ToList();

            context.Hosts.AddRange(hosts);
            await context.SaveChangesAsync();
        }

        var hostIds = await context.Hosts.Select(h => h.Id).ToListAsync();

        // Servisler (her host icin 2 ornek)
        if (!await context.Services.AnyAsync())
        {
            var serviceTemplates = new[]
            {
                (ServiceType.SystemdUnit, "nginx.service", "Nginx Web Sunucusu"),
                (ServiceType.SystemdUnit, "postgresql.service", "PostgreSQL Veritabani"),
                (ServiceType.SystemdUnit, "redis-server.service", "Redis Cache"),
                (ServiceType.SystemdUnit, "rabbitmq-server.service", "RabbitMQ Kuyruk"),
                (ServiceType.DockerContainer, "backend-api", "API Uygulamasi"),
                (ServiceType.DockerContainer, "worker-jobs", "Arka Plan Islemcisi"),
                (ServiceType.DockerContainer, "nginx-proxy", "Ters Proxy"),
                (ServiceType.WindowsService, "W3SVC", "IIS Web Servisi"),
                (ServiceType.WindowsService, "MSSQLSERVER", "SQL Server"),
                (ServiceType.Process, "node-app", "Next.js Dashboard"),
                (ServiceType.Process, "dotnet-api", ".NET API"),
                (ServiceType.SystemdUnit, "telegraf.service", "Ajan Izleme"),
            };

            var serviceStatus = new[] { StatusType.Up, StatusType.Up, StatusType.Warning, StatusType.Degraded, StatusType.Down };

            var services = new List<Service>();
            foreach (var hostId in hostIds.Take(12))
            {
                foreach (var template in serviceTemplates.OrderBy(_ => random.Next()).Take(2))
                {
                    services.Add(new Service
                    {
                        HostId = hostId,
                        ServiceType = template.Item1,
                        ServiceName = template.Item2,
                        DisplayName = template.Item3,
                        Description = $"{template.Item3} servisi",
                        CurrentStatus = serviceStatus[random.Next(serviceStatus.Length)],
                        LastStatusChange = DateTime.UtcNow.AddHours(-random.Next(1, 72)),
                        PreviousStatus = StatusType.Up,
                        MonitoringEnabled = true,
                        AlertOnStop = true,
                        RestartCount = random.Next(0, 5),
                        LastHealthyAt = DateTime.UtcNow.AddHours(-random.Next(1, 48))
                    });
                }
            }

            context.Services.AddRange(services);
            await context.SaveChangesAsync();
        }

        // Kontroller (10-15 arasi)
        if (!await context.Checks.AnyAsync())
        {
            var checks = new[]
            {
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(0), CustomerId = customerIds.ElementAtOrDefault(0), Name = "Ana Site HTTP", CheckType = CheckType.HTTP, Target = "https://www.ornek-site.com", ExpectedStatusCode = 200, IntervalSeconds = 60, TimeoutSeconds = 10, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(1), CustomerId = customerIds.ElementAtOrDefault(1), Name = "API Saglik", CheckType = CheckType.HTTP, Target = "https://api.ornek-site.com/health", ExpectedStatusCode = 200, IntervalSeconds = 60, TimeoutSeconds = 10, CurrentStatus = StatusType.Up, ExpectedKeyword = "ok" },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(2), CustomerId = customerIds.ElementAtOrDefault(2), Name = "PostgreSQL TCP", CheckType = CheckType.TCP, Target = "10.0.12.5:5432", TcpPort = 5432, IntervalSeconds = 120, TimeoutSeconds = 8, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(3), CustomerId = customerIds.ElementAtOrDefault(3), Name = "Redis TCP", CheckType = CheckType.TCP, Target = "10.0.15.10:6379", TcpPort = 6379, IntervalSeconds = 60, TimeoutSeconds = 5, CurrentStatus = StatusType.Warning },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(4), CustomerId = customerIds.ElementAtOrDefault(4), Name = "CDN Ping", CheckType = CheckType.Ping, Target = "cdn.ornek-site.com", IntervalSeconds = 120, TimeoutSeconds = 5, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(5), CustomerId = customerIds.ElementAtOrDefault(5), Name = "VPN TCP", CheckType = CheckType.TCP, Target = "vpn.ornek-site.com:443", TcpPort = 443, IntervalSeconds = 90, TimeoutSeconds = 10, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(6), CustomerId = customerIds.ElementAtOrDefault(6), Name = "Mail HTTP", CheckType = CheckType.HTTP, Target = "https://mail.ornek-site.com/health", ExpectedStatusCode = 200, IntervalSeconds = 60, TimeoutSeconds = 10, CurrentStatus = StatusType.Degraded },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(7), CustomerId = customerIds.ElementAtOrDefault(7), Name = "Queue TCP", CheckType = CheckType.TCP, Target = "10.0.18.20:5672", TcpPort = 5672, IntervalSeconds = 45, TimeoutSeconds = 5, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(8), CustomerId = customerIds.ElementAtOrDefault(8), Name = "Dosya Ping", CheckType = CheckType.Ping, Target = "files.ornek-site.com", IntervalSeconds = 180, TimeoutSeconds = 10, CurrentStatus = StatusType.Warning },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(9), CustomerId = customerIds.ElementAtOrDefault(9), Name = "IIS HTTP", CheckType = CheckType.HTTP, Target = "https://iis.ornek-site.com/health", ExpectedStatusCode = 200, IntervalSeconds = 60, TimeoutSeconds = 8, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(10), CustomerId = customerIds.ElementAtOrDefault(10), Name = "Kubernetes API", CheckType = CheckType.HTTP, Target = "https://k8s.ornek-site.com/readyz", ExpectedStatusCode = 200, IntervalSeconds = 30, TimeoutSeconds = 5, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(11), CustomerId = customerIds.ElementAtOrDefault(11), Name = "Elasticsearch TCP", CheckType = CheckType.TCP, Target = "10.0.22.8:9200", TcpPort = 9200, IntervalSeconds = 90, TimeoutSeconds = 8, CurrentStatus = StatusType.Degraded },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(12), CustomerId = customerIds.ElementAtOrDefault(0), Name = "Node Uygulama HTTP", CheckType = CheckType.HTTP, Target = "https://app.ornek-site.com/health", ExpectedStatusCode = 200, IntervalSeconds = 60, TimeoutSeconds = 6, CurrentStatus = StatusType.Up },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(13), CustomerId = customerIds.ElementAtOrDefault(1), Name = "Win Agent Ping", CheckType = CheckType.Ping, Target = "win-edge-01", IntervalSeconds = 120, TimeoutSeconds = 8, CurrentStatus = StatusType.Warning },
                new Check { OrganizationId = organization.Id, HostId = hostIds.ElementAtOrDefault(14), CustomerId = customerIds.ElementAtOrDefault(2), Name = "VPN HTTPS", CheckType = CheckType.HTTP, Target = "https://vpn.ornek-site.com/status", ExpectedStatusCode = 200, IntervalSeconds = 60, TimeoutSeconds = 6, CurrentStatus = StatusType.Up },
            };

            foreach (var check in checks)
            {
                check.MonitoringEnabled = true;
                check.FollowRedirects = true;
                if (check.TimeoutSeconds == 0) check.TimeoutSeconds = 10;
            }

            context.Checks.AddRange(checks);
            await context.SaveChangesAsync();
        }

        // Olaylar (12 adet)
        if (!await context.Incidents.AnyAsync())
        {
            var incidents = new[]
            {
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(0), IncidentNumber = 1001, Title = "Yuksek CPU Kullanimi", Description = "API sunucusunda CPU %95 uzerinde", Severity = IncidentSeverity.High, Priority = IncidentPriority.High, Status = IncidentStatus.InProgress, Impact = "Kritik hizmetler yavas", CreatedAt = DateTime.UtcNow.AddMinutes(-25) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(1), IncidentNumber = 1002, Title = "Veritabani Baglanti Sorunu", Description = "PostgreSQL baglantilari zaman asimina ugruyor", Severity = IncidentSeverity.Critical, Priority = IncidentPriority.Urgent, Status = IncidentStatus.New, Impact = "Okuma islemleri etkileniyor", CreatedAt = DateTime.UtcNow.AddMinutes(-40) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(2), IncidentNumber = 1003, Title = "Disk Alani Az", Description = "Log diski %90 dolu", Severity = IncidentSeverity.Medium, Priority = IncidentPriority.Medium, Status = IncidentStatus.Acknowledged, Impact = "Yeni log yazimi yavas", CreatedAt = DateTime.UtcNow.AddHours(-3) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(3), IncidentNumber = 1004, Title = "SSL Sertifikasi Bitiyor", Description = "Ana domain sertifikasi 5 gun icinde bitecek", Severity = IncidentSeverity.Low, Priority = IncidentPriority.Low, Status = IncidentStatus.New, Impact = "Kullanici guven uyarisi olusabilir", CreatedAt = DateTime.UtcNow.AddHours(-6) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(4), IncidentNumber = 1005, Title = "Redis Gecikme", Description = "Redis gecikmeleri 300ms uzerinde", Severity = IncidentSeverity.High, Priority = IncidentPriority.High, Status = IncidentStatus.InProgress, Impact = "Oturum yonetimi yavas", CreatedAt = DateTime.UtcNow.AddMinutes(-55) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(5), IncidentNumber = 1006, Title = "Email Kuyrugunda Birikme", Description = "Mail kuyrugunda 1500 mesaj bekliyor", Severity = IncidentSeverity.Medium, Priority = IncidentPriority.Medium, Status = IncidentStatus.Acknowledged, Impact = "Bildirimler gecikiyor", CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(6), IncidentNumber = 1007, Title = "HTTP 500 Hatalari", Description = "Dashboard isteklerinde aralikli 500 hatalari", Severity = IncidentSeverity.High, Priority = IncidentPriority.High, Status = IncidentStatus.InProgress, Impact = "Kullanici oturumu etkileniyor", CreatedAt = DateTime.UtcNow.AddMinutes(-70) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(7), IncidentNumber = 1008, Title = "VPN Kesintisi", Description = "VPN baglantisi 10 dakikadir kapali", Severity = IncidentSeverity.Critical, Priority = IncidentPriority.Urgent, Status = IncidentStatus.New, Impact = "Uzak ofis erisimi yok", CreatedAt = DateTime.UtcNow.AddMinutes(-15) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(8), IncidentNumber = 1009, Title = "Yedekleme Basarisiz", Description = "Gece yedekleme gorevi tamamlanamadi", Severity = IncidentSeverity.Medium, Priority = IncidentPriority.Medium, Status = IncidentStatus.Acknowledged, Impact = "Son yedek 24 saat once", CreatedAt = DateTime.UtcNow.AddHours(-10) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(9), IncidentNumber = 1010, Title = "CDN Gecikmesi", Description = "CDN yanit suresi ortalama 800ms", Severity = IncidentSeverity.Low, Priority = IncidentPriority.Low, Status = IncidentStatus.InProgress, Impact = "Statik icerik yavas", CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(10), IncidentNumber = 1011, Title = "Kubernetes Node Down", Description = "Bir worker node yanit vermiyor", Severity = IncidentSeverity.High, Priority = IncidentPriority.High, Status = IncidentStatus.InProgress, Impact = "Pod yeniden planlaniyor", CreatedAt = DateTime.UtcNow.AddMinutes(-90) },
                new Incident { OrganizationId = organization.Id, CustomerId = customerIds.ElementAtOrDefault(11), IncidentNumber = 1012, Title = "Onbellek Temizlendi", Description = "Cache invalidation sonrasi anlik yukselis", Severity = IncidentSeverity.Info, Priority = IncidentPriority.Low, Status = IncidentStatus.Resolved, Impact = "Kisa sureli gecikme", CreatedAt = DateTime.UtcNow.AddHours(-12), ResolvedAt = DateTime.UtcNow.AddHours(-10) },
            };

            context.Incidents.AddRange(incidents);
            await context.SaveChangesAsync();
        }

        var incidentIds = await context.Incidents.Select(i => i.Id).ToListAsync();

        // Bildirimler (10 adet)
        if (!await context.Notifications.AnyAsync())
        {
            var channels = new[] { "Email", "Slack", "Webhook" };
            var statuses = new[] { NotificationStatus.Sent, NotificationStatus.Pending, NotificationStatus.Failed };
            var notifications = Enumerable.Range(1, 12).Select(i =>
            {
                var status = statuses[random.Next(statuses.Length)];
                return new Notification
                {
                    OrganizationId = organization.Id,
                    TriggerType = "Incident",
                    TriggerId = incidentIds.FirstOrDefault(),
                    Channel = channels[random.Next(channels.Length)],
                    Recipient = $"devops{i}@ornek.com",
                    Subject = $"Olay bildirimi #{1000 + i}",
                    Content = "Ornek bildirim icerigi",
                    Status = status,
                    RetryCount = status == NotificationStatus.Failed ? random.Next(1, 3) : 0,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 240)),
                    SentAt = status != NotificationStatus.Pending ? DateTime.UtcNow.AddMinutes(-random.Next(1, 120)) : null,
                    ErrorMessage = status == NotificationStatus.Failed ? "SMTP baglantisi zaman asimi" : null
                };
            }).ToList();

            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync();
        }

        // Ek kullanicilar
        if (await context.Users.CountAsync() < 5)
        {
            var users = new[]
            {
                new User { OrganizationId = organization.Id, Email = "mehmet.yilmaz@eramonitor.com.tr", FullName = "Mehmet Yilmaz", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"), Role = UserRole.Admin, IsActive = true, EmailVerified = true },
                new User { OrganizationId = organization.Id, Email = "ayse.kaya@eramonitor.com.tr", FullName = "Ayse Kaya", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"), Role = UserRole.Operator, IsActive = true, EmailVerified = true },
                new User { OrganizationId = organization.Id, Email = "ali.demir@eramonitor.com.tr", FullName = "Ali Demir", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"), Role = UserRole.Viewer, IsActive = true, EmailVerified = true },
                new User { OrganizationId = organization.Id, Email = "fatma.celik@eramonitor.com.tr", FullName = "Fatma Celik", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"), Role = UserRole.Operator, IsActive = true, EmailVerified = true },
            };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        Console.WriteLine("Turkce ornek veriler yüklendi.");
        Console.WriteLine($"   - {await context.Locations.CountAsync()} Lokasyon");
        Console.WriteLine($"   - {await context.Customers.CountAsync()} Musteri");
        Console.WriteLine($"   - {await context.Hosts.CountAsync()} Host");
        Console.WriteLine($"   - {await context.Services.CountAsync()} Servis");
        Console.WriteLine($"   - {await context.Checks.CountAsync()} Kontrol");
        Console.WriteLine($"   - {await context.Incidents.CountAsync()} Olay");
        Console.WriteLine($"   - {await context.Users.CountAsync()} Kullanici");
    }
}
