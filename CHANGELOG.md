# Changelog

All notable changes to the **BeeMart POS Web App** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [Unreleased]

- **Phase 7**: Backend Master Data Endpoints (`GET /api/v1/payment-methods`, `GET /api/v1/products/search`).
- **Phase 8**: Backend Transaction Endpoints (`GET /api/v1/transactions/next-invoice`, `POST /api/v1/transactions`).
- **Phase 9**: Backend History Endpoint (`GET /api/v1/transactions/history`).
- **Phase 10**: WebClient Foundations (custom CSS, HTTP API client, token/session state, auth guards, IDR & date formatters).
- **Phase 11**: WebClient Auth Pages (`Login` & `Register`).
- **Phase 12**: WebClient Shared Layout (Header bar, real-time clock, session indicator, navigation tabs, logout).
- **Phase 13**: WebClient New Transaction Page.
- **Phase 14**: WebClient History Page (`Cashier` & `Admin` modes).
- **Phase 15**: Final Checks & Delivery verification.

---

## [Phase 6] - 2026-10-20 — Backend Authentication Endpoints

### Added
- Created Authentication DTOs under `BeeMart.Api/Models/DTOs/Auth/` matching `PRD.md` Section 6.2:
  - `LoginRequest.cs`: `Username`, `Password`, `Role`.
  - `LoginResponse.cs`: `IdUser`, `Username`, `FullName`, `Role`, `Token`.
  - `RegisterRequest.cs`: `Username`, `FullName`, `Password`, `RePassword`, `Role`.
  - `RegisterResponse.cs`: `IdUser`, `Username`, `FullName`, `Role`.
- Created `AuthController` (`BeeMart.Api/Controllers/AuthController.cs`) decorated with `[ApiController]`, `[Route("api/v1/auth")]`, and `[AllowAnonymous]`, injecting `BeeMartDbContext`, `IPasswordHasher`, and `IJwtTokenService`:
  - **`POST /api/v1/auth/register` (`API-AUTH-02`)**:
    - Validates all required fields (`Username`, `FullName`, `Password`, `RePassword`, `Role`).
    - Validates `Password == RePassword` (returns HTTP 400 with `"Password and confirmation password do not match"` on mismatch).
    - Validates `Role` is strictly `"Admin"` or `"Cashier"` (returns HTTP 400 with `"Role must be either Admin or Cashier"` on invalid role).
    - Performs case-insensitive username conflict check on `MsUser` (returns HTTP 400 with exact message `"username has already exists"` on duplicate).
    - Generates a unique 5-digit numeric `IdUser` string formatted with `D5` (finds max numeric `IdUser` in `MsUser`, increments by 1, e.g. `"78887"` after seeded max `"78886"`, with collision fallback).
    - Hashes password via `IPasswordHasher.HashPassword`, sets audit fields (`Stsrc = true`, `DateIn = DateTime.Now`, `UserIn = request.Username`, `Dateup = null`, `Userup = null`), persists to `dbo.MsUser`, and returns **HTTP 201 Created** with `ApiResponse<RegisterResponse>.Success(data, "Account registered successfully")`.
  - **`POST /api/v1/auth/login` (`API-AUTH-01`)**:
    - Validates required fields (`Username`, `Password`, `Role`).
    - Queries active user (`Stsrc == true`), checks role match and `IPasswordHasher.VerifyPassword(request.Password, user.PasswordHash)`, returning **HTTP 401 Unauthorized** with exact message `"Invalid login"` on any mismatch.
    - Generates JWT bearer token via `IJwtTokenService.GenerateToken(user)` and returns **HTTP 200 OK** with `ApiResponse<LoginResponse>.Success(data)`.

