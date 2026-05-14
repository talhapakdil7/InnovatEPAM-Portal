# Tasks: Multi-Stage Innovation Review Workflow

**Input**: Design documents from `specs/004-multi-stage-review/`

**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ideas.md ✓, quickstart.md ✓

**Tests**: Manual testing per `quickstart.md` (10 scenarios + 7 regression). No unit test tasks — MVP manual-testing-first approach (per constitution Gate 4 conditional pass).

**Organization**: Tasks grouped by user story for independent implementation and testing. US1 and US2 are merged (evaluation notes are entered during the advance action — inseparable).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Paralel çalışabilir (farklı dosyalar, bağımlılık yok)
- **[Story]**: Hangi user story'ye ait (US1–US5); US1+US2 = birleşik
- Her görevde tam dosya yolu belirtilmiştir

## Path Conventions

- **Proje kökü**: `src/InnovatEPAM.Portal/`
- **Controllers**: `src/InnovatEPAM.Portal/Controllers/`
- **Services**: `src/InnovatEPAM.Portal/Services/`
- **Models**: `src/InnovatEPAM.Portal/Models/`
- **Views**: `src/InnovatEPAM.Portal/Views/`

---

## Phase 1: Setup

**Amaç**: Yeni boş dosya iskeletlerini oluştur; derleme etkilenmez.

