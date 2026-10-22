# Changelog

All notable changes to the **BeeMart POS Web App** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [Unreleased]

- **Phase 14–15**: UI Polish, End-to-End (E2E) Validation & Final Delivery verification.

---

## [Phase 13] - 2026-10-22 — WebClient Transaction History Page (Role-Aware)

### Added
- Created Transaction History ViewModels and DTOs in `BeeMart.Web/Models/Transaction/TransactionHistoryViewModel.cs`:
  - `TransactionHistoryViewModel`: Filter parameters (`StartDate`, `EndDate`, `CashierId`, `Page`, `PageSize`), paginated response properties (`Items`, `TotalItems`, `TotalPages`, `CurrentPage`), and Admin filter dropdown options (`CashierOptions`).
  - `TransactionHistoryResponse`, `TransactionHistoryItemViewModel`, `TransactionHistoryItemDetailViewModel`, and `CashierFilterOptionViewModel`.
- Created the Role-Aware Transaction History Razor Page (`BeeMart.Web/Pages/Transaction/History.cshtml` & `History.cshtml.cs`):
  - Decorated with `[Authorize]` so both `Admin` and `Cashier` users can access `/Transaction/History` (while unauthenticated users redirect to `/Auth/Login`).
  - Implemented `OnGetAsync` to resolve user identity/role from Cookie Authentication claims, enforce strict UI/parameter role isolation (`Cashier` ignores any `cashierId` query parameter and hides the Cashier filter selector; `Admin` populates active cashier options and forwards optional `cashierId`), and call `IApiClient.GetAsync<TransactionHistoryResponse>("transactions/history?...")` with automatic JWT Bearer token attachment.
  - Built a responsive filter bar (`Start Date`, `End Date`, Admin-only `Kasir` dropdown, `Baris / Hal` page size selector, `Filter / Cari`, and `Reset` buttons).
  - Built a custom CSS transaction history table (`Invoice No`, `Tanggal Transaksi`, `Kasir`, `Metode Pembayaran`, `Total Item`, `Total Belanja`, `Aksi`) with an empty-state message when no transactions match.
  - Added expandable/collapsible inline line-item detail sub-tables (`Detail` / `Tutup` toggle button) displaying `Nama Produk`, `Harga Satuan`, `Qty`, `Subtotal`, and a summary footer (`Total Item` & `Total Bayar`) formatted in Indonesian Rupiah (`Rp xx.xxx`).
  - Implemented pagination controls (`Previous`, page numbers, `Next`, and `Page X of Y`) preserving all active filter parameters (`startDate`, `endDate`, `cashierId`, `pageSize`).

### Changed
- Updated `BeeMart.Web/Pages/Shared/_Layout.cshtml` so the `"Transaksi Baru (New Transaction)"` navigation tab is rendered only for `Cashier` users while `"Riwayat Transaksi (History Transaction)"` is rendered and active-highlighted for both `Admin` and `Cashier` users.
- Extended `BeeMart.Web/wwwroot/css/site.css` with pure custom CSS classes for the history filter bar and expandable detail sub-tables (`.history-subtitle`, `.history-filter-bar`, `.history-filter-group`, `.history-filter-actions`, `.history-detail-row`, `.history-detail-cell`, `.history-detail-box`, `.history-detail-header`, `.history-subtable`).

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Verified access control and role-based views against `http://localhost:5002/Transaction/History`:
  - Unauthenticated `GET /Transaction/History` -> HTTP 302 redirect to `/Auth/Login?ReturnUrl=/Transaction/History`.
  - Authenticated `Cashier` (`steven`) -> HTTP 200 OK, header displays `Role: Cashier | Login: steven`, subtitle displays `Pencarian Data Transaksi (Hanya Kasir: steven)`, Cashier filter dropdown is hidden, only `Steven`'s transactions are displayed (even when attempting `?cashierId=66859`), and expandable line-item detail sub-table renders `Apel Fuji`, `Rp 35.000`, `Qty = 2`, `Subtotal = Rp 70.000`.
  - Authenticated `Admin` (`admin`) -> HTTP 200 OK, header displays `Role: Admin | Login: admin`, subtitle displays `Pencarian Data Transaksi (Menampilkan Semua Kasir)`, Cashier filter dropdown is visible, transactions across all cashiers (`Steven` and `Gabri`) are displayed, and filtering by `?cashierId=76985` scopes results to `Steven`.
  - Verified date range filtering (`startDate` & `endDate`) and pagination controls (`page=1&pageSize=1`) maintain filter query parameters across page links.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 12] - 2026-10-21 — WebClient New Transaction Page (POS Interface)

