# Tasks: Blind Review Mode

**Input**: Design documents from `specs/005-blind-review-mode/`

**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/settings.md ✓, quickstart.md ✓

**Tests**: Manual testing per `quickstart.md` (10 scenarios + 7 regression). No unit test tasks — MVP manual-testing-first approach (per constitution Gate 4 conditional pass).

**Organization**: Phases 1–2 are foundational. Phases 3–6 map to US1–US4. Phase 7 is polish.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Paralel çalışabilir (farklı dosyalar, bağımlılık yok)
- **[Story]**: Hangi user story'ye ait (US1–US4)
- Her görevde tam dosya yolu belirtilmiştir

## Path Conventions

- **Proje kökü**: `src/InnovatEPAM.Portal/`
- **Controllers**: `src/InnovatEPAM.Portal/Controllers/`
- **Services**: `src/InnovatEPAM.Portal/Services/`
- **Models**: `src/InnovatEPAM.Portal/Models/`
- **Views**: `src/InnovatEPAM.Portal/Views/`

---

## Phase 1: Setup

**Amaç**: Yeni dosya iskeletlerini oluştur; derleme bozulmaz.

- [x] T001 `src/InnovatEPAM.Portal/Models/SystemSetting.cs` boş dosyasını oluştur
- [x] T002 [P] `src/InnovatEPAM.Portal/Models/SystemSettingKeys.cs` boş dosyasını oluştur
- [x] T003 [P] `src/InnovatEPAM.Portal/Repositories/Interfaces/ISystemSettingRepository.cs` boş dosyasını oluştur
- [x] T004 [P] `src/InnovatEPAM.Portal/Repositories/SystemSettingRepository.cs` boş dosyasını oluştur
- [x] T005 [P] `src/InnovatEPAM.Portal/Services/Interfaces/IBlindReviewService.cs` boş dosyasını oluştur
- [x] T006 [P] `src/InnovatEPAM.Portal/Services/BlindReviewService.cs` boş dosyasını oluştur
- [x] T007 [P] `src/InnovatEPAM.Portal/ViewModels/SettingsViewModels.cs` boş dosyasını oluştur
- [x] T008 [P] `src/InnovatEPAM.Portal/Controllers/SettingsController.cs` boş dosyasını oluştur
- [x] T009 [P] `src/InnovatEPAM.Portal/Views/Settings/` klasörünü oluştur ve `BlindReview.cshtml` boş dosyasını ekle

**Checkpoint**: `dotnet build` hatasız — tüm dosyalar geçerli namespace bloğu ile derleniyor.

---

## Phase 2: Foundation (Blocking Prerequisites)

**Amaç**: Tüm user story'lerin bağımlı olduğu entity, interface, repository ve DI altyapısı.

**⚠️ KRİTİK**: Bu phase tamamlanmadan hiçbir user story çalışmasına başlanamaz.

- [x] T010 `src/InnovatEPAM.Portal/Models/SystemSetting.cs` — `SystemSetting` entity'sini yaz:
  - `Key` (string, PK, max 100 chars)
  - `Value` (string, max 500 chars)
  - `LastModifiedDate` (DateTime, UTC)
  - `LastModifiedByAdminId` (Guid?, FK to ApplicationUser)
  - `LastModifiedByAdmin` (navigation property, optional)
  - (data-model.md §1)

- [x] T011 [P] `src/InnovatEPAM.Portal/Models/SystemSettingKeys.cs` — `SystemSettingKeys` static class'ını yaz:
  - `public const string BlindReviewEnabled = "BlindReviewEnabled";`
  - (data-model.md §6)

- [x] T012 `src/InnovatEPAM.Portal/Data/ApplicationDbContext.cs` — `SystemSettings` DbSet ve Fluent config ekle:
  - `public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();`
  - `builder.Entity<SystemSetting>` konfigürasyonu: ToTable, HasKey, Property max lengths, HasOne/WithMany FK
  - `HasData` seed: `new SystemSetting { Key = "BlindReviewEnabled", Value = "false" }`
  - (data-model.md §1 EF Core Configuration + Seed Data)

- [x] T013 [P] `src/InnovatEPAM.Portal/Repositories/Interfaces/ISystemSettingRepository.cs` — interface'i yaz:
  - `Task<SystemSetting?> GetByKeyAsync(string key)`
  - `Task UpsertAsync(SystemSetting setting)`
  - (data-model.md §5)

