# Quickstart & Test Scenarios: Idea Scoring System

**Feature**: `006-idea-scoring-system`
**Phase**: 1 — Testing Guide

---

## Prerequisites

- Application running locally (`dotnet run`)
- At least 2 admin accounts and 1 submitter account seeded
- At least 3 ideas in various states: one in `Submitted`, one in `UnderReview`, one in `Accepted`/`Rejected`, one in `Draft`
- Migration `AddIdeaScores` applied (`db.Database.Migrate()` runs at startup)

---

## Manual Test Scenarios

### Scenario 1 — Admin Scores All Four Dimensions (US1 — Happy Path)

**Setup**: Log in as Admin A. Open an idea with `Submitted` or `UnderReview` status.

**Steps**:
1. Navigate to `Admin > [Idea Title]`.
2. Locate the "Evaluation Scores" section.
3. Select a score of 4 for Innovation, 3 for Technical Feasibility, 5 for Business Impact, 2 for Implementation Value.
4. Click **Save Score**.

**Expected**:
- `TempData["Success"]` banner: "Your score has been saved."
- Page reloads. Score summary shows: Innovation 4.0, TF 3.0, BI 5.0, IV 2.0, Overall 3.50.
- Admin's row appears in the breakdown table.
- Scoring form is pre-populated with the saved values.

---

### Scenario 2 — Admin Updates an Existing Score (US1 — Update)

**Setup**: Admin A has already scored idea (from Scenario 1).

**Steps**:
1. Open the same idea detail page.
2. Change Innovation to 2.
3. Click **Save Score**.

**Expected**:
- Saved successfully. Innovation average now shows 2.0. Overall average recalculates to 3.00.
- Only one row for Admin A in the breakdown table (not two rows).

---

### Scenario 3 — Score Outside Valid Range Is Rejected (US1 — Validation)

**Setup**: Log in as Admin A.

**Steps**:
1. Using a direct form submission tool (e.g., browser devtools), submit `Innovation = 0` for a valid idea.
2. (Or: submit `Innovation = 6`).

**Expected**:
- Validation error: "Innovation score must be between 1 and 5."
- No score is saved. Form redisplays with the error.

---

### Scenario 4 — Partial Scoring (US1 — Partial)

**Steps**:
1. Log in as Admin B. Open an idea.
2. Score only Innovation (3) and Business Impact (4). Leave TF and IV blank.
3. Click **Save Score**.

**Expected**:
- Saved successfully. Overall average = (3 + 4) / 2 = 3.50 for Admin B's row.
- Dimension averages for TF and IV are null (shown as "—") for this admin's row.
- No error for missing dimensions.

---

### Scenario 5 — Two Admins Score the Same Idea; Aggregate Visible (US2)

**Setup**: Admin A has scored Innovation=4, TF=3, BI=5, IV=2. Admin B has scored Innovation=2, TF=5, BI=3, IV=4.

**Steps**:
1. Open the idea as any admin.
2. View the Score Summary.

**Expected**:
- AvgInnovation = 3.0, AvgTF = 4.0, AvgBI = 4.0, AvgIV = 3.0, OverallAverage = 3.50.
- "2 reviewers" shown.
- Both Admin A and Admin B rows shown in breakdown table.

---

### Scenario 6 — Unscored Idea Shows "No Scores Yet" (US2 — Empty State)

**Setup**: An idea with no scores.

**Steps**:
1. Open the idea detail page as Admin.

**Expected**:
- Score section shows "No scores yet" placeholder.
- No scoring form blocked — scoring form is still active if idea is in valid status.
- Admin list shows "—" in the Score column for this idea.

---

### Scenario 7 — Admin Retracts Their Score (US3)

**Setup**: Admin A has scored an idea.

**Steps**:
1. Open the idea detail page as Admin A.
2. Click **Remove My Score**. Confirm the action.

**Expected**:
- "Your score has been removed." banner.
- Admin A's row disappears from the breakdown table.
- Aggregate recalculates excluding Admin A's scores.
- If Admin A was the only scorer: "No scores yet" placeholder appears.

---

### Scenario 8 — Retract a Score That Doesn't Exist (US3 — Edge Case)

**Steps**:
1. As Admin B (who has not scored an idea), manually POST to `/Score/Retract/{ideaId}`.

**Expected**:
- No error. Redirect back to the idea detail page.
- No data changed.

---

### Scenario 9 — Submitter Sees Only Overall Aggregate (US4)

**Setup**: Admins have scored idea owned by Submitter X.

**Steps**:
1. Log in as Submitter X.
2. Open "My Ideas" → click the scored idea.

**Expected**:
- "Evaluation Score: 3.50 / 5 (rated by 2 reviewer(s))" section visible.
- No dimension breakdown visible.
- No scorer names visible.
- No scoring form visible.

---

### Scenario 10 — Scoring Disabled for Draft Ideas (Edge Case — FR-001 / SC-005)

**Setup**: A `Draft` idea exists.

**Steps**:
1. Log in as Admin.
2. Navigate to the idea's detail page (admin can access drafts).

**Expected**:
- No scoring form rendered.
- No score section shown (or a "Scoring not available for drafts" note).

---

### Scenario 11 — Scoring Read-Only for Concluded Ideas (Edge Case — FR-010)

**Setup**: An idea with `Accepted` or `Rejected` status.

**Steps**:
1. Log in as Admin. Open the concluded idea.

**Expected**:
- Scoring form is NOT rendered.
- Read-only badge: "Scoring closed — idea has been decided."
- Existing scores remain visible in the summary.

---

### Scenario 12 — Submitter Cannot Submit a Score (FR-011)

**Steps**:
1. Log in as a Submitter.
2. Manually POST to `/Score/Submit` with valid form data.

**Expected**:
- HTTP 403 Forbidden or redirect to AccessDenied page.
- No score is persisted.

---

## Regression Test Scenarios

| # | Test | Expected Outcome |
|---|---|---|
| R1 | Existing multi-stage review workflow | Stage advance, revert, and decision actions work as before — scoring section appears alongside but does not interfere |
| R2 | Blind review mode enabled → admin views scored idea | Scorer names replaced by "Anonymous Reviewer" in breakdown; aggregate scores still visible |
| R3 | Blind review mode enabled → submitter views idea | Only overall aggregate shown; no names; same as without blind review |
| R4 | Admin idea list with mix of scored/unscored ideas | Scored ideas show average + count; unscored show "—"; sorting by status unaffected |
| R5 | Draft idea submission → still no score available | Draft → Submit transition does not inherit scores; scoring becomes available after status changes |
| R6 | Admin changes idea status to Accepted → scoring locks | After RecordDecision marks idea Accepted, scoring form disappears; existing scores remain |
| R7 | Two admins independently navigate to same idea and both score simultaneously | Both UpsertAsync calls succeed with no deadlock; composite PK constraint prevents duplicate rows |
