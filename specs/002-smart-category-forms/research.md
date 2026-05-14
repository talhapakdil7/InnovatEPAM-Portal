# Research: Smart Category-Adaptive Submission Forms

**Feature**: `specs/002-smart-category-forms/spec.md`
**Date**: 2026-05-14
**Phase**: 0 — Technical Investigation

---

## Decision 1: Category Data Storage Strategy

**What was chosen**: JSON string column (`CategoryData`) on the existing `Ideas` table, plus a nullable `Category` string column.

**Rationale**:
- Spec explicitly states: "Category and category-specific data are stored within the existing Idea record structure; no separate database table is required for MVP (JSON/string storage is acceptable)."
- `System.Text.Json.JsonSerializer` (built into .NET) serializes/deserializes `Dictionary<string, string>` to/from the column value.
- PostgreSQL stores the column as `text`; EF Core treats it as a regular string property. No special JSONB configuration needed for MVP.
- `Category` column stores the category key (`TechnicalImprovement`, `ProcessImprovement`, `ClientSolution`) or `null` for legacy ideas.
- `CategoryData` stores a JSON object of field-name → field-value pairs, or `null` when no category selected.

**Alternatives considered**:

| Alternative | Reason Rejected |
|---|---|
| Separate `IdeaCategoryData` table with FK | Over-engineering for MVP; spec explicitly forbids it. Adds join queries for every idea read. |
| EF Core owned entities / value objects | Requires EF Core JSON column feature (`.ToJson()`); introduces EF Core version dependency and migration complexity. Not needed for simple key-value storage. |
| Individual nullable columns per field (9 fields) | Schema bloat; not extensible when new categories are added in Phase 2. |
| PostgreSQL `jsonb` column type | Adds ORM mapping complexity with no query benefit at MVP scale; `text` with app-layer serialization is simpler. |

---

## Decision 2: Dynamic Form Adaptation Approach

**What was chosen**: Vanilla JavaScript (no framework, no library) with all category-specific field sections pre-rendered in the Razor View and toggled via `classList.add/remove('d-none')`.

**Rationale**:
- Spec states: "Dynamic field display/hide behavior is achieved client-side (JavaScript) without server round-trips; category-specific fields are rendered in the page but shown/hidden based on the selection."
- Bootstrap 5 `d-none` utility class is already in the project; no new dependencies needed.
- All three category sections are rendered as hidden `<div>` blocks. On category dropdown `change` event, the relevant section is shown and others hidden.
- Field values are cleared on category change via JavaScript to satisfy FR-004.
- Validation error messages (rendered server-side via FluentValidation + ASP.NET Tag Helpers) remain visible once shown.

**Alternatives considered**:

| Alternative | Reason Rejected |
|---|---|
| HTMX partial view swap | Adds HTMX dependency; introduces server round-trip on category change (violates spec requirement). |
| jQuery show/hide | Unnecessary dependency when vanilla `classList` achieves the same; project currently has no jQuery dependency. |
| Separate page per category | Destroys UX continuity; spec explicitly states "no separate new page." |
| React/Vue component | Massive dependency for a single-page interaction; violates project's MVC monolith architecture. |

---

## Decision 3: Category Definition Architecture

**What was chosen**: Static `CategoryDefinitions` class in `src/InnovatEPAM.Portal/Models/` containing typed field metadata objects.

**Rationale**:
- Spec states categories are "defined in code/configuration for the MVP; no admin UI for category management is in scope."
- A static class with `IReadOnlyDictionary<string, CategoryDefinition>` is type-safe, testable, and inspectable without DB queries.
- Both the validator and the service can reference the same static definitions to avoid duplication.
- JavaScript form behavior reads category keys from the HTML (via `data-category` attributes rendered by Razor) — no separate JSON API needed.