### Added
- Created POS Transaction ViewModels and API Response DTOs in `BeeMart.Web/Models/Transaction/`:
  - `CartItemViewModel.cs`: `IdProduct`, `ProductName`, `Price`, `Quantity`, `MaxStock`, and computed `Subtotal => Price * Quantity`.
  - `CheckoutRequestViewModel.cs`: `CheckoutRequestViewModel` (`IdPaymentMethod`, `CustomerMoney`, `Items`) and `CheckoutItemRequestViewModel` (`IdProduct`, `Quantity`).
  - `TransactionApiModels.cs`: `NextInvoiceResponse`, `PaymentMethodResponse`, `ProductSearchItemResponse`, `ProductSearchResponse`, `TransactionDetailResponse`, and `CreateTransactionResponse`.
- Created the Cashier Point of Sale (POS) Razor Page (`BeeMart.Web/Pages/Transaction/New.cshtml` & `New.cshtml.cs`):
  - Enforced strict role restriction via `[Authorize(Roles = "Cashier")]` (unauthenticated users redirect to `/Auth/Login`; `Admin` users redirect to `/Auth/AccessDenied`).
  - Implemented `OnGetAsync` to concurrently fetch the next invoice number (`GET /api/v1/transactions/next-invoice`), active payment methods (`GET /api/v1/payment-methods`), and initial product catalog (`GET /api/v1/products/search`) via `IApiClient`.
  - Implemented AJAX page handlers `OnGetSearchProductsAsync` (`?handler=SearchProducts`), `OnGetNextInvoiceAsync` (`?handler=NextInvoice`), and `OnPostCheckoutAsync` (`?handler=Checkout` posting to `POST /api/v1/transactions`).
  - Built a responsive 2-column POS interface with pure custom CSS (`FE-STYLE-001`, zero Bootstrap):
    - Top POS metadata bar displaying auto-generated read-only `Invoice No`, logged-in Cashier name, and real-time clock.
    - Product search & catalog section with debounced search + Enter trigger, pagination controls, `"Pilih"` dialog panel (`Barang Terpilih` with unit price, stock, and `Qty` input), and `"+ Keranjang"` quick-add button (disabled when `Stock <= 0`).
    - Dynamic `Daftar Belanjaan` cart table with `-`/`+`/input quantity controls enforcing `1 <= Quantity <= MaxStock` (`"Jumlah barang yang dimasukkan melebihi sisa stok."`), live `Subtotal`, `Total Item`, and `Total Belanja` in Rupiah (`Rp xx.xxx`), and `Hapus` button.
    - Payment & checkout section with dynamic payment method dropdown, `Uang Diterima (Customer Money)` input + quick denomination buttons (`Uang Pas`, `Rp 50.000`, `Rp 100.000`, `Rp 200.000`), live `Kembalian (Change)` display, and underpayment/empty-cart submit prevention.
    - Receipt modal (`#receipt-modal-backdrop`) displaying completed transaction details (`InvoiceNo`, `TransactionDate`, `CashierName`, `PaymentMethodName`, itemized table, `TotalItem`, `TotalPrice`, `CustomerMoney`, `Change`) and a `"Transaksi Baru"` reset button that clears the cart, reloads updated product stocks, and fetches the next sequential invoice number.

### Changed
- Updated `BeeMart.Web/Pages/_ViewImports.cshtml` to import `@using BeeMart.Web.Models.Transaction`.
- Updated `BeeMart.Web/Pages/Index.cshtml` to display role-aware `"Transaksi Baru (New Transaction)"` navigation button for `Cashier` users.
- Extended `BeeMart.Web/wwwroot/css/site.css` with POS layout and receipt modal utility classes (`.pos-layout`, `.pos-meta-bar`, `.qty-control`, `.qty-btn`, `.qty-input`, `.pos-summary-box`, `.receipt-meta-grid`, `.receipt-totals`).

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Verified access control against `http://localhost:5002/Transaction/New`:
  - Unauthenticated request -> HTTP 302 redirect to `/Auth/Login?ReturnUrl=/Transaction/New`.
  - Authenticated `Admin` (`admin`) -> HTTP 302 redirect to `/Auth/AccessDenied?ReturnUrl=/Transaction/New`.
  - Authenticated `Cashier` (`steven`) -> HTTP 200 OK with initial `INV-20261021-0001` and payment methods (`Cash`, `Debit`, `Credit`).
