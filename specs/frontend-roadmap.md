# InnovatEPAM Portal — Frontend Roadmap

**Son güncelleme:** 2026-05-14  
**Kapsam:** `src/InnovatEPAM.Portal/` içindeki sunum katmanı (Razor görünümleri, statik varlıklar, istemci betikleri).  
**User story kaynağı:** `specs/001-innovation-ideas` … `specs/006-idea-scoring-system` içindeki `spec.md` dosyaları.

---

## 1. Mevcut durum (özet envanter)

| Alan | Teknoloji / konum | Notlar |
|------|-------------------|--------|
| Sunucu işleme | ASP.NET Core MVC, Razor (`Views/**/*.cshtml`) | Tam sayfa POST/redirect akışı; çoğu etkileşim sunucu taraflı. |
| Stil | **Tailwind CSS 3.4** → derlenmiş çıktı `wwwroot/css/app.min.css` | Giriş: `app.css`; `npm run build:css` / `watch:css`. `@tailwindcss/forms` eklentisi aktif. |
| Eski / paralel stil | `wwwroot/css/portal/*.css`, `site.css` | Tasarım token’ları ve bileşen stilleri; `_Layout.cshtml` şu an **sadece** `app.min.css` referans veriyor — `portal/*` ve `site.css` layout’ta yüklenmiyor. |
| Kütüphaneler (wwwroot) | jQuery, jQuery Validation, Bootstrap JS/CSS paketleri | `lib/` altında yerel kopyalar mevcut; layout’ta Bootstrap CSS **yok**. |
| İstemci JS | `_Layout.cshtml` içinde satır içi sidebar/menü/sekme yardımcıları; `wwwroot/js/site.js`, `category-form.js` | `site.js`: Eski navbar/sidebar localStorage anahtarları ve Bootstrap `spinner-border` varsayımı; yeni kabuk için büyük ölçüde **kullanılmıyor** olabilir. |
| Bileşenler | Partial’lar (`_StatusBadge`, `_CategoryPicker`, `_ReviewStepperVertical`), ViewComponent `AdminWorkqueueSummary` | Admin özet rozetleri sunucu tarafı render. |

### Sayfa grupları (görünüm tutarlılığı)

- **TailAdmin tarzı kabuk:** `_Layout.cshtml` — sidebar, topbar, breadcrumb, `TempData` uyarıları, Inter font.
- **Ağır Tailwind kullanımı:** `Views/Ideas/Index.cshtml`, `Ideas/Dashboard.cshtml`, admin dashboard parçaları (`_Dashboard*.cshtml`), bir kısmı `Home/*`.
- **Bootstrap sınıfları + Bootstrap Icons (`bi-*`):** `Auth/Login.cshtml`, `Auth/Register.cshtml`, `Ideas/Edit.cshtml`, `Admin/Detail.cshtml`, `Admin/*Stage*.cshtml`, `Settings/BlindReview.cshtml`, `_CategoryPicker.cshtml`, `_ReviewStepperVertical.cshtml`, `Shared/Error.cshtml`, vb.

Bu ikinci grup `row`, `col-*`, `form-control`, `d-flex`, `w-100`, `alert-danger` gibi **Bootstrap’a özgü** yardımcı sınıflar kullanıyor; `_Layout` ise Bootstrap CSS yüklemediği için bu sayfaların ızgara ve form görünümü **tasarım sistemi ile uyumsuz veya eksik stilli** kalma riski taşıyor. `app.css` içinde `.btn-*`, `.card`, `.alert-*` için Tailwind tabanlı **isim çakışan** sınıflar tanımlı; ancak Bootstrap **ızgara** ve birçok yardımcı sınıf karşılığı tek başına Tailwind JIT ile üretilmiyor.

---

## 2. Hedef ilkeler

1. **Tek görsel sistem:** Üretimde tek bir stylesheet stratejisi (veya kontrollü, belgelenmiş iki katman: örn. sadece token + Tailwind).
2. **Erişilebilirlik:** Klavye, odak halkası, `aria-*` sürekliliği (layout’ta iyi başlangıç var; formlarda genişletilebilir).
3. **Performans:** Harici font ve minimal JS; gereksiz lib’lerin kaldırılması veya lazy yükleme.
4. **Bakım:** Satır içi script’leri modüler dosyalara taşımak; build pipeline’da CSS’nin CI’da üretildiğinden emin olmak.

