# Quickstart & Manual Test Guide: Multi-Stage Review Workflow

**Feature**: Multi-Stage Review Workflow (`specs/004-multi-stage-review/spec.md`)
**Date**: 2026-05-14
**Prerequisites**: PostgreSQL running, `dotnet ef database update` applied, app running on `https://localhost:5001`

---

## Setup

1. Log in as Admin (`admin@test.com` / `Admin1234!`) and verify the Review Ideas list shows submitted ideas
2. Log in as a Submitter (`alice@test.com` / `Admin1234!`) and submit at least one fully-completed idea via `/Ideas/Create`
3. Ensure no existing ideas are already in Draft status (they should not appear in admin list)

---

## Scenario 1 — Advance Idea Through All Four Stages (US1 / SC-001)

**Goal**: Admin can advance a submitted idea through all four review stages within 5 minutes.

**Steps**:
1. Log in as Admin
2. Open a submitted idea from the Review Ideas list
3. **Expected**: A "Review Workflow" panel is visible with current stage "Pending Review" and an **Advance to Initial Screening** button
4. Click **Advance to Initial Screening** (leave notes blank) → confirm
5. **Expected**: Page reloads with stage = "Initial Screening" and success banner; overall status = "Under Review"
6. Click **Advance to Technical Review** → confirm
7. **Expected**: Stage = "Technical Review"
8. Click **Advance to Business Impact Assessment** → confirm
9. **Expected**: Stage = "Business Impact Assessment"
10. Click **Advance to Final Decision** → confirm
11. **Expected**: Stage = "Final Decision"; no further "Advance" button shown; a **Record Decision** panel appears

---

## Scenario 2 — Record Evaluation Notes During Advance (US2 / SC-002)

**Goal**: Notes are stored and visible in stage history.

**Steps**:
1. Open a submitted idea as Admin
2. Advance to Initial Screening with notes: `"No budget conflicts identified"`
3. Advance to Technical Review with notes: `"Architecture review required"`
4. **Expected**: Stage History section shows both transitions with the entered notes, admin name, and timestamp
5. Verify SC-002: both transitions have admin identity and timestamp — no blank admin or epoch date

---

## Scenario 3 — Record Final Decision: Accept (US3 / SC-004)

**Goal**: Accepted ideas cannot have their stage modified afterward.

**Steps**:
1. Advance an idea through all four stages to Final Decision
2. In the **Record Decision** panel, select **Accept** and enter notes: `"Strong ROI potential"`
3. Click **Submit Decision**
4. **Expected**: Overall status = "Accepted"; stage workflow panel disappears; no Advance/Revert buttons visible
5. Attempt to navigate to `/Admin/AdvanceStage` by POST — system returns an error
6. **Expected**: TempData error message; idea status unchanged

---

## Scenario 4 — Record Final Decision: Reject (US3)

**Steps**:
1. Advance a different idea to Final Decision
2. Select **Reject**, notes: `"Out of scope for current roadmap"`
3. **Expected**: Status = "Rejected"; stage history shows Final Decision with Outcome = "Rejected"

---

## Scenario 5 — Revert to Previous Stage (US4)

**Goal**: Admin can revert an idea and history shows the backward transition.

**Steps**:
1. Advance an idea to Business Impact Assessment
2. Click **Revert Stage** → select **Technical Review** as target → enter revert reason: `"Need deeper technical analysis"`
3. **Expected**: Stage = "Technical Review"; Stage History shows the revert entry with the reason, distinguished from forward transitions (e.g., different icon or label)
4. Attempt to revert when stage = Initial Screening
5. **Expected**: Revert button is disabled or absent; attempt to POST returns error

---

## Scenario 6 — Submitter Sees Read-Only Stage Progress (US5 / SC-003)

**Goal**: Submitter can see review stage within 3 clicks of the home page.

**Steps**:
1. Log in as Admin, advance alice's idea to Technical Review
2. Log out, log in as `alice@test.com`
3. From home page: My Ideas (1 click) → View Details on the idea (2 clicks)
4. **Expected**: A read-only "Review Progress" section shows "Technical Review" as the current stage (step 2 of 4); no Advance/Revert buttons visible
5. **Expected**: 3 clicks or fewer from home to seeing the stage — SC-003 satisfied

---

## Scenario 7 — Idea with No Stage Assigned Shows "Pending Review" (US5 / Edge Case)

**Steps**:
1. Submit a new idea as Alice
2. Log in as Admin — verify the idea appears in the admin list with no stage badge
3. Log in as Alice, open the idea detail page
4. **Expected**: Stage section shows "Pending Review" or no stage indicator; no progress bar is filled

---

## Scenario 8 — Stage Filter on Admin List (FR-013 / SC-005)

**Goal**: Admin can filter ideas by review stage; existing filters still work.

**Steps**:
1. Log in as Admin; ensure ideas exist in different stages (one in Technical Review, one in Final Decision)
2. Use the **Review Stage** dropdown filter → select "Technical Review" → click Filter
3. **Expected**: Only ideas in Technical Review stage are shown
4. Combine with Status filter → select Status = "Under Review" + Stage = "Business Impact Assessment" → Filter
5. **Expected**: Combined filter works; existing Status and Category filters unaffected (SC-005)
6. Select Stage = "Draft" (if present) — **Expected**: "Draft" is not available in the stage dropdown

---

## Scenario 9 — Draft Idea Cannot Enter Workflow (FR-012 / SC-007)

**Steps**:
1. As Alice, save a new idea as Draft (do not submit)
2. Log in as Admin — verify the draft does NOT appear in the Review Ideas list
3. Attempt POST to `/Admin/AdvanceStage` with the draft's ID (e.g., via curl)
4. **Expected**: Service returns error "Draft ideas cannot enter the review workflow"; idea stage unchanged

---

## Scenario 10 — History is Append-Only (SC-006)

**Goal**: Stage history cannot be deleted or hidden.

**Steps**:
1. Advance an idea through three stages with notes at each
2. Log in as Admin and open the idea detail page
3. **Expected**: All three stage transitions appear in history, ordered by date ascending; no delete/edit button is visible on any history entry

---

## Regression Test Scenarios

After implementing the multi-stage review workflow, re-verify existing behaviors are unbroken:

| # | Scenario | Expected |
|---|---|---|
| R1 | Submit a new idea directly | Works as before; Status = Submitted; no stage assigned |
| R2 | Admin status update (Submitted → Accepted) via existing UpdateStatus form | Still works; AuditLog entry created; stage workflow not affected |
| R3 | Category-adaptive form on Create | JS behavior unchanged |
| R4 | Draft save and submit flow (Spec 003) | Unaffected; Draft still excluded from admin list |
| R5 | Category filter on admin list | Works alongside new stage filter |
| R6 | Submitter detail page for Draft ideas | "Edit Draft" / "Delete Draft" buttons still visible; no stage section shown |
| R7 | Admin detail page for Accepted/Rejected ideas | Existing AuditLog history still visible; stage history also shown; no workflow action buttons |
