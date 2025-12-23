// DM Panel JavaScript
let pendingCharacters = [];
let allCharacters = [];
let currentCharacterId = null;
let characterDetailsModal = null;
let rejectReasonModal = null;

document.addEventListener('DOMContentLoaded', async () => {
    console.log('DM panel loading...');

    // Check if user is DM or Admin
    if (!await checkDMAccess()) {
        window.location.href = '/';
        return;
    }

    // Initialize modals
    characterDetailsModal = new bootstrap.Modal(document.getElementById('characterDetailsModal'));
    rejectReasonModal = new bootstrap.Modal(document.getElementById('rejectReasonModal'));

    // Attach event listeners
    attachEventListeners();

    // Load pending characters on initial load
    await loadPendingCharacters();
});

async function checkDMAccess() {
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
        return data.roles && (data.roles.includes('Admin') || data.roles.includes('DM'));
    } catch (error) {
        console.error('Error checking DM access:', error);
        return false;
    }
}

function attachEventListeners() {
    // Tab switching
    document.getElementById('all-characters-tab').addEventListener('click', async () => {
        if (allCharacters.length === 0) {
            await loadAllCharacters();
        }
    });

    // Modal buttons
    document.getElementById('approveCharacterBtn').addEventListener('click', approveCharacterFromModal);
    document.getElementById('rejectCharacterBtn').addEventListener('click', openRejectReasonModal);
    document.getElementById('confirmRejectBtn').addEventListener('click', confirmRejectCharacter);
}

async function loadPendingCharacters() {
    const token = localStorage.getItem('token');

    try {
        // Show loading
        document.getElementById('pendingLoadingState').classList.remove('d-none');
        document.getElementById('pendingCharactersContent').classList.add('d-none');
        document.getElementById('noPendingState').classList.add('d-none');

        const response = await fetch(`${API_BASE_URL}/api/dm/characters/pending`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load pending characters');
        }

        pendingCharacters = await response.json();
        console.log('Pending characters loaded:', pendingCharacters.length);

        // Update pending count badge
        document.getElementById('pendingCount').textContent = pendingCharacters.length;

        // Hide loading
        document.getElementById('pendingLoadingState').classList.add('d-none');

        if (pendingCharacters.length === 0) {
            document.getElementById('noPendingState').classList.remove('d-none');
        } else {
            renderPendingCharacters();
            document.getElementById('pendingCharactersContent').classList.remove('d-none');
        }

    } catch (error) {
        console.error('Error loading pending characters:', error);
        alert('Failed to load pending characters');
    }
}

function renderPendingCharacters() {
    const grid = document.getElementById('pendingCharactersGrid');
    grid.innerHTML = '';

    pendingCharacters.forEach(character => {
        const card = createPendingCharacterCard(character);
        grid.appendChild(card);
    });
}

function createPendingCharacterCard(character) {
    const col = document.createElement('div');
    col.className = 'col-md-6 col-lg-4 mb-4';

    const totalHP = character.attributes?.totalHP || 0;
    const submittedDate = new Date(character.createdDate).toLocaleDateString();

    col.innerHTML = `
        <div class="card bg-black border-warning h-100">
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <h5 class="card-title text-warning mb-0">${character.name}</h5>
                    <span class="badge bg-warning text-dark">Pending</span>
                </div>
                
                <p class="text-light mb-2">
                    <strong>Level ${character.level} ${character.ancestry} ${character.class}</strong>
                </p>

                <p class="text-muted small mb-1">Player: <strong>${character.player.username}</strong></p>
                ${character.heritage ? `<p class="text-muted small mb-1">Heritage: ${character.heritage}</p>` : ''}
                ${character.background ? `<p class="text-muted small mb-1">Background: ${character.background}</p>` : ''}
                
                <hr class="border-secondary">
                
                <div class="row text-center small mb-3">
                    <div class="col-4">
                        <div class="text-muted">HP</div>
                        <div class="text-warning"><strong>${totalHP}</strong></div>
                    </div>
                    <div class="col-4">
                        <div class="text-muted">Speed</div>
                        <div class="text-warning"><strong>${character.attributes?.speed || 25}</strong></div>
                    </div>
                    <div class="col-4">
                        <div class="text-muted">Key</div>
                        <div class="text-warning"><strong>${character.keyAbility || 'N/A'}</strong></div>
                    </div>
                </div>

                <div class="d-grid gap-2">
                    <button class="btn btn-sm btn-outline-warning" onclick="viewCharacterDetails(${character.id})">
                        View Full Details
                    </button>
                    <div class="btn-group">
                        <button class="btn btn-sm btn-success" onclick="approveCharacter(${character.id})">
                            Approve
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="rejectCharacter(${character.id})">
                            Reject
                        </button>
                    </div>
                </div>
            </div>
            <div class="card-footer bg-dark border-secondary text-muted small">
                Submitted: ${submittedDate}
            </div>
        </div>
    `;

    return col;
}

