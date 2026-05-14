# Research: Idea Draft Management

**Phase**: 0 — Technical Decisions
**Feature**: `specs/003-draft-management/spec.md`
**Date**: 2026-05-14

---

## Decision 1: Draft Storage Strategy — Extend IdeaStatus vs. Separate Table

**Decision**: Extend the existing `IdeaStatus` enum with a `Draft = 0` value. No separate table or entity.

**Rationale**:
- A draft is semantically identical to a submitted idea except for its lifecycle state. All fields (title, description, category, attachments) are identical.
- Reusing the `Idea` entity and adding `Draft = 0` before `Submitted = 1` means the entire existing data access, repository, and service infrastructure applies without duplication.
- Value `0` is currently unused in the database (all existing records have Status ∈ {1, 2, 3, 4}), so adding it is non-breaking.
- Migrations are minimal: only the IdeaStatus enum interpretation changes — no new columns.

**Alternatives considered**:
- **Separate `DraftIdea` table**: Avoids polluting the Idea table with incomplete records, but duplicates ~12 columns and requires a promotion/copy step on submit. Rejected — unnecessary complexity for MVP.
- **LocalStorage / browser-side draft**: Doesn't survive device or browser changes. Rejected — spec requires cross-session persistence.

---

## Decision 2: Edit Draft ViewModel Design

**Decision**: Introduce a dedicated `EditDraftViewModel` that inherits the same category-field properties as `CreateIdeaViewModel`, plus `Id` (Guid), `ExistingAttachment` (display-only), and `RemoveAttachment` (bool checkbox).

**Rationale**:
- Create and Edit scenarios have different concerns: Edit needs the draft's `Id` for routing, must display the existing attachment with a removal option, and must not accept a new `IdeaStatus` from the form.
- Sharing `CreateIdeaViewModel` with an optional `Id?` field would conflate form semantics and complicate the validator.
- A dedicated ViewModel keeps each form's binding surface minimal and explicit.

**Alternatives considered**:
- **Reuse `CreateIdeaViewModel` with optional Id**: Simpler, fewer files. Rejected — mixes create/edit concerns in one ViewModel and complicates attachment display logic.

---

## Decision 3: Save-as-Draft Form Submission (Bypassing Required-Field Validation)

**Decision**: Use a dedicated `POST /Ideas/SaveDraft` action (for new drafts from the Create form) and `POST /Ideas/UpdateDraft/{id}` action (for existing draft edits). These actions explicitly skip `ModelState.IsValid` checks, relying only on anti-forgery token verification.

**Rationale**:
- FR-001 requires saving partial data without triggering required-field validation. FluentValidation runs on every POST that checks `ModelState.IsValid`.
- Separate action methods (`SaveDraft`, `UpdateDraft`) allow clear intent at the HTTP layer — no hidden form fields or action discriminators needed.
- The `Create.cshtml` form gets a second "Save as Draft" button with `formaction="/Ideas/SaveDraft"`. The existing "Submit Idea" button continues posting to `/Ideas/Create` with full validation.
- The `Edit.cshtml` draft form similarly has two buttons posting to `UpdateDraft/{id}` (no validation) and `SubmitDraft/{id}` (full validation).

**Alternatives considered**:
- **Single action with `saveDraft=true` hidden field**: One action, conditional validation. Rejected — mixing validation logic in one action reduces clarity; harder to test in isolation.
- **ModelState.Clear() in the Create action**: Would work but is fragile and breaks the intent of the action. Rejected.

---

## Decision 4: Admin Isolation of Drafts

**Decision**: In `IdeaService.GetAllIdeasAsync`, always prepend a `.Where(i => i.Status != IdeaStatus.Draft)` filter before applying the caller's `statusFilter`. The admin-facing `IdeaStatus` filter dropdown never includes "Draft".

**Rationale**:
- FR-010 is absolute: drafts are never visible to admins. Enforcing this at the service layer (not controller or view) follows Principle I (Clean Architecture) — business rules live in services.
- Adding a permanent base filter in `GetAllIdeasAsync` is the single change that satisfies SC-004 across all admin entry points (current and future).
- The admin's `AvailableStatuses` list in `AdminIdeaListViewModel` is populated from `Enum.GetNames<IdeaStatus>().Where(s => s != "Draft")` to also exclude Draft from the dropdown UI.

**Alternatives considered**:
- **AdminController filter**: Controller-level filter. Rejected — business rules must not live in controllers (Principle I).
- **Separate repository method `GetAllSubmittedAsync`**: Cleaner naming but requires interface change and duplicate query logic. Deferred to future refactor.

---

## Decision 5: Attachment Management During Draft Edit

**Decision**: The Edit Draft form displays the current attachment (filename + size) with a "Remove attachment" checkbox. A new file upload replaces the existing attachment. Service-layer `UpdateDraftAsync` handles file deletion, replacement, and new-file validation in a single transaction.

**Rationale**:
- FR-007 requires add/remove/replace operations on attachments during draft editing.
- Displaying the existing attachment in the edit form satisfies SC-006 (attachments accessible when draft is reopened).
- The MIME check and 10MB size limit from FileStorageHelper still apply when a new file is uploaded, maintaining Principle VI (Secure File Upload Validation).
- "Remove attachment" checkbox with explicit confirmation prevents accidental deletion.

**Alternatives considered**:
- **Always replace attachment on save**: Forces re-upload on every draft save even if unchanged. Rejected — poor UX; wastes bandwidth.
- **Multiple attachments per draft**: Out of scope for this phase per the existing one-attachment-per-idea model.

---

## Decision 6: Submit Draft Flow

**Decision**: A dedicated `POST /Ideas/SubmitDraft/{id}` action applies the full `CreateIdeaValidator` rules (category required, category fields required) to the draft's persisted data. On success, the service sets `Status = IdeaStatus.Submitted` and `LastModifiedDate = DateTime.UtcNow`. On failure, the edit form is re-displayed with inline errors.

**Rationale**:
- FR-008 and SC-005 require full validation at submit time with inline error display and no data loss.
- Reusing `CreateIdeaValidator` (which already handles all category-conditional rules) avoids duplicating validation logic, consistent with Principle III (Service-Layer Business Logic).
- The validation is applied to the live form data (from the Submit button's POST body), not the DB record — so any last-minute edits made before clicking Submit are also validated.

**Alternatives considered**:
- **Validate the stored DB record at submit time**: Simpler server-side call. Rejected — any unsaved edits in the form at submit time would be lost or ignored.
