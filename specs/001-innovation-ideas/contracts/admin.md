# API Contract: Admin Review & Status Management

**Feature**: Employee Innovation Ideas Management  
**Date**: 2026-05-14  
**Section**: Admin review workflow and status updates

---

## Admin Ideas List Endpoint

### Endpoint

```
GET /admin/ideas
```

### Request

**Authentication**: Required (`[Authorize(Roles = "Admin")]`)
**User Role**: Admin only

**Query Parameters**:

| Parameter | Type    | Required | Values                                     | Notes                          |
| --------- | ------- | -------- | ------------------------------------------ | ------------------------------ |
| status    | string  | NO       | Submitted, UnderReview, Accepted, Rejected | Filter by status               |
| page      | integer | NO       | 1+                                         | Pagination (10 ideas per page) |
| sortBy    | string  | NO       | date, status                               | Sort order                     |

**Example**:

```
GET /admin/ideas?status=UnderReview&sortBy=date&page=1
```

### Response

**Success (200 OK)**:

**Content-Type**: `text/html` (Razor view)

**View Model**:

```csharp
public class AdminIdeasListViewModel
{
    public List<AdminIdeaListItemDTO> Ideas { get; set; }
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string FilterStatus { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } // For status summary
}

public class AdminIdeaListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string SubmitterName { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public int AttachmentCount { get; set; }
}
```

**View Content**:

- Dashboard header: "Innovation Ideas Under Review"
- Status summary: Count of Submitted, Under Review, Accepted, Rejected
- Filter dropdown: Quick filter by status
- Table/card layout showing ALL submitted ideas (from all submitters)
- Columns: Title, Submitter, Status (badge), Created Date, Actions
- "Review" button links to detail/review page
- Responsive design (works on tablet/desktop)

**Example HTML**:

```html
<div class="container mt-5">
    <h1>Admin: Innovation Ideas Review</h1>

    <!-- Status Summary -->
    <div class="row mb-4">
        <div class="col-md-3">
            <div class="card bg-light">
                <div class="card-body text-center">
                    <h5>@Model.StatusCounts["Submitted"]</h5>
                    <p>Submitted</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-warning">
                <div class="card-body text-center">
                    <h5>@Model.StatusCounts["UnderReview"]</h5>
                    <p>Under Review</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-success">
                <div class="card-body text-center">
                    <h5>@Model.StatusCounts["Accepted"]</h5>
                    <p>Accepted</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-danger">
                <div class="card-body text-center">
                    <h5>@Model.StatusCounts["Rejected"]</h5>
                    <p>Rejected</p>
                </div>
            </div>
        </div>
    </div>

    <!-- Filter & Sort -->
    <form method="get" class="mb-3">
        <div class="row">
            <div class="col-md-6">
                <select name="status" class="form-select" onchange="this.form.submit()">
                    <option value="">All Statuses</option>
                    <option value="Submitted" @(Model.FilterStatus == "Submitted" ? "selected" : "")>Submitted</option>
                    <option value="UnderReview" @(Model.FilterStatus == "UnderReview" ? "selected" : "")>Under Review</option>
                    <option value="Accepted" @(Model.FilterStatus == "Accepted" ? "selected" : "")>Accepted</option>
                    <option value="Rejected" @(Model.FilterStatus == "Rejected" ? "selected" : "")>Rejected</option>
                </select>
            </div>
        </div>
    </form>

    <!-- Ideas List -->
    @if (Model.Ideas.Any())
    {
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Title</th>
                    <th>Submitter</th>
                    <th>Status</th>
                    <th>Created</th>
                    <th>Files</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var idea in Model.Ideas)
                {
                    <tr>
                        <td>@idea.Title</td>
                        <td>@idea.SubmitterName</td>
                        <td><span class="badge bg-secondary">@idea.Status</span></td>
                        <td>@idea.CreatedDate.ToString("MMM dd, yyyy")</td>
                        <td>@idea.AttachmentCount</td>
                        <td><a href="/admin/ideas/@idea.Id/review" class="btn btn-sm btn-primary">Review</a></td>
                    </tr>
                }
            </tbody>
        </table>

        <!-- Pagination -->
        @if (Model.TotalPages > 1)
        {
            <nav>
                <ul class="pagination">
                    @for (int i = 1; i <= Model.TotalPages; i++)
                    {
                        <li class="page-item @(i == Model.CurrentPage ? "active" : "")">
                            <a class="page-link" href="/admin/ideas?status=@Model.FilterStatus&page=@i">@i</a>
                        </li>
                    }
                </ul>
            </nav>
        }
    }
    else
    {
        <p class="alert alert-info">No ideas found matching your filter.</p>
    }
</div>
```