### Configured
- Updated `BeeMart.Api/Program.cs` to configure `SuppressModelStateInvalidFilter = true` on `AddControllers()` so missing/empty fields are uniformly handled by controller validation and returned using the standard `ApiResponse` JSON wrapper.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Executed live HTTP smoke tests against `https://localhost:7001/api/v1/auth/register` and `https://localhost:7001/api/v1/auth/login` verifying HTTP 201 registration (`idUser = "78887"`), HTTP 400 duplicate username (`"username has already exists"`), HTTP 400 password mismatch and invalid role, HTTP 200 login with valid JWT token, and HTTP 401 wrong password/role (`"Invalid login"`), followed by test user cleanup.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 5] - 2026-10-20 — Backend Foundations

### Added
- Added NuGet packages to `BeeMart.Api/BeeMart.Api.csproj` (pinned to .NET 8 compatible stable versions):
  - `BCrypt.Net-Next` (`4.0.3`)
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (`8.0.13`)
- Created standard JSON response wrapper `BeeMart.Api/Common/ApiResponse.cs`:
  - Implemented `ApiResponse<T>` and non-generic `ApiResponse` with `"status"` (`"success"` or `"error"`), `"message"` (`[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`), and `"data"` (`[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`) matching `PRD.md` Section 6.2.
  - Added static factory methods `ApiResponse<T>.Success(T data, string? message = null)`, `ApiResponse<T>.Error(string message)`, `ApiResponse.Success(string? message = null)`, `ApiResponse.Success<T>(T data, string? message = null)`, and `ApiResponse.Error(string message)`.
- Created BCrypt password hasher `BeeMart.Api/Common/PasswordHasher.cs`:
  - Implemented `IPasswordHasher` and `PasswordHasher` wrapping `BCrypt.Net.BCrypt` with work factor `11` (`HashPassword(string password)` and safe `VerifyPassword(string password, string hash)` handling null/whitespace and malformed salts without throwing unhandled exceptions).
- Created JWT token generator `BeeMart.Api/Common/JwtTokenService.cs`:
  - Implemented `IJwtTokenService` and `JwtTokenService` signing tokens with `SecurityAlgorithms.HmacSha256`.
  - Embedded both standard ASP.NET Core claims and custom BeeMart claims: `ClaimTypes.NameIdentifier` + `"idUser"`, `ClaimTypes.Name` + `"username"`, `"fullName"`, and `ClaimTypes.Role` + `"role"`.
  - Configured token lifetime from `JwtSettings:ExpiryMinutes` (default `1440` minutes = 24 hours).
- Created user claims helper `BeeMart.Api/Common/ClaimsHelper.cs`:
  - Implemented `ClaimsPrincipal` extension methods: `GetUserId()`, `GetUsername()`, `GetFullName()`, `GetRole()`, `IsAdmin()`, and `IsCashier()`.

### Configured
- Added `JwtSettings` section (`SecretKey`, `Issuer = "BeeMartApi"`, `Audience = "BeeMartApp"`, `ExpiryMinutes = 1440`) to `BeeMart.Api/appsettings.json`.
- Updated `BeeMart.Api/Program.cs` to register `IPasswordHasher` and `IJwtTokenService` in DI, configure JWT Bearer authentication (`JwtBearerDefaults.AuthenticationScheme` with Issuer, Audience, Lifetime, and IssuerSigningKey validation), and register `app.UseAuthentication()` before `app.UseAuthorization()`.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Executed live Phase 5 foundations verification tests confirming `ApiResponse` JSON output format, `PasswordHasher` hashing & verification (including against seeded `steven` hash from `Database/BeeMart.sql`), and `JwtTokenService` + `ClaimsHelper` token generation, validation, and claim extraction.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 4] - 2026-10-20 — EF Core Entities & DbContext

