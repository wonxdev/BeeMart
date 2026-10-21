// ============================================================================
// BeeMart POS Web App — Core Client-Side Utilities (Vanilla JS)
// ============================================================================

(function () {
    "use strict";

    /**
     * Formats a Date object as "<M/d/yyyy, HH.mm.ss>" matching PRD FR-NAV-001.
     * Example: "4/9/2026, 10.32.46"
     */
    function formatRealtimeClock(date) {
        var month = date.getMonth() + 1;
        var day = date.getDate();
        var year = date.getFullYear();
        var hours = String(date.getHours()).padStart(2, "0");
        var minutes = String(date.getMinutes()).padStart(2, "0");
        var seconds = String(date.getSeconds()).padStart(2, "0");
        return month + "/" + day + "/" + year + ", " + hours + "." + minutes + "." + seconds;
    }

    /**
     * Formats a numeric value in Indonesian Rupiah ("Rp 35.000") matching PRD FE-STYLE-002.
     */
    function formatRupiah(amount) {
        var numeric = Number(amount) || 0;
        var rounded = Math.round(numeric);
        return "Rp " + rounded.toLocaleString("id-ID");
    }

    function initRealtimeClock() {
        var clockEl = document.getElementById("realtime-clock");
        if (!clockEl) {
            return;
        }

        function tick() {
            clockEl.textContent = formatRealtimeClock(new Date());
        }

        tick();
        setInterval(tick, 1000);
    }

    function initAlertDismissal() {
        document.addEventListener("click", function (event) {
            var closeBtn = event.target.closest("[data-dismiss='alert']");
            if (!closeBtn) {
                return;
            }

            var alertBox = closeBtn.closest(".alert");
            if (alertBox) {
                alertBox.remove();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initRealtimeClock();
        initAlertDismissal();
    });

    window.BeeMart = {
        formatRealtimeClock: formatRealtimeClock,
        formatRupiah: formatRupiah
    };
})();
