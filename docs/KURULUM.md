# InnovatEPAM Portal — kurulum ve işletim

## Gereksinimler

- .NET 8 SDK
- PostgreSQL (bağlantı dizesi `appsettings.json` veya ortam değişkenleri ile)

## Veritabanı migration

Varsayılan olarak uygulama açılışında `Database:ApplyMigrationsOnStartup` `true` ise EF migration’ları uygulanır ([Program.cs](src/InnovatEPAM.Portal/Program.cs)).

Üretim ortamında migration’ları uygulama örneği yerine CI/CD veya kontrollü bir adımda çalıştırmanız önerilir. Bunun için `appsettings.Production.json` içinde:

```json
"Database": { "ApplyMigrationsOnStartup": false }
```

tanımlayın ve dağıtım pipeline’ında `dotnet ef database update` (veya eşdeğeri) kullanın.

## İlk yönetici (Admin) hesabı

Projede **varsayılan bir admin kullanıcısı seed edilmez**; yalnızca `Submitter` ve `Admin` rolleri veritabanına yazılır.

İlk admin için tipik seçenekler:

1. **Geçici kayıt + rol ataması (geliştirme):** Arayüzden `Register` ile bir hesap oluşturun. Ardından veritabanında veya `dotnet` ile bu kullanıcıya `Admin` rolünü ekleyin (ör. SQL: `AspNetUserRoles` tablosuna kullanıcı ve Admin rolü ilişkisi).
2. **Tek seferlik CLI/script:** Organizasyonunuzun güvenlik politikasına uygun şekilde, ilk admini oluşturan küçük bir konsol aracı veya SQL runbook kullanın.

Parola politikası güçlüdür (uzunluk ve karmaşıklık); kullanıcı arayüzünde [Kayıt](src/InnovatEPAM.Portal/Views/Auth/Register.cshtml) ekranında kurallar özetlenir.

## Oturum

Kimlik doğrulama çerez tabanlıdır; süre ve ayarlar [Program.cs](src/InnovatEPAM.Portal/Program.cs) içindeki `ConfigureApplicationCookie` ile yapılandırılır. Kullanılmayan ASP.NET Session middleware kaldırılmıştır.

## İş akışı notu (triaj → inceleme)

Bir fikir `Submitted` (triaj) iken ilk inceleyici puanını kaydettiğinde durum otomatik olarak `UnderReview` olur ([ScoreService](src/InnovatEPAM.Portal/Services/ScoreService.cs)).

## Günlükler

Serilog günlükleri `logs/` altına yazılır.
