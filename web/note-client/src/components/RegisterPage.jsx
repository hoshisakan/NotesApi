import React, { useState } from 'react';
import { register } from '../api/api';

export default function RegisterPage({ onRegisterSuccess, onGoLogin }) {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleRegister = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            await register(username, password);
            setLoading(false);
            alert('註冊成功，請登入');
            onRegisterSuccess();
        } catch (err) {
            setLoading(false);
            setError('註冊失敗，請稍後再試');
        }
    };

    return (
        <div className="container mt-5" style={{ maxWidth: 400 }}>
            <h3 className="mb-4">註冊</h3>
            {error && <div className="alert alert-danger">{error}</div>}

            <form onSubmit={handleRegister}>
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
                        minLength={6}
                    />
                </div>
                <button className="btn btn-primary w-100" disabled={loading}>
                    {loading ? '註冊中...' : '註冊'}
                </button>
            </form>

            <div className="mt-3 text-center">
                已有帳號？{' '}
                <button className="btn btn-link p-0" onClick={onGoLogin}>
                    登入
                </button>
            </div>
        </div>
    );
}
