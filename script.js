// Theme Toggle
const themeBtn = document.getElementById('themeToggle');
const htmlEl = document.documentElement;

// Sistem temasını kontrol et
const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
let currentTheme = localStorage.getItem('theme') || (prefersDark ? 'dark' : 'light');
htmlEl.setAttribute('data-theme', currentTheme);
updateThemeButtonText();

themeBtn.addEventListener('click', () => {
  currentTheme = currentTheme === 'dark' ? 'light' : 'dark';
  htmlEl.setAttribute('data-theme', currentTheme);
  localStorage.setItem('theme', currentTheme);
  updateThemeButtonText();
});

function updateThemeButtonText() {
  themeBtn.textContent = currentTheme === 'dark' ? '☀️ Açık Tema' : '🌙 Koyu Tema';
}
