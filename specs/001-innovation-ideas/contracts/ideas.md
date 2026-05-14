# API Contract: Ideas Management (Submitter & Listing)

**Feature**: Employee Innovation Ideas Management  
**Date**: 2026-05-14  
**Section**: Create, List, View Ideas (for submitters)

---

## List My Ideas Endpoint

### Endpoint

```
GET /ideas
```

### Request

**Authentication**: Required (`[Authorize]`)
**User Role**: Submitter or Admin

**Query Parameters**:

| Parameter | Type    | Required | Values                                     | Notes                          |
| --------- | ------- | -------- | ------------------------------------------ | ------------------------------ |
| status    | string  | NO       | Submitted, UnderReview, Accepted, Rejected | Filter by status               |
| page      | integer | NO       | 1+                                         | Pagination (10 items per page) |

**Example**:

```
GET /ideas?status=Submitted&page=1
```

### Response

**Success (200 OK)**:

**Content-Type**: `text/html` (Razor view)

**View Model**:

```csharp
public class IdeaListViewModel
{
    public List<IdeaListItemDTO> Ideas { get; set; }
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string FilterStatus { get; set; }
}
```

**View Content**:

- Responsive table/card layout (Bootstrap)
- Each idea shows: Title, Status (badge), Created Date
- "View" button links to detail page
- "Create New Idea" button links to create page
- Status filter dropdown for quick filtering
- Pagination if more than 10 ideas

**Example HTML Structure**:

```html
<div class="container mt-5">
    <h1>My Innovation Ideas</h1>

    <a href="/ideas/create" class="btn btn-primary mb-3">Create New Idea</a>

    <!-- Status Filter -->
    <form method="get" class="mb-3">
        <select name="status" class="form-select" onchange="this.form.submit()">
            <option value="">All Statuses</option>
            <option value="Submitted">Submitted</option>
            <option value="UnderReview">Under Review</option>
            <option value="Accepted">Accepted</option>
            <option value="Rejected">Rejected</option>
        </select>
    </form>

    <!-- Ideas List -->
    @if (Model.Ideas.Any())
    {
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Title</th>
                    <th>Status</th>
                    <th>Created</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var idea in Model.Ideas)
                {
                    <tr>
                        <td>@idea.Title</td>
                        <td><span class="badge bg-info">@idea.Status</span></td>
                        <td>@idea.CreatedDate.ToString("MMM dd, yyyy")</td>
                        <td><a href="/ideas/@idea.Id" class="btn btn-sm btn-outline-primary">View</a></td>
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
                            <a class="page-link" href="/ideas?page=@i">@i</a>
                        </li>
                    }
                </ul>
            </nav>
        }
    }
    else
    {
        <p class="alert alert-info">No ideas submitted yet. <a href="/ideas/create">Create your first idea</a>!</p>
    }
</div>
```

### Business Logic

1. **Submitter view**: Load only ideas where `SubmitterId == current user ID`
2. **Admin view**: Load ALL ideas regardless of submitter
3. **Filter**: If `status` parameter provided, filter by `Idea.Status`
4. **Pagination**: Load 10 ideas per page, calculate total pages
5. **Order**: Sort by `CreatedDate DESC` (newest first)

### Implementation Location

- **Controller**: `IdeasController.Index(string status, int page = 1)`
- **Service**: `IdeaService.GetMyIdeasAsync(Guid submitterId, IdeaStatus? status, int page)`
- **View**: `Views/Ideas/Index.cshtml`

---

## Create Idea Page (GET)

### Endpoint

```
GET /ideas/create
```

### Request

**Authentication**: Required (`[Authorize]`)
**User Role**: Submitter or Admin

**Query Parameters**: None

### Response

**Success (200 OK)**:

**Content-Type**: `text/html`

**View Model**:

```csharp
public class CreateIdeaViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile Attachment { get; set; }
}
```

**View Content**:

- Form with text inputs: Title, Description
- File upload input (accept only allowed types)
- Submit button
- Validation error messages displayed above fields

**Example HTML**:

