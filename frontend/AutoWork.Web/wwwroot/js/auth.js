const Auth = {
  REMEMBER_KEY: 'autowork_remember_login',

  saveSession(data) {
    localStorage.setItem('accessToken', data.accessToken || data.AccessToken || '');
    localStorage.setItem('refreshToken', data.refreshToken || data.RefreshToken || '');
    localStorage.setItem('expiresAt', data.expiresAt || data.ExpiresAt || '');
    localStorage.setItem('user', JSON.stringify({
      userId: data.userId || data.UserId,
      email: data.email || data.Email,
      firstName: data.firstName || data.FirstName,
      lastName: data.lastName || data.LastName,
      phone: data.phone || data.Phone || '',
      avatarUrl: data.avatarUrl || data.AvatarUrl || '',
      roles: data.roles || data.Roles || []
    }));
  },

  updateUser(partial) {
    const user = this.getUser() || {};
    Object.assign(user, partial);
    localStorage.setItem('user', JSON.stringify(user));
  },

  saveRememberLogin(email, password) {
    localStorage.setItem(this.REMEMBER_KEY, JSON.stringify({
      email: (email || '').trim(),
      password: password || '',
      remember: true
    }));
  },

  loadRememberLogin() {
    const saved = this.getRememberLogin();
    if (!saved?.remember || !saved.email) return null;
    return {
      email: saved.email,
      password: saved.password || ''
    };
  },

  getRememberLogin() {
    try { return JSON.parse(localStorage.getItem(this.REMEMBER_KEY) || 'null'); }
    catch { return null; }
  },

  clearRememberLogin() {
    localStorage.removeItem(this.REMEMBER_KEY);
  },

  getUser() {
    try { return JSON.parse(localStorage.getItem('user') || 'null'); }
    catch { return null; }
  },

  isLoggedIn() {
    const token = localStorage.getItem('accessToken');
    const expires = localStorage.getItem('expiresAt');
    if (!token || !expires) return false;
    const expiry = new Date(expires);
    return !Number.isNaN(expiry.getTime()) && expiry > new Date();
  },

  logout(redirect = '/login.html') {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expiresAt');
    localStorage.removeItem('user');
    window.location.href = redirect;
  },

  clearSession() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expiresAt');
    localStorage.removeItem('user');
  },

  requireAuth() {
    if (!this.isLoggedIn()) {
      window.location.href = '/login.html?return=' + encodeURIComponent(window.location.pathname);
      return false;
    }
    return true;
  },

  redirectIfLoggedIn(target = '/app/dashboard.html') {
    if (this.isLoggedIn()) window.location.href = target;
  },

  displayName() {
    const u = this.getUser();
    if (!u) return 'User';
    return [u.firstName, u.lastName].filter(Boolean).join(' ') || u.email;
  }
};
