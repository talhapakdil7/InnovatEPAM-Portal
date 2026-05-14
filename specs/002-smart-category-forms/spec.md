# Feature Specification: Smart Category-Adaptive Submission Forms

**Feature Branch**: `002-smart-category-forms`

**Created**: 2026-05-14

**Status**: Draft

**Input**: User description: "Add smart submission forms that dynamically adapt based on the selected innovation category. Different categories such as technical improvement, process improvement, and client solution should display relevant fields, guidance, and validation rules. The dynamic form experience should remain responsive, user-friendly, and fully compatible with the existing idea submission workflow without breaking previous features."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Submitter Selects a Category and Sees Adapted Form (Priority: P1)

A logged-in employee starts submitting an innovation idea. As a first step they choose one of the available innovation categories (Technical Improvement, Process Improvement, or Client Solution). The form immediately adapts: a new set of category-specific fields and contextual guidance hints appear below the common fields, while irrelevant fields disappear. The employee fills in the tailored fields, attaches an optional file, and submits the idea — all within the same page and workflow they already know.

**Why this priority**: This is the core value of the feature. Without category selection driving form adaptation, the entire feature has no effect. Every other user story depends on at least one category being rendered correctly.

**Independent Test**: Can be fully tested by logging in as a Submitter, navigating to the Submit Idea page, selecting each category in turn, and verifying the correct fields appear/disappear — without any admin interaction needed.

**Acceptance Scenarios**:

1. **Given** a logged-in Submitter is on the Submit Idea page, **When** they select "Technical Improvement", **Then** the form shows fields: Technology Area (dropdown), Estimated Implementation Effort (Small/Medium/Large), and Expected Technical Benefit (text area), along with guidance text explaining what information to provide for each field.
2. **Given** a logged-in Submitter is on the Submit Idea page, **When** they select "Process Improvement", **Then** the form shows fields: Affected Department/Team (text), Current Process Pain Point (text area), and Estimated Savings (time or cost, text), along with relevant guidance hints.
3. **Given** a logged-in Submitter is on the Submit Idea page, **When** they select "Client Solution", **Then** the form shows fields: Target Client Segment (text), Client Problem Being Solved (text area), and Expected Business Impact (text), along with relevant guidance hints.
4. **Given** a Submitter has selected a category and filled in category-specific fields, **When** they switch to a different category, **Then** the previous category-specific fields are replaced by the new category's fields, and any data entered in the previous category's specific fields is cleared.
5. **Given** a Submitter has not yet selected any category, **When** they attempt to submit the form, **Then** the system prevents submission and displays a validation message asking them to select a category.

---

### User Story 2 — Category-Specific Validation Enforces Required Fields (Priority: P1)

When a Submitter submits the form, the system validates that all required category-specific fields are filled in. Fields that are optional remain optional. Validation errors are displayed inline, next to the relevant field, with clear messages guiding the Submitter to correct their input before re-submitting.

**Why this priority**: Without validation, the benefit of category-specific data collection collapses — incomplete or incorrect data would reach admins. This story has equal priority to Story 1 because both ship together to be useful.

**Independent Test**: Can be fully tested by selecting each category and attempting to submit with missing required fields, verifying that appropriate per-field error messages appear and the form is not submitted.

**Acceptance Scenarios**:

1. **Given** a Submitter has selected "Technical Improvement" and left the Technology Area field empty, **When** they submit, **Then** the form is not submitted and an error message appears next to Technology Area field.
2. **Given** a Submitter has selected "Process Improvement" and filled all required category fields, **When** they submit, **Then** the idea is created successfully including the category and category-specific field values.
3. **Given** a Submitter has selected "Client Solution" and provided all required fields, **When** they submit, **Then** the idea is saved with all category data and the Submitter is redirected to the idea detail page showing the category information.

---

### User Story 3 — Submitters and Admins View Category Information on Idea Detail Pages (Priority: P2)

After an idea is submitted with a category, the category label and all category-specific field values are visible on the idea detail page for both the Submitter (viewing their own idea) and the Admin (during review). The category information is displayed in a clear, readable format alongside the existing idea details.

**Why this priority**: Viewing the captured category data completes the data lifecycle and gives admins the context needed for better-informed status decisions. It is P2 because the form adaptation (P1) must exist first.

**Independent Test**: Can be fully tested by submitting an idea with a specific category, then viewing the idea detail page as the same Submitter and separately as an Admin, and verifying all category-specific field values are correctly displayed.

**Acceptance Scenarios**:

1. **Given** an idea was submitted under "Technical Improvement" with specific field values, **When** the Submitter views the idea detail page, **Then** they see the category label "Technical Improvement" and all provided field values (Technology Area, Estimated Effort, Expected Technical Benefit).
2. **Given** an idea was submitted under "Process Improvement", **When** an Admin opens the review detail page, **Then** they see the category label and the category-specific fields displayed in a dedicated section.
3. **Given** ideas submitted under different categories exist, **When** an Admin views the ideas list, **Then** each idea's category is visible in the list (as a label or column), allowing quick visual grouping.

---

### User Story 4 — Admin Can Filter Ideas by Category (Priority: P2)

On the admin ideas list page, an Admin can filter the displayed ideas by innovation category. The filter works alongside the existing status filter, allowing combined filtering (e.g., all "Accepted" ideas in the "Technical Improvement" category).

**Why this priority**: Adds operational efficiency for admins reviewing large volumes of ideas. P2 because it enhances the review workflow but is not blocking for the core feature.

**Independent Test**: Can be fully tested by having multiple ideas across different categories in the system and applying the category filter on the admin list page, verifying only the matching ideas are shown.

**Acceptance Scenarios**:

1. **Given** ideas exist across multiple categories, **When** an Admin selects the "Process Improvement" category filter, **Then** only ideas of that category are shown in the list.
2. **Given** an Admin has applied a category filter of "Technical Improvement" and a status filter of "Submitted", **When** they view the list, **Then** only ideas matching both criteria are displayed.
3. **Given** no ideas exist for a selected category filter, **When** the Admin applies that filter, **Then** the list shows an empty state message (not an error).

---

### Edge Cases

- What happens when a user navigates away mid-form after selecting a category and partially filling category fields? The browser's standard "leave page?" warning is sufficient; no special persistence is required.
- What happens with existing ideas that were created before the category feature existed? They are displayed without a category label (shown as "Uncategorized") and no category-specific fields are shown for them.
- What happens if a new category is added in the future? The system should be extensible, but adding categories requires a developer change; no self-service admin category management is in scope.
- What happens if a Submitter submits with a category but leaves an optional category field empty? The idea is saved successfully; empty optional fields are simply not displayed on the detail page.
- What happens on a mobile screen where the form is long? The category-specific fields stack vertically and the form remains scrollable and usable on small screens.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The submission form MUST include a category selector as the first input, before the Title field, so that the correct dynamic fields load before the Submitter writes their idea details.
- **FR-002**: System MUST support exactly three initial categories: Technical Improvement, Process Improvement, and Client Solution.
- **FR-003**: When a category is selected, the form MUST dynamically display the category-specific fields and guidance text without requiring a full page reload.
- **FR-004**: When a category is changed, the form MUST replace the previous category-specific fields with the new ones and MUST clear any data entered in the previous set of category-specific fields.
- **FR-005**: System MUST prevent form submission if no category is selected, displaying an inline validation message.
- **FR-006**: System MUST enforce required category-specific field validation per category (see field definitions below) and display inline error messages adjacent to the failing field.
- **FR-007**: System MUST persist the selected category and all category-specific field values when an idea is submitted.
- **FR-008**: Idea detail pages for Submitters MUST display the idea's category label and all non-empty category-specific field values in a dedicated section.
- **FR-009**: Admin review detail pages MUST display the idea's category label and all non-empty category-specific field values in a dedicated section.
- **FR-010**: The admin ideas list MUST display a category label/badge per idea row.
- **FR-011**: The admin ideas list MUST provide a category filter, combinable with the existing status filter.
- **FR-012**: Ideas submitted before this feature existed MUST display as "Uncategorized" without errors.
- **FR-013**: The dynamic form behavior MUST work correctly on mobile and tablet screen sizes (responsive).
- **FR-014**: All existing idea submission, listing, detail viewing, and admin review functionality MUST continue to work unchanged after this feature is introduced.

### Category-Specific Field Definitions

**Technical Improvement**:
- Technology Area (required, select from: Backend, Frontend, Infrastructure, Security, Data/Analytics, Other)
- Estimated Implementation Effort (required, select from: Small — days, Medium — weeks, Large — months)
- Expected Technical Benefit (required, free text, max 500 characters)

**Process Improvement**:
- Affected Department or Team (required, free text, max 100 characters)
- Current Process Pain Point (required, free text, max 500 characters)
- Estimated Savings (optional, free text, max 200 characters — time or cost estimate)

**Client Solution**:
- Target Client Segment (required, free text, max 200 characters)
- Client Problem Being Solved (required, free text, max 500 characters)
- Expected Business Impact (required, free text, max 300 characters)

### Key Entities

- **IdeaCategory**: Represents an innovation category. Has a name (string), a display label, and an ordered list of category-specific field definitions.
- **CategoryField**: Represents a single field definition within a category — field name, label, type (text/select), options (for select), required flag, character limit, and guidance hint text.
- **Idea** *(extended)*: The existing Idea entity gains a Category property (nullable string for backward compatibility) and a CategoryData property (structured key-value store of category-specific answers).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A Submitter can select a category and see the adapted form fields appear in under 1 second, without a page reload.
- **SC-002**: A Submitter can complete the full idea submission process with category-specific fields in under 5 minutes from form open to confirmation page.
- **SC-003**: 100% of submitted ideas include a valid category value; the system rejects all submissions without a selected category.
- **SC-004**: All category-specific field values are correctly displayed on idea detail and admin review pages for 100% of ideas submitted after this feature is introduced.
- **SC-005**: The admin category filter correctly isolates ideas by category with no false positives or missed results.
- **SC-006**: All existing test scenarios from the original idea submission feature (registration, login, idea creation without category fields — legacy path) continue to pass after this feature is deployed.
- **SC-007**: The dynamic form is fully usable on screens 360 px wide and above without horizontal scrolling.

---

## Assumptions

- The existing ASP.NET Core MVC idea submission page will be extended; no separate new page will be created for category-based submission.
- Dynamic field display/hide behavior is achieved client-side (JavaScript) without server round-trips; category-specific fields are rendered in the page but shown/hidden based on the selection.
- Category definitions (field names, labels, options, required flags) are defined in code/configuration for the MVP; no admin UI for category management is in scope.
- The three categories (Technical Improvement, Process Improvement, Client Solution) are sufficient for MVP. Adding new categories in a future phase will require a developer change.
- Category and category-specific data are stored within the existing Idea record structure; no separate database table is required for MVP (JSON/string storage is acceptable).
- Existing ideas with no category assigned are backward-compatible and shown as "Uncategorized" — no data migration is needed.
- File attachment behavior is unchanged; category selection does not affect file upload rules (10 MB limit, allowed types).
- Session timeouts, role-based access, and authentication flows are unchanged by this feature.
- The Submitter role cannot change the category after submission (read-only, consistent with the existing no-edit-after-submit rule).
