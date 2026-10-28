import { useEffect, useState } from 'react';
import { Alert, Button, Card, Form, Input, Modal, Popconfirm, Select, Space, Table, Tag, Typography } from 'antd';
import { api } from '../api/auth';

type Asset = { id: string; assetCode: string; name: string; status: string };
type Agent = { id: string; assetId: string; instanceId: string; assetCode: string; assetStatus: string; mode: 'real' | 'simulated'; policy: string; connectivity: string; lastSeenAt: string | null; hostname: string; os: string; cpu: string; ramBytes: number; agentVersion: string };
type Token = { token: string; expiresAt: string };

export default function AgentsPage() {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [assets, setAssets] = useState<Asset[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [token, setToken] = useState<Token | null>(null);
  const [assetForm] = Form.useForm();
  const [tokenForm] = Form.useForm();

  async function refresh(signal?: AbortSignal) {
    try {
      const [rows, inventory] = await Promise.all([api<Agent[]>('/agents', { signal }), api<Asset[]>('/assets', { signal })]);
      if (signal?.aborted) return;
      setAgents(rows); setAssets(inventory); setError('');
    } catch (cause) {
      if (!signal?.aborted) setError(cause instanceof Error ? cause.message : 'Không tải được danh sách Agent.');
    } finally { if (!signal?.aborted) setLoading(false); }
  }
  useEffect(() => {
    const controller = new AbortController();
    void refresh(controller.signal);
    const timer = window.setInterval(() => void refresh(controller.signal), 15000);
    return () => { controller.abort(); window.clearInterval(timer); };
  }, []);

  async function mutate(action: () => Promise<void>) {
    setBusy(true); setError('');
    try { await action(); await refresh(); }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'Thao tác thất bại.'); }
    finally { setBusy(false); }
  }

  const available = assets.filter(asset => !agents.some(agent => agent.assetId === asset.id) && !['Disabled', 'Retired'].includes(asset.status));
  return <main className="page-content">
    <div className="page-heading"><div><Typography.Title level={1}>Agent</Typography.Title><Typography.Text>Dữ liệu trực tiếp từ máy chủ · Tự làm mới mỗi 15 giây</Typography.Text></div><Button loading={loading} onClick={() => void refresh()}>Làm mới</Button></div>
    {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}
    <Alert type="info" showIcon message="Real thu thập thông tin máy khi được đồng ý; Simulated gửi dữ liệu giả lập. Hai Agent chạy trên một máy không đại diện cho hai máy vật lý." style={{ marginBottom: 16 }} />
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card title="Chuẩn bị đăng ký">
        <Form form={assetForm} layout="inline" onFinish={values => void mutate(async () => { await api('/assets', { method: 'POST', body: JSON.stringify(values) }); assetForm.resetFields(); })}>
          <Form.Item name="assetCode" label="Mã tài sản" rules={[{ required: true, whitespace: true }]}><Input maxLength={50} placeholder="PC-001" /></Form.Item>
          <Form.Item name="name" label="Tên tài sản" rules={[{ required: true, whitespace: true }]}><Input maxLength={200} placeholder="Máy phòng IT" /></Form.Item>
          <Form.Item><Button htmlType="submit" loading={busy}>Tạo tài sản</Button></Form.Item>
        </Form>
        <Form form={tokenForm} layout="vertical" initialValues={{ mode: 'real' }} style={{ marginTop: 20, maxWidth: 560 }} onFinish={values => void mutate(async () => { setToken(await api<Token>('/agents/enrollment-tokens', { method: 'POST', body: JSON.stringify(values) })); })}>
          <Form.Item name="assetId" label="Tài sản chưa đăng ký Agent" rules={[{ required: true }]}><Select loading={loading} placeholder="Chọn tài sản" options={available.map(asset => ({ value: asset.id, label: `${asset.assetCode} · ${asset.name}` }))} /></Form.Item>
          <Form.Item name="mode" label="Chế độ" rules={[{ required: true }]}><Select options={[{ value: 'real', label: 'Real — máy thực' }, { value: 'simulated', label: 'Simulated — dữ liệu giả lập' }]} /></Form.Item>
          <Button type="primary" htmlType="submit" loading={busy} disabled={available.length === 0}>Cấp token đăng ký</Button>
          <Typography.Paragraph type="secondary" style={{ marginTop: 8 }}>Token có hiệu lực 15 phút, dùng một lần. Cấp token mới sẽ hủy token cũ chưa dùng của tài sản.</Typography.Paragraph>
        </Form>
      </Card>
      <Card title="Agent đã đăng ký">
        <Table<Agent> rowKey="id" loading={loading} dataSource={agents} scroll={{ x: 1150 }} locale={{ emptyText: 'Chưa có Agent đăng ký' }} pagination={{ pageSize: 10 }} columns={[
          { title: 'Tài sản / Agent', key: 'identity', render: (_: unknown, row: Agent) => <><b>{row.assetCode}</b><div>{row.hostname || 'Chưa gửi heartbeat'}</div><Typography.Text type="secondary">{row.instanceId}</Typography.Text></> },
          { title: 'Chế độ', dataIndex: 'mode', render: (value: string) => <Tag color={value === 'simulated' ? 'orange' : 'blue'}>{value === 'simulated' ? 'Simulated' : 'Real'}</Tag> },
          { title: 'Kết nối', dataIndex: 'connectivity', render: (value: string) => <Tag color={value === 'online' ? 'green' : 'default'}>{value === 'online' ? 'Online' : 'Offline'}</Tag> },
          { title: 'Chính sách', dataIndex: 'policy', render: (value: string) => value === 'active' ? 'Hoạt động' : 'Đã vô hiệu hóa' },
          { title: 'Vòng đời tài sản', dataIndex: 'assetStatus' },
          { title: 'Heartbeat cuối', dataIndex: 'lastSeenAt', render: (value: string) => value ? new Date(value).toLocaleString('vi-VN') : 'Chưa có' },
          { title: 'Inventory', key: 'inventory', render: (_: unknown, row: Agent) => <>{row.os}<div>{row.cpu}</div>{row.ramBytes > 0 && <div>{(row.ramBytes / 1024 ** 3).toFixed(1)} GB RAM · v{row.agentVersion}</div>}</> },
          { title: 'Thao tác', key: 'actions', render: (_: unknown, row: Agent) => <Popconfirm title="Vô hiệu hóa Agent?" description="Máy chủ sẽ từ chối heartbeat tiếp theo. Lịch sử vẫn được giữ." onConfirm={() => mutate(async () => { await api(`/agents/${row.id}/disable`, { method: 'POST' }); })} disabled={row.policy === 'disabled' || busy}><Button danger disabled={row.policy === 'disabled' || busy}>Vô hiệu hóa</Button></Popconfirm> },
        ]} />
        <Typography.Text type="secondary">Offline sau 90 giây không có heartbeat mới. Kết nối, chính sách Agent và vòng đời tài sản là các trạng thái độc lập.</Typography.Text>
      </Card>
    </Space>
    <Modal title="Token đăng ký — chỉ hiển thị lần này" open={token !== null} onCancel={() => setToken(null)} footer={<Button onClick={() => setToken(null)}>Đã lưu, đóng</Button>} destroyOnClose>
      {token && <><Typography.Paragraph>Nhập token vào lời nhắc của Python Agent. Không gửi token qua chat hay lưu vào source code.</Typography.Paragraph><Input.TextArea aria-label="Token đăng ký" readOnly value={token.token} autoSize /><Typography.Paragraph>Hết hạn: {new Date(token.expiresAt).toLocaleString('vi-VN')}</Typography.Paragraph></>}
    </Modal>
  </main>;
}

