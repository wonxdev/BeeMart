// BeeMart POS — New Transaction Client Logic
(function () {
    "use strict";

    // DOM Elements
    var invoiceNoEl = document.getElementById("pos-invoice-no");
    var invoiceInputEl = document.getElementById("pos-invoice-input");
    var posClockEl = document.getElementById("pos-realtime-clock");
    var alertContainer = document.getElementById("pos-alert-container");

    var searchInput = document.getElementById("product-search-input");
    var searchBtn = document.getElementById("product-search-btn");
    var resetBtn = document.getElementById("product-reset-btn");
    var catalogCountBadge = document.getElementById("catalog-count-badge");
    var catalogTbody = document.getElementById("product-catalog-tbody");
    var paginationBar = document.getElementById("product-pagination-bar");
    var pageInfoEl = document.getElementById("product-page-info");
    var pagePrevBtn = document.getElementById("page-prev-btn");
    var pageNextBtn = document.getElementById("page-next-btn");
    var pageCurrentBadge = document.getElementById("page-current-badge");

    var selectedPanel = document.getElementById("selected-product-panel");
    var selectedTitle = document.getElementById("selected-product-title");
    var selectedPrice = document.getElementById("selected-product-price");
    var selectedStock = document.getElementById("selected-product-stock");
    var selectedQtyInput = document.getElementById("selected-product-qty");
    var selectedAddBtn = document.getElementById("selected-product-add-btn");
    var selectedCloseBtn = document.getElementById("selected-product-close-btn");
    var selectedErrorEl = document.getElementById("selected-product-error");

    var cartTbody = document.getElementById("cart-tbody");
    var clearCartBtn = document.getElementById("clear-cart-btn");
    var summaryTotalItemEl = document.getElementById("summary-total-item");
    var summaryTotalPriceEl = document.getElementById("summary-total-price");
    var paymentMethodSelect = document.getElementById("payment-method-select");
    var customerMoneyInput = document.getElementById("customer-money-input");
    var customerMoneyHint = document.getElementById("customer-money-hint");
    var summaryChangeEl = document.getElementById("summary-change");
    var submitBtn = document.getElementById("submit-transaction-btn");

    var receiptModal = document.getElementById("receipt-modal-backdrop");
    var receiptInvoiceNo = document.getElementById("receipt-invoice-no");
    var receiptDate = document.getElementById("receipt-date");
    var receiptCashier = document.getElementById("receipt-cashier");
    var receiptPaymentMethod = document.getElementById("receipt-payment-method");
    var receiptItemsTbody = document.getElementById("receipt-items-tbody");
    var receiptTotalItem = document.getElementById("receipt-total-item");
    var receiptTotalPrice = document.getElementById("receipt-total-price");
    var receiptCustomerMoney = document.getElementById("receipt-customer-money");
    var receiptChange = document.getElementById("receipt-change");
    var newTransactionResetBtn = document.getElementById("new-transaction-reset-btn");

    if (!catalogTbody || !cartTbody) {
        return;
    }

    // State initialized from server-rendered DOM attributes
    var cart = [];
    var selectedProduct = null;
    var currentSearch = searchInput ? searchInput.value.trim() : "";
    var currentPage = (paginationBar && parseInt(paginationBar.getAttribute("data-page"), 10)) || 1;
    var totalPages = Math.max(1, (paginationBar && parseInt(paginationBar.getAttribute("data-total-pages"), 10)) || 1);
    var pageSize = (paginationBar && parseInt(paginationBar.getAttribute("data-page-size"), 10)) || 10;
    var searchDebounceTimer = null;
    var isSubmitting = false;

    function formatRupiah(amount) {
        if (window.BeeMart && typeof window.BeeMart.formatRupiah === "function") {
            return window.BeeMart.formatRupiah(amount);
        }
        var num = Math.round(Number(amount) || 0);
        return "Rp " + num.toLocaleString("id-ID");
    }

    function escapeHtml(text) {
        var div = document.createElement("div");
        div.textContent = text == null ? "" : String(text);
        return div.innerHTML;
    }

    function showAlert(message, type) {
        var alertClass = type === "success" ? "alert-success"
            : type === "warning" ? "alert-warning"
            : "alert-danger";

        alertContainer.innerHTML =
            '<div class="alert ' + alertClass + '" role="alert">' +
            '<span>' + escapeHtml(message) + '</span>' +
            '<button type="button" class="alert-close" data-dismiss="alert" aria-label="Close">&times;</button>' +
            '</div>';
    }

    function clearAlert() {
        alertContainer.innerHTML = "";
    }

    // Real-time clock on POS Meta Bar
    if (posClockEl && window.BeeMart && typeof window.BeeMart.formatRealtimeClock === "function") {
        setInterval(function () {
            posClockEl.textContent = window.BeeMart.formatRealtimeClock(new Date());
        }, 1000);
    }

    // Product Search & Pagination via Page Handler
    function loadProducts(page, search) {
        var targetPage = page || 1;
        var targetSearch = search !== undefined ? search : searchInput.value.trim();

        var url = "/Transaction/New?handler=SearchProducts&page=" + encodeURIComponent(targetPage) +
            "&pageSize=" + encodeURIComponent(pageSize);

        if (targetSearch) {
            url += "&search=" + encodeURIComponent(targetSearch);
        }

        fetch(url, {
            method: "GET",
            headers: { "Accept": "application/json" }
        })
        .then(function (res) { return res.json(); })
        .then(function (payload) {
            if (!payload || payload.status !== "success" || !payload.data) {
                showAlert((payload && payload.message) || "Gagal memuat data barang dari API.", "danger");
                return;
            }

            currentSearch = targetSearch;
            currentPage = payload.data.page || 1;
            totalPages = Math.max(1, payload.data.totalPages || 1);
            renderProductCatalog(payload.data.items || [], payload.data.totalItems || 0);
        })
        .catch(function () {
            showAlert("Gagal menghubungi server untuk pencarian barang.", "danger");
        });
    }

    function renderProductCatalog(items, totalItems) {
        catalogCountBadge.textContent = "Total: " + totalItems + " Barang";
        pageInfoEl.textContent = "Halaman " + currentPage + " dari " + totalPages;
        pageCurrentBadge.textContent = String(currentPage);

        if (currentPage <= 1) {
            pagePrevBtn.classList.add("disabled");
            pagePrevBtn.disabled = true;
        } else {
            pagePrevBtn.classList.remove("disabled");
            pagePrevBtn.disabled = false;
        }

        if (currentPage >= totalPages) {
            pageNextBtn.classList.add("disabled");
            pageNextBtn.disabled = true;
        } else {
            pageNextBtn.classList.remove("disabled");
            pageNextBtn.disabled = false;
        }

        if (!items || items.length === 0) {
            catalogTbody.innerHTML =
                '<tr><td colspan="4" class="table-empty">Barang tidak ditemukan.</td></tr>';
            return;
        }

        var html = "";
        for (var i = 0; i < items.length; i++) {
            var p = items[i];
            var outOfStock = (p.stock || 0) <= 0;
            var stockBadge = outOfStock
                ? '<span class="badge badge-danger">Habis (0)</span>'
                : '<span class="badge badge-success">' + p.stock + '</span>';
            var disabledAttr = outOfStock ? "disabled" : "";

            html +=
                '<tr data-product-id="' + escapeHtml(p.idProduct) + '" ' +
                'data-product-name="' + escapeHtml(p.productName) + '" ' +
                'data-product-price="' + Number(p.price) + '" ' +
                'data-product-stock="' + Number(p.stock) + '">' +
                '<td class="font-semibold">' + escapeHtml(p.productName) + '</td>' +
                '<td class="text-right price-text">' + formatRupiah(p.price) + '</td>' +
                '<td class="text-center">' + stockBadge + '</td>' +
                '<td class="text-center">' +
                '<div class="d-flex justify-center gap-1">' +
                '<button type="button" class="btn btn-accent btn-sm js-select-product" ' + disabledAttr + '>Pilih</button>' +
                '<button type="button" class="btn btn-primary btn-sm js-quick-add-product" title="Tambah 1 ke Keranjang" ' + disabledAttr + '>+ Keranjang</button>' +
                '</div>' +
                '</td>' +
                '</tr>';
        }

        catalogTbody.innerHTML = html;
    }

    // Open "Barang Terpilih" dialog panel
    function openSelectedProductPanel(product) {
        selectedProduct = product;
        selectedTitle.textContent = "Barang Terpilih: " + product.productName;
        selectedPrice.textContent = formatRupiah(product.price);
        selectedStock.textContent = String(product.maxStock);
        selectedQtyInput.value = "1";
        selectedQtyInput.max = String(product.maxStock);
        selectedErrorEl.style.display = "none";
        selectedPanel.style.display = "block";
        selectedQtyInput.focus();
    }

    function closeSelectedProductPanel() {
        selectedProduct = null;
        selectedPanel.style.display = "none";
        selectedErrorEl.style.display = "none";
    }

    // Add or accumulate item in Cart with stock ceiling check
    function addItemToCart(product, requestedQty) {
        var qtyToAdd = parseInt(requestedQty, 10);
        if (isNaN(qtyToAdd) || qtyToAdd < 1) {
            showAlert("Jumlah barang minimal 1.", "warning");
            return false;
        }

        if (product.maxStock <= 0) {
            showAlert("Stok barang habis.", "danger");
            return false;
        }

        var existingIndex = -1;
        for (var i = 0; i < cart.length; i++) {
            if (cart[i].idProduct === product.idProduct) {
                existingIndex = i;
                break;
            }
        }

        var currentQtyInCart = existingIndex >= 0 ? cart[existingIndex].quantity : 0;
        var newTotalQty = currentQtyInCart + qtyToAdd;

        if (newTotalQty > product.maxStock) {
            var stockMsg = "Jumlah barang yang dimasukkan melebihi sisa stok.";
            showAlert(stockMsg, "danger");
            if (selectedProduct && selectedProduct.idProduct === product.idProduct) {
                selectedErrorEl.textContent = stockMsg;
                selectedErrorEl.style.display = "block";
            }
            return false;
        }

        if (existingIndex >= 0) {
            cart[existingIndex].quantity = newTotalQty;
            cart[existingIndex].maxStock = product.maxStock;
            cart[existingIndex].price = product.price;
        } else {
            cart.push({
                idProduct: product.idProduct,
                productName: product.productName,
                price: Number(product.price),
                quantity: qtyToAdd,
                maxStock: Number(product.maxStock)
            });
        }

        clearAlert();
        renderCart();
        return true;
    }

    function updateCartItemQuantity(idProduct, newQty) {
        for (var i = 0; i < cart.length; i++) {
            if (cart[i].idProduct === idProduct) {
                var parsed = parseInt(newQty, 10);
                if (isNaN(parsed) || parsed < 1) {
                    parsed = 1;
                }
                if (parsed > cart[i].maxStock) {
                    showAlert("Jumlah barang yang dimasukkan melebihi sisa stok.", "danger");
                    parsed = cart[i].maxStock;
                }
                cart[i].quantity = parsed;
                break;
            }
        }
        renderCart();
    }

    function removeCartItem(idProduct) {
        cart = cart.filter(function (item) {
            return item.idProduct !== idProduct;
        });
        renderCart();
    }

    function calculateTotals() {
        var totalItem = 0;
        var totalPrice = 0;
        for (var i = 0; i < cart.length; i++) {
            var subtotal = cart[i].price * cart[i].quantity;
            totalItem += cart[i].quantity;
            totalPrice += subtotal;
        }
        return {
            totalItem: totalItem,
            totalPrice: totalPrice
        };
    }

    function renderCart() {
        if (cart.length === 0) {
            cartTbody.innerHTML =
                '<tr id="cart-empty-row">' +
                '<td colspan="5" class="table-empty">Daftar belanjaan masih kosong. Silakan pilih barang dari katalog.</td>' +
                '</tr>';
            clearCartBtn.disabled = true;
        } else {
            clearCartBtn.disabled = false;
            var html = "";
            for (var i = 0; i < cart.length; i++) {
                var item = cart[i];
                var subtotal = item.price * item.quantity;
                var minusDisabled = item.quantity <= 1 ? "disabled" : "";
                var plusDisabled = item.quantity >= item.maxStock ? "disabled" : "";

                html +=
                    '<tr data-cart-id="' + escapeHtml(item.idProduct) + '">' +
                    '<td>' +
                    '<div class="font-semibold">' + escapeHtml(item.productName) + '</div>' +
                    '<div class="text-muted" style="font-size: 0.75rem;">Maks. Stok: ' + item.maxStock + '</div>' +
                    '</td>' +
                    '<td class="text-right price-text">' + formatRupiah(item.price) + '</td>' +
                    '<td class="text-center">' +
                    '<div class="qty-control">' +
                    '<button type="button" class="qty-btn js-qty-minus" data-id="' + escapeHtml(item.idProduct) + '" ' + minusDisabled + '>&minus;</button>' +
                    '<input type="number" class="qty-input js-qty-input" data-id="' + escapeHtml(item.idProduct) + '" min="1" max="' + item.maxStock + '" value="' + item.quantity + '" />' +
                    '<button type="button" class="qty-btn js-qty-plus" data-id="' + escapeHtml(item.idProduct) + '" ' + plusDisabled + '>&#43;</button>' +
                    '</div>' +
                    '</td>' +
                    '<td class="text-right price-text">' + formatRupiah(subtotal) + '</td>' +
                    '<td class="text-center">' +
                    '<button type="button" class="btn btn-danger btn-sm js-cart-remove" data-id="' + escapeHtml(item.idProduct) + '">Hapus</button>' +
                    '</td>' +
                    '</tr>';
            }
            cartTbody.innerHTML = html;
        }

        updatePaymentSummary();
    }

    function updatePaymentSummary() {
        var totals = calculateTotals();
        var customerMoney = Number(customerMoneyInput.value) || 0;
        var change = customerMoney - totals.totalPrice;

        summaryTotalItemEl.textContent = totals.totalItem + " item";
        summaryTotalPriceEl.textContent = formatRupiah(totals.totalPrice);

        if (cart.length === 0) {
            summaryChangeEl.textContent = "Rp 0";
            summaryChangeEl.className = "price-text text-success";
            customerMoneyHint.style.display = "none";
            submitBtn.disabled = true;
            return;
        }

        if (customerMoney < totals.totalPrice) {
            summaryChangeEl.textContent = "-" + formatRupiah(Math.abs(change));
            summaryChangeEl.className = "price-text text-danger";
            customerMoneyHint.style.display = "block";
            submitBtn.disabled = true;
        } else {
            summaryChangeEl.textContent = formatRupiah(change);
            summaryChangeEl.className = "price-text text-success";
            customerMoneyHint.style.display = "none";
            submitBtn.disabled = isSubmitting || !paymentMethodSelect.value;
        }
    }

    // Event Listeners: Search
    searchInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            if (searchDebounceTimer) {
                clearTimeout(searchDebounceTimer);
            }
            loadProducts(1, searchInput.value.trim());
        }
    });

    searchInput.addEventListener("input", function () {
        if (searchDebounceTimer) {
            clearTimeout(searchDebounceTimer);
        }
        searchDebounceTimer = setTimeout(function () {
            loadProducts(1, searchInput.value.trim());
        }, 300);
    });

    searchBtn.addEventListener("click", function () {
        loadProducts(1, searchInput.value.trim());
    });

    resetBtn.addEventListener("click", function () {
        searchInput.value = "";
        loadProducts(1, "");
    });

    pagePrevBtn.addEventListener("click", function () {
        if (currentPage > 1) {
            loadProducts(currentPage - 1, currentSearch);
        }
    });

    pageNextBtn.addEventListener("click", function () {
        if (currentPage < totalPages) {
            loadProducts(currentPage + 1, currentSearch);
        }
    });

    // Event Listeners: Product Row Selection
    catalogTbody.addEventListener("click", function (e) {
        var selectBtn = e.target.closest(".js-select-product");
        var quickAddBtn = e.target.closest(".js-quick-add-product");
        if (!selectBtn && !quickAddBtn) {
            return;
        }

        var row = e.target.closest("tr[data-product-id]");
        if (!row) {
            return;
        }

        var product = {
            idProduct: row.getAttribute("data-product-id"),
            productName: row.getAttribute("data-product-name"),
            price: Number(row.getAttribute("data-product-price")),
            maxStock: parseInt(row.getAttribute("data-product-stock"), 10) || 0
        };

        if (selectBtn) {
            openSelectedProductPanel(product);
        } else if (quickAddBtn) {
            addItemToCart(product, 1);
        }
    });

    selectedCloseBtn.addEventListener("click", closeSelectedProductPanel);

    selectedAddBtn.addEventListener("click", function () {
        if (!selectedProduct) {
            return;
        }
        var qty = parseInt(selectedQtyInput.value, 10);
        if (addItemToCart(selectedProduct, qty)) {
            closeSelectedProductPanel();
        }
    });

    selectedQtyInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            selectedAddBtn.click();
        }
    });

    // Event Listeners: Cart Table Controls
    cartTbody.addEventListener("click", function (e) {
        var minusBtn = e.target.closest(".js-qty-minus");
        var plusBtn = e.target.closest(".js-qty-plus");
        var removeBtn = e.target.closest(".js-cart-remove");

        if (minusBtn) {
            var idMinus = minusBtn.getAttribute("data-id");
            var itemMinus = cart.find(function (c) { return c.idProduct === idMinus; });
            if (itemMinus && itemMinus.quantity > 1) {
                updateCartItemQuantity(idMinus, itemMinus.quantity - 1);
            }
        } else if (plusBtn) {
            var idPlus = plusBtn.getAttribute("data-id");
            var itemPlus = cart.find(function (c) { return c.idProduct === idPlus; });
            if (itemPlus) {
                if (itemPlus.quantity >= itemPlus.maxStock) {
                    showAlert("Jumlah barang yang dimasukkan melebihi sisa stok.", "danger");
                } else {
                    updateCartItemQuantity(idPlus, itemPlus.quantity + 1);
                }
            }
        } else if (removeBtn) {
            var idRemove = removeBtn.getAttribute("data-id");
            removeCartItem(idRemove);
        }
    });

    cartTbody.addEventListener("change", function (e) {
        var qtyInput = e.target.closest(".js-qty-input");
        if (!qtyInput) {
            return;
        }
        var id = qtyInput.getAttribute("data-id");
        updateCartItemQuantity(id, qtyInput.value);
    });

    clearCartBtn.addEventListener("click", function () {
        cart = [];
        closeSelectedProductPanel();
        renderCart();
    });

    // Event Listeners: Customer Money & Quick Cash
    customerMoneyInput.addEventListener("input", updatePaymentSummary);
    paymentMethodSelect.addEventListener("change", updatePaymentSummary);

    document.querySelectorAll(".js-quick-cash").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var cashVal = btn.getAttribute("data-cash");
            var totals = calculateTotals();
            if (cashVal === "exact") {
                customerMoneyInput.value = String(Math.round(totals.totalPrice));
            } else {
                customerMoneyInput.value = cashVal;
            }
            updatePaymentSummary();
        });
    });

    // Checkout Execution
    submitBtn.addEventListener("click", function () {
        if (isSubmitting || cart.length === 0) {
            return;
        }

        var totals = calculateTotals();
        var customerMoney = Number(customerMoneyInput.value) || 0;
        if (customerMoney < totals.totalPrice) {
            showAlert("Customer money is insufficient", "danger");
            return;
        }

        var paymentMethodId = paymentMethodSelect.value;
        if (!paymentMethodId) {
            showAlert("Metode pembayaran wajib dipilih.", "danger");
            return;
        }

        var payload = {
            idPaymentMethod: String(paymentMethodId),
            customerMoney: customerMoney,
            items: cart.map(function (c) {
                return {
                    idProduct: c.idProduct,
                    quantity: c.quantity
                };
            })
        };

        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        var csrfToken = tokenInput ? tokenInput.value : "";

        isSubmitting = true;
        submitBtn.disabled = true;
        submitBtn.textContent = "Memproses Transaksi...";
        clearAlert();

        fetch("/Transaction/New?handler=Checkout", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
                "RequestVerificationToken": csrfToken
            },
            body: JSON.stringify(payload)
        })
        .then(function (res) {
            return res.json().then(function (body) {
                return { statusOk: res.ok, statusCode: res.status, body: body };
            });
        })
        .then(function (result) {
            isSubmitting = false;
            submitBtn.textContent = "Submit Transaksi";

            var apiRes = result.body;
            if (!result.statusOk || !apiRes || apiRes.status !== "success" || !apiRes.data) {
                var errMsg = (apiRes && apiRes.message) || "Gagal menyimpan transaksi.";
                showAlert(errMsg, "danger");
                updatePaymentSummary();
                return;
            }

            openReceiptModal(apiRes.data);
        })
        .catch(function () {
            isSubmitting = false;
            submitBtn.textContent = "Submit Transaksi";
            showAlert("Terjadi kesalahan jaringan saat memproses transaksi.", "danger");
            updatePaymentSummary();
        });
    });

    function openReceiptModal(receipt) {
        receiptInvoiceNo.textContent = receipt.invoiceNo || "-";
        var txDate = receipt.transactionDate ? new Date(receipt.transactionDate) : new Date();
        receiptDate.textContent = txDate.toLocaleString("id-ID");
        receiptCashier.textContent = receipt.cashierName || "-";
        receiptPaymentMethod.textContent = receipt.paymentMethodName || "-";
        receiptTotalItem.textContent = (receipt.totalItem || 0) + " item";
        receiptTotalPrice.textContent = formatRupiah(receipt.totalPrice || 0);
        receiptCustomerMoney.textContent = formatRupiah(receipt.customerMoney || 0);
        receiptChange.textContent = formatRupiah(receipt.change || 0);

        var details = receipt.details || [];
        var rowsHtml = "";
        for (var i = 0; i < details.length; i++) {
            var d = details[i];
            rowsHtml +=
                '<tr>' +
                '<td>' + escapeHtml(d.productName) + '</td>' +
                '<td class="text-center">' + d.quantity + '</td>' +
                '<td class="text-right price-text">' + formatRupiah(d.price) + '</td>' +
                '<td class="text-right price-text">' + formatRupiah(d.subtotal) + '</td>' +
                '</tr>';
        }
        receiptItemsTbody.innerHTML = rowsHtml;

        receiptModal.classList.add("show");
    }

    // Reset POS State for Next Transaction
    newTransactionResetBtn.addEventListener("click", function () {
        receiptModal.classList.remove("show");
        cart = [];
        closeSelectedProductPanel();
        customerMoneyInput.value = "0";
        if (paymentMethodSelect.options.length > 0) {
            paymentMethodSelect.selectedIndex = 0;
        }
        clearAlert();
        renderCart();

        // Fetch new sequential Invoice No
        fetch("/Transaction/New?handler=NextInvoice", {
            method: "GET",
            headers: { "Accept": "application/json" }
        })
        .then(function (res) { return res.json(); })
        .then(function (payload) {
            if (payload && payload.status === "success" && payload.data && payload.data.invoiceNo) {
                invoiceNoEl.textContent = payload.data.invoiceNo;
                invoiceInputEl.value = payload.data.invoiceNo;
            }
        });

        // Reload current product list so updated stocks are reflected
        loadProducts(currentPage, currentSearch);
    });
})();
