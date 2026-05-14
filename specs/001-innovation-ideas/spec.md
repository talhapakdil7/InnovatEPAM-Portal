# Feature Specification: Employee Innovation Ideas Management

**Feature Branch**: `001-innovation-ideas`

**Created**: 2026-05-14

**Status**: Draft

**Input**: Build an employee innovation management portal where users can register, log in, and log out securely. The system must support role-based access control between submitters and administrators. Submitters can create innovation ideas with file attachments, view submitted ideas, and track statuses including submitted, under review, accepted, and rejected. Administrators can review ideas, update statuses, and manage the evaluation workflow. The application should provide organized listing and detail pages for ideas with a responsive and user-friendly experience.

## User Scenarios & Testing

### User Story 1 - Secure User Authentication (Priority: P1)

Employees need to access the innovation portal securely. New employees register with email and password; existing employees log in with credentials; users can log out safely. Authentication is a foundational requirement—no feature works without it.

**Why this priority**: Authentication is the foundation for all role-based access and user identification. Without secure login/logout, the entire system is unusable and insecure.

**Independent Test**: Can be tested independently by creating new accounts, logging in, accessing protected pages, and logging out. Delivers value by enabling secure access for all subsequent features.

**Acceptance Scenarios**:

1. **Given** a new employee on the registration page, **When** they enter email, password (and confirm), **Then** an account is created and they can log in immediately
2. **Given** a registered employee on the login page, **When** they enter correct credentials, **Then** they are authenticated and can access the portal
3. **Given** a registered employee on the login page, **When** they enter incorrect credentials, **Then** they see "Invalid email or password" error
4. **Given** a logged-in employee, **When** they click logout, **Then** their session ends and they are redirected to the login page
5. **Given** a logged-out user, **When** they try to access a protected page (e.g., /ideas), **Then** they are redirected to the login page

---

### User Story 2 - Submitter Creates Innovation Ideas (Priority: P1)

Employees (submitters) need to propose innovation ideas. They should create a new idea with title, description, and optional file attachments. Ideas are immediately recorded in the system. This is the core submitter value.

**Why this priority**: Idea submission is the primary submitter action. Without this, the portal cannot collect innovations from employees.

**Independent Test**: Can be tested by submitter accounts creating ideas with/without attachments, verifying they are stored in the system. Delivers core business value of capturing innovation proposals.

**Acceptance Scenarios**:

1. **Given** a logged-in submitter on the create idea page, **When** they enter title and description, **Then** the idea is saved with status "Submitted"
2. **Given** a logged-in submitter on the create idea page, **When** they upload a valid file (PDF, DOC, image), **Then** the file is attached to the idea
3. **Given** a logged-in submitter on the create idea page, **When** they leave the title empty, **Then** they see "Title is required" error
4. **Given** a logged-in submitter on the create idea page, **When** they upload a file larger than the size limit, **Then** they see "File exceeds maximum size" error

---

### User Story 3 - Submitter Views Ideas and Tracks Status (Priority: P1)

Submitters need to see all their submitted ideas and monitor their status. Ideas list shows title, submission date, and current status (Submitted, Under Review, Accepted, Rejected). Submitters can click on an idea to see full details. This provides feedback loop for submitters.

**Why this priority**: Submitters must see their own ideas and understand where they are in the review process. Provides motivation and transparency.

**Independent Test**: Can be tested by viewing the ideas list, filtering/sorting, viewing detail pages. Delivers transparency and trust in the evaluation process.

**Acceptance Scenarios**:

1. **Given** a logged-in submitter on the ideas list page, **When** they view the page, **Then** they see all their submitted ideas with title, date, and status
2. **Given** a logged-in submitter on the ideas list page, **When** they click on an idea, **Then** they see the full idea details: title, description, attachments, current status, and submission date
3. **Given** a logged-in submitter with multiple ideas, **When** the admin updates an idea status, **Then** the submitter sees the updated status in their list on next page load
4. **Given** a logged-in submitter viewing an idea detail, **When** the status is "Accepted" or "Rejected", **Then** they see a message with the decision

---

### User Story 4 - Administrator Reviews Innovation Ideas (Priority: P2)

Administrators need to review submitted ideas to evaluate them. Admin page shows all ideas (from all submitters) in a list with title, submitter, submission date, and current status. Admins can click to view full idea details including description, attachments, and submission metadata.

**Why this priority**: Admins cannot update status without viewing ideas first. This is blocking for the review workflow.

**Independent Test**: Can be tested by admin account viewing ideas list and detail pages. Delivers admin visibility into all submissions.

**Acceptance Scenarios**:

1. **Given** a logged-in administrator on the admin ideas page, **When** they view the page, **Then** they see all submitted ideas (from all submitters) with title, submitter name, date, and status
2. **Given** a logged-in administrator on the admin ideas page, **When** they click on an idea, **Then** they see full details: title, description, submitter name, attachments, and submission date
3. **Given** a logged-in administrator viewing an idea detail, **When** they see an attached file, **Then** they can download or preview it
4. **Given** a logged-out user or submitter, **When** they try to access the admin review page, **Then** they are denied access

---

### User Story 5 - Administrator Updates Idea Status (Priority: P2)

Administrators need to evaluate and track ideas through the review workflow. On the idea detail page, admins can change status from "Submitted" → "Under Review" → "Accepted" or "Rejected". Status changes are immediately reflected in the system and visible to the submitter.

