function renderAppLayout(activePage) {
  const user = Auth.getUser();
  const nav = [
    { href: '/app/dashboard.html', icon: 'bi-speedometer2', label: 'Dashboard' },
    { href: '/app/posts.html', icon: 'bi-file-post', label: 'Bài viết' },
    { href: '/app/credits.html', icon: 'bi-coin', label: 'Credits' },
    { href: '/app/plans.html', icon: 'bi-gem', label: 'Gói dịch vụ' }
  ];

  document.body.innerHTML = `
    <div class="app-shell">
      <aside class="sidebar">
        <a href="/" class="sidebar-brand">
          <span class="brand-icon">⚡</span>
          <span>AutoWork</span>
        </a>
        <nav class="sidebar-nav">
          ${nav.map(n => `
            <a href="${n.href}" class="sidebar-link ${activePage === n.href ? 'active' : ''}">
              <i class="bi ${n.icon}"></i> ${n.label}
            </a>`).join('')}
        </nav>
        <div class="sidebar-footer">
          <div class="user-chip">
            <div class="user-avatar">${(user?.firstName?.[0] || user?.email?.[0] || 'U').toUpperCase()}</div>
            <div>
              <div class="user-name">${Auth.displayName()}</div>
              <div class="user-email">${user?.email || ''}</div>
            </div>
          </div>
          <button class="btn btn-outline-light btn-sm w-100 mt-2" id="btnLogout">
            <i class="bi bi-box-arrow-right"></i> Đăng xuất
          </button>
        </div>
      </aside>
      <main class="app-main">
        <header class="app-header">
          <h1 id="pageTitle" class="page-title"></h1>
          <div id="headerActions"></div>
        </header>
        <div id="pageContent" class="page-content animate-fade-in"></div>
      </main>
    </div>
    <div id="toast" class="toast-msg"></div>`;

  document.getElementById('btnLogout')?.addEventListener('click', () => Auth.logout());
}

function setPageTitle(title, actionsHtml = '') {
  const t = document.getElementById('pageTitle');
  const a = document.getElementById('headerActions');
  if (t) t.textContent = title;
  if (a) a.innerHTML = actionsHtml;
}

function setPageContent(html) {
  const el = document.getElementById('pageContent');
  if (el) el.innerHTML = html;
}
