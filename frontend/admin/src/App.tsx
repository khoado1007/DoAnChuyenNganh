import { lazy, useEffect, useState } from 'react';
import { Alert, Button, Result, Spin } from 'antd';
import { clearSession, getCurrentUser, getToken, sessionExpiredEvent } from './api/auth';
import { ApiError } from './api/client';
import type { UserResponse } from './types/auth';
import LoginPage from './pages/LoginPage';
const AdminPreviewPage = lazy(() => import('./pages/AdminPreviewPage'));
const PortalPage = lazy(() => import('./pages/PortalPage'));
import { destinationAfterLogin, homeForRole, resolveRoute } from './routing';

export default function App() {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [checking, setChecking] = useState(Boolean(getToken()));
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [path, setPath] = useState(window.location.pathname);
  function navigate(next: string, replace = false) {
    if (replace) window.history.replaceState(null, '', next);
    else window.history.pushState(null, '', next);
    setPath(next);
  }
  function logout() { clearSession(); setUser(null); setError(''); navigate('/login', true); }
  useEffect(() => {
    const changed = () => setPath(window.location.pathname);
    window.addEventListener('popstate', changed);
    return () => window.removeEventListener('popstate', changed);
  }, []);
  useEffect(() => {
    if (user && ['/', '/login', '/login/'].includes(path)) navigate(homeForRole(user.role), true);
  }, [user, path]);
  useEffect(() => {
    const controller = new AbortController();
    const expired = () => { setUser(null); setError(''); navigate('/login', true); };
    window.addEventListener(sessionExpiredEvent, expired);
    async function restoreSession() {
      const restoringToken = getToken();
      if (!restoringToken) { setChecking(false); return; }
      setChecking(true); setError('');
      try { const restored = await getCurrentUser(controller.signal); if (!controller.signal.aborted && getToken() === restoringToken) setUser(restored); }
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
  if (!user) return <LoginPage onLogin={value => { setUser(value); navigate(destinationAfterLogin(path, value.role), true); }} />;
  const route = resolveRoute(['/', '/login', '/login/'].includes(path) ? homeForRole(user.role) : path, user.role);
  if (route.kind === 'forbidden' || route.kind === 'not-found') return <Result status={route.kind === 'forbidden' ? '403' : '404'} title={route.kind === 'forbidden' ? 'Bạn không có quyền truy cập khu vực này' : 'Không tìm thấy trang'} extra={<><Button onClick={() => navigate(homeForRole(user.role), true)}>Về trang của tôi</Button><Button onClick={logout}>Đăng xuất</Button></>} />;
  if (route.kind === 'admin') return <AdminPreviewPage user={user} onLogout={logout} section={route.section} setSection={section => navigate(section === 'overview' ? '/admin' : `/admin/${section}`)} />;
  return <PortalPage user={user} onLogout={logout} />;
}

