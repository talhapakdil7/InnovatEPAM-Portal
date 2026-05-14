# Feature Specification: Idea Draft Management

**Feature Branch**: `003-draft-management`

**Created**: 2026-05-14

**Status**: Draft

**Input**: User description: "Add draft management functionality to the innovation submission workflow. Users should be able to save ideas as drafts before final submission, continue editing drafts later, and submit drafts when ready. Drafts must preserve uploaded attachments and dynamic form data while integrating seamlessly with the existing submission workflow and role-based access system."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Save Idea as Draft (Priority: P1)

A submitter begins filling out the innovation submission form — selecting a category, entering a title, and providing category-specific answers — but is not ready to submit yet. They click "Save as Draft" to preserve their progress. The system saves all entered data, including any uploaded attachment, without triggering submission validation. The user can safely close the browser and return later.

**Why this priority**: This is the foundational capability. Without the ability to save a draft, all downstream stories (editing, submitting) are meaningless. It protects submitters from losing work-in-progress.

**Independent Test**: Can be fully tested by logging in as a Submitter, partially filling the idea form, clicking "Save as Draft", closing the browser, and verifying the draft appears in the submitter's draft list upon return.

**Acceptance Scenarios**:

1. **Given** a logged-in submitter on the idea creation form, **When** they click "Save as Draft" with any amount of data entered (including an empty title), **Then** the system saves the draft without displaying validation errors and shows a confirmation message.
2. **Given** a submitter who has uploaded an attachment during draft creation, **When** they save as draft, **Then** the attachment file is preserved and associated with the draft.
3. **Given** a submitter who has selected a category and filled category-specific fields, **When** they save as draft, **Then** all category field answers are preserved exactly as entered.

---

### User Story 2 — View and Manage Drafts (Priority: P2)

A submitter returns to the portal after saving one or more drafts. They can access a dedicated section or filter on their ideas list that shows only their draft ideas. From this view they can open a draft to continue editing, or delete a draft they no longer need.

**Why this priority**: Drafts are useless if the submitter cannot find them again. Discoverability and basic management (open, delete) are required before editing or submission flows make sense.

**Independent Test**: Can be tested by creating multiple drafts and verifying they appear in the submitter's ideas list (with a clear "Draft" indicator), then deleting one and confirming it no longer appears.

**Acceptance Scenarios**:

1. **Given** a submitter with one or more saved drafts, **When** they visit their ideas list, **Then** each draft is displayed with a clear "Draft" status label distinct from submitted, under-review, accepted, or rejected ideas.
2. **Given** a submitter viewing their ideas list, **When** they delete a draft, **Then** the draft and any associated attachments are permanently removed and no longer appear in any listing.
3. **Given** a submitter with multiple drafts, **When** they view their ideas list, **Then** they can distinguish each draft by its title and last-modified date.

---

### User Story 3 — Continue Editing a Draft (Priority: P2)

A submitter opens a previously saved draft and sees all previously entered fields pre-populated, including category selection, category-specific fields, title, description, and any uploaded attachment. They can make changes — adding, modifying, or replacing content — and either save as draft again or proceed to submit.

**Why this priority**: The ability to return and edit is the core value of the draft feature. Without this, saving a draft is merely a backup, not a productivity tool.

**Independent Test**: Can be tested by saving a draft with partial data, reopening it, verifying all fields are pre-populated, modifying a field, saving again, and confirming the new values are persisted.

**Acceptance Scenarios**:

1. **Given** a submitter who opens an existing draft, **When** the edit form loads, **Then** all previously saved field values (category, title, description, category-specific fields) are pre-populated.
2. **Given** a submitter editing a draft that has an attachment, **When** they view the form, **Then** the existing attachment is listed with an option to remove or replace it.
3. **Given** a submitter editing a draft, **When** they click "Save as Draft" again, **Then** the changes are persisted and the draft's last-modified date is updated.

---

### User Story 4 — Submit a Draft (Priority: P3)

A submitter opens a saved draft, reviews the content, and decides it is ready. They click "Submit" to formally submit the idea for admin review. The system validates all required fields (including category-specific ones) and, if valid, transitions the idea from Draft status to Submitted — making it visible in the admin review queue.

**Why this priority**: Submission is the ultimate goal of the draft workflow. Without it, drafts never enter the review process. It is P3 because it depends on the prior stories and adds the submission-time validation layer.