- Verified end-to-end POS workflow: product search for `"Apel"` (`12` total items, `10` returned on page 1), over-stock rejection (`HTTP 400`, `"Insufficient stock for product: Apel Fuji"`), underpayment rejection (`HTTP 400`, `"Customer money is insufficient"`), valid checkout (`HTTP 201 Created`, `2x Apel Fuji`, `TotalPrice = 70000.00`, `CustomerMoney = 100000`, `Change = 30000.00`), next invoice increment (`INV-20261021-0002`), and database stock deduction (`48 -> 46`).
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 11] - 2026-10-21 — WebClient Authentication Pages (Login, Register, Logout, Access Denied)

### Added
- Created Authentication ViewModels and API Request/Response DTOs in `BeeMart.Web/Models/Auth/`:
  - `LoginViewModel.cs`: `LoginViewModel` (`Username`, `Password`, `Role` defaulting to `"Cashier"`), `LoginRequest`, and `LoginResponse`.
  - `RegisterViewModel.cs`: `RegisterViewModel` (`Username`, `FullName`, `Password`, `RePassword` with `[Compare("Password")]`, `Role` defaulting to `"Cashier"`), `RegisterRequest`, and `RegisterResponse`.
- Created Authentication Razor Pages in `BeeMart.Web/Pages/Auth/` styled exclusively with `wwwroot/css/site.css` (`FE-STYLE-001`, zero Bootstrap):
  - `Login.cshtml` & `Login.cshtml.cs`: Renders centered `BeeMart Login` card (`card-auth`) with `Username`, `Password`, `Role` dropdown (`Kasir`/`Cashier`, `Admin`/`Admin`), `Login` submit button, and link to `/Auth/Register`. Calls `_apiClient.PostAsync<LoginRequest, LoginResponse>("auth/login", ...)`, renders backend error messages (e.g., `"Invalid login"`) in a custom `.alert.alert-danger` box, and on success issues the `BeeMart.Auth` cookie via `HttpContext.SignInAsync` storing `ClaimTypes.NameIdentifier` (`idUser`), `ClaimTypes.Name` (`username`), `fullName`, `ClaimTypes.Role` (`role`), and `jwt_token` before redirecting.
  - `Register.cshtml` & `Register.cshtml.cs`: Renders centered `Register Akun` card (`card-auth`) with `Username`, `Full Name`, `Password`, `Re-Password`, `Role` dropdown (`Kasir`/`Cashier`, `Admin`/`Admin`), `Daftar` submit button, and link to `/Auth/Login`. Validates required fields and password confirmation, calls `_apiClient.PostAsync<RegisterRequest, RegisterResponse>("auth/register", ...)`, displays exact backend conflict/error messages (e.g., `"username has already exists"`), and on success sets `TempData["SuccessMessage"] = "Account registered successfully. Please login."` and redirects to `/Auth/Login`.
  - `Logout.cshtml` & `Logout.cshtml.cs`: Clears the Cookie Authentication session via `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)` and redirects to `/Auth/Login`.
  - `AccessDenied.cshtml` & `AccessDenied.cshtml.cs`: Renders a friendly custom 403 Forbidden permission error card with links back to `/Index` and `/Auth/Login`.

### Changed
- Updated `BeeMart.Web/Pages/_ViewImports.cshtml` to import `@using BeeMart.Web.Models.Auth`.
- Updated `BeeMart.Web/Pages/Shared/_Layout.cshtml` to display the authenticated user's full name (`fullName`), username, Indonesian role badge (`Kasir` for `Cashier`, `Admin` for `Admin`), `Logout` form button, and POS navigation tabs (`Transaksi Baru (New Transaction)` and `Riwayat Transaksi (History Transaction)`).

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Verified `BeeMart.Web.csproj` contains zero EF Core or SQL Server references.
- Executed end-to-end Web authentication flow tests against `BeeMart.Api` (`http://localhost:5001`) and `BeeMart.Web` (`http://localhost:5002`) verifying `GET /Auth/Login`, invalid login error alert (`"Invalid login"`), valid login cookie issuance (`BeeMart.Auth`) and redirect for Cashier `steven`, `GET /Auth/Register`, duplicate registration error alert (`"username has already exists"`), new user registration redirect with `TempData["SuccessMessage"]`, `POST /Auth/Logout`, and `GET /Auth/AccessDenied`.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 10] - 2026-10-21 — Web Client Setup, HTTP Client Infrastructure & Custom CSS Foundations

