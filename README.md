# IMS API

Small backend for the Inventory Management System (coursework).

What it is
- A .NET 8 REST API handling customers, staff, vendors, parts, appointments, and invoices.
- Secure endpoints with JWT auth and role-based access.

Quick start
- Requirements: .NET 8 SDK and PostgreSQL.
- Configure the PostgreSQL connection in `appsettings.json` under `DefaultConnection`.
- Run: `dotnet run` from the project folder.
- In Development the API root redirects to Swagger for interactive docs.

Highlights
- JWT authentication and role-based authorization.
- CRUD for customers, staff, vendors, parts, and invoices.
- Appointment booking and history.
- Database migrations applied on startup when DB is available.

Tech
- ASP.NET Core, Entity Framework Core, PostgreSQL, Swagger.

Need help?
- Open an issue or contact the project owner.