### 2.1 User story → arayüz eşlemesi

Aşağıdaki tablolar, `specs/00x-*/spec.md` içindeki kullanıcı hikayelerinin **hangi ekranlarda** karşılanması gerektiğini özetler. Yeni UI çalışması yapılırken ilgili spec ve US numarası mutlaka referans alınmalıdır.

#### `001-innovation-ideas` — Temel portal

| US | Öncelik | Arayüz / görünüm | Tasarımda doğrulanacak noktalar |
|----|---------|------------------|----------------------------------|
| US1 | P1 | `Auth/Login`, `Auth/Register`, `Auth/*` çıkış akışı, `Home/AccessDenied` | Güvenilir hata mesajları (geçersiz giriş), korumalı rota sonrası yönlendirme mesajları, oturum sonu geri bildirimi |
| US2 | P1 | `Ideas/Create` | Başlık/açıklama zorunluluğu, dosya seçici, yükleme boyutu/format geri bildirimi (≤10 MB, izin verilen tipler) |
| US3 | P1 | `Ideas/Index`, `Ideas/Detail` | Liste: başlık, tarih, durum rozeti; detay: ekler, durum, kabul/red sonrası kullanıcı dostu karar özeti |
| US4 | P2 | `Admin/Index`, `Admin/Detail` | Tüm gönderiler tablosu; gönderici, tarih, durum; detayda indirilebilir ekler |
| US5 | P2 | `Admin/Detail` (durum güncelleme) | Durum seçimi ve kayıt sonrası hem admin hem göndericide tutarlı yansıma (TempData + liste) |

#### `002-smart-category-forms` — Kategori formları

| US | Öncelik | Arayüz / bileşen | Tasarımda doğrulanacak noktalar |
|----|---------|------------------|----------------------------------|
| US1 | P1 | `Ideas/Create`, `Ideas/Edit`, `_CategoryPicker`, `category-form.js`, kategori section’ları | Kategori seçilmeden ilerlenebilir alan düzeni (FR-001 sırası); kategori değişince alanların değişmesi **sayfa yenilemesiz**; yönergeler görünür |
| US2 | P1 | Aynı formlar + validation summary | Alan yanında inline hata; sunucu dönüşünde doğru bölümün görünür kalması (`initCategoryForm`) |
| US3 | P2 | `Ideas/Detail`, `Admin/Detail`, `Admin/Index` | Kategori etiketi + kategori alanları okunabilir blok; listede kategori sütunu/etiket |
| US4 | P2 | `Admin/Index` | Kategori filtresinin durum filtresi ile birlikte kullanılabilir UI’si; boş sonuçta **empty state** (hata mesajı değil) |

#### `003-draft-management` — Taslaklar

| US | Öncelik | Arayüz | Tasarımda doğrulanacak noktalar |
|----|---------|--------|----------------------------------|
| US1 | P1 | `Ideas/Create` — “Save as Draft” | Taslak kaydında tam validasyon beklenmez; başarı bildirimi |
| US2 | P2 | `Ideas/Index` (`focus=drafts`), taslak kartları/listesi | Net “Draft” rozeti; silme onayı kopyası; son değişiklik tarihi |
| US3 | P2 | `Ideas/Edit` | Ön doldurma, mevcut ek gösterimi, kaldır/değiştir |
| US4 | P3 | `Ideas/Edit` — Submit | Gönder öncesi tam validasyon mesajları; başarılı gönderim geri bildirimi |

#### `004-multi-stage-review` — Çok aşamalı inceleme

| US | Öncelik | Arayüz | Tasarımda doğrulanacak noktalar |
|----|---------|--------|----------------------------------|
| US1–US2 | P1 | `Admin/Detail` — Advance, notlar, geçmiş | Aşama ilerletme net CTA; not alanı opsiyonel ama görünür; geçmiş satırları: aşama, admin, tarih, not |
| US3 | P1 | `Admin/Detail`, `RecordDecision.cshtml` | Final karar + kapatma; Accepted/Rejected net görsel ayrım |
| US4 | P2 | `Admin/Detail`, `RevertStage.cshtml` | Geri alma nedeni zorunluluğu; geçmişte ileri/geri görsel farkı |
| US5 | P2 | `Ideas/Detail` (submitters), `Admin/Index` (aşama filtresi) | Salt okunur aşama gösterimi veya uygun bekleyen durum placeholder’ı |