### Added
- Created Typed HTTP API Client service in `BeeMart.Web/Services/`:
  - `IApiClient.cs`: Defines `ApiResponse<T>` (`Status`, `Message`, `Data`, `StatusCode`, `IsSuccess`) and `IApiClient` methods (`GetAsync<T>`, `PostAsync<TRequest, TResponse>`, `PostAsync<TResponse>`).
  - `ApiClient.cs`: Implements `IApiClient` using `HttpClient` (`IHttpClientFactory`) and `IHttpContextAccessor`, automatically extracting the authenticated user's JWT Bearer token from Cookie Authentication claims (`"jwt_token"` / `"Token"` / `"access_token"`) and attaching `Authorization: Bearer <token>` to outgoing API requests, while unwrapping `ApiResponse<T>` JSON payloads across both success and error HTTP status codes.
- Implemented Pure Custom CSS Framework (`BeeMart.Web/wwwroot/css/site.css`, `FE-STYLE-001` — zero Bootstrap or external CSS libraries):
  - Defined `:root` CSS Custom Properties for brand palette (`--color-primary: #F59E0B` amber-honey theme, `--color-secondary: #1E88E5` POS blue, `--color-dark-slate: #1E293B`, `--color-bg: #F8FAFC`, `--color-surface: #FFFFFF`, `--color-border: #E2E8F0`, `--color-danger: #EF4444`, `--color-success: #10B981`, `--color-warning: #F59E0B`), system sans-serif typography, border radii, shadows, and transitions.
  - Implemented global reset (`box-sizing: border-box; margin: 0; padding: 0;`), typography utilities (`.price-text`, `.price-total`, `.text-muted`), layout & flex/grid helpers, navigation bar & POS tabs, cards/panels (`.card`, `.card-header`, `.card-body`, `.card-footer`, `.invoice-banner`), form inputs & validation errors, buttons (`.btn`, `.btn-primary`, `.btn-accent`, `.btn-secondary`, `.btn-danger`), responsive tables (`.table`, `.table-striped`, `.table-responsive`), badges/chips, dismissible alerts, modal dialogs (`.modal-backdrop`, `.modal-dialog`), and pagination controls.
- Populated `BeeMart.Web/wwwroot/js/site.js` with vanilla JS client utilities (`window.BeeMart.formatRealtimeClock`, `window.BeeMart.formatRupiah`, automatic `#realtime-clock` 1-second ticker, and `[data-dismiss='alert']` handler).

### Configured
- Updated `BeeMart.Web/appsettings.json`: Configured `ApiSettings:BaseUrl` to `"http://localhost:5001/api/v1/"` with zero database connection strings (`NFR-ARCH-002`).
- Updated `BeeMart.Web/Program.cs`:
  - Registered `AddHttpContextAccessor()` and Typed HTTP Client `AddHttpClient<IApiClient, ApiClient>` pointing to `ApiSettings:BaseUrl`.
  - Registered Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`) with `LoginPath = "/Auth/Login"`, `LogoutPath = "/Auth/Logout"`, `AccessDeniedPath = "/Auth/AccessDenied"`, and `HttpOnly = true`.
  - Configured middleware pipeline (`UseStaticFiles`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `MapRazorPages`).
- Updated `BeeMart.Web/Pages/Shared/_Layout.cshtml` and `BeeMart.Web/Pages/Index.cshtml` with semantic HTML, authentication-aware header & navigation tabs, and `TempData` alert banner rendering.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Verified `BeeMart.Web.csproj` contains zero EF Core or SQL Server references, `BeeMart.Web/appsettings.json` contains zero connection strings, and `_Layout.cshtml` contains zero Bootstrap references.
- Executed runtime smoke test against `http://localhost:5002/`: verified HTTP 200 OK on `/`, `/css/site.css`, and `/js/site.js`.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 9] - 2026-10-21 — Backend History Endpoint (Role-Isolated)