### Added
- Created 5 Entity Framework Core POCO entity classes under `BeeMart.Api/Models/Entities/` matching `Database/BeeMart.sql` and `PRD.md` Section 5.1:
  1. **`MsUser` (`BeeMart.Api/Models/Entities/MsUser.cs`)**: `IdUser` (`string`, PK), `Username`, `FullName`, `PasswordHash`, `Role`, audit columns (`Stsrc`, `DateIn`, `UserIn`, `Dateup`, `Userup`), and navigation `ICollection<TrTransaction> Transactions`.
  2. **`LtPaymentMethod` (`BeeMart.Api/Models/Entities/LtPaymentMethod.cs`)**: `IdPaymentMethod` (`int`, PK, Identity), `MethodName`, audit columns, and navigation `ICollection<TrTransaction> Transactions`.
  3. **`MsProduct` (`BeeMart.Api/Models/Entities/MsProduct.cs`)**: `IdProduct` (`string`, PK), `Name`, `Weight`, `Stock`, `Price`, `Discount`, audit columns, and navigation `ICollection<TrTransactionDetail> TransactionDetails`.
  4. **`TrTransaction` (`BeeMart.Api/Models/Entities/TrTransaction.cs`)**: `IdTransaction` (`string`, PK), `InvoiceNo`, `TransactionDate`, `IdUser` (FK), `IdPaymentMethod` (FK), `[NotMapped] TotalItem` (ignored in SQL mapping as `dbo.TrTransaction` stores item counts on `TrTransactionDetail.Qty`), `TotalPrice`, audit columns, and navigation properties (`MsUser User`, `LtPaymentMethod PaymentMethod`, `ICollection<TrTransactionDetail> TransactionDetails`).
  5. **`TrTransactionDetail` (`BeeMart.Api/Models/Entities/TrTransactionDetail.cs`)**: `IdTransactionDetail` (`string`, PK), `IdTransaction` (FK), `IdProduct` (FK), `Qty`, `Price`, `Discount`, `SubTotal`, audit columns, and navigation properties (`TrTransaction Transaction`, `MsProduct Product`).
- Created `BeeMartDbContext` (`BeeMart.Api/Data/BeeMartDbContext.cs`) inheriting from `DbContext` with `DbSet` properties (`Users`, `PaymentMethods`, `Products`, `Transactions`, `TransactionDetails`) and comprehensive Fluent API configurations in `OnModelCreating`:
  - Explicit table names (`MsUser`, `LtPaymentMethod`, `MsProduct`, `TrTransaction`, `TrTransactionDetail`) and primary key constraint names (`PK_*`).
  - Explicit decimal precisions: `HasPrecision(10, 2)` (`MsProduct.Weight`), `HasPrecision(18, 2)` (`MsProduct.Price`, `TrTransaction.TotalPrice`, `TrTransactionDetail.Price`, `TrTransactionDetail.SubTotal`), and `HasPrecision(5, 2)` (`MsProduct.Discount`, `TrTransactionDetail.Discount`).
  - Explicit string key & column max lengths matching SQL DDL (`HasMaxLength(5)` on `IdUser`, `HasMaxLength(36)` on GUID keys and `UserIn`/`Userup`, `HasMaxLength(50)` on `Username` and `MethodName`, `HasMaxLength(300)` on `FullName`, `HasMaxLength(255)` on `PasswordHash`, `HasMaxLength(20)` on `Role` and `InvoiceNo`, `HasMaxLength(100)` on `MsProduct.Name`).
  - Configured all 5 indexes & unique constraints: `UQ_MsUser_Username` (`IsUnique()`), `UQ_LtPaymentMethod_MethodName` (`IsUnique()`), `UQ_TrTransaction_InvoiceNo` (`IsUnique()`), `IX_TrTransaction_TransactionDate_IdUser`, and `IX_MsProduct_Name`.
  - Configured all 4 foreign key relationships (`FK_TrTransaction_MsUser`, `FK_TrTransaction_LtPaymentMethod`, `FK_TrTransactionDetail_TrTransaction`, `FK_TrTransactionDetail_MsProduct`) with `DeleteBehavior.Restrict` to enforce the `Stsrc` soft-delete rule.

