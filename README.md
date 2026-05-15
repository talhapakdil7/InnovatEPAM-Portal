# 🚀 InnovatEPAM Portal

**İnovasyonista fikirlerin merkezleştirilmiş, yapılandırılmış ve skorlanmış değerlendirilmesini sağlayan kurumsal yönetim sistemi.**

## 📋 Özet

InnovatEPAM Portal, çalışanların inovasyon fikirlerini sunmasını, yöneticilerin bunları sistemli olarak incelemesini, puanlamasını ve kararlaştırmasını sağlayan ASP.NET Core tabanlı bir web uygulamasıdır.

### 🎯 Ana Özellikler

- **Fikir Yönetimi:** Taslak kaydı, revizyon ve sunma
- **Kategorilere Dayalı Formlar:** Teknik, İş Süreci, Müşteri Çözümü gibi kategori-spesifik bilgi toplama
- **Çok Aşamalı İnceleme:** Triaj → Aktif İnceleme → Karar (Kabul/Red)
- **Boyut Tabanlı Puanlama:** Yenilik, Teknik Uygulanabilirlik, İş Etkisi, Uygulama Değeri
- **Kör İnceleme Modu:** Opsiyonel olarak inceleyici kimlik gizleme
- **Eş Zamanlı İçerik Koruma:** Race condition ve optimistic concurrency handling
- **Ek Dosya Desteği:** Güvenli depolama ve virüs kontrolü
- **Denetim İzleri:** Tüm durum değişiklikleri ve kararları kaydı

### 📸 Ekran görüntüleri

Aşağıdaki görseller `screenshots/` klasöründedir (InnovatEPAM Portal arayüzü):

| # | Önizleme |
|---|----------|
| 1 | ![InnovatEPAM Portal — ekran görüntüsü 1](screenshots/Screenshot%202026-05-15%20at%2014.36.59.png) |
| 2 | ![InnovatEPAM Portal — ekran görüntüsü 2](screenshots/Screenshot%202026-05-15%20at%2014.37.17.png) |
| 3 | ![InnovatEPAM Portal — ekran görüntüsü 3](screenshots/Screenshot%202026-05-15%20at%2014.37.44.png) |
| 4 | ![InnovatEPAM Portal — ekran görüntüsü 4](screenshots/Screenshot%202026-05-15%20at%2014.39.06.png) |
| 5 | ![InnovatEPAM Portal — ekran görüntüsü 5](screenshots/Screenshot%202026-05-15%20at%2014.39.40.png) |

Dosya adları: `Screenshot 2026-05-15 at … .png`. Yerelde veya GitHub’da görüntülenmezse dosya yolunun doğru olduğundan ve görsellerin commit edildiğinden emin olun.

---

## 🛠️ Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| **Backend** | ASP.NET Core 10 (C#) |
| **Veritabanı** | PostgreSQL + Entity Framework Core |
| **Kimlik** | ASP.NET Identity |
| **Frontend** | Razor Pages + Tailwind CSS |
| **Mimarı** | Repository Pattern, Service Layer, MVC |

---

## 📦 Proje Yapısı

```
InnovatEPAM Portal
├── screenshots/               # Proje ekran görüntüleri (README’de referanslı)
├── docs/
│   └── KURULUM.md              # Kurulum ve işletim rehberi (Türkçe)
├── specs/
│   ├── frontend-roadmap.md
│   └── 001-006/                # Spec dökümanları her özellik için
├── src/InnovatEPAM.Portal/
│   ├── Controllers/            # HTTP endpoints
│   ├── Services/               # İş mantığı
│   ├── Repositories/           # Veri erişimi (EF Core)
│   ├── Models/                 # Domain modeller
│   ├── DTOs/                   # Veri transfer objeleri
│   ├── Views/                  # Razor Pages (Tailwind UI)
│   ├── Middleware/             # Custom pipelines (İstisna yönetimi, vb.)
│   └── Data/                   # DbContext, Migrations
└── README.md                   # Bu dosya
```

