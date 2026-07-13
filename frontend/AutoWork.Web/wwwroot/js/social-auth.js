let socialAuthConfigPromise;
let googleScriptPromise;
let facebookScriptPromise;
let googleInitialized = false;
let googleCredentialResolver = null;

async function socialLogin(provider, options = {}) {
  const redirectUrl = options.redirectUrl || "/app/dashboard.html";

  try {
    const config = await getSocialAuthConfig();
    let payload;

    if (provider === "google") {
      if (!config.googleClientId) {
        throw new Error("Thiếu Google Client ID trong cấu hình server.");
      }

      const idToken = await requestGoogleIdToken(config.googleClientId);
      payload = { idToken };
    } else if (provider === "facebook") {
      if (!config.facebookAppId) {
        throw new Error("Thiếu Facebook App ID trong cấu hình server.");
      }

      const accessToken = await requestFacebookAccessToken(
        config.facebookAppId,
        config.facebookApiVersion || "v19.0"
      );
      payload = { accessToken };
    } else {
      throw new Error("Nhà cung cấp social login không hợp lệ.");
    }

    const json = await apiPost(`/api/auth/social/${provider}`, payload);
    if (json.success && json.data) {
      Auth.saveSession(json.data);
      showToast?.(`Đăng nhập ${provider} thành công`, "success");
      setTimeout(() => {
        window.location.href = redirectUrl;
      }, 250);
      return;
    }

    alert(json.message || `Đăng nhập ${provider} thất bại`);
  } catch (error) {
    alert(error?.message || `Không thể đăng nhập ${provider}.`);
  }
}

async function getSocialAuthConfig() {
  if (!socialAuthConfigPromise) {
    socialAuthConfigPromise = (async () => {
      const json = await apiGet("/api/auth/social/config");
      if (!json.success || !json.data) {
        throw new Error(json.message || "Không tải được cấu hình social login.");
      }
      return json.data;
    })();
  }

  return socialAuthConfigPromise;
}

async function requestGoogleIdToken(clientId) {
  await ensureGoogleSdkLoaded();
  initGoogleIdentity(clientId);

  return new Promise((resolve, reject) => {
    if (googleCredentialResolver) {
      reject(new Error("Google login đang xử lý, vui lòng thử lại."));
      return;
    }

    const timeout = setTimeout(() => {
      if (googleCredentialResolver) {
        googleCredentialResolver = null;
        reject(new Error("Hết thời gian đăng nhập Google."));
      }
    }, 60000);

    googleCredentialResolver = (credential) => {
      clearTimeout(timeout);
      if (!credential) {
        reject(new Error("Không nhận được idToken từ Google."));
        return;
      }
      resolve(credential);
    };

    window.google.accounts.id.prompt((notification) => {
      if (!googleCredentialResolver) {
        return;
      }

      if (notification?.isNotDisplayed?.() || notification?.isSkippedMoment?.() || notification?.isDismissedMoment?.()) {
        const resolver = googleCredentialResolver;
        googleCredentialResolver = null;
        clearTimeout(timeout);
        resolver(null);
      }
    });
  });
}

function initGoogleIdentity(clientId) {
  if (googleInitialized) return;

  window.google.accounts.id.initialize({
    client_id: clientId,
    callback: (response) => {
      if (!googleCredentialResolver) return;
      const resolver = googleCredentialResolver;
      googleCredentialResolver = null;
      resolver(response?.credential || null);
    },
    auto_select: false,
    cancel_on_tap_outside: true
  });

  googleInitialized = true;
}

async function ensureGoogleSdkLoaded() {
  if (!googleScriptPromise) {
    googleScriptPromise = loadScriptOnce("https://accounts.google.com/gsi/client", "google-identity-sdk");
  }
  await googleScriptPromise;

  if (!window.google?.accounts?.id) {
    throw new Error("Google Identity SDK chưa sẵn sàng.");
  }
}

async function requestFacebookAccessToken(appId, apiVersion) {
  await ensureFacebookSdkLoaded(appId, apiVersion);

  return new Promise((resolve, reject) => {
    window.FB.login((response) => {
      if (!response || !response.authResponse?.accessToken) {
        reject(new Error("Đăng nhập Facebook bị hủy hoặc thất bại."));
        return;
      }

      resolve(response.authResponse.accessToken);
    }, { scope: "public_profile,email" });
  });
}

async function ensureFacebookSdkLoaded(appId, apiVersion) {
  if (!facebookScriptPromise) {
    facebookScriptPromise = new Promise((resolve, reject) => {
      window.fbAsyncInit = function () {
        try {
          window.FB.init({
            appId,
            cookie: true,
            xfbml: false,
            version: apiVersion
          });
          resolve();
        } catch (err) {
          reject(err);
        }
      };

      loadScriptOnce("https://connect.facebook.net/en_US/sdk.js", "facebook-sdk").catch(reject);
    });
  }

  await facebookScriptPromise;
}

function loadScriptOnce(src, id) {
  return new Promise((resolve, reject) => {
    const existing = document.getElementById(id);
    if (existing) {
      if (existing.dataset.loaded === "true") {
        resolve();
        return;
      }

      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error(`Không tải được script: ${src}`)), { once: true });
      return;
    }

    const script = document.createElement("script");
    script.id = id;
    script.src = src;
    script.async = true;
    script.defer = true;
    script.onload = () => {
      script.dataset.loaded = "true";
      resolve();
    };
    script.onerror = () => reject(new Error(`Không tải được script: ${src}`));
    document.head.appendChild(script);
  });
}
