// Characters Page JavaScript
let myCharacters = [];
let createCharacterModal = null;

document.addEventListener('DOMContentLoaded', async () => {
    console.log('Characters page loading...');

    // Check if user is logged in
    const token = localStorage.getItem('token');
    if (!token) {
        console.log('No token found, redirecting to home');
        window.location.href = '/';
        return;
    }

    console.log('Token found, initializing page');

    // Wait for Bootstrap to be ready
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap not loaded!');
        return;
    }

    // Initialize modal
    const modalElement = document.getElementById('createCharacterModal');
    if (!modalElement) {
        console.error('Modal element not found!');
        return;
    }

    createCharacterModal = new bootstrap.Modal(modalElement);
    console.log('Modal initialized');

    // Attach event listeners
    attachEventListeners();
    console.log('Event listeners attached');

    // Load characters
    await loadMyCharacters();
    console.log('Characters loaded');
});

function attachEventListeners() {
    console.log('Attaching event listeners...');

    // Create character button
    const createBtn = document.getElementById('createCharacterBtn');
    if (!createBtn) {
        console.error('Create character button not found!');
        return;
    }
    console.log('Found create character button:', createBtn);
    createBtn.addEventListener('click', () => {
        console.log('Create character button clicked!');
        openCreateCharacterModal();
    });

    // Import method buttons
    document.getElementById('importPathbuilderBtn').addEventListener('click', showPathbuilderImport);
    document.getElementById('manualEntryBtn').addEventListener('click', showManualEntry);

    // Back buttons
    document.getElementById('backToMethodBtn').addEventListener('click', showMethodSelection);
    document.getElementById('backToMethodBtn2').addEventListener('click', showMethodSelection);

    // Parse JSON button
    document.getElementById('parseJsonBtn').addEventListener('click', parsePathbuilderJson);

    // Submit character button
    document.getElementById('submitCharacterBtn').addEventListener('click', submitCharacter);

    console.log('All event listeners attached');
}

function openCreateCharacterModal() {
    console.log('Opening create character modal...');

    if (!createCharacterModal) {
        console.error('Modal not initialized!');
        return;
    }

    // Reset modal to initial state
    showMethodSelection();
    resetForm();

    console.log('Showing modal...');
    createCharacterModal.show();
    console.log('Modal shown');
}

function showMethodSelection() {
    document.getElementById('importMethodSelection').classList.remove('d-none');
    document.getElementById('pathbuilderImportSection').classList.add('d-none');
    document.getElementById('characterForm').classList.add('d-none');
    document.getElementById('submitCharacterBtn').classList.add('d-none');
    document.getElementById('formError').classList.add('d-none');
}

function showPathbuilderImport() {
    document.getElementById('importMethodSelection').classList.add('d-none');
    document.getElementById('pathbuilderImportSection').classList.remove('d-none');
    document.getElementById('characterForm').classList.add('d-none');
}

function showManualEntry() {
    document.getElementById('importMethodSelection').classList.add('d-none');
    document.getElementById('pathbuilderImportSection').classList.add('d-none');
    document.getElementById('characterForm').classList.remove('d-none');
    document.getElementById('submitCharacterBtn').classList.remove('d-none');
}

