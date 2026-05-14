<!--
SYNC IMPACT REPORT
Version: 1.0.0 (INITIAL)
Date: 2026-05-14
Principles Created: 11 core governance principles
Sections Added: Architecture & Design, Security, Development Workflow, Testing & Quality
Templates to Update: plan-template.md (architecture checks), spec-template.md (security/testing sections), tasks-template.md (task categorization)
Status: ✅ Constitution ratified | ⚠ Dependent templates pending alignment review
-->

# InnovatEPAM Portal Constitution

## Core Principles

### I. Clean Architecture

Controllers remain thin, delegating all business logic to service layers. Service layer handles domain rules, validation, and orchestration. Repository pattern enforces data access abstraction. Controllers MUST NOT contain business logic—violations require refactoring before merge. Data flows cleanly through layers: Controller → Service → Repository → Database.

### II. Maintainable ASP.NET Core MVC Development

All code follows ASP.NET Core conventions and best practices. Project structure mirrors responsibility: Controllers/, Services/, Repositories/, Models/, ViewModels/, Views/ are organized consistently across all features. Naming conventions are strict: PascalCase for classes/namespaces, camelCase for variables/parameters. Folder structures remain consistent—no exceptions for "special cases."

### III. Service-Layer Business Logic Separation

Core business logic lives exclusively in service classes. Services are stateless, testable, and reusable. No business logic in controllers, views, or data access layer. Service methods document pre/post conditions and side effects via XML comments. Each service MUST have a single responsibility.

### IV. Secure Authentication and Role-Based Authorization

Authentication is centralized via ASP.NET Core Identity or equivalent. All protected endpoints declare required roles/claims via [Authorize] attributes. Authorization is role-based; claim-based rules are explicit in services. Password policies enforce complexity; credentials are never logged. Sessions and tokens are time-limited; refresh logic is centralized.

### V. Scalable Phased Feature Development

Features are planned in phases with clearly defined scope and dependencies. Each phase is independently deliverable and testable. Phase gates require specification approval before implementation. Database schema changes are versioned and non-breaking when possible. Feature flags control rollout to enable gradual deployment.

### VI. Secure File Upload Validation

All file uploads validate: MIME type (via content inspection, not extension), file size (strict limits enforced), and scan for malware. Uploaded files are stored outside web root with hashed names. File access is controlled via permission checks—users cannot access files outside their scope. Temporary files are cleaned up within 24 hours. Upload endpoints require authentication and role authorization.

### VII. Workflow-Driven Review Systems

All significant work (specs, PRs, releases) flows through a defined review process. Reviewers have explicit responsibilities documented. Review checklists ensure consistency. Feedback is actionable; approval gates are enforced. Workflow state is transparent and auditable. No bypassing review processes.

### VIII. Responsive and Consistent User Experience

User interface remains responsive—long operations use background tasks and progress indicators. Design is consistent: buttons, colors, spacing, typography follow a shared component library. Mobile-first responsive design is required. Accessibility (WCAG 2.1 AA) is non-negotiable. Error messages are clear, actionable, and non-technical for end users.

### IX. Structured Error Handling

All exceptions are caught, logged, and handled explicitly. No silent failures. Custom exception types reflect domain concerns. Logging includes context: user, operation, timestamp, error details. Error pages are user-friendly; detailed errors appear only in logs. Retry logic is explicit and time-bound. Database connection errors trigger circuit breaker logic.

### X. Specification-Driven Development Over Vibe Coding

Every feature begins with a written specification. Specifications define: requirements, constraints, acceptance criteria, and data models. Code is written to satisfy specs—not guesses about intent. Specs are approved before coding starts. Deviations from spec require specification amendment and approval. "This seemed like a good idea" is not sufficient justification.

### XI. Manual Testing of All Core Workflows

All core workflows are manually tested before release. Manual testing documents: user steps, expected outcomes, and confirmation of success. Automated tests complement but do not replace manual testing of user-facing workflows. Test results are recorded in test matrices. Regression testing includes previously-broken workflows.

## Architecture & Design Standards

**Layered Architecture Enforcement**: Projects MUST follow Controller → Service → Repository → Data Access patterns. Projects MUST NOT implement business logic in Entity Framework configurations, Controllers, or Views. Service layer MUST be the exclusive owner of business rules and domain logic.

**XML Documentation**: All public classes, interfaces, methods, and properties MUST have XML documentation comments. Documentation MUST include: purpose, parameters, return values, and notable side effects. Internal and private members SHOULD have documentation when logic is non-obvious.

**Naming and Folder Consistency**: Folder structure MUST match responsibility boundaries (Controllers/, Services/, Repositories/, Models/, ViewModels/, Views/). Naming conventions: PascalCase for types/members, camelCase for variables. No abbreviated names unless universally understood (e.g., `id`, `url`). Consistency is verified in code review.

## Security Requirements

**Authentication & Authorization**: All endpoints requiring data access MUST declare [Authorize] attributes with explicit role/claim requirements. Password complexity MUST meet OWASP guidelines. Session timeouts are 30 minutes for normal users, 15 minutes for administrative access. Multi-factor authentication is required for administrative operations.

**File Upload Security**: File uploads validate MIME type via content inspection. Maximum upload size enforced at application and IIS levels. Uploaded files are scanned for malware. Files are stored outside web root with hashed names. Access to uploaded files is controlled via service-layer permission checks.

**Data Protection**: Sensitive data (passwords, tokens, PII) is never logged. Database connections use encrypted credentials. API responses sanitize user input to prevent XSS attacks. HTTPS is enforced site-wide; no mixed content permitted.

## Development Workflow

**Specification-First Process**: Feature development begins with written specification. Specifications are reviewed and approved before implementation. Code changes that deviate from approved specification require specification amendment and approval. Pull request descriptions reference the specification document.

**Code Review Discipline**: All code merges require peer review. Reviewers verify: specification compliance, architecture adherence, test coverage, documentation completeness. Code review feedback is resolved before merge. Trivial changes (typos, comments) may bypass review with maintainer discretion.

**Phased Rollout**: Features are delivered in phases. Each phase is independently testable and completable. Phase gates require sign-off before proceeding. Database migrations are tested in non-production before production deployment.

## Testing & Quality Standards

**Comprehensive Manual Testing**: All core workflows are manually tested. Test cases document: preconditions, steps, expected outcomes, and pass/fail criteria. Regression testing includes previously-discovered bugs. Test matrix is maintained and visible to stakeholders.

**Automated Test Expectations**: Unit tests cover service layer logic. Integration tests verify data access and workflow orchestration. Test coverage minimum: 70% on service layer, 50% overall. Tests document assumptions and edge cases.

## Governance

**Constitution as Law**: This constitution supersedes all prior practices and conventions. When practices conflict with constitution principles, constitution wins. All team members are responsible for upholding these principles.

**Amendment Process**: Changes to this constitution require: (1) written proposal documenting rationale, (2) review and discussion, (3) consensus approval, (4) documentation of migration plan for existing non-compliant code. Version numbers follow semantic versioning: MAJOR for principle removals, MINOR for new principles, PATCH for clarifications.

**Compliance Verification**: Pull request reviews verify constitution compliance. Architecture reviews occur quarterly. Non-compliance is documented and tracked; no work merges without remediation plan. Maintainers have authority to enforce these principles.

**Version**: 1.0.0 | **Ratified**: 2026-05-14 | **Last Amended**: 2026-05-14