**Independent Test**: Can be tested by opening a fully completed draft and clicking "Submit", then verifying the idea appears in the admin's review list with "Submitted" status and is no longer visible in the submitter's draft list.

**Acceptance Scenarios**:

1. **Given** a submitter viewing a fully completed draft, **When** they click "Submit", **Then** the system validates all required fields, transitions the idea to "Submitted" status, and the submitter sees a success confirmation.
2. **Given** a submitter attempting to submit a draft with missing required fields, **When** they click "Submit", **Then** the system displays inline validation errors and does not submit the draft.
3. **Given** a submitter who submits a draft, **When** an admin views the review list, **Then** the submitted idea is visible with all content (including category data) and the attachment is accessible.

---

### Edge Cases

- What happens when a submitter tries to submit a draft with a missing required category field? → Submission is blocked; inline errors shown; draft remains editable.
- What happens if an attachment file is missing from storage when the draft is reopened? → A warning message is shown; the user can re-upload; draft remains accessible.
- Can a submitter have multiple drafts simultaneously? → Yes, no cap on concurrent drafts per user in this phase.
- What happens to drafts if a user's account is deactivated? → Drafts are retained in the system but inaccessible; handled by the admin account lifecycle process (out of scope for this feature).
- What if a submitter refreshes the page mid-edit without saving? → Unsaved changes are lost; only the last saved state is preserved.
- Can admin users view or access drafts? → No; drafts are private to the submitter and never appear in the admin review queue until formally submitted.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow submitters to save a partially completed idea form as a draft without triggering required-field validation.
- **FR-002**: System MUST preserve all form data in the draft including title, description, category key, and all entered category-specific field values.
- **FR-003**: System MUST preserve uploaded attachments when saving a draft, retaining the file association across sessions.
- **FR-004**: Submitters MUST be able to view a list of their own drafts in their ideas list, clearly identified by a "Draft" status indicator.
- **FR-005**: Submitters MUST be able to open a saved draft and have all previously saved fields pre-populated in the edit form.
- **FR-006**: Submitters MUST be able to update the content of a draft and save the changes without submitting.
- **FR-007**: Submitters MUST be able to add, remove, or replace an attachment when editing a draft.
- **FR-008**: Submitters MUST be able to submit a draft, at which point all required-field validation rules are applied.
- **FR-009**: Submitters MUST be able to permanently delete a draft and its associated attachments.
- **FR-010**: Draft ideas MUST NOT appear in the admin review queue until formally submitted by the submitter.
- **FR-011**: A draft that is successfully submitted MUST transition to "Submitted" status and follow the same review workflow as a directly submitted idea.
- **FR-012**: Only the submitter who created a draft MAY view, edit, or delete that draft.

### Key Entities

- **Draft Idea**: An idea in "Draft" status, owned by a specific submitter. Contains all idea fields (title, description, category, category-specific field data, attachments). Not visible to admins. May be incomplete (required fields not yet filled).
- **Draft Attachment**: A file uploaded during draft creation or editing. Associated with the draft; must remain accessible until the draft is submitted or deleted.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Submitters can save a draft and retrieve the exact same form state (all fields and attachments) in a subsequent login session, with zero data loss.
- **SC-002**: Submitters can locate and open a previously saved draft within 30 seconds of logging into the portal.
- **SC-003**: Submitters can successfully convert a completed draft into a formal submission without re-entering any previously saved data.
- **SC-004**: Drafts are never visible in the admin review list; 100% of ideas in the admin queue have "Submitted" status or later.
- **SC-005**: Submitting a draft with missing required fields fails gracefully with clear inline error messages and no data loss.
- **SC-006**: All previously uploaded draft attachments are accessible when the draft is reopened, with no file loss between sessions.

## Assumptions

- Only the Submitter role creates innovation ideas; Admin accounts do not use the draft workflow.
- The category-adaptive form system from spec 002 (Smart Category-Adaptive Submission Forms) is implemented and active; draft management builds on top of it.
- A submitter may have an unlimited number of simultaneous drafts for this phase; rate limiting or per-user draft caps are deferred.
- Draft data retention follows general platform data retention policies; no automatic expiry is enforced in this phase.
- Email or in-app notifications when a draft is submitted are out of scope (aligned with Faz 2 scope from spec 001).
- Draft deletion is a destructive, irreversible action with no recycle bin or soft-delete in this phase.
- Mobile responsiveness follows the same standard as the rest of the portal (Bootstrap 5 responsive layout).