function parsePathbuilderJson() {
    const jsonText = document.getElementById('pathbuilderJson').value.trim();

    if (!jsonText) {
        alert('Please paste Pathbuilder JSON data');
        return;
    }

    try {
        const data = JSON.parse(jsonText);

        // Check if data has the 'build' property (actual Pathbuilder format)
        const build = data.build || data;

        console.log('Parsing build data:', build);

        // Populate form fields from Pathbuilder JSON
        // Basic Info
        document.getElementById('charName').value = build.name || '';
        document.getElementById('charClass').value = build.class || '';
        document.getElementById('charAncestry').value = build.ancestry || '';
        document.getElementById('charHeritage').value = build.heritage || '';
        document.getElementById('charBackground').value = build.background || '';
        document.getElementById('charLevel').value = build.level || 1;
        document.getElementById('charAlignment').value = build.alignment || '';
        document.getElementById('charKeyAbility').value = build.keyability || '';

        // Size mapping (Pathbuilder uses 2=Small, 3=Medium, etc but sometimes stored as number)
        // Their JSON shows size: 2 and sizeName: "Medium" which seems inconsistent
        let sizeValue = 3; // Default to Medium
        if (build.sizeName) {
            const sizeMap = {
                'Small': 2,
                'Medium': 3,
                'Large': 4
            };
            sizeValue = sizeMap[build.sizeName] || 3;
        } else if (build.size) {
            sizeValue = build.size;
        }
        document.getElementById('charSize').value = sizeValue;

        // Attributes - check if nested in 'attributes' object
        const attrs = build.attributes || build;
        document.getElementById('charAncestryHP').value = attrs.ancestryhp || 0;
        document.getElementById('charClassHP').value = attrs.classhp || 0;
        document.getElementById('charSpeed').value = attrs.speed || 25;

        // Show the form
        document.getElementById('pathbuilderImportSection').classList.add('d-none');
        document.getElementById('characterForm').classList.remove('d-none');
        document.getElementById('submitCharacterBtn').classList.remove('d-none');

        // Show success message
        const formError = document.getElementById('formError');
        formError.className = 'alert alert-success';
        formError.textContent = 'Pathbuilder data imported successfully! Review and submit.';
        formError.classList.remove('d-none');

    } catch (error) {
        console.error('Error parsing JSON:', error);
        alert('Invalid JSON format. Please check your Pathbuilder export and try again.\n\nError: ' + error.message);
    }
}

async function submitCharacter() {
    const token = localStorage.getItem('token');
    const submitBtn = document.getElementById('submitCharacterBtn');

    // Get form values
    const characterData = {
        name: document.getElementById('charName').value.trim(),
        class: document.getElementById('charClass').value.trim(),
        ancestry: document.getElementById('charAncestry').value.trim(),
        heritage: document.getElementById('charHeritage').value.trim() || null,
        background: document.getElementById('charBackground').value.trim() || '',
        level: parseInt(document.getElementById('charLevel').value) || 1,
        alignment: document.getElementById('charAlignment').value.trim() || '',
        size: parseInt(document.getElementById('charSize').value),
        sizeName: getSizeName(parseInt(document.getElementById('charSize').value)),
        keyAbility: document.getElementById('charKeyAbility').value.trim() || '',
        ancestryHP: parseInt(document.getElementById('charAncestryHP').value) || 0,
        classHP: parseInt(document.getElementById('charClassHP').value) || 0,
        speed: parseInt(document.getElementById('charSpeed').value) || 25
    };

    // Validate required fields
    if (!characterData.name || !characterData.class || !characterData.ancestry) {
        showFormError('Name, Class, and Ancestry are required fields');
        return;
    }

    try {
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating...';

        const response = await fetch(`${API_BASE_URL}/api/characters`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(characterData)
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to create character');
        }

        const newCharacter = await response.json();

        // Close modal and reload characters
        createCharacterModal.hide();
        await loadMyCharacters();

        // Show success message (you could add a toast notification here)
        alert('Character created successfully! Awaiting DM approval.');

    } catch (error) {
        console.error('Error creating character:', error);
        showFormError(error.message);
        submitBtn.disabled = false;
        submitBtn.textContent = 'Create Character';
    }
}

function getSizeName(sizeValue) {
    const sizeNames = {
        2: 'Small',
        3: 'Medium',
        4: 'Large'
    };
    return sizeNames[sizeValue] || 'Medium';
}

function showFormError(message) {
    const errorDiv = document.getElementById('formError');
    errorDiv.className = 'alert alert-danger';
    errorDiv.textContent = message;
    errorDiv.classList.remove('d-none');
}

