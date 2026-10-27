import { useEffect, useState } from 'react';
import { Alert, Button, Result, Spin } from 'antd';
import { clearSession, getCurrentUser, getToken, sessionExpiredEvent } from './api/auth';
import { ApiError } from './api/client';
import type { UserResponse } from './types/auth';
import LoginPage from './pages/LoginPage';
import AdminPreviewPage from './pages/AdminPreviewPage';

export default function App() {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [checking, setChecking] = useState(Boolean(getToken()));
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);

  function logout() { clearSession(); setUser(null); setError(''); }

  useEffect(() => {
    const controller = new AbortController();
    const expired = () => { setUser(null); setError(''); };
    window.addEventListener(sessionExpiredEvent, expired);
    async function restoreSession() {
      if (!getToken()) { setChecking(false); return; }
      setChecking(true);
      setError('');
      try { setUser(await getCurrentUser(controller.signal)); }
      catch (cause) {
        if (controller.signal.aborted) return;
        if (cause instanceof ApiError && cause.status === 401) setUser(null);
        else setError('Chưa thể kiểm tra phiên đăng nhập. Kiểm tra kết nối máy chủ và thử lại.');
      } finally { if (!controller.signal.aborted) setChecking(false); }
    }
    void restoreSession();
    return () => { controller.abort(); window.removeEventListener(sessionExpiredEvent, expired); };
  }, [attempt]);

  if (checking) return <div className="session-loading"><Spin size="large" /><p>Đang kiểm tra phiên đăng nhập…</p></div>;
  if (error) return <div className="session-loading"><Alert type="error" showIcon message={error} /><Button onClick={() => setAttempt(value => value + 1)}>Thử lại</Button><Button type="text" onClick={logout}>Về đăng nhập</Button></div>;
  if (!user) return <LoginPage onLogin={setUser} />;
  if (user.role !== 'Admin') return <Result status="403" title="Khu vực quản trị" subTitle="Tài khoản này không thuộc vai trò quản trị. Cổng nhân viên và kỹ thuật viên chưa được triển khai." extra={<Button onClick={logout}>Đăng xuất</Button>} />;
  return <AdminPreviewPage user={user} onLogout={logout} />;
}
