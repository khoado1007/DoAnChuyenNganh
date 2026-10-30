# IT Management

Ứng dụng quản lý tài sản và hỗ trợ CNTT, gồm React + TypeScript, ASP.NET Core Web API, PostgreSQL và Python Agent. Agent, tài khoản và danh mục tài sản đã kết nối API thật. Dashboard và ticket vẫn là bản xem trước; cấp phát, file/quota và workflow ticket đang nằm trong roadmap.

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

- Tên đăng nhập: `admin` (email: `admin@itmanagement.local`)
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
- Admin quản lý Employee/Technician tại `/admin/users`; khóa/mở hoặc đổi vai trò thu hồi JWT cũ. Admin bootstrap được bảo vệ khỏi sửa/khóa qua màn này. Employee/Technician có trang hồ sơ riêng tại `/employee` và `/technician`.
- Tài sản tại `/admin/assets` có tạo, chi tiết, tìm kiếm/phân trang và cập nhật vòng đời. Không đặt Assigned trực tiếp; Retired là trạng thái cuối. Dashboard và ticket vẫn là preview.
- Đã có recovery Agent và API file riêng tư (list/download/delete). Upload/quota đầy đủ, file portal và workflow ticket vẫn là các phần cần hoàn thiện.

Chi tiết trạng thái và kế hoạch nằm trong [itam-codex-engineering/COMPLETION-ROADMAP.md](itam-codex-engineering/COMPLETION-ROADMAP.md). Hợp đồng Agent/API được mô tả trong [agent/README.md](agent/README.md).
## Tiếp tục roadmap và kiểm thử tập trung

Chạy từ thư mục `itam-codex-engineering`:

```powershell
./scripts/Test-Project.ps1             # PostgreSQL test phải đang chạy ở cổng 55432
./scripts/Test-Project.ps1 -Scope backend
./scripts/Test-Project.ps1 -Scope frontend
./scripts/Test-Project.ps1 -Scope agent
```

Script chỉ dùng database test do ApiFactory tạo; không trỏ `TestPostgres` vào hệ thống production. Cách tạo PostgreSQL test và chạy smoke nằm trong `agent/README.md`. Các browser smoke bổ sung: `frontend/admin/tests/users/smoke.cjs` và `frontend/admin/tests/assets/smoke.cjs`, cùng biến môi trường với smoke Agent (`PLAYWRIGHT_MODULE`, `ITAM_UI_URL`, `ITAM_TEST_PASSWORD`).

Migration `UserSecurityVersion` thêm cột để thu hồi phiên theo trạng thái/vai trò. Sau nâng cấp, JWT cũ không có claim phiên bản sẽ yêu cầu đăng nhập lại. API tài khoản: GET/POST `/api/users`, PUT `/api/users/{id}`; API tài sản: GET `/api/assets` vẫn giữ lookup tương thích Agent, GET `/api/assets/search` trả phân trang, GET/PUT `/api/assets/{id}`, POST `/api/assets`. Toàn bộ endpoint quản lý này dành cho Admin.

## Cấp phát và tài sản nhân viên

Admin mở **Tài sản → Cấp phát / lịch sử**, tìm Employee đang hoạt động để cấp tài sản sẵn sàng. Khi trả/thu hồi, ghi chú đóng được giữ riêng với ghi chú cấp; mỗi lần cấp lại tạo bản ghi mới. Nhân viên xem tài sản hiện tại tại `/employee`. Trả tài sản không vô hiệu hóa Agent và không thay đổi dữ liệu cá nhân của tài khoản.

API Admin: POST `/api/assets/{id}/assign` (`userId`, `notes`), POST `/api/assets/{id}/return` hoặc `/revoke` (`assignmentId`, `notes`), GET `/api/assets/{id}/assignments`. API Employee: GET `/api/me/assets`; owner lấy từ JWT, không nhận userId làm bằng chứng quyền. Request đóng phải trùng lần cấp đang hoạt động, chống thao tác từ trang cũ.

Migration `AssetAssignmentLifecycle` bổ sung audit đóng và unique index cho mỗi tài sản có tối đa một assignment active; không xóa dữ liệu cũ. Nếu dữ liệu nhập thủ công trước đây có nhiều assignment active trên một tài sản, phải rà soát dữ liệu trước khi migration có thể áp dụng. Browser smoke: `frontend/admin/tests/assignments/smoke.cjs`, dùng cùng biến môi trường như smoke Agent.

## Kho file riêng tư — P06

Đã có API dành cho Employee: GET `/api/files?page=1&pageSize=20`, GET `/api/files/{id}/download`, DELETE `/api/files/{id}`. Danh sách/tải/xóa được giới hạn theo chủ sở hữu từ JWT; userId/Agent credential không cấp quyền đọc file. DTO không trả đường dẫn đĩa; tải xuống dùng attachment, octet-stream, no-store và nosniff. Chưa có upload hoặc màn file portal: thuộc P07/P08.

Storage key nội bộ là thư mục UUID tài khoản + UUID file, không lấy từ tên hiển thị; chặn traversal, absolute path và reparse point. Private storage phải chỉ cho tiến trình ứng dụng/quản trị tin cậy ghi; không public volume qua nginx/static files. Key cũ dạng UUID + extension đơn giản vẫn được đọc; metadata ngoài định dạng hợp lệ bị từ chối, không tự sửa/xóa.

DELETE trả 204 khi đã dọn blob, kể cả retry hoặc blob đã thiếu. Metadata được đánh dấu xóa và quota logical được trừ trong cùng transaction, khóa trên quota của tài khoản. Sau commit mới xóa blob; nếu filesystem lỗi, API trả lỗi yêu cầu thử DELETE cùng ID lần nữa. Tombstone giữ key để cleanup retry và quota chỉ trừ một lần. Khi tiến trình crash giữa hai bước, blob có thể còn trên đĩa nhưng không còn truy cập được qua API; giữ tombstone để vận hành cleanup. Cơ chế reconciliation tự động và upload quota đầy đủ sẽ làm ở P07, chưa có trong P06. File thiếu blob trả 404 khi download; vẫn có thể DELETE metadata. Quota bị lệch/âm phải được kiểm tra trước, không tự clamp để giấu sai dữ liệu.

Kiểm thử FileTests tạo metadata và blob trực tiếp trong PostgreSQL/kho tạm riêng vì upload chưa mở. Không seed file vào kho nghiệp vụ. ApiFactory dọn cả database và thư mục test sau mỗi fixture.
