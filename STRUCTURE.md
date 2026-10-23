# Project Structure — BeeMart POS Web App

Dokumentasi struktur direktori dan berkas untuk solusi **BeeMart POS Web App** (.NET 8, ASP.NET Core) dengan arsitektur *Separation of Concerns* (`NFR-ARCH-001`, `NFR-ARCH-002`).

---

## 1. Architecture & Project Relationship Diagram

`BeeMart.Web` (WebClient) hanya bertindak sebagai *consumer* yang memanggil `BeeMart.Api` (Backend API) melalui HTTP/JSON (`ApiSettings:BaseUrl`). Seluruh logika bisnis dan akses database SQL Server melalui Entity Framework Core terpusat di `BeeMart.Api` (`ConnectionStrings:DefaultConnection`).

```mermaid
flowchart LR
    User(["Pengguna (Admin / Kasir)"])
    Web["BeeMart.Web<br/>(ASP.NET Core Razor Pages, net8.0)<br/>https://localhost:7002"]
    Api["BeeMart.Api<br/>(ASP.NET Core Web API, net8.0)<br/>https://localhost:7001"]
    Db[("SQL Server<br/>Database: BeeMart<br/>(DDL + Seeded DML)")]

    User -->|"HTTP Browser (Custom CSS, No Bootstrap)"| Web
    Web -->|"HTTP JSON REST API (ApiResponse<T>)<br/>ApiSettings:BaseUrl (/api/v1/...)"| Api
    Api -->|"EF Core 8 (SqlServer)<br/>ConnectionStrings:DefaultConnection"| Db
```

---

## 2. Directory Tree & File Count Statistics

Seluruh komponen solusi pada **Phase 1–15** telah selesai diimplementasikan dan berstatus **Active**.

### 2.1 File Count Summary (Excluding Build Artifacts `bin/` & `obj/`)
| Scope / Project | File Count | Breakdown |
| :--- | :---: | :--- |
| **Solution Root & Database (`BeeMart/` & `Database/`)** | **10** | 8 root configuration/documentation files (`.gitattributes`, `.gitignore`, `README.md`, `BeeMart.sln`, `PRD.md`, `STRUCTURE.md`, `CHANGELOG.md`, `OPEN-ITEMS.md`) + 2 database files (`Database/.gitkeep`, `Database/BeeMart.sql`) |
| **Backend API (`BeeMart.Api/`)** | **35** | 5 project/config files, 5 Controller files (`Controllers/`), 1 `DbContext` (`Data/`), 5 EF Core Entities (`Models/Entities/`), 15 DTOs (`Models/DTOs/`), 4 Common infrastructure files (`Common/`) |
| **WebClient UI (`BeeMart.Web/`)** | **35** | 5 project/config files, 6 ViewModels/DTOs (`Models/`), 2 Typed HTTP Client services (`Services/`), 19 Razor Pages & PageModels (`Pages/`), 3 static assets (`wwwroot/favicon.ico`, `wwwroot/css/site.css`, `wwwroot/js/site.js`) |
| **Total Solution Files** | **80** | **100% Active (Phase 1–15 Complete)** |

### 2.2 Complete Directory Tree