### Configured
- Updated `ConnectionStrings:DefaultConnection` in `BeeMart.Api/appsettings.json` to `"Server=localhost\\SQLEXPRESS;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;"`.
- Registered `BeeMartDbContext` in `BeeMart.Api/Program.cs` using `options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))`.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Executed live EF Core database connectivity and entity mapping verification against `localhost\SQLEXPRESS` (`Database=BeeMart`): confirmed `CanConnect = True`, `PaymentMethods.Count() = 3`, `Users.Count() = 6`, `Products.Count() = 22`, and verified `Include`/`ThenInclude` queries across all foreign keys and navigation properties with 0 model validation or SQL mapping errors.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Repository housekeeping (non-phase)] - 2026-10-20 — Git Ignore & Project README

### Added
- Created root `.gitignore` tailored for Visual Studio / .NET 8 (`bin/`, `obj/`, `.vs/`, `.idea/`, `.vscode/*`, `*.user`, `*.suo`, `*.userprefs`, `*.log`, `TestResults/`, `*.mdf`, `*.ldf`, `scratch/`), while explicitly preserving `launchSettings.json` and `appsettings.Development.json`. Did not modify `.gitattributes`.
- Created root `README.md` documenting project overview, tech stack, repository layout, prerequisites, database & application setup steps (`sqlcmd` and SSMS), fixed ports (`https://localhost:7001` and `https://localhost:7002`), HTTPS certificate trust command, seeded development accounts table, and project status links.
- Inspected tracked files via read-only `git ls-files` and identified 188 build artifact files currently tracked under `BeeMart.Api/bin/`, `BeeMart.Api/obj/`, `BeeMart.Web/bin/`, and `BeeMart.Web/obj/` (recorded in `OPEN-ITEMS.md` with manual `git rm -r --cached` commands for the repository owner).

---

## [Phase 3] - 2026-10-20 — Seed Data (DML)

### Added
- Extended `Database/BeeMart.sql` below `-- PHASE 3: SEED DATA GOES BELOW THIS LINE` (leaving Phase 2 DDL untouched) with idempotent, re-runnable T-SQL seed statements (`IF NOT EXISTS` / `WHERE NOT EXISTS` on natural keys, explicit `UserIn = N'SYSTEM'`, preserving `USE [BeeMart]` and `GO` batch separators):
  1. **`dbo.LtPaymentMethod` (3 rows)**:
     - Seeded `Cash`, `Debit`, and `Credit` in exact order without `SET IDENTITY_INSERT`, producing `IdPaymentMethod` values `1`, `2`, and `3`, guarded on `MethodName`.
  2. **`dbo.MsUser` (6 rows)**:
     - Seeded `admin` (`00001`, role `Admin`), `steven` (`76985`, role `Cashier`), `gabri` (`66859`, role `Cashier`), `mona` (`55794`, role `Cashier`), `felix` (`78886`, role `Cashier`), and `gobuta` (`55356`, role `Cashier`), guarded on `Username`.
     - **Password Hashing Approach**: Generated 6 unique **BCrypt** hashes (`$2a$11$...`, work factor 11, 60 chars each) using `BCrypt.Net-Next` (v4.0.3) in a temporary scratch console project outside the repository, verified all 6 hashes with `BCrypt.Verify("Password123!", hash) == true`, pasted only the hash literals into `Database/BeeMart.sql` (no plain-text password in the SQL file), and deleted the temporary project.
  3. **`dbo.MsProduct` (22 rows)**:
     - Seeded 22 products with fixed, hard-coded GUID strings (`a1b2c3d4-0001-4000-8000-000000000001` through `...0022`), guarded on `IdProduct`:
       - **4 core PRD products (`0001`–`0004`)**: `Apel Fuji`, `Apel Merah`, `Apel Hijau`, `Susu UHT Full Cream`.
       - **9 additional `Apel` products (`0005`–`0013`, 12 `Apel` total)** to test the max-10 search result limit: `Apel Malang`, `Apel Royal Gala`, `Apel Granny Smith`, `Apel Envy`, `Apel Pacific Rose`, `Apel Honeycrisp`, `Apel Pink Lady`, `Apel Red Delicious`, `Apel Manalagi`.
       - **3 additional `Susu` products (`0014`–`0016`, 4 `Susu` total)**: `Susu UHT Cokelat`, `Susu UHT Stroberi`, `Susu Kental Manis Putih`.
       - **6 additional general grocery products (`0017`–`0022`)**: `Beras Pandan Wangi 5 Kg`, `Minyak Goreng Sawit 2 L`, `Gula Pasir Putih 1 Kg`, `Mie Instan Goreng`, `Telur Ayam Negeri 1 Kg`, `Kopi Bubuk Arabika 200 g`.
  4. **No transactions seeded**: `TrTransaction` and `TrTransactionDetail` remain empty (`0` rows) to be created through the application.

