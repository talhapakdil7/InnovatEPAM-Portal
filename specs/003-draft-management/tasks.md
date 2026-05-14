# Tasks: Idea Draft Management

**Input**: Design documents from `specs/003-draft-management/`

**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ideas.md ✓, quickstart.md ✓

**Tests**: Manual testing per `quickstart.md` (10 scenarios + 6 regression). No unit test tasks — MVP manual-testing-first approach (per constitution Gate 4 conditional pass).

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

---

## Phase 1: Setup

**Amaç**: Enum değişikliği — veritabanı migrasyonu gerekmez; tamsayı 0 daha önce kullanılmıyordu.

- [x] T001 `src/InnovatEPAM.Portal/Models/Idea.cs` içindeki `IdeaStatus` enum'una `Draft = 0` değerini ilk sırada ekle; mevcut Submitted=1, UnderReview=2, Accepted=3, Rejected=4 değerleri değişmez. `Idea` entity'sinin default değerini `IdeaStatus.Submitted` olarak koruyun (Create path'i etkilenmesin).

**Checkpoint**: Derleme başarılı; enum `Draft`, `Submitted`, `UnderReview`, `Accepted`, `Rejected` içeriyor.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Amaç**: Tüm user story'lerin bağımlı olduğu ViewModel, Validator, Interface ve Service altyapısı.

**⚠️ KRİTİK**: Bu phase tamamlanmadan hiçbir user story çalışmasına başlanamaz.

- [x] T002 [P] `src/InnovatEPAM.Portal/ViewModels/IdeaViewModels.cs` dosyasını güncelle:
  - `EditDraftViewModel` sınıfını ekle: `Id` (Guid), `Category` (string?), `Title` (Required, max 200), `Description` (max 2000), `ExistingAttachment` (IdeaAttachmentDTO?), `RemoveAttachment` (bool), `Attachment` (IFormFile?), ve spec 002'deki 9 kategori alanı property'si (`TechArea`, `TechEffort`, `TechBenefit`, `ProcDepartment`, `ProcPainPoint`, `ProcSavings`, `ClientSegment`, `ClientProblem`, `ClientImpact`)
  - `IdeaDetailViewModel`'e `IsDraft` (bool) property'si ekle: controller tarafından `idea.Status == "Draft"` ise `true` olarak set edilecek

- [x] T003 [P] `src/InnovatEPAM.Portal/Validators/EditDraftValidator.cs` dosyasını OLUŞTUR:
  - `AbstractValidator<EditDraftViewModel>` base class kullan
  - `Category` zorunlu + `CategoryDefinitions.All.ContainsKey` kontrolü
  - `Title` zorunlu, max 200
  - `Description` max 2000
  - Ek dosya için: boyut ≤ 10 MB ve izin verilen uzantı kontrolü (CreateIdeaValidator ile aynı)
  - `When(x => x.Category == CategoryDefinitions.TechnicalImprovement, ...)`: TechArea (required, geçerli option), TechEffort (required, geçerli option), TechBenefit (required, max 500)
  - `When(x => x.Category == CategoryDefinitions.ProcessImprovement, ...)`: ProcDepartment (required, 100), ProcPainPoint (required, 500), ProcSavings (optional, 200)
  - `When(x => x.Category == CategoryDefinitions.ClientSolution, ...)`: ClientSegment (required, 200), ClientProblem (required, 500), ClientImpact (required, 300)

- [x] T004 [P] `src/InnovatEPAM.Portal/Services/Interfaces/IIdeaService.cs` dosyasına 4 yeni method imzası ekle (XML doc yorumlarıyla):
  - `Task<(bool Success, string? Error, Guid DraftId)> SaveDraftAsync(Guid submitterId, CreateIdeaViewModel vm)`
  - `Task<(bool Success, string? Error)> UpdateDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm)`
  - `Task<(bool Success, string? Error)> SubmitDraftAsync(Guid draftId, Guid submitterId, EditDraftViewModel vm)`
  - `Task<(bool Success, string? Error)> DeleteDraftAsync(Guid draftId, Guid submitterId)`

- [x] T005 `src/InnovatEPAM.Portal/Services/IdeaService.cs` — `GetAllIdeasAsync` metodunu güncelle: `var ideas = await _ideaRepo.GetAllAsync();` satırından hemen sonra `ideas = ideas.Where(i => i.Status != IdeaStatus.Draft).ToList();` ekle. Bu filtre `statusFilter` parametresinden önce uygulanmalı (FR-010: admin tarafı draft göremez).

- [x] T006 `src/InnovatEPAM.Portal/Services/IdeaService.cs` — `SaveDraftAsync` metodunu implemente et:
  - Yeni `Idea` nesnesi oluştur: `Status = IdeaStatus.Draft`, tüm form alanlarını ata (`Category`, `CategoryData = BuildCategoryData(vm)`, `Title`, `Description`)
  - Varsa eki yükle (mevcut `CreateIdeaAsync` attachment mantığıyla aynı — MIME kontrolü + dosya kaydı)
  - `await _ideaRepo.AddAsync(idea)` çağır
  - `(true, null, idea.Id)` döndür; hata durumunda `(false, errorMessage, Guid.Empty)` döndür

- [x] T007 `src/InnovatEPAM.Portal/Services/IdeaService.cs` — `UpdateDraftAsync` metodunu implemente et:
  - `await _ideaRepo.GetByIdAsync(draftId)` ile fikri al; null veya `Status != Draft` veya `SubmitterId != submitterId` ise `(false, "Not found or access denied.")` döndür
  - Alanları güncelle: `Category`, `CategoryData = BuildCategoryData(vm als EditDraft için yeni overload)`, `Title`, `Description`, `LastModifiedDate = DateTime.UtcNow`
  - `vm.RemoveAttachment == true` ise: mevcut eki diskten sil (FileStorageHelper yardımıyla) + EF'ten kaldır
  - `vm.Attachment != null` ise: MIME + boyut kontrolü yap, yeni dosyayı kaydet, eski eki sil (varsa), yeni `IdeaAttachment` ekle
  - `await _ideaRepo.UpdateAsync(idea)` çağır
  - Not: `BuildCategoryData` metodunu `EditDraftViewModel` ile de çalışacak şekilde refactor et veya overload ekle

- [x] T008 `src/InnovatEPAM.Portal/Services/IdeaService.cs` — `SubmitDraftAsync` metodunu implemente et:
  - Ownership + `Status == Draft` kontrolü yap (`UpdateDraftAsync` ile aynı başlangıç mantığı)
  - `UpdateDraftAsync(draftId, submitterId, vm)` metodunu çağırarak son form durumunu kaydet
  - Başarılıysa: `idea.Status = IdeaStatus.Submitted`, `idea.LastModifiedDate = DateTime.UtcNow`, `await _ideaRepo.UpdateAsync(idea)`
  - `(true, null)` döndür; hata durumunda `(false, error)` döndür

- [x] T009 `src/InnovatEPAM.Portal/Services/IdeaService.cs` — `DeleteDraftAsync` metodunu implemente et:
  - Fikri al; ownership + `Status == Draft` kontrolü yap
  - Tüm `IdeaAttachment` kayıtlarını döngüyle dolaş: her dosyayı diskten sil (FileStorageHelper ile)
  - `_db.Ideas.Remove(idea)` + `await _db.SaveChangesAsync()` — EF cascade ile IdeaAttachment kayıtları da silinir
  - `(true, null)` döndür; hata durumunda `(false, error)` döndür

**Checkpoint**: Foundation hazır — `dotnet build` hatasız; tüm interface metodları servis katmanında implemente edilmiş.

---

## Phase 3: User Story 1 — Save Idea as Draft (Priority: P1) 🎯 MVP

**Hedef**: Submitter, Create formundan "Save as Draft" butonuyla kısmi veriyi validation olmadan kaydedebilsin.

**Bağımsız Test**: Login ol, `/Ideas/Create` aç, kısmen doldur, "Save as Draft" tıkla, Edit sayfasına yönlendirildiğini ve **My Ideas**'da "Draft" badge'iyle göründüğünü doğrula.

### US1 Implementasyonu

- [x] T010 [US1] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` dosyasına `SaveDraft` POST action ekle:
  - `[HttpPost, ValidateAntiForgeryToken]` attribute'ları ekle
  - `public async Task<IActionResult> SaveDraft(CreateIdeaViewModel vm)` imzası
  - `ModelState` **kontrolü yapma** (FR-001: validation tetiklenmemeli)
  - `var userId = Guid.Parse(_userManager.GetUserId(User)!);` al
  - `var (success, error, draftId) = await _ideaService.SaveDraftAsync(userId, vm);` çağır
  - Başarısızsa: `ModelState.AddModelError + return View("Create", vm)`
  - Başarılıysa: `TempData["Success"] = "Draft saved."; return RedirectToAction("Edit", new { id = draftId });`

- [x] T011 [US1] `src/InnovatEPAM.Portal/Views/Ideas/Create.cshtml` dosyasını güncelle:
  - Mevcut `<button type="submit" class="btn btn-primary">` "Submit Idea" butonunu koru
  - Yanına ikinci bir buton ekle: `<button type="submit" formaction="@Url.Action("SaveDraft")" class="btn btn-outline-secondary"><i class="bi bi-floppy me-1"></i>Save as Draft</button>`
  - Anti-forgery token'ın `<form>` tag'inde mevcut olduğundan emin ol (zaten var: `asp-action="Create"`)

**Checkpoint**: Create formundan kısmi veri ile "Save as Draft" çalışıyor; validation hatası yok; yönlendirme doğru.

---

## Phase 4: User Story 2 — View and Manage Drafts (Priority: P2)

**Hedef**: Submitter, My Ideas listesinde Draft badge'ini görebilsin, detay sayfasından silebilsin.

**Bağımsız Test**: Birkaç draft oluştur, My Ideas'da "Draft" muted badge'ini gör; birini sil ve kaybolduğunu doğrula.

### US2 Implementasyonu

- [x] T012 [US2] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` dosyasındaki `Detail` action'ını güncelle:
  - `IdeaDetailViewModel` oluşturulurken `IsDraft = idea.Status == "Draft"` olarak set et

- [x] T013 [US2] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` dosyasına `DeleteDraft` POST action ekle:
  - `[HttpPost, ValidateAntiForgeryToken]` attribute'ları
  - `public async Task<IActionResult> DeleteDraft(Guid id)` imzası
  - `var userId = Guid.Parse(_userManager.GetUserId(User)!);`
  - `var (success, error) = await _ideaService.DeleteDraftAsync(id, userId);`
  - Başarısızsa: `TempData["Error"] = error; return RedirectToAction("Index");`
  - Başarılıysa: `TempData["Success"] = "Draft deleted."; return RedirectToAction("Index");`

- [x] T014 [US2] `src/InnovatEPAM.Portal/Views/Ideas/Index.cshtml` dosyasını güncelle:
  - Badge renk switch'ine `"Draft" => "bg-secondary bg-opacity-50"` case'ini ekle (muted farklı badge)
  - Status filtresi `AvailableStatuses` listesinin Draft badge'iyle çalıştığını doğrula (Draft enum değeri otomatik gelecek)

- [x] T015 [US2] `src/InnovatEPAM.Portal/Views/Ideas/Detail.cshtml` dosyasını güncelle:
  - `@if (Model.IsDraft)` bloğu içine şunları ekle:
    - "Edit Draft" butonu: `<a asp-action="Edit" asp-route-id="@Model.Idea.Id" class="btn btn-outline-primary"><i class="bi bi-pencil me-1"></i>Edit Draft</a>`
    - "Delete Draft" formu+butonu: `<form asp-action="DeleteDraft" method="post">` içinde hidden `id` input + submit `<button class="btn btn-outline-danger">Delete Draft</button>` (anti-forgery token dahil)
    - İki buton aynı `d-flex gap-2` container içinde

**Checkpoint**: My Ideas'da Draft'lar muted badge ile görünüyor; Detail sayfasında Edit/Delete butonları sadece Draft için görünüyor; Delete çalışıyor.

---

## Phase 5: User Story 3 — Continue Editing a Draft (Priority: P2)

**Hedef**: Submitter, kaydedilmiş draft'ı açsın, tüm alanlar pre-filled gelsin, değiştirip tekrar kaydedebilsin.

**Bağımsız Test**: Draft oluştur → Detail → Edit Draft → alanlar dolu mu kontrol et → değiştir → Save as Draft → yeniden aç → değişiklikler kalıcı mı doğrula.

### US3 Implementasyonu

- [x] T016 [US3] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` dosyasına `GET Edit` action ekle:
  - `[HttpGet]` attribute
  - `public async Task<IActionResult> Edit(Guid id)` imzası
  - `var userId = Guid.Parse(_userManager.GetUserId(User)!);`
  - `var idea = await _ideaService.GetIdeaDetailAsync(id, userId, isAdmin: false);`
  - `if (idea == null || idea.Status != "Draft") return NotFound();`
  - `EditDraftViewModel`'i oluştur ve tüm alanları `IdeaDetailDTO`'dan doldur:
    - `Id = id`, `Category = idea.Category`, `Title = idea.Title`, `Description = idea.Description`
    - `ExistingAttachment = idea.Attachments.FirstOrDefault()`
    - Kategori alanlarını `idea.CategoryDataFields` dictionary'sinden label→key tersine çevirerek doldur (veya `idea.Category` + CategoryDefinitions ile JSON'dan oku)
  - `return View(vm);`

