# Implementation Plan: Idea Scoring System

**Branch**: `006-idea-scoring-system` | **Date**: 2026-05-14 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/006-idea-scoring-system/spec.md`

---

## Summary

Add a dimension-based scoring system for admin reviewers to rate innovation ideas across four fixed evaluation dimensions (Innovation, Technical Feasibility, Business Impact, Implementation Value) on a 1–5 scale. Scores are persisted per-admin-per-idea in a new `IdeaScores` table. Aggregated averages are computed in the service layer and exposed in both the admin idea list and detail views. The feature integrates with the existing multi-stage review workflow (Spec 004) and blind review mode (Spec 005), using the established ASP.NET Core MVC layered architecture.

---

## Technical Context

**Language/Version**: C# 12 / .NET 10.0

**Primary Dependencies**: ASP.NET Core MVC, Entity Framework Core 10, FluentValidation 11, AutoMapper 16, Serilog, Bootstrap 5 / Bootstrap Icons, PostgreSQL 14+

**Storage**: PostgreSQL 14+ via EF Core Code-First. New table: `IdeaScores` (composite PK: `IdeaId` + `AdminId`).

**Testing**: Manual testing per `quickstart.md` (12 scenarios + 7 regression). MVP manual-first per Constitution §XI.

**Target Platform**: Linux/macOS/Windows server — ASP.NET Core MVC

**Performance Goals**: SC-001 — admin scores all 4 dimensions in < 60 seconds. SC-002 — aggregate reflects latest data on page reload.

**Constraints**: No caching layer for scores (real-time recalculation per SC-002). Scoring UI embedded in existing detail page (SC-001, SC-003). Four dimensions fixed — no dynamic configuration.

**Scale/Scope**: Bounded to existing user base. `IdeaScores` table grows at O(ideas × admins) — negligible for MVP scale.

---

## Constitution Check

| Principle | Status | Notes |
|---|---|---|
| I. Clean Architecture | ✅ PASS | New `ScoreController` (thin), `IScoreService` (business logic), `IIdeaScoreRepository` (data access). Admin/Ideas controllers extended minimally. |
| II. ASP.NET Core Conventions | ✅ PASS | New controller in `Controllers/`, services in `Services/`, repos in `Repositories/`, models in `Models/`. |
| III. Service-Layer Logic | ✅ PASS | All aggregate calculation logic in `ScoreService`. Controllers call service methods only. |
| IV. Auth & Authorization | ✅ PASS | `ScoreController` → `[Authorize(Roles = "Admin")]`. Submitter POST rejected (FR-011). |
| V. Phased Development | ✅ PASS | 4 independent user stories mapped to phases in tasks.md. |
| VI. File Upload Security | N/A | No file uploads in this feature. |
| VII. Workflow-Driven Review | ✅ PASS | Scoring integrates with existing stage/status workflow; does not bypass it. |
| VIII. UX Consistency | ✅ PASS | Bootstrap 5 star-display, inline form, consistent alert patterns. |
| IX. Structured Error Handling | ✅ PASS | Invalid status for scoring → `TempData["Error"]`. Validation failures → inline ModelState errors. |
| X. Specification-Driven | ✅ PASS | All 12 FRs mapped to concrete implementation decisions in research.md and data-model.md. |
| XI. Manual Testing | ✅ PASS | 12 test scenarios + 7 regression scenarios in quickstart.md. |

**Constitution Check Result**: ✅ ALL GATES PASS — proceed to implementation.

---

## Project Structure

### Documentation (this feature)

```text
specs/006-idea-scoring-system/
├── plan.md              ← this file
├── research.md          ← Phase 0 output (8 decisions)
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output (12 + 7 scenarios)
├── checklists/
│   └── requirements.md  ← 16/16 PASS
├── contracts/
│   └── scoring.md       ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks)
```

### Source Code (new files + modifications)

```text
src/InnovatEPAM.Portal/
├── Models/
│   └── IdeaScore.cs                              [NEW]
├── DTOs/
│   └── ScoreSummaryDTO.cs                        [NEW]  (ScoreSummaryDTO + AdminScoreRowDTO)
│   └── IdeaListItemDTO.cs                        [MODIFY] (+AggregateScore, +ScorerCount)
│   └── IdeaDetailDTO.cs                          [MODIFY] (+ScoreSummary, +MyScore)
├── ViewModels/
│   └── ScoreViewModels.cs                        [NEW]  (SubmitScoreViewModel)
│   └── IdeaViewModels.cs                         [MODIFY] (AdminIdeaDetailViewModel: +ScoreSummary, +ScoreForm, +IsScoringAllowed)
│                                                          (IdeaDetailViewModel: +AggregateScore, +ScorerCount)
├── Validators/
│   └── SubmitScoreValidator.cs                   [NEW]
├── Repositories/
│   ├── Interfaces/
│   │   └── IIdeaScoreRepository.cs               [NEW]
│   └── IdeaScoreRepository.cs                    [NEW]
├── Services/
│   ├── Interfaces/
│   │   └── IScoreService.cs                      [NEW]
│   └── ScoreService.cs                           [NEW]
├── Controllers/
│   └── ScoreController.cs                        [NEW]
│   └── AdminController.cs                        [MODIFY] (+IScoreService inject, Detail+Index extended)
│   └── IdeasController.cs                        [MODIFY] (+IScoreService inject, Detail extended)
├── Data/
│   └── ApplicationDbContext.cs                   [MODIFY] (+IdeaScores DbSet, Fluent config)
│   └── Migrations/
│       └── <timestamp>_AddIdeaScores.cs          [NEW — generated]
├── Views/
│   ├── Admin/
│   │   └── Detail.cshtml                         [MODIFY] (+score section: form + summary)
│   │   └── Index.cshtml                          [MODIFY] (+Score column in ideas table)
│   └── Ideas/
│       └── Detail.cshtml                         [MODIFY] (+submitter aggregate score section)
└── Program.cs                                    [MODIFY] (+DI registrations)
```

---

## Research Summary

| # | Decision | Choice |
|---|---|---|
| 1 | Score storage | One row per (IdeaId, AdminId); 4 nullable int columns for dimensions |
| 2 | Aggregate calculation | Application layer in `ScoreService` (not DB stored column) |
| 3 | Scoring UI placement | Embedded in Admin Detail page (not separate page) |
| 4 | Blind review integration | Scorer names masked to "Anonymous Reviewer"; aggregates always visible |
| 5 | Score retraction | Hard delete (not soft delete) |
| 6 | Controller placement | New `ScoreController` for mutations; `AdminController` extended for reads |
| 7 | List view score field | `AggregateScore` (decimal?) + `ScorerCount` (int) on `IdeaListItemDTO` only |
| 8 | Validation | FluentValidation `SubmitScoreValidator`; partial scoring allowed; 1–5 range enforced |
