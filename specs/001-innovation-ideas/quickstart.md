# Quickstart Guide: Local Development Setup

**Feature**: Employee Innovation Ideas Management Portal  
**Date**: 2026-05-14  
**Duration**: ~30 minutes to first login

---

## Prerequisites

Ensure you have installed:

- **.NET 8.0 SDK** ([download](https://dotnet.microsoft.com/download))
- **PostgreSQL 14+** ([download](https://www.postgresql.org/download/)) - or use [PostgreSQL Docker image](https://hub.docker.com/_/postgres)
- **Visual Studio 2022** or **VS Code** + C# extension
- **Git** (already initialized in workspace)

### Verify Installation

```bash
dotnet --version              # Should show 8.0.x
psql --version               # Should show PostgreSQL 14+
```

---

## Step 1: Clone & Open Project

```bash
cd ~/Desktop/InnovatEPAM\ Portal

# Restore NuGet packages
dotnet restore
```

---

## Step 2: Configure PostgreSQL

### Option A: Local PostgreSQL Installation (Recommended for development)

1. **Start PostgreSQL service**:
   - **macOS** (via Homebrew): `brew services start postgresql@15`
   - **Windows**: PostgreSQL installer starts service automatically
   - **Linux**: `sudo systemctl start postgresql`

2. **Create database**:

   ```bash
   # Connect to PostgreSQL
   psql -U postgres

   # In psql prompt:
   CREATE DATABASE innovatepam_dev;
   \q
   ```

3. **Update connection string**:
   - Open `appsettings.Development.json`
   - Verify `DefaultConnection`:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Host=localhost;Port=5432;Database=innovatepam_dev;Username=postgres;Password=postgres"
       }
     }
     ```
   - Adjust Username/Password if different

### Option B: Docker PostgreSQL (No local installation)

```bash
# Start PostgreSQL container
docker run -d \
  --name innovatepam-postgres \
  -e POSTGRES_DB=innovatepam_dev \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:15

# Connection string (same as Option A, localhost)
```

---

## Step 3: Run Entity Framework Migrations

Entity Framework Core will create tables from your data model:

```bash
# Apply migrations to database
dotnet ef database update

# Output should show:
# Done. To undo this action, use 'ef migrations remove'.
```

**What this does**:

- Creates `Users` table with authentication fields
- Creates `Ideas`, `IdeaAttachments`, `AuditLogs` tables
- Creates indexes for query performance
- Seeds initial admin user (if configured)

**To verify in PostgreSQL**:

```bash
psql -U postgres -d innovatepam_dev

# In psql:
\dt                  # List all tables
SELECT * FROM "Users";  # Check users created
\q
```

---

## Step 4: Create Admin User (Manual Setup)

For MVP, admin accounts are created manually. Add an admin user:

```bash
# Open project and add to a migration seed method, OR:
# Use SQL directly:

psql -U postgres -d innovatepam_dev

INSERT INTO "Users"
  ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
   "EmailConfirmed", "FirstName", "LastName", "PasswordHash", "SecurityStamp",
   "ConcurrencyStamp", "LockoutEnabled", "AccessFailedCount")
VALUES (
  'f1234567-89ab-cdef-0123-456789abcdef',
  'admin@innovatepam.local',
  'ADMIN@INNOVATEPAM.LOCAL',
  'admin@innovatepam.local',
  'ADMIN@INNOVATEPAM.LOCAL',
  true,
  'Admin',
  'User',
  'hashed_password_placeholder',  -- Will be replaced via API
  'security_stamp',
  'concurrency_stamp',
  false,
  0
);

\q
```

**Better approach**: Use ASP.NET Core Identity API to hash password properly.

**For now** (MVP): Use the registration flow to create admin account, then manually assign Admin role:

```sql
-- After user registers, assign Admin role:
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName")
VALUES ('role-admin-guid', 'Admin', 'ADMIN');

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('user-id-from-registration', 'role-admin-guid');
```

---

## Step 5: Build & Run Application

```bash
# Build solution (compiles C#, validates dependencies)
dotnet build

# Run application (localhost:5000)
dotnet run

# Output should show:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5000
#       Application started. Press Ctrl+C to exit.
```

Open browser: **http://localhost:5000**

---

## Step 6: First Login - Manual Testing Workflow

### Scenario 1: User Registration (Submitter)

1. **Navigate to**: http://localhost:5000/auth/register
2. **Fill form**:
   - Email: `submitter@innovatepam.local`
   - Password: `MyPassword@123` (meets complexity: 12+ chars, upper, lower, number, special)
   - Confirm Password: `MyPassword@123`
   - First Name: `John`
   - Last Name: `Developer`
3. **Click Register**
4. **Expected result**: Redirected to login page with success message

### Scenario 2: User Login

1. **Navigate to**: http://localhost:5000/auth/login
2. **Fill form**:
   - Email: `submitter@innovatepam.local`
   - Password: `MyPassword@123`
3. **Click Login**
4. **Expected result**: Redirected to `/ideas` (My Ideas dashboard - empty initially)
5. **Verify**: Navbar shows "Logout" (logged in)

### Scenario 3: Create First Idea

1. **On `/ideas` page, click "Create New Idea"**
2. **Fill form**:
   - Title: `Improve Employee Onboarding Process`
   - Description: `A mobile-first onboarding checklist that guides new hires through company procedures.`
   - Attachment: (optional) Upload a PDF or Word doc
3. **Click Submit**
4. **Expected result**: Idea appears in "My Ideas" list with status "Submitted"

### Scenario 4: View Idea Detail

1. **On `/ideas` list, click the idea title**
2. **Expected result**: Full idea detail displayed
   - Title, Description, Status: Submitted
   - Submission date shown
   - Attachment download link (if any)

### Scenario 5: Admin Review Workflow

1. **Logout** current submitter: Click "Logout"
2. **Login as admin** (use admin account created in Step 4)
3. **Navigate to**: `/admin/ideas`
4. **Expected result**: See ALL submitted ideas from ALL users (not just own)
5. **Click an idea to review**:
   - See submitter name, description, attachment
   - Status dropdown shows options: "Under Review", "Accepted", "Rejected"
6. **Change status**: Select "Under Review", click "Save"
7. **Expected result**: Status saved; AuditLog created

### Scenario 6: Submitter Sees Status Update

1. **Logout admin, login as submitter again**
2. **Navigate to `/ideas`**
3. **Expected result**: Idea status now shows "Under Review" (not "Submitted")

---

## Step 7: Verify Database State

Check that data persisted correctly:

```bash
psql -U postgres -d innovatepam_dev

-- List all users
SELECT "Email", "FirstName", "LastName" FROM "Users";

-- List all ideas
SELECT "Title", "Status", "SubmitterId", "CreatedDate" FROM "Ideas";

-- List audit logs (admin actions)
SELECT "OldStatus", "NewStatus", "ChangedByAdminId", "ChangedDate" FROM "AuditLogs";

\q
```

---

## Step 8: Run Manual Test Matrix

Document test results in `tests/manual-testing.md`:

| Scenario              | Steps                                 | Expected Result                          | Pass/Fail | Notes |
| --------------------- | ------------------------------------- | ---------------------------------------- | --------- | ----- |
| User Registration     | Register with valid email/password    | Account created, redirects to login      |           |       |
| Login                 | Login with correct credentials        | Authenticated, redirect to `/ideas`      |           |       |
| Logout                | Click logout                          | Session destroyed, redirect to login     |           |       |
| Create Idea           | Submit title + description + file     | Idea saved with "Submitted" status       |           |       |
| View My Ideas         | Navigate to `/ideas`                  | List shows own ideas only                |           |       |
| Admin Review          | Navigate to `/admin/ideas`            | List shows ALL ideas                     |           |       |
| Update Status         | Change status to "Under Review"       | Status saved, AuditLog created           |           |       |
| Submitter Sees Update | Submitter re-logins                   | Idea status reflects admin change        |           |       |
| File Download         | Click attachment download link        | File served with correct name            |           |       |
| File Size Validation  | Upload > 10MB file                    | Rejected with "File exceeds limit" error |           |       |
| Unauthorized Access   | Submitter navigates to `/admin/ideas` | Redirected to login or 403 error         |           |       |

---

## Debugging Tips

### Application won't start?

1. **Check PostgreSQL connection**:

   ```bash
   psql -U postgres -d innovatepam_dev -c "SELECT 1;"
   # Should return: 1
   ```

2. **Check migration status**:

   ```bash
   dotnet ef migrations list
   # Should show migrations applied
   ```

3. **View application logs**:
   - Set `ASPNETCORE_ENVIRONMENT=Development`
   - Check browser console (F12) for JavaScript errors
   - Check terminal for exception stack traces

### File upload fails?

1. **Check upload directory exists**:

   ```bash
   ls -la upload_storage/
   ```

2. **Check permissions**:

   ```bash
   chmod 755 upload_storage/
   ```

3. **Verify MIME validation**:
   - Try uploading valid file (.pdf, .jpg, .docx)
   - Check service logs for validation errors

### Login doesn't work?

1. **Verify user exists in database**:

   ```bash
   psql -U postgres -d innovatepam_dev
   SELECT "Email", "PasswordHash" FROM "Users" WHERE "Email" = 'submitter@innovatepam.local';
   ```

2. **Check password policy**:
   - Password must be 12+ characters
   - Must contain: 1 uppercase, 1 lowercase, 1 number, 1 special character
   - Example valid: `MyPassword@123`

---

## Next Steps

After manual testing verifies core workflows:

1. **Run all manual tests from test matrix** (document results)
2. **Generate task list**: `/speckit.tasks` (converts design to actionable development tasks)
3. **Begin implementation**: Create controllers, services, views following task list
4. **Write unit tests**: Critical business logic (auth, file validation, status transitions)
5. **Code review**: Every PR reviewed against Constitution principles
6. **Deploy**: After all manual tests pass on staging environment

---

## Useful Commands

```bash
# Database
dotnet ef migrations add AddMyFeature          # Create new migration
dotnet ef database update                      # Apply migrations
dotnet ef database update PreviousMigration    # Rollback migration
dotnet ef migrations remove                    # Remove pending migration

# Building
dotnet build                                   # Compile
dotnet build --configuration Release          # Release build
dotnet run                                     # Run application
dotnet run --configuration Release            # Run release build

# Testing (when unit tests added)
dotnet test                                    # Run all tests
dotnet test --filter "ServiceName"             # Run specific test class
dotnet test --collect:"XPlat Code Coverage"   # Code coverage report

# Cleanup
dotnet clean                                   # Remove build artifacts
rm -rf bin obj                                 # Manual cleanup
```

---

**Setup Duration**: ~30 minutes  
**Next Phase**: Task generation & implementation  
**Questions?**: Review Constitution.md for architecture principles