```html
<div class="container mt-5">
    <h1>Submit New Idea</h1>

    <form method="post" enctype="multipart/form-data">
        @Html.AntiForgeryToken()

        <div class="mb-3">
            <label for="Title" class="form-label">Idea Title *</label>
            <input type="text" class="form-control @(ViewData.ModelState["Title"]?.Errors.Any() == true ? "is-invalid" : "")"
                   id="Title" name="Title" placeholder="Give your idea a catchy title"
                   maxlength="200" required />
            <span class="invalid-feedback">@ViewData.ModelState["Title"]?.Errors.First()?.ErrorMessage</span>
        </div>

        <div class="mb-3">
            <label for="Description" class="form-label">Description</label>
            <textarea class="form-control" id="Description" name="Description"
                      placeholder="Explain your idea in detail (max 2000 characters)"
                      rows="5" maxlength="2000"></textarea>
        </div>

        <div class="mb-3">
            <label for="Attachment" class="form-label">Attachment (optional)</label>
            <input type="file" class="form-control" id="Attachment" name="Attachment"
                   accept=".pdf,.doc,.docx,.xls,.xlsx,.jpg,.jpeg,.png" />
            <small class="form-text text-muted">Allowed: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG (max 10MB)</small>
        </div>

        <button type="submit" class="btn btn-primary">Submit Idea</button>
        <a href="/ideas" class="btn btn-secondary">Cancel</a>
    </form>
</div>
```

---

## Create Idea (POST)

### Endpoint

```
POST /ideas/create
```

### Request

**Authentication**: Required (`[Authorize]`)
**Content-Type**: `multipart/form-data`

**Form Parameters**:

| Parameter   | Type   | Required | Validation                    | Notes         |
| ----------- | ------ | -------- | ----------------------------- | ------------- |
| Title       | string | YES      | 1-200 chars                   | Idea title    |
| Description | string | NO       | 0-2000 chars                  | Idea details  |
| Attachment  | file   | NO       | 10MB max, validated MIME type | Optional file |

### Response

**Success (302 Redirect)**:

```
Redirect: GET /ideas/{ideaId}
Body: Success message "Idea submitted successfully!"
```

**Failure (400 Bad Request)**:

```
Status: 200 (return form with errors)
View: Views/Ideas/Create.cshtml (with error messages displayed)
Body: ModelState errors for each invalid field
```

### Validation Rules

- **Title**: Required, 1-200 characters, non-null
- **Description**: Optional, max 2000 characters
- **Attachment**: Optional
  - File size: ≤ 10MB (10,485,760 bytes)
  - MIME type: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG (validated via magic bytes)
  - Filename: Max 255 characters
  - No executable files (.exe, .bat, .ps1, .zip, etc.)

### Business Logic

1. **Validate input**: FluentValidation checks title length, description length
2. **Validate file** (if uploaded):
   - Check size ≤ 10MB
   - Detect MIME type via magic bytes (not extension)
   - Reject if not in whitelist
   - Return user-friendly error if validation fails
3. **Create Idea**:
   - Set `Status = IdeaStatus.Submitted`
   - Set `SubmitterId = current user ID`
   - Set `CreatedDate = DateTime.UtcNow`
   - Set `LastModifiedDate = DateTime.UtcNow`
4. **Save attachment** (if present):
   - Hash filename for security (prevent directory traversal)
   - Store in `upload_storage/ideas/{IdeaId}/`
   - Create `IdeaAttachment` record in database
5. **Return**: Redirect to idea detail page with success message

### Implementation Location

- **Controller**: `IdeasController.Create(CreateIdeaViewModel)`
- **Service**: `IdeaService.CreateIdeaAsync(Guid submitterId, string title, string description, IFormFile attachment)`
- **Validator**: `CreateIdeaValidator` (FluentValidation)
- **File Service**: `FileValidationService.ValidateUploadAsync(file, maxSize)`

---

## View Idea Detail Endpoint

### Endpoint

```
GET /ideas/{id}
```

### Request

**Authentication**: Required (`[Authorize]`)

**URL Parameters**:

| Parameter | Type | Required | Notes           |
| --------- | ---- | -------- | --------------- |
| id        | GUID | YES      | Idea identifier |

### Response

**Success (200 OK)**:

**Content-Type**: `text/html`

**View Model**:

```csharp
public class IdeaDetailViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string SubmitterName { get; set; }
    public List<IdeaAttachmentDTO> Attachments { get; set; }
}
```

**View Content**:

