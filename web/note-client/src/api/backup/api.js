import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

export async function login(username, password) {

    const response = await axios.post(`${API_BASE_URL}/auth/login`, { username, password });
    const { accessToken, refreshToken } = response.data;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    return accessToken; // 回傳 accessToken
}

export async function register(username, password) {
    const response = await axios.post(`${API_BASE_URL}/auth/register`, { username, password });
    return response.data; // 回傳註冊結果
}

export async function fetchNotes(token) {
    const response = await axios.get(`${API_BASE_URL}/notes`, {
        headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
}

export const createNote = (token, note) => {
    return axios.post(`${API_BASE_URL}/notes`, note, {
        headers: { Authorization: `Bearer ${token}` },
    });
};

export const updateNote = (token, note) => {
    return axios.put(`${API_BASE_URL}/notes/${note.id}`, note, {
        headers: { Authorization: `Bearer ${token}` },
    });
};

export async function deleteNote(token, id) {
    const response = await axios.delete(`${API_BASE_URL}/notes/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    });
    return response.data;
}