```text
BeeMart/
├── .gitattributes                                   [Active - Repo Root (Frozen)]
├── .gitignore                                       [Active - Repo Housekeeping]
├── README.md                                        [Active - Repo Housekeeping]
├── BeeMart.sln                                      [Active - Phase 1-15]
├── PRD.md                                           [Active - Phase 0/1 (Read-Only)]
├── STRUCTURE.md                                     [Active - Phase 1-15]
├── CHANGELOG.md                                     [Active - Phase 1-15]
├── OPEN-ITEMS.md                                    [Active - Phase 1-15]
├── Database/                                        [Active - Phase 1]
│   ├── .gitkeep                                     [Active - Phase 1]
│   └── BeeMart.sql                                  [Active - Phase 2 (DDL) & Phase 3 (Seed DML)]
├── BeeMart.Api/                                     [Active - Phase 1]
│   ├── BeeMart.Api.csproj                           [Active - Phase 1 & Phase 5]
│   ├── Program.cs                                   [Active - Phase 1, Phase 4, Phase 5 & Phase 6]
│   ├── appsettings.json                             [Active - Phase 1, Phase 4 & Phase 5]
│   ├── appsettings.Development.json                 [Active - Phase 1]
│   ├── Properties/
│   │   └── launchSettings.json                      [Active - Phase 1]
│   ├── Controllers/                                 [Active - Phase 1, Phase 6, Phase 7, Phase 8 & Phase 9]
│   │   ├── .gitkeep                                 [Active - Phase 1]
│   │   ├── AuthController.cs                        [Active - Phase 6]
│   │   ├── ProductsController.cs                    [Active - Phase 7]
│   │   ├── PaymentMethodsController.cs              [Active - Phase 7]
│   │   └── TransactionsController.cs                [Active - Phase 8 & Phase 9]
│   ├── Data/                                        [Active - Phase 4]
│   │   └── BeeMartDbContext.cs                      [Active - Phase 4]
│   ├── Models/                                      [Active - Phase 4, Phase 6, Phase 7, Phase 8 & Phase 9]
│   │   ├── Entities/                                [Active - Phase 4]
│   │   │   ├── MsUser.cs                            [Active - Phase 4]
│   │   │   ├── LtPaymentMethod.cs                   [Active - Phase 4]
│   │   │   ├── MsProduct.cs                         [Active - Phase 4]
│   │   │   ├── TrTransaction.cs                     [Active - Phase 4]
│   │   │   └── TrTransactionDetail.cs               [Active - Phase 4]
│   │   └── DTOs/                                    [Active - Phase 6, Phase 7, Phase 8 & Phase 9]
│   │       ├── Auth/                                [Active - Phase 6]
│   │       │   ├── LoginRequest.cs                  [Active - Phase 6]
│   │       │   ├── LoginResponse.cs                 [Active - Phase 6]
│   │       │   ├── RegisterRequest.cs               [Active - Phase 6]
│   │       │   └── RegisterResponse.cs              [Active - Phase 6]
│   │       ├── Master/                              [Active - Phase 7]
│   │       │   ├── PaymentMethodResponse.cs         [Active - Phase 7]
│   │       │   ├── ProductSearchItemResponse.cs     [Active - Phase 7]
│   │       │   └── ProductSearchResponse.cs         [Active - Phase 7]
│   │       └── Transaction/                         [Active - Phase 8 & Phase 9]
│   │           ├── NextInvoiceResponse.cs           [Active - Phase 8]
│   │           ├── TransactionDetailRequest.cs      [Active - Phase 8]
│   │           ├── CreateTransactionRequest.cs      [Active - Phase 8]
│   │           ├── TransactionDetailResponse.cs     [Active - Phase 8]
│   │           ├── CreateTransactionResponse.cs     [Active - Phase 8]
│   │           ├── TransactionHistoryItemDetailResponse.cs [Active - Phase 9]
│   │           ├── TransactionHistoryItemResponse.cs       [Active - Phase 9]
│   │           └── TransactionHistoryResponse.cs           [Active - Phase 9]
│   └── Common/                                      [Active - Phase 5]
│       ├── ApiResponse.cs                           [Active - Phase 5]
│       ├── PasswordHasher.cs                        [Active - Phase 5]
│       ├── JwtTokenService.cs                       [Active - Phase 5]
│       └── ClaimsHelper.cs                          [Active - Phase 5]
└── BeeMart.Web/                                     [Active - Phase 1, Phase 10, Phase 11, Phase 12 & Phase 13]
    ├── BeeMart.Web.csproj                           [Active - Phase 1]
    ├── Program.cs                                   [Active - Phase 1 & Phase 10]
    ├── appsettings.json                             [Active - Phase 1 & Phase 10]
    ├── appsettings.Development.json                 [Active - Phase 1]
    ├── Properties/
    │   └── launchSettings.json                      [Active - Phase 1]
    ├── Models/                                      [Active - Phase 11, Phase 12 & Phase 13]
    │   ├── Auth/                                    [Active - Phase 11]
    │   │   ├── LoginViewModel.cs                    [Active - Phase 11]
    │   │   └── RegisterViewModel.cs                 [Active - Phase 11]
    │   └── Transaction/                             [Active - Phase 12 & Phase 13]
    │       ├── CartItemViewModel.cs                 [Active - Phase 12]
    │       ├── CheckoutRequestViewModel.cs          [Active - Phase 12]
    │       ├── TransactionApiModels.cs              [Active - Phase 12]
    │       └── TransactionHistoryViewModel.cs       [Active - Phase 13]
    ├── Services/                                    [Active - Phase 10]
    │   ├── IApiClient.cs                            [Active - Phase 10]
    │   └── ApiClient.cs                             [Active - Phase 10]
    ├── Pages/                                       [Active - Phase 1, Phase 10, Phase 11, Phase 12 & Phase 13]
    │   ├── _ViewImports.cshtml                      [Active - Phase 1, Phase 11 & Phase 12]
    │   ├── _ViewStart.cshtml                        [Active - Phase 1]
    │   ├── Index.cshtml                             [Active - Phase 1, Phase 10 & Phase 12]
    │   ├── Index.cshtml.cs                          [Active - Phase 1]
    │   ├── Error.cshtml                             [Active - Phase 1]
    │   ├── Error.cshtml.cs                          [Active - Phase 1]
    │   ├── Shared/                                  [Active - Phase 1, Phase 10, Phase 11 & Phase 13]
    │   │   └── _Layout.cshtml                       [Active - Phase 1, Phase 10, Phase 11 & Phase 13]
    │   ├── Auth/                                    [Active - Phase 11]
    │   │   ├── Login.cshtml                         [Active - Phase 11]
    │   │   ├── Login.cshtml.cs                      [Active - Phase 11]
    │   │   ├── Register.cshtml                      [Active - Phase 11]
    │   │   ├── Register.cshtml.cs                   [Active - Phase 11]
    │   │   ├── Logout.cshtml                        [Active - Phase 11]
    │   │   ├── Logout.cshtml.cs                     [Active - Phase 11]
    │   │   ├── AccessDenied.cshtml                  [Active - Phase 11]
    │   │   └── AccessDenied.cshtml.cs               [Active - Phase 11]
    │   └── Transaction/                             [Active - Phase 12 & Phase 13]
    │       ├── New.cshtml                           [Active - Phase 12]
    │       ├── New.cshtml.cs                        [Active - Phase 12]
    │       ├── History.cshtml                       [Active - Phase 13]
    │       └── History.cshtml.cs                    [Active - Phase 13]
    └── wwwroot/                                     [Active - Phase 1, Phase 10, Phase 12 & Phase 13]
        ├── favicon.ico                              [Active - Phase 1]
        ├── css/
        │   └── site.css                             [Active - Phase 10, Phase 12 & Phase 13 (Custom CSS Framework)]
        └── js/
            └── site.js                              [Active - Phase 10 (Clock, Rupiah & Alert Utilities)]
```

---

## 3. Detailed Inventory of Solution Folders & Files (Phase 1–15)

