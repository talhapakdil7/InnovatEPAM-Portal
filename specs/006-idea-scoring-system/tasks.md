# Tasks: Idea Scoring System

**Input**: Design documents from `specs/006-idea-scoring-system/`

**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/scoring.md ✓, quickstart.md ✓

**Tests**: Manual testing per `quickstart.md` (12 scenarios + 7 regression). No unit test tasks — MVP manual-testing-first approach (per constitution Gate XI conditional pass).

**Organization**: Phases 1–2 are foundational. Phases 3–6 map to US1–US4 (priority order). Phase 7 is polish.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Paralel çalışabilir (farklı dosyalar, bağımlılık yok)
- **[Story]**: Hangi user story'ye ait (US1–US4)
- Her görevde tam dosya yolu belirtilmiştir

## Path Conventions

- **Proje kökü**: `src/InnovatEPAM.Portal/`
- **Controllers**: `src/InnovatEPAM.Portal/Controllers/`
- **Services**: `src/InnovatEPAM.Portal/Services/`
- **Models**: `src/InnovatEPAM.Portal/Models/`
- **DTOs**: `src/InnovatEPAM.Portal/DTOs/`
- **Views**: `src/InnovatEPAM.Portal/Views/`

---

## Phase 1: Setup

**Amaç**: Yeni dosya iskeletlerini oluştur; derleme bozulmaz.

- [x] T001 `src/InnovatEPAM.Portal/Models/IdeaScore.cs` boş dosyasını oluştur
- [x] T002 [P] `src/InnovatEPAM.Portal/DTOs/ScoreSummaryDTO.cs` boş dosyasını oluştur
- [x] T003 [P] `src/InnovatEPAM.Portal/ViewModels/ScoreViewModels.cs` boş dosyasını oluştur
- [x] T004 [P] `src/InnovatEPAM.Portal/Validators/SubmitScoreValidator.cs` boş dosyasını oluştur
- [x] T005 [P] `src/InnovatEPAM.Portal/Repositories/Interfaces/IIdeaScoreRepository.cs` boş dosyasını oluştur
- [x] T006 [P] `src/InnovatEPAM.Portal/Repositories/IdeaScoreRepository.cs` boş dosyasını oluştur
- [x] T007 [P] `src/InnovatEPAM.Portal/Services/Interfaces/IScoreService.cs` boş dosyasını oluştur
- [x] T008 [P] `src/InnovatEPAM.Portal/Services/ScoreService.cs` boş dosyasını oluştur
- [x] T009 [P] `src/InnovatEPAM.Portal/Controllers/ScoreController.cs` boş dosyasını oluştur

**Checkpoint**: `dotnet build` hatasız — tüm dosyalar geçerli namespace bloğu ile derleniyor.

---

## Phase 2: Foundation (Blocking Prerequisites)

**Amaç**: Tüm user story'lerin bağımlı olduğu entity, repository, service interface, DI altyapısı ve migration.

**⚠️ KRİTİK**: Bu phase tamamlanmadan hiçbir user story çalışmasına başlanamaz.

- [x] T010 `src/InnovatEPAM.Portal/Models/IdeaScore.cs` — `IdeaScore` entity'sini yaz:
  - `IdeaId` (Guid, composite PK), `Idea` navigation property
  - `AdminId` (Guid, composite PK), `Admin` navigation property (ApplicationUser)
  - `Innovation` (int?, nullable), `TechnicalFeasibility` (int?, nullable), `BusinessImpact` (int?, nullable), `ImplementationValue` (int?, nullable)
  - `SubmittedDate` (DateTime UTC), `LastUpdatedDate` (DateTime UTC)
  - XML doc comments on all public members
  - (data-model.md §1)

- [x] T011 `src/InnovatEPAM.Portal/Models/Idea.cs` — `Scores` navigation collection ekle:
  - `public ICollection<IdeaScore> Scores { get; set; } = new List<IdeaScore>();`
  - (data-model.md §1)

- [x] T012 `src/InnovatEPAM.Portal/Data/ApplicationDbContext.cs` — `IdeaScores` DbSet ve Fluent config ekle:
  - `public DbSet<IdeaScore> IdeaScores => Set<IdeaScore>();`
  - `builder.Entity<IdeaScore>` konfigürasyonu: composite PK `new { s.IdeaId, s.AdminId }`, ToTable("IdeaScores"), FK to Ideas (Cascade), FK to Users (Restrict), indexes on IdeaId and AdminId
  - (data-model.md §1 EF Core Configuration)

