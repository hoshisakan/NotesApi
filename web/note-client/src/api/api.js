import axios from 'axios';

const API_BASE_URL = 'http://localhost/api';
// const API_BASE_URL = 'http://lcalhost:5002/api';
// const API_BASE_URL = 'http://localhost:5000/api';

// 建立 axios 實例
const api = axios.create({
    baseURL: API_BASE_URL,
    headers: { 'Content-Type': 'application/json' },
});

// 請求攔截器，自動帶上 token（如果有）
let isRefreshing = false;
let refreshSubscribers = [];

function onRefreshed(token) {
    refreshSubscribers.forEach((cb) => cb(token));
    refreshSubscribers = [];
}

function addRefreshSubscriber(cb) {
    refreshSubscribers.push(cb);
}

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        if (error.response && error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            if (isRefreshing) {
                return new Promise((resolve) => {
                    addRefreshSubscriber((token) => {
                        originalRequest.headers.Authorization = 'Bearer ' + token;
                        resolve(api(originalRequest));
                    });
                });
            }

            isRefreshing = true;

            const accessToken = localStorage.getItem('accessToken') || '';
            const refreshToken = localStorage.getItem('refreshToken');

            try {
                console.log('read accessToken:', accessToken);
                console.log('read refreshToken:', refreshToken);
                // 同時帶 accessToken 與 refreshToken 到刷新 token API
                const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
                    refreshToken,
                });

                console.log('刷新 token 成功:', response.data);

                const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                localStorage.setItem('accessToken', newAccessToken);
                if (newRefreshToken) {
                    localStorage.setItem('refreshToken', newRefreshToken);
                }

                api.defaults.headers.common['Authorization'] = 'Bearer ' + newAccessToken;

                onRefreshed(newAccessToken);
                isRefreshing = false;

                originalRequest.headers.Authorization = 'Bearer ' + newAccessToken;
                return api(originalRequest);
            } catch (refreshError) {
                console.error('刷新 token 失敗:', refreshError);
                isRefreshing = false;
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                window.location.href = '/login'; // 或其他登出處理
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

// 登入
export async function login(username, password) {
    const response = await api.post('/auth/login', { username, password });
    const { accessToken, refreshToken } = response.data;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    return accessToken;
}

// 註冊
export async function register(username, password) {
    const response = await api.post('/auth/register', { username, password });
    return response.data;
}

// 取得所有筆記
export async function fetchNotes() {
    const response = await api.get('/notes');
    return response.data;
}

// 新增筆記
export async function createNote(note) {
    const response = await api.post('/notes', note);
    return response.data;
}

// 更新筆記
export async function updateNote(note) {
    const response = await api.put(`/notes/${note.id}`, note);
    return response.data;
}

// 刪除筆記
export async function deleteNote(id) {
    console.log('刪除筆記 ID:', id);
    const response = await api.delete(`/notes/${id}`);
    return response.data;
}

// 檢查 token 是否有效
export async function checkToken(token) {
    return api.post('/auth/check-token', { token });
}