### Added
- Created Transaction History DTOs under `BeeMart.Api/Models/DTOs/Transaction/`:
  - `TransactionHistoryItemDetailResponse.cs`: `IdProduct` (`string`), `ProductName` (`string`), `Quantity` (`int`), `Price` (`decimal`), `Subtotal` (`decimal`).
  - `TransactionHistoryItemResponse.cs`: `IdTransaction` (`string`), `InvoiceNo` (`string`), `TransactionDate` (`DateTime`), `CashierId` (`string`), `CashierName` (`string`), `PaymentMethodName` (`string`), `TotalItem` (`int`), `TotalPrice` (`decimal`), `Details` (`List<TransactionHistoryItemDetailResponse>`).
  - `TransactionHistoryResponse.cs`: `Items` (`List<TransactionHistoryItemResponse>`), `TotalItems` (`int`), `Page` (`int`), `PageSize` (`int`), `TotalPages` (`int`).
- Added `GET /api/v1/transactions/history` (`API-HIST-01`) to `TransactionsController` (`BeeMart.Api/Controllers/TransactionsController.cs`):
  - Enforces strict role isolation (`NFR-SEC-002`) via JWT claims (`ClaimsHelper`): `Cashier` requests are strictly scoped to `TrTransaction.IdUser == loggedInUserId` (ignoring any `cashierId` query parameter), while `Admin` requests can view all cashiers' transactions or filter by optional `cashierId`.
  - Supports optional inclusive date range filtering (`startDate` from `00:00:00`, `endDate` through `23:59:59.9999999` when date-only) and pagination (`page` default `1` min `1`, `pageSize` default `10` min `1` max `50`).
  - Filters active transactions (`Stsrc == true`), orders by `TransactionDate` descending then `InvoiceNo` descending, eager-loads `User` (`MsUser`), `PaymentMethod` (`LtPaymentMethod`), and active `TransactionDetails` (`TrTransactionDetail`) with `Product` (`MsProduct`) via `.Include()` / `.ThenInclude()` without N+1 queries, calculates `TotalItem` as the sum of line-item quantities, and returns **HTTP 200 OK** wrapped in `ApiResponse<TransactionHistoryResponse>`.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Executed live HTTP smoke tests against `https://localhost:7001/api/v1/transactions/history`: verified HTTP 401 for unauthenticated requests, Cashier isolation for `steven` (`76985`), Cashier override prevention when `steven` passes `?cashierId=66859`, Admin (`00001`) global visibility across multiple cashiers (`steven` and `gabri`), Admin `?cashierId=76985` filtering, date range filtering (`startDate`/`endDate`), pagination (`page=1&pageSize=1` and `page=1&pageSize=5`), and complete line-item details projection.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 8] - 2026-10-20 — Backend Transaction Endpoints

### Added
- Created Transaction DTOs under `BeeMart.Api/Models/DTOs/Transaction/`:
  - `NextInvoiceResponse.cs`: `InvoiceNo` (`string`).
  - `TransactionDetailRequest.cs`: `IdProduct` (`string`), `Quantity` (`int`).
  - `CreateTransactionRequest.cs`: `IdPaymentMethod` (`string`), `CustomerMoney` (`decimal`), `Items` (`List<TransactionDetailRequest>`).
  - `TransactionDetailResponse.cs`: `IdProduct` (`string`), `ProductName` (`string`), `Quantity` (`int`), `Price` (`decimal`), `Subtotal` (`decimal`).
  - `CreateTransactionResponse.cs`: `InvoiceNo` (`string`), `TransactionDate` (`DateTime`), `CashierId` (`string`), `CashierName` (`string`), `PaymentMethodName` (`string`), `TotalItem` (`int`), `TotalPrice` (`decimal`), `CustomerMoney` (`decimal`), `Change` (`decimal`), `Details` (`List<TransactionDetailResponse>`).