function resetForm() {
    document.getElementById('pathbuilderJson').value = '';
    document.getElementById('characterForm').reset();
    document.getElementById('formError').classList.add('d-none');
}

async function loadMyCharacters() {
    const token = localStorage.getItem('token');

    try {
        // Show loading state
        document.getElementById('loadingState').classList.remove('d-none');
        document.getElementById('charactersContent').classList.add('d-none');
        document.getElementById('noCharactersState').classList.add('d-none');
        document.getElementById('errorState').classList.add('d-none');

        const response = await fetch(`${API_BASE_URL}/api/characters/my`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load characters');
        }

        myCharacters = await response.json();

        // Hide loading
        document.getElementById('loadingState').classList.add('d-none');

        if (myCharacters.length === 0) {
            document.getElementById('noCharactersState').classList.remove('d-none');
        } else {
            renderCharacters();
            document.getElementById('charactersContent').classList.remove('d-none');
        }

    } catch (error) {
        console.error('Error loading characters:', error);
        showError(error.message);
    }
}

function renderCharacters() {
    const grid = document.getElementById('charactersGrid');
    grid.innerHTML = '';

    myCharacters.forEach(character => {
        const card = createCharacterCard(character);
        grid.appendChild(card);
    });
}

function createCharacterCard(character) {
    const col = document.createElement('div');
    col.className = 'col-md-6 col-lg-4 mb-4';

    const statusBadge = getStatusBadge(character.status);
    const totalHP = character.attributes
        ? character.attributes.ancestryHP + character.attributes.classHP + character.attributes.bonusHP
        : 0;

    col.innerHTML = `
        <div class="card bg-black border-secondary h-100">
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <h5 class="card-title text-warning mb-0">${character.name}</h5>
                    ${statusBadge}
                </div>
                
                <p class="text-light mb-2">
                    <strong>Level ${character.level} ${character.ancestry} ${character.class}</strong>
                </p>

                ${character.heritage ? `<p class="text-muted small mb-1">Heritage: ${character.heritage}</p>` : ''}
                ${character.background ? `<p class="text-muted small mb-1">Background: ${character.background}</p>` : ''}
                
                <hr class="border-secondary">
                
                <div class="row text-center small">
                    <div class="col-4">
                        <div class="text-muted">HP</div>
                        <div class="text-warning"><strong>${totalHP}</strong></div>
                    </div>
                    <div class="col-4">
                        <div class="text-muted">Speed</div>
                        <div class="text-warning"><strong>${character.attributes?.speed || 25}</strong></div>
                    </div>
                    <div class="col-4">
                        <div class="text-muted">XP</div>
                        <div class="text-warning"><strong>${character.xp}</strong></div>
                    </div>
                </div>

                ${character.status === 0 ? `
                <div class="mt-3">
                    <button class="btn btn-sm btn-outline-danger w-100" onclick="deleteCharacter(${character.id})">
                        Delete Character
                    </button>
                </div>
                ` : ''}
            </div>
            <div class="card-footer bg-dark border-secondary text-muted small">
                Created: ${new Date(character.createdDate).toLocaleDateString()}
            </div>
        </div>
    `;

    return col;
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

async function deleteCharacter(characterId) {
    if (!confirm('Are you sure you want to delete this character? This action cannot be undone.')) {
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_BASE_URL}/api/characters/${characterId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to delete character');
        }

        // Reload characters
        await loadMyCharacters();
        alert('Character deleted successfully');

    } catch (error) {
        console.error('Error deleting character:', error);
        alert(error.message);
    }
}

function showError(message) {
    document.getElementById('loadingState').classList.add('d-none');
    document.getElementById('charactersContent').classList.add('d-none');
    document.getElementById('noCharactersState').classList.add('d-none');
    document.getElementById('errorState').classList.remove('d-none');
    document.getElementById('errorMessage').textContent = message;
}