# Data Model: Entity Design & Relationships

**Feature**: Employee Innovation Ideas Management Portal  
**Date**: 2026-05-14  
**Phase**: 1 - Design  
**Input**: Specification entities + research decisions

---

## Entity Diagram

```
┌─────────────────────┐
│   ApplicationUser   │
├─────────────────────┤
│ Id: Guid (PK)       │
│ Email: string       │
│ FirstName: string   │
│ LastName: string    │
│ Role: string        │
│ CreatedDate: DateTime
│ PasswordHash        │
│ (from IdentityUser) │
└──────────┬──────────┘
           │
           │ 1:N (Submitter)
           │
           ▼
┌──────────────────────┐         ┌─────────────────────┐
│       Idea           │         │ IdeaAttachment      │
├──────────────────────┤         ├─────────────────────┤
│ Id: Guid (PK)        │◄────────│ Id: Guid (PK)       │
│ Title: string        │ 1:N     │ IdeaId: Guid (FK)   │
│ Description: string  │         │ FileName: string    │
│ Status: enum         │         │ FilePath: string    │
│ SubmitterId: Guid(FK)├────────►│ FileSize: long      │
│ CreatedDate: DateTime           │ UploadedDate: DateTime
│ LastModifiedDate     │         │                     │
│ LastModifiedByAdminId│         └─────────────────────┘
│ (optional, nullable) │
└──────────┬───────────┘
           │
           │ 1:N (Admin)
           │
           ▼
       (UpdatedBy)

┌─────────────────────┐
│    AuditLog         │
├─────────────────────┤
│ Id: Guid (PK)       │
│ IdeaId: Guid (FK)   │
│ OldStatus: string   │
│ NewStatus: string   │
│ ChangedByAdminId    │
│ ChangedDate: DateTime
└─────────────────────┘
```

---

## Entity Definitions

### 1. ApplicationUser (extends IdentityUser<Guid>)

**Purpose**: Represents an employee with authentication credentials and role.

**Database Table**: `Users` (replaces default AspNetUsers)

**Properties**:

| Property         | Type     | Nullable | Constraints         | Notes                                   |
| ---------------- | -------- | -------- | ------------------- | --------------------------------------- |
| Id               | Guid     | NO       | PK                  | User identifier                         |
| Email            | string   | NO       | Unique, Indexed     | Employee email                          |
| FirstName        | string   | NO       | Max 100 chars       | Display name                            |
| LastName         | string   | NO       | Max 100 chars       | Display name                            |
| PasswordHash     | string   | NO       | (from IdentityUser) | Hashed password                         |
| SecurityStamp    | string   | NO       | (from IdentityUser) | For password changes                    |
| ConcurrencyStamp | string   | NO       | (from IdentityUser) | Optimistic concurrency                  |
| EmailConfirmed   | bool     | NO       | (from IdentityUser) | MVP: Not used; set true on registration |
| UserName         | string   | NO       | (from IdentityUser) | = Email for MVP                         |
| CreatedDate      | DateTime | NO       |                     | UTC timestamp                           |

**Navigation Properties**:

| Property       | Type                  | Relationship | Notes                             |
| -------------- | --------------------- | ------------ | --------------------------------- |
| SubmittedIdeas | ICollection<Idea>     | 1:N          | Ideas submitted by this user      |
| UpdatedIdeas   | ICollection<Idea>     | 1:N          | Ideas last updated by this admin  |
| AuditLogs      | ICollection<AuditLog> | 1:N          | Status changes made by this admin |

**Roles** (via AspNetRoles table):

- "Submitter" (default for new registrations)
- "Admin" (assigned by system administrator)

**Validation Rules**:

- Email: Required, valid format, unique across system
- FirstName: Required, 1-100 characters
- LastName: Required, 1-100 characters
- Password: Min 12 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char (OWASP)

**EF Core Configuration**:

```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public ICollection<Idea> SubmittedIdeas { get; set; } = new List<Idea>();
    public ICollection<Idea> UpdatedIdeas { get; set; } = new List<Idea>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

// In DbContext.OnModelCreating:
modelBuilder.Entity<ApplicationUser>(entity =>
{
    entity.ToTable("Users");
    entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
    entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
    entity.HasIndex(u => u.Email).IsUnique();
});
```

---

### 2. Idea

**Purpose**: Represents an innovation proposal submitted by a user, with status tracking and admin modifications.

**Database Table**: `Ideas`

