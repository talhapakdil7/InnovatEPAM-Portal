# Tasks: Employee Innovation Ideas Management

**Input**: Design documents from `/specs/001-innovation-ideas/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Manual testing as primary strategy per MVP approach. Unit tests only for critical business logic (auth, file validation, authorization).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **ASP.NET Core MVC**: `src/` at repository root
- **Controllers**: `src/Controllers/`
- **Services**: `src/Services/`
- **Models**: `src/Models/`
- **Views**: `src/Views/`
- **Tests**: `tests/` (if implemented)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create ASP.NET Core MVC project structure per implementation plan
- [X] T002 Initialize ASP.NET Core MVC project with required dependencies (EF Core, Identity, AutoMapper, FluentValidation, Serilog)
- [X] T003 [P] Configure PostgreSQL connection and environment setup
- [X] T004 [P] Setup Bootstrap 5 and responsive UI foundation

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Setup Entity Framework Core with PostgreSQL and migrations framework
- [X] T006 [P] Implement custom ApplicationUser extending IdentityUser with FirstName/LastName properties
- [X] T007 [P] Configure ASP.NET Core Identity with role-based authorization (Submitter/Admin roles)
- [X] T008 [P] Setup AutoMapper profiles for DTO/Model/ViewModel mapping
- [X] T009 [P] Configure FluentValidation for request validation
- [X] T010 [P] Setup Serilog structured logging infrastructure
- [X] T011 [P] Configure global exception handling and error pages
- [X] T012 [P] Setup secure file upload infrastructure (outside web root, validation)
- [X] T013 Create base entities (ApplicationUser, Idea, IdeaAttachment, AuditLog) per data model
- [X] T014 Configure session management (30min normal users, 15min admins)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

## Phase 3: User Story 1 - Secure User Authentication (Priority: P1) 🎯 MVP

**Goal**: Enable employees to register, login, and logout securely with role-based access control

**Independent Test**: Create new accounts, login with correct/incorrect credentials, logout, verify protected page access

### Implementation for User Story 1

- [X] T015 [P] [US1] Create RegisterViewModel and LoginViewModel in src/ViewModels/AuthViewModels.cs
- [X] T016 [P] [US1] Create RegisterValidator and LoginValidator using FluentValidation in src/Validators/
- [X] T017 [US1] Implement AuthService with RegisterAsync and LoginAsync methods in src/Services/AuthService.cs
- [X] T018 [US1] Implement AuthController with Register and Login actions in src/Controllers/AuthController.cs
- [X] T019 [US1] Create registration and login Razor views in src/Views/Auth/
- [X] T020 [US1] Configure ASP.NET Core Identity authentication middleware and routing
- [X] T021 [US1] Add logout functionality and session management

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

## Phase 4: User Story 2 - Submitter Creates Innovation Ideas (Priority: P1)

**Goal**: Enable submitters to create innovation ideas with file attachments

**Independent Test**: Login as submitter, create ideas with/without attachments, verify validation and storage

### Implementation for User Story 2

- [X] T022 [P] [US2] Create Idea and IdeaAttachment entities in src/Models/
- [X] T023 [P] [US2] Create CreateIdeaViewModel and IdeaAttachmentViewModel in src/ViewModels/
- [X] T024 [P] [US2] Create CreateIdeaValidator with file upload validation in src/Validators/
- [X] T025 [US2] Implement IdeaService with CreateIdeaAsync method in src/Services/IdeaService.cs
- [X] T026 [US2] Implement IdeasController with Create action in src/Controllers/IdeasController.cs
- [X] T027 [US2] Create create idea Razor view with file upload in src/Views/Ideas/Create.cshtml
- [X] T028 [US2] Implement secure file upload handling and storage outside web root

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

## Phase 5: User Story 3 - Submitter Views Ideas and Tracks Status (Priority: P1)

**Goal**: Enable submitters to view their ideas list and track status changes

**Independent Test**: Login as submitter, view ideas list with filtering, click to view details, verify status updates

### Implementation for User Story 3

- [X] T029 [P] [US3] Create IdeaListViewModel and IdeaDetailViewModel in src/ViewModels/
- [X] T030 [US3] Implement GetMyIdeasAsync and GetIdeaDetailAsync in IdeaService
- [X] T031 [US3] Implement IdeasController Index and Detail actions
- [X] T032 [US3] Create ideas list Razor view with status filtering in src/Views/Ideas/Index.cshtml
- [X] T033 [US3] Create idea detail Razor view in src/Views/Ideas/Detail.cshtml
- [X] T034 [US3] Add status display and responsive UI components

**Checkpoint**: All P1 user stories should now be independently functional

## Phase 6: User Story 4 - Administrator Reviews Innovation Ideas (Priority: P2)

**Goal**: Enable administrators to view all submitted ideas for review

**Independent Test**: Login as admin, view all ideas list with submitter info, filter by status, view details with attachments

### Implementation for User Story 4

- [X] T035 [P] [US4] Create AdminIdeasListViewModel and AdminIdeaDetailViewModel in src/ViewModels/
- [X] T036 [US4] Implement GetAllIdeasForReviewAsync in IdeaService
- [X] T037 [US4] Implement AdminController with IdeasList and IdeaDetail actions
- [X] T038 [US4] Create admin ideas list view with status summary in src/Views/Admin/Index.cshtml
- [X] T039 [US4] Create admin idea detail view with attachment preview in src/Views/Admin/Detail.cshtml
- [X] T040 [US4] Configure admin role authorization on admin routes

**Checkpoint**: User Story 4 should be independently functional

## Phase 7: User Story 5 - Administrator Updates Idea Status (Priority: P2)

**Goal**: Enable administrators to update idea status through review workflow

**Independent Test**: Login as admin, change idea status from Submitted to Under Review to Accepted/Rejected, verify audit logging

### Implementation for User Story 5

- [X] T041 [P] [US5] Create AuditLog entity and status update DTOs in src/Models/
- [X] T042 [US5] Implement UpdateIdeaStatusAsync with audit logging in IdeaService
- [X] T043 [US5] Implement AdminController UpdateStatus action with validation
- [X] T044 [US5] Add status update form to admin detail view
- [X] T045 [US5] Implement audit logging for status changes
- [X] T046 [US5] Add status change notifications to submitter views

**Checkpoint**: All user stories should now be independently functional

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T047 [P] Documentation updates and XML comments per Constitution Principle II
- [X] T048 Code cleanup and consistent naming conventions
- [ ] T049 [P] Manual testing validation per quickstart.md scenarios
- [X] T050 Security hardening and input validation review
- [X] T051 Performance optimization and responsive UI improvements
- [ ] T052 Final integration testing and bug fixes

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - P1 stories (US1-US3) can proceed in parallel after Phase 2
  - P2 stories (US4-US5) can proceed after P1 stories or in parallel if staffed
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 3 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - Independent of other stories
- **User Story 5 (P2)**: Can start after Foundational (Phase 2) - Independent of other stories

### Within Each User Story

- ViewModels and Validators before Services
- Services before Controllers
- Controllers before Views
- Core implementation before integration features

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- ViewModels and Validators within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

## Parallel Example: User Story 1

**Team of 2 developers can work simultaneously**:

**Developer A**:

- T015 [P] [US1] Create RegisterViewModel and LoginViewModel
- T016 [P] [US1] Create RegisterValidator and LoginValidator
- T019 [US1] Create registration and login Razor views

**Developer B**:

- T017 [US1] Implement AuthService with RegisterAsync and LoginAsync methods
- T018 [US1] Implement AuthController with Register and Login actions
- T020 [US1] Configure ASP.NET Core Identity authentication middleware
- T021 [US1] Add logout functionality

**Result**: User Story 1 complete in ~2-3 days instead of 4-5 days

## Implementation Strategy

**MVP First**: Complete all P1 user stories (US1-US3) for core functionality, then add P2 features (US4-US5)

**Incremental Delivery**: Each user story delivers independent value and can be deployed separately

**Manual Testing Focus**: Use quickstart.md scenarios for comprehensive workflow validation

**Clean Architecture**: Maintain separation between Controllers (thin), Services (business logic), Models (data), Views (presentation)</content>
<parameter name="filePath">/Users/talhapakdil/Desktop/InnovatEPAM Portal/specs/001-innovation-ideas/tasks.md
