(function () {
  "use strict";

  var categorySelect = document.getElementById("categorySelect");
  if (!categorySelect) return;

  function showSection(categoryKey) {
    var sections = document.querySelectorAll(".category-section");
    sections.forEach(function (section) {
      section.classList.add("hidden");
      section.style.removeProperty("display");
    });

    if (categoryKey) {
      var target = document.getElementById("section-" + categoryKey);
      if (target) {
        target.classList.remove("hidden");
        target.style.removeProperty("display");
      }
    }
  }

  function syncCategoryCards() {
    var val = categorySelect.value;
    document.querySelectorAll("[data-category-pick]").forEach(function (btn) {
      var sel = btn.getAttribute("data-category-pick") === val;
      btn.classList.toggle("is-selected", sel);
      btn.setAttribute("aria-pressed", sel ? "true" : "false");
    });
  }

  categorySelect.addEventListener("change", function () {
    showSection(this.value);
    syncCategoryCards();
  });

  document.querySelectorAll("[data-category-pick]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var key = btn.getAttribute("data-category-pick");
      categorySelect.value = key;
      showSection(key);
      syncCategoryCards();
      categorySelect.dispatchEvent(new Event("input", { bubbles: true }));
      categorySelect.dispatchEvent(new Event("change", { bubbles: true }));
    });
  });

  /**
   * Restores section visibility after server-side validation roundtrip.
   * @param {string} categoryKey
   */
  window.initCategoryForm = function (categoryKey) {
    if (categoryKey) categorySelect.value = categoryKey;
    showSection(categorySelect.value);
    syncCategoryCards();
  };

  showSection(categorySelect.value);
  syncCategoryCards();
})();