- [x] T013 [P] `src/InnovatEPAM.Portal/DTOs/ScoreSummaryDTO.cs` — iki DTO'yu yaz:
  - `ScoreSummaryDTO`: `ScorerCount`, `AvgInnovation`, `AvgTechnicalFeasibility`, `AvgBusinessImpact`, `AvgImplementationValue`, `OverallAverage` (all decimal?), `AdminScores` (List<AdminScoreRowDTO>)
  - `AdminScoreRowDTO`: `AdminName` (string), four int? dimension fields, `RowAverage` (decimal?), `SubmittedDate`
  - XML doc comments
  - (data-model.md §2)

- [x] T014 [P] `src/InnovatEPAM.Portal/DTOs/IdeaListItemDTO.cs` — iki yeni property ekle:
  - `public decimal? AggregateScore { get; set; }`
  - `public int ScorerCount { get; set; }`
  - (data-model.md §2)

- [x] T015 [P] `src/InnovatEPAM.Portal/DTOs/IdeaDetailDTO.cs` — iki yeni property ekle:
  - `public ScoreSummaryDTO? ScoreSummary { get; set; }`
  - `public AdminScoreRowDTO? MyScore { get; set; }` (admin view sadece)
  - (data-model.md §2)

- [x] T016 `src/InnovatEPAM.Portal/Repositories/Interfaces/IIdeaScoreRepository.cs` — interface'i yaz:
  - `Task<IdeaScore?> GetAsync(Guid ideaId, Guid adminId)`
  - `Task<List<IdeaScore>> GetAllForIdeaAsync(Guid ideaId)`
  - `Task UpsertAsync(IdeaScore score)`
  - `Task DeleteAsync(Guid ideaId, Guid adminId)`
  - XML doc comments
  - (data-model.md §3)

- [x] T017 `src/InnovatEPAM.Portal/Repositories/IdeaScoreRepository.cs` — EF Core implementasyonunu yaz:
  - `GetAsync`: `FindAsync(ideaId, adminId)` veya `FirstOrDefaultAsync` + Admin include
  - `GetAllForIdeaAsync`: `Where(s => s.IdeaId == ideaId).Include(s => s.Admin).ToListAsync()`
  - `UpsertAsync`: `FindAsync` → varsa güncelle (4 dimension + LastUpdatedDate), yoksa `AddAsync`; `SaveChangesAsync`
  - `DeleteAsync`: `FindAsync` → varsa `Remove` + `SaveChangesAsync`; yoksa no-op
  - XML doc comments
  - (data-model.md §3)

- [x] T018 `src/InnovatEPAM.Portal/Services/Interfaces/IScoreService.cs` — interface'i yaz:
  - `Task SubmitScoreAsync(Guid ideaId, Guid adminId, SubmitScoreViewModel vm)`
  - `Task RetractScoreAsync(Guid ideaId, Guid adminId)`
  - `Task<ScoreSummaryDTO> GetScoreSummaryAsync(Guid ideaId, bool isBlindReviewActive)`
  - `Task<IdeaScore?> GetMyScoreAsync(Guid ideaId, Guid adminId)`
  - `Task<Dictionary<Guid, (decimal? OverallAverage, int ScorerCount)>> GetAggregatesForIdeasAsync(IEnumerable<Guid> ideaIds)`
  - XML doc comments
  - (data-model.md §4)

- [x] T019 `src/InnovatEPAM.Portal/Program.cs` — DI kayıtlarını ekle:
  - `builder.Services.AddScoped<IIdeaScoreRepository, IdeaScoreRepository>();`
  - `builder.Services.AddScoped<IScoreService, ScoreService>();`

- [x] T020 EF Core migration oluştur: `dotnet ef migrations add AddIdeaScores --no-build`

- [x] T021 `dotnet build` — 0 error, 0 warning doğrulaması yap

---

## Phase 3: US1 — Admin Scores an Idea Across Dimensions (P1)

**Story Goal**: Admin herhangi bir aktif idea'nın detay sayfasından tüm boyutlara puan girerek kaydedebilir; var olan puanı güncelleyebilir.

