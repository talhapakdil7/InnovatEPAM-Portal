# API Contract: Authentication

**Feature**: Employee Innovation Ideas Management  
**Date**: 2026-05-14  
**Section**: User Registration, Login, Logout

---

## User Registration Endpoint

### Endpoint

```
POST /auth/register
```

### Request

**Content-Type**: `application/x-www-form-urlencoded` or `multipart/form-data`

**Form Parameters**:

| Parameter       | Type   | Required | Validation                                       | Notes               |
| --------------- | ------ | -------- | ------------------------------------------------ | ------------------- |
| Email           | string | YES      | Valid email format, unique                       | Employee email      |
| FirstName       | string | YES      | 1-100 chars                                      | Display name        |
| LastName        | string | YES      | 1-100 chars                                      | Display name        |
| Password        | string | YES      | 12+ chars, 1 upper, 1 lower, 1 number, 1 special | OWASP complexity    |
| ConfirmPassword | string | YES      | Must match Password                              | Security validation |

### Response

**Success (201 Created)**:

```
Redirect: GET /auth/login
Header: Set-Cookie: asp.net_sessionid=...
Body: Registration successful message (in view)
```

**Failure (400 Bad Request)**:

```json
{
  "errors": {
    "Email": ["Email already registered"],
    "Password": ["Password must be at least 12 characters"],
    "ConfirmPassword": ["Passwords do not match"]
  }
}
```

### Validation Rules

- **Email**: Must be valid format (`user@domain.com`), unique across all users
- **FirstName/LastName**: Required, 1-100 characters each
- **Password**: OWASP complexity - min 12 chars, 1+ uppercase, 1+ lowercase, 1+ digit, 1+ special char
- **ConfirmPassword**: Must exactly match Password field

### Business Logic

1. Check email uniqueness
2. Hash password using ASP.NET Core Identity
3. Create user with "Submitter" role (default)
4. Log registration in application logs
5. Redirect to login page

### Implementation Location

- **Controller**: `AuthController.Register(RegisterViewModel)`
- **Service**: `AuthService.RegisterAsync(email, password, firstName, lastName)`
- **Validator**: `RegisterValidator` (FluentValidation)

---

## User Login Endpoint

### Endpoint

```
POST /auth/login
```

### Request

**Content-Type**: `application/x-www-form-urlencoded` or `multipart/form-data`

**Form Parameters**:

| Parameter  | Type    | Required | Validation         | Notes                  |
| ---------- | ------- | -------- | ------------------ | ---------------------- |
| Email      | string  | YES      | Valid email format | Employee email         |
| Password   | string  | YES      | Non-empty          | User password          |
| RememberMe | boolean | NO       | true/false         | Extend session timeout |

### Response

**Success (302 Redirect)**:

```
Redirect: GET /ideas
Header: Set-Cookie: .AspNetCore.Identity.Application=...; Path=/; HttpOnly; Secure
Header: Set-Cookie: asp.net_sessionid=...; Path=/; HttpOnly
Body: (none)
```

**Failure (400 Bad Request)**:

```
Redirect: GET /auth/login
Body: Error message "Invalid email or password"
```

### Validation Rules

- **Email**: Must be non-empty, valid format
- **Password**: Must be non-empty

### Business Logic

1. Validate email exists in Users table
2. Compare provided password against PasswordHash (ASP.NET Core Identity)
3. If match, create authentication cookie (httpOnly, Secure, SameSite=Strict)
4. Set session timeout: 30 minutes (normal users) or 15 minutes (admin users)
5. Log successful login (for audit trail)
6. Redirect to /ideas dashboard
7. If no match, log failed attempt and show error (no email verification for security)

### Session Timeout

- **Normal users**: 30 minutes of inactivity
- **Admin users**: 15 minutes of inactivity
- **Sliding expiration**: Cookie extended on each request
- **Custom middleware**: Checks user role to apply appropriate timeout

### Implementation Location

- **Controller**: `AuthController.Login(LoginViewModel)`
- **Service**: `AuthService.LoginAsync(email, password)`
- **Middleware**: `SessionTimeoutMiddleware` (custom, role-based timeout)

---

## User Logout Endpoint

### Endpoint

```
GET /auth/logout
```

### Request

**Query Parameters**: None

**Headers**:

- `Cookie: .AspNetCore.Identity.Application=...; asp.net_sessionid=...`

### Response

**Success (302 Redirect)**:

```
Redirect: GET /auth/login
Header: Set-Cookie: .AspNetCore.Identity.Application=; Expires=Thu, 01 Jan 1970 00:00:00 GMT; Path=/
Header: Set-Cookie: asp.net_sessionid=; Expires=Thu, 01 Jan 1970 00:00:00 GMT; Path=/
Body: (none)
```

### Business Logic

1. Retrieve current user from authentication context
2. Sign out user (clear authentication cookie)
3. Clear session data
4. Log logout event
5. Redirect to login page

### Protected Endpoints

All endpoints except `/auth/register` and `/auth/login` require authentication:

```csharp
[Authorize]
public class IdeasController : Controller
{
    // All actions require authenticated user
    [HttpGet("/ideas")]
    public async Task<IActionResult> Index() { ... }
}

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // All actions require Admin role
    [HttpGet("/admin/ideas")]
    public async Task<IActionResult> Index() { ... }
}
```

---

## Authentication Architecture

```
User Request
    ↓
[Authentication Middleware] - Checks for valid authentication cookie
    ↓
[Authorization Middleware] - Checks [Authorize(Roles = ...)] attributes
    ↓
[Controller Action] - Processes authenticated/authorized request
    ↓
Response (or 401 Unauthorized / 403 Forbidden)
```

### Cookie Structure

```
.AspNetCore.Identity.Application
├── Claims
│   ├── sub (user ID)
│   ├── email
│   ├── role ("Submitter" or "Admin")
│   └── other metadata
├── Issued: <timestamp>
├── Expires: <30 minutes from now (normal) or 15 minutes (admin)>
└── Signature: HMAC-SHA256(secret_key)
```

---

## Security Requirements (Constitution Principle IV)

✅ **Password Complexity**: OWASP guidelines enforced
✅ **Session Timeout**: 30 min (users), 15 min (admins)
✅ **Cookie Security**: HttpOnly, Secure, SameSite=Strict
✅ **No Password Logging**: Passwords never appear in logs
✅ **Role-Based Access**: [Authorize(Roles = ...)] on protected actions
✅ **Centralized Auth**: ASP.NET Core Identity (single source of truth)

---

**Contract Version**: 1.0.0 | **Date**: 2026-05-14 | **Status**: Complete