#### `005-blind-review-mode` — Kör inceleme

| US | Öncelik | Arayüz | Tasarımda doğrulanacak noktalar |
|----|---------|--------|----------------------------------|
| US1 | P1 | `Admin/Index`, `Admin/Detail`, `Admin/ByStage` | Banner + maskelenmiş kimlik için tutarlı placeholder (örn. “Anonymous Submitter”); liste sütununun boş/yanlış hissettirmemesi |
| US2 | P2 | `Settings/BlindReview` | Açık/kapalı durum, onay/toast mesajları, kim değiştirdi bilgisi (spec FR-005 ile uyumlu) |
| US3 | P3 | `Admin/Detail` | Karar sonrası “kimlik görünür” bilgi bandı/rozet tasarımı |
| US4 | P4 | Submitters görünümleri | Blind mode **yalnızca admin yüzü** — göndericide ek maske yok (regresyon kontrol şablonuna ekle) |

#### `006-idea-scoring-system` — Puanlama

| US | Öncelik | Arayüz | Tasarımda doğrulanacak noktalar |
|----|---------|--------|----------------------------------|
| US1 | P1 | `Admin/Detail` — skor formu | 4 boyut (1–5); kısmi puan gönderimi; validasyon geri bildirimi; Draft/Accepted/Rejected’ta form gizli veya salt okunur |
| US2 | P2 | `Admin/Detail`, `Admin/Index` | Aggregate + boyut ortalamaları (admin); “No scores yet” empty state |
| US3 | P3 | `Admin/Detail` | “Remove My Score” onay/tehlike vurgusu; toplamların güncellenmesi |
| US4 | P4 | `Ideas/Detail` | Göndericide yalnızca **birleşik** skor etiketi; kim puanladı / boyut bazlı breakdown **görünmemeli** |

### 2.2 Frontend için zorunlu gereksinim özeti

| Kaynak | Gereksinim tipi | UI / UX karşılığı |
|--------|------------------|-------------------|
| 001 FR-009, SC-007 | Erişilebilir liste/detay | Durum filtresi, okunabilir tablo/kart yerleşimi, dokunmatik hedef boyutları |
| 001 FR-013–014 | Güvenlik & mesajlar | Kullanıcı girdisi güvenli sunum (`Html.Encode` uyumu), field-level ve özet validation ile tutarlı metin |
| 001 FR-005–006 | Dosya yükleme | Tür ve boyut sınırına uygun alan düzeni ve anlaşılır hata çerçeveleri |
| 002 FR-003–005 | Dinamik form | Client-side görünürlük + sunucu hatalarından sonra doğru panele scroll/odak (isteğe bağlı iyileştirme) |
| 003 FR-001 vs FR-008 | Taslak vs gönder | İki ana buton görsel hiyerarşisi ve “zorunlu alanlar yalnızca gönderimde” kullanıcı beklentisi |
| 004 | Aşama + geçmiş | Zaman çizelgesi / stepper tasarımının okunabilirliği (`_ReviewStepperVertical` ile uyum) |
| 005 FR-001–002 | Maskeleme | Admin görünümünde hiçbir sızıntı yok; placeholder tasarım kılavuzu |
| 006 FR-002 | Skor doğrulama | 1–5 aralığı net (radio, select veya number + yardımcı metin); hata mesajı metni spec ile uyumlu |
| Genel (`001` başarı ölçütleri) | Süre kullanılabilirlik hedefleri | Kritik akışlar için mümkün olduğunca az adım ve tek ekranda tamamlanabilir düzen |

**Gereksiz / dışarıda (MVP uyarısı):** Tam metin arama, e-posta bildirimi UI’si, mobil native uygulama — bu maddeler için arayüz genişletmesi yapılmadan önce ürün kapsamı güncellenmelidir (`001` assumptions).

---

## 3. Şablonlar (tasarım, geliştirme, QA)