### 3.1 Solution Root (`BeeMart/`)
| Path | Project / Scope | Purpose (Fungsi) | Connections & Dependencies |
| :--- | :--- | :--- | :--- |
| `.gitattributes` | Repository Root | Konfigurasi normalisasi baris teks Git (`* text=auto`). | Digunakan oleh Git pada seluruh file di repositori. |
| `.gitignore` | Repository Root | Aturan pengabaian file Git standar Visual Studio / .NET (`bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `*.log`, `TestResults/`, `*.mdf`, `*.ldf`, `scratch/`), dengan pengecualian eksplisit agar `launchSettings.json` dan `appsettings.Development.json` tetap terlacak. | Menjaga kebersihan repositori Git dari artefak kompilasi dan file sementara. |
| `README.md` | Repository Root | Panduan utama repositori yang memuat deskripsi proyek BeeMart POS, *tech stack*, tata letak direktori, langkah instalasi database & aplikasi, tabel akun *seed* pengembangan, serta status pengerjaan per fase. | Merujuk ke `Database/BeeMart.sql`, `BeeMart.Api/appsettings.json`, `STRUCTURE.md`, `OPEN-ITEMS.md`, `CHANGELOG.md`, dan `PRD.md`. |
| `BeeMart.sln` | Solution Root | File Solution Visual Studio / .NET yang menggabungkan proyek Backend API dan WebClient (`DELIV-003`). | Mereferensikan `BeeMart.Api/BeeMart.Api.csproj` dan `BeeMart.Web/BeeMart.Web.csproj`. |
| `PRD.md` | Solution Root | Dokumen spesifikasi kebutuhan produk (*Product Requirements Document*) BeeMart POS. | Menjadi acuan arsitektur, ERD database, endpoint API, dan aturan bisnis seluruh proyek. |
| `STRUCTURE.md` | Solution Root | Dokumentasi struktur direktori, fungsi setiap file, serta relasi antar komponen. | Mendokumentasikan `BeeMart.sln`, `BeeMart.Api`, `BeeMart.Web`, `Database/`, dan file tingkat repositori. |
| `CHANGELOG.md` | Solution Root | Catatan riwayat perubahan (*changelog*) per fase pengerjaan dan pemeliharaan repositori. | Mencatat perubahan pada seluruh file di dalam `BeeMart/`. |
| `OPEN-ITEMS.md` | Solution Root | Daftar *checklist* tugas per fase (Phase 1–15), *checklist* pemeliharaan repositori, serta catatan keputusan/asumsi teknis. | Berhubungan dengan rencana pengerjaan di `PRD.md` dan `CHANGELOG.md`. |
| `Database/` | Solution Root | Direktori khusus penyimpanan script SQL database (`DELIV-004`). | Menampung `Database/BeeMart.sql` dan `Database/.gitkeep`. |
| `Database/.gitkeep` | Solution Root | Penanda agar folder `Database/` tetap terlacak di Git. | Dipertahankan di dalam folder `Database/` bersama `Database/BeeMart.sql`. |
| `Database/BeeMart.sql` | Solution Root (`Database/`) | Script T-SQL SQL Server *idempotent* (re-runnable) berisi: (1) **Phase 2 DDL** pembuatan database `BeeMart`, 5 tabel (`LtPaymentMethod`, `MsUser`, `MsProduct`, `TrTransaction`, `TrTransactionDetail`), 4 indeks Section 5.4, serta *primary key*, *foreign key*, *unique*, *check*, dan *default constraints* bernama eksplisit; dan (2) **Phase 3 DML Seed Data** berpelindung `IF NOT EXISTS` / `WHERE NOT EXISTS` untuk 3 metode pembayaran (`LtPaymentMethod`), 6 pengguna dengan *BCrypt password hash* (`MsUser`), dan 22 produk dengan GUID tetap (`MsProduct`: 12 produk `Apel`, 4 produk `Susu`, 6 produk sembako lainnya) (`DELIV-004`). | Dieksekusi di SQL Server (`localhost\SQLEXPRESS`), dan skema serta data awalnya dipetakan secara presisi oleh `BeeMartDbContext` beserta 5 entitas EF Core di `BeeMart.Api` (Phase 4). |

### 3.2 Backend API Project (`BeeMart.Api/`)
| Path | Project | Purpose (Fungsi) | Connections & Dependencies |
| :--- | :--- | :--- | :--- |
| `BeeMart.Api/` | `BeeMart.Api` | Folder utama proyek Backend RESTful API (`DELIV-002`, `NFR-ARCH-001`). | Terdaftar di `BeeMart.sln`; dipanggil oleh `BeeMart.Web` melalui HTTP JSON. |
| `BeeMart.Api/BeeMart.Api.csproj` | `BeeMart.Api` | File definisi proyek C# (`Microsoft.NET.Sdk.Web`) dengan target `net8.0`. | Memuat referensi paket NuGet EF Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`, `.Design`, `.Tools` v8.0.32), `BCrypt.Net-Next` (v4.0.3), dan `Microsoft.AspNetCore.Authentication.JwtBearer` (v8.0.13). |
| `BeeMart.Api/Program.cs` | `BeeMart.Api` | Titik masuk (*entry point*) aplikasi Backend API, registrasi `BeeMartDbContext` (`UseSqlServer("DefaultConnection")`), registrasi DI layanan fondasi (`IPasswordHasher`, `IJwtTokenService`), konfigurasi autentikasi JWT Bearer (`JwtBearerDefaults.AuthenticationScheme` & `TokenValidationParameters`), konfigurasi `SuppressModelStateInvalidFilter = true` pada `AddControllers()`, dan pipeline HTTP middleware (`UseAuthentication`, `UseAuthorization`, `MapControllers`). | Membaca konfigurasi `DefaultConnection` dan `JwtSettings` dari `BeeMart.Api/appsettings.json`, mendaftarkan `BeeMartDbContext`, `PasswordHasher`, dan `JwtTokenService`, serta memetakan route ke `Controllers/`. |
| `BeeMart.Api/appsettings.json` | `BeeMart.Api` | File konfigurasi utama Backend API yang menyimpan connection string SQL Server `DefaultConnection` (`Server=localhost\SQLEXPRESS;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;`) dan pengaturan `JwtSettings` (`SecretKey`, `Issuer`, `Audience`, `ExpiryMinutes`). | Dibaca oleh `Program.cs` dan `JwtTokenService` untuk menginisialisasi `BeeMartDbContext` serta pembuatan/validasi token JWT. |
| `BeeMart.Api/appsettings.Development.json` | `BeeMart.Api` | Konfigurasi spesifik environment `Development` (level logging). | Meng-override `BeeMart.Api/appsettings.json` saat `ASPNETCORE_ENVIRONMENT=Development`. |
| `BeeMart.Api/Properties/launchSettings.json` | `BeeMart.Api` | Pengaturan profil eksekusi lokal dan port tetap Backend API (`https://localhost:7001`, `http://localhost:5001`). | Port `https://localhost:7001` disinkronkan dengan `ApiSettings:BaseUrl` pada `BeeMart.Web/appsettings.json`. |
| `BeeMart.Api/Data/BeeMartDbContext.cs` | `BeeMart.Api` | Kelas `DbContext` EF Core (`BeeMartDbContext`) yang mengekspos `DbSet` (`Users`, `PaymentMethods`, `Products`, `Transactions`, `TransactionDetails`) dan mengonfigurasi pemetaan tabel, tipe data/presisi `decimal`, batas panjang `NVARCHAR`, indeks unik/non-unik, serta relasi *foreign key* (`DeleteBehavior.Restrict`) melalui Fluent API di `OnModelCreating`. | Diregistrasikan di `BeeMart.Api/Program.cs` dan memetakan 5 kelas entitas di `BeeMart.Api/Models/Entities/` ke skema SQL `Database/BeeMart.sql`. |
| `BeeMart.Api/Models/Entities/MsUser.cs` | `BeeMart.Api` | Entitas POCO untuk tabel `dbo.MsUser` (`IdUser`, `Username`, `FullName`, `PasswordHash`, `Role`, kolom audit `Stsrc`/`DateIn`/`UserIn`/`Dateup`/`Userup`, dan koleksi navigasi `Transactions`). | Dipetakan oleh `BeeMartDbContext`; berelasi `1:N` dengan `TrTransaction`. |
| `BeeMart.Api/Models/Entities/LtPaymentMethod.cs` | `BeeMart.Api` | Entitas POCO untuk tabel `dbo.LtPaymentMethod` (`IdPaymentMethod`, `MethodName`, kolom audit, dan koleksi navigasi `Transactions`). | Dipetakan oleh `BeeMartDbContext`; berelasi `1:N` dengan `TrTransaction`. |
| `BeeMart.Api/Models/Entities/MsProduct.cs` | `BeeMart.Api` | Entitas POCO untuk tabel `dbo.MsProduct` (`IdProduct`, `Name`, `Weight`, `Stock`, `Price`, `Discount`, kolom audit, dan koleksi navigasi `TransactionDetails`). | Dipetakan oleh `BeeMartDbContext`; berelasi `1:N` dengan `TrTransactionDetail`. |
| `BeeMart.Api/Models/Entities/TrTransaction.cs` | `BeeMart.Api` | Entitas POCO untuk tabel header transaksi `dbo.TrTransaction` (`IdTransaction`, `InvoiceNo`, `TransactionDate`, `IdUser`, `IdPaymentMethod`, properti `[NotMapped] TotalItem`, `TotalPrice`, kolom audit, serta navigasi `User`, `PaymentMethod`, dan `TransactionDetails`). | Dipetakan oleh `BeeMartDbContext`; berelasi `N:1` ke `MsUser` & `LtPaymentMethod`, serta `1:N` ke `TrTransactionDetail`. |
| `BeeMart.Api/Models/Entities/TrTransactionDetail.cs` | `BeeMart.Api` | Entitas POCO untuk tabel rincian transaksi `dbo.TrTransactionDetail` (`IdTransactionDetail`, `IdTransaction`, `IdProduct`, `Qty`, `Price`, `Discount`, `SubTotal`, kolom audit, serta navigasi `Transaction` dan `Product`). | Dipetakan oleh `BeeMartDbContext`; berelasi `N:1` ke `TrTransaction` dan `MsProduct`. |
| `BeeMart.Api/Models/DTOs/Auth/LoginRequest.cs` | `BeeMart.Api` | DTO request untuk endpoint `POST /api/v1/auth/login` (`Username`, `Password`, `Role`). | Digunakan oleh `AuthController.Login`. |
| `BeeMart.Api/Models/DTOs/Auth/LoginResponse.cs` | `BeeMart.Api` | DTO response data untuk endpoint `POST /api/v1/auth/login` (`IdUser`, `Username`, `FullName`, `Role`, `Token`). | Dibungkus di dalam `ApiResponse<LoginResponse>` oleh `AuthController.Login`. |
| `BeeMart.Api/Models/DTOs/Auth/RegisterRequest.cs` | `BeeMart.Api` | DTO request untuk endpoint `POST /api/v1/auth/register` (`Username`, `FullName`, `Password`, `RePassword`, `Role`). | Digunakan oleh `AuthController.Register`. |
| `BeeMart.Api/Models/DTOs/Auth/RegisterResponse.cs` | `BeeMart.Api` | DTO response data untuk endpoint `POST /api/v1/auth/register` (`IdUser`, `Username`, `FullName`, `Role`). | Dibungkus di dalam `ApiResponse<RegisterResponse>` oleh `AuthController.Register`. |
| `BeeMart.Api/Models/DTOs/Master/PaymentMethodResponse.cs` | `BeeMart.Api` | DTO response data untuk endpoint `GET /api/v1/payment-methods` (`IdPaymentMethod` sebagai `string`, `PaymentMethodName` sebagai `string`). | Dibungkus di dalam `ApiResponse<List<PaymentMethodResponse>>` oleh `PaymentMethodsController.GetPaymentMethods`. |
| `BeeMart.Api/Models/DTOs/Master/ProductSearchItemResponse.cs` | `BeeMart.Api` | DTO response per item produk pada hasil pencarian POS (`IdProduct`, `ProductName`, `Price`, `Stock`, `ProductImage`). | Digunakan sebagai elemen list `Items` pada `ProductSearchResponse`. |
| `BeeMart.Api/Models/DTOs/Master/ProductSearchResponse.cs` | `BeeMart.Api` | DTO response ber-pagination untuk endpoint `GET /api/v1/products/search` (`Items`, `TotalItems`, `Page`, `PageSize`, `TotalPages`). | Dibungkus di dalam `ApiResponse<ProductSearchResponse>` oleh `ProductsController.SearchProducts`. |
| `BeeMart.Api/Models/DTOs/Transaction/NextInvoiceResponse.cs` | `BeeMart.Api` | DTO response data untuk endpoint `GET /api/v1/transactions/next-invoice` (`InvoiceNo`). | Dibungkus di dalam `ApiResponse<NextInvoiceResponse>` oleh `TransactionsController.GetNextInvoice`. |
| `BeeMart.Api/Models/DTOs/Transaction/TransactionDetailRequest.cs` | `BeeMart.Api` | DTO request per item dalam transaksi baru (`IdProduct`, `Quantity`). | Digunakan sebagai elemen list `Items` pada `CreateTransactionRequest`. |
| `BeeMart.Api/Models/DTOs/Transaction/CreateTransactionRequest.cs` | `BeeMart.Api` | DTO request untuk endpoint `POST /api/v1/transactions` (`IdPaymentMethod`, `CustomerMoney`, `Items`). | Digunakan oleh `TransactionsController.CreateTransaction`. |
| `BeeMart.Api/Models/DTOs/Transaction/TransactionDetailResponse.cs` | `BeeMart.Api` | DTO response per item rincian transaksi yang berhasil dibuat (`IdProduct`, `ProductName`, `Quantity`, `Price`, `Subtotal`). | Digunakan sebagai elemen list `Details` pada `CreateTransactionResponse`. |
| `BeeMart.Api/Models/DTOs/Transaction/CreateTransactionResponse.cs` | `BeeMart.Api` | DTO response data untuk endpoint `POST /api/v1/transactions` (`InvoiceNo`, `TransactionDate`, `CashierId`, `CashierName`, `PaymentMethodName`, `TotalItem`, `TotalPrice`, `CustomerMoney`, `Change`, `Details`). | Dibungkus di dalam `ApiResponse<CreateTransactionResponse>` oleh `TransactionsController.CreateTransaction`. |
| `BeeMart.Api/Models/DTOs/Transaction/TransactionHistoryItemDetailResponse.cs` | `BeeMart.Api` | DTO response per item barang dalam riwayat transaksi (`IdProduct`, `ProductName`, `Quantity`, `Price`, `Subtotal`). | Digunakan sebagai elemen list `Details` pada `TransactionHistoryItemResponse`. |
| `BeeMart.Api/Models/DTOs/Transaction/TransactionHistoryItemResponse.cs` | `BeeMart.Api` | DTO response per transaksi dalam daftar riwayat transaksi (`IdTransaction`, `InvoiceNo`, `TransactionDate`, `CashierId`, `CashierName`, `PaymentMethodName`, `TotalItem`, `TotalPrice`, `Details`). | Digunakan sebagai elemen list `Items` pada `TransactionHistoryResponse`. |
| `BeeMart.Api/Models/DTOs/Transaction/TransactionHistoryResponse.cs` | `BeeMart.Api` | DTO response ber-pagination untuk endpoint `GET /api/v1/transactions/history` (`Items`, `TotalItems`, `Page`, `PageSize`, `TotalPages`). | Dibungkus di dalam `ApiResponse<TransactionHistoryResponse>` oleh `TransactionsController.GetTransactionHistory`. |
| `BeeMart.Api/Common/ApiResponse.cs` | `BeeMart.Api` | Model pembungkus respons JSON standar (`ApiResponse<T>` dan `ApiResponse`) sesuai `PRD.md` Section 6.2 dengan properti `status` (`"success"` atau `"error"`), `message`, `data`, serta *factory methods* (`Success`, `Error`). | Digunakan oleh seluruh endpoint controller di `BeeMart.Api/Controllers/` (Phase 6–9). |
| `BeeMart.Api/Common/PasswordHasher.cs` | `BeeMart.Api` | Antarmuka `IPasswordHasher` dan implementasi `PasswordHasher` berbasis `BCrypt.Net-Next` (work factor `11`) untuk melakukan *hashing* password baru dan verifikasi password terhadap `MsUser.PasswordHash`. | Diregistrasikan sebagai Singleton di `BeeMart.Api/Program.cs`; kompatibel penuh dengan 6 akun *seed* di `Database/BeeMart.sql`. |
| `BeeMart.Api/Common/JwtTokenService.cs` | `BeeMart.Api` | Antarmuka `IJwtTokenService` dan implementasi `JwtTokenService` menggunakan algoritma HMAC-SHA256 (`HmacSha256`) untuk menerbitkan token JWT yang memuat klaim pengguna (`idUser`, `username`, `fullName`, `role`). | Membaca `JwtSettings` dari `BeeMart.Api/appsettings.json`; diregistrasikan di `BeeMart.Api/Program.cs`. |
| `BeeMart.Api/Common/ClaimsHelper.cs` | `BeeMart.Api` | Kelas *extension methods* statis pada `ClaimsPrincipal` (`GetUserId`, `GetUsername`, `GetFullName`, `GetRole`, `IsAdmin`, `IsCashier`) beserta konstanta nama klaim dan role. | Digunakan oleh `JwtTokenService` dan controller berotorisasi di `BeeMart.Api/Controllers/` (Phase 6–9). |
| `BeeMart.Api/Controllers/` | `BeeMart.Api` | Direktori untuk menyimpan kelas-kelas API Controller ber-versioning `/api/v1/...`. | Dipetakan oleh `app.MapControllers()` di `BeeMart.Api/Program.cs`. |
| `BeeMart.Api/Controllers/.gitkeep` | `BeeMart.Api` | Penanda agar direktori `Controllers/` tetap ada setelah penghapusan *boilerplate* `WeatherForecastController.cs`. | Dipertahankan di dalam `BeeMart.Api/Controllers/`. |
| `BeeMart.Api/Controllers/AuthController.cs` | `BeeMart.Api` | Controller autentikasi publik (`[Route("api/v1/auth")]`, `[AllowAnonymous]`) yang menyediakan endpoint `POST /api/v1/auth/register` (`API-AUTH-02`, validasi input, password match, role, keunikan username *case-insensitive*, generasi `IdUser` 5 digit `D5`, hashing BCrypt) dan `POST /api/v1/auth/login` (`API-AUTH-01`, verifikasi user aktif, role, BCrypt hash, dan penerbitan JWT token). | Menginjeksi `BeeMartDbContext`, `IPasswordHasher`, dan `IJwtTokenService`, serta menggunakan DTO di `BeeMart.Api/Models/DTOs/Auth/` dan pembungkus `ApiResponse<T>`. |
| `BeeMart.Api/Controllers/PaymentMethodsController.cs` | `BeeMart.Api` | Controller master metode pembayaran (`[Route("api/v1/payment-methods")]`, `[Authorize]`) yang menyediakan endpoint `GET /api/v1/payment-methods` (`API-PAY-01`) untuk mengambil metode pembayaran aktif (`Stsrc == true`) diurutkan berdasarkan `IdPaymentMethod` ascending. | Menginjeksi `BeeMartDbContext` dan mengembalikan `ApiResponse<List<PaymentMethodResponse>>`. |
| `BeeMart.Api/Controllers/ProductsController.cs` | `BeeMart.Api` | Controller master produk (`[Route("api/v1/products")]`, `[Authorize]`) yang menyediakan endpoint `GET /api/v1/products/search` (`API-PROD-01`) dengan filter produk aktif (`Stsrc == true`), stok tersedia (`Stock > 0`), pencarian nama *case-insensitive* (`search`), pengurutan `Name` ascending, dan *pagination* (`page`, `pageSize` maks 50). | Menginjeksi `BeeMartDbContext` dan mengembalikan `ApiResponse<ProductSearchResponse>`. |
| `BeeMart.Api/Controllers/TransactionsController.cs` | `BeeMart.Api` | Controller transaksi (`[Route("api/v1/transactions")]`, `[Authorize]`) yang menyediakan endpoint `GET /api/v1/transactions/next-invoice` (`API-TRX-01`, generasi nomor invoice harian berformat `INV-YYYYMMDD-XXXX`), `POST /api/v1/transactions` (`API-TRX-02`, validasi keranjang, metode pembayaran, stok barang, kalkulasi total & kembalian, pengurangan stok atomik dalam `IDbContextTransaction`, serta penyimpanan ke `TrTransaction` dan `TrTransactionDetail`), dan `GET /api/v1/transactions/history` (`API-HIST-01`, riwayat transaksi ber-pagination dengan isolasi data berbasis role `Cashier` vs `Admin` sesuai `NFR-SEC-002`, filter `startDate`/`endDate`/`cashierId`, serta *eager loading* `.Include()`/`.ThenInclude()` untuk `User`, `PaymentMethod`, `TransactionDetails`, dan `Product`). | Menginjeksi `BeeMartDbContext`, menggunakan `ClaimsHelper`, serta DTO di `BeeMart.Api/Models/DTOs/Transaction/`. |

