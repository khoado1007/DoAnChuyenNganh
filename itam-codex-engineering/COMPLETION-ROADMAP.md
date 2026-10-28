# Kế hoạch hoàn thiện ITAM/ITSM theo từng phiên

Ngày lập: 29/10/2026. Trạng thái: CHỈ LẬP KẾ HOẠCH — chưa cho phép thực thi các phần bên dưới trong phiên hiện tại.

## 1. Mục tiêu và phạm vi hoàn thiện

Hoàn thiện đồ án theo product contract trong skills/architecture-guardian và các E2E của skills/quality-gates: một React web có /admin, /employee, /technician; ASP.NET Core Web API; PostgreSQL; kho file private; Docker Compose; Python Agent real/simulated. Technician chỉ có luồng ticket giới hạn. Không mở rộng sang OS login, remote shell, MongoDB, VM, domain controller, osquery hoặc sản phẩm ITSM quy mô lớn.

Định nghĩa hoàn thiện: nghiệp vụ chính chạy bằng dữ liệu thật, kiểm tra quyền phía server, có kiểm thử bảo mật/đồng thời, có thể dựng lại môi trường và trình diễn theo tài liệu. Không còn dữ liệu mẫu giả làm dữ liệu thật. Dữ liệu nhân viên thuộc tài khoản, không thuộc Agent. Connectivity, policy Agent và vòng đời tài sản là ba trạng thái riêng.

Đây là roadmap chia phiên, không phải cam kết mọi phần đều cần triển khai từ đầu. Trước mỗi phần phải kiểm tra code hiện có và chỉ bổ sung phần thiếu. Chi tiết kỹ thuật còn chưa rõ được chốt trong phạm vi phần đang làm, tránh dành credit thiết kế quá xa.

## 2. Điểm xuất phát đã xác minh

- Có phân tầng Domain/Application/Infrastructure/API và entity cho users/assets/allocations/storage/tickets; có entity không đồng nghĩa đã có API/luồng nghiệp vụ hoàn chỉnh.
- API hiện có AuthController và AgentsController (bao gồm asset create/list tối thiểu).
- Đã hoàn thành enrollment token một lần, hash secret, heartbeat, replay protection, disable và Python real/simulated; màn Agent dùng API thật.
- Còn thiếu hoặc chưa hoàn thiện: quản lý người dùng, cấp phát/thu hồi tài sản, file/quota, ticket, dashboard thật, routing Employee/Technician, vận hành và nghiệm thu toàn hệ thống.
- Agent còn thiếu luồng phục hồi/thay credential; hiện chỉ giữ snapshot inventory gần nhất.
- Các màn ngoài Agent còn là preview; không dùng những số liệu đó làm bằng chứng nghiệp vụ.
- Bằng chứng phiên trước: 20 backend + 13 Python + 3 frontend tests pass; frontend build và Python/browser smoke pass. Đây là kết quả lịch sử, không thay thế chạy lại sau thay đổi.
- Có nhiều thay đổi chưa commit từ trước. Không reset, clean, checkout đè, xóa file chưa xác định chủ sở hữu hoặc commit gom mọi thứ.
- README gốc đã bị xóa trước phiên Agent; không tự khôi phục bản cũ. Tài liệu Agent: ../agent/README.md. Kế hoạch/bằng chứng Agent: AGENT-IMPLEMENTATION.md.

## 3. Chia việc theo limit/credit

Ảnh chụp quota lúc lập kế hoạch: cửa sổ 5 giờ đã dùng 84% (còn khoảng 16%); cửa sổ tuần đã dùng 13% (còn khoảng 87%); credit bổ sung = 0. Credit bổ sung bằng 0 không có nghĩa quota gói đã hết. Các số này sẽ thay đổi, phải đọc lại khi bắt đầu phiên sau.

Không có dữ liệu đáng tin để quy đổi một chức năng thành số credit/USD chính xác. Vì vậy dùng giới hạn phạm vi và mức dung lượng tương đối; không hứa một phần tiêu tốn đúng một tỷ lệ quota.

| Quota còn lại ở đầu phiên | Cách chọn việc đề xuất |
| --- | --- |
| Dưới 30% ở cửa sổ đang hạn chế | Chỉ đọc checkpoint, xử lý lỗi rất nhỏ đã rõ nguyên nhân hoặc lập kế hoạch; không bắt đầu phần nghiệp vụ mới |
| 30–59% | Một phần nhỏ S hoặc nửa phần M có thể kiểm thử độc lập |
| Từ 60%, quota tuần cũng còn thoải mái | Một phần M; với L chỉ làm một tiểu phần A/B |
| Chạm khoảng 20–25% còn lại trong khi làm | Không mở rộng phạm vi; ưu tiên kiểm thử thay đổi hiện tại, ghi checkpoint và dừng |