- [x] T014 `src/InnovatEPAM.Portal/Repositories/SystemSettingRepository.cs` — EF Core implementasyonunu yaz:
  - `GetByKeyAsync`: `FirstOrDefaultAsync(s => s.Key == key)`
  - `UpsertAsync`: var ise `Update`, yoksa `Add` + `SaveChangesAsync`
  - Constructor: `ApplicationDbContext _db`
  - (data-model.md §5 + ADR-004)

- [x] T015 `src/InnovatEPAM.Portal/Services/Interfaces/IBlindReviewService.cs` — interface'i yaz:
  - `Task<bool> IsEnabledAsync()`
  - `Task SetEnabledAsync(bool enabled, Guid adminId)`
  - `void ApplyMasking(IdeaDetailDTO dto, bool isBlindReviewEnabled)`
  - `void ApplyMasking(IEnumerable<IdeaListItemDTO> dtos, bool isBlindReviewEnabled)`
  - `bool ShouldRevealIdentity(string ideaStatus)`
  - (data-model.md §3)

- [x] T016 `src/InnovatEPAM.Portal/Services/BlindReviewService.cs` — tam implementasyonu yaz:
  - `IsEnabledAsync`: `GetByKeyAsync("BlindReviewEnabled")` → parse value string as bool
  - `SetEnabledAsync`: build `SystemSetting` record, call `UpsertAsync`, Serilog log
  - `ShouldRevealIdentity`: returns `true` when `ideaStatus is "Accepted" or "Rejected"`
  - `ApplyMasking(IdeaDetailDTO)`: mask `dto.SubmitterName = "Anonymous Submitter"` when `isBlindReviewEnabled && !ShouldRevealIdentity(dto.Status)`
  - `ApplyMasking(IEnumerable<IdeaListItemDTO>)`: foreach apply same rule on `dto.SubmitterName`
  - Constructor: `ISystemSettingRepository`, `ILogger<BlindReviewService>`
  - (data-model.md §3, contracts/settings.md §IBlindReviewService Contract)

- [x] T017 `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` — `AdminIdeaListViewModel` ve `AdminIdeaDetailViewModel`'e `IsBlindReviewActive` property ekle:
  - `public bool IsBlindReviewActive { get; set; }`
  - (data-model.md §4)

- [x] T018 `src/InnovatEPAM.Portal/Program.cs` — DI kayıtlarını ekle:
  - `builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();`
  - `builder.Services.AddScoped<IBlindReviewService, BlindReviewService>();`

- [x] T019 EF Core migration oluştur: `dotnet ef migrations add AddSystemSettings --no-build`

- [x] T020 `dotnet build` — 0 error, 0 warning doğrulaması yap

---

## Phase 3: US1 — Admin Reviews Without Seeing Submitter Identity (P1)

**Story Goal**: Admin herhangi bir fikir listesi veya detay sayfasını açtığında, blind review modu aktifse submitter kimliği maskelenmiş olarak görünür.

**Independent Test**: Blind review modu etkinleştir → admin olarak `/Admin` sayfasını aç → "Anonymous Submitter" görün. quickstart.md Senaryo 2 + 3.

