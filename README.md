# IT Desk — MVC refactor

## Tiến độ

Phase A: backend MVC/API foundation và đăng nhập thật. React là View; ASP.NET Core Controller nhận request/trả JSON, Application định nghĩa contract và interface Service, Infrastructure triển khai Identity/Repository, Domain giữ entity.

Các bảng tài khoản/tài sản/ticket và dashboard trong `AdminPreviewPage` **vẫn là dữ liệu minh họa**, có nhãn trên giao diện. Thao tác thêm ở đó chưa lưu DB. Phase B nối các module này; Phase C nối dashboard/quota và bỏ dữ liệu mẫu. Chưa triển khai cổng Employee/Technician hay quản lý tệp.

## Chạy cục bộ

Yêu cầu: .NET SDK 10, Node.js 24 (để chạy test TypeScript bằng Node), Docker Desktop đang chạy.

Tại thư mục gốc, khởi động PostgreSQL:

```powershell
docker compose -f backend/docker-compose.yml up -d postgres
dotnet run --project backend/src/ITManagement.API --launch-profile http
```

API: `http://localhost:5038`, Swagger: `http://localhost:5038/swagger`.
Ứng dụng tự áp dụng migration và seed. Database local là `itmanagement_dev`; Docker Compose API dùng `itmanagement`.

Terminal khác:

```powershell
cd frontend/admin
npm ci
npm run dev -- --host 127.0.0.1
```

Mở `http://localhost:5173`. Vite proxy `/api` đến API ở cổng 5038. Khi dùng Docker API ở 8080, tạo `.env.local` từ `.env.example` và đổi `API_PROXY_TARGET`.

Tài khoản seed cục bộ: **admin / Admin@12345**. Seed chạy lại không đổi mật khẩu và không tạo admin trùng. Đăng xuất xóa token trong tab; reload kiểm tra token bằng `/api/auth/me`. Không có refresh-token flow ở Phase A.

## API hiện có

| Method | Route | Chức năng |
|---|---|---|
| POST | `/api/auth/login` | `{ "username": "admin", "password": "..." }` → accessToken và user |
| GET | `/api/auth/me` | Thông tin tài khoản từ Bearer token |

Response: `{ success, message, data, errors }`, thuộc tính camelCase; enum là tên string (`Admin`, `Active`, ...). Sai thông tin đăng nhập trả 401; dữ liệu nhập sai trả 400 với lỗi theo trường; thiếu quyền trả 403.

DTO của Users/Assets/Tickets/Dashboard đã chuẩn bị, endpoint tương ứng sẽ được triển khai ở Phase B/C. Mã tài sản được nhập rõ ràng; không tự tạo mã từ số dòng frontend. Ticket dùng sequence toàn cục PostgreSQL, không reset theo năm và có thể có khoảng trống sau rollback.

## Kiểm thử

```powershell
dotnet test backend/ITManagement.slnx
cd frontend/admin
npm test
npm run build
```

Integration test cần PostgreSQL ở localhost:5432. Có thể đặt `TEST_POSTGRES` để đổi host/port/tài khoản. Test tự tạo database `itmanagement_test_<guid>` và xóa đúng database đó khi kết thúc; tài khoản test cần quyền tạo DB. Không dùng database phát triển để chạy test.

Migration tooling (cài cục bộ, thư mục `.tools` được ignore):

```powershell
dotnet tool install dotnet-ef --tool-path backend/.tools --version 10.0.13
backend/.tools/dotnet-ef migrations has-pending-model-changes --project backend/src/ITManagement.Infrastructure --startup-project backend/src/ITManagement.Infrastructure
```

## Giới hạn đang giữ lại

- Chưa harden bảo mật/deploy; tiếp tục sử dụng Identity/JWT và chính sách có sẵn.
- Chưa tối ưu bundle frontend; cảnh báo bundle lớn vẫn tồn tại.
- EF có cảnh báo quan hệ bắt buộc với soft-delete query filter trong model cũ; luồng xóa/lịch sử chưa thuộc phase này.
- MediatR/AutoMapper và behavior cũ sẽ dọn khi chuyển hết module, không đưa chúng vào luồng đăng nhập mới.