Đây là ngưỡng vận hành đề xuất, không phải dự báo tiêu hao. Không đợi đến 0% mới bàn giao. Không kiểm tra quota liên tục: kiểm tra đầu phiên và trước khi nhận thêm việc là đủ.

S = phạm vi nhỏ, ít tầng. M = một lát cắt nghiệp vụ với test. L = có đồng thời/bảo mật/tích hợp nhiều tầng, phải chia A/B nếu limit không đủ. Mỗi lần người dùng gọi chỉ xử lý một mã phần; không tự chạy phần kế tiếp dù còn quota. Không lập automation hoặc tự tiếp tục sau reset.

## 4. Các phần thực hiện

| Mã | Cỡ | Phụ thuộc | Công việc | Tiêu chí xong và kiểm thử |
| --- | --- | --- | --- | --- |
| P00 — Chốt trạng thái và nền test | S | Không | Đọc git status/diff, phân loại thay đổi sẵn có; rà tài liệu; ghi cách dựng DB test và chạy test tập trung. Chỉ dọn artifact chứng minh được là dư; ưu tiên ignore thay xóa. Chuẩn hóa checkpoint trong file này. | Chạy được baseline hoặc ghi rõ lỗi môi trường; không mất thay đổi người dùng; có danh sách khoảng trống thực tế. Không refactor diện rộng. |
| P01 — Tài khoản và quyền phía server | M | P00 | Admin tạo/list/update nhân viên và technician, khóa/mở theo mô hình hiện có; validation, uniqueness, DTO; không public admin registration. Kiểm tra ảnh hưởng khóa tài khoản tới token đang có và chọn cách thu hồi quyền server-side. | Admin được phép, role khác bị 403; account khóa không tiếp tục thao tác bảo vệ; không lộ password hash; persistence và duplicate tests. |
| P02 — Routing và màn tài khoản | M | P01 | Route thật /admin, /employee, /technician; guard UX; màn Admin quản lý tài khoản từ API; Employee/Technician có layout và hồ sơ tối thiểu. Giữ phong cách hiện tại, không thiết kế lại toàn bộ UI. | Login/refresh/logout/direct link đúng role; truy cập chéo bị chặn; loading/empty/error; test role routes và build. |
| P03 — Quản lý tài sản thật | M | P00 | Hoàn thiện asset API từ create/list hiện có: detail, sửa thông tin, tìm kiếm/phân trang, validation, lifecycle hợp lệ. Nối màn tài sản hiện đang preview; không tạo service CRUD trùng lặp. | Dữ liệu persist sau reload; unique asset code; role enforcement; retired không được enrollment; migration nếu cần và UI/API tests. |
| P04A — Cấp phát, trả và thu hồi phía server | M | P01, P03 | Application service cho assign/return/revoke; lịch sử bất biến; bảo đảm một tài sản không có hai allocation active khi request đồng thời; cập nhật lifecycle atomically; API nhân viên xem tài sản của mình. | Hai request assign đua nhau chỉ một thắng; return/revoke hợp lệ; audit còn nguyên; nhân viên khác không truy cập được dữ liệu riêng. |
| P04B — UI cấp phát và tài sản nhân viên | S/M | P02, P04A | Admin thao tác cấp/trả/thu hồi và xem lịch sử; Employee xem tài sản đang được cấp. | E2E assign -> employee thấy -> return/revoke -> quyền/danh sách đổi; refresh giữ đúng dữ liệu. |
| P05 — Phục hồi vòng đời Agent | M | P03 | Thiết kế và triển khai Admin cấp lại credential/đăng ký thay thế khi mất state hoặc mất response enrollment; khóa identity cũ phía server; giữ audit và liên kết tài sản. Chọn chính sách rõ cho instance cũ, không âm thầm ghi đè. UI thao tác và hướng dẫn recovery. | Credential cũ bị từ chối sau thay thế; token mới dùng một lần; race/replay không tạo hai Agent active; real/simulated vẫn đúng nhãn; Python recovery test. Không thêm lịch sử mọi heartbeat nếu không có yêu cầu. |
| P06 — Kho file private và phân quyền | M | P01 | Rà LocalFileStorageService; hoàn thiện list/download/delete và metadata theo owner. Dùng opaque storage IDs; chống path traversal; file không public; DTO không lộ đường dẫn đĩa. Test fixtures tạo file trong kho test. Delete cập nhật accounting nhất quán. | A đọc/xóa file A; B và Agent credential không được phép; tên file độc hại không thoát root; không mất file người khác; xử lý file/metadata thiếu nhất quán. Chưa mở upload nếu P07 chưa an toàn. |
| P07A — Upload và quota nguyên tử | M/L | P06 | Thiết kế reservation/transaction cho quota dưới concurrency; stream upload; giới hạn kích thước thực tế; cleanup khi lỗi/hủy; xử lý commit DB và filesystem không nguyên tử, không chỉ tin Content-Length. | Concurrent uploads không vượt quota; fail/cancel không rò quota; oversize bị chặn; kiểm thử DB thật + storage tạm. |
| P07B — Hoàn thiện lỗi và quota Admin | S/M | P07A | API xem/chỉnh quota hợp lệ, kiểm tra crash/orphan recovery trong phạm vi hợp lý; củng cố delete đồng thời và retry. | Không quota âm/double release; hạ quota có chính sách rõ khi vượt usage; có tests failure injection và hướng dẫn recovery. |
| P08 — File portal nhân viên | M | P02, P07B | UI upload/list/download/delete và dung lượng; progress/loading/error/quota exceeded. Gắn vào tài khoản, không gắn lựa chọn máy giả lập. | E2E A upload/download/delete; B không truy cập link; quota UI khớp API; logout không rò dữ liệu; build và responsive checks. |
| P09 — Ticket API và state machine | M | P01, P04A | Employee tạo/xem ticket của mình; Admin phân công; Technician chỉ xử lý ticket được giao; comment/history bất biến; transition hợp lệ. Tận dụng sequence ticket đã có. | Wrong owner/role bị chặn; transition trái phép bị từ chối; đồng thời không mất cập nhật; ticket number duy nhất; lịch sử và persistence. |
| P10 — Ticket UI ba vai trò | M | P02, P09 | Employee tạo/theo dõi; Admin phân công; Technician cập nhật trạng thái và phản hồi; filter/detail/loading/error. | E2E employee tạo -> admin giao -> technician xử lý -> employee thấy kết quả; kiểm tra truy cập chéo và reload. |
| P11 — Dashboard thật và audit | M | P04B, P05, P08, P10 | Thay số liệu preview bằng aggregate API; số tài sản theo lifecycle, Agent theo connectivity/policy, ticket theo trạng thái, quota phù hợp role. Màn xem audit cho thao tác quản trị cần thiết. Không suy ra hai máy vật lý từ hai Agent. | Aggregate khớp DB fixtures, role scope đúng; không còn bảng/số hardcode bị trình bày như dữ liệu thật; empty/error states và test queries. |
| P12A — Cấu hình và bảo vệ triển khai | M | P11 | Rà Compose/nginx/config; secret từ môi trường; bootstrap Admin có kiểm soát; production errors; TLS deployment contract; giới hạn upload/request/rate phù hợp reverse proxy. Không tự đổi sang tin mọi forwarded headers. | Production startup an toàn; không seed password mặc định ngoài cấu hình kiểm soát; protected endpoints, proxy auth/status, private volume không bị public; kiểm tra Compose config/build. |
| P12B — CI, backup và restore | M | P12A | CI backend/Python/frontend/integration với PostgreSQL; image build; script/hướng dẫn backup metadata + private files và restore theo thứ tự; smoke check/healthcheck. | CI hoặc tương đương local pass; restore vào môi trường sạch kiểm tra metadata/file khớp; không backup secret vào repository. Không tuyên bố backup thành công nếu chưa thử restore. |
| P13A — Nghiệm thu tích hợp và sửa lỗi | M/L | P12B | Chạy 8 E2E quality-gates trên môi trường sạch; review quyền, race và migration; chỉ sửa bug chặn nghiệm thu, không thêm tính năng. | Có bảng kết quả từng E2E, toàn suite pass, migration từ DB rỗng và nâng cấp không mất dữ liệu; không còn lỗi Critical/Important chưa xử lý. Nếu lỗi lớn, tách thành phần fix riêng có checkpoint. |
| P13B — Đóng gói đồ án và bàn giao | S/M | P13A | Tài liệu cài đặt/vận hành, sơ đồ kiến trúc/ERD, API, hướng dẫn Agent, demo script, tài khoản test an toàn, giới hạn hiện tại; rà README gốc để viết tài liệu mới theo trạng thái thực tế. Chụp minh chứng UI khi cần. | Người khác dựng lại và chạy demo theo tài liệu; phân biệt rõ implemented/future scope; ghi test evidence và known issues; không còn secret/test artifact ngoài ý muốn. |