**Properties**:

| Property              | Type     | Nullable | Constraints                              | Notes                                   |
| --------------------- | -------- | -------- | ---------------------------------------- | --------------------------------------- |
| Id                    | Guid     | NO       | PK                                       | Idea identifier                         |
| Title                 | string   | NO       | Max 200 chars                            | Idea name                               |
| Description           | string   | YES      | Max 2000 chars                           | Idea details                            |
| Status                | enum     | NO       | Submitted/Under Review/Accepted/Rejected | Current workflow state                  |
| SubmitterId           | Guid     | NO       | FK → ApplicationUser                     | Idea creator                            |
| CreatedDate           | DateTime | NO       |                                          | UTC timestamp                           |
| LastModifiedDate      | DateTime | NO       |                                          | UTC timestamp (= CreatedDate initially) |
| LastModifiedByAdminId | Guid     | YES      | FK → ApplicationUser                     | Admin who last updated status           |

**Navigation Properties**:

| Property        | Type                        | Relationship | Notes                   |
| --------------- | --------------------------- | ------------ | ----------------------- |
| Submitter       | ApplicationUser             | N:1          | User who created idea   |
| UpdatedByAdmin  | ApplicationUser             | N:1          | Admin who last modified |
| IdeaAttachments | ICollection<IdeaAttachment> | 1:N          | Attached files          |
| AuditLogs       | ICollection<AuditLog>       | 1:N          | Status change history   |

**Enums**:

```csharp
public enum IdeaStatus
{
    Submitted = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4
}
```

**Validation Rules**:

- Title: Required, 1-200 characters
- Description: Optional, 0-2000 characters
- Status: Must be valid enum value
- SubmitterId: Must reference existing user
- CreatedDate/LastModifiedDate: Must be UTC, not in future

**Status Transitions**:

- Submitter perspective: Can only view (cannot modify)
- Admin perspective: Submitted → Under Review → (Accepted OR Rejected)
- Admin can revert: Accepted → Under Review → Rejected (or back)

**EF Core Configuration**:

```csharp
public class Idea
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public IdeaStatus Status { get; set; }

    public Guid SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; }

    public Guid? LastModifiedByAdminId { get; set; }
    public ApplicationUser UpdatedByAdmin { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }

    public ICollection<IdeaAttachment> IdeaAttachments { get; set; } = new List<IdeaAttachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

// In DbContext.OnModelCreating:
modelBuilder.Entity<Idea>(entity =>
{
    entity.ToTable("Ideas");
    entity.HasKey(i => i.Id);

    entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
    entity.Property(i => i.Description).HasMaxLength(2000);
    entity.Property(i => i.Status).IsRequired().HasConversion<int>();

    entity.HasOne(i => i.Submitter)
        .WithMany(u => u.SubmittedIdeas)
        .HasForeignKey(i => i.SubmitterId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(i => i.UpdatedByAdmin)
        .WithMany(u => u.UpdatedIdeas)
        .HasForeignKey(i => i.LastModifiedByAdminId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasIndex(i => i.SubmitterId);
    entity.HasIndex(i => i.Status);
});
```

---

### 3. IdeaAttachment

**Purpose**: Represents a file attached to an idea, with secure storage path and metadata.

**Database Table**: `IdeaAttachments`

**Properties**:

| Property     | Type     | Nullable | Constraints   | Notes                                 |
| ------------ | -------- | -------- | ------------- | ------------------------------------- |
| Id           | Guid     | NO       | PK            | Attachment identifier                 |
| IdeaId       | Guid     | NO       | FK → Idea     | Parent idea                           |
| FileName     | string   | NO       | Max 255 chars | Original filename (for display)       |
| FilePath     | string   | NO       | Max 500 chars | Hashed storage path (outside wwwroot) |
| FileSize     | long     | NO       |               | Bytes; Max 10MB enforced at service   |
| UploadedDate | DateTime | NO       |               | UTC timestamp                         |

**Navigation Properties**:

| Property | Type | Relationship | Notes       |
| -------- | ---- | ------------ | ----------- |
| Idea     | Idea | N:1          | Parent idea |

**Validation Rules**:

- FileName: Required, max 255 chars (filesystem limit)
- FilePath: Required, secure path outside wwwroot
- FileSize: ≤ 10,485,760 bytes (10MB)
- UploadedDate: UTC, not in future

**File Type Whitelist** (enforced at service layer):