Bu şablonlar yeni ekran veya user story doğrultusunda UI işi açılırken **aynı yapıda** kullanılmalıdır. Metni kopyalayıp sprint/issue açıklamasına veya ayrı bir `design-notes.md` dosyasına yapıştırın.

### Şablon A — Sayfa özeti (Page brief)

```markdown
## Sayfa: <Route / View adı>

### İlişkili user story’ler
- Spec: `specs/___/spec.md` — US__: <kısa başlık>

### Roller
- [ ] Submitter | [ ] Admin | [ ] Anonim

### Zorunlu UI öğeleri
1. ...
2. ...

### Empty / edge durumları
- Boş liste: ...
- Blind review açık: ...
- Yetkisiz erişim: ...

### Erişilebilirlik kontrolü
- Odak sırası, form label’ları, canlı bölgeler (aria-live) ihtiyacı: ...

### Referans görünümler
- Mevcut: `Views/...`
```

### Şablon B — User story UI kabul kontrolü

```markdown
## US__ — <Başlık> — UI Acceptance

**Spec bağlantısı:** `specs/___/spec.md`

| # | Acceptance (spec’ten) | UI kanıtı (ekran/link) | Durum |
|---|----------------------|-------------------------|--------|
| 1 | Given ... When ... Then ... | ... | ☐ |
| 2 | ... | ... | ☐ |

**Regresyon:** Etkilenen diğer sayfalar: ...
```

### Şablon C — Bileşen / partial spesifikasyonu

```markdown
## Bileşen: <Ad>

### Amaç
<User story veya FR referansı>

### Props / model alanları
- ...

### Durumlar
- Varsayılan / yükleme / hata / boş içerik

### Tasarım tokenları
Renk / spacing: `app.css` @layer components veya `portal/tokens.css` ile uyum

### Test notları
- Klavye, ekran okuyucu, mobil breakpoint
```

### Şablon D — Responsive & breakpoint notu

```markdown
## Breakpoint doğrulama — <Sayfa>

- [ ] ≥1280 — sidebar tam, tablolar taşmadan
- [ ] 768–1279 — topbar arama/düzen
- [ ] <768 — mobil menü, formlar tek sütun, dosya yükleme alanı dokunmalı uygun

**Komut paleti:** Admin ⌘K (varsa): odak tuzakları kontrol edildi mi?
```

---

## 4. Roadmap fazları

> **Not:** Fazlar, yukarıdaki user story haritasına göre önceliklendirilir; yeni özellik UI’si önce ilgili `specs/00x/` US satırlarına bağlanır, ardından bu fazlardan teknik iş seçilir.

### Faz A — Temel tutarlılık ve düzeltmeler (1–2 hafta)

| Öncelik | Görev | Çıktı |
|--------|--------|--------|
| P0 | **Bootstrap↔Tailwind uyumu kararı:** Ya `_Layout`’a yalın Bootstrap grid + icons CSS eklenerek geçiş dönemi “hibrit” resmileştirilsin ya da Bootstrap sınıflı tüm görünümler Tailwind `app.css` bileşenlerine taşınsın. | Tutarlı sayfa görünümü; QA kontrol listesi. |
| P0 | **`_ValidationScriptsPartial`:** jQuery Validate unobtrusive için **jquery.js** sırasının garanti altına alınması (partial veya layout’ta eksikse ekleme). | Form doğrulamanın tarayıcıda güvenilir çalışması. |
| P1 | **`portal/*.css` ve `site.css`:** Ya layout’a dahil edilip Tailwind ile çakışma analizi yapılsın ya da kullanılmıyorsa arşivlensin. | Tek kaynak doğrusu; gereksiz dosya karmaşası azalması. |
| P1 | **`site.js`:** Ya `_Layout`/ilgili sayfalarda kullanılıyor doğrulansın ya da kaldırılsın / yeni kabuk ile hizalanan tek dosyada birleştirilsin. | Ölü kod ve çift sidebar localStorage anahtarlarının (`iepamSidebarCollapsed` vs `portal.sidebar.collapsed`) temizlenmesi. |

### Faz B — Tasarım sistemi sıkılaştırma (2–4 hafta)

