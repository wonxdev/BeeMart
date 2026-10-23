# Changelog

All notable changes to the **BeeMart POS** project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-10-23

### Added
- **Backend API (`BeeMart.Api`)**:
  - RESTful API backend built with ASP.NET Core (.NET 8) and Entity Framework Core 8.
  - Standardized `ApiResponse<T>` JSON envelope across all `/api/v1/...` endpoints.
  - Authentication module with BCrypt password hashing (work factor 11), 5-digit `IdUser` generation, and HMAC-SHA256 JWT token issuance (`POST /api/v1/auth/login` and `POST /api/v1/auth/register`).
  - Master data endpoints for active payment methods (`GET /api/v1/payment-methods`) and paginated, in-stock product search (`GET /api/v1/products/search`).
  - POS checkout engine (`POST /api/v1/transactions`) with sequential daily invoice numbers (`INV-YYYYMMDD-XXXX`), server-side stock and payment validation, and atomic inventory deduction.
  - Role-isolated transaction history endpoint (`GET /api/v1/transactions/history`) enforcing cashier self-scoping (`Cashier`) and global supervision with cashier filtering (`Admin`).
- **Database (`Database/BeeMart.sql`)**:
  - Idempotent SQL Server DDL & DML script defining `MsUser`, `LtPaymentMethod`, `MsProduct`, `TrTransaction`, and `TrTransactionDetail` with indexes and foreign key constraints.
  - Initial seed data for 3 payment methods (`Cash`, `Debit`, `Credit`), 6 staff accounts (`admin`, `steven`, `gabri`, `mona`, `felix`, `gobuta`), and 22 grocery products.
- **Web Frontend (`BeeMart.Web`)**:
  - Decoupled ASP.NET Core Razor Pages client consuming the backend API over HTTP via a typed `IApiClient` service with zero direct database dependencies.
  - Cookie Authentication session management with automatic JWT Bearer token forwarding.
  - Custom responsive CSS stylesheet (`wwwroot/css/site.css`) and vanilla JavaScript utilities (`wwwroot/js/site.js`) built without external CSS frameworks.
  - Login, Registration, Logout, and Access Denied pages (`/Auth/...`).
  - Interactive POS cash register page (`/Transaction/New`) with live product search, stock-validated cart controls, real-time change calculation, and a receipt modal.
  - Expandable transaction history page (`/Transaction/History`) with date-range filtering, role-aware cashier selection, pagination, and inline line-item details.

### Security
- Enforced BCrypt password hashing (`BCrypt.Net-Next`, work factor 11) with unique per-user salts for all seeded and newly registered accounts.
- Configured `HttpOnly` authentication cookies on `BeeMart.Web` and strict JWT token validation (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey`) on `BeeMart.Api`.
- Implemented server-side role authorization so `Cashier` accounts can only query their own transaction history while `Transaction/New` is restricted to `Cashier` sessions.
- Protected Razor Page AJAX endpoints with ASP.NET Core Anti-Forgery tokens (`RequestVerificationToken`).
