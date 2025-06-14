import React, { useState } from 'react';
import { login } from '../api/backup/api';

export default function LoginPage({ onLoginSuccess, onGoRegister }) {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

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
