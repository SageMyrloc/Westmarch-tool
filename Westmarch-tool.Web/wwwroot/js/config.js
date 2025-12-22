// API Base URL - environment-aware configuration
const API_BASE_URL = window.location.hostname === 'localhost'
    ? 'http://localhost:5245'  // Local development API
    : '/api';  // Production (proxied through Nginx)

console.log('API Base URL configured as:', API_BASE_URL);