- Idea title (heading)
- Status badge (colored: Submitted=blue, UnderReview=yellow, Accepted=green, Rejected=red)
- Description (full text)
- Submitter name, Created/Modified dates
- Attachment list with download links (if any)
- Back button to ideas list

**Example HTML**:

```html
<div class="container mt-5">
  <div class="row">
    <div class="col-12">
      <h1>@Model.Title</h1>
      <span class="badge bg-info">@Model.Status</span>

      <div class="card mt-3">
        <div class="card-body">
          <p><strong>Submitted by:</strong> @Model.SubmitterName</p>
          <p>
            <strong>Created:</strong> @Model.CreatedDate.ToString("MMM dd,
            yyyy")
          </p>
          <p>
            <strong>Last Modified:</strong>
            @Model.LastModifiedDate.ToString("MMM dd, yyyy")
          </p>

          <h4>Description</h4>
          <p>@Model.Description</p>

          @if (Model.Attachments.Any()) {
          <h4>Attachments</h4>
          <ul>
            @foreach (var attachment in Model.Attachments) {
            <li>
              <a href="/ideas/@Model.Id/download/@attachment.Id"
                >@attachment.FileName</a
              >
              <small>(@(attachment.FileSize / 1024.0)KB)</small>
            </li>
            }
          </ul>
          }
        </div>
      </div>

      <a href="/ideas" class="btn btn-secondary mt-3">Back to Ideas</a>
    </div>
  </div>
</div>
```

**Authorization Check**:

- Submitters can view only their own ideas
- Admins can view all ideas
- If unauthorized: Return 403 Forbidden

**Failure (404 Not Found)**:

```
Status: 404
View: Error page "Idea not found"
```

### Business Logic

1. **Retrieve idea** by ID
2. **Check authorization**:
   - If submitter: Must own idea (SubmitterId == current user)
   - If admin: Can view any idea
   - Return 403 Forbidden if unauthorized
3. **Load attachments**: Fetch related `IdeaAttachments`
4. **Load submitter name**: Join with User table
5. **Return**: Display detail view

### Implementation Location

- **Controller**: `IdeasController.Detail(Guid id)`
- **Service**: `IdeaService.GetIdeaDetailAsync(Guid ideaId, Guid currentUserId, string userRole)`
- **View**: `Views/Ideas/Detail.cshtml`

---

## Download Attachment Endpoint

### Endpoint

```
GET /ideas/{ideaId}/download/{attachmentId}
```

### Request

**Authentication**: Required (`[Authorize]`)

**URL Parameters**:

| Parameter    | Type | Required | Notes                 |
| ------------ | ---- | -------- | --------------------- |
| ideaId       | GUID | YES      | Parent idea ID        |
| attachmentId | GUID | YES      | Attachment identifier |

### Response

**Success (200 OK)**:

**Content-Type**: `application/octet-stream` or detected MIME type

**Headers**:

```
Content-Disposition: attachment; filename="original-filename.pdf"
Content-Length: 1024000
```

**Body**: Binary file contents

**Failure (404 Not Found)**:

```
Status: 404
Body: "Attachment not found"
```

**Failure (403 Forbidden)**:

```
Status: 403
Body: "You do not have access to this idea"
```

### Business Logic

1. **Retrieve attachment** by attachmentId
2. **Get parent idea** from attachment.IdeaId
3. **Check authorization**: Can user view this idea?
   - Submitter: Must own the idea
   - Admin: Can download from any idea
4. **Retrieve file** from `upload_storage/ideas/{IdeaId}/{HashedFileName}`
5. **Stream file** as binary download (preserves original filename in Content-Disposition)
6. **Error handling**: File not found → 404; Unauthorized → 403

### Implementation Location

- **Controller**: `IdeasController.Download(Guid ideaId, Guid attachmentId)`
- **Service**: `IdeaService.GetAttachmentAsync(Guid attachmentId)`

---

## Authorization & Security

**Authentication Required**: All endpoints require `[Authorize]` attribute

**Data Isolation**:

- Submitters see/download only their own ideas
- Admins see all ideas
- Authorization checked at service layer (Constitution Principle IV)

**File Security**:

- Files stored outside `wwwroot/` (not directly accessible via HTTP)
- File access controlled via service-layer permission checks
- Hashed filenames prevent directory traversal attacks

---

**Contract Version**: 1.0.0 | **Date**: 2026-05-14 | **Status**: Complete