- [x] T001 `src/InnovatEPAM.Portal/Models/ReviewStage.cs` boş dosyasını oluştur (Phase 2'de doldurulacak)
- [x] T002 [P] `src/InnovatEPAM.Portal/Models/ReviewStageHelper.cs` boş dosyasını oluştur
- [x] T003 [P] `src/InnovatEPAM.Portal/Models/StageTransition.cs` boş dosyasını oluştur
- [x] T004 [P] `src/InnovatEPAM.Portal/DTOs/StageTransitionDTO.cs` boş dosyasını oluştur
- [x] T005 [P] `src/InnovatEPAM.Portal/ViewModels/ReviewWorkflowViewModels.cs` boş dosyasını oluştur
- [x] T006 [P] `src/InnovatEPAM.Portal/Services/Interfaces/IReviewWorkflowService.cs` boş dosyasını oluştur
- [x] T007 [P] `src/InnovatEPAM.Portal/Services/ReviewWorkflowService.cs` boş dosyasını oluştur
- [x] T008 [P] `src/InnovatEPAM.Portal/Repositories/Interfaces/IStageTransitionRepository.cs` boş dosyasını oluştur
- [x] T009 [P] `src/InnovatEPAM.Portal/Repositories/StageTransitionRepository.cs` boş dosyasını oluştur
- [x] T010 [P] `src/InnovatEPAM.Portal/Validators/AdvanceStageValidator.cs` boş dosyasını oluştur
- [x] T011 [P] `src/InnovatEPAM.Portal/Validators/RevertStageValidator.cs` boş dosyasını oluştur
- [x] T012 [P] `src/InnovatEPAM.Portal/Validators/RecordDecisionValidator.cs` boş dosyasını oluştur

**Checkpoint**: `dotnet build` hatasız; tüm dosyalar boş namespace bloğu ile derleniyor.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Amaç**: Tüm user story'lerin bağımlı olduğu model, DTO, ViewModel, interface, repository ve veritabanı altyapısı.

**⚠️ KRİTİK**: Bu phase tamamlanmadan hiçbir user story çalışmasına başlanamaz.

- [x] T013 `src/InnovatEPAM.Portal/Models/ReviewStage.cs` — `ReviewStage` enum'unu yaz: `InitialScreening = 1`, `TechnicalReview = 2`, `BusinessImpactAssessment = 3`, `FinalDecision = 4` (data-model.md §1)

- [x] T014 `src/InnovatEPAM.Portal/Models/ReviewStageHelper.cs` — `ReviewStageHelper` static sınıfını yaz:
  - `IReadOnlyList<ReviewStage> Stages` — enum değerlerini sıralı liste olarak döner
  - `ReviewStage? NextStage(ReviewStage current)` — sonraki stage, yoksa null
  - `bool IsFirstStage(ReviewStage stage)` — InitialScreening kontrolü
  - `bool IsLastStage(ReviewStage stage)` — FinalDecision kontrolü
  - `string DisplayName(ReviewStage stage)` — human-readable isim
  - `string DisplayName(ReviewStage? stage)` — null için "Pending Review" döner
  (data-model.md §9)

- [x] T015 `src/InnovatEPAM.Portal/Models/StageTransition.cs` — `StageTransition` entity sınıfını yaz:
  - `Guid Id`, `Guid IdeaId`, `Idea Idea` navigasyon
  - `ReviewStage? FromStage`, `ReviewStage ToStage`, `bool IsAdvance`
  - `string? Notes` (max 1000), `string? RevertReason` (max 500)
  - `string? Outcome` — sadece FinalDecision geçişlerinde "Accepted" veya "Rejected"
  - `Guid TransitionedByAdminId`, `ApplicationUser TransitionedByAdmin` navigasyon
  - `DateTime TransitionDate = DateTime.UtcNow`
  (data-model.md §2)

- [x] T016 `src/InnovatEPAM.Portal/Models/Idea.cs` — mevcut `Idea` entity'sine iki yeni property ekle:
  - `ReviewStage? CurrentReviewStage { get; set; }` (nullable, default null)
  - `ICollection<StageTransition> StageTransitions { get; set; } = new List<StageTransition>()`
  (data-model.md §3)

- [x] T017 [P] `src/InnovatEPAM.Portal/DTOs/StageTransitionDTO.cs` — `StageTransitionDTO` sınıfını yaz:
  - `string FromStageName` ("None" = ilk stage öncesi), `string ToStageName`, `int ToStageOrder`
  - `bool IsAdvance`, `string? Notes`, `string? RevertReason`, `string? Outcome`
  - `string TransitionedByAdmin`, `DateTime TransitionDate`
  (data-model.md §4)

- [x] T018 [P] `src/InnovatEPAM.Portal/DTOs/IdeaDetailDTO.cs` — mevcut sınıfa 3 yeni property ekle:
  - `string? CurrentReviewStageName` — null ise "Pending Review"
  - `int? CurrentReviewStageOrder` — 1–4; null ise stage henüz atanmamış
  - `List<StageTransitionDTO> StageHistory { get; set; } = new()`
  (data-model.md §5)

- [x] T019 [P] `src/InnovatEPAM.Portal/DTOs/IdeaListItemDTO.cs` — `string? CurrentReviewStageName` property'si ekle (null = stage atanmamış) (data-model.md §5)

- [x] T020 [P] `src/InnovatEPAM.Portal/ViewModels/ReviewWorkflowViewModels.cs` — üç ViewModel sınıfını yaz:
  - `AdvanceStageViewModel`: `Guid IdeaId`, `string? Notes` ([StringLength(1000)])
  - `RevertStageViewModel`: `Guid IdeaId`, `ReviewStage TargetStage`, `[Required] string RevertReason` ([StringLength(500, MinimumLength=1)]), `string? Notes` ([StringLength(1000)])
  - `RecordDecisionViewModel`: `Guid IdeaId`, `[Required] string Outcome`, `string? Notes` ([StringLength(1000)])
  (data-model.md §6)

- [x] T021 [P] `src/InnovatEPAM.Portal/Repositories/Interfaces/IStageTransitionRepository.cs` — interface'i yaz:
  - `Task<List<StageTransition>> GetByIdeaIdAsync(Guid ideaId)` — IdeaId'ye göre tarih sıralı (ASC)
  - `Task AddAsync(StageTransition transition)`

- [x] T022 `src/InnovatEPAM.Portal/Repositories/StageTransitionRepository.cs` — `IStageTransitionRepository` implementasyonu:
  - `GetByIdeaIdAsync`: `_db.StageTransitions.Where(t => t.IdeaId == ideaId).Include(t => t.TransitionedByAdmin).OrderBy(t => t.TransitionDate).ToListAsync()`
  - `AddAsync`: entity ekle + `SaveChangesAsync`

- [x] T023 [P] `src/InnovatEPAM.Portal/Services/Interfaces/IReviewWorkflowService.cs` — interface'i XML doc yorumlarıyla yaz:
  - `Task<(bool Success, string? Error)> AdvanceStageAsync(Guid ideaId, Guid adminId, string? notes)`
  - `Task<(bool Success, string? Error)> RevertStageAsync(Guid ideaId, ReviewStage targetStage, Guid adminId, string revertReason, string? notes)`
  - `Task<(bool Success, string? Error)> RecordFinalDecisionAsync(Guid ideaId, Guid adminId, string outcome, string? notes)`
  - `Task<List<StageTransitionDTO>> GetStageHistoryAsync(Guid ideaId)`
  (data-model.md §7)

- [x] T024 `src/InnovatEPAM.Portal/Data/ApplicationDbContext.cs` — güncelle:
  - `DbSet<StageTransition> StageTransitions => Set<StageTransition>();` ekle
  - `OnModelCreating`'e `StageTransition` konfigürasyonu ekle:
    - `ToTable("StageTransitions")`
    - `HasKey(t => t.Id)`
    - `Property(t => t.Notes).HasMaxLength(1000).IsRequired(false)`
    - `Property(t => t.RevertReason).HasMaxLength(500).IsRequired(false)`
    - `Property(t => t.Outcome).HasMaxLength(20).IsRequired(false)`
    - `Property(t => t.FromStage).IsRequired(false).HasConversion<int>()`
    - `Property(t => t.ToStage).IsRequired().HasConversion<int>()`
    - `HasOne(t => t.Idea).WithMany(i => i.StageTransitions).HasForeignKey(t => t.IdeaId).OnDelete(DeleteBehavior.Cascade)`
    - `HasOne(t => t.TransitionedByAdmin).WithMany().HasForeignKey(t => t.TransitionedByAdminId).OnDelete(DeleteBehavior.Restrict)`
    - `HasIndex(t => t.IdeaId)`, `HasIndex(t => t.TransitionedByAdminId)`, `HasIndex(t => t.TransitionDate)`
  - `Idea` entity konfigürasyonuna `Property(i => i.CurrentReviewStage).IsRequired(false).HasConversion<int?>()` ekle

- [x] T025 `src/InnovatEPAM.Portal/Data/Migrations/` — EF Core migration oluştur:
  - Terminalde çalıştır: `dotnet ef migrations add AddStageTransitions --project src/InnovatEPAM.Portal`
  - Migration: `Ideas` tablosuna nullable `CurrentReviewStage integer` kolonu; `StageTransitions` tablosu + tüm FK'lar + indexler oluşturulur
  - `dotnet ef database update` ile veritabanına uygula

- [x] T026 `src/InnovatEPAM.Portal/Mapping/AutoMapperProfile.cs` — güncellemeler:
  - `StageTransition → StageTransitionDTO` map ekle:
    - `FromStageName`: `s.FromStage.HasValue ? ReviewStageHelper.DisplayName(s.FromStage.Value) : "None"`
    - `ToStageName`: `ReviewStageHelper.DisplayName(s.ToStage)`
    - `ToStageOrder`: `(int)s.ToStage`
    - `TransitionedByAdmin`: `s.TransitionedByAdmin.FullName`
  - `Idea → IdeaDetailDTO` mapping'ine `Ignore()` ekle: `CurrentReviewStageName`, `CurrentReviewStageOrder`, `StageHistory` (servis katmanında doldurulacak — C1 bulgusu)
  - `Idea → IdeaListItemDTO` mapping'ine `CurrentReviewStageName` için `Ignore()` ekle (servis katmanında doldurulacak)

- [x] T027 `src/InnovatEPAM.Portal/Services/Interfaces/IIdeaService.cs` — `GetAllIdeasAsync` imzasını güncelle:
  - `Task<List<IdeaListItemDTO>> GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null, string? reviewStageFilter = null)`

- [x] T028 `src/InnovatEPAM.Portal/Services/IdeaService.cs` — güncelle:
  - `GetAllIdeasAsync`: `reviewStageFilter` null/boş değilse `ideas.Where(i => i.CurrentReviewStage.HasValue && ReviewStageHelper.DisplayName(i.CurrentReviewStage.Value) == reviewStageFilter)` filtresi ekle (categoryFilter'dan sonra uygulanır)
  - `GetAllIdeasAsync`: DTO'ları enrich ederken `dto.CurrentReviewStageName = ReviewStageHelper.DisplayName(idea.CurrentReviewStage)` ata (mapper sonrası)
  - `GetIdeaDetailAsync`: DTO oluşturulduktan sonra:
    - `dto.CurrentReviewStageName = ReviewStageHelper.DisplayName(idea.CurrentReviewStage)`
    - `dto.CurrentReviewStageOrder = idea.CurrentReviewStage.HasValue ? (int)idea.CurrentReviewStage.Value : (int?)null`
    - `dto.StageHistory = _mapper.Map<List<StageTransitionDTO>>(idea.StageTransitions.OrderBy(t => t.TransitionDate).ToList())`
    - NOT: `IIdeaRepository.GetByIdAsync`'in `StageTransitions`'ı include ettiğinden emin ol (IdeaRepository güncellemesi gerekebilir)

- [x] T029 `src/InnovatEPAM.Portal/Repositories/IdeaRepository.cs` — `GetByIdAsync` metodunda `.Include(i => i.StageTransitions).ThenInclude(t => t.TransitionedByAdmin)` ekle; `GetAllAsync` ve `GetBySubmitterAsync`'e de `Include(i => i.StageTransitions)` ekle

- [x] T030 `src/InnovatEPAM.Portal/Program.cs` — DI container'a kayıtları ekle:
  - `builder.Services.AddScoped<IReviewWorkflowService, ReviewWorkflowService>();`
  - `builder.Services.AddScoped<IStageTransitionRepository, StageTransitionRepository>();`

**Checkpoint**: `dotnet build` hatasız; migration uygulandı; tüm interface'ler ve servisler DI'a kayıtlı.

---

## Phase 3: User Story 1+2 — Admin Advances Idea Through Stages with Notes (Priority: P1) 🎯 MVP

**Hedef**: Admin, submitted/under-review bir fikri sıradaki review stage'e ilerletebilsin; isteğe bağlı değerlendirme notu girebilsin; geçmiş stage history admin detail sayfasında görünsün.

**Bağımsız Test**: Admin olarak giriş yap, submitted bir fikri Initial Screening'e ilerlet (not girerek), Technical Review'a ilerlet (not girmeden), Business Impact Assessment'a ilerlet — her adımda stage'in doğru güncellendiğini ve history'nin timestamp + admin adıyla göründüğünü doğrula. Submitter'ın bu işlemleri yapamadığını test et.

### US1+US2 Implementasyonu

- [x] T031 [P] [US1] `src/InnovatEPAM.Portal/Validators/AdvanceStageValidator.cs` — `AbstractValidator<AdvanceStageViewModel>` implement et:
  - `RuleFor(x => x.IdeaId).NotEmpty()`
  - `RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("Notes must be at most 1000 characters.")`

- [x] T032 [US1] `src/InnovatEPAM.Portal/Services/ReviewWorkflowService.cs` — sınıf iskeletini yaz, bağımlılıkları inject et (`IIdeaRepository`, `IAuditLogRepository`, `IStageTransitionRepository`, `IIdeaService`, `IMapper`, `ILogger<ReviewWorkflowService>`), ardından şunları implemente et:
  - **`AdvanceStageAsync`**:
    - `var idea = await _ideaRepo.GetByIdAsync(ideaId)` — null ise `(false, "Idea not found.")`
    - Precondition: `idea.Status ∉ {Draft, Accepted, Rejected}` → `(false, "Cannot advance stage for this idea.")`
    - Precondition: `idea.CurrentReviewStage == FinalDecision` → `(false, "Already at Final Decision.")`
    - `nextStage = idea.CurrentReviewStage == null ? ReviewStage.InitialScreening : ReviewStageHelper.NextStage(idea.CurrentReviewStage.Value)!.Value`
    - Eğer `idea.Status == Submitted`: `await _ideaService.UpdateStatusAsync(ideaId, "UnderReview", adminId)` çağır
    - `var fromStage = idea.CurrentReviewStage`; `idea.CurrentReviewStage = nextStage`; `await _ideaRepo.UpdateAsync(idea)`
    - `await _transitionRepo.AddAsync(new StageTransition { Id = Guid.NewGuid(), IdeaId = ideaId, FromStage = fromStage, ToStage = nextStage, IsAdvance = true, Notes = notes, TransitionedByAdminId = adminId, TransitionDate = DateTime.UtcNow })`
    - Log: `_logger.LogInformation("Idea {IdeaId} advanced to {Stage} by admin {AdminId}", ...)`
    - `return (true, null)`
  - **`GetStageHistoryAsync`**:
    - `var transitions = await _transitionRepo.GetByIdeaIdAsync(ideaId)`
    - `return _mapper.Map<List<StageTransitionDTO>>(transitions)`

- [x] T033 [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `AdvanceStage` POST action ekle:
  - `[HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]`
  - `public async Task<IActionResult> AdvanceStage(AdvanceStageViewModel vm)`
  - ModelState geçersizse: `TempData["Error"] = ...` + `RedirectToAction(nameof(Detail), new { id = vm.IdeaId })`
  - `var adminId = Guid.Parse(_userManager.GetUserId(User)!)`
  - `var (success, error) = await _reviewWorkflowService.AdvanceStageAsync(vm.IdeaId, adminId, vm.Notes)`
  - Hata: `TempData["Error"] = error` → redirect Detail; Başarı: `TempData["Success"] = $"Stage advanced to {stageName}."` → redirect Detail

- [x] T034 [US1] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Detail` action'ını güncelle:
  - `IReviewWorkflowService` constructor'a inject et
  - Detail ViewModel'de `IdeaDetailDTO` artık `CurrentReviewStageName`, `CurrentReviewStageOrder`, `StageHistory` içeriyor (T028'de servis katmanında dolduruldu)
  - `AdminIdeaDetailViewModel`'e `AvailableRevertStages` (admin'in revert edebileceği stage listesi) hesapla: `ReviewStageHelper.Stages.Where(s => s < idea.CurrentReviewStage).Select(s => new {Value = s, Name = ReviewStageHelper.DisplayName(s)})`

- [x] T035 [US1] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — "Review Workflow" paneli ekle:
  - Sayfanın üst kısmına (mevcut description card'ından önce) stage progress indicator ekle: 4 adımlı Bootstrap step-indicator (step 1–4), mevcut stage'i aktif göster; henüz stage atanmamışsa "Pending Review" göster
  - **Advance butonu formu**: `@if (idea.Status ∉ {Accepted, Rejected, Draft} && idea.CurrentReviewStageOrder < 4 || idea.CurrentReviewStageName == null)` bloğu içinde:
    - `<form asp-action="AdvanceStage" method="post">` + antiforgery + hidden `IdeaId`
    - `<textarea asp-for="Notes" ... maxlength="1000" placeholder="Optional evaluation notes..." rows="3"></textarea>`
    - Buton: `<button type="submit" class="btn btn-primary">Advance to @(nextStageName)</button>` (next stage adı Razor'da `ReviewStageHelper.NextStage(...)` ile)
  - **Stage History** bölümü: `@if (Model.Idea.StageHistory.Any())` — transition listesi; her satırda: ok ikonu (→ advance, ← revert), stage adı, admin adı, tarih, notlar; revert ise kırmızı ok ve revert reason göster

**Checkpoint**: Admin detail sayfasında stage progress görünüyor; Advance butonu çalışıyor; notlar kaydediliyor; history gösteriliyor. Submitter bu formları göremez.

---

## Phase 4: User Story 3 — Admin Records Final Decision (Priority: P1)

**Hedef**: Final Decision aşamasındaki fikir için Admin, Accept veya Reject kararını kayıt edebilsin; fikrin genel statüsü buna göre güncellensin.

**Bağımsız Test**: Final Decision'daki bir fikri Accept'le; genel statünün "Accepted" olduğunu, admin listesinde göründüğünü doğrula. Başka bir fikri Reject'le. Her iki durumda da Advance/Revert butonlarının kaybolduğunu doğrula.

### US3 Implementasyonu

- [x] T036 [P] [US3] `src/InnovatEPAM.Portal/Validators/RecordDecisionValidator.cs` — `AbstractValidator<RecordDecisionViewModel>` implement et:
  - `RuleFor(x => x.IdeaId).NotEmpty()`
  - `RuleFor(x => x.Outcome).NotEmpty().Must(o => o == "Accepted" || o == "Rejected").WithMessage("Outcome must be Accepted or Rejected.")`
  - `RuleFor(x => x.Notes).MaximumLength(1000)`

- [x] T037 [US3] `src/InnovatEPAM.Portal/Services/ReviewWorkflowService.cs` — `RecordFinalDecisionAsync` metodunu implemente et:
  - `var idea = await _ideaRepo.GetByIdAsync(ideaId)` — null ise `(false, "Idea not found.")`
  - Precondition: `idea.Status == UnderReview` → aksi hâlde `(false, "Idea is not under review.")`
  - Precondition: `idea.CurrentReviewStage == FinalDecision` → aksi hâlde `(false, "Idea must be in Final Decision stage.")`
  - Precondition: `outcome ∈ {"Accepted", "Rejected"}` → aksi hâlde `(false, "Invalid outcome.")`
  - `await _ideaService.UpdateStatusAsync(ideaId, outcome, adminId)` — AuditLog'u servis oluşturur
  - `await _transitionRepo.AddAsync(new StageTransition { ..., FromStage = FinalDecision, ToStage = FinalDecision, IsAdvance = true, Outcome = outcome, Notes = notes, ... })`
  - Log + `return (true, null)`

- [x] T038 [US3] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `RecordDecision` POST action ekle:
  - `[HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]`
  - `public async Task<IActionResult> RecordDecision(RecordDecisionViewModel vm)`
  - Servis çağrısı + TempData + redirect pattern (T033 ile aynı)
  - Başarı: `TempData["Success"] = $"Idea marked as {vm.Outcome}."`

- [x] T039 [US3] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — **Final Decision paneli** ekle:
  - `@if (Model.Idea.CurrentReviewStageName == "Final Decision" && Model.Idea.Status == "UnderReview")` bloğu içine:
    - `<form asp-action="RecordDecision" method="post">` + hidden `IdeaId`
    - Accept ve Reject radio butonları veya iki ayrı submit butonu (`formaction` ile)
    - Notes textarea (max 1000)
    - "Submit Decision" primary button
  - `@if (Model.Idea.Status == "Accepted" || Model.Idea.Status == "Rejected")` → mevcut workflow panelini gizle; sadece history göster

**Checkpoint**: Final Decision aşamasında karar verme çalışıyor; genel status güncelleniyor; Accepted/Rejected fikirler için workflow paneli kaybolup yalnızca history görünüyor.

---

## Phase 5: User Story 4 — Admin Reverts a Review Stage (Priority: P2)

**Hedef**: Admin, fikri önceki bir review stage'e geri alabilsin; revert reason zorunlu olsun; history'de geri alma açıkça ayırt edilebilsin.

**Bağımsız Test**: Business Impact Assessment'taki fikri Technical Review'a geri al, neden: "Need deeper technical analysis". History'de geri alma kaydının revert ikonu/etiketi ile göründüğünü doğrula. Initial Screening'deki fikri geri almaya çalış — sistem engelliyeceğini doğrula.

### US4 Implementasyonu

- [x] T040 [P] [US4] `src/InnovatEPAM.Portal/Validators/RevertStageValidator.cs` — `AbstractValidator<RevertStageViewModel>` implement et:
  - `RuleFor(x => x.IdeaId).NotEmpty()`
  - `RuleFor(x => x.TargetStage).IsInEnum().WithMessage("Invalid target stage.")`
  - `RuleFor(x => x.RevertReason).NotEmpty().MaximumLength(500).WithMessage("Revert reason is required (max 500 characters).")`
  - `RuleFor(x => x.Notes).MaximumLength(1000)`

- [x] T041 [US4] `src/InnovatEPAM.Portal/Services/ReviewWorkflowService.cs` — `RevertStageAsync` metodunu implemente et:
  - `var idea = await _ideaRepo.GetByIdAsync(ideaId)` — null ise `(false, "Idea not found.")`
  - Precondition: `idea.Status == UnderReview AND idea.Status ∉ {Accepted, Rejected}` → aksi `(false, "Cannot revert.")`
  - Precondition: `idea.CurrentReviewStage != null AND idea.CurrentReviewStage != InitialScreening` → `(false, "Already at first stage.")`
  - Precondition: `targetStage < idea.CurrentReviewStage` → aksi `(false, "Target must be an earlier stage.")`
  - `var fromStage = idea.CurrentReviewStage`; `idea.CurrentReviewStage = targetStage`; `await _ideaRepo.UpdateAsync(idea)`
  - `await _transitionRepo.AddAsync(new StageTransition { ..., FromStage = fromStage, ToStage = targetStage, IsAdvance = false, RevertReason = revertReason, Notes = notes, ... })`
  - Log + `return (true, null)`

- [x] T042 [US4] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `RevertStage` POST action ekle:
  - `[HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]`
  - `public async Task<IActionResult> RevertStage(RevertStageViewModel vm)`
  - `var adminId = Guid.Parse(_userManager.GetUserId(User)!)`
  - `var (success, error) = await _reviewWorkflowService.RevertStageAsync(vm.IdeaId, vm.TargetStage, adminId, vm.RevertReason, vm.Notes)`
  - TempData + redirect pattern

- [x] T043 [US4] `src/InnovatEPAM.Portal/Views/Admin/Detail.cshtml` — **Revert paneli** ekle:
  - `@if (idea.CurrentReviewStageOrder > 1 && idea.Status == "UnderReview")` bloğu içine "Revert Stage" collapse section (Bootstrap accordion/collapse):
    - `<form asp-action="RevertStage" method="post">` + hidden `IdeaId`
    - `<select asp-for="TargetStage">` — `AvailableRevertStages` listesinden seçenekler
    - `<textarea asp-for="RevertReason" required maxlength="500" ...></textarea>`
    - `<textarea asp-for="Notes" maxlength="1000" ...></textarea>`
    - "Revert Stage" outline-warning butonu
  - Stage History'de revert kayıtlarını ayırt etmek için farklı ikon/renk (ör. `bi-arrow-counterclockwise text-warning`) ve "Reverted to: {stage}" etiketi

**Checkpoint**: Revert çalışıyor; revert reason zorunlu; Initial Screening'den geri alınamıyor; history'de revert kayıtları ileri geçişlerden ayırt edilebilir.

---

## Phase 6: User Story 5 — Submitter Tracks Review Stage + Admin Stage Filter (Priority: P2)

**Hedef**: Submitter, fikir detay sayfasında review stage'ini read-only görebilsin; Admin, fikir listesinde stage'e göre filtreleyebilsin.

**Bağımsız Test**: Submitter olarak giriş yap, Technical Review'daki bir fikrin detay sayfasını aç — stage progress görünüyor ve edit butonu yok. Admin olarak fikir listesini "Technical Review" filtresiyle filtrele — sadece o stage'deki fikirler görünüyor.

### US5 Implementasyonu

- [x] T044 [P] [US5] `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` — `AdminIdeaListViewModel`'e ekle:
  - `string? ReviewStageFilter { get; set; }`
  - `List<string> AvailableReviewStages { get; set; } = new()` — `ReviewStageHelper.Stages.Select(ReviewStageHelper.DisplayName).ToList()`

- [x] T045 [US5] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Index` action'ını güncelle:
  - `string? reviewStageFilter` parametresi ekle
  - `await _ideaService.GetAllIdeasAsync(statusFilter, categoryFilter, reviewStageFilter)` — yeni parametre geçilir
  - `AvailableReviewStages = ReviewStageHelper.Stages.Select(ReviewStageHelper.DisplayName).ToList()` doldur
  - `ReviewStageFilter = reviewStageFilter` ata

- [x] T046 [US5] `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml` — filtre ve tablo güncellemeleri:
  - Filtre formuna üçüncü dropdown ekle:
    ```html
    <div>
      <label class="form-label small text-muted mb-1">Review Stage</label>
      <select name="reviewStageFilter" class="form-select w-auto">
        <option value="">All Stages</option>
        @foreach (var stage in Model.AvailableReviewStages) { ... }
      </select>
    </div>
    ```
  - Tablo başlığına "Review Stage" sütunu ekle (Category ve Status sütunları arasına)
  - Tablo gövdesinde her satıra `@(idea.CurrentReviewStageName ?? "—")` yaz; atanmış stage varsa küçük badge ile göster

- [x] T047 [US5] `src/InnovatEPAM.Portal/Views/Ideas/Detail.cshtml` — submitter için read-only "Review Progress" bölümü ekle:
  - `@if (!string.IsNullOrEmpty(Model.Idea.CurrentReviewStageName))` bloğu içinde:
    - "Review Progress" başlıklı card
    - `CurrentReviewStageOrder` değerine göre 4 adımlı step indicator (1/4, 2/4 vb.): Bootstrap progress bar veya custom step list
    - Mevcut stage adı badge olarak göster
    - Hiçbir Advance/Revert/Decision formu yok (sadece statü bilgisi)
  - `@else { <p class="text-muted small">Pending Review</p> }` — stage henüz atanmamış

**Checkpoint**: Submitter detail sayfasında read-only stage progress görünüyor; Admin filtresi stage'e göre çalışıyor; mevcut Status ve Category filtreleri bozulmadı.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Amaç**: XML dokümantasyon, görsel tutarlılık, entegrasyon doğrulaması.

- [x] T048 [P] Tüm yeni public sınıf ve metodlara XML dokümantasyon yorumları ekle (constitution Prensibi II):
  - `ReviewStage.cs`, `ReviewStageHelper.cs`, `StageTransition.cs`
  - `StageTransitionDTO.cs`, `ReviewWorkflowViewModels.cs`
  - `IReviewWorkflowService.cs` — tüm metodlar
  - `ReviewWorkflowService.cs` — tüm public metodlar + constructor

- [x] T049 [P] Admin Detail görünümünde edge case doğrulaması:
  - Stage atanmamış (null) → "Pending Review" gösteriyor mu?
  - Accepted/Rejected fikir → workflow paneli gizleniyor, sadece history görünüyor mu?
  - Final Decision stage → Advance butonu yok, sadece Decision paneli var mı?
  - Initial Screening'de → Revert paneli gizli/devre dışı mı?

- [x] T050 `quickstart.md` Senaryo 1–10'u ve 7 regresyon testini sırayla çalıştır; sonuçları kaydet

---

## Bağımlılıklar ve Çalıştırma Sırası

### Phase Bağımlılıkları

```
Phase 1 (T001–T012: Boş dosya iskeletleri)
    ↓
Phase 2 (T013–T030: Enum, Entity, DTO, ViewModel, Interface, Service, Repo, Migration, DI)
    ↓
Phase 3 US1+US2 (T031–T035) — AdvanceStage + Notes + History
    ↓ (US3 ve US4 paralel başlayabilir; farklı servis metodları + farklı view bölümleri)
Phase 4 US3 (T036–T039) — Final Decision
Phase 5 US4 (T040–T043) — Revert Stage
    ↓ (her iki story tamamlanınca)
Phase 6 US5 (T044–T047) — Submitter Progress + Admin Filter
    ↓
Phase 7 Polish (T048–T050)
```

### User Story Bağımlılıkları

- **US1+US2 (P1)**: Foundation tamamlanınca başlayabilir — `ReviewWorkflowService.AdvanceStageAsync` bağımsız
- **US3 (P1)**: US1+US2 ile paralel başlayabilir — aynı servis sınıfına method ekleniyor; farklı action ve farklı view bloğu
- **US4 (P2)**: US1+US2 tamamlanınca başlayabilir (Revert paneli aynı Admin Detail view'ında; merge conflict riski)
- **US5 (P2)**: Foundation tamamlanınca başlayabilir; US1/US2/US3/US4'ten bağımsız (farklı view'lar + Index action)

### Her User Story İçinde

- Validator → Service method → Controller action → View sırası izlenir
- Paralel [P] görevler farklı dosyalarda olduğu için eş zamanlı çalışabilir
- Her story checkpoint'inde bağımsız test edilmeli

### Paralel Fırsatlar

- Phase 2: T013–T023 büyük ölçüde paralel (farklı dosyalar); T024 (DB context) T013–T023 tamamlanınca; T025 (migration) T024 sonrası
- Phase 3 + Phase 4 paralel başlayabilir (farklı servis metodları + farklı view section'ları)
- Phase 4 US4 Validator (T040) + Phase 4 US3 Validator (T036) paralel
- Phase 7: T048 + T049 paralel

---

## Paralel Örnek: Phase 2 (Foundational)

**Tek developer — önerilen sıra**:

1. T013 → ReviewStage enum
2. T014 [P] + T015 [P] + T017 [P] + T019 [P] + T020 [P] paralel → Helper + Entity + StageTransitionDTO + IdeaDetailDTO/ListItemDTO + ViewModels
3. T016 → Idea.cs (entity değişikliği T015'e bağlı)
4. T021 [P] + T023 [P] paralel → Repository interface + Service interface
5. T022 → StageTransitionRepository (T021 bağlı)
6. T024 → DbContext (T015 + T016 bağlı)
7. T025 → Migration (T024 bağlı)
8. T026 → AutoMapper (T017 + T019 bağlı)
9. T027 [P] + T029 [P] paralel → IIdeaService signature + IdeaRepository include
10. T028 → IdeaService (T027 + T029 bağlı)
11. T030 → Program.cs DI

**İki developer ile paralel**:

Developer A: T013 → T015 → T016 → T024 → T025
Developer B: T014 + T017 + T019 + T020 + T021 + T023 → T022

---

## Implementation Strategy

### MVP First (US1+US2 — P1)

1. Phase 1 + Phase 2 tamamla (Foundation)
2. Phase 3 (US1+US2) tamamla: AdvanceStage action + notes + stage history
3. **DUR ve DOĞRULA**: `quickstart.md` Senaryo 1 + 2'yi test et
4. Çalışıyorsa Phase 4 (US3: Final Decision) + Phase 5 (US4: Revert)'e geç

### Incremental Delivery

1. Phase 1–2 → Altyapı hazır; `dotnet build` doğrula
2. Phase 3 → Advance + notes + history çalışıyor (Senaryo 1–2)
3. Phase 4 → Final Decision çalışıyor (Senaryo 3–4)
4. Phase 5 → Revert çalışıyor (Senaryo 5)
5. Phase 6 → Submitter progress + admin filtresi çalışıyor (Senaryo 6–8)
6. Phase 7 → Tüm manuel testler + regresyon

---

## Görev Sayısı Özeti

| Phase | Görev Sayısı | Paralel Fırsat |
|---|---|---|
| Phase 1: Setup | 12 | T002–T012 paralel |
| Phase 2: Foundational | 18 | T014–T023 büyük ölçüde paralel |
| Phase 3: US1+US2 | 5 | T031 [P] |
| Phase 4: US3 | 4 | T036 [P] |
| Phase 5: US4 | 4 | T040 [P] |
| Phase 6: US5 | 4 | T044 [P] |
| Phase 7: Polish | 3 | T048 + T049 paralel |
| **Toplam** | **50** | |

- US1+US2 (P1 — MVP): 5 görev
- US3 (P1): 4 görev
- US4 (P2): 4 görev
- US5 (P2): 4 görev
