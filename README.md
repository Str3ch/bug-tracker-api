# BugTracker API

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
- Assign developers
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
Open ? InProgress ? Resolved ? Closed
                  ? Reopened
Closed ? Reopened ? InProgress