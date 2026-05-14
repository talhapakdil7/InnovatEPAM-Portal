# Architecture Decision Records — InnovatEPAM Portal

**Proje**: InnovatEPAM Portal — Çalışan İnovasyon Fikirleri Yönetim Portalı
**Oluşturulma**: 2026-05-14
**Durum**: Aktif

Bu dosya projedeki tüm önemli teknoloji ve mimari kararları, seçilme gerekçeleri ve reddedilen alternatifleriyle birlikte belgeler. Yeni bir teknoloji eklendiğinde veya mevcut bir karar değiştirildiğinde bu dosya güncellenir.

---

## ADR-001: Uygulama Çerçevesi

**Karar**: ASP.NET Core MVC (Monolith)
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
Çalışanların fikir gönderdiği ve adminlerin incelediği bir iç portal. Sunucu taraflı render, rol tabanlı yetkilendirme ve form odaklı iş akışları ön planda.

### Karar
ASP.NET Core MVC — Razor Views ile sunucu taraflı render, Controller → Service → Repository katmanlı mimarisi.

### Gerekçe
- Form ağırlıklı iş akışları (fikir gönderme, admin inceleme) MVC pattern'ine doğal uyum sağlar
- Razor Views sunucu taraflı render ile SEO ve ilk yükleme hızını artırır
- ASP.NET Core Identity ile yerleşik rol tabanlı yetkilendirme entegrasyonu
- MVP kapsamı için SPA/API ayrımı gereksiz karmaşıklık yaratır

### Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|---|---|
| Blazor Server | Öğrenme eğrisi; MVC ekibin daha aşina olduğu yapı |
| React + Web API | Frontend/backend ayrımı MVP için over-engineering; iki ayrı proje yönetimi |
| Minimal API | UI/View katmanı yok; portal uygulaması için uygunsuz |

---

## ADR-002: Programlama Dili ve Platform Versiyonu

**Karar**: C# 12 / .NET 10.0
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
Spec 001 implementation aşamasında `.csproj` dosyası `net10.0` hedef çerçevesiyle ve tüm paketler 10.x versiyonlarıyla kuruldu. Tüm bağımlılıklar .NET 10 ekosistemindedir.

### Karar
`<TargetFramework>net10.0</TargetFramework>` — C# 12 dil özellikleri (primary constructors, collection expressions, vb.) kullanılır.

### Gerekçe
- Proje kurulumunda .NET 10 seçildi; tüm NuGet paketleri bu versiyona göre kilitli
- .NET 10 ile gelen performans iyileştirmeleri ve C# 12 sözdizimi geliştirmelerinden faydalanılır
- LTS olmasa da proje yaşam döngüsü boyunca desteklenecek

### Paket Versiyonları (Kilitli)

| Paket | Versiyon |
|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.8 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.8 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.1 |
| `AutoMapper` | 16.1.1 |
| `FluentValidation.AspNetCore` | 11.3.1 |
| `Serilog.AspNetCore` | 10.0.0 |

### Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|---|---|
| .NET 8 LTS | Paketler zaten 10.x versiyonlarına güncellendi; geri dönmek tüm bağımlılıkları kırar |
| .NET 9 | Zaten .NET 10'a geçildi |

---

## ADR-003: Veritabanı

**Karar**: PostgreSQL 14+
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
İlişkisel veri modeli (User, Idea, IdeaAttachment, AuditLog). Üretim ortamında güvenilirlik ve açık kaynak lisansı öncelikli.

### Karar
PostgreSQL 14+ — Entity Framework Core üzerinden `Npgsql.EntityFrameworkCore.PostgreSQL` sürücüsüyle erişilir.

### Gerekçe
- Ücretsiz, açık kaynak ve kurumsal üretim ortamlarında kanıtlanmış
- EF Core ile tam uyum; `Npgsql` sürücüsü aktif olarak bakımı yapılan olgun bir proje
- JSONB desteği; gelecekte kategori verisi için native JSON sorguları mümkün
- EPAM altyapısında yaygın kullanım

### Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|---|---|
| SQL Server | Lisans maliyeti; EPAM kurumsal lisansı dışında zorunluluk yok |
| SQLite | Üretim eşzamanlılığı sınırlı; yük altında performans yetersiz |
| MySQL | PostgreSQL'in JSON ve JSONB desteği daha olgun |

