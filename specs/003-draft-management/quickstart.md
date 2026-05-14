# Quickstart & Manual Test Guide: Idea Draft Management

**Feature**: Draft Management (`specs/003-draft-management/spec.md`)
**Date**: 2026-05-14
**Prerequisites**: PostgreSQL running, `dotnet ef database update` applied, app running on `https://localhost:5001`

---

## Setup

1. Register two Submitter accounts: `alice@test.com` / `Admin1234!` and `bob@test.com` / `Admin1234!`
2. Register one Admin account (or seed via database): `admin@test.com` / `Admin1234!`
3. Ensure at least one existing submitted idea exists in the system

---

## Scenario 1 — Save Partially Filled Form as Draft (US1 / SC-001)

**Goal**: Verify draft save works with incomplete data and no validation errors.

**Steps**:
1. Log in as `alice@test.com`
2. Navigate to **Submit New Idea**
3. Select category **Technical Improvement**
4. Fill only **Title**: `"My Draft Idea"`
5. Leave all category fields empty
6. Click **Save as Draft** (second button, distinct from Submit)
7. **Expected**: Redirected to the Edit Draft form, success banner shown, no validation error messages
8. Navigate to **My Ideas**
9. **Expected**: `"My Draft Idea"` appears with a muted "Draft" badge, distinct from other status badges

---

## Scenario 2 — Save Draft with Attachment (US1 / SC-006)

**Goal**: Verify attachment is preserved in draft.

**Steps**:
1. Log in as `alice@test.com`
2. Navigate to **Submit New Idea**
3. Select category **Process Improvement**, fill **Title**: `"Process Draft"`
4. Upload a valid PDF attachment (< 10 MB)
5. Click **Save as Draft**
6. **Expected**: Draft saved with success message
7. Re-open the draft from **My Ideas → Edit**
8. **Expected**: The existing attachment filename is displayed with a Remove checkbox; file can be downloaded

---

## Scenario 3 — View Draft List with Distinct Status (US2 / SC-002)

**Goal**: Submitter can distinguish drafts from submitted ideas within 30 seconds.

**Steps**:
1. Log in as `alice@test.com` (has at least one draft and one submitted idea)
2. Navigate to **My Ideas**
3. **Expected**: Draft ideas show muted "Draft" badge; submitted/reviewed ideas show their respective status badges
4. The draft's last-modified date is visible in the card

---

## Scenario 4 — Delete Draft (US2 / FR-009)

**Goal**: Draft and its attachment are permanently removed.

**Steps**:
1. Log in as `alice@test.com`
2. Identify a draft in **My Ideas**
3. Click **View Details** on the draft → verify "Delete Draft" button is visible
4. Click **Delete Draft** → confirm prompt if shown
5. **Expected**: Redirected to **My Ideas**, success message shown, deleted draft no longer appears
6. If the draft had an attachment, verify the file is no longer accessible via direct URL

---

## Scenario 5 — Continue Editing a Draft (US3 / SC-001)

**Goal**: All previously saved field values are pre-populated.

**Steps**:
1. Log in as `alice@test.com`
2. Open the draft saved in Scenario 1 from **My Ideas → Edit**
3. **Expected**: Category = "Technical Improvement", Title = "My Draft Idea", all other fields blank
4. Fill in **Technology Area**: "Backend", **Estimated Effort**: "Medium — weeks", **Technical Benefit**: "Improve response times"
5. Click **Save as Draft**
6. **Expected**: Success message; form reload shows all three fields pre-populated with new values

---

## Scenario 6 — Replace Attachment on Draft Edit (US3 / FR-007)

**Goal**: Existing attachment can be removed or replaced.

**Steps**:
1. Open a draft that already has an attachment
2. On the Edit form, check **Remove attachment**
3. Upload a new `.docx` file
4. Click **Save as Draft**
5. **Expected**: The old file is removed from storage, new file is associated with the draft
6. Reopen the draft — new filename appears, old filename gone

---

## Scenario 7 — Submit Draft Successfully (US4 / SC-003)

**Goal**: Fully completed draft can be submitted without re-entering data.

**Steps**:
1. Log in as `alice@test.com`
2. Ensure a draft has all required fields filled (Category selected, Title, all category-specific required fields)
3. Open draft via Edit, click **Submit**
4. **Expected**: Redirected to the Idea Detail page with success banner; status shows "Submitted"
5. Log in as Admin → navigate to **Review Ideas**
6. **Expected**: The submitted idea appears in the admin list with correct category badge and all content

---

## Scenario 8 — Submit Draft with Missing Required Fields (US4 / SC-005)

**Goal**: Submission is blocked gracefully with inline errors; draft data is not lost.

**Steps**:
1. Log in as `alice@test.com`
2. Open a draft with **no category selected**
3. Click **Submit**
4. **Expected**: Edit form is re-displayed with inline error: "Please select a category."; no redirect
5. Select **Client Solution** category, fill only **Target Client Segment**, leave others blank
6. Click **Submit**
7. **Expected**: Inline errors on **Client Problem Being Solved** and **Expected Business Impact**; draft data is preserved

---

## Scenario 9 — Admin Cannot See Drafts (US4 / SC-004 / FR-010)

**Goal**: 100% of ideas in admin queue are Submitted or later.

**Steps**:
1. Log in as `alice@test.com` — create 2 drafts, submit none
2. Log in as Admin → navigate to **Review Ideas**
3. **Expected**: Neither draft appears in the admin list
4. Change status filter to every available value — verify "Draft" is not an option in the dropdown
5. Log in as `alice@test.com`, submit one draft
6. Log in as Admin — **Expected**: The submitted idea appears; the remaining draft does not

---

## Scenario 10 — Draft Ownership Isolation (FR-012)

**Goal**: A submitter cannot view or edit another submitter's draft.

**Steps**:
1. Log in as `alice@test.com` — create a draft, note its URL: `/Ideas/Edit/{draftId}`
2. Log out, log in as `bob@test.com`
3. Navigate directly to `alice`'s draft Edit URL
4. **Expected**: 404 Not Found (or redirect to My Ideas with access denied message)
5. Navigate to `/Ideas/DeleteDraft/{draftId}` (POST via form — simulate via curl or browser form)
6. **Expected**: 404 Not Found; alice's draft is unchanged

---

## Regression Test Scenarios

After implementing draft management, re-verify these existing behaviors are unbroken:

| # | Scenario | Expected |
|---|---|---|
| R1 | Submit a new idea directly (no draft) | Works as before; Status = Submitted immediately |
| R2 | Admin view: existing submitted/reviewed ideas appear | Unaffected; Draft filter transparent |
| R3 | Category-adaptive form on Create still shows/hides sections | JS behavior unchanged |
| R4 | Attachment download for submitted ideas | Still works via `/Ideas/Download/{attachmentId}` |
| R5 | Admin status update (Submitted → Accepted) | Unaffected |
| R6 | Submitter detail page for submitted ideas | No "Edit Draft" or "Delete Draft" buttons visible |
