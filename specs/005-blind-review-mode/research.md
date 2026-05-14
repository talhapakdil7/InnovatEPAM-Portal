# Research: Blind Review Mode

**Feature**: `specs/005-blind-review-mode`
**Date**: 2026-05-14

---

## Decision 1: Persistent Storage for the Blind Review Setting

**Decision**: A new `SystemSetting` entity with a string key / string value schema, persisted in a dedicated `SystemSettings` PostgreSQL table.

**Rationale**:
- A single-purpose `BlindReviewSetting` table would work for this feature but creates a proliferation of single-row tables as new settings are added. A generic key-value `SystemSetting` entity is equally simple today and avoids schema churn for every future toggle.
- `appsettings.json` cannot be modified at runtime without a server restart, violating FR-011 (immediate effect on next page load).
- ADR-004 (EF Core Code-First) demands all schema additions go through migrations; the new `SystemSettings` table follows that rule cleanly.

**Alternatives Considered**:
| Alternative | Reason Rejected |
|---|---|
| Single-row `BlindReviewSetting` table | Works, but creates N single-row tables as settings grow |
| `appsettings.json` entry | Cannot be changed at runtime; violates FR-011 |
| Distributed cache (Redis) | Out of scope, over-engineered for MVP |
| In-memory singleton (static field) | Lost on server restart; violates FR-005 |

---

## Decision 2: Masking Layer Placement

**Decision**: `IBlindReviewService` exposes two stateless masking methods (`ApplyMasking(IdeaDetailDTO)` and `ApplyMasking(IEnumerable<IdeaListItemDTO>)`) that are called from `AdminController` action methods after the `IIdeaService` call returns. The controller passes the result through the masking pipeline before building the ViewModel.

**Rationale**:
- Constitution Principle III: "No business logic in controllers, views, or data access layer." Masking logic lives in the service, not the controller. Controllers only call the service and pass results to views.
- Keeping `IIdeaService` methods pure (returning real data) preserves their existing tested behavior and avoids coupling idea retrieval with access-control presentation logic.
- AutoMapper `AfterMap` hooks would couple the mapping profile to a runtime service dependency, making it harder to test and reason about. Mapping profiles should be pure transformations.
- A dedicated `IBlindReviewService` with masking methods is a single-responsibility service (Constitution Principle III), testable in isolation, and trivially injectable anywhere a blind review check is needed.

**Alternatives Considered**:
| Alternative | Reason Rejected |
|---|---|
| Modify `IdeaService.GetIdeaDetailAsync` to accept a `bool isBlindReview` parameter | Bleeds blind-review concern into the idea service; violates single responsibility |
| AutoMapper `AfterMap` hook injecting `IBlindReviewService` | Profile becomes service-dependent; harder to test; violates mapping-as-pure-transform principle |
| Razor view conditional rendering (`@if (blindReview)`) | Business logic in views; violates Constitution Principle III |

---

## Decision 3: Settings Page Location

**Decision**: A new dedicated `SettingsController` under the Admin route (`/Admin/Settings` or `/Settings`) handles the settings page GET and POST.

**Rationale**:
- Constitution Principle II: "Project structure mirrors responsibility." A settings page managing cross-feature toggles logically belongs to a `SettingsController`, not to `AdminController` (which handles idea management actions). Separate controllers = separate responsibilities.
- `AdminController` is already growing (idea list, detail, stage actions). Keeping settings separate prevents a bloated controller.
- `[Authorize(Roles = "Admin")]` applied at the class level; no change to the authorization model.

**Alternatives Considered**:
| Alternative | Reason Rejected |
|---|---|
| Add settings actions to `AdminController` | Controller grows beyond single responsibility; mixes idea management and system configuration |
| Home/shared controller | Wrong semantic; settings are admin-only |

---

## Decision 4: Masking Scope — Which Fields to Mask

**Decision**: In all admin-facing views, replace only `SubmitterName` in `IdeaDetailDTO` and `IdeaListItemDTO` with the constant `"Anonymous Submitter"`. Email and department are not currently persisted in the DTO surface (the `ApplicationUser` model has `Email` but it is not projected into any idea DTO). Therefore, masking `SubmitterName` is sufficient to satisfy FR-001 and FR-002 for the current data model.

**Rationale**:
- The existing `IdeaDetailDTO` and `IdeaListItemDTO` only expose `SubmitterName` (resolved from `ApplicationUser.FullName`). Email and department are never returned in idea-related DTOs today.
- Future phases that expose email/department will need to revisit this service.

**Alternatives Considered**:
| Alternative | Reason Rejected |
|---|---|
| Mask additional fields not in current DTOs | Over-engineering fields that do not exist in the current surface |
| Mask in the database layer (computed column) | Violates FR-010 (data must not be altered); DB masking is permanent |

---

## Decision 5: Identity-Reveal Trigger

**Decision**: Identity is revealed when `idea.Status` is `"Accepted"` or `"Rejected"` (i.e., `IdeaStatus.Accepted` or `IdeaStatus.Rejected`). The `FinalDecision` review *stage* alone does not trigger reveal.

**Rationale**:
- An idea can be at the Final Decision review stage for an extended period while the review committee deliberates. Revealing identity at that point would defeat the purpose of blind review during the evaluation.
- Only after `RecordDecision` is called (which sets status to Accepted/Rejected) does the evaluation conclude, making identity reveal appropriate.
- This matches Spec assumption: "Identity is revealed when an idea's `IdeaStatus` is `Accepted` or `Rejected`."

---

## Decision 6: Blind Review Status in ViewModels

**Decision**: `AdminController` actions read the blind review setting once per request (via `IBlindReviewService.IsEnabledAsync()`) and pass a `bool IsBlindReviewActive` property on the ViewModel. Views use this flag only to show a contextual banner ("Blind review is active — submitter identities are hidden"). The actual masking has already been applied to the DTO by the service before the ViewModel is populated.

**Rationale**:
- The view should not re-derive whether masking is active by re-reading a setting; the DTO already contains the masked/unmasked name.
- A banner informs the admin why names appear anonymised, improving UX clarity (Constitution Principle VIII).