---

## 🚀 Hızlı Başlangıç

### Gereksinimler

- **.NET 8+ SDK**
- **PostgreSQL 12+**
- **Node.js 18+** (Tailwind CSS derleme için)

### 1. Depoyu Klonlayın

```bash
git clone https://github.com/talhapakdil7/InnovatEPAM-Portal.git
cd "InnovatEPAM Portal"
```

### 2. Bağlantı Dizesini Yapılandırın

`appsettings.Development.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=InnovatEPAM;Username=postgres;Password=your_password"
  }
}
```

### 3. Veritabanı Migration'larını Çalıştırın

```bash
dotnet ef database update
```

*(Uygulama açılışında `ApplyMigrationsOnStartup: true` ise otomatik olur)*

### 4. İlk Admin Hesabı Oluşturun

Arayüzden `Register` ile bir hesap oluşturun, ardından veritabanında `Admin` rolü atayın.

### 5. CSS'i Derleyin (Tailwind)

```bash
cd src/InnovatEPAM.Portal
npm install
npm run build:css
```

### 6. Uygulamayı Çalıştırın

```bash
dotnet run
```

Tarayıcıda `https://localhost:5001` adresine gidin.

---

## 🔐 Eş Zamanlı Yazma Koruması (Concurrency Handling)

Sistem, veritabanı operasyonları sırasında race condition'lara karşı korunur:

- **DbUpdateConcurrencyException Detection:** Diğer işlemler tarafından silinen/modifiye edilen kayıtları detect eder
- **Kullanıcı-Dostu Hata Mesajları:** Anlaşılabilir açıklama ve "Yenile & Tekrar Dene" düğmesi
- **Graceful Retry:** Sayfa yenilemesi sonrası en güncel verileri yükler

**Örnek Senaryo:**
1. Kullanıcı A bir fikri düzenlemek için açar
2. Kullanıcı B aynı fikri siler
3. Kullanıcı A'nın güncelleme isteği başarısız olur
4. Sistem: *"Bu fikir başka bir işlemce değiştirildi. Lütfen sayfayı yenileyip tekrar deneyin."*
5. Kullanıcı "Yenile & Tekrar Dene"ye tıklar, sayfa yenilenir

---

## 📚 Ana Özellikler Detay

### 1. **Taslak Yönetimi**
- Çalışanlar fikir taslakları kaydedebilir (Çerçeve doğrulaması olmadan)
- Taslakları sunmadan önce kaç kez düzenleyebilirler
- Sunmadan sonra yalnızca puanlama başlanmadan önce revize edilebilir

### 2. **Kategori-Spesifik Formlar**
- **Teknik İyileştirme:** Alan, Çaba, Fayda
- **İş Süreci İyileştirmesi:** Departman, Acı Noktası, Tasarruf
- **Müşteri Çözümü:** Segment, Problem, Etki

### 3. **Çok Aşamalı Gözden Geçirme**
- **Triaj (Submitted):** İlk teslim durumu
- **Aktif İnceleme (UnderReview):** İlk puanlama işlemi tarafından tetiklenir
- **Karar (Accepted/Rejected):** Yönetici tarafından kapatılır

### 4. **Boyut Tabanlı Puanlama**
Yöneticiler her fikri 4 kriterle puanlarlar (1-5 ölçeği):
- 💡 **Yenilik** – Fikirle ilgili önerilen fikrin orijinalliği
- ⚙️ **Teknik Uygulanabilirlik** – Teknik olarak gerçekçiliği
- 📈 **İş Etkisi** – Şirkete potansiyel ekonomik/stratejik katkı
- ⏱️ **Uygulama Değeri** – Zaman ve kaynağa göre uygulanabilirlik

Puanlar birden fazla inceleyici tarafından girilir; sistem tüm skorlar için örtüştürülmüş ortalamaları hesaplar.

