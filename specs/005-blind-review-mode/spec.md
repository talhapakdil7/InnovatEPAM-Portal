# Feature Specification: Blind Review Mode

**Feature Branch**: `005-blind-review-mode`

**Created**: 2026-05-14

**Status**: Draft

**Input**: User description: "Add a blind review mode to the innovation evaluation workflow where administrator reviewers cannot see submitter identity information during the evaluation process. User details such as name, email, and department should remain hidden while preserving the existing review workflow, status tracking, and role-based access system."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Admin Reviews Idea Without Seeing Submitter Identity (Priority: P1)

When blind review mode is active, an administrator opens any idea for review and finds that all fields identifying the submitter — including name, email, and department — are replaced by a neutral placeholder. The admin can still read the idea's title, description, category, attachments, and category-specific fields. The admin can advance the idea through review stages, record evaluation notes, and make final decisions, all without ever learning who submitted the idea.

**Why this priority**: This is the core value of the feature. All other stories depend on this behaviour being correct. Without this story, blind review mode provides no fairness guarantee.

**Independent Test**: Can be fully tested by enabling blind review mode, opening any idea as an admin, and verifying that no submitter-identifying information is visible in any admin-facing page. Delivers the primary fairness guarantee independently.

**Acceptance Scenarios**:

1. **Given** blind review mode is enabled and an idea is in "Submitted" status, **When** an admin opens the idea detail page, **Then** the submitter's name, email, and department are not visible anywhere on the page; a neutral label (e.g., "Anonymous Submitter") is shown in their place.
2. **Given** blind review mode is enabled, **When** an admin views the idea list page, **Then** the "Submitted By" column shows a neutral placeholder instead of the real submitter name.
3. **Given** blind review mode is enabled, **When** an admin advances the idea to the next review stage and adds notes, **Then** the stage transition is recorded correctly and the submitter's identity is still not revealed.
4. **Given** blind review mode is enabled, **When** an admin views the stage transition history, **Then** the admin username (who performed the transition) is still visible, but the submitter's identity remains hidden.
5. **Given** blind review mode is enabled, **When** an admin views the audit (status-change) history, **Then** no submitter-identifying information appears in that panel.

---

### User Story 2 — Admin Toggles Blind Review Mode On or Off (Priority: P2)

An administrator with appropriate privileges can navigate to a settings area and enable or disable blind review mode for the entire system. The change takes effect immediately: the next page load by any admin will reflect the new mode. The setting persists across server restarts and sessions.

**Why this priority**: Without an on/off toggle, the feature cannot be activated or deactivated. This story is the control mechanism for US1.

**Independent Test**: Can be fully tested by visiting the settings page, toggling the mode, and confirming the change is reflected in admin idea views on the very next request. Delivers administrative control independently.

**Acceptance Scenarios**:

1. **Given** blind review mode is currently disabled, **When** an admin activates it via the settings page, **Then** a confirmation message is displayed and admin idea views immediately stop showing submitter identity.
2. **Given** blind review mode is currently enabled, **When** an admin deactivates it via the settings page, **Then** a confirmation message is displayed and submitter identity is visible again in admin idea views.
3. **Given** any admin visits the settings page, **When** the page loads, **Then** the current state of blind review mode (on/off) is clearly displayed.

---

### User Story 3 — Submitter Identity Is Revealed After a Final Decision Is Recorded (Priority: P3)

Once an admin records a final decision on an idea (marking it as Accepted or Rejected), the submitter's identity becomes visible again to admins viewing that specific idea, even while blind review mode remains globally enabled. This allows the system to notify or acknowledge the submitter after the evaluation is complete.

**Why this priority**: Post-decision identity reveal is a natural workflow closure step. It depends on US1 and US2 being functional but delivers additional workflow completeness.

**Independent Test**: Can be fully tested by enabling blind review mode, advancing an idea to Final Decision, recording a decision, and confirming that the submitter's name is now visible on the idea detail page. Delivers post-evaluation disclosure independently.

**Acceptance Scenarios**:

1. **Given** blind review mode is enabled and an idea's status is "Accepted" or "Rejected", **When** an admin opens the idea detail page, **Then** the submitter's name is visible (identity reveal applies to concluded ideas).
2. **Given** blind review mode is enabled and an idea's status is "Under Review", **When** an admin opens the idea detail page, **Then** the submitter's identity remains hidden.
3. **Given** blind review mode is enabled and an idea's status returns to "Under Review" after having been Accepted, **When** an admin opens the idea detail page, **Then** identity is hidden again.

---

### User Story 4 — Submitter Experience Is Unaffected (Priority: P4)