### 3.3 WebClient Project (`BeeMart.Web/`)
| Path | Project | Purpose (Fungsi) | Connections & Dependencies |
| :--- | :--- | :--- | :--- |
| `BeeMart.Web/` | `BeeMart.Web` | Folder utama proyek Frontend/UI WebClient (`DELIV-001`, `NFR-ARCH-001`, `NFR-ARCH-002`). | Terdaftar di `BeeMart.sln`; hanya berkomunikasi dengan `BeeMart.Api` melalui HTTP Request (tanpa akses database langsung). |
| `BeeMart.Web/BeeMart.Web.csproj` | `BeeMart.Web` | File definisi proyek C# (`Microsoft.NET.Sdk.Web`) dengan target `net8.0` tanpa paket EF Core maupun driver database. | Hanya bergantung pada SDK Web standar `.NET 8`. |
| `BeeMart.Web/Program.cs` | `BeeMart.Web` | Titik masuk (*entry point*) aplikasi WebClient Razor Pages, registrasi `AddRazorPages`, `AddHttpContextAccessor`, Typed HTTP Client `AddHttpClient<IApiClient, ApiClient>` dengan `ApiSettings:BaseUrl`, Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`, `LoginPath = "/Auth/Login"`, `AccessDeniedPath = "/Auth/AccessDenied"`), serta middleware (`UseStaticFiles`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `MapRazorPages`). | Membaca konfigurasi `ApiSettings:BaseUrl` dari `BeeMart.Web/appsettings.json`, mengelola sesi Cookie Authentication, dan menyajikan halaman dari `Pages/` serta aset dari `wwwroot/`. |
| `BeeMart.Web/appsettings.json` | `BeeMart.Web` | File konfigurasi utama WebClient yang menyimpan `ApiSettings:BaseUrl` (`http://localhost:5001/api/v1/`). Tidak memiliki connection string (`NFR-ARCH-002`). | Menunjuk langsung ke alamat endpoint `BeeMart.Api` (`http://localhost:5001/api/v1/`). |
| `BeeMart.Web/appsettings.Development.json` | `BeeMart.Web` | Konfigurasi logging untuk environment `Development` pada WebClient (tanpa connection string). | Meng-override `BeeMart.Web/appsettings.json` saat `ASPNETCORE_ENVIRONMENT=Development`. |
| `BeeMart.Web/Properties/launchSettings.json` | `BeeMart.Web` | Pengaturan profil eksekusi lokal dan port tetap WebClient (`https://localhost:7002`, `http://localhost:5002`). | Menggunakan port berbeda dari `BeeMart.Api` agar kedua proyek dapat berjalan bersamaan. |
| `BeeMart.Web/Models/Auth/LoginViewModel.cs` | `BeeMart.Web` | ViewModel form login (`LoginViewModel`: `Username`, `Password`, `Role`) beserta DTO request/response API (`LoginRequest`, `LoginResponse`). | Digunakan oleh `Pages/Auth/Login.cshtml` dan `Login.cshtml.cs` untuk memanggil `POST /api/v1/auth/login`. |
| `BeeMart.Web/Models/Auth/RegisterViewModel.cs` | `BeeMart.Web` | ViewModel form registrasi (`RegisterViewModel`: `Username`, `FullName`, `Password`, `RePassword`, `Role`) beserta DTO request/response API (`RegisterRequest`, `RegisterResponse`). | Digunakan oleh `Pages/Auth/Register.cshtml` dan `Register.cshtml.cs` untuk memanggil `POST /api/v1/auth/register`. |
| `BeeMart.Web/Models/Transaction/CartItemViewModel.cs` | `BeeMart.Web` | ViewModel item keranjang belanja POS (`CartItemViewModel`: `IdProduct`, `ProductName`, `Price`, `Quantity`, `MaxStock`, dan kalkulasi `Subtotal => Price * Quantity`). | Digunakan pada state keranjang belanja di `Pages/Transaction/New.cshtml(.cs)`. |
| `BeeMart.Web/Models/Transaction/CheckoutRequestViewModel.cs` | `BeeMart.Web` | ViewModel request checkout POS (`CheckoutRequestViewModel`: `IdPaymentMethod`, `CustomerMoney`, `Items` bertipe `List<CheckoutItemRequestViewModel>`). | Digunakan oleh `NewModel.OnPostCheckoutAsync` untuk memanggil `POST /api/v1/transactions`. |
| `BeeMart.Web/Models/Transaction/TransactionApiModels.cs` | `BeeMart.Web` | Kumpulan DTO response untuk modul transaksi POS (`NextInvoiceResponse`, `PaymentMethodResponse`, `ProductSearchItemResponse`, `ProductSearchResponse`, `TransactionDetailResponse`, `CreateTransactionResponse`). | Digunakan oleh `NewModel` (`Pages/Transaction/New.cshtml.cs`) saat memanggil endpoint `transactions/next-invoice`, `payment-methods`, `products/search`, dan `transactions`. |
| `BeeMart.Web/Models/Transaction/TransactionHistoryViewModel.cs` | `BeeMart.Web` | Kumpulan ViewModel dan DTO response untuk modul riwayat transaksi (`TransactionHistoryViewModel`, `TransactionHistoryResponse`, `TransactionHistoryItemViewModel`, `TransactionHistoryItemDetailViewModel`, `CashierFilterOptionViewModel`). | Digunakan oleh `HistoryModel` (`Pages/Transaction/History.cshtml.cs`) dan tampilan `History.cshtml` saat memanggil `GET /api/v1/transactions/history`. |
| `BeeMart.Web/Services/IApiClient.cs` | `BeeMart.Web` | Kontrak antarmuka `IApiClient` (`GetAsync<T>`, `PostAsync<TRequest, TResponse>`, `PostAsync<TResponse>`) beserta kelas pembungkus respons standar `ApiResponse<T>` (`Status`, `Message`, `Data`, `StatusCode`, `IsSuccess`). | Diimplementasikan oleh `ApiClient` dan diregistrasikan melalui `AddHttpClient<IApiClient, ApiClient>` di `BeeMart.Web/Program.cs`. |
| `BeeMart.Web/Services/ApiClient.cs` | `BeeMart.Web` | Implementasi `IApiClient` berbasis `HttpClient` (`IHttpClientFactory`) dan `IHttpContextAccessor` untuk mengirim request HTTP `GET` dan `POST` ke `BeeMart.Api`, menyisipkan header `Authorization: Bearer <token>` secara otomatis dari klaim Cookie Authentication pengguna (`jwt_token` / `Token` / `access_token`), serta men-deserialisasi `ApiResponse<T>`. | Menginjeksi `HttpClient`, `IHttpContextAccessor`, dan `ILogger<ApiClient>`. |
| `BeeMart.Web/Pages/` | `BeeMart.Web` | Direktori halaman Razor Pages (`.cshtml` dan `.cshtml.cs`). | Dirender oleh engine Razor Pages yang dikonfigurasi di `BeeMart.Web/Program.cs`. |
| `BeeMart.Web/Pages/_ViewImports.cshtml` | `BeeMart.Web` | Deklarasi namespace (`BeeMart.Web`, `BeeMart.Web.Models.Auth`, `BeeMart.Web.Models.Transaction`) dan Tag Helpers (`Microsoft.AspNetCore.Mvc.TagHelpers`) yang berlaku bagi seluruh Razor Pages. | Digunakan oleh seluruh file `.cshtml` di dalam `BeeMart.Web/Pages/`. |
| `BeeMart.Web/Pages/_ViewStart.cshtml` | `BeeMart.Web` | Menetapkan layout default (`_Layout`) untuk seluruh halaman Razor. | Menghubungkan halaman-halaman di `Pages/` dengan `Pages/Shared/_Layout.cshtml`. |
| `BeeMart.Web/Pages/Shared/` | `BeeMart.Web` | Direktori komponen tampilan dan layout bersama (*shared views*). | Diakses oleh seluruh halaman Razor di `BeeMart.Web/Pages/`. |
| `BeeMart.Web/Pages/Shared/_Layout.cshtml` | `BeeMart.Web` | Kerangka HTML semantik utama bebas Bootstrap (`FE-STYLE-001`) yang memuat header brand `BeeMart`, status sesi pengguna (`#realtime-clock` / `Role` & `Login`, nama lengkap, badge `Admin`/`Kasir`), tombol `Login`/`Register`/`Logout`, navigasi tab (`Transaksi Baru (New Transaction)` untuk `Cashier` & `Riwayat Transaksi (History Transaction)` untuk `Admin`/`Cashier`), perenderan pesan alert `TempData`, dan `<main class="container">@RenderBody()</main>`. | Hanya memuat `/css/site.css` dan `/js/site.js` lokal dari `wwwroot/`. |
| `BeeMart.Web/Pages/Auth/Login.cshtml` & `Login.cshtml.cs` | `BeeMart.Web` | Halaman autentikasi `BeeMart Login` (`FEAT-AUTH-01`) dengan input `Username`, `Password`, dan dropdown `Role` (`Kasir`/`Cashier`, `Admin`/`Admin`), menampilkan pesan error API (mis. `"Invalid login"`), menerbitkan Cookie Authentication (`BeeMart.Auth`) beserta klaim user dan `jwt_token`, serta mengarahkan pengguna setelah login. | Menginjeksi `IApiClient` dan memanggil `POST /api/v1/auth/login`. |
| `BeeMart.Web/Pages/Auth/Register.cshtml` & `Register.cshtml.cs` | `BeeMart.Web` | Halaman registrasi `Register Akun` (`FEAT-AUTH-02`) dengan input `Username`, `Full Name`, `Password`, `Re-Password`, dan dropdown `Role`, memvalidasi kecocokan password, menampilkan pesan error API secara persis (mis. `"username has already exists"`), dan mengarahkan ke `/Auth/Login` dengan pesan sukses `TempData["SuccessMessage"]`. | Menginjeksi `IApiClient` dan memanggil `POST /api/v1/auth/register`. |
| `BeeMart.Web/Pages/Auth/Logout.cshtml` & `Logout.cshtml.cs` | `BeeMart.Web` | Handler logout (`FEAT-AUTH-03`) yang menghapus sesi Cookie Authentication melalui `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)` dan mengarahkan pengguna ke `/Auth/Login`. | Dipanggil oleh form tombol `Logout` pada `_Layout.cshtml`. |
| `BeeMart.Web/Pages/Auth/AccessDenied.cshtml` & `AccessDenied.cshtml.cs` | `BeeMart.Web` | Halaman penolakan akses (`403 Forbidden`) menggunakan CSS kustom ketika pengguna tidak memiliki izin role yang memadai. | Terdaftar pada `options.AccessDeniedPath = "/Auth/AccessDenied"` di `BeeMart.Web/Program.cs`. |
| `BeeMart.Web/Pages/Transaction/New.cshtml` & `New.cshtml.cs` | `BeeMart.Web` | Halaman kasir Point of Sale (`FEAT-TRX-01`, `[Authorize(Roles = "Cashier")]`) yang menginisialisasi `Invoice No` otomatis (`GET /api/v1/transactions/next-invoice`), metode pembayaran dinamis (`GET /api/v1/payment-methods`), dan katalog produk (`GET /api/v1/products/search`), menyediakan handler AJAX (`?handler=SearchProducts`, `?handler=NextInvoice`, `?handler=Checkout`), mengelola keranjang belanja dengan validasi stok maksimum, menghitung `Total Item`, `Total Belanja`, dan `Kembalian` secara *real-time*, serta menampilkan modal struk transaksi berhasil dengan tombol `Transaksi Baru`. | Menginjeksi `IApiClient` dan menggunakan model di `BeeMart.Web/Models/Transaction/`. |
| `BeeMart.Web/Pages/Transaction/History.cshtml` & `History.cshtml.cs` | `BeeMart.Web` | Halaman riwayat transaksi berbasis peran (`FEAT-HIST-01` & `FEAT-HIST-02`, `[Authorize]`) untuk role `Cashier` (`Pencarian Data Transaksi (Hanya Kasir: <username>)`, tanpa dropdown kasir) dan `Admin` (`Pencarian Data Transaksi (Menampilkan Semua Kasir)`, dengan dropdown filter `Kasir`), mendukung filter `startDate`, `endDate`, `cashierId` (khusus `Admin`), `page`, dan `pageSize` via `GET /api/v1/transactions/history`, tabel transaksi responsif dengan sub-tabel rincian item yang dapat dibuka/ditutup (`Detail` / `Tutup`), serta kontrol pagination. | Menginjeksi `IApiClient` dan menggunakan `TransactionHistoryViewModel`. |
| `BeeMart.Web/Pages/Index.cshtml` & `Index.cshtml.cs` | `BeeMart.Web` | Halaman beranda awal (`/`) yang menggunakan komponen kartu dan tombol CSS kustom `site.css`. | Menggunakan `_Layout.cshtml` dan mengarahkan pengguna ke alur autentikasi atau transaksi POS. |
| `BeeMart.Web/Pages/Error.cshtml` & `Error.cshtml.cs` | `BeeMart.Web` | Halaman penanganan *exception* standar bebas class Bootstrap. | Dipanggil oleh middleware `app.UseExceptionHandler("/Error")` di `BeeMart.Web/Program.cs`. |
| `BeeMart.Web/wwwroot/` | `BeeMart.Web` | Direktori root file statis yang disajikan ke browser oleh `app.UseStaticFiles()`. | Diakses oleh `_Layout.cshtml` dan halaman Razor lainnya. |
| `BeeMart.Web/wwwroot/favicon.ico` | `BeeMart.Web` | Ikon tab browser untuk aplikasi WebClient. | Disajikan secara statis dari `wwwroot/`. |
| `BeeMart.Web/wwwroot/css/` | `BeeMart.Web` | Direktori file stylesheet CSS kustom. | Direferensikan oleh `Pages/Shared/_Layout.cshtml`. |
| `BeeMart.Web/wwwroot/css/site.css` | `BeeMart.Web` | Framework CSS kustom murni (`FE-STYLE-001`, tanpa Bootstrap) yang mendefinisikan CSS Custom Properties (`:root` palet amber `#F59E0B`, dark slate `#1E293B`, POS blue `#1E88E5`), reset box-sizing, tipografi, form controls, tombol, kartu, tabel responsif, badge, alert, modal dialog, pagination, komponen grid/kontrol POS (`.pos-layout`, `.pos-meta-bar`, `.qty-control`, `.pos-summary-box`, `.receipt-meta-grid`), serta komponen filter dan rincian riwayat transaksi (`.history-filter-bar`, `.history-detail-row`, `.history-subtable`). | Direferensikan oleh `Pages/Shared/_Layout.cshtml`. |
| `BeeMart.Web/wwwroot/js/` | `BeeMart.Web` | Direktori script JavaScript sisi klien. | Direferensikan oleh `Pages/Shared/_Layout.cshtml`. |
| `BeeMart.Web/wwwroot/js/site.js` | `BeeMart.Web` | Utilitas JavaScript murni sisi klien (`window.BeeMart`) untuk jam *real-time* (`<M/d/yyyy, HH.mm.ss>`), pemformat mata uang Rupiah (`Rp xx.xxx`), dan penutupan kotak alert (`[data-dismiss='alert']`). | Direferensikan oleh `Pages/Shared/_Layout.cshtml`. |