**Independent Test**: Admin → `/Admin/Detail/{id}` → scoring form'u doldur → Save → sayfa yenileniyor, puan görünüyor (quickstart.md Senaryo 1, 2, 3, 4).

- [x] T022 [US1] `src/InnovatEPAM.Portal/ViewModels/ScoreViewModels.cs` — `SubmitScoreViewModel`'i yaz:
  - `public Guid IdeaId { get; set; }`
  - `[Range(1,5)] public int? Innovation { get; set; }` (ve diğer 3 dimension)
  - XML doc comments
  - (data-model.md §5)

- [x] T023 [US1] `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` — `AdminIdeaDetailViewModel`'e üç property ekle:
  - `public ScoreSummaryDTO? ScoreSummary { get; set; }`
  - `public SubmitScoreViewModel ScoreForm { get; set; } = new();`
  - `public bool IsScoringAllowed { get; set; }`
  - (data-model.md §5)

- [x] T024 [US1] `src/InnovatEPAM.Portal/Validators/SubmitScoreValidator.cs` — `SubmitScoreValidator`'ı yaz:
  - En az bir dimension non-null kuralı (cross-property Must rule, mesaj: "At least one evaluation dimension must be scored.")
  - Her dimension için: `When(x => x.D.HasValue, () => RuleFor(...).InclusiveBetween(1,5))`
  - (data-model.md §6)

- [x] T025 [US1] `src/InnovatEPAM.Portal/Services/ScoreService.cs` — `SubmitScoreAsync` ve `GetMyScoreAsync` metodlarını yaz:
  - Constructor: `IIdeaScoreRepository`, `IIdeaRepository` (status kontrolü için), `ILogger<ScoreService>`
  - `SubmitScoreAsync`: idea'yı yükle → status Draft/Accepted/Rejected ise `InvalidOperationException` fırlat → `IdeaScore` oluştur veya güncelle → `UpsertAsync` → Serilog log
  - `GetMyScoreAsync`: `_repo.GetAsync(ideaId, adminId)`
  - (data-model.md §4 + contracts/scoring.md §IScoreService)

- [x] T026 [US1] `src/InnovatEPAM.Portal/Controllers/ScoreController.cs` — `SubmitScore` POST action'ını yaz:
  - `[Authorize(Roles = "Admin")]` class attribute
  - Constructor: `IScoreService`, `UserManager<ApplicationUser>`
  - `[HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Submit(SubmitScoreViewModel vm)`:
    - ModelState geçersizse → `TempData["Error"]` + `RedirectToAction("Detail", "Admin", new { id = vm.IdeaId })`
    - `_scoreService.SubmitScoreAsync(vm.IdeaId, adminId, vm)` → try/catch `InvalidOperationException` → `TempData["Error"]`
    - Başarı: `TempData["Success"] = "Your score has been saved."` → `RedirectToAction("Detail", "Admin")`
  - (contracts/scoring.md §ScoreController)