**Structure**:
```
CategoryDefinitions (static class)
  ├── All: IReadOnlyDictionary<string, CategoryDefinition>
  ├── TechnicalImprovement: CategoryDefinition
  ├── ProcessImprovement: CategoryDefinition
  └── ClientSolution: CategoryDefinition

CategoryDefinition
  ├── Key: string          ("TechnicalImprovement")
  ├── DisplayName: string  ("Technical Improvement")
  └── Fields: List<CategoryFieldDefinition>

CategoryFieldDefinition
  ├── Key: string          ("TechArea")
  ├── Label: string        ("Technology Area")
  ├── InputType: string    ("select" | "text" | "textarea")
  ├── Options: List<string> (for selects)
  ├── IsRequired: bool
  ├── MaxLength: int
  └── GuidanceHint: string
```

**Alternatives considered**:

| Alternative | Reason Rejected |
|---|---|
| JSON config file (`appsettings.json`) | Parsed at runtime, not type-safe; harder to reference from validators. |
| Database table (`Categories`, `CategoryFields`) | No admin UI in scope; adds migration complexity for static data. |
| Enum + switch statements spread across codebase | Duplication; no single source of truth for field metadata. |

---

## Decision 4: FluentValidation Conditional Category Rules

**What was chosen**: FluentValidation `When()` conditions keyed on `CreateIdeaViewModel.Category`.

**Rationale**:
- FluentValidation `When(x => x.Category == "TechnicalImprovement", () => { RuleFor(...) })` applies rules only when the relevant category is selected.
- This keeps all validation in `CreateIdeaValidator`, consistent with the existing pattern.
- The ViewModel carries ALL possible category fields as nullable strings, allowing model binding to capture any field regardless of selection.
- Server-side validation catches manipulation attempts (submitting category fields not matching selected category).

**Pattern**:
```csharp
RuleFor(x => x.Category)
    .NotEmpty().WithMessage("Please select a category.");

When(x => x.Category == "TechnicalImprovement", () => {
    RuleFor(x => x.TechArea).NotEmpty()...;
    RuleFor(x => x.TechEffort).NotEmpty()...;
    RuleFor(x => x.TechBenefit).NotEmpty().MaximumLength(500)...;
});
// Repeat for ProcessImprovement, ClientSolution
```

**Alternatives considered**:

| Alternative | Reason Rejected |
|---|---|
| Separate validator per category | Requires factory pattern in controller; more complexity than `When()`. |
| Data annotation `[Required]` on ViewModel | Cannot be conditional; would require all fields required always. |
| Custom `IValidationFilter` middleware | Over-engineering; FluentValidation `When()` is the idiomatic solution. |

---

## Decision 5: Backward Compatibility for Legacy Ideas

**What was chosen**: Nullable `Category` column; display as "Uncategorized" when null. No data migration.

**Rationale**:
- Spec states: "Ideas submitted before this feature existed MUST display as 'Uncategorized' without errors."
- `Category` and `CategoryData` columns are nullable. EF Core migration adds them as `ALTER TABLE ... ADD COLUMN ... NULL`.
- AutoMapper: `Category ?? "Uncategorized"` in the mapping expression (or in the DTO display logic in Razor views).
- Existing tests and workflows are not impacted; the new columns have no effect when null.

---

## Decision 6: Admin Category Filter Implementation

**What was chosen**: In-memory filtering on `IdeaListItemDTO.Category` after DB query, consistent with existing `statusFilter` pattern.

**Rationale**:
- Existing `GetAllIdeasAsync` already filters in memory after loading from DB: `ideas.Where(i => i.Status == status)`.
- At MVP scale, in-memory filtering is acceptable; at larger scale (Phase 2), DB-level filtering can be added via an EF Core LINQ `Where` clause.
- Keeps service method signatures consistent: `GetAllIdeasAsync(string? statusFilter, string? categoryFilter = null)`.

---

## All NEEDS CLARIFICATION Items: Resolved

No NEEDS CLARIFICATION markers were present in spec.md. All technical decisions above are self-contained within the existing project architecture.

---

**Version**: 1.0.0 | **Created**: 2026-05-14
