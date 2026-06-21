const DEMO = {
  PROJECT_ID: '13131313-1313-1313-1313-131313131313',
  CHANNEL_FB: '14141414-1414-1414-1414-141414141414',
  CHANNEL_WP: '15151515-1515-1515-1515-151515151515',
  CHANNEL_ZALO: '16161616-1616-1616-1616-161616161616',
  STORAGE_KEY: 'autowork_demo_state'
};

const DemoStore = {
  load() {
    try { return JSON.parse(localStorage.getItem(DEMO.STORAGE_KEY) || '{}'); }
    catch { return {}; }
  },
  save(state) {
    localStorage.setItem(DEMO.STORAGE_KEY, JSON.stringify(state));
  },
  get(key, fallback = null) {
    return this.load()[key] ?? fallback;
  },
  set(key, value) {
    const s = this.load();
    s[key] = value;
    this.save(s);
  },
  getChannels() {
    return this.get('channels', { facebook: false, wordpress: false, zalo: false });
  },
  connectChannel(name) {
    const ch = this.getChannels();
    ch[name] = true;
    this.set('channels', ch);
  },
  getNotifications() {
    return this.get('notifications', []);
  },
  addNotification(title, message, type = 'info') {
    const list = this.getNotifications();
    list.unshift({
      id: crypto.randomUUID(),
      title, message, type,
      isRead: false,
      createdAt: new Date().toISOString()
    });
    this.set('notifications', list.slice(0, 50));
  },
  markRead(id) {
    const list = this.getNotifications().map(n =>
      n.id === id ? { ...n, isRead: true } : n);
    this.set('notifications', list);
  },
  markAllRead() {
    this.set('notifications', this.getNotifications().map(n => ({ ...n, isRead: true })));
  },
  unreadCount() {
    return this.getNotifications().filter(n => !n.isRead).length;
  }
};

function isAdmin() {
  const u = Auth.getUser();
  return u?.roles?.some(r => r === 'SuperAdmin' || r === 'Admin') ?? false;
}

function showModal(title, bodyHtml, footerHtml = '') {
  const existing = document.getElementById('demoModal');
  if (existing) existing.remove();

  const overlay = document.createElement('div');
  overlay.id = 'demoModal';
  overlay.className = 'modal-overlay';
  overlay.innerHTML = `
    <div class="modal-card animate-fade-in">
      <div class="modal-header">
        <h3>${title}</h3>
        <button class="modal-close" onclick="closeModal()">&times;</button>
      </div>
      <div class="modal-body">${bodyHtml}</div>
      ${footerHtml ? `<div class="modal-footer">${footerHtml}</div>` : ''}
    </div>`;
  overlay.addEventListener('click', e => { if (e.target === overlay) closeModal(); });
  document.body.appendChild(overlay);
}

function closeModal() {
  document.getElementById('demoModal')?.remove();
}

function confirmAction(message, onConfirm) {
  showModal('Xác nhận', `<p>${message}</p>`,
    `<button class="btn-outline btn-sm" onclick="closeModal()">Hủy</button>
     <button class="btn-primary btn-sm" id="modalConfirmBtn">Xác nhận</button>`);
  document.getElementById('modalConfirmBtn')?.addEventListener('click', async () => {
    closeModal();
    await onConfirm();
  });
}

function mockAiContent(topic, channel = 'facebook') {
  const templates = {
    facebook: `🔥 ${topic} — Ưu đãi có hạn!\n\nBộ sương mù mới vừa về kho với chất liệu cao cấp, form dáng chuẩn. Giảm ngay 20% cho 50 khách đầu tiên!\n\n👉 Inbox ngay để được tư vấn size\n📦 Freeship đơn từ 500K\n\n#thoitrang #${topic.replace(/\s+/g, '')} #sale #autowork`,
    wordpress: `<h2>${topic}: Xu hướng nổi bật 2026</h2>\n<p>Trong bài viết này, chúng tôi phân tích chi tiết về <strong>${topic}</strong> — từ nguyên liệu, phong cách đến cách phối đồ thông minh.</p>\n<p>Meta: Tối ưu SEO với từ khóa "${topic}", giúp website lên top Google nhanh hơn.</p>`,
    zalo: `Xin chào! Shop có tin vui về ${topic} 🎉\nGiảm 20% tuần này — nhắn "MUA" để nhận ưu đãi nhé!`,
    seo: `Tiêu đề: ${topic} — Hướng dẫn chi tiết 2026\n\nMô tả meta: Khám phá ${topic} với bí quyết từ chuyên gia.\n\nNội dung đầy đủ 800 từ, chuẩn SEO, internal links...`
  };
  return {
    title: `${topic} — AutoWork AI Demo`,
    content: templates[channel] || templates.facebook,
    hashtags: `#${topic.replace(/\s+/g, '')} #marketing #AI #AutoWork`,
    creditsUsed: 5,
    tokensUsed: 420,
    isDemo: true
  };
}

async function tryApiOrDemo(apiFn, demoFn, successMsg) {
  try {
    const result = await apiFn();
    if (result?.success === false) throw new Error(result.message);
    if (successMsg) showToast(successMsg, 'success');
    return result;
  } catch (e) {
    const demo = demoFn();
    showToast(`Demo: ${e.message || 'API chưa cấu hình'} — hiển thị kết quả mô phỏng`, 'info');
    return { success: true, data: demo, isDemo: true };
  }
}

function renderChannelBadge(name, connected) {
  const icons = { facebook: 'bi-facebook', wordpress: 'bi-wordpress', zalo: 'bi-chat-dots' };
  const labels = { facebook: 'Facebook', wordpress: 'WordPress', zalo: 'Zalo OA' };
  return `<span class="channel-badge ${connected ? 'connected' : ''}">
    <i class="bi ${icons[name]}"></i> ${labels[name]}
    ${connected ? '<i class="bi bi-check-circle-fill"></i>' : ''}
  </span>`;
}

function btn(icon, label, onclick, cls = 'btn-outline btn-sm') {
  return `<button class="${cls}" onclick="${onclick}"><i class="bi ${icon}"></i> ${label}</button>`;
}
