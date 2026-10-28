# Python Agent — đăng ký và heartbeat

Một codebase cho `real` và `simulated`, mỗi instance dùng một file state riêng. `real` chỉ gửi hostname, OS, CPU, tổng RAM và phiên bản Agent khi truyền `--consent-inventory`. `simulated` đọc fixture và server giữ nhãn chế độ từ token, không cho heartbeat tự đổi nhãn. Không tạo tài khoản OS, không chạy lệnh từ xa, không đọc tài liệu cá nhân.

## Chạy từ thư mục itam-codex-engineering

Cần Python 3.11+ và API/PostgreSQL đang chạy. Lệnh ví dụ PowerShell:

```powershell
python -m venv ../agent/.venv
../agent/.venv/Scripts/python.exe -m pip install -r ../agent/requirements-dev.txt
```

Đăng nhập Admin trên web, mở **Agent**, tạo/chọn tài sản và cấp token đúng chế độ. Chạy lệnh sau, dán token vào lời nhắc ẩn; không truyền token qua command line:

```powershell
../agent/.venv/Scripts/python.exe ../agent/itam_agent.py enroll --server http://127.0.0.1:8080 --mode real --state ../agent/state/real.json --consent-inventory
../agent/.venv/Scripts/python.exe ../agent/itam_agent.py run --server http://127.0.0.1:8080 --mode real --state ../agent/state/real.json --consent-inventory
```

Đối với mô phỏng, cấp token thứ hai cho tài sản thứ hai rồi dùng `--mode simulated --state ../agent/state/simulated.json`. Hai instance trên một PC không chứng minh hai thiết bị vật lý. Có thể dùng `once` thay `run` để gửi một heartbeat. Ctrl+C dừng nhẹ nhàng.

HTTP chỉ chấp nhận localhost/loopback; môi trường khác bắt buộc HTTPS có chứng chỉ hợp lệ. Không tự theo redirect. Request timeout 15 giây; heartbeat mặc định 30 giây, tùy chọn 10–60 giây; lỗi mạng/408/429/5xx dùng backoff+jitter tối đa 300 giây. 401/403/410 dừng và lưu trạng thái bị thu hồi, không tự đăng ký lại. Các lỗi giao thức 4xx khác cũng dừng.

State chứa credential riêng của Agent; không chia sẻ/commit. File được ghi atomic, POSIX 0600 hoặc Windows ACL chỉ user hiện tại. Không dùng chung file state giữa instance, server hoặc chế độ. File `.lock` có thể còn trên đĩa nhưng khóa hệ điều hành tự nhả khi process chết. Không xóa state để sửa lỗi mạng: sequence và heartbeat chờ gửi cần giữ qua restart.

## Hợp đồng API v1

Mọi response dùng envelope `success/message/data/errors`. OpenAPI tại `/swagger` khi API chạy Development.

| Endpoint | Quyền / nội dung |
| --- | --- |
| GET/POST `/api/assets` | Admin; tạo tài sản Desktop tối thiểu bằng `assetCode`, `name` |
| POST `/api/agents/enrollment-tokens` | Admin; `{assetId, mode}`; trả token một lần, hết hạn 15 phút; cấp mới hủy token cũ chưa dùng |
| POST `/api/agents/enroll` | `{token, instanceId: UUID, mode, protocolVersion: 1}`; trả `agentId`, `credential`, interval |
| POST `/api/agents/heartbeat` | Header `Authorization: Agent <credential>`; sequence tăng, protocolVersion=1, hostname, os, cpu, ramBytes, agentVersion |
| GET `/api/agents` | Admin; mode, policy, connectivity, assetStatus, lastSeenAt và inventory |
| POST `/api/agents/{id}/disable` | Admin; từ chối heartbeat sau khi disable hoàn tất, giữ snapshot trước đó |

Token/credential trong database chỉ là SHA-256 của secret ngẫu nhiên 256 bit. Agent credential không đăng nhập được như người dùng. Mỗi tài sản chỉ có một Agent; mỗi instance ID là duy nhất. Token được tiêu thụ trong transaction cùng lúc tạo Agent. Enrollment không retry tự động: nếu server đã hoàn tất nhưng response/ghi state thất bại, cần quản trị xử lý cấp lại; UI thay credential/tái đăng ký chưa nằm trong đợt này.

Server dùng UTC; sequence trùng/cũ không cập nhật last-seen hoặc inventory. Online khi heartbeat mới nhất cách hiện tại <90 giây. Offline không tự đổi tài sản sang retired; disabled là chính sách độc lập, vì vậy Agent vừa bị disable có thể còn hiển thị Online đến hết ngưỡng. Snapshot inventory gần nhất được giữ, chưa có bảng lịch sử từng heartbeat. Giới hạn theo IP: 20 enrollment/phút, 120 heartbeat/phút; cụm nhiều Agent sau cùng NAT cần điều chỉnh trước triển khai lớn.

## Kiểm thử

```powershell
# PostgreSQL tách riêng; không dùng database nghiệp vụ
 docker run -d --name itam-agent-tests -e POSTGRES_PASSWORD=postgres -p 127.0.0.1:55432:5432 postgres:16-alpine
$env:TEST_POSTGRES='Host=127.0.0.1;Port=55432;Username=postgres;Password=postgres'
dotnet test ../backend/ITManagement.slnx
../agent/.venv/Scripts/python.exe -m pytest ../agent/tests -q
npm --prefix ../frontend/admin test
npm --prefix ../frontend/admin run build
```

Live smoke: khởi chạy API Development trên `127.0.0.1:5189`, trỏ `ConnectionStrings__DefaultConnection` tới database `itam_agent_e2e` trên PostgreSQL test. Đặt `ITAM_TEST_PASSWORD` bằng mật khẩu admin test rồi chạy:

```powershell
../agent/.venv/Scripts/python.exe ../agent/e2e_smoke.py --server http://127.0.0.1:5189 --consent-inventory
```

UI smoke: Vite cổng 5190 với `API_PROXY_TARGET=http://127.0.0.1:5189`, cần Playwright và Edge. Đặt `PLAYWRIGHT_MODULE` tới module Playwright nếu không có trong node_modules; chạy `node ../frontend/admin/tests/agents/smoke.cjs`. Có thể đặt `ITAM_UI_URL` và `ITAM_SCREENSHOT`. Smoke test tạo bản ghi test thật, chỉ chạy trên database dùng thử. Các bài kiểm thử allocation/files/quota/tickets không thuộc thay đổi này.
