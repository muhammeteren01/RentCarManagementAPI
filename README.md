# Rent Car Management API

Araç kiralama yönetimi için .NET 10 tabanlı REST API. Katmanlı mimari ile auth, kullanıcı profili, araç, kiralama, hasar ve fiyatlandırma iş kurallarını sağlar.

## Özellikler

1. **Kimlik doğrulama** — Register / Login, JWT ile token tabanlı erişim
2. **Kullanıcı yönetimi** — Profil ve ehliyet bilgisi görüntüleme / güncelleme, şifre değiştirme
3. **Araç yönetimi** — CRUD, müsaitlik, bakım (send / complete / due list)
4. **Kiralama yönetimi** — Kirala, süre uzat (`Extended`), iade, iptal, geçmiş
5. **Hasar yönetimi** — Hasar bildirimi, bedel, admin ile ödeme alma (`collect-payment`)
6. **İş mantığı** — Günluk ücret, geç iade, ekstra km, bakım eşikleri

## Çözüm yapısı

```
Rent_Car_Management_API/
├── API/              Controllers, middleware, JWT, Swagger, Serilog
├── Core/             Entities, DTOs, enums, interfaces, FluentValidation
├── Repository/       EF Core, SQL Server, migrations, Unit of Work
├── Service/          Business services (Auth, Users, Cars, Rentals, DamageReports)
└── Service.Tests/    Unit tests (xUnit, Moq, FluentAssertions)
```

### Service klasörleri

```
Service/Services/
  Common/           GenericService
  Auth/             Auth, Token, Password
  Users/            UserService
  Cars/             CarService
  Rentals/          RentalService, PricingService
  DamageReports/    DamageReportService
```

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (ör. `localhost\SQLEXPRESS`)
- Visual Studio / VS Code / Rider (opsiyonel)

## Kurulum

1. Repoyu klonlayın:

```bash
git clone https://github.com/muhammeteren01/RentCarManagementAPI.git
cd Rent_Car_Management_API
```

2. Bağlantı dizesini kontrol edin: `API/appsettings.json` → `ConnectionStrings:DefaultConnection`

3. Veritabanı migration:

```bash
dotnet ef database update --project Repository --startup-project API
```

4. API’yi çalıştırın:

```bash
dotnet run --project API
```

5. Swagger (Development): tarayıcıda `/swagger`  
   Örnek: `https://localhost:7101/swagger`

## Kimlik doğrulama (Swagger)

1. `POST /api/Auth/register` veya `POST /api/Auth/login`
2. Dönen `token` değerini kopyalayın
3. Swagger → **Authorize** → `Bearer {token}`

Roller: `Customer`, `Admin` (register şu an rol seçimine izin verir; production’da kısıtlanmalıdır).

## Ana endpoint grupları

| Alan | Route öneki | Not |
|------|-------------|-----|
| Auth | `/api/Auth` | register, login, me |
| Users | `/api/Users` | me, password |
| Cars | `/api/Cars` | CRUD + maintenance (admin) |
| Rentals | `/api/Rentals` | create, extend, return, cancel, history |
| Damage | `/api/DamageReports` | create; `collect-payment` (admin) |

## Fiyatlandırma kuralları (özet)

- Kiralama günü = takvim gün farkı (min. 1)
- Base = gün × `DailyPrice`
- Dahil km = gün × **200**
- Geç iade = geciken takvim günü × `DailyPrice`
- Ekstra km = (sürülen − dahil) × `ExtraKmFee`
- Hasar bedeli kiralama toplamına **dahil edilmez**; ayrı rapor + admin tahsilatı

## Middleware

Pipeline sırası:

1. **CorrelationId** — istek izleme (`X-Correlation-Id`)
2. **Request logging** — Serilog istek logları
3. **Exception middleware** — merkezi hata → JSON (400/401/404/409/500)
4. Authentication / Authorization — JWT + roller

Loglar: `API/Logs/log-*.json`

## Unit testler

```bash
dotnet test
```

Her testi ayrı görmek için:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Testler gerçek veritabanına bağlanmaz; repository / UoW mock’lanır.

## Yapılandırma notları

- `JwtSettings` değerlerini production ortamında kendi secret’ınızla değiştirin; secret’ı public repoya koyuyorsanız rotasyon yapın.
- SQL connection string ortamınıza göre güncellenmelidir.

## Lisans / kullanım

Staj / eğitim amaçlı örnek proje.
