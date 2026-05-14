# Implementation Plan: Smart Category-Adaptive Submission Forms

**Branch**: `002-smart-category-forms` | **Date**: 2026-05-14 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-smart-category-forms/spec.md`

**Note**: This plan extends the architecture established in `specs/001-innovation-ideas/plan.md`. All existing patterns (Clean Architecture, FluentValidation, AutoMapper, EF Core migrations) are reused without modification.

## Summary

Extends the existing ASP.NET Core MVC idea submission workflow with dynamic category-based forms. When a Submitter selects an innovation category (Technical Improvement, Process Improvement, Client Solution), the submission form adapts client-side (vanilla JavaScript, no page reload) to display category-specific fields and validation guidance. Category and category-specific field values are persisted as JSON within the existing `Ideas` table. Admins gain a category column in the ideas list and a category filter. Both Submitters and Admins see category information on idea detail pages.

## Technical Context

**Language/Version**: C# 12 / .NET 10 (upgraded from .NET 8 in plan 001)

**Primary Dependencies**:
- ASP.NET Core MVC (existing)
- Entity Framework Core + PostgreSQL (existing, new migration required)
- ASP.NET Core Identity (existing, unchanged)
- AutoMapper (existing, new mappings required)
- FluentValidation (existing, new `When()` rules required)
- Serilog (existing, unchanged)
- Bootstrap 5 `d-none` utility (existing, used for show/hide)
- Vanilla JavaScript (no new JS library; `classList.toggle` for form adaptation)
- `System.Text.Json` (built-in .NET; used for category data serialization)

**Storage**: PostgreSQL — two nullable columns added to `Ideas` table: `Category` (varchar 50) and `CategoryData` (text/JSON). No new tables.

**Testing**: Manual testing per `quickstart.md` (10 scenarios). No new unit test files required for MVP; conditional FluentValidation rules are integration-tested via the manual test matrix.

**Target Platform**: Web application (desktop + mobile-responsive down to 360 px, Bootstrap 5 responsive grid)

**Performance Goals**:
- Category field adaptation: < 1 second (client-side JS only, no server call) — SC-001
- Full submission flow: < 5 minutes from form open to confirmation — SC-002

**Constraints**:
- No server round-trip on category selection (client-side show/hide only)
- Submitter cannot change category after submission (consistent with no-edit-after-submit rule)
- Category definitions are code-only for MVP (no admin UI for category management)
- Legacy ideas (null Category) display as "Uncategorized" with no errors
- File upload rules unchanged

**Scale/Scope**:
- 4 user stories (2 P1, 2 P2)
- 2 new columns on existing `Ideas` table
- 1 new static class (`CategoryDefinitions`)
- 1 new JavaScript file (`category-form.js`)
- ~15 modified files across Models, DTOs, ViewModels, Validators, Services, Controllers, Views
- ~500–700 lines of new/modified business logic code

## Constitution Check

### Pre-Phase 0 Gates

**Gate 1: Clean Architecture Alignment** ✅ PASS
- Category selection logic resides in `CreateIdeaViewModel` (ViewModel) and `CreateIdeaValidator` (Validator)
- Category persistence and data serialization in `IdeaService` (Service layer)
- Category filter passed through `IdeasController`/`AdminController` (thin controllers) to `IdeaService`
- No business logic in Views or Controllers

**Gate 2: ASP.NET Core MVC Conventions** ✅ PASS
- New `CategoryDefinitions` class in `Models/` (model-layer, no DB mapping)
- New `category-form.js` in `wwwroot/js/` (static assets)
- All new ViewModel fields follow PascalCase naming
- Folder structure unchanged and consistent

**Gate 3: Security Requirements** ✅ PASS
- Category value validated server-side (FluentValidation); client-side JS is UX-only
- Category field keys come from static `CategoryDefinitions` — no user-controlled key names stored
- Injection risk mitigated: category data stored as plain text JSON, rendered with `@Html.Encode()` in Razor
- No new authentication or authorization changes required

**Gate 4: Testing Strategy** ✅ CONDITIONAL PASS
- Manual testing covers all 10 quickstart scenarios including regression
- FluentValidation conditional rules tested via end-to-end form submission scenarios
- Full unit test coverage for `CreateIdeaValidator` conditional rules deferred to Phase 2

**Gate 5: Specification-Driven Development** ✅ PASS
- All 14 Functional Requirements (FR-001 to FR-014) mapped to concrete implementation tasks
- No features implemented beyond spec scope

**Gate 6: Documentation Standards** ✅ PASS
- `CategoryDefinitions` class and all public methods will carry XML comments
- `research.md`, `data-model.md`, `contracts/`, `quickstart.md` all generated

### Overall Constitution Gate Status

**✅ ALL GATES PASS**

## Project Structure

### Documentation (this feature)

```text
specs/002-smart-category-forms/
├── plan.md              ✅ This file
├── research.md          ✅ Phase 0 — technical decisions
├── data-model.md        ✅ Phase 1 — entity & ViewModel design
├── quickstart.md        ✅ Phase 1 — 10 manual test scenarios
├── contracts/
│   └── ideas.md         ✅ Phase 1 — updated Ideas contract
├── checklists/
│   └── requirements.md  ✅ Spec validation (all passed)
└── tasks.md             ⏳ Phase 2 — generated by /speckit.tasks
```

### Source Code — Files to Create or Modify

```text
src/InnovatEPAM.Portal/

