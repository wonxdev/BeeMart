# Project Status & Future Enhancements

## Current Project Status (`v1.0.0`)

All core functional and architectural requirements for **BeeMart POS v1.0.0** are complete and verified:

- **Two-Tier .NET 8 Architecture**: Complete separation between `BeeMart.Api` (ASP.NET Core Web API + EF Core 8) and `BeeMart.Web` (Razor Pages HTTP consumer with zero direct database dependencies).
- **Database & Seed Data**: Idempotent SQL Server schema (`Database/BeeMart.sql`) with 5 relational tables, explicit constraints, indexes, and initial seed data for payment methods, BCrypt-hashed users, and products.
- **Authentication & Authorization**: JWT Bearer authentication on the API bridged with Cookie Authentication on the web client, enforcing role permissions between `Cashier` and `Admin`.
- **POS Checkout & History**: Real-time product search, stock-validated shopping cart, sequential daily invoice generation (`INV-YYYYMMDD-XXXX`), atomic stock deduction, and role-scoped transaction history with expandable item details.

---

## Future Roadmap & Enhancements

If we continue iterating on BeeMart POS beyond this submission, here are the practical improvements we plan to tackle next:

1. **Admin Master Data Management (CRUD UI)**
   - Add dedicated Admin pages to create, update, restock, and soft-delete products (`MsProduct`) and payment methods (`LtPaymentMethod`) directly from the web UI instead of relying on SQL scripts.
   - Include low-stock warning badges on the product dashboard when inventory drops below a configurable threshold.
2. **Thermal Receipt Printing & PDF Export**
   - Integrate browser print stylesheets (`@media print`) and ESC/POS thermal printer formatting on the checkout receipt modal so cashiers can print physical 58mm/80mm receipts.
   - Allow customers or cashiers to download a clean PDF invoice copy.
3. **Sales Reporting & Excel/CSV Export**
   - Allow administrators to export filtered transaction history from `/Transaction/History` into `.xlsx` or `.csv` spreadsheets.
   - Add a summary analytics card showing daily gross revenue, transaction count, and top-selling items.
4. **USB Barcode Scanner Support**
   - Add SKU/barcode columns to `MsProduct` and keyboard-wedge listener support on `/Transaction/New` so cashiers can scan items directly into the shopping cart.
5. **Dark Mode Theme Toggle**
   - Extend the CSS custom properties in `wwwroot/css/site.css` with a dark slate color theme and persist the cashier's preference in `localStorage`.
6. **Refresh Tokens & Password Management**
   - Add sliding refresh tokens for long cashier shifts and a self-service password change form for staff accounts.
