import { Component, Suspense, type ReactNode } from 'react';
export default class PageBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false };
  static getDerivedStateFromError() { return { failed: true }; }
  render() {
    if (this.state.failed) return <div className="session-loading" role="alert"><p>Không tải được trang. Kiểm tra kết nối và tải lại.</p><button onClick={() => window.location.reload()}>Tải lại trang</button></div>;
    return <Suspense fallback={<div className="session-loading" role="status">Đang tải trang…</div>}>{this.props.children}</Suspense>;
  }
}
