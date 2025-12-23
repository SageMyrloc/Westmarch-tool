// Admin Page JavaScript
let allUsers = [];
let allRoles = [];
let currentUserId = null;
let roleModal = null;

document.addEventListener('DOMContentLoaded', async () => {
    // Check if user is admin
    if (!await checkAdminAccess()) {
        window.location.href = '/';
        return;
    }

    // Initialize Bootstrap modal
    roleModal = new bootstrap.Modal(document.getElementById('roleModal'));

    // Load data
    await loadAdminData();
});

async function checkAdminAccess() {
    const token = localStorage.getItem('token');
    if (!token) {
        return false;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            return false;
        }

        const data = await response.json();
        return data.roles && data.roles.includes('Admin');
    } catch (error) {
        console.error('Error checking admin access:', error);
        return false;
    }
}

async function loadAdminData() {
    try {
        // Show loading state
        document.getElementById('loadingState').classList.remove('d-none');
        document.getElementById('usersContent').classList.add('d-none');
        document.getElementById('errorState').classList.add('d-none');

        const token = localStorage.getItem('token');

        // Fetch users and roles in parallel
        const [usersResponse, rolesResponse] = await Promise.all([
            fetch(`${API_BASE_URL}/api/admin/users`, {
                headers: { 'Authorization': `Bearer ${token}` }
            }),
            fetch(`${API_BASE_URL}/api/admin/roles`, {
                headers: { 'Authorization': `Bearer ${token}` }
            })
        ]);

        if (!usersResponse.ok || !rolesResponse.ok) {
            throw new Error('Failed to load admin data');
        }

        allUsers = await usersResponse.json();
        allRoles = await rolesResponse.json();

        renderUsersTable();

        // Hide loading, show content
        document.getElementById('loadingState').classList.add('d-none');
        document.getElementById('usersContent').classList.remove('d-none');

    } catch (error) {
        console.error('Error loading admin data:', error);
        showError(error.message);
    }
}

function renderUsersTable() {
    const tbody = document.getElementById('usersTableBody');
    tbody.innerHTML = '';

    allUsers.forEach(user => {
        const row = document.createElement('tr');

        // Format dates
        const createdDate = new Date(user.createdDate).toLocaleDateString();
        const lastLogin = user.lastLoginDate
            ? new Date(user.lastLoginDate).toLocaleDateString()
            : 'Never';

        // Create role badges
        const roleBadges = user.roles
            .map(r => `<span class="badge bg-warning text-dark me-1">${r.name}</span>`)
            .join('');

        row.innerHTML = `
            <td>${user.username}</td>
            <td>${user.discordId || '<span class="text-muted">Not set</span>'}</td>
            <td>${roleBadges}</td>
            <td>${createdDate}</td>
            <td>${lastLogin}</td>
            <td>
                <button class="btn btn-sm btn-outline-warning" onclick="openRoleModal(${user.id})">
                    Manage Roles
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

async function openRoleModal(userId) {
    currentUserId = userId;
    const user = allUsers.find(u => u.id === userId);

    if (!user) {
        console.error('User not found');
        return;
    }

    // Set modal title
    document.getElementById('modalUsername').textContent = user.username;

    // Render current roles
    renderCurrentRoles(user);

    // Render available roles
    renderAvailableRoles(user);

    // Clear any previous messages
    document.getElementById('roleModalMessage').classList.add('d-none');

    // Show modal
    roleModal.show();
}

function renderCurrentRoles(user) {
    const container = document.getElementById('currentRoles');
    container.innerHTML = '';

    if (user.roles.length === 0) {
        container.innerHTML = '<p class="text-muted">No roles assigned</p>';
        return;
    }

    user.roles.forEach(role => {
        const roleDiv = document.createElement('div');
        roleDiv.className = 'd-flex justify-content-between align-items-center mb-2 p-2 bg-secondary rounded';
        roleDiv.innerHTML = `
            <div>
                <strong class="text-warning">${role.name}</strong>
                <br>
                <small class="text-muted">Assigned: ${new Date(role.assignedDate).toLocaleDateString()}</small>
            </div>
            <button class="btn btn-sm btn-danger" onclick="removeRole(${user.id}, ${role.roleId})">
                Remove
            </button>
        `;
        container.appendChild(roleDiv);
    });
}

function renderAvailableRoles(user) {
    const container = document.getElementById('availableRoles');
    container.innerHTML = '';

    // Filter out roles the user already has
    const userRoleIds = user.roles.map(r => r.roleId);
    const availableRoles = allRoles.filter(r => !userRoleIds.includes(r.id));

    if (availableRoles.length === 0) {
        container.innerHTML = '<p class="text-muted">User has all available roles</p>';
        return;
    }

    availableRoles.forEach(role => {
        const roleDiv = document.createElement('div');
        roleDiv.className = 'd-flex justify-content-between align-items-center mb-2 p-2 bg-secondary rounded';
        roleDiv.innerHTML = `
            <div>
                <strong class="text-light">${role.name}</strong>
                <br>
                <small class="text-muted">${role.description}</small>
            </div>
            <button class="btn btn-sm btn-success" onclick="assignRole(${user.id}, ${role.id})">
                Assign
            </button>
        `;
        container.appendChild(roleDiv);
    });
}

async function assignRole(userId, roleId) {
    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}/roles/${roleId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to assign role');
        }

        const result = await response.json();
        showModalMessage(result.message, 'success');

        // Reload data
        await loadAdminData();

        // Re-render modal with updated data
        const user = allUsers.find(u => u.id === userId);
        if (user) {
            renderCurrentRoles(user);
            renderAvailableRoles(user);
        }

    } catch (error) {
        console.error('Error assigning role:', error);
        showModalMessage(error.message, 'danger');
    }
}

async function removeRole(userId, roleId) {
    const token = localStorage.getItem('token');

    // Confirm action
    if (!confirm('Are you sure you want to remove this role?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}/roles/${roleId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to remove role');
        }

        const result = await response.json();
        showModalMessage(result.message, 'success');

        // Reload data
        await loadAdminData();

        // Re-render modal with updated data
        const user = allUsers.find(u => u.id === userId);
        if (user) {
            renderCurrentRoles(user);
            renderAvailableRoles(user);
        }

    } catch (error) {
        console.error('Error removing role:', error);
        showModalMessage(error.message, 'danger');
    }
}

function showModalMessage(message, type) {
    const messageDiv = document.getElementById('roleModalMessage');
    messageDiv.className = `alert alert-${type} mt-3`;
    messageDiv.textContent = message;
    messageDiv.classList.remove('d-none');

    // Auto-hide after 3 seconds
    setTimeout(() => {
        messageDiv.classList.add('d-none');
    }, 3000);
}

function showError(message) {
    document.getElementById('loadingState').classList.add('d-none');
    document.getElementById('usersContent').classList.add('d-none');
    document.getElementById('errorState').classList.remove('d-none');
    document.getElementById('errorMessage').textContent = message;
}