- [x] T021 [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — constructor'a `IBlindReviewService _blindReviewService` inject et (alan + constructor parametresi)

- [x] T022 [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Index` action'ını güncelle:
  - `var isBlindReview = await _blindReviewService.IsEnabledAsync();`
  - `_blindReviewService.ApplyMasking(ideas, isBlindReview);`
  - `IsBlindReviewActive = isBlindReview` → `AdminIdeaListViewModel`'e ata
  - (contracts/settings.md §AdminController Modifications — Index)

- [x] T023 [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Detail` action'ını güncelle:
  - `var isBlindReview = await _blindReviewService.IsEnabledAsync();`
  - `_blindReviewService.ApplyMasking(idea, isBlindReview);`
  - `IsBlindReviewActive = isBlindReview` → `AdminIdeaDetailViewModel`'e ata
  - (contracts/settings.md §AdminController Modifications — Detail)

- [x] T024 [P] [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `ByStage` action'ını güncelle:
  - `var isBlindReview = await _blindReviewService.IsEnabledAsync();`
  - `_blindReviewService.ApplyMasking(filtered, isBlindReview);`
  - `ViewBag.IsBlindReviewActive = isBlindReview`

- [x] T025 [US1] `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml` — blind review info banner ekle:
  - `@if (Model.IsBlindReviewActive)` → Bootstrap `alert alert-info` banner: `"Blind review is active — submitter identities are hidden."`
  - Banner, `<h2>` başlığının hemen altına yerleştir

- [x] T026 [P] [US1] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — blind review info banner ekle:
  - `@if (Model.IsBlindReviewActive)` → Bootstrap `alert alert-info` banner
  - Başlık satırının altına, kart bloklarının üstüne yerleştir

- [x] T027 [P] [US1] `src/InnovatEPAM.Portal/Views/Admin/ByStage.cshtml` — blind review info banner ekle:
  - `@if ((bool)(ViewBag.IsBlindReviewActive ?? false))` → Bootstrap `alert alert-info` banner

---

## Phase 4: US2 — Admin Toggles Blind Review Mode On/Off (P2)

**Story Goal**: Admin, ayarlar sayfasından blind review modunu açıp kapatabilir; değişiklik hemen ve kalıcı olarak geçerli olur.

**Independent Test**: Ayarlar sayfasına git → toggle → kaydet → `TempData["Success"]` mesajını gör → admin fikir listesine dön → etki doğrula. quickstart.md Senaryo 1 + 7.

- [x] T028 [US2] `src/InnovatEPAM.Portal/ViewModels/SettingsViewModels.cs` — `BlindReviewSettingsViewModel`'i yaz:
  - `public bool IsEnabled { get; set; }`
  - `public DateTime? LastModifiedDate { get; set; }`
  - `public string? LastModifiedByAdminName { get; set; }`
  - (data-model.md §2)

- [x] T029 [US2] `src/InnovatEPAM.Portal/Controllers/SettingsController.cs` — tam implementasyonu yaz:
  - `[Authorize(Roles = "Admin")]` class attribute
  - Constructor: `IBlindReviewService`, `ISystemSettingRepository`, `UserManager<ApplicationUser>`
  - `GET BlindReview`: `IsEnabledAsync()` + `GetByKeyAsync("BlindReviewEnabled")` → `BlindReviewSettingsViewModel` → `View(vm)`
  - `POST BlindReview`: `[HttpPost, ValidateAntiForgeryToken]`, `SetEnabledAsync(vm.IsEnabled, adminId)`, `TempData["Success"]` mesajı, `RedirectToAction`
  - (contracts/settings.md §SettingsController Actions)

- [x] T030 [US2] `src/InnovatEPAM.Portal/Views/Settings/BlindReview.cshtml` — ayarlar sayfasını yaz:
  - Breadcrumb: `Admin > Settings > Blind Review`
  - Current state card: enabled/disabled badge + last changed by + date
  - Toggle form: checkbox veya Bootstrap 5 form-switch + Save button
  - `TempData["Success"]` / `TempData["Error"]` alert'leri
  - Anti-forgery token içerir

- [x] T031 [US2] `src/InnovatEPAM.Portal/Views/Shared/_Layout.cshtml` — Admin nav'ına Settings linki ekle:
  - `@if (User.IsInRole("Admin"))` koşuluyla `<a asp-controller="Settings" asp-action="BlindReview">Settings</a>` linki
  - Mevcut Admin nav item grubuna ekle

---

## Phase 5: US3 — Identity Revealed After Final Decision (P3)

**Story Goal**: Blind review aktif olsa bile, Accepted/Rejected durumdaki fikirlerde submitter kimliği görünür.

**Independent Test**: Blind review aç → Accepted/Rejected durumlu fikri aç → gerçek isim görün. quickstart.md Senaryo 4 + 10.

- [x] T032 [US3] `src/InnovatEPAM.Portal/Services/BlindReviewService.cs` — `ShouldRevealIdentity` metodu içinde tüm `IdeaStatus` değerlerinin doğru işlendiğini doğrula:
  - `"Accepted"` → `true` (maskele değil)
  - `"Rejected"` → `true` (maskele değil)
  - `"Submitted"`, `"UnderReview"`, `"Draft"` → `false` (maskele)
  - Tüm case'ler için explicit bir switch/pattern kullan; catch-all `false` döner

- [x] T033 [P] [US3] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — concluded idea için "kimlik açıklandı" göstergesi ekle:
  - `@if (Model.IsBlindReviewActive && (Model.Idea.Status == "Accepted" || Model.Idea.Status == "Rejected"))` → küçük `badge bg-success` veya `text-muted` not: `"Identity visible — evaluation concluded"`
  - Blind review banner'ının yanına yerleştir

---

## Phase 6: US4 — Submitter Experience Unaffected (P4)

**Story Goal**: Submitter kendi fikirlerini her zaman tam kimlikli görmeli; blind review modu submitter akışını etkilememelidir.

**Independent Test**: Blind review aç → submitter olarak giriş → kendi fikirlerini aç → gerçek isim ve tüm detaylar görünür. quickstart.md Senaryo 6.

- [x] T034 [US4] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` — `IBlindReviewService` inject edilmediğini ve hiçbir action'da `ApplyMasking` çağrısı olmadığını doğrula (mimari garanti review); gerekirse kod yorumu ekle

- [x] T035 [P] [US4] `src/InnovatEPAM.Portal/Views/Shared/_Layout.cshtml` — Settings nav linkinin yalnızca `Admin` rolünde görüneceğini doğrula: `@if (User.IsInRole("Admin"))` koşulu T031'de doğru uygulandı mı kontrol et

---

## Phase 7: Polish & Cross-Cutting Concerns

**Amaç**: XML dokümantasyonu, görsel tutarlılık, son build doğrulaması.

- [x] T036 Tüm yeni `public` sınıf, interface ve metodlara XML `///` doc comments ekle:
  - `SystemSetting.cs`, `SystemSettingKeys.cs`
  - `ISystemSettingRepository.cs`, `SystemSettingRepository.cs`
  - `IBlindReviewService.cs`, `BlindReviewService.cs`
  - `SettingsController.cs`, `BlindReviewSettingsViewModel`

- [x] T037 [P] `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml`, `Detail.cshtml`, `ByStage.cshtml` — blind review banner'larının Bootstrap stil tutarlılığını gözden geçir: `alert-info` + `bi-eye-slash` Bootstrap Icons ikonuyla görsel iyileştirme yap

- [x] T038 `dotnet build` son doğrulama — 0 error, 0 warning

- [x] T039 `specs/005-blind-review-mode/tasks.md` — tamamlanan tüm görevleri `[x]` ile işaretle

---

## Dependencies

```
Phase 1 (T001–T009)
    ↓
Phase 2 (T010–T020)   ← tüm US'lerin kilidi buradadır
    ↓         ↓         ↓         ↓
Phase 3     Phase 4   Phase 5   Phase 6
(US1)       (US2)     (US3)     (US4)
T021–T027   T028–T031 T032–T033 T034–T035
    ↓         ↓         ↓         ↓
                Phase 7 (T036–T039)
```

**US3 notu**: T032 (`ShouldRevealIdentity` doğrulaması) aslında T016'nın bir parçasıdır. Eğer T016 tam yazılırsa T032 sadece review + edge case güçlendirmesidir.

**US1 → US4 bağımsızlığı**: US2 (toggle), US3 (reveal) ve US4 (submitter) birbirinden bağımsız çalışır. Yalnızca US1 uygulanmış olsa bile Scenario 2–3 test edilebilir.

---

## Implementation Strategy

| MVP Scope | Açıklama |
|---|---|
| Phase 1–3 (US1) | Core blind review maskeleme; toggle olmadan `BlindReviewEnabled = "true"` veritabanına seed edilmiş olarak bile çalışır |
| + Phase 4 (US2) | Settings sayfası ile toggle yönetimi eklenir |
| + Phase 5 (US3) | Post-decision reveal; US1 içindeki `ShouldRevealIdentity` logic'i zaten çalışır |
| + Phase 6–7 | Submitter doğrulaması ve polish |

**Parallel Opportunities**:
- T001–T009 (Setup): Tümü aynı anda çalışabilir
- T010, T011, T013: Birbirinden bağımsız — paralel yazılabilir
- T025, T026, T027 (Banner views): Birbirinden bağımsız
- T028, T029, T030, T031 (US2 bileşenleri): T028 tamamlandıktan sonra T029–T031 paralel olabilir

---

## Summary

| Phase | Görev Sayısı | Kapsam |
|---|---|---|
| Phase 1: Setup | 9 (T001–T009) | Boş dosya iskeletleri |
| Phase 2: Foundation | 11 (T010–T020) | Entity, repo, service, DI, migration |
| Phase 3: US1 | 7 (T021–T027) | Admin maskeleme + banner |
| Phase 4: US2 | 4 (T028–T031) | Settings toggle sayfası |
| Phase 5: US3 | 2 (T032–T033) | Post-decision reveal |
| Phase 6: US4 | 2 (T034–T035) | Submitter doğrulaması |
| Phase 7: Polish | 4 (T036–T039) | XML doc, görsel, build |
| **Toplam** | **39** | |
