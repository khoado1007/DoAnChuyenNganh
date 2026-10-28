# IT Management

Ứng dụng quản lý tài sản và hỗ trợ CNTT, gồm React + TypeScript, ASP.NET Core Web API, PostgreSQL và Python Agent. Hiện tại luồng Agent đã kết nối API thật; một số màn hình quản trị khác vẫn là bản xem trước dùng dữ liệu minh họa.

## Thành phần

- `backend/`: API .NET 10 theo các lớp API, Application, Domain và Infrastructure; PostgreSQL qua Entity Framework Core.
- `frontend/admin/`: ứng dụng React 19 + TypeScript + Vite cho giao diện quản trị.
- `agent/`: một Python Agent chạy ở chế độ `real` hoặc `simulated`.
- `itam-codex-engineering/`: hướng dẫn kỹ thuật, skills và roadmap; không phải mã chạy của ứng dụng.

## Chạy local bằng Docker Compose

Cần Docker Desktop đang chạy. Từ thư mục gốc repository:

```powershell
docker compose -f backend/docker-compose.yml up --build
```

Sau khi các container khởi động:

- Web: <http://localhost:8088>
- API: <http://localhost:8080>
- Swagger: không bật trong cấu hình Compose Production; chạy API ở Development để dùng `/swagger`.

API tự chạy migration và seed dữ liệu khi khởi động. Tài khoản Admin mặc định hiện được seed trong mã nguồn:

- Email: `admin@itmanagement.local`
- Mật khẩu: `Admin@12345`

Đây là thông tin demo, không an toàn cho môi trường dùng chung hoặc public. Compose hiện dùng mật khẩu PostgreSQL mặc định và JWT có giá trị fallback; chỉ chạy trong máy local đáng tin cậy, không mở các cổng ra mạng công cộng. Hãy thay cấu hình bảo mật và thiết kế bootstrap phù hợp trước khi triển khai. Thay đổi `JWT_SECRET` một mình không đủ để bảo vệ database.

Compose lưu dữ liệu PostgreSQL và file trong named volumes. Dừng ứng dụng mà vẫn giữ dữ liệu:

```powershell
docker compose -f backend/docker-compose.yml down
```

Không thêm `-v` nếu muốn giữ dữ liệu; tùy chọn đó xóa các volumes.

## Python Agent

Agent cần API và PostgreSQL đang chạy. Đăng nhập bằng tài khoản Admin, mở trang Agent, tạo tài sản nếu cần rồi cấp enrollment token. Token chỉ hiển thị một lần; không đưa token hoặc credential vào source control hay command line.

Hướng dẫn cài Python, enrollment, heartbeat, chế độ `real`/`simulated` và giới hạn bảo mật nằm trong [agent/README.md](agent/README.md). Hai Agent chạy trên cùng một máy không đại diện cho hai thiết bị vật lý. Chế độ `real` chỉ gửi inventory khi có đồng ý rõ ràng.

## Chạy frontend và API riêng

Để phát triển giao diện ngoài Docker, cần Node.js và npm:

```powershell
cd frontend/admin
npm ci
npm run dev
```

Vite in URL local sau khi khởi động. API phải được chạy riêng và cấu hình kết nối PostgreSQL, JWT, CORS và file storage tương ứng trong `backend/src/ITManagement.API/appsettings*.json` hoặc biến môi trường. Không dùng cấu hình mẫu mặc định cho môi trường public. Docker Compose là lựa chọn khởi chạy nhanh đã cấu hình sẵn các dịch vụ.

## Kiểm thử

Frontend:

```powershell
npm --prefix frontend/admin test
npm --prefix frontend/admin run build
```

Python Agent (cài dependencies development trước):

```powershell
python -m venv .venv
.venv\Scripts\python.exe -m pip install -r agent/requirements-dev.txt
.venv\Scripts\python.exe -m pytest agent/tests -q
```

Backend/integration tests cần .NET 10 SDK và PostgreSQL test riêng, không dùng database có dữ liệu cần giữ:

```powershell
dotnet test backend/ITManagement.slnx
```

Xem [agent/README.md](agent/README.md) để biết cách cấu hình PostgreSQL test, chạy live smoke và UI smoke.

## Trạng thái chức năng

- Đã có đăng nhập qua API và quản lý Agent: enrollment token dùng một lần, heartbeat, hiển thị connectivity/policy riêng biệt và vô hiệu hóa Agent.
- Agent giữ credential trong state riêng; `real` cần cờ đồng ý inventory, còn `simulated` dùng fixture và được đánh dấu rõ.
- Các bảng dashboard và một số màn tài sản, tài khoản, ticket vẫn là preview với dữ liệu minh họa; chưa xem đó là nghiệp vụ đã hoàn thiện hoặc dữ liệu thật.
- Enrollment lại/thay credential sau khi mất state, quản lý vòng đời cấp phát tài sản, file/quota và workflow ticket đầy đủ vẫn là các phần cần hoàn thiện.

Chi tiết trạng thái và kế hoạch nằm trong [itam-codex-engineering/COMPLETION-ROADMAP.md](itam-codex-engineering/COMPLETION-ROADMAP.md). Hợp đồng Agent/API được mô tả trong [agent/README.md](agent/README.md).