- [x] T017 [US3] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` dosyasına `POST UpdateDraft` action ekle:
  - `[HttpPost, ValidateAntiForgeryToken]`
  - `public async Task<IActionResult> UpdateDraft(Guid id, EditDraftViewModel vm)` imzası
  - `ModelState` **kontrolü yapma** (FR-006: kayıt için validation yok)
  - `var userId = Guid.Parse(_userManager.GetUserId(User)!);`
  - `var (success, error) = await _ideaService.UpdateDraftAsync(id, userId, vm);`
  - Hata durumunda: Edit view'ını mevcut vm ile döndür + ModelState hatası
  - Başarılıysa: `TempData["Success"] = "Draft saved."; return RedirectToAction("Edit", new { id });`

- [x] T018 [US3] `src/InnovatEPAM.Portal/Views/Ideas/Edit.cshtml` dosyasını OLUŞTUR:
  - `@model EditDraftViewModel` ile başla
  - Sayfa başlığı: "Edit Draft — @Model.Title"
  - Kategori dropdown (Create.cshtml ile aynı yapı — CategoryDefinitions.All'dan) — `asp-for="Category"` ile
  - Create.cshtml'deki 3 kategori section'ını (TechnicalImprovement, ProcessImprovement, ClientSolution) kopyala — aynı `id="section-{CategoryKey}"` ve `class="category-section"` yapısını koru (JS aynı kalacak)
  - Title, Description alanları (Create.cshtml ile aynı)
  - Ek yönetimi bölümü:
    - `@if (Model.ExistingAttachment != null)`: mevcut ek adı + boyutu göster + `<input type="checkbox" asp-for="RemoveAttachment" />` "Remove attachment" checkbox'ı
    - Yeni ek yükleme: `<input asp-for="Attachment" type="file" class="form-control" ... />`
  - Buton grubu:
    - "Save as Draft": `<button type="submit" formaction="@Url.Action("UpdateDraft", new { id = Model.Id })">Save as Draft</button>`
    - "Submit": `<button type="submit" formaction="@Url.Action("SubmitDraft", new { id = Model.Id })" class="btn btn-primary">Submit</button>`
    - "Cancel": `<a asp-action="Index" class="btn btn-outline-secondary">Cancel</a>`
  - `@section Scripts`: `category-form.js` referansı ekle + `initCategoryForm('@Model.Category')` çağrısı

**Checkpoint**: Kayıtlı draft Edit formunda tüm alanlar dolu; değiştirip Save as Draft → alanlar kalıcı; JS section show/hide çalışıyor.

---

## Phase 6: User Story 4 — Submit a Draft (Priority: P3)

**Hedef**: Edit formundaki Submit butonu, tam validation uygular; başarılı olursa fikirler admin kuyruğuna girer.

**Bağımsız Test**: Eksiksiz dolu draft'ı Submit → admin listesinde görün; eksik alanlarla Submit → inline hatalar; admin listesinde draft görünmesin (FR-010).

### US4 Implementasyonu

- [x] T019 [US4] `src/InnovatEPAM.Portal/Controllers/IdeasController.cs` dosyasına `POST SubmitDraft` action ekle:
  - `[HttpPost, ValidateAntiForgeryToken]`
  - `public async Task<IActionResult> SubmitDraft(Guid id, EditDraftViewModel vm)` imzası
  - **Validator çalıştır**: `if (!ModelState.IsValid)` → `vm.Id = id` set et, `ExistingAttachment` ve `Id` kaybolmasın diye servis'ten yeniden doldur, `return View("Edit", vm);`
  - `var userId = Guid.Parse(_userManager.GetUserId(User)!);`
  - `var (success, error) = await _ideaService.SubmitDraftAsync(id, userId, vm);`
  - Hata: Edit view'ına dön + `ModelState.AddModelError`
  - Başarı: `TempData["Success"] = "Idea submitted successfully."; return RedirectToAction("Detail", new { id });`
  - **Not**: `EditDraftValidator` FluentValidation ile otomatik çalışır — `AddFluentValidationAutoValidation()` sayesinde `ModelState` dolacak.

- [x] T020 [US4] `src/InnovatEPAM.Portal/Controllers/AdminController.cs` — `Index` action'ındaki `AvailableStatuses` listesinden "Draft" değerini çıkar:
  - `AvailableStatuses = Enum.GetNames<IdeaStatus>().Where(s => s != "Draft").ToList()` olarak güncelle (SC-004: admin filtre dropdown'unda Draft seçeneği olmamalı)

**Checkpoint**: Submit butonu tam validation çalıştırıyor; eksik alanlar inline hata gösteriyor; başarılı submit admin listesinde görünüyor; admin dropdown'da "Draft" yok.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Amaç**: XML dokümantasyon, badge renkleri, Admin görünüm izolasyonu son doğrulaması, manual test.

- [x] T021 [P] Tüm yeni public class ve metodlara XML dokümantasyon yorumları ekle (constitution Prensibi II):
  - `EditDraftViewModel` tüm property'leri
  - `EditDraftValidator` sınıfı
  - `IIdeaService` — 4 yeni method
  - `IdeaService` — 4 yeni method + `GetAllIdeasAsync` güncellenen davranışına yorum ekle

- [x] T022 [P] `src/InnovatEPAM.Portal/Views/Ideas/Index.cshtml` ve `src/InnovatEPAM.Portal/Views/Admin/Index.cshtml` badge renk switch bloklarının eksiksiz olduğunu doğrula:
  - Submitter Index: `"Draft" => "bg-secondary bg-opacity-50"` case mevcut
  - Admin Index: Draft case gerekli değil (admin listesinde Draft gelmeyecek ama defansif olarak eklenebilir)

- [ ] T023 `quickstart.md` senaryolarını 1–10 ve 6 regresyon testini sırayla çalıştır ve sonuçları kaydet

---

## Bağımlılıklar ve Çalıştırma Sırası

### Phase Bağımlılıkları

```
Phase 1 (T001: Enum)
    ↓
