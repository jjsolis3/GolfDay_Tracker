// GolfDay Tracker - Main Site JavaScript

'use strict';

(function () {
    // Auto-dismiss alerts after 5 seconds
    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // Activate Bootstrap tooltips
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        bootstrap.Tooltip.getOrCreateInstance(el);
    });

    // Confirm dangerous actions
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            if (!confirm(this.dataset.confirm || 'Are you sure?')) {
                e.preventDefault();
            }
        });
    });
})();

// Score color helper — apply data-score-to-par styling
function applyScoreColor(input) {
    const strokes = parseInt(input.value);
    const par = parseInt(input.dataset.par || 4);
    if (isNaN(strokes) || strokes <= 0) {
        input.removeAttribute('data-score-to-par');
        return;
    }
    const diff = strokes - par;
    input.setAttribute('data-score-to-par', diff.toString());
}

// Format score-to-par display
function formatScoreToPar(score, par) {
    const diff = score - par;
    if (diff < 0) return diff.toString();
    if (diff === 0) return 'E';
    return '+' + diff;
}

// Score label helper
function getScoreLabel(scoreToPar) {
    switch (scoreToPar) {
        case -3: return 'Albatross';
        case -2: return 'Eagle';
        case -1: return 'Birdie';
        case  0: return 'Par';
        case  1: return 'Bogey';
        case  2: return 'Double Bogey';
        case  3: return 'Triple Bogey';
        default: return scoreToPar > 0 ? '+' + scoreToPar : scoreToPar.toString();
    }
}

// ================================================
// Dark Mode Toggle
// ================================================
(function () {
    var STORAGE_KEY = 'golfday-theme';

    function getPreferred() {
        var stored = localStorage.getItem(STORAGE_KEY);
        if (stored) return stored;
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);
        var btn = document.getElementById('dark-mode-toggle');
        if (btn) {
            btn.querySelector('i').className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
            btn.setAttribute('title', theme === 'dark' ? 'Switch to Light Mode' : 'Switch to Dark Mode');
        }
    }

    // Apply immediately to avoid flash
    applyTheme(getPreferred());

    document.addEventListener('DOMContentLoaded', function () {
        applyTheme(getPreferred());
        var btn = document.getElementById('dark-mode-toggle');
        if (btn) {
            btn.addEventListener('click', function () {
                var current = document.documentElement.getAttribute('data-bs-theme') || 'light';
                applyTheme(current === 'dark' ? 'light' : 'dark');
            });
        }
    });
})();

// ================================================
// Quick Actions FAB
// ================================================
document.addEventListener('DOMContentLoaded', function () {
    var fabMain = document.getElementById('fab-main');
    var fabActions = document.getElementById('fab-actions');
    if (!fabMain || !fabActions) return;

    fabMain.addEventListener('click', function () {
        var isOpen = fabActions.classList.contains('show');
        if (isOpen) {
            fabActions.classList.remove('show');
            fabMain.classList.remove('open');
            fabMain.setAttribute('aria-expanded', 'false');
        } else {
            fabActions.classList.add('show');
            fabMain.classList.add('open');
            fabMain.setAttribute('aria-expanded', 'true');
        }
    });

    // Close FAB when clicking outside
    document.addEventListener('click', function (e) {
        var container = document.getElementById('fab-container');
        if (container && !container.contains(e.target)) {
            fabActions.classList.remove('show');
            fabMain.classList.remove('open');
            fabMain.setAttribute('aria-expanded', 'false');
        }
    });
});
