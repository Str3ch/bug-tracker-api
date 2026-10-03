# BugTracker API
![CI](https://github.com/Str3ch/bug-tracker-api/actions/workflows/ci.yml/badge.svg)
A RESTful bug tracking API built with ASP.NET Core 8.

The project demonstrates authentication, role-based authorization, Entity Framework Core, workflow validation, audit logging, integration testing, and API documentation.

## Features

- JWT authentication
- Role-based authorization: Admin, Developer, Tester
- Project and bug management
- Bug assignment workflow
- Controlled status transitions
- Status history
- Comments and tags
- Search, filtering and pagination
- Soft delete
- Audit logging
- Admin dashboard
- Global error handling
- Swagger/OpenAPI
- xUnit integration tests
- Postman collection

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- SQLite in-memory for integration tests
- JWT Bearer Authentication
- BCrypt.Net
- Swagger
- xUnit
- Postman

## Roles

### Admin

- Manage projects
- Manage users and roles
- Assign and unassign developers
- Manage tags
- Delete bugs
- Access dashboard and audit logs

### Tester

- Create and update bugs
- Add comments and tags
- Perform QA-related status transitions

### Developer

- Work with assigned bugs
- Add comments
- Change bug status according to workflow

## Bug Workflow

```text
Open -> InProgress -> Resolved -> Closed

Resolved -> Reopened
Closed   -> Reopened
Reopened -> InProgress
```

Developers can change the status only of bugs assigned to them.

## Main Endpoints

```text
POST   /api/Auth/register
POST   /api/Auth/login
GET    /api/Auth/me

GET    /api/Projects
GET    /api/Projects/{id}
POST   /api/Projects
PUT    /api/Projects/{id}
DELETE /api/Projects/{id}

GET    /api/Bugs
GET    /api/Bugs/{id}
POST   /api/Bugs
PUT    /api/Bugs/{id}
DELETE /api/Bugs/{id}

PUT    /api/Bugs/{id}/assign
PUT    /api/Bugs/{id}/status
GET    /api/Bugs/{id}/status-history

POST   /api/Bugs/{id}/comment
GET    /api/Bugs/{id}/comments

GET    /api/Tags
POST   /api/Tags
DELETE /api/Tags/{id}

POST   /api/Bugs/{id}/tags/{tagId}
GET    /api/Bugs/{id}/tags
DELETE /api/Bugs/{id}/tags/{tagId}

GET    /api/Dashboard
GET    /api/AuditLogs
```

## Getting Started

### Requirements

- .NET 8 SDK
- SQL Server or SQL Server Express

### Clone the repository

```bash
git clone https://github.com/Str3ch/bug-tracker-api.git
cd bug-tracker-api
```

### Configure JWT secret

The JWT signing key should not be committed to source control.

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR-SECURE-DEVELOPMENT-JWT-KEY"
```

JWT issuer and audience are configured in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "BugTracker.Api",
    "Audience": "BugTracker.Client"
  }
}
```

### Configure the database

The development configuration uses SQL Server.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BugTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Change the connection string if your SQL Server configuration is different.

### Apply migrations

```bash
dotnet ef database update
```

If `dotnet-ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

### Run the API

```bash
dotnet run
```

Swagger is available in Development mode at:

```text
https://localhost:<port>/swagger
```

The actual port is shown in the application console.

## Authentication

Public registration creates users with the `Tester` role.

Example login request:

```http
POST /api/Auth/login
```

```json
{
  "username": "tester",
  "password": "your-password"
}
```

Authenticated requests use:

```text
Authorization: Bearer <JWT_TOKEN>
```

## Integration Tests

Integration tests are located in:

```text
tests/BugTracker.Api.IntegrationTests
```

They use:

- xUnit
- `WebApplicationFactory`
- SQLite in-memory database
- the real ASP.NET Core authentication and authorization pipeline

Run all tests:

```bash
dotnet test BugTracker.Api.sln
```

The tests cover authentication, authorization, validation, bug creation, assignment, invalid assignment, status workflow and status history.
## Continuous Integration

GitHub Actions automatically runs on pushes and pull requests to `main`.

The CI pipeline:

- restores NuGet dependencies;
- builds the solution in Release configuration;
- runs the integration test suite.
## Postman

The Postman collection is located in:

```text
postman/BugTracker.Api.postman_collection.json
```

It contains requests for:

- authentication
- projects
- bugs
- assignment and status workflow
- comments
- tags
- dashboard
- audit logs

Set the `baseUrl` collection variable to the local API address before running the collection.

## Security

- Passwords are stored as BCrypt hashes
- JWT signing secrets are stored outside source control
- New users cannot register as Admin
- Role-based authorization protects privileged endpoints
- Only Developers can be assigned to bugs
- Internal exceptions are not exposed to API clients

## Author

Backend portfolio project built with ASP.NET Core and .NET 8.