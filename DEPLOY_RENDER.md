# Hướng dẫn deploy lên Render (free)

Kiến trúc sau khi chuyển đổi:
- **7 web service** (gateway, iam, patient, testorder, instrument, blog, notify) chạy Docker trên **Render free**
- **Database: PostgreSQL trên Neon.tech** (free vĩnh viễn, thay cho SQL Server)
- **Message queue: CloudAMQP** (RabbitMQ free, thay cho container rabbitmq)

## Bước 1 — Tạo database trên Neon (miễn phí)

1. Vào https://neon.tech → đăng ký (đăng nhập bằng Google/GitHub đều được)
2. Tạo 1 project mới, region chọn **Singapore (ap-southeast-1)**
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
   > Mỗi service dùng cùng host/user/password, chỉ đổi `Database=...`

## Bước 2 — Tạo RabbitMQ trên CloudAMQP (miễn phí)

1. Vào https://www.cloudamqp.com → đăng ký
2. Create New Instance → chọn plan **Little Lemur (Free)**, region Singapore
3. Vào instance → copy **AMQP URL**, dạng:
   ```
   amqps://user:password@host.rmq.cloudamqp.com/vhost
   ```
   Đây là giá trị cho biến `RabbitMQ__Url` của 4 service: iam, patient, testorder, notify.

## Bước 3 — Đẩy code lên GitHub

Repo hiện ở GitLab nội bộ FPT nên Render không kết nối được. Tạo repo mới trên GitHub:

1. Vào https://github.com/new → đặt tên (vd `laboratory-management`) → **Private** hoặc Public đều được → Create
2. Push code:
   ```bash
   git remote add github https://github.com/<username>/laboratory-management.git
   git push github thien
   ```

## Bước 4 — Deploy Blueprint trên Render

1. Vào https://dashboard.render.com → **New → Blueprint**
2. Kết nối GitHub, chọn repo vừa push, branch `thien`
3. Render đọc file `render.yaml` và tạo 7 service
4. Điền các biến bị đánh dấu (sync: false):
   - `ConnectionStrings__*`: connection string Neon (đúng tên database từng service)
   - `RabbitMQ__Url`: AMQP URL của CloudAMQP
   - `PATIENT_PII_KEYS` (service patient): 3 dòng key như trong docker-compose.yml
   - `Email__Smtp__Password` (service notify): app password Gmail
5. Bấm **Apply** và chờ build (build .NET khá lâu, ~10 phút/service lần đầu)

## Bước 5 — Kiểm tra sau khi deploy

⚠️ **Quan trọng:** nếu tên service bị trùng trên Render, URL sẽ bị thêm hậu tố ngẫu nhiên
(vd `hemalink-iam-x7k2.onrender.com`). Khi đó phải vào service **gateway** sửa lại các biến
`IAM_URL`, `PATIENT_URL`, `TESTORDER_URL`, `BLOG_URL`, `INSTRUMENT_URL`, và các biến
`Grpc__IamUrl` (patient), `PatientApiUrl` + `VnPay__PaymentReturnUrl` (testorder),
`TestOrderBaseUrl` (instrument) cho khớp URL thật.

Kiểm tra:
- `https://hemalink-gateway.onrender.com/` → "Gateway up"
- `https://hemalink-gateway.onrender.com/iam/healthz` → OK
- Swagger từng service: `https://hemalink-iam.onrender.com/swagger`, v.v.

## Những điều cần biết về gói free

- **Service tự ngủ sau 15 phút không có request** — request đầu tiên sau đó mất ~1 phút để đánh thức. 
- **750 giờ free/tháng cho cả workspace** — đủ cho demo (service ngủ không tính giờ), không đủ chạy 7 service 24/7.
- **Data cũ trên Azure không tự chuyển sang** — database Neon bắt đầu trống, code tự tạo bảng khi service khởi động (`EnsureCreated`). Dữ liệu danh mục (test catalog, roles, tài khoản admin...) phải tạo lại qua API/Swagger hoặc chạy SQL.
- Ảnh upload của Blog/Instrument lưu trên đĩa container → **sẽ mất khi redeploy/restart** (Render free không có persistent disk).
- CORS hiện cho phép `http://hema-link.io.vn` và `localhost:5174`. Khi deploy frontend chỗ mới, thêm origin vào các Program.cs.

## Những gì đã đổi trong code (so với bản Azure)

- EF Core: `UseSqlServer` → `UseNpgsql` (6 service), package `Npgsql.EntityFrameworkCore.PostgreSQL`
- Default SQL: `getdate()/SYSUTCDATETIME()` → `now()`, `NEWSEQUENTIALID()` → `gen_random_uuid()`, kiểu cột `datetime` → `timestamp`
- Tất cả service tự tạo schema khi khởi động (`EnsureCreated`)
- MassTransit nhận `RabbitMQ__Url` (amqps URI của CloudAMQP), vẫn tương thích host/user/pass cũ khi chạy docker-compose
- Gateway (YARP) nhận URL backend qua env: `IAM_URL`, `PATIENT_URL`, ...
- Thêm endpoint `/healthz` cho testorder, blog, instrument, gateway
