import React, { useEffect, useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { fetchNotes, createNote, updateNote, deleteNote } from '../api/api';

export default function NotesPage({ token, onLogout }) {
    const [notes, setNotes] = useState([]);
    const [selectedNote, setSelectedNote] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // 載入筆記
    const loadNotes = React.useCallback(async () => {
        try {
            setLoading(true);
            const data = await fetchNotes(token);
            setNotes(data.$values || []);
            if (data.length > 0) setSelectedNote(data[0]);
            setLoading(false);
        } catch (err) {
            setError('無法載入筆記');
            setLoading(false);
        }
    }, [token]);

    useEffect(() => {
        loadNotes();
    }, [loadNotes]);

    // 選取筆記
    const selectNote = (note) => {
        setSelectedNote(note);
        setError('');
    };

    // 新增筆記（清空編輯區）
    const onAddNew = () => {
        setSelectedNote({ title: '', content: '' });
        setError('');
    };

    // 變更內容
    const onChangeField = (field, value) => {
        setSelectedNote((prev) => ({ ...prev, [field]: value }));
    };

    const onSave = async () => {
        try {
            setLoading(true);
            if (selectedNote.id) {
                console.log('更新筆記:', selectedNote);
                // 更新
                await updateNote(selectedNote);
            } else {
                // 新增
                const res = await createNote(selectedNote);
                console.log('新增回傳 res:', res);
                console.log('HTTP 狀態碼:', res.status); // 你會看到 201
                if (res.status === 201) {
                    setSelectedNote(res.data); // 用整個物件覆蓋 selectedNote
                }
            }
            await loadNotes();
            setError('');
            setLoading(false);
            alert('儲存成功');
        } catch (err) {
            setError('儲存失敗');
            setLoading(false);
        }
    };

    // 刪除筆記
    const onDelete = async () => {
        console.log('刪除筆記:', selectedNote?.id);
        if (!selectedNote?.id) return alert('請選擇一筆筆記刪除');
        if (!window.confirm('確定刪除此筆記嗎？')) return;

        try {
            setLoading(true);
            await deleteNote(selectedNote.id);
            await loadNotes();
            setSelectedNote(null);
            setError('');
            setLoading(false);
            alert('刪除成功');
        } catch {
            setError('刪除失敗');
            setLoading(false);
        }
    };

    return (
        <>
            {/* 導航列 */}
            <nav className="navbar navbar-expand navbar-dark bg-primary">
                <div className="container">
                    <a className="navbar-brand" href="#">
                        筆記管理
                    </a>
                    <div className="ms-auto d-flex align-items-center">
                        <span className="text-white me-3">已登入</span>
                        <button className="btn btn-outline-light btn-sm" onClick={onLogout}>
                            登出
                        </button>
                    </div>
                </div>
            </nav>

            <div className="container mt-4">
                {error && <div className="alert alert-danger">{error}</div>}

                <div className="row">
                    {/* 筆記列表 */}
                    <div className="col-md-4 mb-3">
                        <div className="d-flex justify-content-between align-items-center mb-2">
                            <h5>筆記列表</h5>
                            <button className="btn btn-success btn-sm" onClick={onAddNew}>
                                新增筆記
                            </button>
                        </div>
                        <div className="list-group" style={{ maxHeight: '70vh', overflowY: 'auto' }}>
                            {loading && <div>載入中...</div>}
                            {!loading && notes.length === 0 && <div>尚無筆記</div>}
                            {notes.map((note) => (
                                <button
                                    key={note.id}
                                    className={`list-group-item list-group-item-action ${
                                        selectedNote?.id === note.id ? 'active' : ''
                                    }`}
                                    onClick={() => selectNote(note)}
                                >
                                    {note.title || <i>無標題</i>}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* 筆記編輯區 */}
                    <div className="col-md-8">
                        <div className="card">
                            <div className="card-header">
                                <h5>{selectedNote?.id ? '編輯筆記' : '新增筆記'}</h5>
                            </div>
                            <div className="card-body">
                                {!selectedNote ? (
                                    <div>請從左側選擇或新增筆記</div>
                                ) : (
                                    <>
                                        <div className="mb-3">
                                            <label className="form-label">標題</label>
                                            <input
                                                type="text"
                                                className="form-control"
                                                value={selectedNote.title}
                                                onChange={(e) => onChangeField('title', e.target.value)}
                                                placeholder="輸入標題"
                                            />
                                        </div>
                                        <div className="mb-3">
                                            <label className="form-label">內容</label>
                                            <textarea
                                                rows={10}
                                                className="form-control"
                                                value={selectedNote.content}
                                                onChange={(e) => onChangeField('content', e.target.value)}
                                                placeholder="輸入內容"
                                            />
                                        </div>
                                        <div className="d-flex justify-content-between">
                                            <button
                                                className="btn btn-danger"
                                                onClick={onDelete}
                                                disabled={!selectedNote.id}
                                            >
                                                刪除
                                            </button>
                                            <button className="btn btn-primary" onClick={onSave} disabled={loading}>
                                                {loading ? '儲存中...' : '儲存'}
                                            </button>
                                        </div>
                                    </>
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}