---

## ADR-004: ORM ve Veritabanı Erişimi

**Karar**: Entity Framework Core 10 — Code-First, Migration tabanlı
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
Şema yönetimi ve veri erişimi için bir ORM gerekli. Ham SQL yazmaktan kaçınılması constitution'da da belirtilmiş.

### Karar
EF Core 10 ile Code-First yaklaşım. Tüm şema değişiklikleri `dotnet ef migrations add` ile versiyonlanır.

### Gerekçe
- Constitution Prensibi I: "Ham SQL query yasak → her zaman EF Core LINQ kullan"
- Code-First migration'lar şema geçmişini Git'te takip edilebilir kılar
- Repository pattern ile EF Core DbContext soyutlaması test edilebilirliği artırır

### Kurallar
- `DbContext` doğrudan Controller veya Service'te kullanılamaz; Repository üzerinden erişilir
- Ham SQL (`FromSqlRaw`, `ExecuteSqlRaw`) yasak — sadece LINQ sorguları
- Her özellik için ayrı migration; breaking change'ler nullable column ekleyerek önlenir

### Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|---|---|
| Dapper | Ham SQL gerektiriyor; constitution ihlali |
| Database-First | Şema değişikliklerini takip etmeyi zorlaştırır |

---

## ADR-005: Kimlik Doğrulama ve Yetkilendirme

**Karar**: ASP.NET Core Identity — Cookie tabanlı, rol bazlı (Submitter / Admin)
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
İki rol: Submitter (kendi fikirlerini görür) ve Admin (tüm fikirleri yönetir). Hesaplar manuel oluşturulur (self-registration Submitter için açık, Admin hesapları manuel).

### Karar
ASP.NET Core Identity — `ApplicationUser : IdentityUser<Guid>` ile özelleştirilmiş, `IdentityRole<Guid>` ile rol yönetimi. Cookie tabanlı oturum, sliding expiration.

### Oturum Süreleri
- Submitter: 30 dakika sliding expiration
- Admin: 15 dakika sliding expiration

### Gerekçe
- .NET ekosistemiyle tam entegrasyon; şifre hash, lockout, rol yönetimi hazır gelir
- Cookie tabanlı kimlik doğrulama MVC formu için doğal; JWT gereksiz
- Constitution Prensibi IV: "Authentication is centralized via ASP.NET Core Identity"

### Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|---|---|
| JWT Bearer Token | SPA olmayan MVC'de gereksiz; cookie daha basit ve güvenli |
| OAuth2 / SSO | Faz 2'ye ertelendi (spec kapsamı dışı) |
| Özel auth sistemi | Güvenlik açığı riski; Identity battle-tested |

---

## ADR-006: Validasyon

**Karar**: FluentValidation — Sunucu taraflı, servis öncesi
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
Form validasyonu için her ViewModel'e ait kural seti gerekli. Koşullu validasyon (kategori bazlı alan kuralları) desteklenmeli.

### Karar
`FluentValidation.AspNetCore` — Her ViewModel için ayrı `AbstractValidator<T>` sınıfı. `When()` / `Unless()` koşullu kurallar.

### Gerekçe
- Koşullu validasyon (`When(x => x.Category == "TechnicalImprovement", ...)`) Data Annotation ile mümkün değil
- Validator sınıfları bağımsız test edilebilir
- `AddFluentValidationAutoValidation()` ile MVC pipeline'a otomatik entegrasyon
- Constitution: "Model doğrulaması için FluentValidation kullan; `[Required]` gibi data annotation'lar sadece ViewModel'de"

### Kurallar
- Her ViewModel için bir validator sınıfı (`CreateIdeaValidator`, `LoginValidator`, `RegisterValidator`)
- Business rule validasyonu service katmanında; format/alan validasyonu FluentValidation'da
- Data Annotation (`[Required]`, `[MaxLength]`) yalnızca ViewModel'de display metadata için kullanılabilir

### Reddedilen Alternatifler

| Alternatif | Red Sebebi |
|---|---|
| Yalnızca Data Annotations | Koşullu kurallar desteklenmiyor; test edilebilirlik zayıf |
| Manuel validasyon (if/else) | Tekrarlayan kod; merkezi yönetim yok |

---

## ADR-007: DTO ve ViewModel Mapping

