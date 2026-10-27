# Đề cương đồ án chuyên ngành — Bản chốt

## 1. Tên đề tài đề xuất
**Phân tích, thiết kế và xây dựng hệ thống quản lý tài sản CNTT, tài khoản người dùng và yêu cầu hỗ trợ nội bộ tích hợp lưu trữ cá nhân tập trung.**

## 2. Ý tưởng
Xây dựng hệ thống mô phỏng hoạt động IT trong một công ty có nhiều máy tính được cấp phát cho nhân viên. Hệ thống quản lý thiết bị, tài khoản, lịch sử cấp phát/thu hồi, yêu cầu hỗ trợ và vùng lưu trữ cá nhân có hạn mức. Khi nhân viên đăng nhập trên một máy mô phỏng khác, họ vẫn truy cập được dữ liệu cá nhân của mình, không phụ thuộc vào máy đang chọn.

## 3. Cách truy cập
Triển khai **một ứng dụng web, một domain, một backend và một cơ sở dữ liệu**, phân chia khu vực theo đường dẫn:

- `/admin`: khu vực quản trị, có màn hình đăng nhập riêng. Admin gốc được khởi tạo khi cài đặt; không có đăng ký Admin công khai.
- `/employee`: cổng nhân viên; hiển thị danh sách máy mô phỏng đang hoạt động, cho phép chọn máy rồi đăng nhập.
- `/technician`: khu vực kỹ thuật viên để xem và xử lý các ticket được phân công; có thể mở rộng nghiệp vụ sau này.

Các đường dẫn chỉ phân chia giao diện và luồng sử dụng, không thay thế cơ chế bảo mật. Backend phải xác thực tài khoản và kiểm tra quyền trên từng API.

### Luồng nghiệp vụ tiêu biểu
1. Admin tạo tài khoản nhân viên và thiết bị mới.
2. Thiết bị đang hoạt động tự xuất hiện trong danh sách lựa chọn của cổng nhân viên; khi thiết bị bị vô hiệu hóa/thu hồi, nó không còn được chọn cho phiên mới.
3. Admin cấp thiết bị cho nhân viên và đặt hạn mức lưu trữ, ví dụ 10 GB.
4. Nhân viên chọn Máy 1 hoặc Máy 2, đăng nhập và truy cập vùng tệp cá nhân.
5. Nhân viên tạo ticket; kỹ thuật viên xử lý; Admin thực hiện các thao tác quản trị cần quyền cao.

**Giới hạn phạm vi:** Máy 1/Máy 2 là máy đầu cuối mô phỏng trong ứng dụng web, không phải máy ảo hay phiên đăng nhập Windows/Linux thật. Đồ án không tạo tài khoản hệ điều hành và không kiểm soát toàn bộ ổ đĩa vật lý.

## 4. Chức năng chính
1. **ITAM:** quản lý thiết bị, mã tài sản, trạng thái, cấp phát/thu hồi và lịch sử.
2. **Tài khoản và phân quyền:** tạo/khóa tài khoản, các vai trò Admin, kỹ thuật viên, nhân viên; kiểm tra quyền ở backend.
3. **Lưu trữ cá nhân tập trung:** mỗi tài khoản có vùng tệp riêng; xem/tải lên/tải xuống tệp; xem dung lượng đã dùng/còn lại; chặn tải lên khi vượt quota.
4. **ITSM:** tạo, phân công, cập nhật, giải quyết và đóng ticket; lưu lịch sử xử lý.
5. **Báo cáo:** tình trạng thiết bị, lịch sử cấp phát, sử dụng dung lượng và tình trạng ticket.

## 5. Công nghệ và cách lưu trữ
- **Frontend:** React + TypeScript + Vite.
- **Backend:** C# / ASP.NET Core.
- **Cơ sở dữ liệu nghiệp vụ:** PostgreSQL.
- **Lưu nội dung tệp thực tế:** Docker volume được backend quản lý; không lưu trực tiếp toàn bộ nội dung tệp vào PostgreSQL.
- **Metadata của tệp:** lưu trong PostgreSQL, gồm chủ sở hữu, tên hiển thị, kích thước, loại tệp, thời gian tải lên và định danh/vị trí lưu tệp.
- **Chạy và kiểm thử:** Docker Compose.
- **MongoDB:** chưa sử dụng trong phiên bản đầu vì PostgreSQL và file storage đã đáp ứng nhu cầu, giúp giảm độ phức tạp triển khai và sao lưu.

Backend phải kiểm tra quyền truy cập và quota trước khi ghi tệp; nội dung tệp không được đặt trong thư mục public để người dùng có thể truy cập bỏ qua kiểm tra phân quyền. Cần có phương án sao lưu riêng cho cả PostgreSQL và vùng lưu trữ tệp.

## 6. Tình huống kiểm thử tiêu biểu
- Tạo thiết bị mới thì xuất hiện trong danh sách máy; vô hiệu hóa thiết bị thì không thể bắt đầu phiên mới với thiết bị đó.
- Nhân viên đổi giữa các máy mô phỏng nhưng vẫn thấy tệp thuộc tài khoản của mình.
- Nhân viên không thể đọc tệp của tài khoản khác hoặc tự sửa quota.
- Tải tệp vượt quota bị từ chối; dung lượng sử dụng được cập nhật chính xác.
- Khóa tài khoản/thu hồi thiết bị khiến thao tác mới không được chấp nhận; lịch sử vẫn được lưu.
- Nhân viên tạo ticket, kỹ thuật viên cập nhật/giải quyết, nhân viên theo dõi kết quả.

## 7. Nội dung xin ý kiến giảng viên
- Tên đề tài và phạm vi kết hợp ITAM, quản lý tài khoản, lưu trữ cá nhân và ITSM có phù hợp không?
- Mô phỏng nhiều máy đầu cuối ở tầng ứng dụng web có đáp ứng mục tiêu đồ án không?
- Nên ưu tiên chức năng nào để bảo đảm khối lượng vừa sức và có tiêu chí đánh giá rõ ràng?
