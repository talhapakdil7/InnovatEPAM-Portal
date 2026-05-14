# Tasks: Smart Category-Adaptive Submission Forms

**Input**: Design documents from `specs/002-smart-category-forms/`

**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ideas.md ✓, quickstart.md ✓

**Tests**: Manual testing per `quickstart.md` (10 scenarios). No unit test tasks — MVP manual-testing-first approach (per constitution Gate 4 conditional pass).

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Paralel çalışabilir (farklı dosyalar, bağımlılık yok)
- **[Story]**: Hangi user story'ye ait (US1–US4)
- Her görevde tam dosya yolu belirtilmiştir

## Path Conventions

- **Proje kökü**: `src/InnovatEPAM.Portal/`
- **Controllers**: `src/InnovatEPAM.Portal/Controllers/`
- **Services**: `src/InnovatEPAM.Portal/Services/`
- **Models**: `src/InnovatEPAM.Portal/Models/`
- **Views**: `src/InnovatEPAM.Portal/Views/`
- **wwwroot/js**: `src/InnovatEPAM.Portal/wwwroot/js/`

---

## Phase 1: Setup

**Amaç**: Yeni dosya iskeletlerini oluştur

- [x] T001 `src/InnovatEPAM.Portal/wwwroot/js/category-form.js` boş dosyasını oluştur (Phase 3'te doldurulacak)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Amaç**: Tüm user story'lerin bağımlı olduğu temel altyapı değişiklikleri

**⚠️ KRİTİK**: Bu phase tamamlanmadan hiçbir user story çalışmasına başlanamaz

- [x] T002 `src/InnovatEPAM.Portal/Models/Idea.cs` dosyasına nullable `Category` (string?, max 50) ve `CategoryData` (string?) property'leri ekle
- [x] T003 [P] `src/InnovatEPAM.Portal/Models/CategoryDefinitions.cs` yeni dosyasını oluştur — `CategoryFieldDefinition`, `CategoryDefinition` sınıflarını ve `CategoryDefinitions` static registry'yi 3 kategori + 9 alan tanımıyla yaz (data-model.md'deki field tablosuna göre)
- [x] T004 [P] `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` içindeki `CreateIdeaViewModel`'e `Category` string? property'si ve 9 kategori alanı property'sini ekle (`TechArea`, `TechEffort`, `TechBenefit`, `ProcDepartment`, `ProcPainPoint`, `ProcSavings`, `ClientSegment`, `ClientProblem`, `ClientImpact`)
- [x] T005 [P] `src/InnovatEPAM.Portal/Data/ApplicationDbContext.cs` içindeki `Idea` entity konfigürasyonuna `Category` max-length(50) + nullable ve `CategoryData` nullable + `HasIndex(i => i.Category)` ekle
- [x] T006 Migration el ile oluşturuldu: `Data/Migrations/20260514130000_AddIdeaCategoryFields.cs` + `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs` güncellendi

**Checkpoint**: Foundation hazır — user story implementasyonları başlayabilir

---

## Phase 3: User Story 1 — Dinamik Form Adaptasyonu (Priority: P1) 🎯 MVP

**Hedef**: Submitter kategori seçtiğinde ilgili alanlar sayfa yenilenmeden görünür; kategori değiştirince eski alanlar temizlenir

**Bağımsız Test**: Login ol, `/Ideas/Create` aç, her kategoriyi seç, doğru alanların anında geldiğini doğrula — admin etkileşimi gerekmez

### US1 Implementasyonu

