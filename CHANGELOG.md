# Changelog

All notable changes to the **BeeMart POS Web App** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [Unreleased]

- **Phase 3**: Seed Data (DML) — payment methods, users with hashed passwords, products, and sample transactions combined into `Database/BeeMart.sql`.
- **Phase 4**: EF Core Entities & `BeeMartDbContext`.
- **Phase 5**: Backend Foundations (JSON response wrapper, `/api/v1` versioning, password hasher, JWT authentication, claims helper).
- **Phase 6**: Backend Auth Endpoints (`POST /api/v1/auth/register`, `POST /api/v1/auth/login`).
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
