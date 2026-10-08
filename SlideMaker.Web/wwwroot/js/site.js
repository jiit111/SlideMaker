(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var toggle = document.getElementById("sidebarToggle");
        var sidebar = document.getElementById("appSidebar");
        if (toggle && sidebar) {
            toggle.addEventListener("click", function () {
                sidebar.classList.toggle("show");
            });
        }

        initCardTilt();
    });

    window.SlideMaker = window.SlideMaker || {};

    window.SlideMaker.showToast = function (message, variant) {
        variant = variant || "success";
        var container = document.getElementById("toastContainer");
        if (!container) return;

        var toastEl = document.createElement("div");
        toastEl.className = "toast align-items-center text-bg-" + variant + " border-0";
        toastEl.setAttribute("role", "alert");
        toastEl.innerHTML =
            '<div class="d-flex">' +
            '<div class="toast-body">' + message + "</div>" +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>' +
            "</div>";
        container.appendChild(toastEl);
        var toast = new bootstrap.Toast(toastEl, { delay: 4000 });
        toast.show();
        toastEl.addEventListener("hidden.bs.toast", function () { toastEl.remove(); });
    };

    window.SlideMaker.getAntiForgeryToken = function () {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    };

    // ---- Global loader overlay -------------------------------------------------
    var loaderDepth = 0;

    window.SlideMaker.showLoader = function () {
        loaderDepth++;
        var el = document.getElementById("globalLoader");
        if (el) el.classList.add("show");
    };

    window.SlideMaker.hideLoader = function (force) {
        loaderDepth = force ? 0 : Math.max(0, loaderDepth - 1);
        if (loaderDepth === 0) {
            var el = document.getElementById("globalLoader");
            if (el) el.classList.remove("show");
        }
    };

    document.addEventListener("DOMContentLoaded", function () {
        // Any normal (non-AJAX) form submit gets the loader, since it means a full
        // page round-trip is coming — opt out with data-no-loader (e.g. the file
        // upload form, which already shows its own progress bar).
        document.body.addEventListener("submit", function (e) {
            var form = e.target;
            if (form && form.tagName === "FORM" && !form.hasAttribute("data-no-loader") && !e.defaultPrevented) {
                window.SlideMaker.showLoader();
            }
        });
    });

    // A page restored from the back/forward cache should never show a stale loader.
    window.addEventListener("pageshow", function () {
        window.SlideMaker.hideLoader(true);
    });

    window.SlideMaker.postForm = function (url, data) {
        var body = new URLSearchParams();
        Object.keys(data || {}).forEach(function (key) {
            body.append(key, data[key]);
        });
        body.append("__RequestVerificationToken", window.SlideMaker.getAntiForgeryToken());

        window.SlideMaker.showLoader();
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: body.toString()
        }).finally(function () { window.SlideMaker.hideLoader(); });
    };

    window.SlideMaker.postJson = function (url, data) {
        window.SlideMaker.showLoader();
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": window.SlideMaker.getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        }).finally(function () { window.SlideMaker.hideLoader(); });
    };

    // ---- Subtle 3D mouse-tilt on cards ------------------------------------------
    function initCardTilt() {
        var cards = document.querySelectorAll(".dashboard-card");
        cards.forEach(function (card) {
            card.classList.add("tilt-3d");

            card.addEventListener("mousemove", function (e) {
                var rect = card.getBoundingClientRect();
                var px = (e.clientX - rect.left) / rect.width - 0.5;
                var py = (e.clientY - rect.top) / rect.height - 0.5;
                card.style.setProperty("--ry", (px * 6) + "deg");
                card.style.setProperty("--rx", (-py * 6) + "deg");
                card.style.setProperty("--tz", "6px");
            });

            card.addEventListener("mouseleave", function () {
                card.style.setProperty("--ry", "0deg");
                card.style.setProperty("--rx", "0deg");
                card.style.setProperty("--tz", "0px");
            });
        });
    }
})();
