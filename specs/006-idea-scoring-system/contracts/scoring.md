# API Contracts: Idea Scoring System

**Feature**: `006-idea-scoring-system`
**Phase**: 1 — Interface Contracts

---

## ScoreController Actions

All actions require `[Authorize(Roles = "Admin")]`.

---

### `POST /Score/Submit`

**Purpose**: Submits or updates the calling admin's score for an idea.

**Form inputs** (`SubmitScoreViewModel`):
| Field | Type | Required | Constraints |
|---|---|---|---|
| `IdeaId` | `Guid` | Yes | Must reference an existing idea |
| `Innovation` | `int?` | No | 1–5 inclusive when provided |
| `TechnicalFeasibility` | `int?` | No | 1–5 inclusive when provided |
| `BusinessImpact` | `int?` | No | 1–5 inclusive when provided |
| `ImplementationValue` | `int?` | No | 1–5 inclusive when provided |

**Business rules**:
- At least one dimension score must be provided.
- Idea must be in `Submitted` or `UnderReview` status. If `Draft`, `Accepted`, or `Rejected` → HTTP 403 / `TempData["Error"]`.
- If the admin already has a score record for this idea, it is updated (upsert). Otherwise, a new record is inserted.
- Anti-forgery token required.

**Success**: `TempData["Success"] = "Your score has been saved."` → `RedirectToAction("Detail", "Admin", new { id = IdeaId })`

**Validation failure**: Returns `Admin/Detail` view with `ModelState` errors displayed inline.

---

### `POST /Score/Retract/{ideaId}`

**Purpose**: Retracts the calling admin's score for the specified idea.

**Route parameter**:
| Parameter | Type | Description |
|---|---|---|
| `ideaId` | `Guid` | The idea whose score is being retracted |

**Business rules**:
- If no score record exists for this admin + idea, the action is a no-op (no error).
- Idea status is not checked for retraction (retracting a score on a concluded idea is allowed for data hygiene).
- Anti-forgery token required.

**Success**: `TempData["Success"] = "Your score has been removed."` → `RedirectToAction("Detail", "Admin", new { id = ideaId })`

---

## AdminController Modifications

### `GET /Admin/Detail/{id}` — additions

The existing action is extended to populate score data:

```csharp
// New injected dependency: IScoreService _scoreService

var isBlindReview = await _blindReviewService.IsEnabledAsync();
var scoreSummary = await _scoreService.GetScoreSummaryAsync(id, isBlindReview);
var myScore = await _scoreService.GetMyScoreAsync(id, adminId);
var isScoringAllowed = idea.Status is "Submitted" or "UnderReview";

// Populate AdminIdeaDetailViewModel:
vm.ScoreSummary = scoreSummary;
vm.ScoreForm = new SubmitScoreViewModel
{
    IdeaId = id,
    Innovation = myScore?.Innovation,
    TechnicalFeasibility = myScore?.TechnicalFeasibility,
    BusinessImpact = myScore?.BusinessImpact,
    ImplementationValue = myScore?.ImplementationValue
};
vm.IsScoringAllowed = isScoringAllowed;
```

### `GET /Admin/Index` — additions

The existing action is extended to populate aggregate scores for the idea list:

```csharp
// After loading ideas list:
var ideaIds = ideas.Select(i => i.Id).ToList();
var aggregates = await _scoreService.GetAggregatesForIdeasAsync(ideaIds);
foreach (var idea in ideas)
{
    if (aggregates.TryGetValue(idea.Id, out var agg))
    {
        idea.AggregateScore = agg.OverallAverage;
        idea.ScorerCount = agg.ScorerCount;
    }
}
```

---

## IScoreService Contract

```csharp
/// <summary>Submits or updates the admin's score. Throws InvalidOperationException if scoring is disallowed for this idea status.</summary>
Task SubmitScoreAsync(Guid ideaId, Guid adminId, SubmitScoreViewModel vm);

/// <summary>Retracts the admin's score. Silent no-op if no score exists.</summary>
Task RetractScoreAsync(Guid ideaId, Guid adminId);

/// <summary>
/// Returns the full score summary including all admin rows.
/// Scorer names masked to "Anonymous Reviewer" when isBlindReviewActive is true.
/// Returns ScoreSummaryDTO with ScorerCount=0 and all null averages when no scores exist.
/// </summary>
Task<ScoreSummaryDTO> GetScoreSummaryAsync(Guid ideaId, bool isBlindReviewActive);

/// <summary>Returns this admin's own score record, or null.</summary>
Task<IdeaScore?> GetMyScoreAsync(Guid ideaId, Guid adminId);

/// <summary>
/// Bulk aggregate fetch for list views.
/// Returns a dictionary keyed by IdeaId; missing keys = 0 scorers.
/// </summary>
Task<Dictionary<Guid, (decimal? OverallAverage, int ScorerCount)>> GetAggregatesForIdeasAsync(IEnumerable<Guid> ideaIds);
```

---

## IdeasController Modifications (Submitter View)

### `GET /Ideas/Detail/{id}` — additions

The submitter-facing detail action is extended to provide the overall aggregate score only:

```csharp
// New injected dependency: IScoreService _scoreService

var scoreSummary = await _scoreService.GetScoreSummaryAsync(id, isBlindReviewActive: false);
// Pass only OverallAverage and ScorerCount to the ViewModel — no admin breakdown.
vm.AggregateScore = scoreSummary.OverallAverage;
vm.ScorerCount = scoreSummary.ScorerCount;
```

`IdeaDetailViewModel` additions:
```csharp
public decimal? AggregateScore { get; set; }
public int ScorerCount { get; set; }
```

The submitter-facing detail view renders a read-only "Evaluation Score" section when `AggregateScore` is not null. No dimension breakdowns or scorer identities are shown (FR-009).

---

## View Rendering Rules

### `Views/Admin/Detail.cshtml` — Score Section

```
[Score Section — rendered below the idea metadata card, above the Review Pipeline]

IF IsScoringAllowed:
  Scoring form (POST → /Score/Submit, anti-forgery)
  Four dimension dropdowns (1–5 + "Not scored")
  Pre-populated from ScoreForm (admin's existing scores)
  [Save Score] button

IF NOT IsScoringAllowed AND idea concluded:
  Read-only badge: "Scoring closed — idea has been decided"

[Score Summary — always rendered when ScoreSummary.ScorerCount > 0]
  Overall average: X.XX / 5 (N reviewers)
  Dimension averages table (per-dimension avg)
  Admin score breakdown table (masked names when blind review active)

IF ScoreSummary.ScorerCount == 0:
  "No scores yet" placeholder
```

### `Views/Admin/Index.cshtml` — Score Column

```
Add "Score" column to the ideas table:
  IF AggregateScore != null → display "X.XX ★ (N)"
  ELSE → display "—"
```

### `Views/Ideas/Detail.cshtml` — Submitter Score Section

```
IF AggregateScore != null:
  "Evaluation Score" section: "X.XX / 5 (rated by N reviewer(s))"
ELSE:
  No score section shown (FR-009 — no "Pending" label per US4 scenario 2 optional)
```
