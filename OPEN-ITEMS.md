# Open Items & Project Roadmap — BeeMart POS Web App

Daftar *to-do* dan status pengerjaan proyek **BeeMart POS Web App** berdasarkan rencana 15 fase, beserta catatan keputusan teknis dan asumsi.

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
- [ ] Seed `LtPaymentMethod` (`Cash`, `Debit`, `Credit`).
- [ ] Seed `MsUser` accounts (`admin`, `steven`, `gabri`, `mona`, `felix`, `gobuta`) with hashed passwords and 5-digit `IdUser` values matching the PDF examples.
- [ ] Seed `MsProduct` items (`Apel Fuji`, `Apel Merah`, `Apel Hijau`, `Susu UHT Full Cream`, plus additional products to test the max-10 search limit) and sample historical transactions if needed.
- [ ] Combine all DDL and DML into a single executable SQL script (`Database/BeeMart.sql`).

### Phase 4: EF Core Entities & DbContext
- [ ] Create entity classes for `MsUser`, `LtPaymentMethod`, `MsProduct`, `TrTransaction`, and `TrTransactionDetail` in `BeeMart.Api`.
- [ ] Create `BeeMartDbContext` with Fluent API configurations matching the SQL schema and register it in `BeeMart.Api/Program.cs` using `DefaultConnection`.

### Phase 5: Backend Foundations
- [ ] Implement standard JSON response wrapper (`status`, `message`, `data`) and `/api/v1` routing convention.
- [ ] Implement password hasher & verifier (`PasswordHash`).
- [ ] Implement JWT token generation, authentication/authorization middleware, and claims helper (`IdUser`, `Username`, `FullName`, `Role`).

### Phase 6: Backend Auth Endpoints
- [ ] Implement `POST /api/v1/auth/register` (validate required fields, password match, role, case-insensitive unique username with `"username has already exists"`, generate 5-digit `IdUser`, hash password).
- [ ] Implement `POST /api/v1/auth/login` (validate credentials & role against active `MsUser` records, return `"Invalid login"` on mismatch, issue JWT token on success).

### Phase 7: Backend Master Data Endpoints
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

## 2. Notes / Decisions / Blockers (Assumptions Recorded in Phase 1 & Phase 2)

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
   - **Live Execution & Idempotency Verification**: Script `Database/BeeMart.sql` telah **dieksekusi secara langsung sebanyak dua kali berturut-turut** menggunakan `sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -i "Database\BeeMart.sql"` dengan hasil `exit code: 0` tanpa error pada kedua eksekusi. Pemeriksaan katalog sistem (`sys.tables`, `sys.indexes`, `sys.foreign_keys`, `sys.check_constraints`, `sys.default_constraints`) mengonfirmasi keberadaan 5 tabel, 4 indeks Section 5.4 (+ `UQ_LtPaymentMethod_MethodName` & 5 PK), 4 FK, 2 CHECK constraint, dan 26 DEFAULT constraint yang 100% sesuai dengan `PRD.md` Section 5.1 & 5.3.
   - **Catatan Instance SQL Server Lokal (`localhost\SQLEXPRESS`)**: Instance SQL Server yang aktif pada mesin ini adalah `MSSQL$SQLEXPRESS` (`localhost\SQLEXPRESS`). Pada Phase 4 (ketika mengonfigurasi `BeeMartDbContext`), pastikan apakah `DefaultConnection` di `BeeMart.Api/appsettings.json` perlu disesuaikan ke `Server=localhost\\SQLEXPRESS` atau tetap `Server=localhost`.
