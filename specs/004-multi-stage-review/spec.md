# Feature Specification: Multi-Stage Innovation Review Workflow

**Feature Branch**: `004-multi-stage-review`

**Created**: 2026-05-14

**Status**: Draft

**Input**: User description: "Add a multi-stage review workflow to the innovation evaluation system with four evaluation stages: initial screening, technical review, business impact assessment, and final decision. Administrators should be able to move ideas between stages, track review progress, and record evaluation outcomes while maintaining compatibility with the existing status tracking and role-based access workflows."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Admin Moves Idea Through Review Stages (Priority: P1)

An administrator picks up a submitted idea and progresses it through the four evaluation stages: Initial Screening → Technical Review → Business Impact Assessment → Final Decision. At each stage, the admin can record their evaluation notes. Moving an idea to the first stage automatically marks it as "Under Review" in the existing status system.

**Why this priority**: This is the core value of the feature. Without the ability to advance stages, no review tracking is possible.

**Independent Test**: Log in as Admin, open a submitted idea, advance it to Initial Screening, verify the stage is recorded. Advance it to Technical Review, verify stage changes. Repeat through all four stages — no submitter interaction required.

**Acceptance Scenarios**:

1. **Given** an idea in Submitted status, **When** an admin moves it to Initial Screening, **Then** the idea's review stage is set to "Initial Screening" and its overall status becomes "Under Review"
2. **Given** an idea in Initial Screening, **When** an admin advances it to Technical Review, **Then** the stage updates to "Technical Review"; the overall status remains "Under Review"
3. **Given** an idea in Technical Review, **When** an admin advances it to Business Impact Assessment, **Then** the stage updates accordingly
4. **Given** an idea in Business Impact Assessment, **When** an admin advances it to Final Decision, **Then** the stage updates to "Final Decision"
5. **Given** an idea in any stage, **When** a submitter attempts to change the stage, **Then** the system denies the action

---

### User Story 2 — Admin Records Evaluation Notes per Stage (Priority: P1)

When transitioning an idea to a new review stage, an administrator can optionally enter evaluation notes specific to that stage — observations, questions raised, scores, or rationale. These notes are preserved as part of the stage history.

**Why this priority**: Traceability is a business requirement. Evaluation notes justify decisions and enable handover between reviewers.

**Independent Test**: Log in as Admin, advance an idea to Technical Review while entering notes. View the idea's review history — the notes must appear alongside the stage record.

**Acceptance Scenarios**:

1. **Given** an idea at any review stage, **When** an admin advances the stage with evaluation notes entered, **Then** the notes are stored and associated with that stage transition
2. **Given** an idea at any review stage, **When** an admin advances the stage without entering notes, **Then** the transition is recorded with an empty notes field (notes are optional)
3. **Given** a stage transition has been recorded, **When** an admin views the idea's review history, **Then** each transition shows: stage name, responsible admin, date, and notes

---

### User Story 3 — Admin Records Final Decision Outcome (Priority: P1)

From the Final Decision stage, an administrator can select the final outcome: Accepted or Rejected. This selection maps to the existing overall idea status (Accepted or Rejected) and closes the review workflow.

**Why this priority**: The workflow must produce an actionable outcome; without this story the four-stage process has no conclusion.

**Independent Test**: Advance an idea to Final Decision stage, select "Accept", verify the overall status becomes Accepted. Repeat with "Reject".

**Acceptance Scenarios**:

1. **Given** an idea in the Final Decision stage, **When** an admin selects "Accept" and confirms, **Then** the overall idea status becomes "Accepted" and no further stage transitions are possible
2. **Given** an idea in the Final Decision stage, **When** an admin selects "Reject" and confirms, **Then** the overall idea status becomes "Rejected" and no further stage transitions are possible
3. **Given** an idea in the Final Decision stage, **When** an admin does not yet select an outcome, **Then** the idea remains in Final Decision and the review workflow remains open
4. **Given** an idea with overall status Accepted or Rejected, **When** any user views the review history, **Then** the final decision stage and its outcome are visible in the audit trail

---

### User Story 4 — Admin Reverts a Review Stage (Priority: P2)

An administrator can move an idea back to a previous review stage when an evaluation was premature or requires re-examination. Stage reverts are logged with a reason.

**Why this priority**: Mistakes happen and re-review may be necessary; forced sequential progress without an escape path creates bottlenecks.

**Independent Test**: Advance an idea to Business Impact Assessment, then revert it to Technical Review while entering a revert reason. Verify the stage history shows the forward and backward transitions.

**Acceptance Scenarios**:

1. **Given** an idea in Technical Review or later, **When** an admin reverts to a previous stage with a reason, **Then** the stage is updated and the revert is recorded in the stage history
2. **Given** an idea in Initial Screening (the first stage), **When** an admin attempts to revert, **Then** the system prevents the action (no previous stage to revert to)
3. **Given** a stage revert has been recorded, **When** an admin views review history, **Then** the revert is clearly distinguished from a forward transition

---

### User Story 5 — Submitter Tracks Review Stage Progress (Priority: P2)