### Verified
- Executed `sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i "Database\BeeMart.sql"` twice consecutively; both runs exited with code `0`.
- Verified via SQL queries after the second run: `LtPaymentMethod` = 3 rows (`1=Cash`, `2=Debit`, `3=Credit`), `MsUser` = 6 rows with matching `IdUser` and `Role` values and 0 invalid/plain-text hashes (all 6 start with `$2a$11$` and have length 60), `MsProduct` = 22 rows (`12` matching `LIKE '%Apel%'`, `4` matching `LIKE '%Susu%'`), and `0` duplicate rows.

### Changed
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md` to reflect Phase 3 completion and repository housekeeping tasks.

---

## [Phase 2] - 2026-10-20 — Database Schema (DDL)

### Added
- Created `Database/BeeMart.sql` containing re-runnable (idempotent) T-SQL DDL for SQL Server (`BeeMart` database creation with `IF DB_ID(N'BeeMart') IS NULL` and `USE [BeeMart]`, plus `IF OBJECT_ID(..., N'U') IS NULL` table guards). Kept `Database/.gitkeep` in `Database/`.
- Added 5 tables in dependency order matching `PRD.md` Section 5.1 & Section 5.3 ERD:
  1. `dbo.LtPaymentMethod` (`IdPaymentMethod INT IDENTITY(1,1)`, `MethodName NVARCHAR(50)`, `Stsrc BIT`, `DateIn DATETIME2`, `UserIn NVARCHAR(36)`, `Dateup DATETIME2`, `Userup NVARCHAR(36)`).
  2. `dbo.MsUser` (`IdUser NVARCHAR(5)`, `Username NVARCHAR(50)`, `FullName NVARCHAR(300)`, `PasswordHash NVARCHAR(255)`, `Role NVARCHAR(20)`, `Stsrc BIT`, `DateIn DATETIME2`, `UserIn NVARCHAR(36)`, `Dateup DATETIME2`, `Userup NVARCHAR(36)`).
  3. `dbo.MsProduct` (`IdProduct NVARCHAR(36)`, `Name NVARCHAR(100)`, `Weight DECIMAL(10,2)`, `Stock INT`, `Price DECIMAL(18,2)`, `Discount DECIMAL(5,2)`, `Stsrc BIT`, `DateIn DATETIME2`, `UserIn NVARCHAR(36)`, `Dateup DATETIME2`, `Userup NVARCHAR(36)`).
  4. `dbo.TrTransaction` (`IdTransaction NVARCHAR(36)`, `InvoiceNo NVARCHAR(20)`, `TransactionDate DATETIME2`, `IdUser NVARCHAR(5)`, `IdPaymentMethod INT`, `TotalPrice DECIMAL(18,2)`, `Stsrc BIT`, `DateIn DATETIME2`, `UserIn NVARCHAR(36)`, `Dateup DATETIME2`, `Userup NVARCHAR(36)`).
  5. `dbo.TrTransactionDetail` (`IdTransactionDetail NVARCHAR(36)`, `IdTransaction NVARCHAR(36)`, `IdProduct NVARCHAR(36)`, `Qty INT`, `Price DECIMAL(18,2)`, `Discount DECIMAL(5,2)`, `SubTotal DECIMAL(18,2)`, `Stsrc BIT`, `DateIn DATETIME2`, `UserIn NVARCHAR(36)`, `Dateup DATETIME2`, `Userup NVARCHAR(36)`).
- Added Primary Key constraints:
  - `PK_LtPaymentMethod` on `LtPaymentMethod(IdPaymentMethod)`
  - `PK_MsUser` on `MsUser(IdUser)`
  - `PK_MsProduct` on `MsProduct(IdProduct)`
  - `PK_TrTransaction` on `TrTransaction(IdTransaction)`
  - `PK_TrTransactionDetail` on `TrTransactionDetail(IdTransactionDetail)`
- Added Foreign Key constraints:
  - `FK_TrTransaction_MsUser` (`TrTransaction.IdUser` $\rightarrow$ `MsUser.IdUser`)
  - `FK_TrTransaction_LtPaymentMethod` (`TrTransaction.IdPaymentMethod` $\rightarrow$ `LtPaymentMethod.IdPaymentMethod`)
  - `FK_TrTransactionDetail_TrTransaction` (`TrTransactionDetail.IdTransaction` $\rightarrow$ `TrTransaction.IdTransaction`)
  - `FK_TrTransactionDetail_MsProduct` (`TrTransactionDetail.IdProduct` $\rightarrow$ `MsProduct.IdProduct`)
- Added Unique & Check constraints:
  - `UQ_LtPaymentMethod_MethodName`: `UNIQUE` on `LtPaymentMethod(MethodName)`
  - `CK_LtPaymentMethod_MethodName`: `CHECK ([MethodName] IN (N'Cash', N'Debit', N'Credit'))`
  - `CK_MsUser_Role`: `CHECK ([Role] IN (N'Admin', N'Cashier'))`
- Added Default constraints (26 total across the 5 tables):
  - `LtPaymentMethod`: `DF_LtPaymentMethod_Stsrc` (`1`), `DF_LtPaymentMethod_DateIn` (`SYSDATETIME()`), `DF_LtPaymentMethod_UserIn` (`N'SYSTEM'`)
  - `MsUser`: `DF_MsUser_Stsrc` (`1`), `DF_MsUser_DateIn` (`SYSDATETIME()`), `DF_MsUser_UserIn` (`N'SYSTEM'`)
  - `MsProduct`: `DF_MsProduct_IdProduct` (`CONVERT(NVARCHAR(36), NEWID())`), `DF_MsProduct_Weight` (`0`), `DF_MsProduct_Stock` (`0`), `DF_MsProduct_Price` (`0`), `DF_MsProduct_Discount` (`0`), `DF_MsProduct_Stsrc` (`1`), `DF_MsProduct_DateIn` (`SYSDATETIME()`), `DF_MsProduct_UserIn` (`N'SYSTEM'`)
  - `TrTransaction`: `DF_TrTransaction_IdTransaction` (`CONVERT(NVARCHAR(36), NEWID())`), `DF_TrTransaction_TransactionDate` (`SYSDATETIME()`), `DF_TrTransaction_TotalPrice` (`0`), `DF_TrTransaction_Stsrc` (`1`), `DF_TrTransaction_DateIn` (`SYSDATETIME()`)
  - `TrTransactionDetail`: `DF_TrTransactionDetail_IdTransactionDetail` (`CONVERT(NVARCHAR(36), NEWID())`), `DF_TrTransactionDetail_Qty` (`1`), `DF_TrTransactionDetail_Price` (`0`), `DF_TrTransactionDetail_Discount` (`0`), `DF_TrTransactionDetail_SubTotal` (`0`), `DF_TrTransactionDetail_Stsrc` (`1`), `DF_TrTransactionDetail_DateIn` (`SYSDATETIME()`)
- Added `PRD.md` Section 5.4 indexes with `IF NOT EXISTS` guards on `sys.indexes`:
  - `UQ_MsUser_Username`: Unique nonclustered index on `MsUser(Username ASC)`
  - `UQ_TrTransaction_InvoiceNo`: Unique nonclustered index on `TrTransaction(InvoiceNo ASC)`
  - `IX_TrTransaction_TransactionDate_IdUser`: Nonclustered index on `TrTransaction(TransactionDate ASC, IdUser ASC)`
  - `IX_MsProduct_Name`: Nonclustered index on `MsProduct(Name ASC)`
- Added Phase 3 marker comment (`-- PHASE 3: SEED DATA GOES BELOW THIS LINE`) at the end of `Database/BeeMart.sql`.

### Changed
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md` to reflect Phase 2 completion and verification results.

