import React, { useState, useEffect } from 'react';
import { login, checkToken } from '../api/api';

export default function LoginPage({ onLoginSuccess, onGoRegister }) {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [checkingToken, setCheckingToken] = useState(true);

    useEffect(() => {
        async function verifyToken() {
            const token = localStorage.getItem('accessToken');
            if (token) {
                try {
                    const checkResponse = await checkToken(token);
                    const checkStatus = checkResponse.status;

                    if (checkStatus !== 200) {
                        throw new Error('Token is invalid');
                    }
                    console.log('Token is valid:', checkStatus);
                    // token 有效，直接呼叫 onLoginSuccess 並跳過登入頁
                    onLoginSuccess(token);
                } catch (err) {
                    // token 無效，清除並停留在登入頁
                    localStorage.removeItem('accessToken');
                    localStorage.removeItem('refreshToken');
                }
            }
            setCheckingToken(false);
        }
        verifyToken();
    }, [onLoginSuccess]);

    const handleLogin = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);
        try {
            const res = await login(username, password);

            setLoading(false);
            onLoginSuccess(res);
        } catch (err) {
            setLoading(false);
            setError('登入失敗，請檢查帳號密碼');
        }
    };

    if (checkingToken) {
        return <div>正在檢查登入狀態...</div>;
    }

    return (
        <div className="container mt-5" style={{ maxWidth: 400 }}>
            <h3 className="mb-4">登入</h3>
            {error && <div className="alert alert-danger">{error}</div>}
            <form onSubmit={handleLogin}>
                <div className="mb-3">
                    <label>帳號</label>
                    <input
                        type="text"
                        className="form-control"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                        autoFocus
                    />
                </div>
                <div className="mb-3">
                    <label>密碼</label>
                    <input
                        type="password"
                        className="form-control"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button className="btn btn-primary w-100" disabled={loading}>
                    {loading ? '登入中...' : '登入'}
                </button>
            </form>
            <div className="mt-3 text-center">
                還沒有帳號？{' '}
                <button className="btn btn-link p-0" onClick={onGoRegister}>
                    註冊
                </button>
            </div>
        </div>
    );
}
