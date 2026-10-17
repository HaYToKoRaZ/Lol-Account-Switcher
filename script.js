/* ==============================================================================
   Lol Account Switcher - Official Web Ecosystem Interaction Engine
   Hextech Elite UI/UX • Theme & Locale Engine • Interactive Mockup Simulation
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
      playSynthChime(current === 'dark' ? 520 : 660);
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

  // 3. SYNTHETIC AUDIO FEEDBACK (Zero external files, pure Web Audio API)
  function playSynthChime(freq) {
    try {
      const AudioContext = window.AudioContext || window.webkitAudioContext;
      if (!AudioContext) return;
      const ctx = new AudioContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq || 587.33, ctx.currentTime); // D5
      osc.frequency.exponentialRampToValueAtTime(880, ctx.currentTime + 0.12); // A5

      gain.gain.setValueAtTime(0.04, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.16);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start();
      osc.stop(ctx.currentTime + 0.18);
    } catch (e) {
      // AudioContext unavailable or restricted by browser gesture
    }
  }

  // 4. INTERACTIVE HEXTECH MOCKUP SIMULATION (4-Phase Protocol)
  const mockCards = document.querySelectorAll('.mock-card');
  const mockToast = document.getElementById('mockToast');
  const mockHzPill = document.querySelector('.mock-hz-pill');
  let isSwitching = false;

  // Toggle Mock Hz Pill between 144Hz & 60Hz
  if (mockHzPill) {
    mockHzPill.addEventListener('click', function () {
      const is144 = mockHzPill.classList.toggle('mode-60hz');
      const isTR = document.documentElement.lang === 'tr';
      if (is144) {
        mockHzPill.innerHTML = isTR ? '🖥️ 60Hz (Masaüstü)' : '🖥️ 60Hz (Desktop)';
        playSynthChime(440);
      } else {
        mockHzPill.innerHTML = isTR ? '🖥️ 144Hz (Oyun Aktif)' : '🖥️ 144Hz Active';
        playSynthChime(740);
      }
    });
  }

  mockCards.forEach(function (card) {
    const switchBtn = card.querySelector('.mock-switch-btn');
    if (switchBtn) {
      switchBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        if (card.classList.contains('active') || isSwitching) return;

        isSwitching = true;
        playSynthChime(620);

        const isTR = document.documentElement.lang === 'tr';
        const accName = card.querySelector('.mock-acc-name')?.textContent || 'Hesap';

        // 4-Phase Step Visualizer
        const steps = isTR ? [
          '🛑 Faz 1: Açık Riot/LoL süreçleri kapatılıyor...',
          '📂 Faz 2: AppData oturum tokenları aktarılıyor...',
          '⚡ Faz 3: Win32 API 144Hz & Nihai Güç uygulanıyor...',
          `🚀 Faz 4: ${accName} başlatıldı!`
        ] : [
          '🛑 Phase 1: Terminating open LoL/Riot processes...',
          '📂 Phase 2: Deploying secure AppData session...',
          '⚡ Phase 3: Activating Win32 144Hz & Ultimate Power...',
          `🚀 Phase 4: ${accName} launched successfully!`
        ];

        let stepIndex = 0;
        if (mockToast) {
          mockToast.className = 'mock-toast active step-pulse';
          mockToast.textContent = steps[stepIndex];
        }

        const interval = setInterval(function () {
          stepIndex++;
          if (stepIndex < steps.length) {
            if (mockToast) mockToast.textContent = steps[stepIndex];
            playSynthChime(500 + stepIndex * 80);
          } else {
            clearInterval(interval);
            // Complete transition
            mockCards.forEach(function (c) {
              c.classList.remove('active');
              const btn = c.querySelector('.mock-switch-btn');
              if (btn) btn.textContent = isTR ? 'Geçiş Yap ➔' : 'Switch ➔';
            });

            card.classList.add('active');
            switchBtn.textContent = isTR ? 'Aktif ✓' : 'Active ✓';

            // Ensure 144Hz mode is displayed in header
            if (mockHzPill) {
              mockHzPill.classList.remove('mode-60hz');
              mockHzPill.innerHTML = isTR ? '🖥️ 144Hz (Oyun Aktif)' : '🖥️ 144Hz Active';
            }

            playSynthChime(880);

            setTimeout(function () {
              if (mockToast) {
                mockToast.classList.remove('active', 'step-pulse');
              }
              isSwitching = false;
            }, 1800);
          }
        }, 340);
      });
    }
  });

  // 5. COPY SNIPPET INTERACTION
  const copyBtns = document.querySelectorAll('.code-copy-btn');
  copyBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      const codeBlock = btn.closest('.arch-code-box')?.querySelector('code') || btn.nextElementSibling;
      if (codeBlock) {
        navigator.clipboard.writeText(codeBlock.textContent.trim()).then(function () {
          const original = btn.textContent;
          btn.textContent = 'Copied! ✓';
          setTimeout(function () {
            btn.textContent = original;
          }, 1800);
        });
      }
    });
  });

  // 6. PERSONALIZED HERO SIMULATOR ENGINE
  const simApplyBtn = document.getElementById('simApplyBtn');
  const simRiotId = document.getElementById('simRiotId');
  const simTierSelect = document.getElementById('simTierSelect');

  function applyPersonalizedMockup() {
    const rawVal = simRiotId ? simRiotId.value.trim() : '';
    const riotId = rawVal || 'YourAccount#EUW';
    const tier = simTierSelect ? simTierSelect.value : 'challenger';
    const isTR = document.documentElement.lang === 'tr';

    const card1 = document.querySelector('.mock-card[data-id="1"]');
    if (card1) {
      const accName = card1.querySelector('.mock-acc-name');
      const serverBadge = card1.querySelector('.mock-server-badge');
      const subRow = card1.querySelector('.mock-row-2');
      const rankMedal = card1.querySelector('.mock-rank-medal');
      const rankText = card1.querySelector('.mock-rank-text');

      const parts = riotId.split('#');
      const name = parts[0] || 'YourAccount';
      const tag = parts[1] ? parts[1].toUpperCase() : 'EUW';

      if (accName) accName.textContent = `${name} (Main)`;
      if (serverBadge) serverBadge.textContent = tag.substring(0, 4);

      if (subRow && subRow.firstElementChild) {
        subRow.firstElementChild.textContent = `${name}#${tag}`;
      }

      const tierCap = tier.charAt(0).toUpperCase() + tier.slice(1);
      const isRoot = window.location.pathname.endsWith('/tr/') || window.location.pathname.endsWith('/tr/index.html');
      const rankPathPrefix = isRoot ? '../ranks/' : 'ranks/';

      if (rankMedal) {
        rankMedal.src = `${rankPathPrefix}${tier}.webp`;
        rankMedal.onerror = function () {
          this.src = `${rankPathPrefix}${tier}.png`;
        };
        rankMedal.alt = `${tierCap} Medal`;
      }
      if (rankText) {
        rankText.textContent = `${tierCap} • 820 LP`;
      }

      playSynthChime(780);

      if (mockToast) {
        mockToast.className = 'mock-toast active step-pulse';
        mockToast.textContent = isTR ? `✨ ${name} profili önizlemeye yüklendi!` : `✨ ${name} loaded into preview!`;
        setTimeout(function () {
          mockToast.classList.remove('active', 'step-pulse');
        }, 2200);
      }
    }
  }

  if (simApplyBtn) {
    simApplyBtn.addEventListener('click', applyPersonalizedMockup);
  }

  if (simRiotId) {
    simRiotId.addEventListener('keydown', function (e) {
      if (e.key === 'Enter') {
        e.preventDefault();
        applyPersonalizedMockup();
      }
    });
  }

  // PWA Service Worker Registration
  if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
      const isTR = window.location.pathname.endsWith('/tr/') || window.location.pathname.endsWith('/tr/index.html');
      const swPath = isTR ? '../sw.js' : 'sw.js';
      navigator.serviceWorker.register(swPath).catch(function () {});
    });
  }

})();
