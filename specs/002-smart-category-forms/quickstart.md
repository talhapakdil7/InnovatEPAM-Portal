# Quickstart: Smart Category-Adaptive Submission Forms

**Feature**: `specs/002-smart-category-forms/spec.md`
**Date**: 2026-05-14
**Prerequisite**: `specs/001-innovation-ideas/quickstart.md` setup complete (app running, users seeded)

---

## Prerequisites

- Application is running locally (`dotnet run` from `src/InnovatEPAM.Portal/`)
- PostgreSQL is running with the existing database
- At least one Submitter account exists
- At least one Admin account exists
- The new EF Core migration (`AddIdeaCategoryFields`) has been applied

## Apply the New Migration

```bash
cd src/InnovatEPAM.Portal
dotnet ef database update
```

Verify: `Category` and `CategoryData` columns appear in the `Ideas` table.

---

## Manual Testing Scenarios

### Scenario 1 — Category Selection Drives Form Adaptation (US1 / P1)

**Precondition**: Logged in as Submitter. Navigate to `/Ideas/Create`.

**Steps**:

1. Observe the form: Title, Description, Attachment fields visible. No category-specific fields visible.
2. Select **"Technical Improvement"** from the Category dropdown.
3. **Expect**: Three new fields appear immediately (no page reload):
   - Technology Area (dropdown: Backend, Frontend, Infrastructure, Security, Data/Analytics, Other)
   - Estimated Implementation Effort (dropdown: Small — days, Medium — weeks, Large — months)
   - Expected Technical Benefit (textarea, max 500 chars)
   - Guidance hints visible below each field.
4. Select **"Process Improvement"** from the Category dropdown.
5. **Expect**: Technical Improvement fields disappear. Three new fields appear:
   - Affected Department or Team (text input, max 100 chars)
   - Current Process Pain Point (textarea, max 500 chars)
   - Estimated Savings (text input, max 200 chars, marked optional)
   - Previously entered Technical Improvement field values are cleared.
6. Select **"Client Solution"** from the Category dropdown.
7. **Expect**: Process Improvement fields disappear. Three new fields appear:
   - Target Client Segment (text input, max 200 chars)
   - Client Problem Being Solved (textarea, max 500 chars)
   - Expected Business Impact (text input, max 300 chars)

**Pass criteria**: All category-specific field sets appear/disappear without page reload. Field values clear on switch.

---

### Scenario 2 — Category Required Validation (US2 / P1)

**Precondition**: On `/Ideas/Create`. No category selected.

**Steps**:

1. Fill in Title only.
2. Click **Submit Idea**.
3. **Expect**: Form is not submitted. Inline error message: "Please select a category."

**Pass criteria**: Submission blocked, error displayed inline next to Category field.

---

### Scenario 3 — Category-Specific Field Validation (US2 / P1)

**Precondition**: On `/Ideas/Create`. Select **"Technical Improvement"**.

**Steps**:

1. Fill Title. Leave **Technology Area** empty. Fill Effort and Benefit.
2. Click **Submit Idea**.
3. **Expect**: Form rejected. Error: "Technology Area is required." visible next to Technology Area field.
4. Fill Technology Area. Leave **Expected Technical Benefit** empty.
5. Click **Submit Idea**.
6. **Expect**: Form rejected. Error: "Expected Technical Benefit is required."
7. Fill all required Technical Improvement fields. Click **Submit Idea**.
8. **Expect**: Idea created. Redirected to idea detail page.

**Pass criteria**: Each required field validated independently with inline error messages.

---

### Scenario 4 — Successful Submission with Technical Improvement (US2 / P1)

**Precondition**: On `/Ideas/Create`. Select **"Technical Improvement"**.

**Steps**:

1. Title: "Improve API Connection Pooling"
2. Description: "Reduce database connection overhead."
3. Technology Area: **Backend**
4. Estimated Implementation Effort: **Medium — weeks**
5. Expected Technical Benefit: "Reduces API latency by 40%."
6. Click **Submit Idea**.

**Expect**:
- Redirected to idea detail page.
- Detail page shows a **"Technical Improvement"** category badge/section.
- Section displays: Technology Area = Backend, Estimated Effort = Medium — weeks, Expected Technical Benefit = "Reduces API latency by 40%."

