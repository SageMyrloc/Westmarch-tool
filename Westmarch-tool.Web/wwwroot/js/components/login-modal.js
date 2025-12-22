class LoginModal extends HTMLElement {
    constructor() {
        super();
        this.modalId = 'loginModal';
    }

    connectedCallback() {
        this.render();
        this.attachEventListeners();
    }

    render() {
        this.innerHTML = `
            <div class="modal fade" id="${this.modalId}" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content bg-dark border-warning">
                        <div class="modal-header border-warning">
                            <h5 class="modal-title text-warning" id="loginModalLabel">Login</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <form id="loginForm">
                                <div class="mb-3">
                                    <label for="loginUsername" class="form-label text-light">Username</label>
                                    <input type="text" class="form-control bg-black text-light border-secondary" id="loginUsername" required>
                                </div>
                                <div class="mb-3">
                                    <label for="loginPassword" class="form-label text-light">Password</label>
                                    <input type="password" class="form-control bg-black text-light border-secondary" id="loginPassword" required>
                                </div>
                                <div class="alert alert-danger d-none" id="loginError"></div>
                            </form>
                        </div>
                        <div class="modal-footer border-warning">
                            <button type="button" class="btn btn-outline-warning" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-warning" id="loginSubmitBtn">Login</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    attachEventListeners() {
        const form = this.querySelector('#loginForm');
        const submitBtn = this.querySelector('#loginSubmitBtn');
        const errorDiv = this.querySelector('#loginError');

        submitBtn.addEventListener('click', async (e) => {
            e.preventDefault();

            const username = this.querySelector('#loginUsername').value;
            const password = this.querySelector('#loginPassword').value;

            if (!username || !password) {
                this.showError('Please fill in all fields');
                return;
            }

            try {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Logging in...';

                const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ username, password })
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.message || 'Login failed');
                }

                const data = await response.json();

                // Store token and user info
                localStorage.setItem('token', data.token);
                localStorage.setItem('username', data.username);
                localStorage.setItem('roles', JSON.stringify(data.roles));

                // Reload page to update navbar
                window.location.reload();

            } catch (error) {
                this.showError(error.message);
                submitBtn.disabled = false;
                submitBtn.textContent = 'Login';
            }
        });

        // Submit on Enter key
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            submitBtn.click();
        });
    }

    showError(message) {
        const errorDiv = this.querySelector('#loginError');
        errorDiv.textContent = message;
        errorDiv.classList.remove('d-none');
    }

    show() {
        const modal = new bootstrap.Modal(this.querySelector(`#${this.modalId}`));
        modal.show();
    }
}

customElements.define('login-modal', LoginModal);