Ước lượng tổ chức: khoảng 18 phiên phạm vi vừa/nhỏ nếu mỗi phiên làm một dòng; P00 có thể rất ngắn, các dòng L có thể cần thêm phiên. Đây không phải dự báo số credit hoặc ngày hoàn thành. Không gộp nhiều phần để cố đạt con số phiên trên nếu chất lượng kiểm thử giảm.

## 5. Quy trình tiết kiệm credit cho mỗi phiên

1. Đọc file này, checkpoint mới nhất và skill đúng phần: architecture-guardian + quality-gates; thêm aspnet-api/python-agent/react-ui/delivery-devops khi thực sự liên quan. Không đọc lại toàn bộ repository.
2. Kiểm tra quota, git status và chỉ các file liên quan; xác nhận phần trước đã thật sự xong. Không dùng kết quả cũ để tuyên bố build hiện tại pass.
3. Lập checklist ngắn cho đúng một phần. Với phần M/L, chốt contract và chia A/B trước khi viết code; không tạo tài liệu lặp nếu checkpoint đã đủ.
4. Viết test cho rủi ro chính trước; implement theo từng bước. Khi lỗi xuất hiện, đọc diagnostics và sửa nguyên nhân; không chạy đi chạy lại toàn bộ suite trong lúc chưa đổi gì.
5. Trong vòng lặp dùng targeted tests. Trước khi bàn giao chạy bộ kiểm thử liên quan và build bị ảnh hưởng; ở các mốc tích hợp chạy toàn suite. Không bỏ test bảo mật/concurrency chỉ để tiết kiệm credit.
6. Mặc định làm trong một chat/agent. Chỉ review độc lập ở phần rủi ro cao hoặc mốc nghiệm thu khi quy trình áp dụng yêu cầu; không chia nhiều agent cho thao tác nhỏ.
7. Dự trữ quota cho kiểm thử và checkpoint. Nếu không đủ để hoàn tất an toàn, ghi rõ việc đang dang dở, lỗi còn lại, lệnh cần chạy tiếp; không đánh dấu hoàn thành.
8. Kết thúc đúng phần được giao. Không tự commit/push/deploy, không tự bắt đầu phần kế tiếp. Chỉ dọn file/process/container do phiên đó tạo và không còn cần; giữ dữ liệu nghiệp vụ.