A submitter can view the current review stage of their submitted idea on the idea detail page. The four-stage progress is displayed in a read-only format alongside the existing status information.

**Why this priority**: Transparency reduces support inquiries and builds trust in the review process.

**Independent Test**: Log in as Submitter, open a submitted idea that has been moved to Technical Review by an admin. Verify the review stage is visible and read-only.

**Acceptance Scenarios**:

1. **Given** an idea under review with a stage assigned, **When** the submitter views the idea detail page, **Then** the current review stage is displayed in a read-only section
2. **Given** an idea not yet in any review stage (Submitted status, no stage assigned), **When** the submitter views the idea, **Then** no stage indicator is shown (or a "Pending Review" placeholder is shown)
3. **Given** an idea that has been Accepted or Rejected, **When** the submitter views the idea, **Then** the final review stage and outcome are visible in the history

---

### Edge Cases

- What happens when an admin tries to advance a Draft idea to a review stage? → System prevents it; only Submitted or Under Review ideas can enter the stage workflow.
- What happens when an idea has no assigned review stage but its overall status is "Under Review"? → The idea is shown as "Under Review — Stage not yet assigned" and can be advanced to Initial Screening.
- What happens if the same idea is advanced by two admins simultaneously? → Last-write-wins; the transition is logged with the responsible admin's identity.
- What happens when evaluation notes exceed the maximum length? → System enforces a character limit and shows an inline validation error before the transition is saved.
- What happens when an idea is in Final Decision but the admin closes the browser before selecting an outcome? → The idea remains in Final Decision with no outcome; the admin can return and record the outcome later.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define exactly four review stages in fixed sequential order: Initial Screening (1), Technical Review (2), Business Impact Assessment (3), Final Decision (4)
- **FR-002**: Admins MUST be able to advance an idea to the next review stage from the idea detail page
- **FR-003**: Admins MUST be able to revert an idea to a previous review stage, with a mandatory revert reason (max 500 characters)
- **FR-004**: System MUST automatically transition the overall idea status to "Under Review" when an idea is moved to any review stage for the first time
- **FR-005**: Admins MUST be able to enter evaluation notes (optional, max 1000 characters) when performing any stage transition (advance or revert)
- **FR-006**: System MUST record every stage transition with: stage name, direction (advance/revert), responsible admin, timestamp, and notes
- **FR-007**: System MUST display the complete stage transition history on the admin idea detail page
- **FR-008**: System MUST display the current review stage (read-only) on the submitter idea detail page when a stage has been assigned
- **FR-009**: From the Final Decision stage, admins MUST be able to record an outcome of either "Accepted" or "Rejected"; doing so sets the overall idea status accordingly
- **FR-010**: System MUST prevent any stage transitions (advance, revert, or final decision) on ideas with overall status Accepted or Rejected
- **FR-011**: System MUST prevent submitters from performing any stage transition or recording evaluation notes
- **FR-012**: System MUST prevent Draft ideas from entering the review stage workflow
- **FR-013**: Admins MUST be able to filter the idea list by review stage in addition to the existing status and category filters

### Key Entities

- **ReviewStage**: Represents a specific stage in the evaluation workflow; has a fixed set of four values with sequential order
- **StageTransition**: A single recorded movement of an idea from one stage to another (or from no stage to the first stage); captures direction, responsible admin, timestamp, evaluation notes, and optional outcome for Final Decision transitions
- **Idea** (extended): Gains a nullable current review stage field linking to the ReviewStage; existing Status, Category, and CategoryData fields are unchanged

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admins can advance an idea through all four review stages within 5 minutes without leaving the portal
- **SC-002**: 100% of stage transitions are recorded with admin identity and timestamp; no silent or unlogged transitions occur
- **SC-003**: Submitters can see their idea's current review stage within 3 clicks from the home page
- **SC-004**: Accepted and Rejected ideas cannot have their review stage modified under any circumstance
- **SC-005**: Existing status filter and category filter on the admin idea list continue to work correctly alongside the new stage filter
- **SC-006**: Review stage history is visible to admins at all times; history is never deleted or hidden
- **SC-007**: Draft ideas are completely excluded from the review stage workflow; no stage can be assigned to a Draft

---

## Assumptions

- The four review stages are fixed and not configurable by users in this phase; custom stage management is out of scope
- Only users with the Admin role can perform stage transitions and record evaluation notes; Submitters are read-only participants
- A single admin performs all stages; collaborative multi-reviewer workflows (assign to specific reviewer) are out of scope for this phase
- Stage transition history is append-only and cannot be deleted or edited after recording
- The feature depends on Spec 001 (innovation ideas), Spec 002 (category forms), and Spec 003 (draft management) being implemented and deployed; in particular, the Draft status exclusion from Spec 003 (FR-012) must already be in place
- Email notifications when an idea changes stage are out of scope for this phase (deferred to a future notification feature)
- There is no time limit or SLA enforcement per stage in this phase; stage deadlines are tracked manually by admins
- An idea that is reverted all the way to Initial Screening and then re-advanced goes through the full four-stage sequence again; historical transitions from previous cycles remain in the history
