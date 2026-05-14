# Implementation Plan: Blind Review Mode

**Feature**: `specs/005-blind-review-mode`
**Date**: 2026-05-14
**Branch**: `005-blind-review-mode`

---

## Technical Context

**Stack**: C# 12 / .NET 10.0 · ASP.NET Core MVC · EF Core 10 · PostgreSQL 14+ · Bootstrap 5

**Existing Architecture**:
- `AdminController` retrieves ideas via `IIdeaService` and renders them in Razor Views.
- `IIdeaService.GetAllIdeasAsync` / `GetIdeaDetailAsync` return plain DTOs with real submitter identity.
- `AutoMapperProfile` maps `Idea → IdeaDetailDTO` / `IdeaListItemDTO`.
- `Program.cs` registers all services and repositories via DI.

**User Guidance**: Extend the existing review workflow to support conditional anonymous evaluation views without changing the underlying data model relationships. Implement blind review behavior through service-layer logic and UI rendering while preserving compatibility with existing evaluation stages and status workflows.

---

## Constitution Check

| Principle | Assessment |
|---|---|
| I. Clean Architecture | ✅ Masking logic in `BlindReviewService`, not controllers or views |
| II. Maintainable MVC | ✅ New `SettingsController` + `Views/Settings/` follow naming conventions |
| III. Service-Layer Logic | ✅ `IBlindReviewService` owns all masking and setting business rules |
| IV. Role-Based Authorization | ✅ `SettingsController` decorated with `[Authorize(Roles = "Admin")]` |
| V. Phased Feature Development | ✅ Single migration (`AddSystemSettings`), no breaking schema changes |
| VIII. Responsive UX | ✅ Info banner on admin pages; settings form is a simple Bootstrap toggle |
| IX. Structured Error Handling | ✅ Service logs setting changes with structured Serilog entries |
| X. Specification-Driven | ✅ All decisions traced to spec.md FR/SC items |

**Constitution Gate**: PASS — no violations.

---

## Project Structure Changes

### New Files

| File | Purpose |
|---|---|
| `Models/SystemSetting.cs` | New entity; key-value system-wide settings store |
| `Models/SystemSettingKeys.cs` | Constants for well-known setting keys |
| `Repositories/Interfaces/ISystemSettingRepository.cs` | GetByKey + Upsert interface |
| `Repositories/SystemSettingRepository.cs` | EF Core implementation |
| `Services/Interfaces/IBlindReviewService.cs` | Interface: IsEnabled, SetEnabled, ApplyMasking, ShouldRevealIdentity |
| `Services/BlindReviewService.cs` | Concrete implementation |
| `ViewModels/SettingsViewModels.cs` | `BlindReviewSettingsViewModel` |
| `Controllers/SettingsController.cs` | GET + POST for `/Settings/BlindReview` |
| `Views/Settings/BlindReview.cshtml` | Settings page with toggle form |

### Modified Files

| File | Change |
|---|---|
| `Data/ApplicationDbContext.cs` | Add `SystemSettings` DbSet + Fluent config + seed |
| `ViewModels/IdeaViewModels.cs` | Add `IsBlindReviewActive` to `AdminIdeaListViewModel` + `AdminIdeaDetailViewModel` |
| `Controllers/AdminController.cs` | Inject `IBlindReviewService`; call `ApplyMasking` in `Index` + `Detail`; pass `IsBlindReviewActive` in VMs |
| `Program.cs` | Register `ISystemSettingRepository` + `IBlindReviewService` |
| `Views/Admin/Index.cshtml` | Show info banner when `Model.IsBlindReviewActive` |
| `Views/Admin/Detail.cshtml` | Show info banner when `Model.IsBlindReviewActive` |
| `Views/Admin/ByStage.cshtml` | Inject blind review masking (via controller action update) |
| `Views/Shared/_Layout.cshtml` | Add "Settings" nav link for Admin role |
| `Mapping/AutoMapperProfile.cs` | No changes needed — masking is post-mapping, not in mapper |

### New Migration

`dotnet ef migrations add AddSystemSettings`

---

## Research Summary

| Decision | Choice |
|---|---|
| Setting storage | `SystemSetting` key-value table — extensible, runtime-changeable, EF-managed |
| Masking layer | `IBlindReviewService.ApplyMasking()` called in `AdminController` after DTO retrieval |
| Settings page location | New `SettingsController` — keeps `AdminController` focused on idea management |
| Masking scope | `SubmitterName` field only (the only submitter identity exposed in current DTOs) |
| Identity-reveal trigger | `IdeaStatus.Accepted` or `IdeaStatus.Rejected` — not the FinalDecision stage |
| Admin UX | Info banner on list and detail pages when blind review is active |