Lưu ý môi trường đã gặp: sandbox exec từng lỗi khởi tạo; cần dùng cơ chế quyền phù hợp nếu lặp lại. Trên Windows phải dừng API test trước build nếu đang khóa DLL. Có venv ../agent/.venv; `python` trên PATH từng trỏ Store alias. PostgreSQL test cổng 55432 chỉ được tạo khi chạy test; container test và API/Vite tạm đã dọn sau phiên Agent. Không giả định môi trường test đang chạy.

## 6. Điểm dừng và mẫu bàn giao

Cập nhật ngay trong mục này, không tạo hàng loạt file status:

- Phần đang làm: ...
- Trạng thái: CHƯA BẮT ĐẦU / ĐANG LÀM / BLOCKED / HOÀN THÀNH.
- File đã sửa: ...
- Quyết định kỹ thuật và lý do: ...
- Test đã chạy, lệnh và kết quả thật: ...
- Test chưa chạy/lỗi còn lại: ...
- Dữ liệu/process/container tạm: ...
- Việc tiếp theo chính xác: ...
- Có thể bắt đầu phần kế tiếp chưa: Có/Không, vì ...

Checkpoint hiện tại: Agent enrollment/heartbeat đã hoàn thành theo AGENT-IMPLEMENTATION.md. Roadmap P00–P13B chưa bắt đầu. Phiên tiếp theo nên làm P00 trước; nếu quota thấp chỉ hoàn thành kiểm kê và checkpoint, không chuyển P01.

## 7. Prompt dùng cho lần sau

> Đọc D:/DoAnChuyenNganh/itam-codex-engineering/COMPLETION-ROADMAP.md và checkpoint. Chỉ thực hiện phần P00 trong phiên này, chạy lệnh từ folder itam-codex-engineering. Dùng skill liên quan, bảo toàn thay đổi chưa commit. Kiểm tra limit đầu phiên; dành ngân sách cho test và bàn giao. Hoàn thành hoặc ghi checkpoint phần đang dở rồi dừng; không tự chuyển phần tiếp theo, không commit/push/deploy.

Các lần sau thay P00 bằng mã phần kế tiếp chưa hoàn thành. Nếu muốn chỉ lập kế hoạch chi tiết, ghi rõ “chỉ lập kế hoạch, chưa sửa code”.
