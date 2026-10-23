# Changelog

All notable changes to the BeeMart POS project will be documented in this file.

## [1.0.0] - 2026-10-23
### Added
- Complete Point of Sale (POS) backend API (.NET 8 Web API, EF Core, SQL Server).
- Authentication module with BCrypt password hashing and JWT Bearer tokens.
- Role-based authorization distinguishing Cashier and Admin permissions.
- Product catalog search with pagination and stock verification.
- Checkout system with sequential daily invoice numbering and stock deduction.
- Role-scoped transaction history reporting.
- Razor Pages frontend client with pure custom CSS responsive design.
- POS checkout terminal with dynamic cart, change calculation, and modal receipt display.