# BeeMart POS — Functional & Technical Specifications

We built **BeeMart POS** as a full-stack Point of Sale web application in C# and .NET 8. The primary goal of the project is to demonstrate a clean separation of concerns between a stateless RESTful backend API and a decoupled Razor Pages web client, backed by Microsoft SQL Server.

---

## 1. Overview & Tech Stack

BeeMart POS handles daily retail store operations: user registration and role-based login, live product catalog lookups, cart management, cash register checkout with change calculation, and role-scoped transaction history reporting.

| Layer | Technology | Purpose |
| :--- | :--- | :--- |
| **Framework & Runtime** | .NET 8 (`net8.0`, C# 12) | Core application runtime for both backend and frontend projects |
| **Backend API** | ASP.NET Core Web API | RESTful endpoints (`/api/v1/...`), business validation, and data access |
| **ORM & Data Access** | Entity Framework Core 8 | Fluent API entity mapping, LINQ queries, and atomic database transactions |
| **Database** | Microsoft SQL Server | Relational storage initialized via an idempotent T-SQL script (`Database/BeeMart.sql`) |
| **Security** | BCrypt.Net-Next & JWT Bearer | Password hashing (work factor 11) and HMAC-SHA256 token authentication |
| **Frontend Web Client** | ASP.NET Core Razor Pages | Server-rendered UI and AJAX page handlers consuming the API over HTTP |
| **Styling & Scripting** | Custom CSS3 & Vanilla JS | Responsive layout built from scratch without external CSS frameworks |

---

## 2. Architecture

The solution (`BeeMart.sln`) is split into two independent projects that communicate strictly over HTTP/JSON:

```text
[ Browser Client ]
        │  HTTP / Razor Pages & AJAX Handlers
        ▼
[ BeeMart.Web ]  (Port 5002 / 7002)
  - Razor Pages UI, Cookie Auth session, custom CSS/JS
  - Zero EF Core or SQL Server dependencies
        │  HTTP JSON + Authorization: Bearer <JWT>
        ▼
[ BeeMart.Api ]  (Port 5001 / 7001)
  - Controllers, Business Logic, EF Core DbContext
        │  TDS / SQL Connection
        ▼
[ MS SQL Server: BeeMart ]
```

### Key Architectural Rules
1. **Strict Two-Tier Decoupling**: `BeeMart.Web` acts purely as an HTTP consumer through a typed `IApiClient` service pointed at `http://localhost:5001/api/v1/`. It contains no database connection strings, no EF Core packages, and no direct SQL references.
2. **Unified JSON Envelope**: Every API endpoint returns a consistent `ApiResponse<T>` JSON payload containing `status` (`"success"` or `"error"`), `message`, and `data`.
3. **Custom Frontend Design**: All layouts, tables, modals, badges, and POS controls in `BeeMart.Web/wwwroot/css/site.css` are written in pure CSS using custom properties (Amber `#F59E0B`, Dark Slate `#1E293B`, and POS Blue `#1E88E5`) with zero Bootstrap or Tailwind dependencies.

---

## 3. Database Schema

The `BeeMart` database consists of five relational tables under the `dbo` schema. Every table includes standard audit and soft-delete columns (`Stsrc` bit flag defaulting to `1`, `DateIn`, `UserIn`, `Dateup`, `Userup`), and foreign keys use `NO ACTION` (`DeleteBehavior.Restrict`) to preserve historical integrity.

### 3.1 `MsUser` (Master Users)
Stores staff accounts for cashiers and administrators.
- **Primary Key**: `IdUser` (`NVARCHAR(5)`) — 5-digit zero-padded string (e.g., `"00001"`, `"76985"`).
- **Columns**:
  - `Username` (`NVARCHAR(50)`, unique index `UQ_MsUser_Username`)
  - `FullName` (`NVARCHAR(300)`)
  - `PasswordHash` (`NVARCHAR(255)`)
  - `Role` (`NVARCHAR(20)`, constrained to `'Admin'` or `'Cashier'`)

### 3.2 `LtPaymentMethod` (Lookup Payment Methods)
Stores supported checkout payment types.
- **Primary Key**: `IdPaymentMethod` (`INT IDENTITY(1,1)`).
- **Columns**:
  - `MethodName` (`NVARCHAR(50)`, seeded with `'Cash'`, `'Debit'`, and `'Credit'`)

### 3.3 `MsProduct` (Master Products)
Stores retail inventory items available for sale.
- **Primary Key**: `IdProduct` (`NVARCHAR(36)`) — string GUID.
- **Columns**:
  - `Name` (`NVARCHAR(200)`, indexed via `IX_MsProduct_Name`)
  - `Weight` (`DECIMAL(10,2)`)
  - `Stock` (`INT`)
  - `Price` (`DECIMAL(18,2)`)
  - `Discount` (`DECIMAL(5,2)`)

### 3.4 `TrTransaction` (Transaction Headers)
Stores completed POS sale headers.
- **Primary Key**: `IdTransaction` (`NVARCHAR(36)`) — string GUID.
- **Columns**:
  - `InvoiceNo` (`NVARCHAR(20)`, unique index `UQ_TrTransaction_InvoiceNo`)
  - `TransactionDate` (`DATETIME`, composite index `IX_TrTransaction_TransactionDate_IdUser`)
  - `IdUser` (`NVARCHAR(5)` FK to `MsUser`)
  - `IdPaymentMethod` (`INT` FK to `LtPaymentMethod`)
  - `TotalPrice` (`DECIMAL(18,2)`)
- **Computed Mapping**: `TotalItem` is exposed on the C# model as `[NotMapped]` and calculated dynamically by summing line-item quantities (`SUM(Qty)`).

### 3.5 `TrTransactionDetail` (Transaction Line Items)
Stores individual products purchased within a transaction.
- **Primary Key**: `IdTransactionDetail` (`NVARCHAR(36)`) — string GUID.
- **Columns**:
  - `IdTransaction` (`NVARCHAR(36)` FK to `TrTransaction`)
  - `IdProduct` (`NVARCHAR(36)` FK to `MsProduct`)
  - `Qty` (`INT`)
  - `Price` (`DECIMAL(18,2)`)
  - `Discount` (`DECIMAL(5,2)`)
  - `SubTotal` (`DECIMAL(18,2)`)

---

## 4. Authentication & Roles

### 4.1 Password Hashing & JWT-Cookie Bridging
- **BCrypt Hashing**: Passwords are never stored in plain text. `PasswordHasher` hashes and verifies passwords using `BCrypt.Net-Next` with work factor `11`.
- **Token Issuance**: Upon valid login at `POST /api/v1/auth/login`, the API signs a 24-hour JWT containing `idUser`, `username`, `fullName`, and `role` claims.
- **Web Session Bridging**: `BeeMart.Web` stores the user's claims and raw JWT inside an encrypted `HttpOnly` authentication cookie (`BeeMart.Auth`). Whenever a Razor Page calls `IApiClient`, the client automatically extracts the `jwt_token` claim from `HttpContext.User` and attaches `Authorization: Bearer <token>` to outgoing API requests.

### 4.2 Role Permission Matrix

| Capability / Route | Guest (Unauthenticated) | Cashier (`Cashier`) | Admin (`Admin`) |
| :--- | :--- | :--- | :--- |
| **Login & Register (`/Auth/Login`, `/Auth/Register`)** | Allowed | Redirected to Home | Redirected to Home |
| **New POS Transaction (`/Transaction/New`)** | Redirected to Login | Allowed | Denied (`403 AccessDenied`) |
| **Product & Payment Lookup APIs** | `401 Unauthorized` | Allowed | Allowed |
| **Checkout Submission (`POST /api/v1/transactions`)** | `401 Unauthorized` | Allowed | Allowed (API level) |
| **Transaction History (`/Transaction/History`)** | Redirected to Login | Scoped to own sales only | Global view + Cashier filter |

---

## 5. Core Workflows & API Contracts

### 5.1 User Registration & Login
- **`POST /api/v1/auth/register`**:
  - Validates required fields (`Username`, `FullName`, `Password`, `RePassword`, `Role`), checks that `Password` matches `RePassword`, and verifies `Role` is `Admin` or `Cashier`.
  - Enforces case-insensitive username uniqueness (returning `"username has already exists"` on conflict).
  - Generates the next sequential 5-digit `IdUser` (`D5`), hashes the password with BCrypt, and inserts the active `MsUser` record.
- **`POST /api/v1/auth/login`**:
  - Verifies username, active status (`Stsrc == true`), BCrypt hash, and selected role (`Admin` or `Cashier`).
  - Returns `401 Unauthorized` with `"Invalid login"` if any check fails, or `200 OK` with user details and a signed JWT token on success.

### 5.2 Product Search & Payment Methods
- **`GET /api/v1/payment-methods`**:
  - Returns active payment methods (`Cash`, `Debit`, `Credit`) ordered by `IdPaymentMethod` ascending.
- **`GET /api/v1/products/search?search=&page=1&pageSize=10`**:
  - Queries active products with available inventory (`Stsrc == true && Stock > 0`), filters case-insensitively by product name, sorts alphabetically, and returns paginated results (default 10 items per page, max 50).

### 5.3 POS Checkout & Invoice Sequencing
- **Sequential Daily Invoices (`GET /api/v1/transactions/next-invoice`)**:
  - Generates invoice numbers using the format `INV-YYYYMMDD-XXXX` (e.g., `INV-20261023-0001`). The 4-digit sequence increments per transaction on the current date and resets to `0001` each new day.
- **Interactive Cart (`/Transaction/New`)**:
  - Cashiers search products, inspect remaining stock in the selection panel, and add quantities to the cart.
  - Client-side validation prevents adding quantities above available stock (`MaxStock`) and updates total items, total price, and customer change (`CustomerMoney - TotalPrice`) in real time.
- **Atomic Checkout (`POST /api/v1/transactions`)**:
  1. Opens an explicit EF Core database transaction (`BeginTransactionAsync`).
  2. Validates non-empty cart items, active payment method, sufficient stock per product (`"Insufficient stock for product: {Name}"`), and sufficient customer payment (`"Customer money is insufficient"`).
  3. Deducts `MsProduct.Stock` for each item, generates the final `InvoiceNo`, and saves `TrTransaction` and `TrTransactionDetail` rows atomically.
  4. Returns a complete receipt payload displayed in the frontend receipt modal before resetting the register for the next customer.

### 5.4 Role-Scoped Transaction History
- **`GET /api/v1/transactions/history`**:
  - Supports pagination (`page`, `pageSize`) and inclusive date-range filtering (`startDate` from `00:00:00` through `endDate` at `23:59:59.9999999`), ordered newest-first.
- **Data Isolation**:
  - When called by a **Cashier**, the API enforces `t.IdUser == loggedInUserId` from the JWT claims and ignores any `cashierId` query parameter.
  - When called by an **Admin**, the API returns transactions across all cashiers and enables optional filtering by `cashierId`.
- **Expandable Line Items**:
  - Uses eager loading (`.Include()` / `.ThenInclude()`) so each transaction row in `/Transaction/History` can expand inline to show purchased product names, unit prices, quantities, and subtotals without N+1 queries.
