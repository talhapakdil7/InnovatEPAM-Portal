# API Contracts: Blind Review Mode Settings

**Feature**: `specs/005-blind-review-mode`
**Date**: 2026-05-14

---

## SettingsController Actions

### GET /Settings/BlindReview

Returns the blind review settings page.

**Authorization**: `[Authorize(Roles = "Admin")]`

**Response**: Renders `Views/Settings/BlindReview.cshtml` with `BlindReviewSettingsViewModel`.

**ViewModel populated by**:
```csharp
var isEnabled = await _blindReviewService.IsEnabledAsync();
var setting = await _settingRepo.GetByKeyAsync(SystemSettingKeys.BlindReviewEnabled);
var vm = new BlindReviewSettingsViewModel
{
    IsEnabled = isEnabled,
    LastModifiedDate = setting?.LastModifiedDate,
    LastModifiedByAdminName = setting?.LastModifiedByAdmin?.FullName
};
```

---

### POST /Settings/BlindReview

Toggles blind review mode on or off.

**Authorization**: `[Authorize(Roles = "Admin")]`
**Anti-Forgery**: `[ValidateAntiForgeryToken]`

**Form Fields**:
| Field | Type | Required | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | Yes | New state: true = enable, false = disable |

**Success Flow**:
1. Call `await _blindReviewService.SetEnabledAsync(vm.IsEnabled, adminId)`
2. Set `TempData["Success"]` = appropriate confirmation message
3. Redirect to GET `/Settings/BlindReview`

**Error Flow**:
- If `ModelState` is invalid → re-render the view with validation errors.
- If service returns an error → set `TempData["Error"]` and redirect.

**Validation Rules** (BlindReviewSettingsValidator):
- `IsEnabled` — no validation needed (bool is always valid); no FluentValidation required.

---

## IBlindReviewService Contract

### `Task<bool> IsEnabledAsync()`

- Reads `SystemSettings` row where `Key = "BlindReviewEnabled"`.
- Returns `true` if value is `"true"` (case-insensitive), `false` otherwise.
- If the row does not exist (never seeded), returns `false`.

### `Task SetEnabledAsync(bool enabled, Guid adminId)`

- Upserts `SystemSettings` row with `Key = "BlindReviewEnabled"`.
- Sets `Value = enabled ? "true" : "false"`.
- Sets `LastModifiedDate = DateTime.UtcNow`.
- Sets `LastModifiedByAdminId = adminId`.
- Logs: `"Blind review mode {State} by admin {AdminId}"` (structured log).

### `void ApplyMasking(IdeaDetailDTO dto, bool isBlindReviewEnabled)`

**Masking Rule**:
```
mask = isBlindReviewEnabled && !ShouldRevealIdentity(dto.Status)
if mask:
    dto.SubmitterName = "Anonymous Submitter"
```

**Side effects**: Mutates `dto.SubmitterName` in-place. No other fields are changed.

### `void ApplyMasking(IEnumerable<IdeaListItemDTO> dtos, bool isBlindReviewEnabled)`

Applies the same masking rule to each item:
```
foreach dto in dtos:
    mask = isBlindReviewEnabled && !ShouldRevealIdentity(dto.Status)
    if mask:
        dto.SubmitterName = "Anonymous Submitter"
```

### `bool ShouldRevealIdentity(string ideaStatus)`

Returns `true` when `ideaStatus` is `"Accepted"` or `"Rejected"` (case-sensitive, matches `IdeaStatus.ToString()`).
Returns `false` for all other statuses.

---

## AdminController Modifications

### Index action update

```csharp
public async Task<IActionResult> Index(string? statusFilter, string? categoryFilter)
{
    var ideas = await _ideaService.GetAllIdeasAsync(statusFilter, categoryFilter);
    var isBlindReview = await _blindReviewService.IsEnabledAsync();
    _blindReviewService.ApplyMasking(ideas, isBlindReview);

    var vm = new AdminIdeaListViewModel
    {
        // ... existing properties ...
        IsBlindReviewActive = isBlindReview
    };
    return View(vm);
}
```

### Detail action update

```csharp
public async Task<IActionResult> Detail(Guid id)
{
    var adminId = Guid.Parse(_userManager.GetUserId(User)!);
    var idea = await _ideaService.GetIdeaDetailAsync(id, adminId, isAdmin: true);
    if (idea == null) return NotFound();

    var isBlindReview = await _blindReviewService.IsEnabledAsync();
    _blindReviewService.ApplyMasking(idea, isBlindReview);

    var allowedStatuses = Enum.GetNames<IdeaStatus>().Where(s => s != "Draft").ToList();
    return View(new AdminIdeaDetailViewModel
    {
        Idea = idea,
        AllowedStatuses = allowedStatuses,
        IsBlindReviewActive = isBlindReview
    });
}
```
