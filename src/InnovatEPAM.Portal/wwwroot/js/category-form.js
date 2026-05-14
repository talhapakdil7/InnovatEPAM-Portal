(function () {
    'use strict';

    var categorySelect = document.getElementById('categorySelect');

    if (!categorySelect) return;

    /**
     * Shows the section matching the given category key and hides all others.
     * @param {string} categoryKey
     */
    function showSection(categoryKey) {
        var sections = document.querySelectorAll('.category-section');
        sections.forEach(function (section) {
            section.style.display = 'none';
        });

        if (categoryKey) {
            var target = document.getElementById('section-' + categoryKey);
            if (target) {
                target.style.display = '';
            }
        }
    }

    categorySelect.addEventListener('change', function () {
        showSection(this.value);
    });

    /**
     * Called on page load when returning from a failed server-side validation
     * so the previously selected category section is restored.
     * @param {string} categoryKey
     */
    window.initCategoryForm = function (categoryKey) {
        if (categoryKey) {
            showSection(categoryKey);
        }
    };

    // Apply initial state on first load (handles browser back-button autofill)
    showSection(categorySelect.value);
}());
