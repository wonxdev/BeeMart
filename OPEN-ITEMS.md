# Open Items & Project Roadmap — BeeMart POS Web App

Daftar *to-do* dan status pengerjaan proyek **BeeMart POS Web App** berdasarkan rencana 15 fase, beserta *checklist* pemeliharaan repositori dan catatan keputusan teknis/asumsi.

---

## 1. Phase Checklist

### Phase 1: Solution & Project Scaffolding
- [x] **Task 1**: Create the solution file `BeeMart.sln` at the root (`BeeMart/`).
- [x] **Task 2**: Create the Backend API project `BeeMart.Api` (ASP.NET Core Web API, `net8.0`, Controller-based) and remove `WeatherForecast` boilerplate.
- [x] **Task 3**: Create the WebClient project `BeeMart.Web` (ASP.NET Core Razor Pages, `net8.0`), remove all Bootstrap files/links/references, and leave `wwwroot/css/site.css` as an empty custom CSS file.
- [x] **Task 4**: Add both `BeeMart.Api` and `BeeMart.Web` to `BeeMart.sln`.
- [x] **Task 5**: Add NuGet packages `Microsoft.EntityFrameworkCore.SqlServer` (8.x), `Microsoft.EntityFrameworkCore.Design` (8.x), and `Microsoft.EntityFrameworkCore.Tools` (8.x) to `BeeMart.Api` only.
- [x] **Task 6**: Add SQL Server connection string `DefaultConnection` to `BeeMart.Api/appsettings.json`.
- [x] **Task 7**: Add `ApiSettings:BaseUrl` (`https://localhost:7001/api/v1/`) to `BeeMart.Web/appsettings.json` and configure distinct, consistent fixed ports in both `launchSettings.json` files (`7001`/`5001` for API, `7002`/`5002` for Web).
- [x] Create empty `Database/` folder with `.gitkeep` and root documentation files (`STRUCTURE.md`, `CHANGELOG.md`, `OPEN-ITEMS.md`), and verify `dotnet build` succeeds with 0 errors.

### Phase 2: Database Schema (DDL)
- [x] Define DDL for the 5 tables (`MsUser`, `LtPaymentMethod`, `MsProduct`, `TrTransaction`, `TrTransactionDetail`) matching the ERD types, PK/FK constraints, and audit trail columns (`Stsrc`, `DateIn`, `UserIn`, `Dateup`, `Userup`).
- [x] Add the 4 required indexes (`UQ_MsUser_Username`, `UQ_TrTransaction_InvoiceNo`, `IX_TrTransaction_TransactionDate_IdUser`, `IX_MsProduct_Name`) and default constraints.

### Phase 3: Seed Data (DML)
- [x] Seed `LtPaymentMethod` (`Cash`, `Debit`, `Credit`).
- [x] Seed `MsUser` accounts (`admin`, `steven`, `gabri`, `mona`, `felix`, `gobuta`) with hashed passwords (BCrypt work factor 11) and 5-digit `IdUser` values matching the PDF examples.
- [x] Seed `MsProduct` items (`Apel Fuji`, `Apel Merah`, `Apel Hijau`, `Susu UHT Full Cream`, plus 18 additional products—totaling 12 `Apel` products, 4 `Susu` products, and 6 general grocery products—to test the max-10 search limit; `TrTransaction` and `TrTransactionDetail` are intentionally not seeded as they are created by the app).
- [x] Combine all DDL and DML into a single executable, re-runnable SQL script (`Database/BeeMart.sql`) and verify execution twice against `localhost\SQLEXPRESS`.

### Phase 4: EF Core Entities & DbContext
- [x] Create entity classes for `MsUser`, `LtPaymentMethod`, `MsProduct`, `TrTransaction`, and `TrTransactionDetail` in `BeeMart.Api`.
- [x] Create `BeeMartDbContext` with Fluent API configurations matching the SQL schema and register it in `BeeMart.Api/Program.cs` using `DefaultConnection` (and update `DefaultConnection` in `BeeMart.Api/appsettings.json` to `localhost\SQLEXPRESS`).

