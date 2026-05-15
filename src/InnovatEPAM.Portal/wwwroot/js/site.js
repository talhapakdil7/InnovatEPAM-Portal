(function () {
  "use strict";

  function qs(sel, ctx) {
    return (ctx || document).querySelector(sel);
  }

  function initNavbarScroll(nav) {
    if (!nav || !("IntersectionObserver" in window)) return;
    var sentinel = qs(".portal-navbar-sentinel");
    if (!sentinel) return;

    var io = new IntersectionObserver(
      function (entries) {
        var e = entries[0];
        nav.classList.toggle("is-scrolled", !e.isIntersecting || e.intersectionRatio < 1);
      },
      { rootMargin: "0px", threshold: [1] }
    );
    io.observe(sentinel);
  }

  function initSidebarToggle(btn) {
    if (!btn) return;
    var root = document.documentElement;
    var key = "portal.sidebar.collapsed";
    try {
      if (window.matchMedia("(min-width: 992px)").matches && localStorage.getItem(key) === "1") {
        root.classList.add("portal-sidebar-collapsed");
      }
    } catch (_) {}

    btn.addEventListener("click", function () {
      root.classList.toggle("portal-sidebar-collapsed");
      try {
        if (window.matchMedia("(min-width: 992px)").matches)
          localStorage.setItem(key, root.classList.contains("portal-sidebar-collapsed") ? "1" : "0");
      } catch (_) {}
    });
  }

  function initFormLoading(btn) {
    if (!btn) return;
    var form = btn.closest("form");
    if (!form) return;

    form.addEventListener(
      "submit",
      function () {
        if (!form.checkValidity() || btn.disabled) return;
        if (!btn.hasAttribute("data-portal-loading")) return;
        btn.disabled = true;
        btn.dataset.portalWasHtml = btn.innerHTML;
        btn.classList.add("portal-btn-loading");
        btn.innerHTML =
          '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>';
      },
      { capture: false }
    );
  }

  function initFileDropLabel(zone) {
    var input = zone.querySelector('input[type="file"]');
    var nameEl = zone.querySelector("[data-dropzone-file-name]");
    if (!input || !nameEl) return;
    input.addEventListener("change", function () {
      nameEl.textContent = input.files && input.files[0] ? input.files[0].name : "No file chosen";
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initNavbarScroll(qs("#portalNavbar"));
    initSidebarToggle(qs('[data-sidebar-toggle="desktop"]'));

    document.querySelectorAll('[data-portal-loading="submit"]').forEach(initFormLoading);

    qs("body") && qs("body").querySelectorAll("[data-dropzone]").forEach(initFileDropLabel);
  });
})();
