# Research Document: Technical Investigation & Decisions

**Feature**: Employee Innovation Ideas Management Portal  
**Date**: 2026-05-14  
**Phase**: 0 - Technical Research  
**Input**: plan.md Technical Context + implementation arguments

---

## Research Task 1: ASP.NET Core Identity Customization

**Objective**: Extend ASP.NET Core Identity to support FirstName/LastName and custom role enum (Submitter/Admin)

### Investigation

ASP.NET Core Identity provides `IdentityUser` as the base class. By default, it includes:

- UserName, Email, EmailConfirmed, PhoneNumber, PasswordHash, SecurityStamp
- Lacks: FirstName, LastName (common requirement)

**Options Evaluated**:

1. **Create custom ApplicationUser extending IdentityUser** (Recommended)
   - Extend `IdentityUser<T>` where T is key type (GUID)
   - Add FirstName, LastName properties
   - Configure in DbContext: `modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers")`
   - Advantage: Full control, aligns with Constitution Principle II (conventions)
   - Disadvantage: Requires migration from default Identity schema

2. **Use IdentityUser with claims for FirstName/LastName**
   - Store FirstName/LastName in Identity Claims
   - Advantage: No schema changes
   - Disadvantage: Violates clean architecture; User properties scattered across Claims + database

3. **Separate UserProfile table linking to IdentityUser**
   - IdentityUser unchanged; UserProfile has FK to Id
   - Advantage: Minimal changes to Identity schema
   - Disadvantage: Extra join queries, complexity

### Decision

**✅ Option 1: Custom ApplicationUser extending IdentityUser**

**Rationale**:

- Cleanest data model; FirstName/LastName as first-class properties
- Aligns with Constitution Principle II (clean folder structure, consistent naming)
- Aligns with Principle I (Clean Architecture) - single source of truth for user data
- Industry standard pattern for ASP.NET Core projects

**Implementation Pattern**:

```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // Navigation property
    public ICollection<Idea> SubmittedIdeas { get; set; }
}
```

Configure in DbContext:

```csharp
builder.Entity<ApplicationUser>().ToTable("Users");
builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
```

**Role Management** (Submitter/Admin enum):

- Store roles as IdentityRole records: "Submitter", "Admin"
- Load in services via `userManager.GetRolesAsync(user)`
- Decorate controllers/actions with `[Authorize(Roles = "Admin")]`
- Constitution Principle IV satisfied: role-based authorization declarative

---

## Research Task 2: Entity Framework Core Migration Strategy

**Objective**: Establish migration workflow supporting phased development, rollback procedures, non-breaking schema changes

### Investigation

EF Core Migrations enable version-controlled database schema evolution. Key considerations:

- Initial migration captures initial schema (User, Idea, IdeaAttachment, AuditLog)
- Future Phase 2 features (email notifications, search) require additive migrations
- Production rollback strategy: migrate down to previous checkpoint
- Seed data: create admin user during initial migration

**Options Evaluated**:

1. **Migrations-based (Recommended)**
   - Each schema change = new migration file
   - `dotnet ef migrations add [MigrationName]`
   - `dotnet ef database update` applies to target database
   - Advantage: Version controlled, reversible, clear audit trail
   - Disadvantage: Requires SQL knowledge for complex changes

2. **Code-First with automatic migrations**
   - Automatic schema sync on app start
   - Advantage: Fast iteration
   - Disadvantage: Hard to track changes, risky in production

3. **Manual SQL scripts**
   - Write SQL directly; apply via migrations
   - Advantage: Full control
   - Disadvantage: Error-prone, hard to rollback

### Decision

**✅ Option 1: Explicit migrations with seed data**

**Rationale**:

- Aligns with Constitution Principle V (Phased feature development) - each phase documents schema changes
- Supports rollback (critical for production safety)
- Version controlled (git history of schema evolution)
- Team can review migrations before deployment

**Implementation Pattern**:

```csharp
// In Migrations/[timestamp]_Initial.cs
migrationBuilder.CreateTable(
    name: "Users",
    columns: table => new
    {
        Id = table.Column<Guid>(nullable: false),
        Email = table.Column<string>(maxLength: 256),
        FirstName = table.Column<string>(maxLength: 100),
        LastName = table.Column<string>(maxLength: 100),
        // ... other Identity columns
    },
    constraints: table => table.PrimaryKey("PK_Users", x => x.Id)
);

// Seed admin user
migrationBuilder.InsertData(
    table: "Users",
    columns: new[] { "Id", "Email", "FirstName", "LastName", "UserName" },
    values: new object[] { Guid.NewGuid(), "admin@innovatepam.local", "Admin", "User", "admin" }
);
```

Migration workflow:

1. Make model changes (e.g., add Property to Idea entity)
2. `dotnet ef migrations add AddNewField`
3. Review generated [timestamp]\_AddNewField.cs
4. `dotnet ef database update` applies to dev database
5. Commit migration file to git
6. On production: same `dotnet ef database update` applies migration

**Rollback Procedure** (if migration causes issues):

- `dotnet ef database update [PreviousMigrationName]` rolls back to previous state
- Investigate issue, create new migration with fix
- Reapply

---

## Research Task 3: File Upload Security & Storage

**Objective**: Implement secure file upload validation (MIME type detection, size limits) and secure storage outside web root

### Investigation

File uploads are a critical security vulnerability. Common attacks:

- **Executable upload**: Attacker uploads .exe disguised as PDF by changing extension
- **ZIP bombs**: Attacker uploads malicious ZIP that expands to consume disk
- **Path traversal**: Attacker uploads "../../sensitive.txt" to escape upload directory

**Security Layers**:

1. **Client-side validation** (HTML5, JavaScript)
   - Quick feedback to user (not security measure)
   - Rejects files based on extension + size before upload

2. **Server-side validation** (Security boundary)
   - **MIME type detection via magic bytes**: Read file header to detect real type (not extension)
   - **File size enforcement**: Reject > 10MB at app level + IIS level
   - **Filename sanitization**: Hash filename to prevent directory traversal
   - **Scan for executables**: Detect .exe headers, scripts, archives

3. **Storage location**
   - Store outside `wwwroot/` (web-accessible directory)
   - Prevents direct HTTP access; access controlled via FileDownloadController
   - Outside app directory = survives app restarts

**Options Evaluated**:

1. **MIME type detection via magic bytes + secure storage (Recommended)**
   - Read file header to determine real type
   - Example: PDF files always start with `%PDF`, JPEG with `FF D8 FF`
   - Store with hashed filename outside wwwroot
   - Advantage: Secure, portable (local file storage works for MVP)
   - Disadvantage: Limited scalability (Phase 2 → cloud storage)

2. **Extension-only validation**
   - Check file.Extension against whitelist
   - Advantage: Simple
   - Disadvantage: INSECURE (attacker renames .exe to .pdf)

3. **Cloud storage (AWS S3, Azure Blob)**
   - Managed security, scalability
   - Advantage: Production-grade, scales to thousands of users
   - Disadvantage: Overkill for MVP, adds dependency

### Decision

**✅ Option 1: Magic byte detection + local secure storage**

**Rationale**:

- Satisfies Constitution Principle VI (Secure File Upload Validation): "validate MIME type via content inspection"
- MVP scope: Local storage sufficient for course project
- Phase 2: Easy migration to cloud storage
- Portable: No external dependencies during MVP

**Implementation Pattern**:

```csharp
public class FileValidationService : IFileValidationService
{
    private static readonly Dictionary<string, string> MagicNumbers = new()
    {
        { "%PDF", "application/pdf" },
        { "PK\u0003\u0004", "application/zip" }, // Also .docx, .xlsx
        { "D0CF11E0A1B11AE1", "application/msword" }, // .doc
        { "FF D8 FF", "image/jpeg" },
        { "89 50 4E 47", "image/png" },
    };

    public async Task<Result<FileMetadata>> ValidateUploadAsync(IFormFile file, long maxSize)
    {
        if (file.Length > maxSize)
            return Result.Failure("File exceeds 10MB limit");

        // Read first N bytes
        var header = new byte[512];
        using (var stream = file.OpenReadStream())
            await stream.ReadAsync(header, 0, Math.Min(512, (int)file.Length));

        var detectedType = DetectMimeType(header);
        if (!IsAllowedType(detectedType))
            return Result.Failure($"File type {detectedType} not allowed");

        return Result.Success(new FileMetadata
        {
            RealMimeType = detectedType,
            Size = file.Length,
            OriginalName = file.FileName
        });
    }

    public string GetSecureStoragePath(Guid ideaId, string originalFileName)
    {
        var hashedName = $"{ideaId}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}";
        var extension = Path.GetExtension(originalFileName);
        return Path.Combine("upload_storage", "ideas", ideaId.ToString(),
                           $"{hashedName}{extension}");
    }
}
```

Storage structure:

```
upload_storage/
├── ideas/
│   ├── {IdeaId}/
│   │   └── {HashedFileName.ext}    # Named by hash; real file on disk
│   └── {IdeaId}/
└── temp/                            # Cleanup script removes files > 24h old
```

Access Control: Download endpoint checks user has access to idea before serving file.

---

## Research Task 4: Session Management & Inactivity Timeout

**Objective**: Configure ASP.NET Core session timeout (30 min normal, 15 min admin), implement inactivity tracking

### Investigation

ASP.NET Core session management via middleware and cookies. Key settings:

- Session cookies: "asp.net_sessionid" (httpOnly for security)
- Timeout: Configured in ISession.IdleTimeout
- Sliding expiration: Extends timeout on each request

**Options Evaluated**:

1. **Cookie-based sessions with sliding expiration (Recommended)**
   - Configure in Program.cs: `.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30))`
   - Cookie automatically extended on each request
   - Advantage: Simple, built-in, secure (httpOnly + Secure flags)
   - Disadvantage: Different timeout per role requires custom middleware

2. **JWT tokens with explicit refresh**
   - Client stores token; server validates on each request
   - Advantage: Stateless, suitable for APIs/SPAs
   - Disadvantage: More complex for traditional MVC app

3. **Database-backed sessions**
   - Store session data in SQL
   - Advantage: Shared across servers (for load balancing)
   - Disadvantage: Overkill for MVP

### Decision

**✅ Option 1: Cookie-based with custom middleware for role-based timeout**

**Rationale**:

- Aligns with Constitution Principle IV (Secure Authentication & RBAC)
- Specification requires: "30 min for normal users, 15 min for admins"
- Built-in, secure, no external dependencies
- Custom middleware can differentiate timeout by role

**Implementation Pattern**:

```csharp
// Program.cs
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Default
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
});

// AuthController.cs - Login
var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, userRole) // "Submitter" or "Admin"
}, "ApplicationCookie"));

await HttpContext.SignInAsync("Cookies", principal);

// Middleware: Custom session timeout based on role
public class SessionTimeoutMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTimeoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var idleTimeout = role == "Admin" ? 15 : 30; // minutes

        // Update session cookie expiration
        if (context.Session.IsAvailable)
        {
            context.Session.SetString("LastActivity", DateTime.UtcNow.ToString());
        }

        await _next(context);
    }
}
```

---

## Research Task 5: Responsive UI Design - CSS Framework Selection

**Objective**: Choose CSS framework (Bootstrap, Tailwind) for responsive mobile-first design supporting Constitution Principle VIII (Responsive and Consistent UX)

### Investigation

Two leading approaches for responsive design in ASP.NET Core:

1. **Bootstrap 5** (Popular, batteries-included)
   - Pre-built components: buttons, forms, modals, grid
   - Theming: Built-in color/spacing customization
   - Grid system: 12-column responsive grid
   - Advantage: Quick prototyping, extensive documentation
   - Disadvantage: Larger CSS file, can feel "Bootstrap-y"