**Karar**: AutoMapper
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Bağlam
Model → DTO → ViewModel dönüşümleri her request döngüsünde gerçekleşiyor. Manuel mapping kod tekrarı yaratır.

### Karar
`AutoMapper` — Tüm Model ↔ DTO ve DTO ↔ ViewModel dönüşümleri `AutoMapperProfile`'da tanımlanır.

### Gerekçe
- Constitution: "DTO ↔ ViewModel dönüşümü için AutoMapper kullan; manuel mapping yasak"
- Merkezi `Profile` sınıfı mapping kurallarını tek yerde toplar
- `AfterMap()` hook'ları ile özel dönüşümler (JSON deserializasyon) desteklenir

### Kurallar
- Manuel property kopyalama (`dest.X = src.X`) yasak
- Tüm yeni mapping'ler `AutoMapperProfile.cs`'e eklenir
- `Ignore()` ve `AfterMap()` kullanımı gerekçesiyle yorumlanır

---

## ADR-008: Loglama

**Karar**: Serilog — Yapısal loglama, console + dosya
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Karar
`Serilog.AspNetCore` — Console ve günlük döngülü dosya (`logs/app-.log`) çıktısı.

### Gerekçe
- Yapısal loglama (property tabanlı) log analiz araçlarıyla (Seq, Elastic) entegrasyona hazır
- Constitution Prensibi IX: "Logging includes context: user, operation, timestamp, error details"

### Kurallar
- String interpolation ile loglama yasak: `_logger.LogInformation($"...")` yasak
- Yapısal loglama zorunlu: `_logger.LogInformation("Idea {IdeaId} created", idea.Id)`
- Exception log'ları her zaman exception nesnesini içerir: `_logger.LogError(ex, "...")`

---

## ADR-009: Dosya Yükleme Güvenliği

**Karar**: wwwroot dışında depolama, MIME type doğrulama (magic bytes)
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Karar
Yüklenen dosyalar `uploads/` klasöründe (wwwroot dışı) saklanır. MIME tipi dosya uzantısından değil magic byte'lardan doğrulanır.

### Gerekçe
- Constitution Prensibi VI: "Uploaded files are stored outside web root with hashed names"
- Uzantı bazlı doğrulama kolayca atlatılabilir; content-based doğrulama güvenlidir
- Dosyaya doğrudan URL erişimi engellenir; indirme controller action'ı üzerinden yapılır

### Kurallar
- İzin verilen tipler: `.pdf .doc .docx .xls .xlsx .jpg .jpeg .png`
- Maksimum boyut: 10 MB
- Depolama yolu: `uploads/ideas/{IdeaId}/{HashedFileName}`
- Dosya adı hash'lenir, orijinal ad metadata olarak DB'de saklanır

---

## ADR-010: UI Framework

**Karar**: Bootstrap 5 — Razor Views ile sunucu taraflı render
**Tarih**: 2026-05-14
**Durum**: ✅ Kesinleşti

### Karar
Bootstrap 5 CDN üzerinden yüklenir. Tüm UI bileşenleri Bootstrap utility class'larıyla oluşturulur. Dinamik form davranışı vanilla JavaScript ile sağlanır.

### Gerekçe
- MVC monolith mimarisiyle uyumlu; ayrı bir frontend build pipeline gerektirmez
- Bootstrap Icons (`bi-*`) ikonlar için kullanılır
- Vanilla JS yeterli — jQuery veya React gereksiz bağımlılık yaratır
- Constitution Prensibi VIII: "Mobile-first responsive design is required"

### Kurallar
- `d-none` class'ı show/hide için kullanılır (JavaScript `classList.toggle`)
- Durum badge class'ları: `status-submitted`, `status-under-review`, `status-accepted`, `status-rejected`
- `@Html.Raw()` XSS riski nedeniyle yasak; `@Html.Encode()` veya Razor tag helper kullanılır

---

## Değişiklik Geçmişi

| Tarih | ADR | Değişiklik |
|---|---|---|
| 2026-05-14 | ADR-001–010 | İlk oluşturma — spec 001 ve spec 002 teknoloji kararları belgelendi |

---

**Versiyon**: 1.0.0 | **Oluşturan**: Speckit Plan Aşaması | **Son Güncelleme**: 2026-05-14