- [x] T027 [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Detail` action'ına score yükleme ekle:
  - `IScoreService _scoreService` inject et (constructor'a ekle)
  - `Detail` action içinde: `var myScore = await _scoreService.GetMyScoreAsync(id, adminId);`
  - `IsScoringAllowed = idea.Status is "Submitted" or "UnderReview"`
  - `ScoreForm` pre-populate: `new SubmitScoreViewModel { IdeaId = id, Innovation = myScore?.Innovation, ... }`
  - `AdminIdeaDetailViewModel`'e ata
  - (contracts/scoring.md §AdminController Modifications — Detail)

- [x] T028 [US1] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — scoring form section ekle:
  - Idea metadata kartının altına, "Review Pipeline" bölümünün üstüne yerleştir
  - `@if (Model.IsScoringAllowed)` → POST `/Score/Submit`, anti-forgery, `SubmitScoreViewModel`
  - Dört dimension için Bootstrap `<select>` (1–2–3–4–5 + "Not scored" / null), `asp-for` bağlantısı
  - Mevcut puan varsa form pre-populated (ScoreForm değerleriyle)
  - Validation summary (`asp-validation-summary="All"`)
  - `[Save Score]` button (primary)
  - `@if (!Model.IsScoringAllowed && (Model.Idea.Status == "Accepted" || Model.Idea.Status == "Rejected"))` → `"Scoring closed — idea has been decided"` badge
  - (contracts/scoring.md §View Rendering Rules)

**Checkpoint**: Senaryo 1, 2, 3, 4 manuel test — puan kaydediliyor, güncelleniyor, validasyon çalışıyor.

---

## Phase 4: US2 — Admins View Aggregated Score (P2)

**Story Goal**: Herhangi bir admin, idea listesinde ve detay sayfasında toplu (aggregate) puanı ve boyut bazlı ortalamaları görebilir.

**Independent Test**: İki admin bir idea'yı puanladıktan sonra, admin detay ve liste sayfalarında doğru aggregate değerleri görünüyor (quickstart.md Senaryo 5, 6).

- [x] T029 [US2] `src/InnovatEPAM.Portal/Services/ScoreService.cs` — `GetScoreSummaryAsync` metodunu yaz:
  - `GetAllForIdeaAsync(ideaId)` → tüm `IdeaScore` listesini yükle (Admin include)
  - Her dimension için: non-null değerlerin ortalamasını hesapla → `decimal? AvgD`
  - `OverallAverage` = non-null `AvgD` değerlerin ortalaması, 2 ondalık basamağa round
  - `ScorerCount` = toplam satır sayısı
  - `AdminScores` listesi: her satır için `AdminScoreRowDTO` oluştur; `isBlindReviewActive` true ise `AdminName = "Anonymous Reviewer"`; `RowAverage` = satırdaki non-null değerlerin ortalaması
  - (data-model.md §7 + contracts/scoring.md §IScoreService)

- [x] T030 [US2] `src/InnovatEPAM.Portal/Services/ScoreService.cs` — `GetAggregatesForIdeasAsync` metodunu yaz:
  - `_db.IdeaScores.Where(s => ideaIds.Contains(s.IdeaId))` ile bulk çek (veya `IIdeaScoreRepository` üzerinden)
  - Grup: `GroupBy(s => s.IdeaId)` → her grup için `OverallAverage` ve `ScorerCount` hesapla
  - `Dictionary<Guid, (decimal?, int)>` döndür
  - **Not**: `IIdeaScoreRepository`'e `GetBulkForIdeasAsync(IEnumerable<Guid>)` metodu eklemek gerekebilir

- [x] T031 [US2] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Detail` action'ına score summary yükleme ekle:
  - `var scoreSummary = await _scoreService.GetScoreSummaryAsync(id, isBlindReview);`
  - `vm.ScoreSummary = scoreSummary;`
  - (contracts/scoring.md §AdminController Modifications — Detail)

- [x] T032 [US2] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Index` action'ına aggregate score yükleme ekle:
  - `var ideaIds = ideas.Select(i => i.Id);`
  - `var aggregates = await _scoreService.GetAggregatesForIdeasAsync(ideaIds);`
  - `foreach` döngüsünde `idea.AggregateScore` ve `idea.ScorerCount` ata
  - (contracts/scoring.md §AdminController Modifications — Index)

- [x] T033 [P] [US2] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — score summary section ekle:
  - `@if (Model.ScoreSummary != null && Model.ScoreSummary.ScorerCount > 0)` → summary kartı
  - Overall average: `X.XX / 5` + `(N reviewer(s))` — Bootstrap badge ile vurgula
  - Boyut ortalamaları tablosu: Innovation, Technical Feasibility, Business Impact, Implementation Value → avg veya "—"
  - Admin breakdown tablosu: her `AdminScoreRowDTO` için bir satır (AdminName, dört dimension, row avg, tarih)
  - `@else` → `"No scores yet"` placeholder (bilgi alert)
  - (contracts/scoring.md §View Rendering Rules)

- [x] T034 [P] [US2] `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml` — "Score" sütunu ekle:
  - Mevcut ideas tablosuna yeni sütun başlığı: `Score`
  - Her satır: `@(item.AggregateScore.HasValue ? $"{item.AggregateScore:F2} ★ ({item.ScorerCount})" : "—")`
  - (contracts/scoring.md §View Rendering Rules)

**Checkpoint**: Senaryo 5, 6 manuel test — aggregate doğru hesaplanıyor, liste ve detay sayfalarında görünüyor.

---

## Phase 5: US3 — Admin Removes Their Score (P3)

**Story Goal**: Admin kendi puanını bir idea'dan geri çekebilir; aggregate anında yeniden hesaplanır.

**Independent Test**: Admin → "Remove My Score" → confirm → aggregate güncelleniyor (quickstart.md Senaryo 7, 8).

- [x] T035 [US3] `src/InnovatEPAM.Portal/Services/ScoreService.cs` — `RetractScoreAsync` metodunu yaz:
  - `_repo.DeleteAsync(ideaId, adminId)` → no-op when not exists (repo zaten no-op yapar)
  - Serilog log: retraction
  - (contracts/scoring.md §ScoreController — Retract)

- [x] T036 [US3] `src/InnovatEPAM.Portal/Controllers/ScoreController.cs` — `Retract` POST action'ını ekle:
  - `[HttpPost, ValidateAntiForgeryToken, Route("Score/Retract/{ideaId}")]`
  - `await _scoreService.RetractScoreAsync(ideaId, adminId)`
  - `TempData["Success"] = "Your score has been removed."`
  - `RedirectToAction("Detail", "Admin", new { id = ideaId })`
  - (contracts/scoring.md §ScoreController — Retract)

- [x] T037 [US3] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — "Remove My Score" butonu ekle:
  - `@if (Model.ScoreForm.Innovation.HasValue || Model.ScoreForm.TechnicalFeasibility.HasValue || ...)` → "Bu adminin mevcut puanı var" koşulu
  - POST form: `/Score/Retract/@Model.Idea.Id`, anti-forgery
  - `[Remove My Score]` button (outline-danger), yanında "Bu işlem geri alınamaz" uyarısı
  - Scoring form'un yanına veya altına yerleştir

**Checkpoint**: Senaryo 7, 8 manuel test — retraction çalışıyor, aggregate güncelleniyor.

---

## Phase 6: US4 — Submitter Views Aggregated Score (P4)

**Story Goal**: Submitter kendi idea'sının genel ortalama puanını görebilir; admin breakdown ve scorer isimleri görünmez.

**Independent Test**: Submitter login → kendi idea'sını aç → "Evaluation Score: X.XX / 5" görünüyor (quickstart.md Senaryo 9, 12).

- [x] T038 [US4] `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` — `IdeaDetailViewModel`'e iki property ekle:
  - `public decimal? AggregateScore { get; set; }`
  - `public int ScorerCount { get; set; }`
  - (contracts/scoring.md §IdeasController Modifications)

- [x] T039 [US4] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` — `Detail` action'ına aggregate score yükleme ekle:
  - `IScoreService _scoreService` inject et (constructor'a ekle)
  - `var scoreSummary = await _scoreService.GetScoreSummaryAsync(idea.Id, isBlindReviewActive: false);`
  - `vm.AggregateScore = scoreSummary.OverallAverage;`
  - `vm.ScorerCount = scoreSummary.ScorerCount;`
  - **Not**: Mevcut `Detail` action'ın doğru `IdeaDetailViewModel` döndürdüğünü doğrula
  - (contracts/scoring.md §IdeasController Modifications)

- [x] T040 [US4] `src/InnovatEPAM.Portal/Views/Ideas/Detail.cshtml` — submitter score section ekle:
  - `@if (Model.AggregateScore.HasValue)` → "Evaluation Score" info kartı: `"@Model.AggregateScore:F2 / 5 (rated by @Model.ScorerCount reviewer(s))"`
  - `@else` → section'ı tamamen gizle (FR-009 — Submitter "No scores" görmez)
  - Hiçbir admin breakdown, scorer ismi veya dimension detayı gösterilmez
  - (contracts/scoring.md §View Rendering Rules — Submitter)

**Checkpoint**: Senaryo 9, 12 manuel test — submitter doğru görünümü alıyor; authorization check çalışıyor.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Amaç**: XML dokümantasyonu, görsel tutarlılık, `IIdeaScoreRepository` bulk method, son build ve regresyon.

- [x] T041 `src/InnovatEPAM.Portal/Repositories/Interfaces/IIdeaScoreRepository.cs` — bulk fetch method ekle (T030'dan ihtiyaç duyulursa):
  - `Task<List<IdeaScore>> GetBulkForIdeasAsync(IEnumerable<Guid> ideaIds);`
  - `IdeaScoreRepository.cs`'de: `Where(s => ideaIds.Contains(s.IdeaId)).ToListAsync()`

- [x] T042 [P] Tüm yeni `public` sınıf, interface ve metodlara XML `///` doc comments eklendiğini doğrula:
  - `IdeaScore.cs`, `ScoreSummaryDTO.cs`, `AdminScoreRowDTO.cs`
  - `IIdeaScoreRepository.cs`, `IdeaScoreRepository.cs`
  - `IScoreService.cs`, `ScoreService.cs`
  - `ScoreController.cs`, `SubmitScoreViewModel.cs`, `SubmitScoreValidator.cs`

- [x] T043 [P] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — score section'ın görsel tutarlılığını kontrol et:
  - Score summary kartı: Bootstrap card + `bi-star-fill` ikon
  - Aggregate badge: Bootstrap `badge bg-warning text-dark` veya `bg-primary`
  - Admin breakdown tablosu: `table table-sm table-hover`
  - "No scores yet": `alert alert-light` ile `bi-star` ikon

- [x] T044 [P] `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml` — Score sütununun tablo responsive'liğini kontrol et; gerekirse `d-none d-md-table-cell` ekle

- [x] T045 Regresyon testleri (quickstart.md R1–R7):
  - R1: Multi-stage workflow bozulmamış
  - R2: Blind review + score — scorer adları maskeleniyor
  - R3: Blind review + submitter — sadece aggregate görünüyor
  - R4: Liste sayfası karma (puanlı/puansız) ideas
  - R5: Draft → Submit geçişi sonrası scoring aktif
  - R6: Accepted → scoring formu kapanıyor
  - R7: Concurrent scoring — duplicate PK yok

- [x] T046 `dotnet build` son doğrulama — 0 error, 0 warning

- [x] T047 `specs/006-idea-scoring-system/tasks.md` — tamamlanan tüm görevleri `[x]` ile işaretle

---

## Dependencies

```
Phase 1 (T001–T009)  ← boş dosyalar
    ↓
Phase 2 (T010–T021)  ← entity, repo, service interface, DI, migration
    ↓         ↓         ↓         ↓
Phase 3     Phase 4   Phase 5   Phase 6
(US1)       (US2)     (US3)     (US4)
T022–T028   T029–T034 T035–T037 T038–T040
    ↓         ↓         ↓         ↓
                Phase 7 (T041–T047)
```

**US2 → US1 bağımlılığı**: T029 (`GetScoreSummaryAsync`), T030 (`GetAggregatesForIdeasAsync`) US1'deki `ScoreService` iskeletine bağlıdır; aynı dosya olduğundan US1 Phase 3 tamamlandıktan sonra başla.

**US3 → US1 bağımlılığı**: T035 (`RetractScoreAsync`) US1'de kurulan `ScoreService` ve `IIdeaScoreRepository`'e bağlıdır.

**US4 → US2 bağımlılığı**: T039 (`GetScoreSummaryAsync` çağrısı) US2'de implement edilen metoda bağlıdır.

---

## Implementation Strategy

| MVP Scope | Açıklama |
|---|---|
| Phase 1–3 (US1) | Tek admin puanlama + form — admin bir idea'yı puanlayabilir, sayfada görebilir |
| + Phase 4 (US2) | Aggregate + breakdown tablosu + liste sütunu |
| + Phase 5 (US3) | Retract (puan geri çekme) |
| + Phase 6–7 (US4) | Submitter aggregate görünümü + polish |

**Parallel Opportunities**:
- T001–T009 (Setup): Tümü aynı anda çalışabilir
- T013, T014, T015 (DTO additions): Birbirinden bağımsız — paralel yazılabilir
- T033, T034 (Admin Detail + Index views): Aynı anda yazılabilir
- T042, T043, T044 (Polish): Tümü paralel

---

## Summary

| Phase | Görev Sayısı | Kapsam |
|---|---|---|
| Phase 1: Setup | 9 (T001–T009) | Boş dosya iskeletleri |
| Phase 2: Foundation | 12 (T010–T021) | Entity, repo, service interface, DI, migration |
| Phase 3: US1 | 7 (T022–T028) | Admin scoring form + submit |
| Phase 4: US2 | 6 (T029–T034) | Aggregate calculation + list/detail views |
| Phase 5: US3 | 3 (T035–T037) | Score retraction |
| Phase 6: US4 | 3 (T038–T040) | Submitter aggregate view |
| Phase 7: Polish | 7 (T041–T047) | Bulk method, XML docs, görsel, regresyon, build |
| **Toplam** | **47** | |