MODIFY:
├── Models/
│   └── Idea.cs                              # + Category, CategoryData properties

CREATE:
├── Models/
│   └── CategoryDefinitions.cs               # Static category + field definitions

MODIFY:
├── DTOs/
│   ├── IdeaListItemDTO.cs                   # + Category, CategoryDisplayName
│   └── IdeaDetailDTO.cs                     # + Category, CategoryDisplayName, CategoryDataFields

MODIFY:
├── ViewModels/
│   └── IdeaViewModels.cs                    # + category fields in CreateIdeaViewModel
│                                            # + CategoryFilter, AvailableCategories in list VMs

MODIFY:
├── Validators/
│   └── CreateIdeaValidator.cs               # + Category required + When() per-category rules

MODIFY:
├── Services/
│   ├── Interfaces/
│   │   └── IIdeaService.cs                  # + categoryFilter param to GetMyIdeasAsync, GetAllIdeasAsync
│   └── IdeaService.cs                       # + category data persistence, filter, DTO mapping

MODIFY:
├── Controllers/
│   ├── IdeasController.cs                   # + categoryFilter param; pass to service
│   └── AdminController.cs                   # + categoryFilter param; pass to service

MODIFY:
├── Mapping/
│   └── AutoMapperProfile.cs                 # + Category, CategoryDisplayName mapping; CategoryData JSON AfterMap

MODIFY:
├── Data/
│   ├── ApplicationDbContext.cs              # + Category, CategoryData EF config + index
│   └── Migrations/                          # NEW migration: AddIdeaCategoryFields

MODIFY:
├── Views/
│   ├── Ideas/
│   │   ├── Create.cshtml                    # + Category dropdown + 3 hidden category sections + JS
│   │   ├── Index.cshtml                     # + category badge + category filter dropdown
│   │   └── Detail.cshtml                    # + CategoryDisplayName badge + CategoryDataFields section
│   └── Admin/
│       ├── Index.cshtml                     # + category column + category filter dropdown
│       └── Detail.cshtml                    # + CategoryDisplayName badge + CategoryDataFields section

CREATE:
└── wwwroot/
    └── js/
        └── category-form.js                 # Vanilla JS: category show/hide + field clear on switch
```

## Phase 0: Research & Technical Investigation

All unknowns resolved — see `research.md` for full decision log.

**Key decisions**:
1. **Storage**: JSON string column on `Ideas` table (no new table)
2. **Dynamic UI**: Vanilla JS with pre-rendered hidden sections (`d-none`)
3. **Category definitions**: Static `CategoryDefinitions` class in `Models/`
4. **Validation**: FluentValidation `When()` conditions in `CreateIdeaValidator`
5. **Backward compat**: Nullable columns + "Uncategorized" display fallback
6. **Admin filter**: In-memory `Where()` on `IdeaListItemDTO.Category`

## Phase 1: Design & Contracts

All Phase 1 artifacts generated:

1. ✅ `data-model.md` — entity changes, ViewModel design, DTO extensions, validation rules
2. ✅ `contracts/ideas.md` — updated Ideas contract with all category endpoints and validation matrix
3. ✅ `quickstart.md` — 10 manual test scenarios covering all 4 user stories + regression

## Phase 2: Task Generation (Next Step)

Run `/speckit.tasks` to generate `tasks.md` with:

- **Phase 1**: Setup — EF migration, CategoryDefinitions class, JS file scaffold
- **Phase 2**: Foundational — ViewModel, DTO, validator updates (parallel-eligible)
- **Phase 3**: US1 + US2 (P1) — Create form dynamic behavior + validation
- **Phase 4**: US3 (P2) — Detail page category display for Submitter and Admin
- **Phase 5**: US4 (P2) — Admin category filter
- **Phase 6**: Polish — regression testing, backward compat verification

## Governance & Next Steps

**Current Status**: ✅ Phase 0 & 1 Design Complete

**Artifacts Generated**:
1. ✅ plan.md (this file)
2. ✅ research.md
3. ✅ data-model.md
4. ✅ contracts/ideas.md
5. ✅ quickstart.md

**Next Steps**:
1. Run `/speckit.tasks` to generate the actionable task list
2. Run `/speckit.implement` to execute tasks phase-by-phase

**Constitution Compliance**: ✅ All 11 principles addressed

---

**Version**: 1.0.0 | **Created**: 2026-05-14 | **Status**: Ready for /speckit.tasks
