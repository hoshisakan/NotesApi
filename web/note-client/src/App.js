import React, { useState } from 'react';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import NotesPage from './components/NotesPage';

export default function App() {
    const [token, setToken] = useState(null);
    const [page, setPage] = useState('login'); // login, register, note

    const handleLoginSuccess = (accessToken) => {
        setToken(accessToken);
        setPage('note');
    };

    const handleLogout = () => {
        setToken(null);
        setPage('login');
    };

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
