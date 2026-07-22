function renderUserAvatar(user, className = 'user-avatar') {
  const letter = (user?.firstName?.[0] || user?.email?.[0] || "U").toUpperCase();
  const url = typeof resolveMediaUrl === 'function' ? resolveMediaUrl(user?.avatarUrl) : user?.avatarUrl;
  if (url) {
    return `<img src="${url}" alt="" class="${className} user-avatar-img" onerror="this.onerror=null;this.outerHTML='<div class=&quot;${className}&quot;>${letter}</div>';" />`;
  }
  return `<div class="${className}">${letter}</div>`;
}

function renderAppLayout(activePage, options = {}) {
  const user = Auth.getUser();
  const unread = typeof DemoStore !== "undefined" ? DemoStore.unreadCount() : 0;
  const showUpgradeBanner = options.showUpgradeBanner !== false;

  const mainNav = [
    { href: "/app/dashboard.html", icon: "bi-house-door", label: "Trang chủ" },
    { href: "/app/workspace.html", icon: "bi-kanban", label: "Workspace & Task" },
    { href: "/app/brand-memory.html", icon: "bi-journal-richtext", label: "Brand Memory" },
    { href: "/app/schedules.html", icon: "bi-diagram-3", label: "Chiến dịch tự động" },
    { href: "/app/posts.html", icon: "bi-pencil-square", label: "Tạo bài viết đơn" },
    { href: "/app/ai.html", icon: "bi-stars", label: "Tạo ảnh AI" }
  ];

  const manageNav = [
    { href: "/app/channels.html", icon: "bi-share", label: "Kết nối kênh" },
    { href: "/app/media.html", icon: "bi-images", label: "Thư viện Media" },
    { href: "/app/plans.html", icon: "bi-gem", label: "Gói dịch vụ" },
    { href: "/app/credits.html", icon: "bi-coin", label: "Credits" },
    { href: "/app/affiliate.html", icon: "bi-people", label: "Affiliate" },
    { href: "/app/notifications.html", icon: "bi-bell", label: "Thông báo", badge: unread }
  ];

  if (typeof isAdmin === "function" && isAdmin()) {
    manageNav.push({ href: "/app/admin.html", icon: "bi-shield-lock", label: "Admin" });
  }

  document.body.innerHTML = `
    <div class="app-shell">
      <aside class="sidebar">
        <a href="/app/dashboard.html" class="sidebar-brand">
          <span class="brand-icon">⚡</span>
          <span>FlowMate</span>
        </a>

        <div class="sidebar-profile">
          <div class="user-chip">
            ${renderUserAvatar(user)}
            <div>
              <div class="user-name">${Auth.displayName()}</div>
              <div class="user-email">${user?.email || ""}</div>
            </div>
          </div>
        </div>

        <nav class="sidebar-nav">
          <div class="nav-section">Menu chính</div>
          ${mainNav.map(n => `
            <a href="${n.href}" class="sidebar-link ${activePage === n.href ? "active" : ""}">
              <i class="bi ${n.icon}"></i>
              <span>${n.label}</span>
            </a>`).join("")}

          <div class="nav-section">Hệ thống</div>
          ${manageNav.map(n => `
            <a href="${n.href}" class="sidebar-link ${activePage === n.href ? "active" : ""}">
              <i class="bi ${n.icon}"></i>
              <span>${n.label}</span>
              ${n.badge ? `<span class="nav-badge">${n.badge}</span>` : ""}
            </a>`).join("")}
        </nav>

        <div class="sidebar-footer">
          <a href="/app/settings.html" class="sidebar-link ${activePage === "/app/settings.html" ? "active" : ""}">
            <i class="bi bi-gear"></i>
            <span>Cài đặt</span>
          </a>
          <button class="btn-outline btn-sm w-100" id="btnLogout">
            <i class="bi bi-box-arrow-right"></i> Đăng xuất
          </button>
        </div>
      </aside>

      <main class="app-main">
        <header class="topbar">
          <div class="topbar-search">
            <i class="bi bi-search" style="color:#99a1b7"></i>
            <input type="text" placeholder="Tìm kiếm..." />
          </div>
          <div class="topbar-actions">
            <a href="/app/ai.html" class="btn-outline btn-sm"><i class="bi bi-robot"></i> Hỏi Flowmate AI</a>
            <a href="/app/posts.html" class="btn-outline btn-sm"><i class="bi bi-plus-circle"></i> Tạo nhanh</a>
            <a href="/app/notifications.html" class="topbar-icon-btn" title="Thông báo">
              <i class="bi bi-bell"></i>
              ${unread ? `<span class="topbar-badge">${unread}</span>` : ""}
            </a>
          </div>
        </header>

        ${showUpgradeBanner ? `
        <div class="workspace-banner">
          <span>Bạn đang dùng bản FREE với tính năng giới hạn. Nâng cấp để khai thác toàn bộ sức mạnh FlowMate.</span>
          <a href="/app/plans.html" class="btn-outline btn-sm">Xem và chọn gói</a>
        </div>` : ""}

        <section class="page-wrap">
          <header class="app-header">
            <h1 id="pageTitle" class="page-title"></h1>
            <div id="headerActions" class="header-actions"></div>
          </header>
          <div id="pageContent" class="page-content animate-fade-in"></div>
        </section>
      </main>
    </div>
    <div id="toast" class="toast-msg"></div>`;

  document.getElementById("btnLogout")?.addEventListener("click", () => Auth.logout("/login.html?fresh=1"));
}

function setPageTitle(title, actionsHtml = "") {
  const t = document.getElementById("pageTitle");
  const a = document.getElementById("headerActions");
  if (t) t.textContent = title;
  if (a) a.innerHTML = actionsHtml;
}

function setPageContent(html) {
  const el = document.getElementById("pageContent");
  if (el) el.innerHTML = html;
}

function refreshSidebarProfile() {
  const user = Auth.getUser();
  const chip = document.querySelector(".sidebar-profile .user-chip");
  if (!chip) return;
  chip.innerHTML = `
    ${renderUserAvatar(user)}
    <div>
      <div class="user-name">${Auth.displayName()}</div>
      <div class="user-email">${user?.email || ""}</div>
    </div>`;
}

function appScripts() {
  return `
    <script src="/js/api.js?v=3"><\/script>
    <script src="/js/auth.js?v=3"><\/script>
    <script src="/js/demo.js"><\/script>
    <script src="/js/layout.js?v=3"><\/script>`;
}