async function viewCharacterDetails(characterId) {
    const character = pendingCharacters.find(c => c.id === characterId);

    if (!character) {
        alert('Character not found');
        return;
    }

    currentCharacterId = characterId;

    // Populate modal
    document.getElementById('characterDetailsTitle').textContent = character.name;

    const totalHP = character.attributes?.totalHP || 0;

    const detailsHtml = `
        <div class="row">
            <div class="col-md-6 mb-3">
                <h6 class="text-warning">Basic Information</h6>
                <table class="table table-sm table-dark">
                    <tr><td>Player:</td><td><strong>${character.player.username}</strong></td></tr>
                    <tr><td>Class:</td><td>${character.class}${character.dualClass ? ` / ${character.dualClass}` : ''}</td></tr>
                    <tr><td>Level:</td><td>${character.level}</td></tr>
                    <tr><td>Ancestry:</td><td>${character.ancestry}</td></tr>
                    ${character.heritage ? `<tr><td>Heritage:</td><td>${character.heritage}</td></tr>` : ''}
                    ${character.background ? `<tr><td>Background:</td><td>${character.background}</td></tr>` : ''}
                    <tr><td>Alignment:</td><td>${character.alignment}</td></tr>
                    ${character.deity && character.deity !== 'Not set' ? `<tr><td>Deity:</td><td>${character.deity}</td></tr>` : ''}
                </table>
            </div>
            <div class="col-md-6 mb-3">
                <h6 class="text-warning">Attributes</h6>
                <table class="table table-sm table-dark">
                    <tr><td>Size:</td><td>${character.sizeName}</td></tr>
                    <tr><td>Key Ability:</td><td>${character.keyAbility}</td></tr>
                    <tr><td>HP:</td><td><strong class="text-warning">${totalHP}</strong> (Ancestry: ${character.attributes?.ancestryHP || 0}, Class: ${character.attributes?.classHP || 0})</td></tr>
                    <tr><td>Speed:</td><td>${character.attributes?.speed || 25} ft</td></tr>
                    ${character.gender && character.gender !== 'Not set' ? `<tr><td>Gender:</td><td>${character.gender}</td></tr>` : ''}
                    ${character.age && character.age !== 'Not set' ? `<tr><td>Age:</td><td>${character.age}</td></tr>` : ''}
                </table>
            </div>
        </div>
        <div class="alert alert-info">
            <small><strong>Submitted:</strong> ${new Date(character.createdDate).toLocaleString()}</small>
        </div>
    `;

    document.getElementById('characterDetailsBody').innerHTML = detailsHtml;
    characterDetailsModal.show();
}

async function approveCharacter(characterId) {
    if (!confirm('Are you sure you want to approve this character?')) {
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_BASE_URL}/api/dm/characters/${characterId}/approve`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to approve character');
        }

        const result = await response.json();
        alert(result.message);

        // Reload pending characters
        await loadPendingCharacters();

        // Close modal if open
        if (characterDetailsModal) {
            characterDetailsModal.hide();
        }

    } catch (error) {
        console.error('Error approving character:', error);
        alert(error.message);
    }
}

async function approveCharacterFromModal() {
    await approveCharacter(currentCharacterId);
}

function rejectCharacter(characterId) {
    const character = pendingCharacters.find(c => c.id === characterId);

    if (!character) {
        alert('Character not found');
        return;
    }

    currentCharacterId = characterId;
    document.getElementById('rejectCharacterName').textContent = `${character.name} by ${character.player.username}`;
    document.getElementById('rejectReason').value = '';

    // Close details modal if open
    if (characterDetailsModal) {
        characterDetailsModal.hide();
    }

    rejectReasonModal.show();
}

function openRejectReasonModal() {
    // Close details modal
    characterDetailsModal.hide();

    const character = pendingCharacters.find(c => c.id === currentCharacterId);
    if (character) {
        document.getElementById('rejectCharacterName').textContent = `${character.name} by ${character.player.username}`;
    }

    rejectReasonModal.show();
}

async function confirmRejectCharacter() {
    const token = localStorage.getItem('token');
    const reason = document.getElementById('rejectReason').value.trim();

    try {
        const response = await fetch(`${API_BASE_URL}/api/dm/characters/${currentCharacterId}/reject`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ reason: reason || null })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to reject character');
        }

        const result = await response.json();
        alert(result.message);

        // Close modal
        rejectReasonModal.hide();

        // Reload pending characters
        await loadPendingCharacters();

    } catch (error) {
        console.error('Error rejecting character:', error);
        alert(error.message);
    }
}

async function loadAllCharacters() {
    const token = localStorage.getItem('token');

    try {
        // Show loading
        document.getElementById('allLoadingState').classList.remove('d-none');
        document.getElementById('allCharactersContent').classList.add('d-none');

        const response = await fetch(`${API_BASE_URL}/api/dm/characters/all`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load characters');
        }

        allCharacters = await response.json();
        console.log('All characters loaded:', allCharacters.length);

        renderAllCharacters();

        // Hide loading
        document.getElementById('allLoadingState').classList.add('d-none');
        document.getElementById('allCharactersContent').classList.remove('d-none');

    } catch (error) {
        console.error('Error loading all characters:', error);
        alert('Failed to load characters');
    }
}

function renderAllCharacters() {
    const tbody = document.getElementById('allCharactersTableBody');
    tbody.innerHTML = '';

    allCharacters.forEach(character => {
        const row = document.createElement('tr');

        const statusBadge = getStatusBadge(character.status);
        const createdDate = new Date(character.createdDate).toLocaleDateString();

        row.innerHTML = `
            <td><strong class="text-warning">${character.name}</strong></td>
            <td>${character.player.username}</td>
            <td>${character.class}</td>
            <td>${character.level}</td>
            <td>${statusBadge}</td>
            <td>${createdDate}</td>
            <td>
                <button class="btn btn-sm btn-outline-warning" onclick="alert('Character details coming soon')">
                    View
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

function getStatusBadge(status) {
    const statusMap = {
        0: { text: 'Awaiting Approval', class: 'bg-warning text-dark' },
        1: { text: 'Alive', class: 'bg-success' },
        2: { text: 'Dead', class: 'bg-danger' },
        3: { text: 'Retired', class: 'bg-secondary' }
    };

    const statusInfo = statusMap[status] || { text: 'Unknown', class: 'bg-secondary' };
    return `<span class="badge ${statusInfo.class}">${statusInfo.text}</span>`;
}