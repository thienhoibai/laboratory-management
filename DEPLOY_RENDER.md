# Hướng dẫn deploy lên Render (free)

Kiến trúc sau khi chuyển đổi:
- **7 web service** (gateway, iam, patient, testorder, instrument, blog, notify) chạy Docker trên **Render free**
- **Database: PostgreSQL trên Neon.tech** (free vĩnh viễn, thay cho SQL Server)
- **Message queue: Private Redis Service trên Render** (Tự động khởi tạo và kết nối thông qua Blueprint render.yaml, thay cho CloudAMQP/RabbitMQ)

## Bước 1 — Tạo database trên Neon (miễn phí)

1. Vào https://neon.tech → đăng ký (đăng nhập bằng Google/GitHub đều được)
2. Tạo 1 project mới, chọn region gần bạn hoặc **US East (N. Virginia)** — để các service Render cũng đặt ở region `virginia` giúp tối ưu tốc độ truy vấn cơ sở dữ liệu.
3. Trong project, vào tab **Databases** → tạo đủ 6 database:
   - `LabIAM`
   - `PatientService4`
   - `TestOrderDB`
   - `BlogServiceDB`
   - `InstrumentDB`
   - `notify`
4. Vào **Dashboard → Connection Details**, lấy host + user + password.
   Connection string cho .NET (Npgsql) có dạng:
   ```
   Host=ep-xxxx-pooler.ap-southeast-1.aws.neon.tech;Database=LabIAM;Username=neondb_owner;Password=XXXX;SSL Mode=Require;Trust Server Certificate=true
   ```
   > Mỗi service dùng chung host/user/password, chỉ đổi tên ở phần `Database=...` cho tương ứng từng service.

## Bước 2 — Đẩy code lên GitHub

Repo hiện ở GitLab nội bộ FPT nên Render không kết nối được. Hãy tạo repo mới trên GitHub:

1. Vào https://github.com/new → đặt tên (vd `laboratory-management`) → **Private** hoặc Public đều được → Create
2. Thực hiện push code lên GitHub:
   ```bash
   git remote add github https://github.com/<username>/laboratory-management.git
   git push github thien
   ```

## Bước 3 — Deploy Blueprint trên Render

1. Vào https://dashboard.render.com → **New → Blueprint Route**
2. Kết nối tài khoản GitHub, chọn repo `laboratory-management` vừa push, branch tương ứng (ví dụ: `thien`)
3. Render sẽ tự động đọc file `render.yaml` và phát hiện ra 8 dịch vụ (bao gồm 1 Private Redis Service và 7 Web Docker Services).
4. Điền các biến môi trường được đánh dấu cần cấu hình thủ công (`sync: false`):
   - `ConnectionStrings__*`: Connection string Neon (đúng tên database cho từng service tương ứng).
   - `PATIENT_PII_KEYS` (trong service patient): 3 dòng key mã hóa AES-GCM như trong file docker-compose.yml.
   - `Email__Smtp__Password` (trong service notify): App password của Gmail (mật khẩu ứng dụng) để gửi email.
5. Bấm **Apply** và chờ Render build (quá trình build Docker cho các service .NET lần đầu tiên mất khoảng 5-10 phút).

## Bước 4 — Kiểm tra sau khi deploy

⚠️ **Quan trọng:** Nếu tên service bị trùng trên Render, URL sẽ bị thêm hậu tố ngẫu nhiên (ví dụ: `hemalink-iam-x7k2.onrender.com`).
Khi đó, bạn cần vào service **hemalink-gateway** để cập nhật lại các biến môi trường URL đích cho khớp:
`IAM_URL`, `PATIENT_URL`, `TESTORDER_URL`, `BLOG_URL`, `INSTRUMENT_URL`.
Và cập nhật các biến liên lạc chéo:
- `Grpc__IamUrl` ở service `patient`
- `PatientApiUrl` + `VnPay__PaymentReturnUrl` ở service `testorder`
- `TestOrderBaseUrl` ở service `instrument`

Kiểm tra hoạt động:
- `https://hemalink-gateway.onrender.com/` → Trả về "Gateway up"
- `https://hemalink-gateway.onrender.com/iam/healthz` → Trả về OK
- Kiểm tra Swagger của các service: `https://hemalink-iam.onrender.com/swagger/index.html`, v.v.

## Những điều cần biết về gói free của Render

- **Các service sẽ tự động ngủ sau 15 phút không hoạt động** — request đầu tiên đánh thức lại service sẽ mất khoảng 1 phút.
- **Dữ liệu hình ảnh tải lên (Blog/Instrument)** được lưu trên ổ đĩa tạm của container Render nên **sẽ bị mất khi restart/redeploy** (Gói free không hỗ trợ Persistent Disk).
- **CORS** hiện tại được cấu hình cho phép `http://hema-link.io.vn` và `localhost:5174`. Nếu bạn deploy frontend lên domain mới (ví dụ Vercel/Netlify), hãy nhớ thêm origin đó vào file `Program.cs` của các service.
