# IMS API Backend

Backend Code for our Group Coursework.

This project is a .NET 8 Web API for an Inventory Management System built with ASP.NET Core, Entity Framework Core, PostgreSQL, JWT authentication, and role-based authorization. It provides the server-side logic for managing customers, staff, vendors, parts, appointments, and invoices.

## Summary

This backend was developed for our Group Coursework as a complete API layer for the IMS application. It organizes the business logic into controllers, repositories, data contexts, and DTOs, and it exposes secure endpoints for customer, staff, and admin workflows.

## Features Added

1. JWT-based login and registration for customers and staff.
2. Role-based access control for Admin, Staff, and Customer users.
3. Admin staff management, including staff creation, status updates, and role updates.
4. Customer profile management and vehicle management.
5. Staff customer registration with vehicle details.
6. Customer appointment booking and appointment history access.
7. Vendor management with create, update, list, and status controls.
8. Part inventory management with create, update, delete, and status controls.
9. Purchase invoice creation and tracking for admin users.
10. Sales invoice creation, listing, and customer history access for staff and customers.

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger/OpenAPI

## Notes

- The API root redirects to Swagger in development.
- Database migrations are applied on startup when the configured PostgreSQL connection is available.