**Authorization Check**:

- Only Admin users can access (`[Authorize(Roles = "Admin")]`)
- Return 403 Forbidden if non-admin tries to access

**Failure (403 Forbidden)**:

```
Status: 403
Body: "You do not have permission to access this page"
Redirect: /auth/login or dashboard
```

### Business Logic

1. **Check authorization**: User must have "Admin" role
2. **Load all ideas** (from all submitters)
3. **Filter by status** (if provided): Submitted, UnderReview, Accepted, Rejected
4. **Count by status**: Calculate summary for cards
5. **Paginate**: 10 ideas per page
6. **Order**: Sort by CreatedDate DESC (newest first), or Status if requested
7. **Return**: Display admin review dashboard

### Implementation Location

- **Controller**: `AdminController.Index(string status, int page = 1, string sortBy = "date")`
- **Service**: `IdeaService.GetAllIdeasAsync(IdeaStatus? status, int page, string sortBy)`
- **View**: `Views/Admin/Index.cshtml`

---

## Admin Review Idea Detail Endpoint

### Endpoint

```
GET /admin/ideas/{id}/review
```

### Request

**Authentication**: Required (`[Authorize(Roles = "Admin")]`)
**User Role**: Admin only

**URL Parameters**:

| Parameter | Type | Required | Notes           |
| --------- | ---- | -------- | --------------- |
| id        | GUID | YES      | Idea identifier |

### Response

**Success (200 OK)**:

**Content-Type**: `text/html`

**View Model**:

```csharp
public class AdminReviewViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CurrentStatus { get; set; }
    public string SubmitterName { get; set; }
    public string SubmitterEmail { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<IdeaAttachmentDTO> Attachments { get; set; }
    public List<AuditLogDTO> StatusHistory { get; set; }
}
```

**View Content**:

- Idea title, description
- Submitter name + email (for contact)
- Current status displayed prominently
- **Status Update Form**:
  - Dropdown: Select "Under Review", "Accepted", or "Rejected"
  - Submit button "Update Status"
- Attachments list with download links
- Status change history (AuditLog):
  - Table showing: Old Status → New Status, Admin name, Change date/time
- Back button to admin ideas list

**Example HTML**:

```html
<div class="container mt-5">
  <h1>@Model.Title</h1>
  <p>
    <strong>Submitter:</strong> @Model.SubmitterName (@Model.SubmitterEmail)
  </p>
  <p><strong>Created:</strong> @Model.CreatedDate.ToString("MMM dd, yyyy")</p>

  <!-- Current Status -->
  <div class="alert alert-info">
    <strong>Current Status:</strong>
    <span class="badge bg-primary">@Model.CurrentStatus</span>
  </div>

  <!-- Description -->
  <h3>Idea Description</h3>
  <p>@Model.Description</p>

  <!-- Attachments -->
  @if (Model.Attachments.Any()) {
  <h3>Attachments</h3>
  <ul class="list-group">
    @foreach (var attachment in Model.Attachments) {
    <li class="list-group-item">
      <a href="/ideas/@Model.Id/download/@attachment.Id"
        >@attachment.FileName</a
      >
      <small class="text-muted">(@(attachment.FileSize / 1024.0)KB)</small>
    </li>
    }
  </ul>
  }

  <!-- Status Update Form -->
  <div class="card mt-4">
    <div class="card-header bg-primary text-white">
      <h5>Update Status</h5>
    </div>
    <div class="card-body">
      <form method="post" action="/admin/ideas/@Model.Id/status">
        @Html.AntiForgeryToken()

        <div class="mb-3">
          <label for="NewStatus" class="form-label">New Status</label>
          <select name="NewStatus" class="form-select" required>
            <option value="">-- Select Status --</option>
            <option value="Submitted">Submitted</option>
            <option value="UnderReview">Under Review</option>
            <option value="Accepted">Accepted</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>

        <button type="submit" class="btn btn-primary">Update Status</button>
        <a href="/admin/ideas" class="btn btn-secondary">Back to List</a>
      </form>
    </div>
  </div>

  <!-- Status Change History -->
  @if (Model.StatusHistory.Any()) {
  <h3 class="mt-4">Status Change History</h3>
  <table class="table table-sm">
    <thead>
      <tr>
        <th>Old Status</th>
        <th>New Status</th>
        <th>Admin</th>
        <th>Date</th>
      </tr>
    </thead>
    <tbody>
      @foreach (var log in Model.StatusHistory) {
      <tr>
        <td>@log.OldStatus</td>
        <td>@log.NewStatus</td>
        <td>@log.AdminName</td>
        <td>@log.ChangedDate.ToString("MMM dd, yyyy HH:mm")</td>
      </tr>
      }
    </tbody>
  </table>
  }
</div>
```