**Pass criteria**: All fields saved correctly and displayed on detail page.

---

### Scenario 5 — Successful Submission with Process Improvement (optional field) (US2 / P1)

**Precondition**: On `/Ideas/Create`. Select **"Process Improvement"**.

**Steps**:

1. Title: "Automate CI/CD Pipeline"
2. Affected Department or Team: "Engineering"
3. Current Process Pain Point: "Manual deployment takes 3 hours."
4. Estimated Savings: (leave blank)
5. Click **Submit Idea**.

**Expect**: Idea created. Detail page shows Process Improvement section. "Estimated Savings" field is NOT shown (empty optional field).

**Pass criteria**: Optional empty field not displayed on detail page.

---

### Scenario 6 — Submitter Views Category on Detail Page (US3 / P2)

**Precondition**: A Technical Improvement idea exists (from Scenario 4).

**Steps**:

1. Log in as the Submitter who created it.
2. Navigate to `/Ideas` → click **View Details** on the idea.
3. **Expect**: A dedicated "Category" section shows:
   - Category label: "Technical Improvement"
   - Technology Area, Estimated Effort, Expected Technical Benefit values.

**Pass criteria**: All category fields visible on submitter detail page.

---

### Scenario 7 — Admin Views Category on Detail Page (US3 / P2)

**Precondition**: Ideas from multiple categories exist.

**Steps**:

1. Log in as Admin.
2. Navigate to `/Admin`.
3. **Expect**: Each idea row shows a category badge (Technical Improvement / Process Improvement / Client Solution / Uncategorized).
4. Click **Review** on a Technical Improvement idea.
5. **Expect**: Admin detail page shows the category section with all field values.

**Pass criteria**: Category badge visible in list; category section visible in admin detail.

---

### Scenario 8 — Admin Category Filter (US4 / P2)

**Precondition**: Ideas exist across at least two different categories.

**Steps**:

1. Log in as Admin. Navigate to `/Admin`.
2. From the **Category** dropdown filter, select **"Process Improvement"**.
3. Click **Filter**.
4. **Expect**: Only Process Improvement ideas are shown. Ideas of other categories are hidden.
5. Combine with Status filter: select **Status = Submitted** and **Category = Technical Improvement**.
6. **Expect**: Only Submitted Technical Improvement ideas shown.
7. Select a category filter with no matching ideas (e.g., Client Solution if none exist).
8. **Expect**: Empty state message ("No ideas found."), not an error.

**Pass criteria**: Category filter works independently and in combination with status filter. Empty state handled gracefully.

---

### Scenario 9 — Legacy Ideas Backward Compatibility (Edge Case)

**Precondition**: Ideas exist that were created before this feature (no `Category` value).

**Steps**:

1. Navigate to any legacy idea's detail page (as Submitter or Admin).
2. **Expect**: No category section shown, OR a label "Uncategorized" is shown. No errors.
3. Navigate to admin list page.
4. **Expect**: Legacy ideas show "Uncategorized" badge (or no badge). No errors.

**Pass criteria**: Legacy ideas display correctly without runtime errors.

---

### Scenario 10 — Mobile Responsiveness (SC-007)

**Precondition**: Open the Create Idea form on a mobile-size viewport (360px wide).

**Steps**:

1. Resize browser or use DevTools to simulate 360px width.
2. Select a category. Observe category-specific fields.
3. **Expect**: Fields stack vertically. No horizontal scrolling. All labels and inputs remain readable.

**Pass criteria**: Form fully usable at 360px width.

---

## Regression Test Scenarios (FR-014 — Existing Functionality)

Run all scenarios from `specs/001-innovation-ideas/quickstart.md`:

- ✅ User registration and login
- ✅ Creating an idea without category (old form path — should now require category)
  - NOTE: After this feature, ALL new submissions require a category. Legacy ideas (pre-existing) remain unchanged.
- ✅ Admin status update workflow
- ✅ File attachment upload and download
- ✅ Submitter cannot view another submitter's ideas

---

**Version**: 1.0.0 | **Created**: 2026-05-14
