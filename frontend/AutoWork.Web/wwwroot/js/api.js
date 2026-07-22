let API_BASE = 'https://localhost:7165';
let API_PREFIX = '';
let USE_PROXY = false;
const apiReady = (async () => {
  try {
    const res = await fetch('/api/config');
    if (res.ok) {
      const cfg = await res.json();
      if (cfg.useProxy) {
        USE_PROXY = true;
        API_BASE = '';
        API_PREFIX = '/backend-api';
      } else if (cfg.apiBaseUrl) {
        API_BASE = cfg.apiBaseUrl.replace(/\/$/, '');
        API_PREFIX = '';
      }
    }
  } catch { /* use default */ }
})();

function resolveMediaUrl(url) {
  if (!url) return '';
  if (/^https?:\/\//i.test(url)) return url;
  if (url.startsWith('/uploads/')) {
    if (USE_PROXY) return url;
    return `${API_BASE}${url}`;
  }
  return url;
}

function apiUrl(path) {
  return `${API_BASE}${API_PREFIX}${path}`;
}

function normalizeApiJson(json) {
  if (!json || typeof json !== 'object') return { success: false, message: 'Phản hồi không hợp lệ từ server.' };
  return {
    success: json.success ?? json.Success ?? false,
    message: json.message ?? json.Message,
    data: json.data ?? json.Data,
    errors: json.errors ?? json.Errors
  };
}

function formatApiError(json, fallback = 'Có lỗi xảy ra.') {
  const errors = json?.errors ?? json?.Errors;
  if (Array.isArray(errors) && errors.length) return errors.join(' ');
  const msg = json?.message ?? json?.Message;
  if (msg && msg !== 'Validation failed') return msg;
  return fallback;
}

function validatePasswordRules(password) {
  const issues = [];
  if (!password || password.length < 8) issues.push('ít nhất 8 ký tự');
  if (!/[A-Z]/.test(password)) issues.push('1 chữ in hoa');
  if (!/[a-z]/.test(password)) issues.push('1 chữ thường');
  if (!/[0-9]/.test(password)) issues.push('1 chữ số');
  return issues;
}

async function parseApiResponse(res) {
  const text = await res.text();
  if (!text) {
    return { success: res.ok, message: res.ok ? undefined : `Lỗi HTTP ${res.status}` };
  }

  try {
    return normalizeApiJson(JSON.parse(text));
  } catch {
    throw new Error(`Phản hồi không hợp lệ (HTTP ${res.status}). Hãy kiểm tra API đang chạy.`);
  }
}

async function apiFetch(path, options = {}) {
  await apiReady;

  const isPublicAuth = /^\/api\/auth\/(login|register|refresh|forgot-password|reset-password)/i.test(path);
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  const token = localStorage.getItem('accessToken');

  if (token && !isPublicAuth) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(apiUrl(path), { ...options, headers });

  if (res.status === 401 && token && !isPublicAuth) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      headers['Authorization'] = `Bearer ${localStorage.getItem('accessToken')}`;
      return fetch(apiUrl(path), { ...options, headers });
    }
    Auth.logout('/login.html?fresh=1');
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
    const json = await parseApiResponse(res);
    if (json.success && json.data) {
      Auth.saveSession(json.data);
      return true;
    }
  } catch { /* ignore */ }
  return false;
}

async function apiGet(path) {
  const res = await apiFetch(path);
  return parseApiResponse(res);
}

async function apiPost(path, body) {
  const res = await apiFetch(path, { method: 'POST', body: JSON.stringify(body) });
  return parseApiResponse(res);
}

async function apiPut(path, body) {
  const res = await apiFetch(path, { method: 'PUT', body: JSON.stringify(body) });
  return parseApiResponse(res);
}

async function apiDelete(path) {
  const res = await apiFetch(path, { method: 'DELETE' });
  return parseApiResponse(res);
}

async function apiUpload(path, file, query = '') {
  await apiReady;
  const form = new FormData();
  form.append('file', file);
  const headers = {};
  const token = localStorage.getItem('accessToken');
  if (token) headers['Authorization'] = `Bearer ${token}`;

  let res = await fetch(apiUrl(path + query), { method: 'POST', headers, body: form });

  if (res.status === 401 && token) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      headers['Authorization'] = `Bearer ${localStorage.getItem('accessToken')}`;
      res = await fetch(apiUrl(path + query), { method: 'POST', headers, body: form });
    } else {
      Auth.logout('/login.html?fresh=1');
      return parseApiResponse(res);
    }
  }

  return parseApiResponse(res);
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
