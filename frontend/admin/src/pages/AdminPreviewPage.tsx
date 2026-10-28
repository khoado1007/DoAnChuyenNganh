import AgentsPage from './AgentsPage';
import AdminLayout from '../layouts/AdminLayout';
import type { Section } from '../layouts/AdminLayout';
import type { UserResponse } from '../types/auth';
import { useMemo, useState } from 'react';
import {
  ArrowDownOutlined,
  ArrowUpOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  DesktopOutlined,
  FileSearchOutlined,
  FilterOutlined,
  LaptopOutlined,
  PlusOutlined,
  SearchOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import {
  Avatar,
  Button,
  Card,
  Empty,
  Form,
  Input,
  Modal,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
  message,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';

const { Text, Title } = Typography;

type AssetStatus = 'Đang sử dụng' | 'Trong kho' | 'Bảo trì';
type TicketStatus = 'Mới' | 'Đang xử lý' | 'Đã giải quyết' | 'Đã đóng';
type Row = {
  key: string;
  code: string;
  name: string;
  category?: string;
  owner?: string;
  department?: string;
  status: AssetStatus | TicketStatus | 'Hoạt động';
  updated: string;
  priority?: 'Cao' | 'Trung bình' | 'Thấp';
};

const initialAssets: Row[] = [
  { key: '1', code: 'AST-0248', name: 'MacBook Pro 14”', category: 'Máy tính xách tay', owner: 'Nguyễn Minh Anh', status: 'Đang sử dụng', updated: '22/10/2026' },
  { key: '2', code: 'AST-0247', name: 'Dell UltraSharp U2723QE', category: 'Màn hình', owner: 'Trần Quốc Bảo', status: 'Đang sử dụng', updated: '21/10/2026' },
  { key: '3', code: 'AST-0246', name: 'ThinkPad X1 Carbon Gen 11', category: 'Máy tính xách tay', owner: '—', status: 'Trong kho', updated: '20/10/2026' },
  { key: '4', code: 'AST-0245', name: 'HP LaserJet Pro M404', category: 'Máy in', owner: 'Văn phòng tầng 3', status: 'Bảo trì', updated: '19/10/2026' },
  { key: '5', code: 'AST-0244', name: 'Logitech Brio 4K', category: 'Thiết bị ngoại vi', owner: 'Lê Thu Hà', status: 'Đang sử dụng', updated: '17/10/2026' },
];

const initialUsers: Row[] = [
  { key: '1', code: 'NV-0102', name: 'Nguyễn Minh Anh', category: 'Nhân viên', department: 'Sản phẩm', status: 'Hoạt động', updated: '22/10/2026' },
  { key: '2', code: 'NV-0098', name: 'Trần Quốc Bảo', category: 'Kỹ thuật viên', department: 'Công nghệ thông tin', status: 'Hoạt động', updated: '21/10/2026' },
  { key: '3', code: 'NV-0087', name: 'Lê Thu Hà', category: 'Nhân viên', department: 'Nhân sự', status: 'Hoạt động', updated: '20/10/2026' },
  { key: '4', code: 'NV-0061', name: 'Phạm Đức Long', category: 'Nhân viên', department: 'Tài chính', status: 'Hoạt động', updated: '18/10/2026' },
];

const initialTickets: Row[] = [
  { key: '1', code: 'REQ-1284', name: 'Không thể kết nối VPN công ty', owner: 'Nguyễn Minh Anh', category: 'Mạng & kết nối', priority: 'Cao', status: 'Đang xử lý', updated: '22/10/2026 · 09:42' },
  { key: '2', code: 'REQ-1283', name: 'Yêu cầu cài đặt phần mềm Figma', owner: 'Lê Thu Hà', category: 'Phần mềm', priority: 'Trung bình', status: 'Mới', updated: '22/10/2026 · 09:18' },
  { key: '3', code: 'REQ-1282', name: 'Màn hình nhấp nháy khi sử dụng', owner: 'Trần Quốc Bảo', category: 'Thiết bị', priority: 'Trung bình', status: 'Đã giải quyết', updated: '21/10/2026 · 16:30' },
  { key: '4', code: 'REQ-1281', name: 'Xin cấp quyền thư mục dùng chung', owner: 'Phạm Đức Long', category: 'Tài khoản & quyền', priority: 'Thấp', status: 'Đang xử lý', updated: '21/10/2026 · 14:05' },
];


const titles: Record<Section, { title: string; description: string }> = {
  agents: { title: 'Agent', description: 'Đăng ký và theo dõi kết nối Agent.' },
  overview: { title: 'Tổng quan', description: 'Theo dõi nhanh tình hình tài sản và dịch vụ CNTT.' },
  assets: { title: 'Tài sản', description: 'Danh sách thiết bị và trạng thái cấp phát hiện tại.' },
  users: { title: 'Tài khoản', description: 'Tài khoản nội bộ và vai trò trong hệ thống.' },
  tickets: { title: 'Yêu cầu hỗ trợ', description: 'Tiếp nhận và theo dõi các yêu cầu hỗ trợ CNTT.' },
};

function StatusTag({ status }: { status: Row['status'] }) {
  const colors: Record<string, string> = {
    'Đang sử dụng': 'green',
    'Trong kho': 'blue',
    'Bảo trì': 'gold',
    'Mới': 'blue',
    'Đang xử lý': 'gold',
    'Đã giải quyết': 'green',
    'Đã đóng': 'default',
    'Hoạt động': 'green',
  };
  return <Tag color={colors[status]}>{status}</Tag>;
}

function AdminPreviewPage({ user, onLogout }: { user: UserResponse; onLogout: () => void }) {
  const [section, setSection] = useState<Section>('overview');
  const [query, setQuery] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [assets, setAssets] = useState(initialAssets);
  const [users, setUsers] = useState(initialUsers);
  const [tickets, setTickets] = useState(initialTickets);
  const [form] = Form.useForm();
  const [messageApi, contextHolder] = message.useMessage();

  const currentRows = section === 'assets' ? assets : section === 'users' ? users : tickets;
  const filteredRows = useMemo(() => currentRows.filter((row) => `${row.code} ${row.name} ${row.owner ?? ''} ${row.category ?? ''} ${row.department ?? ''}`.toLocaleLowerCase('vi').includes(query.toLocaleLowerCase('vi'))), [currentRows, query]);

  const columns: ColumnsType<Row> = section === 'assets' ? [
    { title: 'MÃ TÀI SẢN', dataIndex: 'code', width: 125, render: (value: string) => <Text className="code-text">{value}</Text> },
    { title: 'TÊN THIẾT BỊ', dataIndex: 'name', render: (value: string, row: Row) => <div className="record-cell"><span className="record-icon"><DesktopOutlined /></span><span><Text strong>{value}</Text><Text className="cell-subtitle">{row.category}</Text></span></div> },
    { title: 'NGƯỜI ĐANG GIỮ', dataIndex: 'owner', render: (value: string) => value === '—' ? <Text type="secondary">Chưa cấp phát</Text> : value },
    { title: 'TRẠNG THÁI', dataIndex: 'status', render: (value: Row['status']) => <StatusTag status={value} /> },
    { title: 'CẬP NHẬT', dataIndex: 'updated', width: 130 },
  ] : section === 'users' ? [
    { title: 'MÃ NHÂN VIÊN', dataIndex: 'code', width: 130, render: (value: string) => <Text className="code-text">{value}</Text> },
    { title: 'HỌ VÀ TÊN', dataIndex: 'name', render: (value: string, row: Row) => <div className="record-cell"><Avatar size={34} className="user-avatar">{value.split(' ').slice(-1)[0]?.slice(0, 1)}</Avatar><span><Text strong>{value}</Text><Text className="cell-subtitle">{row.category}</Text></span></div> },
    { title: 'PHÒNG BAN', dataIndex: 'department' },
    { title: 'TRẠNG THÁI', dataIndex: 'status', render: (value: Row['status']) => <StatusTag status={value} /> },
    { title: 'CẬP NHẬT', dataIndex: 'updated', width: 130 },
  ] : [
    { title: 'MÃ YÊU CẦU', dataIndex: 'code', width: 130, render: (value: string) => <Text className="code-text">{value}</Text> },
    { title: 'NỘI DUNG', dataIndex: 'name', render: (value: string, row: Row) => <div><Text strong>{value}</Text><Text className="cell-subtitle">{row.category}</Text></div> },
    { title: 'NGƯỜI GỬI', dataIndex: 'owner', width: 150 },
    { title: 'ƯU TIÊN', dataIndex: 'priority', width: 115, render: (value?: Row['priority']) => value && <span className={`priority priority-${value === 'Cao' ? 'high' : value === 'Thấp' ? 'low' : 'medium'}`}><span />{value}</span> },
    { title: 'TRẠNG THÁI', dataIndex: 'status', width: 145, render: (value: Row['status']) => <StatusTag status={value} /> },
    { title: 'CẬP NHẬT', dataIndex: 'updated', width: 160 },
  ];

  function openCreate() {
    form.resetFields();
    setModalOpen(true);
  }

  function createRecord(values: { name: string; category?: string; owner?: string; department?: string }) {
    const key = String(Date.now());
    const today = '22/10/2026';
    if (section === 'assets') setAssets((rows) => [{ key, code: `AST-${String(rows.length + 249).padStart(4, '0')}`, name: values.name, category: values.category || 'Thiết bị', owner: values.owner || '—', status: values.owner ? 'Đang sử dụng' : 'Trong kho', updated: today }, ...rows]);
    else if (section === 'users') setUsers((rows) => [{ key, code: `NV-${String(rows.length + 103).padStart(4, '0')}`, name: values.name, category: values.category || 'Nhân viên', department: values.department || 'Chưa phân phòng ban', status: 'Hoạt động', updated: today }, ...rows]);
    else setTickets((rows) => [{ key, code: `REQ-${1285 + rows.length}`, name: values.name, owner: 'Quản trị viên', category: values.category || 'Khác', priority: 'Trung bình', status: 'Mới', updated: today }, ...rows]);
    setModalOpen(false);
    messageApi.success('Đã thêm vào dữ liệu xem trước.');
  }


  const title = titles[section];
  const createLabel = section === 'assets' ? 'Thêm tài sản' : section === 'users' ? 'Thêm tài khoản' : 'Tạo yêu cầu';

  if (section === 'agents') return <AdminLayout user={user} onLogout={onLogout} section={section} title={title.title} onSectionChange={setSection}><AgentsPage /></AdminLayout>;

  return (
    <>
      {contextHolder}
      <AdminLayout user={user} onLogout={onLogout} section={section} title={title.title} onSectionChange={(next) => { setSection(next); setQuery(''); }}>
        <main className="page-content">
          <div className="preview-banner"><span className="preview-dot" /><span><b>Bản xem trước giao diện</b><span className="banner-separator">·</span>Các bảng vẫn dùng dữ liệu minh họa — đăng nhập đã kết nối API</span></div>
          <div className="page-heading"><div><Text className="eyebrow">{section === 'overview' ? 'THỨ NĂM, 22 THÁNG 10, 2026' : 'QUẢN TRỊ HỆ THỐNG'}</Text><Title level={1}>{title.title}</Title><Text className="page-description">{title.description}</Text></div>{section !== 'overview' && <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>{createLabel}</Button>}</div>

          {section === 'overview' ? <>
            <div className="metric-grid">
              <Card className="metric-card"><div className="metric-head"><span className="metric-icon navy"><LaptopOutlined /></span><span className="metric-period">Toàn hệ thống</span></div><Statistic title="Tổng tài sản" value={248} /><div className="metric-foot"><span className="metric-up"><ArrowUpOutlined /> 12</span><span>so với tháng trước</span></div></Card>
              <Card className="metric-card"><div className="metric-head"><span className="metric-icon teal"><TeamOutlined /></span><span className="metric-period">Tài khoản nội bộ</span></div><Statistic title="Người dùng" value={186} /><div className="metric-foot"><span className="metric-up"><ArrowUpOutlined /> 8</span><span>so với tháng trước</span></div></Card>
              <Card className="metric-card"><div className="metric-head"><span className="metric-icon amber"><ClockCircleOutlined /></span><span className="metric-period">Cần theo dõi</span></div><Statistic title="Yêu cầu đang mở" value={12} /><div className="metric-foot"><span className="metric-down"><ArrowDownOutlined /> 3</span><span>so với tuần trước</span></div></Card>
              <Card className="metric-card"><div className="metric-head"><span className="metric-icon green"><CheckCircleOutlined /></span><span className="metric-period">Tháng này</span></div><Statistic title="Đã giải quyết" value={34} /><div className="metric-foot"><span className="metric-neutral">Trong tháng 10</span></div></Card>
            </div>

            <div className="overview-grid">
              <Card className="panel-card ticket-panel" title={<div className="panel-heading"><span><Title level={4}>Yêu cầu gần đây</Title><Text>Những yêu cầu mới nhất cần được theo dõi</Text></span><Button type="link" onClick={() => setSection('tickets')}>Xem tất cả <span className="link-arrow">→</span></Button></div>}>
                <div className="table-scroll"><Table<Row> className="data-table compact-table" columns={[
                  { title: 'YÊU CẦU', dataIndex: 'name', render: (value: string, row: Row) => <div><Text className="code-text">{row.code}</Text><Text className="row-title">{value}</Text></div> },
                  { title: 'NGƯỜI GỬI', dataIndex: 'owner', width: 145 },
                  { title: 'ƯU TIÊN', dataIndex: 'priority', width: 120, render: (value: Row['priority']) => <span className={`priority priority-${value === 'Cao' ? 'high' : value === 'Thấp' ? 'low' : 'medium'}`}><span />{value}</span> },
                  { title: 'TRẠNG THÁI', dataIndex: 'status', width: 140, render: (value: Row['status']) => <StatusTag status={value} /> },
                ]} dataSource={tickets.slice(0, 4)} pagination={false} size="middle" /> </div>
              </Card>
              <Card className="panel-card status-panel" title={<div className="panel-heading"><span><Title level={4}>Tình trạng tài sản</Title><Text>Phân bổ theo trạng thái</Text></span><span className="small-date">Tháng 10</span></div>}>
                <div className="asset-summary"><div className="donut-wrap"><div className="donut"><svg viewBox="0 0 120 120" aria-label="Biểu đồ tài sản: 186 đang sử dụng, 42 trong kho, 20 bảo trì" role="img"><circle className="donut-track" cx="60" cy="60" r="48" /><circle className="donut-segment in-use-segment" cx="60" cy="60" r="48" /><circle className="donut-segment in-stock-segment" cx="60" cy="60" r="48" /><circle className="donut-segment maintenance-segment" cx="60" cy="60" r="48" /></svg><div className="donut-center"><b>248</b><small>tài sản</small></div></div></div><div className="legend-list"><div><span className="legend-dot in-use" /><span>Đang sử dụng</span><b>186</b></div><div><span className="legend-dot in-stock" /><span>Trong kho</span><b>42</b></div><div><span className="legend-dot maintenance" /><span>Bảo trì</span><b>20</b></div></div></div>
                <div className="panel-divider" /><div className="asset-summary-foot"><span>Tỷ lệ sử dụng</span><b>75%</b></div>
              </Card>
            </div>
            <div className="quick-actions"><div><Text className="eyebrow">THAO TÁC NHANH</Text><Title level={4}>Bắt đầu công việc</Title></div><button onClick={() => { setSection('assets'); openCreate(); }}><span className="quick-icon asset"><LaptopOutlined /></span><span><b>Thêm tài sản</b><small>Ghi nhận thiết bị mới</small></span><span className="quick-arrow">→</span></button><button onClick={() => { setSection('users'); openCreate(); }}><span className="quick-icon user"><TeamOutlined /></span><span><b>Tạo tài khoản</b><small>Thêm người dùng nội bộ</small></span><span className="quick-arrow">→</span></button><button onClick={() => { setSection('tickets'); openCreate(); }}><span className="quick-icon ticket"><FileSearchOutlined /></span><span><b>Tạo yêu cầu</b><small>Ghi nhận hỗ trợ CNTT</small></span><span className="quick-arrow">→</span></button></div>
          </> : <Card className="listing-card">
            <div className="listing-toolbar"><div><Title level={4}>Danh sách {section === 'assets' ? 'tài sản' : section === 'users' ? 'tài khoản' : 'yêu cầu'}</Title><Text>{filteredRows.length} bản ghi <span className="toolbar-separator">·</span> Dữ liệu minh họa</Text></div><Space size={10} className="listing-filters"><Input allowClear prefix={<SearchOutlined />} placeholder={section === 'assets' ? 'Tìm mã, tên thiết bị...' : section === 'users' ? 'Tìm tên, mã nhân viên...' : 'Tìm yêu cầu, người gửi...'} value={query} onChange={(event) => setQuery(event.target.value)} /><Button icon={<FilterOutlined />} onClick={() => messageApi.info('Bộ lọc nâng cao sẽ khả dụng khi API được kết nối.')}>Bộ lọc</Button></Space></div>
            {filteredRows.length ? <div className="table-scroll"><Table<Row> className="data-table" columns={columns} dataSource={filteredRows} pagination={{ pageSize: 8, showSizeChanger: false, showTotal: (total) => `Tổng ${total} bản ghi`, position: ['bottomRight'] }} scroll={{ x: section === 'tickets' ? 950 : 760 }} /></div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không tìm thấy bản ghi phù hợp" />}
          </Card>}
          <footer className="page-footer"><span>IT Desk <span className="footer-separator">·</span> Cổng quản trị nội bộ</span><span>Giao diện xem trước <span className="footer-separator">·</span> 2026</span></footer>
        </main>
      </AdminLayout>

      <Modal title={createLabel} open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnClose className="form-modal">
        <Text className="modal-description">Thông tin được thêm vào dữ liệu xem trước trên trình duyệt.</Text>
        <Form form={form} layout="vertical" requiredMark="optional" onFinish={createRecord}>
          <Form.Item name="name" label={section === 'assets' ? 'Tên tài sản' : section === 'users' ? 'Họ và tên' : 'Tiêu đề yêu cầu'} rules={[{ required: true, message: 'Vui lòng nhập thông tin này.' }]}><Input autoFocus placeholder={section === 'assets' ? 'Ví dụ: MacBook Pro 14”' : section === 'users' ? 'Ví dụ: Nguyễn Minh Anh' : 'Mô tả ngắn gọn vấn đề'} /></Form.Item>
          {section === 'assets' && <><Form.Item name="category" label="Loại thiết bị"><Select placeholder="Chọn loại thiết bị" options={['Máy tính xách tay', 'Màn hình', 'Máy in', 'Thiết bị ngoại vi'].map((value) => ({ label: value, value }))} /></Form.Item><Form.Item name="owner" label="Người đang giữ" rules={[]}><Input placeholder="Để trống nếu chưa cấp phát" /></Form.Item></>}
          {section === 'users' && <><Form.Item name="category" label="Vai trò"><Select placeholder="Chọn vai trò" options={['Nhân viên', 'Kỹ thuật viên', 'Quản trị viên'].map((value) => ({ label: value, value }))} /></Form.Item><Form.Item name="department" label="Phòng ban"><Input placeholder="Ví dụ: Công nghệ thông tin" /></Form.Item></>}
          {section === 'tickets' && <Form.Item name="category" label="Danh mục"><Select placeholder="Chọn danh mục" options={['Thiết bị', 'Mạng & kết nối', 'Phần mềm', 'Tài khoản & quyền', 'Khác'].map((value) => ({ label: value, value }))} /></Form.Item>}
          <div className="modal-actions"><Button onClick={() => setModalOpen(false)}>Hủy</Button><Button type="primary" htmlType="submit">Thêm vào bản xem trước</Button></div>
        </Form>
      </Modal>
    </>
  );
}

export default AdminPreviewPage;