- Documents: `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`
- Images: `.jpg`, `.jpeg`, `.png`
- MIME detection (magic bytes) validates real type, not extension

**EF Core Configuration**:

```csharp
public class IdeaAttachment
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }

    public Idea Idea { get; set; }
}

// In DbContext.OnModelCreating:
modelBuilder.Entity<IdeaAttachment>(entity =>
{
    entity.ToTable("IdeaAttachments");
    entity.HasKey(a => a.Id);

    entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
    entity.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
    entity.Property(a => a.FileSize).IsRequired();

    entity.HasOne(a => a.Idea)
        .WithMany(i => i.IdeaAttachments)
        .HasForeignKey(a => a.IdeaId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasIndex(a => a.IdeaId);
});
```

---

### 4. AuditLog

**Purpose**: Records all status changes for compliance, debugging, and admin review transparency.

**Database Table**: `AuditLogs`

**Properties**:

| Property         | Type     | Nullable | Constraints          | Notes                       |
| ---------------- | -------- | -------- | -------------------- | --------------------------- |
| Id               | Guid     | NO       | PK                   | Log entry identifier        |
| IdeaId           | Guid     | NO       | FK → Idea            | Idea that was changed       |
| OldStatus        | string   | NO       | Max 50 chars         | Previous status (enum name) |
| NewStatus        | string   | NO       | Max 50 chars         | New status (enum name)      |
| ChangedByAdminId | Guid     | NO       | FK → ApplicationUser | Admin who made change       |
| ChangedDate      | DateTime | NO       |                      | UTC timestamp of change     |

**Navigation Properties**:

| Property       | Type            | Relationship | Notes                 |
| -------------- | --------------- | ------------ | --------------------- |
| Idea           | Idea            | N:1          | Idea that was changed |
| ChangedByAdmin | ApplicationUser | N:1          | Admin who made change |

**Validation Rules**:

- OldStatus/NewStatus: Must be valid IdeaStatus enum name
- ChangedByAdminId: Must reference existing user with Admin role
- ChangedDate: UTC timestamp, not in future
- One entry per status change (created in service when status updates)

**EF Core Configuration**:

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public Guid ChangedByAdminId { get; set; }
    public DateTime ChangedDate { get; set; }

    public Idea Idea { get; set; }
    public ApplicationUser ChangedByAdmin { get; set; }
}