- [x] T007 [US1] `src/InnovatEPAM.Portal/Views/Ideas/Create.cshtml` dosyasını güncelle:
  - Formun en başına (Title'dan önce) `Category` `<select>` dropdown ekle (`--Select Category--` seçeneği ile)
  - Aşağısına 3 adet gizli `<div class="d-none" id="fields-TechnicalImprovement">` / `fields-ProcessImprovement` / `fields-ClientSolution` section'ı ekle
  - Her section içinde o kategoriye ait alanları (data-model.md'deki tanımlara göre: label, input tipi, guidance hint, `asp-validation-for`) yaz
  - `<script src="~/js/category-form.js"></script>` referansını view sonuna ekle
- [x] T008 [US1] `src/InnovatEPAM.Portal/wwwroot/js/category-form.js` dosyasını yaz:
  - `DOMContentLoaded` event'inde `#Category` select'i dinle
  - `change` event'inde: tüm `.category-fields` div'lerini `d-none` yap, seçilen kategorinin section'ını göster
  - Kategori değiştiğinde önceki kategorinin input/textarea/select alanlarını temizle (value = '')
  - Sayfa ilk yüklendiğinde mevcut `Category` değeri varsa (model hatası sonrası) doğru section'ı göster

**Checkpoint**: Kategori dropdown çalışıyor, alanlar JS ile gösterilip gizleniyor — submit olmadan test edilebilir

---

## Phase 4: User Story 2 — Validasyon ve Kalıcı Kayıt (Priority: P1)

**Hedef**: Kategori seçilmeden ve gerekli kategori alanları doldurulmadan form submit edilemesin; başarılı submit'te kategori verisi DB'ye kaydedilsin

**Bağımsız Test**: Her kategori için eksik alanlı submit dene, inline hata mesajlarını doğrula; tam dolu submit'te fikir oluşturulduğunu doğrula

### US2 Implementasyonu

- [x] T009 [US2] `src/InnovatEPAM.Portal/Validators/CreateIdeaValidator.cs` dosyasını güncelle:
  - `RuleFor(x => x.Category).NotEmpty()` ekle → "Please select a category."
  - `When(x => x.Category == CategoryDefinitions.TechnicalImprovement, ...)` bloğu: TechArea, TechEffort, TechBenefit için required + MaximumLength kuralları
  - `When(x => x.Category == CategoryDefinitions.ProcessImprovement, ...)` bloğu: ProcDepartment (req, 100), ProcPainPoint (req, 500), ProcSavings (optional, 200)
  - `When(x => x.Category == CategoryDefinitions.ClientSolution, ...)` bloğu: ClientSegment (req, 200), ClientProblem (req, 500), ClientImpact (req, 300)
- [x] T010 [US2] `src/InnovatEPAM.Portal/Services/IdeaService.cs` içindeki `CreateIdeaAsync` metodunu güncelle:
  - `idea.Category = vm.Category` ata
  - Seçili kategoriye göre ilgili VM alanlarından `Dictionary<string, string>` oluştur
  - `System.Text.Json.JsonSerializer.Serialize(dict)` ile `idea.CategoryData`'ya yaz
  - Yalnızca seçili kategorinin alanlarını serialize et (diğer kategorilerin alanlarını yoksay)

**Checkpoint**: US1 + US2 birlikte çalışıyor — kategori seçme, doldurma, submit ve DB'ye yazma end-to-end test edilebilir

---

## Phase 5: User Story 3 — Detay Sayfalarında Kategori Gösterimi (Priority: P2)

**Hedef**: Fikir detay sayfasında (Submitter ve Admin için) kategori etiketi ve kategori alanları görünsün; eski fikirler "Uncategorized" göstersin

**Bağımsız Test**: Kategori ile oluşturulan fikrin detay sayfasını Submitter ve Admin olarak aç; eski fikrin detay sayfasında hata olmadığını doğrula

### US3 Implementasyonu

- [x] T011 [P] [US3] `src/InnovatEPAM.Portal/DTOs/IdeaListItemDTO.cs` dosyasına `Category` (string?) ve `CategoryDisplayName` (string?) property'lerini ekle
- [x] T012 [P] [US3] `src/InnovatEPAM.Portal/DTOs/IdeaDetailDTO.cs` dosyasına `Category` (string?), `CategoryDisplayName` (string?), `CategoryDataFields` (Dictionary<string, string>) property'lerini ekle
- [x] T013 [US3] `src/InnovatEPAM.Portal/Mapping/AutoMapperProfile.cs` dosyasını güncelle:
  - `Idea → IdeaListItemDTO`: `Category` direkt map, `CategoryDisplayName` Ignore → servis katmanında çözümleniyor (C1 bulgusu uygulandı)
  - `Idea → IdeaDetailDTO`: `Category` map; `CategoryDisplayName` + `CategoryDataFields` Ignore → `IdeaService.GetIdeaDetailAsync` içinde JSON deserialize + label çözümleme yapılıyor (C1 uyumu)
- [x] T014 [US3] `src/InnovatEPAM.Portal/Views/Ideas/Detail.cshtml` dosyasını güncelle:
  - `Model.Idea.Category` null değilse veya varsa "Category" section kartı ekle
  - Kategori display name badge'ini göster
  - `CategoryDataFields` dict'ini döngüyle label: value satırları olarak render et
  - Legacy fikir (Category null) için "Uncategorized" badge göster
- [x] T015 [US3] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` dosyasını güncelle:
  - Submitter detail ile aynı kategori section'ı ekle (admin review paneline uygun yerleşim)

**Checkpoint**: Submitter ve Admin detay sayfalarında kategori bilgisi görünüyor; eski fikirler hatasız gösteriliyor

---

## Phase 6: User Story 4 — Admin Kategori Filtresi (Priority: P2)

**Hedef**: Admin fikir listesinde kategori sütunu ve kategori filtresi çalışsın; durum filtresiyle birlikte kullanılabilsin

**Bağımsız Test**: Farklı kategorilerden fikirler varken admin listesinde kategori filtrele; birleşik filtre (status + category) dene; sonuç yokken boş state mesajı görün

### US4 Implementasyonu

- [x] T016 [P] [US4] `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` dosyasını güncelle:
  - `AdminIdeaListViewModel`'e `CategoryFilter` (string?) ve `AvailableCategories` (Dictionary<string,string>) ekle (S1: IdeaListViewModel'e eklenMEdi)
- [x] T017 [P] [US4] `src/InnovatEPAM.Portal/Services/Interfaces/IIdeaService.cs` dosyasını güncelle:
  - `GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null)` imzasına güncelle
  - `GetMyIdeasAsync` imzası değiştirilMEdi (S1 bulgusu: submitter filtresi spec dışı)
- [x] T018 [US4] `src/InnovatEPAM.Portal/Services/IdeaService.cs` dosyasını güncelle:
  - `GetAllIdeasAsync`: `categoryFilter` null/boş değilse `ideas.Where(i => i.Category == categoryFilter)` uygula
  - `GetMyIdeasAsync`: kategori filtresi EKLENMEDİ (S1 uyumu)
- [x] T019 [US4] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` içindeki `Index` action'ını güncelle:
  - `string? categoryFilter` parametresi ekle
  - `GetAllIdeasAsync` çağrısına `categoryFilter` geçir
  - `AdminIdeaListViewModel`'e `CategoryFilter` ve `AvailableCategories` doldur
- **[SKIPPED S1]** T020: `IdeasController.Index` kategori filtresi eklenmedi — spec US4 sadece admin filtresi tanımlıyor
- [x] T021 [US4] `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml` dosyasını güncelle:
  - Tablo başlığına "Category" sütunu eklendi
  - Status + Category dropdown filtreleri eklendi
- [x] T022 [US4] `src/InnovatEPAM.Portal/Views/Ideas/Index.cshtml` dosyasını güncelle:
  - Her fikir kartına küçük kategori badge'i eklendi (filtre dropdown EKLENMEDİ — S1 uyumu)

**Checkpoint**: Tüm user story'ler bağımsız ve birlikte çalışıyor

---

## Phase 7: Polish & Cross-Cutting Concerns

**Amaç**: Kalite, geriye uyumluluk ve doğrulama

- [x] T023 [P] XML dokümantasyon yorumları eklendi: `CategoryDefinitions.cs` (tüm class + property + const), `IdeaListItemDTO.cs`, `IdeaDetailDTO.cs`, `IIdeaService.cs`, `IdeaService.cs` (`CreateIdeaAsync`, `BuildCategoryData`, `EnrichCategoryDisplayNames`) — C2 bulgusu uygulandı
- [ ] T024 [P] Geriye uyumluluk doğrulaması: migration uygulandıktan sonra mevcut fikirler (Category = NULL) tüm view'larda hatasız "Uncategorized" gösterdiğini doğrula
- [ ] T025 `quickstart.md` senaryoları 1–10'u sırayla çalıştır ve sonuçları kaydet

---

## Bağımlılıklar ve Çalıştırma Sırası

### Phase Bağımlılıkları

- **Setup (Phase 1)**: Bağımlılık yok — hemen başlanabilir
- **Foundational (Phase 2)**: Setup'a bağlı — TÜM user story'leri bloke eder
- **User Stories (Phase 3–6)**: Foundational tamamlanınca başlayabilir
  - US1 + US2 (P1): Paralel başlanabilir ama birbirini tamamlar
  - US3 + US4 (P2): US1/US2 sonrasında veya paralel (farklı developer'larla)
- **Polish (Phase 7)**: Tüm istenen user story'ler tamamlanınca

### User Story Bağımlılıkları

- **US1 (P1)**: Foundational tamamlandıktan sonra — T003 (CategoryDefinitions) + T004 (ViewModel) gerekli
- **US2 (P1)**: US1 ile paralel başlayabilir; T003 (CategoryDefinitions) + T004 (ViewModel) gerekli
- **US3 (P2)**: Foundational tamamlandıktan sonra; T002 (Idea model) gerekli
- **US4 (P2)**: US3'teki T011/T012 (DTO değişiklikleri) + T013 (AutoMapper) tamamlanınca başlanabilir

### Her User Story İçinde

- Models/Static classes → Validators → Services → Controllers → Views sırası izlenir
- Paralel [P] görevler farklı dosyalarda olduğu için eş zamanlı çalışabilir
- Her story kendi checkpoint'inde bağımsız test edilmeli

### Paralel Fırsatlar

- Phase 2'de T003, T004, T005 birlikte çalışabilir (farklı dosyalar)
- Phase 5'te T011, T012 birlikte çalışabilir
- Phase 6'da T016, T017 birlikte çalışabilir
- Phase 7'de T023, T024 birlikte çalışabilir

---

## Paralel Örnek: Phase 2 (Foundational)

**Tek developer — sıralı önerilen sıra**:

1. T002 → `Idea.cs` değişiklikleri
2. T003 + T004 + T005 paralel → CategoryDefinitions + ViewModel + DbContext
3. T006 → Migration oluştur (T002 + T005 tamamlanınca)

**İki developer ile paralel**:

Developer A:
- T002 → Idea.cs
- T003 → CategoryDefinitions.cs
- T006 → Migration (T002 + T005 beklenir)

Developer B:
- T004 → ViewModel
- T005 → DbContext

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Phase 1 + Phase 2 tamamla (Foundational)
2. Phase 3 tamamla (US1 — dinamik form görsel)
3. Phase 4 tamamla (US2 — validasyon + kayıt)
4. **DUR ve DOĞRULA**: `quickstart.md` senaryo 1–5'i test et
5. Çalışıyorsa Phase 5 + 6'ya geç

### Incremental Delivery

1. Phase 1–2 → Altyapı hazır
2. Phase 3–4 → Dinamik form çalışıyor, test et (P1 MVP teslim)
3. Phase 5 → Detay sayfaları kategori bilgisini gösteriyor, test et
4. Phase 6 → Admin filtresi çalışıyor, test et
5. Phase 7 → Polish + tam regresyon testi

---

## Görev Sayısı Özeti

| Phase | Görev Sayısı | Paralel Fırsat |
|---|---|---|
| Phase 1: Setup | 1 | — |
| Phase 2: Foundational | 5 | T003, T004, T005 paralel |
| Phase 3: US1 | 2 | — |
| Phase 4: US2 | 2 | — |
| Phase 5: US3 | 5 | T011, T012 paralel |
| Phase 6: US4 | 7 | T016, T017 paralel |
| Phase 7: Polish | 3 | T023, T024 paralel |
| **Toplam** | **25** | |

- US1 (P1): 2 görev
- US2 (P1): 2 görev
- US3 (P2): 5 görev
- US4 (P2): 7 görev