When blind review mode is enabled, submitters log in and use the portal exactly as they always have. They can view their own ideas (including their own name and details), submit new ideas, save drafts, and track review progress. Blind review mode is purely an admin-facing concept.

**Why this priority**: Backward compatibility is important but is a constraint rather than a core capability. US1–3 deliver the primary value; this story documents that submitters are not impacted.

**Independent Test**: Can be fully tested by enabling blind review mode and logging in as a submitter, verifying that the submitter's name and details still appear normally in their own views.

**Acceptance Scenarios**:

1. **Given** blind review mode is enabled, **When** a submitter views their own idea detail page, **Then** their own name, submission date, and all other details are displayed normally.
2. **Given** blind review mode is enabled, **When** a submitter views their idea list, **Then** no information is masked or hidden.

---

### Edge Cases

- What happens when blind review mode is toggled mid-review? Idea pages immediately reflect the new mode — no stale caches should retain identity information.
- What happens if the only admin account toggles blind review mode? The change should still take effect immediately and apply to their own session.
- What happens if an admin directly accesses the submitter's profile page? Submitter profile pages are outside the idea review workflow scope; they are unaffected by blind review mode.
- What happens when an idea is re-opened for review after being Accepted/Rejected? Identity should be hidden again while the idea is under active review.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: When blind review mode is enabled, the system MUST hide all submitter-identifying information (name, email, department) from all admin-facing views related to idea review.
- **FR-002**: The system MUST replace hidden submitter identity with a consistent neutral label (e.g., "Anonymous Submitter") so that admin views remain visually coherent.
- **FR-003**: Admins MUST be able to enable or disable blind review mode from a dedicated settings page accessible to all admin users.
- **FR-004**: Blind review mode MUST be a global, system-wide setting — not per-idea or per-reviewer.
- **FR-005**: The current state of blind review mode (enabled/disabled) MUST be persisted and survive server restarts.
- **FR-006**: Identity masking MUST apply across all admin-facing idea views: idea list, idea detail, review stage history, and audit history panels.
- **FR-007**: When an idea's status is "Accepted" or "Rejected", the submitter's identity MUST be visible to admins even when blind review mode is globally enabled.
- **FR-008**: Blind review mode MUST NOT affect submitters' own views of their ideas; submitters must always see their own identity and submission details normally.
- **FR-009**: Blind review mode MUST NOT prevent any review workflow actions (advance stage, revert stage, record decision, update status).
- **FR-010**: The underlying submitter identity data MUST remain stored in full; masking is applied only at the presentation layer and never alters persisted data.
- **FR-011**: The settings change MUST take effect immediately on the next page load — no delayed activation or scheduled jobs required.

### Key Entities

- **BlindReviewSetting**: A single system-wide record that stores whether blind review mode is currently active, along with the timestamp of the last change and the identity of the admin who changed it.
- **IdeaPresentation (concept)**: The resolved view of an idea shown to an admin, which applies the blind review mask conditionally based on the global setting and the idea's current status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When blind review mode is enabled, zero submitter-identifying fields (name, email, department) are visible to any admin in any idea-review page.
- **SC-002**: An admin can complete the entire review workflow — from idea submission through Final Decision — without the blind review mode interfering with any workflow action.
- **SC-003**: The blind review mode toggle takes effect within one page load (no server restart or cache flush required).
- **SC-004**: Submitters experience zero functional or visual change in their portal workflow regardless of whether blind review mode is on or off.
- **SC-005**: After a final decision (Accepted/Rejected) is recorded, 100% of admin idea detail pages for that idea display the submitter's real identity.
- **SC-006**: The settings page correctly reflects the current blind review mode state 100% of the time.
- **SC-007**: No changes to persisted idea or user data occur as a result of enabling or disabling blind review mode.

## Assumptions

- Blind review mode applies to all admin users equally; there is no admin sub-role or "super admin" exemption from identity masking.
- "Department" refers to any organizational grouping stored in the user profile (e.g., team or business unit). If the current user model does not include department, only name and email are masked.
- Identity is revealed when an idea's `IdeaStatus` is `Accepted` or `Rejected` — not merely when it has reached the `FinalDecision` review stage, since a Final Decision stage idea may still be under deliberation.
- The settings page for blind review mode is accessible to all admin users, not just a designated system administrator.
- Notification emails (Faz 2) are out of scope; if the system eventually sends emails to submitters after a decision, blind review mode does not need to redact those emails.
- SSO and directory-service integration are out of scope.
- The existing `AuditLog` entries (which record status changes by admin) are not affected since they record admin identity, not submitter identity.
- Stage transition history records the reviewing admin's identity, which remains always visible since it is not submitter data.
