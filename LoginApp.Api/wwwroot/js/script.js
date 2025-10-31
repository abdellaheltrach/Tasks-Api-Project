// API base
const API_URL = "https://localhost:7252/api/auth";

// UI elements
const resultEl = () => document.getElementById("result");
const signupResultEl = () => document.getElementById("signupResult");
const signupOverlay = () => document.getElementById("signupOverlay");

// helper: parse JWT payload safely
function parseJwt(token) {
  try {
    const payload = token.split(".")[1];
    const json = decodeURIComponent(
      atob(payload)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

// store token + optional meta
function storeAuth(accessToken, role = null) {
  localStorage.setItem("token", accessToken);
  if (role) localStorage.setItem("role", role);

  const payload = parseJwt(accessToken);
  if (payload) {
    const userId =
      payload.nameid ||
      payload.nameId ||
      payload.sub ||
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    const username =
      payload.unique_name ||
      payload.name ||
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
    if (userId) localStorage.setItem("userId", userId);
    if (username) localStorage.setItem("username", username);
  }
}

// clear auth
function clearAuth() {
  localStorage.removeItem("token");
  localStorage.removeItem("role");
  localStorage.removeItem("userId");
  localStorage.removeItem("username");
}

// Open signup modal
function openSignup() {
  const overlay = signupOverlay();
  overlay.classList.remove("hidden");
  overlay.setAttribute("aria-hidden", "false");
  document.getElementById("regUsername").value = "";
  document.getElementById("regPassword").value = "";
  document.getElementById("confirmPassword").value = "";
  signupResultEl().textContent = "";
  document.getElementById("regUsername").focus();
}

// Close signup modal
function closeSignup() {
  const overlay = signupOverlay();
  overlay.classList.add("hidden");
  overlay.setAttribute("aria-hidden", "true");
  signupResultEl().textContent = "";
}

// Close on backdrop click
document.addEventListener("click", (e) => {
  const overlay = signupOverlay();
  if (!overlay || overlay.classList.contains("hidden")) return;
  if (e.target === overlay) closeSignup();
});

// Close on modal-close button
document.getElementById?.("modalClose")?.addEventListener("click", (e) => {
  e.preventDefault();
  closeSignup();
});

// LOGIN
async function login() {
  const username = document.getElementById("username").value.trim();
  const password = document.getElementById("password").value.trim();
  const r = resultEl();

  r.className = "result";
  if (!username || !password) {
    r.textContent = "Please fill in both fields.";
    r.classList.add("error");
    return;
  }

  r.textContent = "Signing in…";

  // generate or reuse deviceId
  const deviceId = localStorage.getItem("deviceId") || crypto.randomUUID();
  const deviceName = navigator.userAgent || "Unknown Device";
  localStorage.setItem("deviceId", deviceId);

  try {
    const res = await fetch(`${API_URL}/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password, deviceId, deviceName }),
      credentials: "include", // include cookies for refresh token
    });

    if (!res.ok) {
      const txt = await res.text();
      r.textContent = txt || "Invalid credentials.";
      r.classList.add("error");
      return;
    }

    const data = await res.json();
    if (data && data.accessToken) {
      storeAuth(data.accessToken, data.role);
    }

    r.textContent = "Welcome back!";
    r.classList.add("ok");

    // redirect to tasks page
    window.location.replace("/tasks.html");
  } catch (err) {
    console.error(err);
    r.textContent = "Network error — try again.";
    r.classList.add("error");
  }
}

// SIGNUP
async function register() {
  const username = document.getElementById("regUsername").value.trim();
  const password = document.getElementById("regPassword").value;
  const confirm = document.getElementById("confirmPassword").value;
  const r = signupResultEl();

  r.className = "result";
  if (!username || !password || !confirm) {
    r.textContent = "All fields are required.";
    r.classList.add("error");
    return;
  }
  if (password.length < 6) {
    r.textContent = "Password must be at least 6 characters.";
    r.classList.add("error");
    return;
  }
  if (password !== confirm) {
    r.textContent = "Passwords do not match.";
    r.classList.add("error");
    return;
  }

  r.textContent = "Creating account…";
  try {
    const res = await fetch(`${API_URL}/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password }),
    });

    if (!res.ok) {
      const txt = await res.text();
      r.textContent = txt || "Registration failed.";
      r.classList.add("error");
      return;
    }

    r.textContent = "Account created. You can sign in now.";
    r.classList.add("ok");

    // Auto-close signup modal
    setTimeout(closeSignup, 1500);
  } catch (err) {
    console.error(err);
    r.textContent = "Network error — try again.";
    r.classList.add("error");
  }
}

// REFRESH TOKEN HANDLER (optional)
async function refreshToken() {
  try {
    const res = await fetch(`${API_URL}/refresh`, {
      method: "POST",
      credentials: "include", // send HttpOnly refresh cookie
    });

    if (!res.ok) throw new Error("Unable to refresh token");

    const data = await res.json();
    if (data && data.accessToken) {
      storeAuth(data.accessToken, data.role);
    }

    return data.accessToken;
  } catch (err) {
    clearAuth();
    window.location.replace("/login.html");
  }
}