Phase 2 (T002, T003, T004 paralel) → (T005, T006, T007, T008, T009 sıralı — aynı dosya)
    ↓
Phase 3 US1 (T010, T011)
    ↓ (US2, US3 paralel başlayabilir — farklı controller actions + views)
Phase 4 US2 (T012, T013, T014, T015)
Phase 5 US3 (T016, T017, T018)
    ↓ (her iki story tamamlanınca)
Phase 6 US4 (T019, T020)
    ↓
Phase 7 Polish (T021, T022 paralel) → T023
```

### User Story Bağımlılıkları

- **US1 (P1)**: Foundation (Phase 2) tamamlanınca başlayabilir
- **US2 (P2)**: US1 tamamlanınca başlayabilir (Draft'ların listede görünmesi için SaveDraft çalışmalı)
- **US3 (P2)**: US1 ile paralel başlayabilir; US2'deki Detail görünümü Edit sayfasına link veriyor
- **US4 (P3)**: US3 tamamlanınca başlayabilir (Submit butonu Edit.cshtml üzerinde)

### Paralel Fırsatlar

- Phase 2: T002, T003, T004 birlikte çalışabilir (farklı dosyalar)
- Phase 4 + Phase 5: US2 ve US3 farklı developer'larla paralel başlanabilir
- Phase 7: T021, T022 birlikte çalışabilir

---

## Implementation Strategy

### MVP First (US1 — P1)

1. Phase 1 + Phase 2 tamamla
2. Phase 3 (US1) tamamla: Create formuna "Save as Draft" butonu + SaveDraft action
3. **DUR ve DOĞRULA**: quickstart.md Senaryo 1 + 2'yi test et
4. Çalışıyorsa Phase 4 + 5'e geç

### Incremental Delivery

1. Phase 1–2 → Altyapı hazır (build doğrula)
2. Phase 3 → Draft kayıt çalışıyor (Senaryo 1–2)
3. Phase 4 → Liste + silme çalışıyor (Senaryo 3–4)
4. Phase 5 → Edit formu çalışıyor (Senaryo 5–6)
5. Phase 6 → Submit çalışıyor; admin izolasyonu son doğrulama (Senaryo 7–9)
6. Phase 7 → Ownership isolation test (Senaryo 10) + regresyon

---

## Görev Sayısı Özeti

| Phase | Görev Sayısı | Paralel Fırsat |
|---|---|---|
| Phase 1: Setup | 1 | — |
| Phase 2: Foundational | 8 | T002, T003, T004 paralel |
| Phase 3: US1 | 2 | — |
| Phase 4: US2 | 4 | T014 [P] |
| Phase 5: US3 | 3 | — |
| Phase 6: US4 | 2 | — |
| Phase 7: Polish | 3 | T021, T022 paralel |
| **Toplam** | **23** | |

- US1 (P1 — MVP): 2 görev
- US2 (P2): 4 görev
- US3 (P2): 3 görev
- US4 (P3): 2 görev
