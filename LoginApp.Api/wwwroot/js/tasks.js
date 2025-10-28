// Replace this with your real JWT token after login
const token = localStorage.getItem('token') || 'YOUR_JWT_TOKEN_HERE';

const apiUrl = 'https://localhost:7252/api/Task'; // adjust port if needed
const taskForm = document.getElementById('taskForm');
const tasksList = document.getElementById('tasksList');

// Fetch tasks on load
document.addEventListener('DOMContentLoaded', () => {
    fetchTasks();
});

// Handle form submission


taskForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const task = {
        title: document.getElementById('title').value,
        description: document.getElementById('description').value,
        //dueDate: document.getElementById('dueDate').value || null,
        // taskStatusId: parseInt(document.getElementById('status').value)
    };

    try {
        const res = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(task)
        });

        if (!res.ok) throw new Error('Failed to add task');

        // Reset form
        taskForm.reset();
        fetchTasks();
    } catch (err) {
        alert(err.message);
    }
});

// Fetch all tasks
async function fetchTasks() {
    try {
        const res = await fetch(apiUrl, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!res.ok) throw new Error('Failed to fetch tasks');

        const tasks = await res.json();
        renderTasks(tasks);
    } catch (err) {
        console.error(err);
        tasksList.innerHTML = '<li>Error loading tasks</li>';
    }
}

// Render tasks in the list
function renderTasks(tasks) {
    tasksList.innerHTML = '';

    if (tasks.length === 0) {
        tasksList.innerHTML = '<li>No tasks yet</li>';
        return;
    }

    tasks.forEach(task => {
        const li = document.createElement('li');
        li.textContent = `${task.title} - ${task.description || ''} - Status: ${task.taskStatusId} - Due: ${task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'N/A'}`;
        tasksList.appendChild(li);
    });
}