// In DbContext.OnModelCreating:
modelBuilder.Entity<AuditLog>(entity =>
{
    entity.ToTable("AuditLogs");
    entity.HasKey(a => a.Id);

    entity.Property(a => a.OldStatus).IsRequired().HasMaxLength(50);
    entity.Property(a => a.NewStatus).IsRequired().HasMaxLength(50);

    entity.HasOne(a => a.Idea)
        .WithMany(i => i.AuditLogs)
        .HasForeignKey(a => a.IdeaId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(a => a.ChangedByAdmin)
        .WithMany(u => u.AuditLogs)
        .HasForeignKey(a => a.ChangedByAdminId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(a => a.IdeaId);
    entity.HasIndex(a => a.ChangedByAdminId);
    entity.HasIndex(a => a.ChangedDate);
});
```

---

## Database Schema (PostgreSQL DDL)

```sql
-- Users table (extends AspNetUsers)
CREATE TABLE "Users" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "UserName" varchar(256) NOT NULL,
    "NormalizedUserName" varchar(256),
    "Email" varchar(256) NOT NULL,
    "NormalizedEmail" varchar(256) NOT NULL,
    "EmailConfirmed" boolean NOT NULL DEFAULT false,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL DEFAULT false,
    "TwoFactorEnabled" boolean NOT NULL DEFAULT false,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL DEFAULT true,
    "AccessFailedCount" integer NOT NULL DEFAULT 0,
    "FirstName" varchar(100) NOT NULL,
    "LastName" varchar(100) NOT NULL,
    CONSTRAINT "UX_Users_Email" UNIQUE ("Email")
);

CREATE INDEX "IX_Users_NormalizedEmail" ON "Users"("NormalizedEmail");

-- Ideas table
CREATE TABLE "Ideas" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Title" varchar(200) NOT NULL,
    "Description" varchar(2000),
    "Status" integer NOT NULL,
    "SubmitterId" uuid NOT NULL,
    "LastModifiedByAdminId" uuid,
    "CreatedDate" timestamp with time zone NOT NULL,
    "LastModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_Ideas_Users_SubmitterId" FOREIGN KEY ("SubmitterId") REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Ideas_Users_UpdatedByAdminId" FOREIGN KEY ("LastModifiedByAdminId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Ideas_SubmitterId" ON "Ideas"("SubmitterId");
CREATE INDEX "IX_Ideas_Status" ON "Ideas"("Status");

-- IdeaAttachments table
CREATE TABLE "IdeaAttachments" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "IdeaId" uuid NOT NULL,
    "FileName" varchar(255) NOT NULL,
    "FilePath" varchar(500) NOT NULL,
    "FileSize" bigint NOT NULL,
    "UploadedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_IdeaAttachments_Ideas_IdeaId" FOREIGN KEY ("IdeaId") REFERENCES "Ideas"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_IdeaAttachments_IdeaId" ON "IdeaAttachments"("IdeaId");

-- AuditLogs table
CREATE TABLE "AuditLogs" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "IdeaId" uuid NOT NULL,
    "OldStatus" varchar(50) NOT NULL,
    "NewStatus" varchar(50) NOT NULL,
    "ChangedByAdminId" uuid NOT NULL,
    "ChangedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_AuditLogs_Ideas_IdeaId" FOREIGN KEY ("IdeaId") REFERENCES "Ideas"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AuditLogs_Users_ChangedByAdminId" FOREIGN KEY ("ChangedByAdminId") REFERENCES "Users"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AuditLogs_IdeaId" ON "AuditLogs"("IdeaId");
CREATE INDEX "IX_AuditLogs_ChangedByAdminId" ON "AuditLogs"("ChangedByAdminId");
CREATE INDEX "IX_AuditLogs_ChangedDate" ON "AuditLogs"("ChangedDate");
```

---

## DTOs (Data Transfer Objects)

DTOs separate API contracts from domain models:

### CreateIdeaDTO

```csharp
public class CreateIdeaDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile Attachment { get; set; }
}
```

### UpdateIdeaStatusDTO

```csharp
public class UpdateIdeaStatusDTO
{
    public string NewStatus { get; set; }
}
```

### IdeaListItemDTO

```csharp
public class IdeaListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SubmitterName { get; set; } // For admin view
}
```

### IdeaDetailDTO

```csharp
public class IdeaDetailDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string SubmitterName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public List<IdeaAttachmentDTO> Attachments { get; set; }
}
```

### IdeaAttachmentDTO

```csharp
public class IdeaAttachmentDTO
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
}
```

---

## Data Access Patterns (Repository Layer)

### IUserRepository

```csharp
public interface IUserRepository
{
    Task<ApplicationUser> GetByIdAsync(Guid id);
    Task<ApplicationUser> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(ApplicationUser user);
}
```

### IIdeaRepository

```csharp
public interface IIdeaRepository
{
    Task<Idea> GetByIdAsync(Guid id);
    Task<List<Idea>> GetBySubmitterAsync(Guid submitterId);
    Task<List<Idea>> GetAllAsync();
    Task<List<Idea>> GetByStatusAsync(IdeaStatus status);
    Task AddAsync(Idea idea);
    Task UpdateAsync(Idea idea);
    Task DeleteAsync(Idea idea);
}
```

### IAuditLogRepository

```csharp
public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetByIdeaAsync(Guid ideaId);
    Task AddAsync(AuditLog log);
}
```

---

## Constraints & Indexes

**Unique Constraints**:

- User.Email (business rule: email uniqueness)

**Foreign Key Constraints**:

- Idea.SubmitterId → User.Id (ON DELETE RESTRICT: prevent orphaned ideas)
- Idea.LastModifiedByAdminId → User.Id (ON DELETE SET NULL: admin user deletion allowed)
- IdeaAttachment.IdeaId → Idea.Id (ON DELETE CASCADE: delete files when idea deleted)
- AuditLog.IdeaId → Idea.Id (ON DELETE CASCADE: cleanup logs when idea deleted)
- AuditLog.ChangedByAdminId → User.Id (ON DELETE RESTRICT: prevent audit trail tampering)

**Indexes** (for query performance):

- Ideas.SubmitterId (filter ideas by submitter)
- Ideas.Status (filter by status in admin view)
- IdeaAttachments.IdeaId (fetch attachments for idea detail)
- AuditLogs.IdeaId (fetch change history)
- AuditLogs.ChangedDate (order by recent changes)

---

**Document Version**: 1.0.0 | **Date**: 2026-05-14 | **Status**: Complete