2. **Tailwind CSS** (Utility-first, minimal)
   - Utility classes: `flex`, `grid-cols-3`, `rounded-lg`
   - Build process: Tailwind CLI or PostCSS
   - Customization: Tailwind.config.js
   - Advantage: Smaller output, fully customizable
   - Disadvantage: Steeper learning curve, requires build process

3. **Custom CSS** (Pure CSS, no framework)
   - Full control, minimal dependencies
   - Advantage: Lightweight
   - Disadvantage: More code, slower development

### Decision

**✅ Option 1: Bootstrap 5 (for MVP course project)**

**Rationale**:

- Constitution Principle VIII: Responsive design & consistent UX
- Bootstrap provides pre-built responsive components (faster development)
- Supports WCAG 2.1 AA accessibility out of the box (Constitution requirement)
- Large community; students familiar with it
- Mobile-first responsive grid: `col-12 col-md-6 col-lg-4` patterns
- Easy to customize via Bootstrap Sass variables

**Implementation Pattern**:

```html
<!-- Layout.cshtml - Master layout with responsive navbar -->
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link
      href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
      rel="stylesheet"
    />
  </head>
  <body>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <div class="container">
        <a class="navbar-brand" href="/">InnovatEPAM Portal</a>
        <button
          class="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
        >
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item">
              <a class="nav-link" href="/ideas">My Ideas</a>
            </li>
            @if (User.IsInRole("Admin")) {
            <li class="nav-item">
              <a class="nav-link" href="/admin/ideas">Review</a>
            </li>
            }
            <li class="nav-item">
              <a class="nav-link" href="/auth/logout">Logout</a>
            </li>
          </ul>
        </div>
      </div>
    </nav>
    <main class="container mt-4">@RenderBody()</main>
  </body>
</html>

<!-- Ideas/Index.cshtml - Responsive ideas list -->
<div class="row">
  @foreach (var idea in Model.Ideas) {
  <div class="col-12 col-md-6 col-lg-4 mb-4">
    <div class="card">
      <div class="card-body">
        <h5 class="card-title">@idea.Title</h5>
        <p class="card-text text-truncate">@idea.Description</p>
        <span class="badge bg-primary">@idea.Status</span>
        <a href="/ideas/@idea.Id" class="btn btn-sm btn-primary mt-3">View</a>
      </div>
    </div>
  </div>
  }
</div>
```

Responsive breakpoints:

- Mobile: `col-12` (full width on small screens)
- Tablet: `col-md-6` (2 columns on medium screens)
- Desktop: `col-lg-4` (3 columns on large screens)

Accessibility: Bootstrap 5 includes ARIA labels, semantic HTML. Enhance with:

- Color contrast: `text-dark` on `bg-light` meets WCAG AA
- Form labels: Always associated with inputs via `for` attribute
- Error messages: Displayed clearly in `.invalid-feedback`

---

## Research Task 6: AutoMapper + FluentValidation Integration

**Objective**: Establish clean integration pattern: FluentValidation at controller boundary, AutoMapper for DTO↔Model mapping, services receive validated DTOs

### Investigation

Three layers of validation & mapping:

1. **Request-level validation** (Controller)
   - Input from user: RegisterViewModel, CreateIdeaDTO
   - FluentValidation checks: Email format, required fields, file size
   - Map valid DTO → service call

2. **Service-level validation** (Business rules)
   - Business rule enforcement: Email uniqueness, idea status transitions
   - Logged exceptions if violations
   - Map Model → DTO for response

3. **Database-level validation** (Entity Framework)
   - Data annotations: [MaxLength], [Required]
   - Constraints: Unique indexes, foreign keys
   - Final safety net

**Options Evaluated**:

1. **FluentValidation + AutoMapper (Recommended)**
   - Validators for each DTO: `CreateIdeaDTOValidator`
   - AutoMapper profiles map DTOs ↔ Models
   - Advantage: Clear separation; composition over inheritance
   - Disadvantage: Extra mapper setup

