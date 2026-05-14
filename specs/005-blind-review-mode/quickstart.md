# Quickstart & Test Scenarios: Blind Review Mode

**Feature**: `specs/005-blind-review-mode`
**Date**: 2026-05-14

---

## Setup Prerequisites

1. Run the database migration: `dotnet ef database update`
2. Start the application: `dotnet run`
3. Log in as an Admin user
4. Have at least one submitted idea (status: Submitted or Under Review) and one concluded idea (status: Accepted or Rejected)

---

## Manual Test Scenarios

### Scenario 1 — Enable Blind Review Mode (US2, FR-003)

**Steps**:
1. Log in as Admin.
2. Navigate to **Settings → Blind Review**.
3. The page shows current state: "Disabled".
4. Toggle the switch to **Enabled** and click Save.

**Expected**:
- A success banner: "Blind review mode has been enabled."
- The settings page now shows "Enabled" with your admin name and the current timestamp.

---

### Scenario 2 — Admin Idea List Is Anonymised (US1, FR-001, FR-002, SC-001)

**Steps**:
1. Blind review mode is **Enabled** (from Scenario 1).
2. Navigate to **Admin → All Ideas**.

**Expected**:
- An info banner at the top: "Blind review is active — submitter identities are hidden."
- The "Submitted By" column shows **"Anonymous Submitter"** for all ideas with status Submitted or Under Review.
- Ideas with status Accepted or Rejected still show the real submitter name.

---

### Scenario 3 — Admin Idea Detail Is Anonymised (US1, FR-006)

**Steps**:
1. Blind review mode is **Enabled**.
2. Open the detail page of an idea that is **Under Review**.

**Expected**:
- The "Submitted by" field shows **"Anonymous Submitter"**.
- The info banner "Blind review is active…" is visible.
- All review workflow action buttons (Advance Stage, Revert Stage) are still present and functional.

---

### Scenario 4 — Identity Revealed After Final Decision (US3, FR-007, SC-005)

**Steps**:
1. Blind review mode is **Enabled**.
2. Open the detail page of an idea with status **Accepted** or **Rejected**.

**Expected**:
- The "Submitted by" field shows the **real submitter name**.
- The info banner is still visible (blind review mode is still globally on).
- No other functional change.

---

### Scenario 5 — Full Review Workflow Unaffected (US1, FR-009, SC-002)

**Steps**:
1. Blind review mode is **Enabled**.
2. Set an idea to **Under Review** status and advance it through review stages.
3. Record a Final Decision.

**Expected**:
- Every stage advance, revert, and final decision action completes without error.
- Stage transition notes are saved correctly.
- After recording Accepted/Rejected, the submitter's identity becomes visible on the detail page.

---

### Scenario 6 — Submitter View Unaffected (US4, FR-008, SC-004)

**Steps**:
1. Blind review mode is **Enabled**.
2. Log in as a **Submitter**.
3. Navigate to "My Ideas" and open one idea.

**Expected**:
- The submitter sees their own name and all their details exactly as usual.
- No "Anonymous Submitter" label appears anywhere.
- No info banner about blind review appears in submitter views.

---

### Scenario 7 — Disable Blind Review Mode (US2, FR-003)

**Steps**:
1. Log in as Admin.
2. Navigate to **Settings → Blind Review**.
3. Toggle the switch to **Disabled** and click Save.

**Expected**:
- A success banner: "Blind review mode has been disabled."
- Admin idea list immediately shows real submitter names for all ideas.

---

### Scenario 8 — Mid-Review Toggle (Edge Case)

**Steps**:
1. Open an admin idea detail page (Under Review) with blind review **Disabled** — real name is visible.
2. In another tab, navigate to Settings and **Enable** blind review.
3. Return to the first tab and refresh the idea detail page.

**Expected**:
- After refresh, the submitter name is replaced with "Anonymous Submitter".
- No stale data; the change is reflected on the next full page load.

---

### Scenario 9 — Settings Page Shows Last Changed By (US2, FR-005)

**Steps**:
1. Enable blind review as Admin A.
2. Log out, log in as Admin B.
3. Navigate to Settings → Blind Review.

**Expected**:
- The settings page shows: "Last changed by: [Admin A name] on [date/time]."

---

### Scenario 10 — Idea Re-Opened After Decision (Edge Case, FR-007)

**Steps**:
1. Blind review mode is **Enabled**.
2. An idea is Accepted (identity is visible).
3. An admin changes the status back to **Under Review** via the Update Status form.
4. Refresh the idea detail page.

**Expected**:
- The submitter's identity is hidden again ("Anonymous Submitter").
- Blind review re-applies because the status is no longer a concluded state.

---

## Regression Test Scenarios

| # | Regression Area | Test |
|---|---|---|
| R1 | Draft management (Spec 003) | Enable blind review; create and submit a draft — no interference with draft save/submit/delete workflow. |
| R2 | Multi-stage review (Spec 004) | Enable blind review; advance idea through all four stages — all buttons and forms remain functional. |
| R3 | Admin status update | With blind review enabled, use the "Update Status" form on idea detail — status updates correctly. |
| R4 | Attachment download | With blind review enabled, download an attachment from an admin detail view — download still works. |
| R5 | ByStage view | With blind review enabled, navigate to Admin → Browse by Stage — submitter names are anonymised in the filtered list. |
| R6 | Submitter draft edit | Enable blind review; submitter edits and re-submits a draft — no change in submitter workflow. |
| R7 | Settings persistence | Enable blind review, restart the application — blind review mode remains enabled after restart. |