---

## 4. Phase Delivery Status Summary (Phase 1 – Phase 15)

| Phase Range | Target Scope | Status | Summary |
| :--- | :--- | :---: | :--- |
| **Phase 1–3** | Solution Scaffolding & SQL Server Database (`Database/BeeMart.sql`) | **Active (Completed)** | `BeeMart.sln`, `BeeMart.Api`, `BeeMart.Web`, 5-table DDL schema, indexes, constraints, and idempotent DML seed data (3 payment methods, 6 BCrypt-hashed users, 22 products). |
| **Phase 4–9** | Backend RESTful API (`BeeMart.Api`) | **Active (Completed)** | EF Core 8 `BeeMartDbContext`, `ApiResponse<T>`, `PasswordHasher` (BCrypt), `JwtTokenService`, `ClaimsHelper`, and `/api/v1` endpoints for Auth, Payment Methods, Product Search, POS Transactions, and Role-Isolated History. |
| **Phase 10–13** | WebClient Razor Pages UI (`BeeMart.Web`) | **Active (Completed)** | Zero-DB/Zero-Bootstrap Razor Pages client using `IApiClient` (`HttpClient`), Cookie Auth + JWT Bearer forwarding, custom CSS framework (`wwwroot/css/site.css`), Login/Register/Logout/AccessDenied, POS New Transaction, and Role-Aware Transaction History. |
| **Phase 14–15** | Final QA, Architecture Compliance Audit & Delivery Sign-Off | **Active (Completed)** | Full solution build (`0 Warning(s)`, `0 Error(s)`), Architecture & Styling compliance audit (`NFR-ARCH-001/002`, `FE-STYLE-001`, `NFR-SEC-001/002`), automated E2E regression verification, and living documentation sign-off (`v1.0.0`). |
