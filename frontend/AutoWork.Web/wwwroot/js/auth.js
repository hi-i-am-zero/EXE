const Auth = {
  saveSession(data) {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('expiresAt', data.expiresAt);
    localStorage.setItem('user', JSON.stringify({
      userId: data.userId,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      roles: data.roles || []
    }));
  },

  getUser() {
    try { return JSON.parse(localStorage.getItem('user') || 'null'); }
    catch { return null; }
  },

  isLoggedIn() {
    const token = localStorage.getItem('accessToken');
    const expires = localStorage.getItem('expiresAt');
    if (!token || !expires) return false;
    return new Date(expires) > new Date();
  },

  logout(redirect = '/login.html') {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expiresAt');
    localStorage.removeItem('user');
    window.location.href = redirect;
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
