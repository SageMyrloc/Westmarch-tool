// Use environment-aware API URL
const API_BASE_URL = window.location.hostname === 'localhost'
    ? 'http://localhost:5245'  // Local development
    : '/api';  // Production (proxied through Nginx)

document.getElementById('testBtn').addEventListener('click', async () => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/test`);

        // Log the raw response
        console.log('Response status:', response.status);
        console.log('Response ok:', response.ok);

        const text = await response.text();
        console.log('Raw response:', text);

        // Try to parse it
        const data = JSON.parse(text);
        document.getElementById('result').textContent = JSON.stringify(data, null, 2);
    } catch (error) {
        console.error('Full error:', error);
        document.getElementById('result').textContent = `Error: ${error.message}`;
    }
});