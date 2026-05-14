# Feature Specification: Idea Scoring System

**Feature Branch**: `006-idea-scoring-system`

**Created**: 2026-05-14

**Status**: Draft

**Input**: User description: "Add a scoring system to the innovation evaluation workflow where administrators can rate submitted ideas using multiple evaluation dimensions with scores from 1 to 5. Evaluation categories may include innovation, technical feasibility, business impact, and implementation value. The system should calculate and display aggregated scores while integrating seamlessly with the existing multi-stage review workflow and role-based access system."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Admin Scores an Idea Across Evaluation Dimensions (Priority: P1)

An administrator is reviewing an idea that is currently in the multi-stage review pipeline. From the idea detail page, the admin can submit a score for each evaluation dimension (Innovation, Technical Feasibility, Business Impact, Implementation Value) using a 1–5 scale. Submitting the scores is an explicit action, and scores can be updated at any time by any admin until a final decision is recorded.

**Why this priority**: This is the core feature — without the ability to record scores, no other part of the system delivers value.

**Independent Test**: An admin opens any idea currently in review, selects scores (1–5) for each dimension, submits the form, and sees the scores saved and displayed on the idea detail page.

**Acceptance Scenarios**:

1. **Given** an admin is logged in and viewing an idea in "Submitted" or "Under Review" status, **When** the admin enters a score between 1 and 5 for each evaluation dimension and submits, **Then** the scores are saved and displayed on the idea detail page.
2. **Given** an admin has previously scored an idea, **When** the admin submits an updated score for any dimension, **Then** the new score replaces the previous value and the aggregate score is recalculated.
3. **Given** an admin attempts to submit a score outside the 1–5 range, **When** the form is validated, **Then** a clear error message is shown and no score is saved.
4. **Given** an admin has scored some but not all dimensions, **When** the admin submits, **Then** the system accepts the partial score and calculates the aggregate only over the scored dimensions.

---

### User Story 2 — Admins View the Aggregated Score for an Idea (Priority: P2)

Any administrator can see the aggregated overall score for an idea, calculated as the average across all scored dimensions. The aggregated score is visible on both the idea detail page and the admin idea list. When multiple admins have scored the same idea, the system displays a combined average.

**Why this priority**: Score aggregation and visibility are essential for comparative decision-making; without this, the raw scores have limited utility.

**Independent Test**: After two admins have each scored an idea, the admin idea list and detail page both show the combined average score with the number of scorers displayed.

**Acceptance Scenarios**:

1. **Given** one admin has scored an idea, **When** another admin views the idea, **Then** they see the score submitted by the first admin along with the calculated aggregate.
2. **Given** two admins have scored an idea with different values for the same dimension, **When** any admin views the aggregate, **Then** the displayed score reflects the mean across all admin submissions for that dimension.
3. **Given** an idea has not been scored yet, **When** an admin views the idea, **Then** a "No scores yet" indicator is displayed instead of a numeric score.
4. **Given** an idea has been scored, **When** an admin views the admin idea list, **Then** the overall aggregate score is shown in the list alongside the idea title and status.

---

### User Story 3 — Admin Removes Their Score (Priority: P3)

An administrator who has previously scored an idea can retract their score for that idea. After removal, the aggregate is recalculated excluding the retracted scores. Removal is per-admin — removing one admin's score does not affect other admins' scores.

**Why this priority**: Score management is important for workflow integrity, but the ability to retract a score is less critical than initial scoring and aggregation.

**Independent Test**: An admin who has scored an idea clicks "Remove My Score", confirms the action, and the aggregate is updated to exclude their score.

**Acceptance Scenarios**:

1. **Given** an admin has previously scored an idea, **When** the admin confirms the removal of their score, **Then** the score record is deleted and the aggregate is recalculated.
2. **Given** an admin attempts to remove a score on an idea they have not scored, **Then** no removal action is available or the action is silently ignored.

---

### User Story 4 — Submitter Views the Aggregated Score for Their Own Idea (Priority: P4)

A submitter can view the aggregated score for their own ideas once a score has been recorded. The individual admin scores and scorer identities are not visible to the submitter — only the overall aggregated score is shown.

**Why this priority**: Submitter visibility is a transparency improvement but not required for admin operations; it is the lowest-priority story.

**Independent Test**: A submitter opens their own idea's detail page and sees the overall average score (but not who scored it or individual dimension scores by admin).

**Acceptance Scenarios**:

1. **Given** at least one admin has scored an idea, **When** the submitter views that idea, **Then** the submitter sees the overall aggregated score and a label such as "Evaluation Score".
2. **Given** an idea has not been scored, **When** the submitter views that idea, **Then** no score section is shown or a "Pending evaluation" label is displayed.
3. **Given** a submitter views their own idea, **Then** individual dimension breakdowns by admin are not visible.