---

## [Phase 1] - 2026-10-20 — Solution & Project Scaffolding

### Added
- Created root solution file `BeeMart.sln` and registered both `BeeMart.Api` and `BeeMart.Web` projects.
- Created `Database/` folder at the solution root with `Database/.gitkeep` to hold upcoming SQL scripts (`DELIV-004`).
- Created Backend API project `BeeMart.Api` (`BeeMart.Api/BeeMart.Api.csproj`) targeting `net8.0` (Controller-based ASP.NET Core Web API).
- Installed Entity Framework Core 8 NuGet packages in `BeeMart.Api/BeeMart.Api.csproj` only:
  - `Microsoft.EntityFrameworkCore.SqlServer` (`8.0.32`)
  - `Microsoft.EntityFrameworkCore.Design` (`8.0.32`)
  - `Microsoft.EntityFrameworkCore.Tools` (`8.0.32`)
- Created `BeeMart.Api/Controllers/.gitkeep` to preserve the `Controllers/` directory after removing template boilerplate.
- Created WebClient project `BeeMart.Web` (`BeeMart.Web/BeeMart.Web.csproj`) targeting `net8.0` (ASP.NET Core Razor Pages) with zero database or EF Core dependencies (`NFR-ARCH-002`).
- Created project documentation files at the root: `STRUCTURE.md`, `CHANGELOG.md`, and `OPEN-ITEMS.md`.

