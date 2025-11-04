
    // ===== CONFIGURATION =====
    const API_BASE_URL = 'https://localhost:7252/api'; // Update this URL
    
    let allTasks = [];
    let editingTaskId = null;

    // DOM Elements
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const profileTrigger = document.getElementById('profileTrigger');
    const profileDropdown = document.getElementById('profileDropdown');
    const addTaskBtn = document.getElementById('addTaskBtn');
    const taskModal = document.getElementById('taskModal');
    const modalClose = document.getElementById('modalClose');
    const cancelBtn = document.getElementById('cancelBtn');
    const taskForm = document.getElementById('taskForm');
    const tasksGrid = document.getElementById('tasksGrid');
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const toast = document.getElementById('toast');
    const logoutBtn = document.getElementById('logoutBtn');
    const userName = document.getElementById('userName');
    const userAvatar = document.getElementById('userAvatar');

    // ===== AUTH HELPERS =====
    function getToken() {
      return localStorage.getItem('token');
    }

    function getUsername() {
      return localStorage.getItem('username') || 'Guest';
    }

    function setAuthData(token, username) {
      localStorage.setItem('token', token);
      localStorage.setItem('username', username);
    }

    function clearAuthData() {
      localStorage.removeItem('token');
      localStorage.removeItem('username');
    }

    function updateUserProfile() {
      const username = getUsername();
      userName.textContent = username;
      userAvatar.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(username)}&background=000&color=fff`;
    }

    // ===== API HELPERS =====
    async function apiCall(method, endpoint, body = null) {
      const token = getToken();

      if (!token) {
        redirectToLogin();
        return null;
      }

      const options = {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        }
      };

      if (body) {
        options.body = JSON.stringify(body);
      }

      try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, options);

        if (response.status === 401) {
          clearAuthData();
          redirectToLogin();
          return null;
        }

        if (!response.ok) {
          const errorData = await response.json().catch(() => ({ message: `HTTP ${response.status}` }));
          throw new Error(errorData.message || `HTTP ${response.status}`);
        }

        // Handle 200 OK with empty body
        if (response.status === 200 && response.headers.get('content-length') === '0') {
          return { success: true };
        }

        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
          return await response.json();
        }

        return { success: true };
      } catch (error) {
        console.error('API Error:', error);
        showToast(`Error: ${error.message}`, 'error');
        return null;
      }
    }

    function redirectToLogin() {
      window.location.href = './auth.html';
    }

    // ===== STATUS MAPPING =====
    function getStatusId(statusString) {
      const statusMap = {
        'pending': 1,
        'inprogress': 2,
        'done': 3
      };
      return statusMap[statusString] || 1;
    }

    function getStatusString(statusId) {
      const statusMap = {
        1: 'pending',
        2: 'inprogress',
        3: 'done'
      };
      return statusMap[statusId] || 'pending';
    }

    // ===== TASK FUNCTIONS =====
    async function loadTasks() {
      const data = await apiCall('GET', '/task');
      if (Array.isArray(data)) {
        // Convert TaskStatusId to status string for frontend
        allTasks = data.map(task => ({
          ...task,
          status: getStatusString(task.taskStatusId)
        }));
        renderTasks();
        updateStats();
      }
    }

    async function createTask(taskData) {
      const payload = {
        title: taskData.title,
        description: taskData.description,
        taskStatusId: getStatusId(taskData.status),
        dueDate: taskData.dueDate ? new Date(taskData.dueDate).toISOString() : null
      };

      const response = await apiCall('POST', '/task', payload);
      if (response !== null) {
        showToast('Task added successfully!');
        closeModal();
        await loadTasks();
        return true;
      }
      return false;
    }

    async function updateTask(id, taskData) {
      const payload = {
        title: taskData.title,
        description: taskData.description,
        taskStatusId: getStatusId(taskData.status),
        dueDate: taskData.dueDate ? new Date(taskData.dueDate).toISOString() : null
      };

      const response = await apiCall('PUT', `/task/${id}`, payload);
      if (response !== null) {
        showToast('Task updated successfully!');
        closeModal();
        await loadTasks();
        return true;
      }
      return false;
    }

    async function deleteTask(id) {
      if (!confirm('Are you sure you want to delete this task?')) return;

      const response = await apiCall('DELETE', `/task/${id}`);
      if (response !== null) {
        allTasks = allTasks.filter(t => t.id !== id);
        showToast('Task deleted successfully!');
        renderTasks();
        updateStats();
      }
    }

    function renderTasks() {
      const searchTerm = searchInput.value.toLowerCase();
      const filterStatus = statusFilter.value;

      let filteredTasks = allTasks.filter(task => {
        const matchesSearch = (task.title?.toLowerCase().includes(searchTerm) || false) || 
                            (task.description?.toLowerCase().includes(searchTerm) || false);
        const matchesFilter = filterStatus === 'all' || task.status === filterStatus;
        return matchesSearch && matchesFilter;
      });

      if (filteredTasks.length === 0) {
        tasksGrid.innerHTML = `
          <div class="col-span-full text-center py-16 text-gray-400">
            <i class="fas fa-inbox text-6xl mb-4 opacity-50"></i>
            <p class="text-lg mb-6">${allTasks.length === 0 ? 'No tasks yet. Click the + button to add one!' : 'No tasks match your filters.'}</p>
          </div>
        `;
        return;
      }

      const statusColors = {
        pending: 'bg-yellow-100 text-yellow-800',
        inprogress: 'bg-blue-100 text-blue-800',
        done: 'bg-green-100 text-green-800'
      };

      tasksGrid.innerHTML = filteredTasks.map(task => `
        <div onclick="handleEditTask(${task.id})" class="bg-white rounded-xl p-6 shadow-sm hover:-translate-y-1 hover:shadow-md transition cursor-pointer border-l-4 border-black">
          <div class="flex justify-between items-start mb-4">
            <div class="flex-1">
              <h3 class="text-lg font-semibold mb-2">${escapeHtml(task.title)}</h3>
              <p class="text-gray-500 text-sm">${escapeHtml(task.description || 'No description')}</p>
            </div>
            <button onclick="event.stopPropagation(); handleDeleteTask(${task.id})" class="text-gray-400 hover:text-red-600 transition ml-2">
              <i class="fas fa-trash"></i>
            </button>
          </div>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2 text-sm text-gray-500">
              <i class="fas fa-calendar"></i>
              <span>${task.dueDate ? formatDate(task.dueDate) : 'No date'}</span>
            </div>
            <span class="px-3 py-1 rounded-full text-xs font-semibold uppercase ${statusColors[task.status] || 'bg-gray-100 text-gray-800'}">
              ${task.status === 'inprogress' ? 'In Progress' : task.status}
            </span>
          </div>
        </div>
      `).join('');
    }

    function updateStats() {
      const pending = allTasks.filter(t => t.status === 'pending').length;
      const inProgress = allTasks.filter(t => t.status === 'inprogress').length;
      const done = allTasks.filter(t => t.status === 'done').length;

      document.getElementById('pendingCount').textContent = pending;
      document.getElementById('inProgressCount').textContent = inProgress;
      document.getElementById('doneCount').textContent = done;
      document.getElementById('totalCount').textContent = allTasks.length;
    }

    // ===== UI FUNCTIONS =====
    function formatDate(dateString) {
      try {
        const date = new Date(dateString);
        const options = { month: 'short', day: 'numeric', year: 'numeric' };
        return date.toLocaleDateString('en-US', options);
      } catch {
        return 'Invalid date';
      }
    }

    function escapeHtml(text) {
      const div = document.createElement('div');
      div.textContent = text;
      return div.innerHTML;
    }

    function showToast(message, type = 'success') {
      const toastEl = document.getElementById('toast');
      const toastMsg = document.getElementById('toastMessage');
      const icon = toastEl.querySelector('i');

      toastMsg.textContent = message;

      if (type === 'error') {
        toastEl.classList.remove('border-green-500');
        toastEl.classList.add('border-red-500');
        icon.classList.remove('fa-check-circle', 'text-green-500');
        icon.classList.add('fa-exclamation-circle', 'text-red-500');
      } else {
        toastEl.classList.remove('border-red-500');
        toastEl.classList.add('border-green-500');
        icon.classList.remove('fa-exclamation-circle', 'text-red-500');
        icon.classList.add('fa-check-circle', 'text-green-500');
      }

      toastEl.classList.remove('opacity-0', 'translate-y-24');

      setTimeout(() => {
        toastEl.classList.add('opacity-0', 'translate-y-24');
      }, 3000);
    }

    function closeModal() {
      taskModal.classList.add('opacity-0', 'invisible');
      taskModal.querySelector('.bg-white').classList.add('-translate-y-5');
      editingTaskId = null;
      taskForm.reset();
    }

    async function handleEditTask(id) {
      const task = allTasks.find(t => t.id === id);
      if (!task) return;

      editingTaskId = id;
      document.getElementById('modalTitle').textContent = 'Edit Task';
      document.getElementById('taskTitle').value = task.title;
      document.getElementById('taskDescription').value = task.description || '';
      document.getElementById('taskStatus').value = task.status;
      document.getElementById('taskDate').value = task.dueDate ? task.dueDate.split('T')[0] : '';
      taskModal.classList.remove('opacity-0', 'invisible');
      taskModal.querySelector('.bg-white').classList.remove('-translate-y-5');
    }

    async function handleDeleteTask(id) {
      await deleteTask(id);
    }

    // ===== EVENT LISTENERS =====
    sidebarToggle?.addEventListener('click', () => {
      sidebar.classList.toggle('w-64');
      sidebar.classList.toggle('w-20');
      const spans = sidebar.querySelectorAll('span');
      spans.forEach(span => span.classList.toggle('hidden'));
    });

    profileTrigger.addEventListener('click', (e) => {
      e.stopPropagation();
      profileDropdown.classList.toggle('opacity-0');
      profileDropdown.classList.toggle('invisible');
      profileDropdown.classList.toggle('-translate-y-2');
    });

    document.addEventListener('click', (e) => {
      if (!profileDropdown.contains(e.target) && !profileTrigger.contains(e.target)) {
        profileDropdown.classList.add('opacity-0', 'invisible', '-translate-y-2');
      }
    });

    addTaskBtn.addEventListener('click', () => {
      editingTaskId = null;
      document.getElementById('modalTitle').textContent = 'New Task';
      taskForm.reset();
      taskModal.classList.remove('opacity-0', 'invisible');
      taskModal.querySelector('.bg-white').classList.remove('-translate-y-5');
    });

    modalClose.addEventListener('click', closeModal);
    cancelBtn.addEventListener('click', closeModal);
    taskModal.addEventListener('click', (e) => {
      if (e.target === taskModal) closeModal();
    });

    taskForm.addEventListener('submit', async (e) => {
      e.preventDefault();

      const taskData = {
        title: document.getElementById('taskTitle').value.trim(),
        description: document.getElementById('taskDescription').value.trim(),
        status: document.getElementById('taskStatus').value,
        dueDate: document.getElementById('taskDate').value
      };

      if (!taskData.title) {
        showToast('Task title is required', 'error');
        return;
      }

      const submitBtn = document.getElementById('submitBtn');
      submitBtn.disabled = true;
      submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Saving...';

      try {
        if (editingTaskId) {
          await updateTask(editingTaskId, taskData);
        } else {
          await createTask(taskData);
        }
      } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = 'Save Task';
      }
    });

    searchInput.addEventListener('input', renderTasks);
    statusFilter.addEventListener('change', renderTasks);

    logoutBtn.addEventListener('click', async (e) => {
      e.preventDefault();
      
      const username = getUsername();
      const deviceId = localStorage.getItem('deviceId') || '';
      const deviceName = localStorage.getItem('deviceName') || 'Web App';

      const logoutPayload = {
        username: username,
        deviceId: deviceId,
        deviceName: deviceName
      };

      const response = await apiCall('POST', '/auth/logout', logoutPayload);
      if (response !== null) {
        clearAuthData();
        localStorage.removeItem('deviceId');
        localStorage.removeItem('deviceName');
        showToast('Logged out successfully!');
        setTimeout(() => {
          redirectToLogin();
        }, 1000);
      }
    });

    // Global functions for inline onclick handlers
    window.handleEditTask = handleEditTask;
    window.handleDeleteTask = handleDeleteTask;

    // ===== INITIALIZE =====
    async function init() {
      const token = getToken();
      if (!token) {
        redirectToLogin();
        return;
      }

      updateUserProfile();
      await loadTasks();
    }

    init();