### Phase 5: Backend Foundations
- [x] Implement standard JSON response wrapper (`status`, `message`, `data`) and `/api/v1` routing convention.
- [x] Add `BCrypt.Net-Next` to `BeeMart.Api` and implement password hasher & verifier (`PasswordHash`).
- [x] Implement JWT token generation, authentication/authorization middleware, and claims helper (`IdUser`, `Username`, `FullName`, `Role`).

### Phase 6: Backend Auth Endpoints
- [x] Implement `POST /api/v1/auth/register` (validate required fields, password match, role, case-insensitive unique username with `"username has already exists"`, generate 5-digit `IdUser`, hash password).
- [x] Implement `POST /api/v1/auth/login` (validate credentials & role against active `MsUser` records, return `"Invalid login"` on mismatch, issue JWT token on success).

### Phase 7: Backend Master Data Endpoints (Next Open Phase)
- [ ] Implement `GET /api/v1/payment-methods` (fetch active payment methods from `LtPaymentMethod`).
- [ ] Implement `GET /api/v1/products/search?keyword=...` (search active products in `MsProduct` by name, limited to max 10 items).

### Phase 8: Backend Transaction Endpoints
- [ ] Implement `GET /api/v1/transactions/next-invoice` (generate read-only `InvoiceNo` with format `[5 digit userID][ddmmyy][3 digit running number]`).
- [ ] Implement `POST /api/v1/transactions` (atomic DB transaction: validate items & stock with error `"Jumlah barang yang dimasukkan melebihi sisa stok."`, calculate `SubTotal` and `TotalPrice`, deduct `MsProduct.Stock`, insert `TrTransaction` & `TrTransactionDetail`, return `nextInvoiceNo`).

### Phase 9: Backend History Endpoint
- [ ] Implement `GET /api/v1/transactions/history?date={yyyy-MM-dd}&page={page}&pageSize=20` with mandatory date validation, `ORDER BY TransactionDate ASC`, pagination (max 20 per page), and role-based filtering (`Cashier` sees only own transactions; `Admin` sees all cashiers).

### Phase 10: WebClient Foundations
- [ ] Build pure custom CSS (`wwwroot/css/site.css`) matching the PDF mockups without Bootstrap.
- [ ] Implement typed `HttpClient` wrapper pointing to `ApiSettings:BaseUrl`, session/token storage, authentication/role route guards, and Rupiah (`Rp xx.xxx`) & local time formatters.

### Phase 11: WebClient Auth Pages
- [ ] Implement `Login` page (`/Auth/Login`) with `Username`, `Password`, `Role` dropdown (`Admin`/`Admin`, `Kasir`/`Cashier`), error popup/label, and link to Register.
- [ ] Implement `Register` page (`/Auth/Register`) with `Username`, `Full Name`, `Password`, `Re-Password`, `Role` dropdown, error handling, and link to Login.

### Phase 12: WebClient Shared Layout
- [ ] Implement top navbar (`BeeMart` brand, context-aware right header showing `<M/d/yyyy, HH.mm.ss> | Kasir: <username>` on New Transaction or `Role: <Role> | Login: <username>` on History Transaction, red `Logout` button) and `New Transaction` / `History Transaction` navigation tabs.

### Phase 13: WebClient New Transaction Page
- [ ] Implement `/Transaction/New` page: auto read-only `Invoice No`, product search on `Enter` (max 10 rows), `Pilih` dialog (`Barang Terpilih`, unit price, remaining stock, `Qty` input with stock validation), `Daftar Belanjaan` cart table with `Hapus`, `Total` calculation, dynamic `Metode Pembayaran` dropdown from API, and `Submit Transaksi` with automatic page reset and new invoice number.

### Phase 14: WebClient History Page
- [ ] Implement `/Transaction/History` page supporting both `Cashier` (`Pencarian Data Transaksi (Hanya Kasir: <username>)`) and `Admin` (`Pencarian Data Transaksi (Menampilkan Semua Kasir)`) views, mandatory date picker (`Pilih Tanggal`), `Cari Data` button, ascending transaction table, and pagination controls (max 20 rows per page).