### 5. **Kör İnceleme**
Ayarlar sayfasında yöneticiler tüm sistemde kör incelemeyi etkinleştirebilir. İnceleyici isimlerini gizler; puanlama hala toplanır, ancak denetim izleri korunur.

---

## 🗂️ Veritabanı Modeli

### Ana Tablolar

| Tablo | Amaç |
|-------|------|
| `Ideas` | Fikir kaydı (başlık, açıklama, durum, vb.) |
| `IdeaScores` | İnceleyici puanlamaları (4 boyut × çok inceleyici) |
| `IdeaAttachments` | Ek dosya depolaması (işaretler, PDF'ler) |
| `AuditLogs` | Durum değişimi ve karar geçmişi |
| `AspNetUsers` | Çalışan ve yönetici hesapları |
| `AspNetRoles` | `Submitter` ve `Admin` rollerine sahip |

---

## 🔧 Geliştirme

### Backend Yapısı

```csharp
Controllers/
  ├── IdeasController      // Çalışan fikir endpoints
  ├── AdminController      // Yönetici inceleme endpoints
  └── ...

Services/
  ├── IdeaService         // Fikir CRUD ve iş mantığı
  ├── ScoreService        // Puanlama aggregation
  └── AuthService         // Kimlik yönetimi

Repositories/
  ├── IIdeaRepository     // Interface
  └── IdeaRepository      // EF Core implementation
```

### Middleware

- **ExceptionHandlingMiddleware:** Global istisna yakalaması, concurrency hataları için özel muamele

### Validators

- FluentValidation kullanan custom kurallar
- Fikir oluşturma, taslak güncellemeleri, puanlama girdileri için

---

## 📋 Ortam Değişkenleri

### Üretim Yapılandırması

`appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_production_connection_string"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Database": {
    "ApplyMigrationsOnStartup": false
  }
}
```

### Geliştirme Yapılandırması

`appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=InnovatEPAM_Dev;..."
  },
  "Database": {
    "ApplyMigrationsOnStartup": true
  }
}
```

---

## 🐛 Sorun Giderme

### Veritabanı Bağlantısı Başarısız Oldu

```bash
# ConnectionString'i doğrulayın
psql -U postgres -h localhost -d InnovatEPAM

# Migration'ları el ile çalıştırın
dotnet ef database update
```

### Tailwind CSS Derlenmiyor

```bash
cd src/InnovatEPAM.Portal
npm install
npm run build:css
```

### Port 5001 Zaten Kullanımda

```bash
dotnet run --urls="https://localhost:5002"
```

---

## 📖 İlave Belgeler

- 📸 **[screenshots/](screenshots/)** – Proje arayüzü ekran görüntüleri (README’de de gömülü)
- 📄 **[KURULUM.md](docs/KURULUM.md)** – Detaylı kurulum ve işletim rehberi (Türkçe)
- 📋 **[Spec Dökümanları](specs/)** – Her özellik için teknik tasarım

---

## 👨‍💻 Katkıda Bulunmak

1. Bu depoyu fork edin
2. Özellik dalı oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişiklikleri commit edin (`git commit -m 'Add amazing feature'`)
4. Dala push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

---

## 📝 Lisans

Bu proje [MIT Lisansı](LICENSE) altında yayınlanmıştır.

---

## 💬 İletişim

Sorular veya geri bildirim için:
- 📧 **E-posta:** [Your Email]
- 🐙 **GitHub Issues:** [Yeni Issue Açın](https://github.com/talhapakdil7/InnovatEPAM-Portal/issues)

---

## 🙏 Teşekkürler

- ASP.NET Core ekibine
- Entity Framework Core için
- Tailwind CSS topluluğuna

---

**Oluşturma Tarihi:** Mayıs 2026  
**Son Güncelleme:** 15 Mayıs 2026