**Authorization Check**:

- Only Admin users can access
- Return 403 Forbidden if non-admin

**Failure (404 Not Found)**:

```
Status: 404
View: Error page "Idea not found"
```

### Business Logic

1. **Check authorization**: User must have "Admin" role
2. **Retrieve idea** by ID
3. **Load submitter information**: Name, email
4. **Load attachments**: All files attached to this idea
5. **Load status history**: All AuditLog entries for this idea
6. **Return**: Display review page

### Implementation Location

- **Controller**: `AdminController.Review(Guid id)`
- **Service**: `IdeaService.GetIdeaDetailAsync(Guid ideaId)` (with submitter + history)
- **View**: `Views/Admin/Review.cshtml`

---

## Update Idea Status Endpoint

### Endpoint

```
POST /admin/ideas/{id}/status
```

### Request

**Authentication**: Required (`[Authorize(Roles = "Admin")]`)
**Content-Type**: `application/x-www-form-urlencoded`

**URL Parameters**:

| Parameter | Type | Required | Notes           |
| --------- | ---- | -------- | --------------- |
| id        | GUID | YES      | Idea identifier |

**Form Parameters**:

| Parameter | Type   | Required | Values                                     | Notes      |
| --------- | ------ | -------- | ------------------------------------------ | ---------- |
| NewStatus | string | YES      | Submitted, UnderReview, Accepted, Rejected | New status |

### Response

**Success (302 Redirect)**:

```
Redirect: GET /admin/ideas/{id}/review
Status: 200 (with success message)
Body: "Status updated successfully"
```

**Failure (400 Bad Request)**:

```
Status: 400
Body: "Invalid status value" or "Idea not found"
```

**Failure (403 Forbidden)**:

```
Status: 403
Body: "You do not have permission to update ideas"
```

### Validation Rules

- **NewStatus**: Must be valid enum value (Submitted, UnderReview, Accepted, Rejected)
- **Idea exists**: ID must reference existing idea
- **Admin role**: Only admins can update status

### Business Logic

1. **Check authorization**: User must have "Admin" role
2. **Retrieve idea** by ID
3. **Validate new status**: Must be valid enum value
4. **Check for changes**: Is new status different from current status?
5. **Update status**:
   - Set `Idea.Status = newStatus`
   - Set `Idea.LastModifiedByAdminId = current admin ID`
   - Set `Idea.LastModifiedDate = DateTime.UtcNow`
6. **Create audit log**:
   - Save entry: `IdeaId, OldStatus, NewStatus, ChangedByAdminId, ChangedDate`
   - For submitter transparency
   - For compliance tracking
7. **Return**: Redirect back to review page with success message

### Implementation Location

- **Controller**: `AdminController.UpdateStatus(Guid id, UpdateIdeaStatusDTO dto)`
- **Service**: `IdeaService.UpdateIdeaStatusAsync(Guid ideaId, string newStatus, Guid adminId)`
- **Repository**: `AuditLogRepository.AddAsync(auditLog)`

---

## Audit Log Model (for status tracking)

```csharp
public class AuditLogDTO
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public string AdminName { get; set; }
    public DateTime ChangedDate { get; set; }
}
```

### Audit Log Display

Every status change is recorded and displayed in review history:

| Change                   | Shown in History                             |
| ------------------------ | -------------------------------------------- |
| Submitted → Under Review | "Under Review by Admin Name on May 14, 2026" |
| Under Review → Accepted  | "Accepted by Admin Name on May 14, 2026"     |
| Accepted → Under Review  | "Moved back to Under Review by Admin Name"   |
| Under Review → Rejected  | "Rejected by Admin Name on May 14, 2026"     |

---

## Authorization & Security

**Authentication Required**: All endpoints require `[Authorize(Roles = "Admin")]`

**Admin-Only Operations**:

- View all submitted ideas (not just their own)
- Review idea details with submitter info
- Update idea status
- Access admin dashboard

**Data Protection**:

- Admins can see submitter email (for communication)
- Status change history audited
- Unauthorized access → 403 Forbidden

---

## Constitution Alignment (Principles IV, IX, X)

✅ **Principle IV (Security & RBAC)**: Admin role required for all review actions
✅ **Principle IX (Structured Error Handling)**: Explicit validation, user-friendly messages
✅ **Principle X (Specification-Driven)**: All endpoints match specification requirements
✅ **Audit Trail**: Every status change logged with admin + timestamp

---

**Contract Version**: 1.0.0 | **Date**: 2026-05-14 | **Status**: Complete
