import React, { useEffect, useState } from 'react';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import NotesPage from './components/NotesPage';
import { checkAndRefreshToken } from './utils/auth';

export default function App() {
    const [token, setToken] = useState(null);
    const [page, setPage] = useState('login');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const validate = async () => {
            const validToken = await checkAndRefreshToken();
            console.log('Valid token:', validToken);
            if (validToken) {
                setToken(validToken);
                setPage('note');
            } else {
                setToken(null);
                setPage('login');
            }
            setLoading(false);
        };
        validate();
    }, []);

    const handleLoginSuccess = (accessToken) => {
        localStorage.setItem('accessToken', accessToken);
        setToken(accessToken);
        setPage('note');
    };

    const handleLogout = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        setToken(null);
        setPage('login');
    };

    if (loading) return <div>Loading...</div>;

    return (
        <>
            {page === 'login' && (
                <LoginPage onLoginSuccess={handleLoginSuccess} onGoRegister={() => setPage('register')} />
            )}
            {page === 'register' && (
                <RegisterPage onRegisterSuccess={() => setPage('login')} onGoLogin={() => setPage('login')} />
            )}
            {page === 'note' && <NotesPage token={token} onLogout={handleLogout} />}
        </>
    );
}