| Görev | Açıklama |
|-------|----------|
| Auth sayfalarını yeni dile taşıma | Login/Register: gradient auth arka planı ile uyumlu Tailwind kartları (`Create.cshtml` çizgisine yaklaştırma). |
| Admin detay ve aşama formları | `Admin/Detail`, `AdvanceStage`, `RecordDecision`, `RevertStage`: `bi` ikonlarını inline SVG veya tek bir ikon seti stratejisi ile değiştirme; Bootstrap grid → Tailwind grid/flex. |
| Ortak form bileşenleri | Tekrarlayan alan blokları için partial veya Tag Helper; `form-input` / `form-label` ile ASP.NET expression’ların hizalanması. |
| Durum ve rozetler | `_StatusBadge` + `Index` donut/metrikler için ortak renk/token kullanımı (`tokens.css` ile `app.css` birleştirme opsiyonu). |

### Faz C — Geliştirici deneyimi ve kalite (sürekli)

| Görev | Açıklama |
|-------|----------|
| CI’da CSS build | PR/merge öncesi `npm ci` + `npm run build:css`; `app.min.css` commit politikasını netleştirme (kaynak: `app.css`, çıktı: min). |
| Lint / format | İsteğe bağlı Prettier (`cshtml` sınırlı), Stylelint veya Tailwind sınıf sıralaması. |
| Erişilebilirlik turu | Admin tabloları, komut paleti araması (`admin-global-search`), modal benzeri akışların odak tuzakları. |

### Faz D — Ürün ve etkileşim (orta–uzun vade)

Bu proje şu an **SSR + form gönderimi** ağırlıklı; aşağıdakiler kullanıcı ihtiyaçlarına göre sıraya konmalıdır.

| Yön | Seçenekler |
|-----|-------------|
| Daha akıcı listeler | HTMX veya küçük `fetch()` katmanı ile sayfalamada tam yenilemesiz güncelleme. |
| Zengin metin | Fikir açıklaması için kontrollü rich text (güvenlik ve XSS politikası ile). |
| Gerçek zamanlı | SignalR ile kuyruk/inceleme sayıları (`AdminWorkqueueSummary` önbelleği). |
| SPA / API |İhtiyaç halinde kritik bir modül için ayrıştırılmış frontend (risk: SSR ile çift paralel yaşam). |

---

## 5. Riskler ve varsayımlar

- **`ScoreController`** ve çoğu controller **JSON API değil** MVC redirect döner; SPA geçişi API genişletmesini gerektirir.
- **Çift yerelleştirme:** Arayüzde Türkçe/İngilizce karışık etiketler (ör. sidebar “Dashboard” vs breadcrumb “Home”); roadmap’te ayrı bir “içerik dili standardı” maddesi açılabilir.
- **`category-form.js`:** Bootstrap `d-none` kullanıyor — Tailwind projesinde `hidden` ile hizalamak uzun vadede tutarlılık sağlar.

---

## 6. Kontrol listesi (hızlı doğrulama)

### Genel teknik

- [ ] Giriş, kayıt ve fikir düzenleme formları tüm breakpoints’te okunabilir ve tıklanabilir mi?  
- [ ] `_ValidationScriptsPartial` kullanan sayfalarda jQuery sırası doğru mu?  
- [ ] `npm run build:css` sonrası prod’da yalnızca güncel `app.min.css` servis ediliyor mu?  
- [ ] Admin global arama (⌘/Ctrl+K) ve mobil sidebar klavye ile kapanıyor mu?  

### User story uyumu — örnek smoke

- [ ] **002**: Üç kategori için alanların görünümü ve kategori değişince önceki kategori verilerinin sıfırlanma beklentisi  
- [ ] **003**: Taslakta “Kaydet” vs gönderimde tam validasyon farkının arayüzde anlaşılır olması  
- [ ] **005**: Blind açıkken admin liste/detay maskelemesi; karar sonrası kimlik görünür (US3)  
- [ ] **006**: Admin skor formu (1–5), aggregate liste/detay; göndericide yalnızca toplam skor bilgisi  

---

*Bu doküman, depo içi `Views/`, `wwwroot/`, `package.json` ile `specs/001`–`006` özellik spesifikasyonlarına dayanır; öncelik için ilgili `spec.md` ile senkron tutulmalıdır.*
