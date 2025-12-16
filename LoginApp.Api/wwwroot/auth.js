const API_URL = "/api/auth"; // relative path works for both http/https

// Elements
const flipCardInner = document.getElementById("flipCardInner");
const switchToRegister = document.getElementById("switchToRegister");
const switchToLogin = document.getElementById("switchToLogin");
const passwordToggles = document.querySelectorAll(".password-toggle");

// Helper: show error message
function showError(inputId, message) {
  let errorEl = document.getElementById(`${inputId}-error`);
  if (!errorEl) {
    const input = document.getElementById(inputId);
    errorEl = document.createElement("p");
    errorEl.id = `${inputId}-error`;
    errorEl.className = "text-red-600 text-sm mt-1";
    input.parentNode.appendChild(errorEl);
  }
  errorEl.textContent = message;
}

// Helper: clear all error messages
function clearErrors(formId) {
  const form = document.getElementById(formId);
  form.querySelectorAll("p.text-red-600").forEach((el) => el.remove());
}

// Helper: convert backend errors to user-friendly messages
function getFriendlyMessage(res, fallback) {
  if (res.status === 401) return "Invalid username or password.";
  if (res.status === 400) return "This username is already taken..";
  if (res.status === 409) return "User already exists.";
  return fallback || "Something went wrong. Try again.";
}

// ----------------------
// Flip card logic
// ----------------------
switchToRegister.addEventListener("click", (e) => {
  e.preventDefault();
  flipCardInner.classList.add("flipped");
});

switchToLogin.addEventListener("click", (e) => {
  e.preventDefault();
  flipCardInner.classList.remove("flipped");
});

// ----------------------
// Password visibility toggle
// ----------------------
passwordToggles.forEach((toggle) => {
  toggle.addEventListener("click", (e) => {
    e.preventDefault();
    const input = document.getElementById(toggle.dataset.input);
    const icon = toggle.querySelector("i");
    const hidden = input.type === "password";

    input.type = hidden ? "text" : "password";
    icon.classList.toggle("fa-eye", !hidden);
    icon.classList.toggle("fa-eye-slash", hidden);
  });
});

// ----------------------
// LOGIN FORM SUBMISSION
// ----------------------
document
  .getElementById("loginFormElement")
  .addEventListener("submit", async (e) => {
    e.preventDefault();
    clearErrors("loginFormElement");

    const identifier = document.getElementById("loginIdentifier").value.trim();
    const password = document.getElementById("loginPassword").value.trim();

    if (!identifier) return showError("loginIdentifier", "Username or Email is required");
    if (!password) return showError("loginPassword", "Password is required");

    try {
      const res = await fetch(`${API_URL}/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ identifier, password }),
      });

      if (!res.ok) {
        const text = await res.text().catch(() => null);
        return showError("loginIdentifier", getFriendlyMessage(res, text));
      }

      const data = await res.json();
      localStorage.setItem("token", data.accessToken);
      localStorage.setItem("identifier", identifier);

      // redirect to index page
      window.location.href = "/task.html";
    } catch (err) {
      console.error(err);
      showError("loginIdentifier", "Network error. Try again.");
    }
  });

// ----------------------
// REGISTER FORM SUBMISSION
// ----------------------
document
  .getElementById("registerFormElement")
  .addEventListener("submit", async (e) => {
    e.preventDefault();
    clearErrors("registerFormElement");

    const username = document.getElementById("registerName").value.trim();
    const password = document.getElementById("registerPassword").value;
    const confirm = document.getElementById("confirmPassword").value;

    if (!username) return showError("registerName", "Full name is required");
    if (!password) return showError("registerPassword", "Password is required");
    if (!confirm)
      return showError("confirmPassword", "Confirm password is required");
    if (password !== confirm) {
      document.getElementById("confirmPassword").value = ""; //clear field
      return showError("confirmPassword", "Passwords do not match");
    }

    try {
      const res = await fetch(`${API_URL}/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });

      if (!res.ok) {
        const text = await res.text().catch(() => null);
        return showError("registerName", getFriendlyMessage(res, text));
      }

      // Success: flip back to login
      flipCardInner.classList.remove("flipped");
      clearErrors("registerFormElement");
      showError("loginUsername", "Account created successfully! Please login.");
    } catch (err) {
      console.error(err);
      showError("registerName", "Network error. Try again.");
    }
  });
