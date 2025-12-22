class AppNavbar extends HTMLElement {
    constructor() {
        super();
    }

    async connectedCallback() {
        const activePage = this.getAttribute('active') || '';

        // Determine auth state based on token
        const token = localStorage.getItem('token');
        const showAuth = !token;

        // Check if user is admin
        let isAdmin = false;
        if (token) {
            isAdmin = await this.checkIfAdmin(token);
        }

        this.render(showAuth, activePage, isAdmin);

        setTimeout(() => {
            this.attachEventListeners();
        }, 0);
    }

    async checkIfAdmin(token) {
        try {
            const response = await fetch(`${API_BASE_URL}/api/Auth/me`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) return false;

            const user = await response.json();
            return user.roles && user.roles.includes('Admin');
        } catch (error) {
            console.error('Error checking admin status:', error);
            return false;
        }
    }

    render(showAuth, activePage, isAdmin) {
        this.innerHTML = `
            <nav class="navbar navbar-expand-lg navbar-dark bg-black border-bottom border-secondary px-4 position-relative">
                <div class="container-fluid">
                    <!-- Left: Brand -->
                    <a class="navbar-brand me-auto me-lg-0" href="/">
                        <div class="home-button">TB</div>
                    </a>

                    <!-- Center: Title (always visible on all screen sizes) -->
                    <div class="navbar-title-container position-absolute start-50 translate-middle-x">
                        <h2 class="navbar-title text-warning mb-0">Torch Bearers</h2>
                        <p class="navbar-subtitle mb-0 d-none d-md-block">A westmarch style game set in a world of mist and soul lanterns</p>
                    </div>

                    <!-- Burger button for mobile -->
                    <button class="navbar-toggler ms-auto" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                        <span class="navbar-toggler-icon"></span>
                    </button>

                    <!-- Collapsible content -->
                    <div class="collapse navbar-collapse position-lg-relative" id="navbarNav">
                    <!-- Nav Links -->
                    <ul class="navbar-nav me-auto">
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'home' ? 'active' : ''}" href="/">Home</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'characters' ? 'active' : ''}" href="/characters.html">Characters</a>
                        </li>
                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle ${activePage === 'settlement' || activePage === 'graveyard' ? 'active' : ''}" href="#" role="button">
                                Community
                            </a>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item" href="settlement.html">Settlement</a></li>
                                <li><a class="dropdown-item" href="graveyard.html">Graveyard</a></li>
                            </ul>
                        </li>
                        <!-- Coming soon:
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'sessions' ? 'active' : ''}" href="/sessions.html">Sessions</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'shops' ? 'active' : ''}" href="/shops.html">Shops</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'map' ? 'active' : ''}" href="/map.html">Map</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'archives' ? 'active' : ''}" href="/archives.html">Archives</a>
                        </li>
                        -->
                        ${isAdmin ? `
                        <li class="nav-item">
                            <a class="nav-link ${activePage === 'admin' ? 'active' : ''}" href="/admin.html">Admin</a>
                        </li>
                        ` : ''}
                    </ul>

                    <!-- Right: Auth buttons -->
                    ${showAuth ? this.renderAuthButtons() : this.renderUserMenu()}
                </div>
                </div>
            </nav>
            
            ${this.renderModals()}

            <style>
                .navbar-title-container {
                    text-align: center;
                    pointer-events: none;
                    z-index: 1;
                }

                .navbar-title {
                    font-size: 1.5rem;
                    line-height: 1.2;
                }

                @media (max-width: 576px) {
                    .navbar-title {
                        font-size: 1.1rem;
                    }
                }

                .navbar-subtitle {
                    font-size: 0.75rem;
                    color: #cfc4a6;
                    font-style: italic;
                }

                /* Mobile menu overlay */
                @media (max-width: 991px) {
                    .navbar-collapse {
                        position: absolute;
                        top: 100%;
                        left: 0;
                        right: 0;
                        background-color: #1b1b1b;
                        border-top: 1px solid #3a3a3a;
                        z-index: 1000;
                        padding: 1rem;
                    }
                    
                    .navbar-nav {
                        width: 100%;
                    }
                }

                /* Ensure navbar items don't overlap with centered title on medium screens */
                @media (min-width: 992px) and (max-width: 1400px) {
                    .navbar-nav {
                        max-width: 30%;
                    }
                }

                /* Ensure collapsible section has proper z-index */
                .navbar-collapse {
                    z-index: 2;
                }
            </style>
        `;
    }

    renderAuthButtons() {
        return `
            <div class="d-flex gap-2 ms-lg-auto">
                <button class="btn btn-outline-warning btn-sm" id="signInBtn">Sign In</button>
                <button class="btn btn-warning btn-sm" id="registerBtn">Register</button>
            </div>
        `;
    }

    renderUserMenu() {
        const username = localStorage.getItem('username') || 'User';
        return `
            <div class="d-flex gap-2 align-items-center ms-lg-auto">
                <span class="text-warning">Welcome, ${username}</span>
                <button class="btn btn-outline-warning btn-sm" id="logoutBtn">Logout</button>
            </div>
        `;
    }

    renderModals() {
        return `
            <login-modal id="loginModal"></login-modal>
            <register-modal id="registerModal"></register-modal>
        `;
    }

    attachEventListeners() {
        const token = localStorage.getItem('token');

        if (!token) {
            // Login/Register button listeners
            const signInBtn = this.querySelector('#signInBtn');
            const registerBtn = this.querySelector('#registerBtn');

            if (signInBtn) {
                signInBtn.addEventListener('click', async () => {
                    await customElements.whenDefined('login-modal');
                    const loginModal = this.querySelector('#loginModal');
                    if (loginModal && loginModal.show) {
                        loginModal.show();
                    }
                });
            }

            if (registerBtn) {
                registerBtn.addEventListener('click', async () => {
                    await customElements.whenDefined('register-modal');
                    const registerModal = this.querySelector('#registerModal');
                    if (registerModal && registerModal.show) {
                        registerModal.show();
                    }
                });
            }

        } else {
            // Logout button listener
            const logoutBtn = this.querySelector('#logoutBtn');
            if (logoutBtn) {
                logoutBtn.addEventListener('click', () => {
                    localStorage.removeItem('token');
                    localStorage.removeItem('username');
                    localStorage.removeItem('roles');
                    window.location.href = '/';
                });
            }
        }
    }
}

customElements.define('app-navbar', AppNavbar);