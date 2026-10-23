# BeeMart POS

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API_%26_Razor_Pages-512BD4)
![SQL Server](https://img.shields.io/badge/Database-MS_SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Version](https://img.shields.io/badge/Version-1.0.0-10B981)

**BeeMart POS** is a full-stack Point of Sale web application built with **.NET 8**, **Entity Framework Core 8**, **ASP.NET Core Razor Pages**, and **Microsoft SQL Server**. We designed this project around a strict two-tier architecture that separates backend business logic and data persistence from the frontend web interface.

---

## Architecture Overview

The solution (`BeeMart.sln`) is split into two independent projects:

1. **`BeeMart.Api` (Backend REST API — Port `5001` / `7001`)**
   Centralizes all business rules, BCrypt password hashing, JWT token generation, and SQL Server access via Entity Framework Core (`BeeMartDbContext`).
2. **`BeeMart.Web` (Frontend Web Client — Port `5002` / `7002`)**
   An ASP.NET Core Razor Pages application styled with pure custom CSS (zero Bootstrap/Tailwind). It has no database packages or connection strings and communicates with `BeeMart.Api` exclusively over HTTP/JSON via a typed `IApiClient` service.

```text
[ Browser ] ──► [ BeeMart.Web (Port 5002) ] ──HTTP/JSON (JWT)──► [ BeeMart.Api (Port 5001) ] ──EF Core──► [ SQL Server: BeeMart ]
```

---

## Prerequisites

- **.NET 8 SDK** (`net8.0`)
- **Microsoft SQL Server** or **SQL Server Express** (`localhost` or `localhost\SQLEXPRESS`)
- **`sqlcmd`** CLI utility or **SQL Server Management Studio (SSMS)**

---

## Quick Start / How to Run

### 1. Initialize & Seed the Database

Run the idempotent script [`Database/BeeMart.sql`](Database/BeeMart.sql) to create the `BeeMart` database, tables, indexes, and seed data (3 payment methods, 6 staff users, and 22 grocery products).

**Option A — Using `sqlcmd`:**
```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i "Database\BeeMart.sql"
```
*(If using a default SQL Server instance, replace `"localhost\SQLEXPRESS"` with `"localhost"`.)*

**Option B — Using SSMS:**
Open `Database/BeeMart.sql` in SQL Server Management Studio and press `F5` to execute.

### 2. Verify the Connection String

Check [`BeeMart.Api/appsettings.json`](BeeMart.Api/appsettings.json) under `ConnectionStrings:DefaultConnection` and make sure the `Server` value matches your local SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Run the Backend API (Port 5001)

Open a terminal at the repository root and start `BeeMart.Api`:

```powershell
dotnet run --project BeeMart.Api --launch-profile http
```
The API will listen on `http://localhost:5001` (or use `--launch-profile https` for `https://localhost:7001` and `http://localhost:5001`).

### 4. Run the Web Frontend (Port 5002)

Open a second terminal at the repository root and start `BeeMart.Web`:

```powershell
dotnet run --project BeeMart.Web --launch-profile http
```
Open your browser and navigate to **`http://localhost:5002`** (or `https://localhost:7002` with `--launch-profile https`).

---

## Default Seed Credentials

All seeded users in `Database/BeeMart.sql` are stored as BCrypt hashes (work factor 11) and share the same local development password:

- **Default Password:** `Password123!`

| User ID | Username | Full Name | Role | Access Scope |
| :--- | :--- | :--- | :--- | :--- |
| `00001` | `admin` | Admin | `Admin` | Global transaction history across all cashiers |
| `76985` | `steven` | Steven | `Cashier` | POS cash register + personal transaction history |
| `66859` | `gabri` | Gabri | `Cashier` | POS cash register + personal transaction history |
| `55794` | `mona` | Mona | `Cashier` | POS cash register + personal transaction history |
| `78886` | `felix` | Felix | `Cashier` | POS cash register + personal transaction history |
| `55356` | `gobuta` | Gobuta | `Cashier` | POS cash register + personal transaction history |

---

## Project Documentation

- [`SPECIFICATION.md`](SPECIFICATION.md) — Functional and technical specifications, database schema, role matrix, and workflows.
- [`STRUCTURE.md`](STRUCTURE.md) — Solution directory tree and component overview.
- [`CHANGELOG.md`](CHANGELOG.md) — Release history for `v1.0.0`.
- [`OPEN-ITEMS.md`](OPEN-ITEMS.md) — Project completion status and future enhancement ideas.
