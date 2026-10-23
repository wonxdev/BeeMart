# Project Directory Structure

This document outlines the directory layout and key components of the **BeeMart POS** solution.

## Directory Tree

```text
BeeMart/
├── BeeMart.sln                          # Visual Studio / .NET 8 solution file
├── README.md                            # Project overview and quick start guide
├── SPECIFICATION.md                     # Functional and technical specifications
├── STRUCTURE.md                         # Directory structure overview
├── CHANGELOG.md                         # Release version history
├── OPEN-ITEMS.md                        # Project status and future enhancements
├── Database/
│   └── BeeMart.sql                      # Idempotent SQL Server DDL & seed data script
├── BeeMart.Api/                         # Backend RESTful Web API (Port 5001 / 7001)
│   ├── Program.cs                       # DI registration, EF Core & JWT middleware setup
│   ├── appsettings.json                 # SQL Server connection string & JWT settings
│   ├── Common/                          # ApiResponse wrapper, BCrypt hasher, JWT & claims helpers
│   ├── Controllers/                     # REST controllers (/api/v1/auth, products, payment-methods, transactions)
│   ├── Data/
│   │   └── BeeMartDbContext.cs          # EF Core DbContext and Fluent API schema mappings
│   └── Models/
│       ├── Entities/                    # Database entities (MsUser, MsProduct, LtPaymentMethod, TrTransaction, TrTransactionDetail)
│       └── DTOs/                        # Request and response contracts for Auth, Master, and Transaction endpoints
└── BeeMart.Web/                         # Frontend Razor Pages Client (Port 5002 / 7002)
    ├── Program.cs                       # Cookie authentication, typed HttpClient, and Razor Pages pipeline
    ├── appsettings.json                 # Backend API base URL configuration (http://localhost:5001/api/v1/)
    ├── Models/                          # ViewModels for authentication forms, POS cart, and transaction history
    ├── Services/
    │   ├── IApiClient.cs                # HTTP client contract and frontend ApiResponse wrapper
    │   └── ApiClient.cs                 # Typed HttpClient service with automatic JWT Bearer forwarding
    ├── Pages/
    │   ├── Shared/_Layout.cshtml        # Main application shell, navigation tabs, and live session header
    │   ├── Auth/                        # Login, Register, Logout, and AccessDenied Razor Pages
    │   └── Transaction/                 # POS cash register (New.cshtml) and role-scoped history (History.cshtml)
    └── wwwroot/
        ├── css/site.css                 # Custom responsive stylesheet (no external CSS frameworks)
        └── js/site.js                   # Client-side utilities for live clock, Rupiah formatting, and alerts
```

## Main Directory Descriptions

- **`Database/`**: Contains `BeeMart.sql`, a re-runnable T-SQL script that creates the `BeeMart` database, defines all 5 tables with indexes and constraints, and seeds initial payment methods, staff accounts, and 22 grocery products.
- **`BeeMart.Api/Common`**: Houses shared backend infrastructure including the standardized `ApiResponse<T>` JSON envelope, `PasswordHasher` (BCrypt), `JwtTokenService`, and `ClaimsHelper` extension methods.
- **`BeeMart.Api/Controllers`**: Exposes versioned REST endpoints (`/api/v1/...`) for user authentication, product catalog search, payment method lookup, atomic POS checkout, and role-filtered transaction history.
- **`BeeMart.Api/Data` & `Models/`**: Defines `BeeMartDbContext`, the 5 EF Core entity classes mapped to SQL Server tables, and strongly-typed DTOs for API requests and responses.
- **`BeeMart.Web/Services`**: Implements `ApiClient`, which wraps `HttpClient` to communicate with `BeeMart.Api` and automatically attaches the logged-in user's JWT from their authentication cookie.
- **`BeeMart.Web/Pages/Auth`**: Provides the user login, account registration, logout, and access-denied views backed by ASP.NET Core Cookie Authentication.
- **`BeeMart.Web/Pages/Transaction`**: Contains the interactive POS checkout page (`New.cshtml`) for cashiers and the expandable transaction history report (`History.cshtml`) for both cashiers and admins.
- **`BeeMart.Web/wwwroot`**: Serves static frontend assets, featuring a custom CSS design system (`site.css`) and lightweight vanilla JavaScript helpers (`site.js`).