**Why this priority**: Status management is the core admin responsibility and enables the review workflow to function.

**Independent Test**: Can be tested by changing statuses, verifying they persist and update in both admin and submitter views. Delivers the core review workflow.

**Acceptance Scenarios**:

1. **Given** a logged-in administrator viewing an idea detail page, **When** they see the status dropdown, **Then** they can select "Under Review", "Accepted", or "Rejected"
2. **Given** an idea with status "Submitted", **When** admin changes it to "Under Review", **Then** the status is saved and the idea appears under "Under Review" in the admin list
3. **Given** an idea with status "Under Review", **When** admin changes it to "Accepted", **Then** the idea appears as accepted and submitter sees the decision
4. **Given** an idea with status "Under Review", **When** admin changes it to "Rejected", **Then** the idea appears as rejected and submitter sees the decision

---

### Edge Cases

- What happens when a submitter tries to edit or delete their idea after it moves to "Under Review"? (Assumption: Read-only after submission)
- How does the system handle file uploads if the file server is temporarily unavailable? (Assumption: Return user-friendly error; retry available)
- Can an administrator undo a status change? (Assumption: Yes, status can be changed back at any time during review)
- What happens if a submitter tries to upload a .exe or other executable file? (File validation prevents it)

## Requirements

### Functional Requirements

- **FR-001**: System MUST authenticate users via email/password with secure session management
- **FR-002**: System MUST support user registration with email validation
- **FR-003**: System MUST enforce role-based access control: Submitters see only their ideas; Administrators see all ideas
- **FR-004**: System MUST allow submitters to create ideas with title, description, and optional file attachments
- **FR-005**: System MUST store idea files securely with validated file types (PDF, DOC, DOCX, XLS, XLSX, JPG, PNG)
- **FR-006**: System MUST reject files larger than 10MB per file
- **FR-007**: System MUST track idea status with four states: Submitted, Under Review, Accepted, Rejected
- **FR-008**: System MUST allow administrators to view all submitted ideas and update their status
- **FR-009**: System MUST provide submitters with a list of their submitted ideas with filtering by status
- **FR-010**: System MUST provide idea detail pages showing title, description, submitter, submission date, status, and attachments
- **FR-011**: System MUST log all status changes with timestamp and admin user who made the change
- **FR-012**: System MUST enforce logout and session expiration after inactivity
- **FR-013**: System MUST validate all user input to prevent XSS and SQL injection attacks
- **FR-014**: System MUST provide user-friendly error messages for validation failures

### Key Entities

- **User**: Represents an employee. Attributes: ID, Email, PasswordHash, FirstName, LastName, Role (Submitter/Admin), CreatedDate. Relationships: One-to-Many with Ideas (submitter).
- **Idea**: Represents an innovation proposal. Attributes: ID, Title, Description, Status (Submitted/Under Review/Accepted/Rejected), SubmitterId, CreatedDate, LastModifiedDate, LastModifiedByAdminId. Relationships: Many-to-One with User (submitter), Many-to-One with User (admin who last updated), One-to-Many with IdeaAttachments.
- **IdeaAttachment**: Represents a file attached to an idea. Attributes: ID, IdeaId, FileName, FilePath, FileSize, UploadedDate. Relationships: Many-to-One with Idea.
- **AuditLog**: Represents status change history. Attributes: ID, IdeaId, OldStatus, NewStatus, ChangedByAdminId, ChangedDate. For manual testing and admin review.

## Success Criteria

### Measurable Outcomes

- **SC-001**: New users can complete registration and first login in under 3 minutes
- **SC-002**: Submitters can create and submit an idea (with attachment) in under 5 minutes
- **SC-003**: Administrators can view all submitted ideas and update status for one idea in under 2 minutes
- **SC-004**: System handles 100 concurrent users without performance degradation (based on manual load testing)
- **SC-005**: All core workflows (authentication, idea submission, status review) are manually tested and documented before release
- **SC-006**: 100% of user-facing workflows tested manually; critical business logic (auth, file validation, status changes) have unit tests
- **SC-007**: No security vulnerabilities in authentication, file upload, or data access (verified via manual security review)

## Assumptions

- **Scope**: Mobile app support is out of scope for MVP. Web application only (desktop/tablet responsive).
- **Authentication**: Email/password only. Single sign-on (SSO) and multi-factor authentication are Phase 2 features.
- **File Storage**: Files are stored on the application server within the project directory (not cloud storage). Phase 2 will evaluate moving to cloud storage.
- **Notification**: Email notifications for status changes are out of scope for MVP. Status changes visible only when submitter logs in.
- **Search**: Full-text search is out of scope for MVP. Phase 2 feature. MVP provides basic list view with status filtering.
- **Submitter Editing**: Submitters cannot edit ideas after creation. Status is immutable by submitters (read-only after submission).
- **Admin Users**: Admin accounts are created manually by system administrator. No self-service admin signup.
- **File Types**: Supported formats are Office documents (DOC, DOCX, XLS, XLSX), PDFs, and images (JPG, PNG). Executable files, scripts (.exe, .bat, .ps1), and archives (.zip) are rejected.
- **Testing**: The project prioritizes manual testing of all core workflows. Unit tests are required for critical business logic only (authentication, authorization, file validation). Integration tests are Phase 2.