---

### Edge Cases

- What happens when all admins retract their scores? — The idea returns to "No scores yet" state; the aggregate section shows no score.
- What happens when an admin scores a "Draft" idea? — Scoring is not available for Draft-status ideas; the scoring section is hidden.
- What if a dimension score is submitted as 0 or a non-integer? — Validation rejects any value outside the integer range 1–5.
- What if an admin has scored an idea and the idea is subsequently moved to "Accepted" or "Rejected"? — Scores remain visible but the scoring form is disabled (read-only) for concluded ideas.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow admin users to submit scores for each of the four evaluation dimensions (Innovation, Technical Feasibility, Business Impact, Implementation Value) for any idea that is not in Draft, Accepted, or Rejected status.
- **FR-002**: Each individual score MUST be an integer value between 1 and 5 (inclusive). Values outside this range MUST be rejected with a user-facing validation message.
- **FR-003**: An admin MAY score a subset of the four dimensions; full scoring across all dimensions is encouraged but not enforced.
- **FR-004**: The system MUST allow an admin to update their previously submitted scores for any idea at any time while the idea remains in an active (non-concluded) status.
- **FR-005**: The system MUST display an aggregated overall score per idea, calculated as the mean of all admin scores across all scored dimensions.
- **FR-006**: The system MUST display dimension-level aggregated scores (mean per dimension across all admin submissions) on the idea detail page visible to admins.
- **FR-007**: The system MUST display the aggregated overall score alongside each idea in the admin idea list view.
- **FR-008**: An admin MUST be able to retract their own score for an idea; retraction recalculates all aggregates immediately.
- **FR-009**: A submitter MUST be able to view only the overall aggregated score for their own ideas; individual dimension breakdowns and scorer identities MUST NOT be visible to submitters.
- **FR-010**: Scoring MUST be disabled (read-only display) for ideas with Accepted or Rejected status.
- **FR-011**: The scoring form MUST NOT be accessible by submitter-role users; any direct access attempt MUST be rejected with an authorization error.
- **FR-012**: The system MUST maintain a record of which admin submitted which score, for audit purposes.

### Key Entities

- **IdeaScore**: A single admin's score submission for one idea. Attributes: idea reference, scoring admin reference, per-dimension scores (Innovation, TechnicalFeasibility, BusinessImpact, ImplementationValue — each 1–5 integer, nullable), submission timestamp, last updated timestamp. One record per (idea, admin) pair.
- **ScoreSummary** *(derived, not persisted)*: Computed aggregate view of all `IdeaScore` records for one idea. Contains per-dimension average, overall average, total scorer count.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An admin can complete scoring an idea across all four dimensions in under 60 seconds from the idea detail page.
- **SC-002**: The aggregated score displayed on the idea list and detail page reflects updates from any admin's score change within a single page reload — no caching delay.
- **SC-003**: All four evaluation dimensions are visible and scorable from a single page without requiring navigation between screens.
- **SC-004**: Submitters can view the aggregated score for their own ideas without seeing individual admin scores or scorer identities — verified by a submitter-role user session.
- **SC-005**: Ideas with Draft, Accepted, or Rejected status do not expose an active scoring form — verified by attempting access in each status.
- **SC-006**: Score aggregation is accurate to two decimal places for the average when displayed, and recalculates correctly when scores are added, updated, or removed.
- **SC-007**: Unauthorized access attempts by submitter-role users to submit scores result in a clear rejection — no score is persisted.

---

## Assumptions

- Only users with the Admin role can submit, update, or remove scores. The Submitter role is read-only for scores.
- The four evaluation dimensions (Innovation, Technical Feasibility, Business Impact, Implementation Value) are fixed for this feature. Dynamic or user-configurable dimensions are out of scope.
- Multiple admins scoring the same idea is fully supported; each admin has exactly one score record per idea.
- Score data is retained permanently alongside the idea; deleting a score record is a user-initiated per-admin action (retract), not an automatic system cleanup.
- The scoring system is additive to the existing multi-stage review workflow — it does not replace status tracking, stage transitions, or final decision recording.
- Ideas in "Draft" status cannot be scored, as they are not yet formally submitted. Ideas in "Submitted" and "Under Review" status are fully scorable.
- The aggregate score is calculated in the application layer (not as a stored computed column), ensuring consistency with the current data access patterns.
- Blind review mode (Spec 005) applies to scores: when blind review is active, the scorer names are not displayed in admin score views. Aggregated scores remain visible.
- No email notifications are triggered by score events in this version; notification support is deferred.