### Configured
- `BeeMart.Api/appsettings.json`: Added `ConnectionStrings:DefaultConnection` with value `"Server=localhost;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;"`.
- `BeeMart.Web/appsettings.json`: Added `ApiSettings:BaseUrl` with value `"https://localhost:7001/api/v1/"` (no database connection string in WebClient).
- `BeeMart.Api/Properties/launchSettings.json`: Configured fixed ports `https://localhost:7001` and `http://localhost:5001` (`launchBrowser: false`).
- `BeeMart.Web/Properties/launchSettings.json`: Configured fixed ports `https://localhost:7002` and `http://localhost:5002` (`launchBrowser: true`).
- `BeeMart.Web/Program.cs`: Configured `.NET 8` Razor Pages middleware pipeline (`UseHttpsRedirection`, `UseStaticFiles`, `UseRouting`, `UseAuthorization`, `MapRazorPages`).
- `BeeMart.Web/wwwroot/css/site.css`: Emptied default template styles to leave a clean, pure custom CSS file (`FE-STYLE-001`).

### Removed
- Removed template boilerplate from `BeeMart.Api`:
  - `BeeMart.Api/WeatherForecast.cs`
  - `BeeMart.Api/Controllers/WeatherForecastController.cs`
  - `BeeMart.Api/BeeMart.Api.http`
- Removed all Bootstrap files, third-party client libraries, and unneeded template files from `BeeMart.Web` (`NFR-TECH-002`, `FE-STYLE-001`):
  - `BeeMart.Web/wwwroot/lib/` (including `bootstrap`, `jquery`, `jquery-validation`, `jquery-validation-unobtrusive`)
  - `BeeMart.Web/Pages/Shared/_Layout.cshtml.css`
  - `BeeMart.Web/Pages/Shared/_ValidationScriptsPartial.cshtml`
  - `BeeMart.Web/Pages/Privacy.cshtml` and `BeeMart.Web/Pages/Privacy.cshtml.cs`
  - Stripped all Bootstrap CSS/JS links and class names from `BeeMart.Web/Pages/Shared/_Layout.cshtml`, `BeeMart.Web/Pages/Index.cshtml`, and `BeeMart.Web/Pages/Error.cshtml`.
