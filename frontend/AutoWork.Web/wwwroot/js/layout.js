function renderAppLayout(activePage) {
  const user = Auth.getUser();
  const unread = typeof DemoStore !== 'undefined' ? DemoStore.unreadCount() : 0;

  const nav = [
    { section: 'Tổng quan' },
    { href: '/app/dashboard.html', icon: 'bi-speedometer2', label: 'Dashboard' },
    { section: 'Nội dung' },
    { href: '/app/ai.html', icon: 'bi-robot', label: 'AI Content' },
    { href: '/app/posts.html', icon: 'bi-file-post', label: 'Bài viết' },
    { href: '/app/schedules.html', icon: 'bi-calendar-event', label: 'Lên lịch' },
    { href: '/app/media.html', icon: 'bi-images', label: 'Thư viện Media' },
    { section: 'Kênh' },
    { href: '/app/channels.html', icon: 'bi-share', label: 'Kết nối kênh' },
    { section: 'Tài chính' },
    { href: '/app/credits.html', icon: 'bi-coin', label: 'Credits' },
    { href: '/app/plans.html', icon: 'bi-gem', label: 'Gói dịch vụ' },
    { href: '/app/affiliate.html', icon: 'bi-people', label: 'Affiliate' },
    { section: 'Hệ thống' },
    { href: '/app/notifications.html', icon: 'bi-bell', label: 'Thông báo', badge: unread },
    { href: '/app/settings.html', icon: 'bi-gear', label: 'Cài đặt' },
  ];

  if (typeof isAdmin === 'function' && isAdmin()) {
    nav.push({ href: '/app/admin.html', icon: 'bi-shield-lock', label: 'Admin' });
  }

  document.body.innerHTML = `
    <div class="app-shell">
      <aside class="sidebar">
        <a href="/" class="sidebar-brand">
          <span class="brand-icon">⚡</span>
          <span>AutoWork</span>
          <span class="demo-pill">DEMO</span>
        </a>
        <nav class="sidebar-nav">
          ${nav.map(n => n.section
            ? `<div class="nav-section">${n.section}</div>`
            : `<a href="${n.href}" class="sidebar-link ${activePage === n.href ? 'active' : ''}">
                <i class="bi ${n.icon}"></i> ${n.label}
                ${n.badge ? `<span class="nav-badge">${n.badge}</span>` : ''}
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
        <div class="demo-banner">
          <i class="bi bi-info-circle"></i>
          Bản <strong>DEMO</strong> tương tác — trải nghiệm đầy đủ nút chức năng. Một số tính năng dùng dữ liệu mô phỏng khi API chưa cấu hình.
        </div>
        <header class="app-header">
          <h1 id="pageTitle" class="page-title"></h1>
          <div id="headerActions"></div>
        </header>
        <div id="pageContent" class="page-content animate-fade-in"></div>
      </main>
    </div>
    <div id="toast" class="toast-msg"></div>`;

  document.getElementById('btnLogout')?.addEventListener('click', () => Auth.logout('/login.html?fresh=1'));
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

function appScripts() {
  return `
    <script src="/js/api.js"><\/script>
    <script src="/js/auth.js"><\/script>
    <script src="/js/demo.js"><\/script>
    <script src="/js/layout.js"><\/script>`;
}