- Created `TransactionsController` (`BeeMart.Api/Controllers/TransactionsController.cs`) decorated with `[ApiController]`, `[Route("api/v1/transactions")]`, and `[Authorize]`:
  - **`GET /api/v1/transactions/next-invoice` (`API-TRX-01`)**: Inspects existing `TrTransaction.InvoiceNo` records matching today's prefix (`INV-YYYYMMDD-`), determines the next 4-digit sequence number (`0001` if none exist today), and returns **HTTP 200 OK** wrapped in `ApiResponse<NextInvoiceResponse>`.
  - **`POST /api/v1/transactions` (`API-TRX-02`)**:
    - Extracts logged-in cashier/admin identity (`idUser`, `username`, `fullName`) from JWT claims via `ClaimsHelper`.
    - Validates non-empty `Items` (`"Transaction must contain at least one item"`), positive `Quantity > 0`, active `LtPaymentMethod` (`"Invalid payment method"`), active `MsProduct` existence, sufficient stock (`"Insufficient stock for product: {ProductName}"`), and `CustomerMoney >= TotalPrice` (`"Customer money is insufficient"`).
    - Executes within an EF Core database transaction (`BeginTransactionAsync`), re-fetches the next invoice number atomically, deducts `MsProduct.Stock` (updating `Dateup` and `Userup`), inserts `TrTransaction` and `TrTransactionDetail` records with audit trail fields (`Stsrc = true`, `DateIn = DateTime.Now`, `UserIn = username`), commits the transaction, and returns **HTTP 201 Created** wrapped in `ApiResponse<CreateTransactionResponse>`.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Executed live HTTP smoke tests against `https://localhost:7001/api/v1/transactions/next-invoice` and `https://localhost:7001/api/v1/transactions` as Cashier `steven`: verified initial invoice `INV-20261020-0001`, HTTP 400 validation on empty cart, insufficient customer money, and quantity exceeding stock, HTTP 201 on valid cart (`2 Apel Fuji`, `TotalPrice = 70000.00`, `CustomerMoney = 100000`, `Change = 30000.00`, `MsProduct.Stock` decremented from `50` to `48`), and sequence increment to `INV-20261020-0002`.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

---

## [Phase 7] - 2026-10-20 — Backend Master Data Endpoints

### Added
- Created Master Data DTOs under `BeeMart.Api/Models/DTOs/Master/`:
  - `PaymentMethodResponse.cs`: `IdPaymentMethod` (`string`), `PaymentMethodName` (`string`).
  - `ProductSearchItemResponse.cs`: `IdProduct` (`string`), `ProductName` (`string`), `Price` (`decimal`), `Stock` (`int`), `ProductImage` (`string?`).
  - `ProductSearchResponse.cs`: `Items` (`List<ProductSearchItemResponse>`), `TotalItems` (`int`), `Page` (`int`), `PageSize` (`int`), `TotalPages` (`int`).
- Created `PaymentMethodsController` (`BeeMart.Api/Controllers/PaymentMethodsController.cs`) decorated with `[ApiController]`, `[Route("api/v1/payment-methods")]`, and `[Authorize]`:
  - **`GET /api/v1/payment-methods` (`API-PAY-01`)**: Queries active payment methods (`Stsrc == true`) from `dbo.LtPaymentMethod` ordered by `IdPaymentMethod` ascending, maps to `List<PaymentMethodResponse>`, and returns **HTTP 200 OK** wrapped in `ApiResponse<List<PaymentMethodResponse>>`.
- Created `ProductsController` (`BeeMart.Api/Controllers/ProductsController.cs`) decorated with `[ApiController]`, `[Route("api/v1/products")]`, and `[Authorize]`:
  - **`GET /api/v1/products/search` (`API-PROD-01`)**: Accepts optional query parameters `search` (`string?`), `page` (`int`, default `1`, min `1`), and `pageSize` (`int`, default `10`, min `1`, max `50`). Filters `dbo.MsProduct` by `Stsrc == true` and `Stock > 0`, applies case-insensitive `Name` contains filtering when `search` is provided, orders by `Name` ascending, calculates `TotalItems` and `TotalPages`, applies `.Skip((page - 1) * pageSize).Take(pageSize)`, and returns **HTTP 200 OK** wrapped in `ApiResponse<ProductSearchResponse>`.

### Verified
- Executed `dotnet build BeeMart.sln`: succeeded with `0 Warning(s)` and `0 Error(s)`.
- Executed live HTTP smoke tests against `https://localhost:7001/api/v1/payment-methods` and `https://localhost:7001/api/v1/products/search` verifying HTTP 401 without token, HTTP 200 with Bearer JWT token for payment methods (3 active records), paginated product search (`page=1&pageSize=5`), keyword filtering (`search=Apel`), and exclusion of out-of-stock (`Stock <= 0`) and inactive (`Stsrc == false`) products.
- Updated `STRUCTURE.md`, `OPEN-ITEMS.md`, and `CHANGELOG.md`.

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
