class RegisterModal extends HTMLElement {
    constructor() {
        super();
        this.modalId = 'registerModal';
    }

    connectedCallback() {
        this.render();
        this.attachEventListeners();
    }

    render() {
        this.innerHTML = `
            <div class="modal fade" id="${this.modalId}" tabindex="-1" aria-labelledby="registerModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content bg-dark border-warning">
                        <div class="modal-header border-warning">
                            <h5 class="modal-title text-warning" id="registerModalLabel">Register</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <form id="registerForm">
                                <div class="mb-3">
                                    <label for="registerUsername" class="form-label text-light">Username</label>
                                    <input type="text" class="form-control bg-black text-light border-secondary" id="registerUsername" required minlength="3">
                                    <div class="form-text text-muted">At least 3 characters</div>
                                </div>
                                <div class="mb-3">
                                    <label for="registerPassword" class="form-label text-light">Password</label>
                                    <input type="password" class="form-control bg-black text-light border-secondary" id="registerPassword" required minlength="8">
                                    <div class="form-text text-muted">At least 8 characters</div>
                                </div>
                                <div class="mb-3">
                                    <label for="registerPasswordConfirm" class="form-label text-light">Confirm Password</label>
                                    <input type="password" class="form-control bg-black text-light border-secondary" id="registerPasswordConfirm" required>
                                </div>
                                <div class="alert alert-danger d-none" id="registerError"></div>
                                <div class="alert alert-success d-none" id="registerSuccess"></div>
                            </form>
                        </div>
                        <div class="modal-footer border-warning">
                            <button type="button" class="btn btn-outline-warning" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-warning" id="registerSubmitBtn">Register</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    attachEventListeners() {
        const form = this.querySelector('#registerForm');
        const submitBtn = this.querySelector('#registerSubmitBtn');

        submitBtn.addEventListener('click', async (e) => {
            e.preventDefault();

            const username = this.querySelector('#registerUsername').value;
            const password = this.querySelector('#registerPassword').value;
            const passwordConfirm = this.querySelector('#registerPasswordConfirm').value;

            // Validation
            if (!username || !password || !passwordConfirm) {
                this.showError('Please fill in all fields');
                return;
            }

            if (password !== passwordConfirm) {
                this.showError('Passwords do not match');
                return;
            }

            if (username.length < 3) {
                this.showError('Username must be at least 3 characters');
                return;
            }

            if (password.length < 8) {
                this.showError('Password must be at least 8 characters');
                return;
            }

            try {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Registering...';

                const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ username, password })
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.message || 'Registration failed');
                }

                const data = await response.json();

                // Store token and user info
                localStorage.setItem('token', data.token);
                localStorage.setItem('username', data.username);
                localStorage.setItem('roles', JSON.stringify(data.roles));

                // Show success and reload
                this.showSuccess('Registration successful! Redirecting...');
                setTimeout(() => {
                    window.location.reload();
                }, 1500);

            } catch (error) {
                this.showError(error.message);
                submitBtn.disabled = false;
                submitBtn.textContent = 'Register';
            }
        });

        // Submit on Enter key
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            submitBtn.click();
        });
    }

    showError(message) {
        const errorDiv = this.querySelector('#registerError');
        const successDiv = this.querySelector('#registerSuccess');
        errorDiv.textContent = message;
        errorDiv.classList.remove('d-none');
        successDiv.classList.add('d-none');
    }

    showSuccess(message) {
        const errorDiv = this.querySelector('#registerError');
        const successDiv = this.querySelector('#registerSuccess');
        successDiv.textContent = message;
        successDiv.classList.remove('d-none');
        errorDiv.classList.add('d-none');
    }

    show() {
        const modal = new bootstrap.Modal(this.querySelector(`#${this.modalId}`));
        modal.show();
    }
}

customElements.define('register-modal', RegisterModal);