### Phase 15: Final Checks & Delivery
- [ ] Verify end-to-end flows against all PRD acceptance criteria, confirm zero direct DB access in `BeeMart.Web`, ensure clean build (`dotnet build`), and finalize documentation (`STRUCTURE.md`, `CHANGELOG.md`, `OPEN-ITEMS.md`).

---

## 2. Repository Housekeeping Checklist (Non-Phase)

- [x] Create standard Visual Studio / .NET `.gitignore` at the repository root (ignoring `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `*.userprefs`, `*.log`, `TestResults/`, `.idea/`, `.vscode/*`, `*.mdf`, `*.ldf`, `scratch/`, while keeping `launchSettings.json` and `appsettings.Development.json` tracked).
- [x] Create practical `README.md` at the repository root (project overview, tech stack, layout, prerequisites, setup steps, seeded accounts table with local-dev password note, and phase status).
- [ ] **Manual Git Action Required by User**: Untrack previously committed `bin/` and `obj/` build artifacts (`BeeMart.Api/bin/`, `BeeMart.Api/obj/`, `BeeMart.Web/bin/`, `BeeMart.Web/obj/` — 188 files total) by running:
  ```powershell
  git rm -r --cached BeeMart.Api/bin BeeMart.Api/obj BeeMart.Web/bin BeeMart.Web/obj
  ```

---

## 3. Notes / Decisions / Blockers (Assumptions Recorded in Phases 1–6)

1. **WebClient Project Template Choice (`ASP.NET Core Razor Pages`)**:
   - Sesuai `NFR-TECH-002` dan instruksi Phase 1 Task 3 (*"ASP.NET Core Razor Pages or MVC, net8.0"*), proyek `BeeMart.Web` dibuat menggunakan template **ASP.NET Core Razor Pages** karena sangat selaras dengan pembagian fase berbasis halaman (*Phase 11: Auth Pages*, *Phase 12: Shared Layout*, *Phase 13: New Transaction Page*, *Phase 14: History Page*).
2. **Penghapusan Penuh Bootstrap & Library Bawaan Template di `BeeMart.Web`**:
   - Seluruh isi folder `wwwroot/lib/` (termasuk Bootstrap dan jQuery bawaan template), file `_Layout.cshtml.css`, `_ValidationScriptsPartial.cshtml`, dan halaman `Privacy.cshtml` telah dihapus agar `BeeMart.Web` 100% bebas dari Bootstrap (`FE-STYLE-001`) serta bebas dari *boilerplate* yang tidak diperlukan. File `wwwroot/css/site.css` dikosongkan sepenuhnya untuk diisi CSS kustom murni pada Phase 10.
3. **Konfigurasi Port Tetap (`launchSettings.json` & `appsettings.json`)**:
   - `BeeMart.Api` dikonfigurasi berjalan pada `https://localhost:7001` (dan `http://localhost:5001`).
   - `BeeMart.Web` dikonfigurasi berjalan pada `https://localhost:7002` (dan `http://localhost:5002`) serta memanggil Backend API melalui `ApiSettings:BaseUrl = "https://localhost:7001/api/v1/"`.
4. **Kompatibilitas SDK & Target Framework (`net8.0`)**:
   - Mesin pengembangan menggunakan .NET SDK `10.0.401` dengan runtime `.NET 8` (`8.0.28`) terinstal. Kedua file `.csproj` (`BeeMart.Api` dan `BeeMart.Web`) dikunci secara eksplisit ke `<TargetFramework>net8.0</TargetFramework>`, file solusi dibuat dalam format klasik `.sln` (`BeeMart.sln`), dan paket NuGet EF Core dikunci pada versi `8.0.32`.
5. **Asumsi Bisnis PRD (Section 12) yang Dicatat untuk Fase Berikutnya**:
   - **Poin 3.a–3.c pada PDF Hal. 7**: Merupakan duplikasi teks validasi *Register* dari Hal. 6; pesan duplikasi username akan didukung dengan jelas pada endpoint Register (Phase 6).
   - **Generasi `IdUser` (`NVARCHAR(5)`)**: User baru yang mendaftar melalui Register akan diberikan 5 digit angka unik (`IdUser`) agar konsisten dengan format 14 karakter `InvoiceNo` (`[5 digit userID][ddmmyy][3 digit running number]`).
   - **Reset `Running Number` 3 Digit pada `InvoiceNo`**: Nomor urut 3 digit terakhir di-reset kembali ke `001` setiap pergantian tanggal (`ddmmyy`).
   - **Akumulasi Item yang Sama di Keranjang & Pengurangan Stok**: Memilih barang yang sudah ada di `Daftar Belanjaan` akan mengakumulasi `Qty` dengan tetap memvalidasi total `Qty <= Stock`, dan pengurangan `MsProduct.Stock` di database dilakukan secara atomik saat `Submit Transaksi`.
6. **Phase 2 DDL Decisions & Verification against Local SQL Server**:
   - **UUID String Default (`CONVERT(NVARCHAR(36), NEWID())`)**: Untuk kolom `PK` bertipe `NVARCHAR(36)` (`MsProduct.IdProduct`, `TrTransaction.IdTransaction`, `TrTransactionDetail.IdTransactionDetail`), ekspresi `DEFAULT (CONVERT(NVARCHAR(36), NEWID()))` digunakan agar eksplisit bertipe string `NVARCHAR(36)` di T-SQL.
   - **No Foreign Key `ON DELETE CASCADE`**: Karena seluruh tabel menerapkan *soft-delete* melalui kolom `Stsrc BIT NOT NULL DEFAULT (1)`, *foreign key* dibuat dengan perilaku standar `NO ACTION` (tanpa penghapusan fisik berantai).
   - **Explicit Constraint & Index Naming**: Seluruh *Primary Key*, *Foreign Key*, *Unique*, *Check* (`CK_MsUser_Role`, `CK_LtPaymentMethod_MethodName`), dan *Default* (`DF_<Table>_<Column>`) diberi nama secara eksplisit, serta keempat indeks dari `PRD.md` Section 5.4 (`UQ_MsUser_Username`, `UQ_TrTransaction_InvoiceNo`, `IX_TrTransaction_TransactionDate_IdUser`, `IX_MsProduct_Name`) dilindungi dengan pengecekan `sys.indexes` agar *idempotent*.
   - **Kept `Database/.gitkeep`**: File `Database/.gitkeep` tetap dipertahankan berdampingan dengan `Database/BeeMart.sql` dan telah dicatat pada `CHANGELOG.md` serta `STRUCTURE.md`.
   - **Live Execution & Idempotency Verification**: Script `Database/BeeMart.sql` telah **dieksekusi secara langsung sebanyak dua kali berturut-turut** menggunakan `sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i "Database\BeeMart.sql"` dengan hasil `exit code: 0` tanpa error pada kedua eksekusi.
7. **Phase 3 Seed Data (DML) & Password Hashing Decisions**:
   - **Algoritma Password Hashing (`BCrypt`) & Shared Development Password**: Dipilih algoritma **BCrypt** (work factor `11`, menghasilkan hash sepanjang 60 karakter berawalan `$2a$11$`). Keenam user awal (`admin`, `steven`, `gabri`, `mona`, `felix`, `gobuta`) menggunakan satu *shared development password* khusus pengembangan lokal yaitu `Password123!` (sesuai contoh `PRD.md` Section 6.2), di mana setiap user memiliki *salt* dan nilai `PasswordHash` unik masing-masing. Password teks murni tidak pernah ditulis di dalam `Database/BeeMart.sql`.
   - **Penambahan Paket `BCrypt.Net-Next` pada Phase 5**: Paket NuGet `BCrypt.Net-Next` **harus ditambahkan ke `BeeMart.Api` pada Phase 5** saat mengimplementasikan `PasswordHasher.cs` untuk verifikasi login (`BCrypt.Verify`) dan registrasi user baru (`BCrypt.HashPassword`).
   - **Penyesuaian Connection String ke `localhost\SQLEXPRESS` pada Phase 4**: Instance SQL Server lokal yang aktif pada mesin ini adalah `localhost\SQLEXPRESS`. Pada **Phase 4**, nilai `ConnectionStrings:DefaultConnection` di `BeeMart.Api/appsettings.json` perlu diubah dari `Server=localhost;...` menjadi `Server=localhost\\SQLEXPRESS;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;`.
   - **Fixed Product GUIDs & Pencarian > 10 Produk**: Seluruh 22 baris produk pada `MsProduct` menggunakan GUID statis (`a1b2c3d4-0001-4000-8000-000000000001` s/d `...0022`) agar konsisten dengan contoh JSON di `PRD.md` Section 6.2, mencakup 12 produk `Apel` (untuk menguji batas pencarian maksimal 10 data), 4 produk `Susu`, dan 6 produk kebutuhan pokok lainnya. Tabel `TrTransaction` dan `TrTransactionDetail` sengaja tidak di-*seed* karena transaksi dibuat melalui aplikasi.
8. **Phase 4 EF Core Entities, Fluent API Mapping & Connection String Decisions**:
   - **Exact Alignment with `Database/BeeMart.sql` & `PRD.md` Section 5.1**: Seluruh 5 entitas (`MsUser`, `LtPaymentMethod`, `MsProduct`, `TrTransaction`, `TrTransactionDetail`) dan `BeeMartDbContext` dikonfigurasi menggunakan Fluent API agar selaras 100% dengan DDL aktual di `Database/BeeMart.sql`:
     - Panjang kolom string disesuaikan dengan DDL SQL Server: `IdUser` (`HasMaxLength(5)`), GUID PK/FK (`IdProduct`, `IdTransaction`, `IdTransactionDetail` dengan `HasMaxLength(36)`), `MsUser.FullName` (`HasMaxLength(300)`), `TrTransaction.InvoiceNo` (`HasMaxLength(20)`), serta kolom audit `UserIn` dan `Userup` (`string?`, `HasMaxLength(36)`).
     - Properti `TotalItem` (`int`) tetap disediakan pada kelas C# `TrTransaction` namun ditandai `[NotMapped]` serta `.Ignore(e => e.TotalItem)` pada `BeeMartDbContext` karena tabel fisik `dbo.TrTransaction` di `Database/BeeMart.sql` (dan ERD `PRD.md` Section 5.1/5.3) tidak memiliki kolom `TotalItem` (jumlah item transaksi dihitung dari agregasi `SUM(TrTransactionDetail.Qty)` sesuai `DB-HIST-K01` dan `BR-CALC-003`). Hal ini memastikan query EF Core ke `dbo.TrTransaction` berjalan tanpa error `Invalid column name 'TotalItem'`.
   - **Explicit Decimal Precision**: Seluruh properti `decimal` dikonfigurasi secara eksplisit di `OnModelCreating` untuk mencegah *truncation warning* EF Core: `HasPrecision(10, 2)` untuk `MsProduct.Weight`, `HasPrecision(18, 2)` untuk `MsProduct.Price`, `TrTransaction.TotalPrice`, `TrTransactionDetail.Price`, dan `TrTransactionDetail.SubTotal`, serta `HasPrecision(5, 2)` untuk `MsProduct.Discount` dan `TrTransactionDetail.Discount`.
   - **Foreign Key Delete Behavior (`DeleteBehavior.Restrict`)**: Keempat relasi *foreign key* (`FK_TrTransaction_MsUser`, `FK_TrTransaction_LtPaymentMethod`, `FK_TrTransactionDetail_TrTransaction`, `FK_TrTransactionDetail_MsProduct`) dikonfigurasi menggunakan `DeleteBehavior.Restrict` agar selaras dengan aturan `NO ACTION` / *soft-delete* (`Stsrc = 1`) di database.
   - **Connection String Update & Live Verification**: `ConnectionStrings:DefaultConnection` pada `BeeMart.Api/appsettings.json` telah diperbarui menjadi `"Server=localhost\\SQLEXPRESS;Database=BeeMart;Trusted_Connection=True;TrustServerCertificate=True;"`, didaftarkan melalui `AddDbContext<BeeMartDbContext>` di `BeeMart.Api/Program.cs`, dan diverifikasi secara langsung terhadap database `BeeMart` (`PaymentMethods.Count() = 3`, `Users.Count() = 6`, `Products.Count() = 22`).
9. **Phase 5 Backend Foundations Decisions (Response Wrapper, BCrypt, JWT & Claims)**:
   - **Standard JSON Response Wrapper (`ApiResponse<T>` & `ApiResponse`)**: Properti `Message` dan `Data` menggunakan atribut `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` sehingga serialisasi JSON sesuai persis dengan contoh `PRD.md` Section 6.2 (respons sukses tanpa pesan hanya memuat `"status"` dan `"data"`, respons sukses dengan pesan memuat `"status"`, `"message"`, dan `"data"`, sedangkan respons error hanya memuat `"status": "error"` dan `"message"`).
   - **Dual JWT Claim Mapping (`JwtTokenService` & `ClaimsHelper`)**: Token JWT menyertakan klaim kustom eksplisit (`"idUser"`, `"username"`, `"fullName"`, `"role"`) sekaligus klaim standar .NET (`ClaimTypes.NameIdentifier`, `ClaimTypes.Name`, `ClaimTypes.Role`). Hal ini memastikan atribut `[Authorize(Roles = "Admin,Cashier")]` dan `User.Identity.Name` bawaan ASP.NET Core bekerja otomatis, sementara `ClaimsHelper` (`GetUserId()`, `GetUsername()`, `GetFullName()`, `GetRole()`, `IsAdmin()`, `IsCashier()`) dapat mengekstrak keempat klaim pengguna secara konsisten.
   - **Token Lifetime & Validation**: Durasi token JWT dikonfigurasi melalui `JwtSettings:ExpiryMinutes` di `BeeMart.Api/appsettings.json` dengan nilai default `1440` menit (24 jam) menggunakan algoritma HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`) dan divalidasi secara penuh (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey`, `ClockSkew = TimeSpan.Zero`) pada pipeline `UseAuthentication()` di `BeeMart.Api/Program.cs`.
10. **Phase 6 Backend Authentication Endpoints Decisions (`AuthController`, 5-Digit `IdUser` Generator & Validation)**:
    - **5-Digit `IdUser` Generation Strategy (`GenerateNextUserIdAsync`)**: Saat registrasi user baru pada `POST /api/v1/auth/register`, sistem mengambil seluruh nilai `IdUser` dari tabel `MsUser`, mem-parsing nilai numerik, mencari nilai maksimum saat ini (`< 99999`), menambahkan `+ 1`, dan memformatnya dengan `D5` (contoh: setelah user *seed* tertinggi `78886` milik `felix`, user baru berikutnya mendapat `"78887"`). Terdapat pula *fallback loop* (`00001`–`99999`) berbasis `HashSet` untuk menjamin tidak pernah terjadi bentrokan primary key meskipun `IdUser` mencapai batas atas.
    - **Case-Insensitive Username Uniqueness & Exact Error Messages**: Validasi keunikan `Username` dilakukan secara *case-insensitive* (`u.Username.ToLower() == trimmedUsername.ToLower()`) dan mengembalikan **HTTP 400 Bad Request** dengan pesan persis `"username has already exists"`. Ketidakcocokan konfirmasi password mengembalikan `"Password and confirmation password do not match"`, role di luar `"Admin"` / `"Cashier"` mengembalikan `"Role must be either Admin or Cashier"`, dan kegagalan login (user tidak ditemukan / tidak aktif `Stsrc == false` / role tidak sesuai / verifikasi BCrypt gagal) mengembalikan **HTTP 401 Unauthorized** dengan pesan persis `"Invalid login"`.
    - **Custom Validation Wrapper Consistency (`SuppressModelStateInvalidFilter = true`)**: Pada `BeeMart.Api/Program.cs`, opsi `SuppressModelStateInvalidFilter = true` diaktifkan pada `AddControllers()` agar request dengan properti kosong/null tidak diinterupsi oleh format default `ValidationProblemDetails` ASP.NET Core, melainkan selalu divalidasi oleh controller dan dikembalikan dalam format `ApiResponse.Error(...)`.
