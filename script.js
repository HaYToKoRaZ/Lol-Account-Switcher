/* ==============================================================================
   Lol Account Switcher - Official Web Ecosystem Interaction Engine
   Theme & Locale Engine • Interactive Mockup • SEO Friendly
   ============================================================================== */

(function () {
  'use strict';

  // 1. THEME DETECTION & INITIALIZATION
  const themeToggleBtn = document.getElementById('themeToggle');
  const storedTheme = localStorage.getItem('theme');
  const systemPrefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
  const initialTheme = storedTheme || (systemPrefersDark ? 'dark' : 'light');

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    if (themeToggleBtn) {
      themeToggleBtn.innerHTML = theme === 'dark' ? '🌙' : '☀️';
      themeToggleBtn.setAttribute('title', theme === 'dark' ? 'Açık Piltover Temasına Geç' : 'Koyu Hextech Temasına Geç');
      themeToggleBtn.setAttribute('aria-label', theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme');
    }
  }

  applyTheme(initialTheme);

  if (themeToggleBtn) {
    themeToggleBtn.addEventListener('click', function () {
      const current = document.documentElement.getAttribute('data-theme') || 'dark';
      const nextTheme = current === 'dark' ? 'light' : 'dark';
      applyTheme(nextTheme);
    });
  }

  // 2. LANGUAGE NAVIGATION ENGINE
  const langBtnTR = document.getElementById('langTR');
  const langBtnEN = document.getElementById('langEN');

  if (langBtnTR) {
    langBtnTR.addEventListener('click', function (e) {
      localStorage.setItem('lang', 'tr');
      const href = langBtnTR.getAttribute('href');
      if (href && !href.startsWith('#')) {
        window.location.href = href;
      }
    });
  }

  if (langBtnEN) {
    langBtnEN.addEventListener('click', function (e) {
      localStorage.setItem('lang', 'en');
      const href = langBtnEN.getAttribute('href');
      if (href && !href.startsWith('#')) {
        window.location.href = href;
      }
    });
  }

  // 3. INTERACTIVE MOCKUP SIMULATION
  const mockCards = document.querySelectorAll('.mock-card');
  const mockToast = document.getElementById('mockToast');

  mockCards.forEach(function (card) {
    const switchBtn = card.querySelector('.mock-switch-btn');
    if (switchBtn) {
      switchBtn.addEventListener('click', function (e) {
        e.stopPropagation();

        // Update active class
        mockCards.forEach(c => c.classList.remove('active'));
        card.classList.add('active');

        // Show feedback toast
        if (mockToast) {
          const accName = card.querySelector('.mock-acc-name')?.textContent || 'Hesap';
          const isTurkish = document.documentElement.lang === 'tr';
          mockToast.textContent = isTurkish ? `⚡ ${accName} Aktifleştirildi!` : `⚡ ${accName} Activated!`;
          mockToast.style.display = 'block';

          setTimeout(function () {
            mockToast.style.display = 'none';
          }, 2400);
        }
      });
    }
  });

})();
