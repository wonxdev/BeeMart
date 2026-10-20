# BeeMart POS Web App

**BeeMart** is a Point of Sale (POS) web application built for the **Binus IT Bootcamp .NET C# Test (2026/2027)**. The solution is built on **.NET 8** using a strict **Separation of Concerns** architecture consisting of two independent projects: a **Backend RESTful API** (`BeeMart.Api`) that centralizes all business logic and SQL Server data persistence via Entity Framework Core, and a **WebClient UI** (`BeeMart.Web`) that acts strictly as an HTTP/JSON consumer with zero direct database access.

---

## Tech Stack

- **Runtime & SDK**: .NET 8 (`net8.0`, C#)
- **Backend (`BeeMart.Api`)**: ASP.NET Core Web API (Controller-based, `/api/v1/...`), Entity Framework Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Frontend (`BeeMart.Web`)**: ASP.NET Core Razor Pages, pure custom CSS (no Bootstrap), vanilla JavaScript, `HttpClient` JSON consumer
- **Database**: Microsoft SQL Server (`Database/BeeMart.sql` — idempotent T-SQL DDL + DML seed script)
- **Password Hashing**: BCrypt (`BCrypt.Net-Next`, work factor 11)

---

## Repository Layout

```text
BeeMart/
├── BeeMart.sln              # Visual Studio / .NET Solution file
├── Database/
│   └── BeeMart.sql          # Idempotent SQL Server DDL + Seed Data (DML) script
├── BeeMart.Api/             # Backend RESTful API (https://localhost:7001)
├── BeeMart.Web/             # WebClient Razor Pages UI (https://localhost:7002)
├── PRD.md                   # Product Requirements Document
├── STRUCTURE.md             # Detailed directory & file architecture documentation
├── CHANGELOG.md             # Phase-by-phase change history
├── OPEN-ITEMS.md            # 15-phase roadmap, task checklist, and technical notes
├── README.md                # Setup guide and project overview
├── .gitignore               # Standard .NET / Visual Studio Git ignore rules
└── .gitattributes           # Line-ending normalization rules
```

See [`STRUCTURE.md`](STRUCTURE.md) for the complete file inventory and component relationship diagram.

---

## Prerequisites

1. **.NET 8 SDK** (or newer SDK with the `.NET 8.0` runtime installed).
2. **Microsoft SQL Server** or **SQL Server Express** (e.g., `localhost` or `localhost\SQLEXPRESS`).
3. **`sqlcmd`** CLI utility or **SQL Server Management Studio (SSMS)** to execute the database script.

---

## Setup Steps

### 1. Initialize & Seed the Database
Run the idempotent script [`Database/BeeMart.sql`](Database/BeeMart.sql) to create the `BeeMart` database, tables, constraints, indexes, and initial seed data (`LtPaymentMethod`, `MsUser`, and `MsProduct`).

- **Using `sqlcmd` (SQL Server Express example):**
  ```powershell
  sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i "Database\BeeMart.sql"
  ```
  *(If using a default SQL Server instance, replace `"localhost\SQLEXPRESS"` with `"localhost"`.)*

- **Using SQL Server Management Studio (SSMS):**
  Open `Database/BeeMart.sql` in SSMS, connect to your local SQL Server instance, and execute the script (`F5`). The script is re-runnable and safe to execute multiple times.

### 2. Check the Database Connection String
The Backend API connection string is configured in [`BeeMart.Api/appsettings.json`](BeeMart.Api/appsettings.json) under `ConnectionStrings:DefaultConnection`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
> **Note:** If your local SQL Server runs as a named instance such as `localhost\SQLEXPRESS`, update `Server=localhost` to `Server=localhost\\SQLEXPRESS` (scheduled in Phase 4 when `BeeMartDbContext` is wired up).

### 3. Trust the Local HTTPS Development Certificate
Ensure the local ASP.NET Core HTTPS development certificate is trusted:
```powershell
dotnet dev-certs https --trust
```

### 4. Run the Backend API and WebClient
Run both projects in separate terminals using their configured fixed ports:

- **Terminal 1 — Backend API (`https://localhost:7001`):**
  ```powershell
  dotnet run --project BeeMart.Api --launch-profile https
  ```

- **Terminal 2 — WebClient UI (`https://localhost:7002`):**
  ```powershell
  dotnet run --project BeeMart.Web --launch-profile https
  ```

---

## Seeded Accounts (Local Development Only)

All 6 users seeded by `Database/BeeMart.sql` are stored with **BCrypt password hashes** (work factor 11). For local development and testing only, all seeded accounts share the following development password:

- **Shared Development Password (local dev only):** `Password123!`

| `IdUser` | Username | Full Name | Role | UI Role Label |
| :--- | :--- | :--- | :--- | :--- |
| `00001` | `admin` | Admin | `Admin` | Admin |
| `76985` | `steven` | Steven | `Cashier` | Kasir |
| `66859` | `gabri` | Gabri | `Cashier` | Kasir |
| `55794` | `mona` | Mona | `Cashier` | Kasir |
| `78886` | `felix` | Felix | `Cashier` | Kasir |
| `55356` | `gobuta` | Gobuta | `Cashier` | Kasir |

---

## Project Status & Documentation

This project is being built incrementally across **15 phases**:
- **Completed**:
  - **Phase 1**: Solution & Project Scaffolding (`BeeMart.sln`, `BeeMart.Api`, `BeeMart.Web`)
  - **Phase 2**: Database Schema DDL (`Database/BeeMart.sql`)
  - **Phase 3**: Database Seed Data DML (`LtPaymentMethod`, `MsUser`, `MsProduct` in `Database/BeeMart.sql`)
- **In Progress / Upcoming (Phases 4–15)**:
  - EF Core entities & `BeeMartDbContext`, JWT authentication & BCrypt verification, REST API endpoints (`/api/v1/...`), and WebClient Razor Pages UI (`Login`, `Register`, `New Transaction`, `History Transaction`) are not built yet and are tracked in [`OPEN-ITEMS.md`](OPEN-ITEMS.md).

### Project Documentation Files
- [`PRD.md`](PRD.md) — Product Requirements Document and specifications.
- [`STRUCTURE.md`](STRUCTURE.md) — Solution architecture, directory tree, and file inventory.
- [`CHANGELOG.md`](CHANGELOG.md) — Detailed history of changes across phases.
- [`OPEN-ITEMS.md`](OPEN-ITEMS.md) — Phase checklist, remaining tasks (Phases 4–15), and technical decisions.
