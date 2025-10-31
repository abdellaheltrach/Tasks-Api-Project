// API base
const API_URL = "https://localhost:7252/api/Task";

// UI elements
const taskForm = document.getElementById('taskForm');
const tasksList = document.getElementById('tasksList');

// Helper: get access token
function getAccessToken() {
  return localStorage.getItem("token");
}

// Helper: call API with automatic token refresh
async function apiFetch(url, options = {}) {
  options.headers = options.headers || {};
  const token = getAccessToken();
  if (token) {
    options.headers['Authorization'] = `Bearer ${token}`;
  }
  options.credentials = 'include'; // include cookies for refresh token

  let res = await fetch(url, options);

  // If access token expired, try refresh
  if (res.status === 401) {
    const newToken = await refreshToken();
    if (!newToken) throw new Error("Session expired");

    options.headers['Authorization'] = `Bearer ${newToken}`;
    res = await fetch(url, options); // retry
  }

  return res;
}

// Refresh access token
async function refreshToken() {
  try {
    const res = await fetch("https://localhost:7252/api/auth/refresh", {
      method: "POST",
      credentials: "include" // send HttpOnly refresh cookie
    });

    if (!res.ok) {
      clearAuth();
      window.location.replace("/login.html");
      return null;
    }

    const data = await res.json();
    if (data && data.accessToken) {
      storeAuth(data.accessToken, data.role);
      return data.accessToken;
    }

    return null;
  } catch (err) {
    clearAuth();
    window.location.replace("/login.html");
    return null;
  }
}

// Fetch tasks on page load
document.addEventListener('DOMContentLoaded', () => {
  fetchTasks();
});

// Handle form submission
taskForm.addEventListener('submit', async (e) => {
  e.preventDefault();

  const task = {
    title: document.getElementById('title').value.trim(),
    description: document.getElementById('description').value.trim(),
    dueDate: document.getElementById('dueDate').value || null
    // taskStatusId: parseInt(document.getElementById('status').value) // optional
  };

  try {
    const res = await apiFetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task)
    });

    if (!res.ok) throw new Error('Failed to add task');

    taskForm.reset();
    fetchTasks();
  } catch (err) {
    alert(err.message);
  }
});

// Fetch all tasks
async function fetchTasks() {
  try {
    const res = await apiFetch(API_URL);
    if (!res.ok) throw new Error('Failed to fetch tasks');

    const tasks = await res.json();
    renderTasks(tasks);
  } catch (err) {
    console.error(err);
    tasksList.innerHTML = '<li>Error loading tasks</li>';
  }
}

// Render tasks
function renderTasks(tasks) {
  tasksList.innerHTML = '';

  if (!tasks || tasks.length === 0) {
    tasksList.innerHTML = '<li>No tasks yet</li>';
    return;
  }

  tasks.forEach(task => {
    const li = document.createElement('li');
    li.textContent = `${task.title} - ${task.description || ''} - Status: ${task.taskStatusId} - Due: ${task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'N/A'}`;
    tasksList.appendChild(li);
  });
}

// Store access token helper
function storeAuth(accessToken, role = null) {
  localStorage.setItem("token", accessToken);
  if (role) localStorage.setItem("role", role);

  const payload = parseJwt(accessToken);
  if (payload) {
    const userId = payload.nameid || payload.nameId || payload.sub;
    const username = payload.unique_name || payload.name;
    if (userId) localStorage.setItem("userId", userId);
    if (username) localStorage.setItem("username", username);
  }
}

// Clear auth helper
function clearAuth() {
  localStorage.removeItem("token");
  localStorage.removeItem("role");
  localStorage.removeItem("userId");
  localStorage.removeItem("username");
}

// JWT parsing helper
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
