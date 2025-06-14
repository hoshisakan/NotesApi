// src/utils/auth.js
import axios from 'axios';

const API_BASE_URL = 'http://localhost/api';
// const API_BASE_URL = 'http://localhost:5002/api';
// const API_BASE_URL = 'http://localhost:5000/api';

export async function checkAndRefreshToken() {
    const accessToken = localStorage.getItem('accessToken');
    const refreshToken = localStorage.getItem('refreshToken');

    console.log('read accessToken:', accessToken);
    console.log('read refreshToken:', refreshToken);

    if (!accessToken || !refreshToken) {
        return null;
    }

    try {
        const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
            refreshToken,
        });

        const { accessToken: newAccessToken } = response.data;
        localStorage.setItem('accessToken', newAccessToken);
        return newAccessToken;
    } catch (error) {
        console.error('Token refresh failed:', error);
        return null;
    }
}
