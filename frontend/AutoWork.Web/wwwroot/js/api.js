let API_BASE = 'https://localhost:7165';
let API_PREFIX = '';
const apiReady = (async () => {
  try {
    const res = await fetch('/api/config');
    if (res.ok) {
      const cfg = await res.json();
      if (cfg.useProxy) {
        API_BASE = '';
        API_PREFIX = '/backend-api';
      } else if (cfg.apiBaseUrl) {
        API_BASE = cfg.apiBaseUrl.replace(/\/$/, '');
        API_PREFIX = '';
      }
    }
  } catch { /* use default */ }
})();

function apiUrl(path) {
  return `${API_BASE}${API_PREFIX}${path}`;
}

async function apiFetch(path, options = {}) {
  await apiReady;

  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  const token = localStorage.getItem('accessToken');
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(apiUrl(path), { ...options, headers });

  if (res.status === 401 && token) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      headers['Authorization'] = `Bearer ${localStorage.getItem('accessToken')}`;
      return fetch(apiUrl(path), { ...options, headers });
    }
    Auth.logout('/login.html');
    return res;
  }

  return res;
}

async function tryRefreshToken() {
  const accessToken = localStorage.getItem('accessToken');
  const refreshToken = localStorage.getItem('refreshToken');
  if (!accessToken || !refreshToken) return false;

  try {
    await apiReady;
    const res = await fetch(apiUrl('/api/auth/refresh'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ accessToken, refreshToken })
    });
    if (!res.ok) return false;
    const json = await res.json();
    if (json.success && json.data) {
      Auth.saveSession(json.data);
      return true;
    }
  } catch { /* ignore */ }
  return false;
}

async function apiGet(path) {
  const res = await apiFetch(path);
  return res.json();
}

async function apiPost(path, body) {
  const res = await apiFetch(path, { method: 'POST', body: JSON.stringify(body) });
  return res.json();
}

async function apiPut(path, body) {
  const res = await apiFetch(path, { method: 'PUT', body: JSON.stringify(body) });
  return res.json();
}

async function apiDelete(path) {
  const res = await apiFetch(path, { method: 'DELETE' });
  return res.json();
}

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function formatDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('vi-VN');
}

function showToast(message, type = 'info') {
  const el = document.getElementById('toast');
  if (!el) return;
  el.className = `toast-msg toast-${type} show`;
  el.textContent = message;
  setTimeout(() => el.classList.remove('show'), 3500);
}

function postStatusLabel(status) {
  const map = { 0: 'Nháp', 1: 'Đã lên lịch', 2: 'Đã đăng', 3: 'Lỗi', 4: 'Đang xử lý' };
  return map[status] ?? `Trạng thái ${status}`;
}

function postStatusClass(status) {
  const map = { 0: 'badge-secondary', 1: 'badge-warning', 2: 'badge-success', 3: 'badge-danger', 4: 'badge-info' };
  return map[status] ?? 'badge-secondary';
}