2. **Data Annotations only**
   - `[Required]`, `[EmailAddress]` on DTOs
   - Advantage: Simpler, built-in
   - Disadvantage: Less flexible for complex rules

3. **Service-level validation only**
   - All validation in services
   - Advantage: Centralized
   - Disadvantage: No early feedback to user; business logic in controller

### Decision

**✅ Option 1: FluentValidation + AutoMapper**

**Rationale**:

- Constitution Principle III (Service-Layer Business Logic): Services receive validated data, focus on business rules
- Constitution Principle II (Maintainable ASP.NET Core): Clear separation of concerns (validation ≠ business logic)
- FluentValidation provides detailed error messages for UI feedback
- AutoMapper eliminates manual DTO↔Model mapping boilerplate

**Implementation Pattern**:

```csharp
// DTOs/CreateIdeaDTO.cs
public class CreateIdeaDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile Attachment { get; set; }
}

// Validators/CreateIdeaDTOValidator.cs
public class CreateIdeaDTOValidator : AbstractValidator<CreateIdeaDTO>
{
    public CreateIdeaDTOValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(1, 200).WithMessage("Title must be 1-200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description max 2000 characters");

        When(x => x.Attachment != null, () =>
        {
            RuleFor(x => x.Attachment.Length)
                .LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("File must be ≤ 10MB");
        });
    }
}

// Controllers/IdeasController.cs
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateIdeaDTO dto)
{
    // FluentValidation runs here (via ModelState validation)
    if (!ModelState.IsValid)
        return View("Create", dto); // Return form with errors

    // Services receive validated DTO
    var result = await _ideaService.CreateIdeaAsync(
        userId: User.FindFirst(ClaimTypes.NameIdentifier).Value,
        dto: dto
    );

    if (!result.IsSuccess)
        return BadRequest(result.Error);

    return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
}

// Services/IdeaService.cs
public async Task<Result<IdeaDTO>> CreateIdeaAsync(string userId, CreateIdeaDTO dto)
{
    // DTO is already validated; focus on business logic
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
        return Result.Failure("User not found");

    var idea = _mapper.Map<Idea>(dto);
    idea.SubmitterId = userId;
    idea.Status = IdeaStatus.Submitted;

    await _ideaRepository.AddAsync(idea);
    return Result.Success(_mapper.Map<IdeaDTO>(idea));
}

// Mapping/AutoMapperProfile.cs
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<CreateIdeaDTO, Idea>();
        CreateMap<Idea, IdeaDTO>();
        CreateMap<IdeaAttachment, IdeaAttachmentDTO>();
    }
}
```

Program.cs setup:

```csharp
services.AddAutoMapper(typeof(AutoMapperProfile));
services.AddValidatorsFromAssemblyContaining<CreateIdeaDTOValidator>();
```

---

## Summary: All Research Items Resolved

| Task                      | Decision                                                   | Rationale                                     |
| ------------------------- | ---------------------------------------------------------- | --------------------------------------------- |
| 1. Identity Customization | Custom ApplicationUser extending IdentityUser              | Clean data model; Constitution Principle I    |
| 2. EF Core Migrations     | Explicit migrations with seed data                         | Version controlled; reversible; Phase support |
| 3. File Upload Security   | Magic byte detection + local storage                       | Constitution Principle VI; MVP-appropriate    |
| 4. Session Management     | Cookie-based with custom middleware for role-based timeout | Built-in, secure; role differentiation        |
| 5. Responsive UI          | Bootstrap 5 with mobile-first grid                         | Fast development; WCAG accessibility          |
| 6. Validation & Mapping   | FluentValidation + AutoMapper                              | Clean separation; Constitution Principle III  |

**Status**: ✅ **All NEEDS CLARIFICATION items resolved**

All research findings support implementation without ambiguity. Ready to proceed to Phase 1 design (data-model.md, contracts/).

---

**Document Version**: 1.0.0 | **Date**: 2026-05-14 | **Status**: Complete
