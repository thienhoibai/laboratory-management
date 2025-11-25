# Tóm tắt: Chuyển đổi Status từ String sang Enum (Byte)

## 📋 Tổng quan thay đổi

### Các field đã thay đổi:
1. **Instrument.Status**: `string` → `InstrumentStatus` (byte enum)
2. **Instrument.ReagentStatus**: `string` → `ReagentStatus` (byte enum)
3. **InstrumentRun.Status**: `string` → `RunStatus` (byte enum)

### Lợi ích:
✅ Tiết kiệm storage (15 bytes → 1 byte per record)  
✅ Tăng performance khi query/index  
✅ Type-safe trong code  
✅ Dễ maintain và extend  
✅ Giảm lỗi nhập liệu sai format  

---

## 📦 Files đã tạo/sửa

### 1. **Domain Layer - Enums mới**
- `Instrument.Domain/Enums/InstrumentStatus.cs` ✨ NEW
- `Instrument.Domain/Enums/ReagentStatus.cs` ✨ NEW
- `Instrument.Domain/Enums/RunStatus.cs` ✨ NEW

### 2. **Domain Layer - Entities**
- `Instrument.Domain/Entities/Instrument.cs` ✏️ UPDATED
- `Instrument.Domain/Entities/InstrumentRun.cs` ✏️ UPDATED

### 3. **Infrastructure Layer**
- `Instrument.Infrastructure/InstrumentDbContext.cs` ✏️ UPDATED
- `Instrument.Infrastructure/Configs/InstrumentConfig.cs` ✏️ UPDATED
- `Instrument.Infrastructure/Migrations/Manual_Migration_Status_To_Enum.sql` ✨ NEW (QUAN TRỌNG!)

### 4. **Application Layer - DTOs**
- `Instrument.Application/Instruments/DTOs/Requests/CreateInstrumentRequest.cs` ✏️ UPDATED
- `Instrument.Application/Instruments/DTOs/Requests/UpdateInstrumentRequest.cs` ✏️ UPDATED
- `Instrument.Application/Instruments/DTOs/Responses/InstrumentResponse.cs` ✏️ UPDATED
- `Instrument.Application/Instruments/DTOs/Responses/InstrumentListItem.cs` ✏️ UPDATED
- `Instrument.Application/Instruments/DTOs/Responses/InstrumentStatusDto.cs` ✏️ UPDATED
- `Instrument.Application/Runs/DTOs/Responses/RunDetailDto.cs` ✏️ UPDATED
- `Instrument.Application/Runs/DTOs/Responses/StartRunResponse.cs` ✏️ UPDATED

### 5. **Application Layer - Services**
- `Instrument.Application/Services/InstrumentService.cs` ✏️ UPDATED
- `Instrument.Application/Services/RunService.cs` ✏️ UPDATED

### 6. **Presentation Layer - Controllers**
- `Instrument.Presentation/Controllers/InstrumentsController.cs` ✏️ UPDATED
- `Instrument.Presentation/Controllers/RunsController.cs` ✏️ UPDATED

---

## 🗺️ Mapping giá trị

### InstrumentStatus
| String (Cũ)  | Byte (Mới) | Enum Name   |
|-------------|-----------|-------------|
| "ONLINE"    | 0         | Online      |
| "OFFLINE"   | 1         | Offline     |
| "FAULT"     | 2         | Fault       |
| "MAINTENANCE" | 3       | Maintenance |

### ReagentStatus
| String (Cũ) | Byte (Mới) | Enum Name |
|------------|-----------|-----------|
| "OK"       | 0         | OK        |
| "LOW"      | 1         | Low       |
| "OUT"      | 2         | Out       |

### RunStatus
| String (Cũ)   | Byte (Mới) | Enum Name |
|--------------|-----------|-----------|
| "RUNNING"    | 0         | Running   |
| "COMPLETED"  | 1         | Completed |
| "FAILED"     | 2         | Failed    |

---

## 🚀 Cách Migration (THỦ CÔNG - SQL SCRIPT)

### Bước 1: Backup Database
```sql
-- Trong SQL Server Management Studio
BACKUP DATABASE InstrumentDB 
TO DISK = 'C:\Backups\InstrumentDB_Backup_BeforeEnum.bak'
WITH FORMAT, NAME = 'Before Enum Migration';
```

### Bước 2: Mở SQL Script
1. Mở SQL Server Management Studio (SSMS)
2. Connect tới database server
3. Open file: `Instrument.Infrastructure/Migrations/Manual_Migration_Status_To_Enum.sql`

### Bước 3: Chạy Script
1. **Đọc kỹ script** trước khi chạy
2. Script sẽ:
   - Tạo cột tạm (Status_New, ReagentStatus_New)
   - Convert dữ liệu từ string → byte
   - Xóa cột cũ
   - Rename cột mới
3. **Execute script** (F5 hoặc click Execute)
4. Kiểm tra kết quả trong các SELECT statement

### Bước 4: Verify Migration
```sql
-- Kiểm tra data type
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Instruments', 'InstrumentRuns')
  AND COLUMN_NAME IN ('Status', 'ReagentStatus');

-- Kiểm tra dữ liệu đã convert
SELECT InstrumentCode, Status, ReagentStatus FROM Instruments;
SELECT RunId, Status FROM InstrumentRuns;
```

### Bước 5: Deploy Code Mới
```bash
# Rebuild Docker container
docker-compose build instrument.api

# Restart service
docker-compose up -d instrument.api

# Kiểm tra logs
docker logs -f instrument.api
```

---

## 📝 API Changes

### Trước (String-based)
```http
POST /api/instruments
Content-Type: application/json

{
  "instrumentCode": "INSTR001",
  "name": "Máy xét nghiệm sinh hóa",
  "status": "ONLINE"
}
```

### Sau (Enum-based - số)
```http
POST /api/instruments
Content-Type: application/json

{
  "instrumentCode": "INSTR001",
  "name": "Máy xét nghiệm sinh hóa",
  "status": 0
}
```

### Response (Enum as number)
```json
{
  "instrumentId": 1,
  "instrumentCode": "INSTR001",
  "name": "Máy xét nghiệm sinh hóa",
  "status": 0,
  "reagentStatus": 0,
  "createdAt": "2025-01-01T10:00:00Z"
}
```

---

## ✅ Testing Checklist

Sau khi migration, test các endpoint sau:

### Instruments
- [ ] `GET /api/instruments` - Lấy danh sách máy
- [ ] `GET /api/instruments/{code}` - Lấy chi tiết máy
- [ ] `POST /api/instruments` - Tạo máy mới (status: 0-3)
- [ ] `PUT /api/instruments/{code}` - Cập nhật máy (status: 0-3, reagentStatus: 0-2)
- [ ] `DELETE /api/instruments/{code}` - Xóa máy
- [ ] `GET /api/instruments/{code}/status` - Lấy trạng thái máy

### Runs
- [ ] `POST /api/instrument/runs/start` - Start run (BookingId + InstrumentCode)
- [ ] `GET /api/instrument/runs/{runId}` - Lấy chi tiết run

---

## 🔄 Rollback (Nếu cần)

Uncomment phần `ROLLBACK SCRIPT` ở cuối file `Manual_Migration_Status_To_Enum.sql` và execute.

```sql
-- Script sẽ convert ngược: byte → string
-- Status: 0 → 'ONLINE', 1 → 'OFFLINE', ...
```

---

## 🎯 So sánh với các service khác

| Service    | Entity           | Status Field Type | Reason                    |
|-----------|------------------|-------------------|---------------------------|
| IAM       | User             | boolean (IsActive)| Simple active/inactive    |
| Patient   | PatientEntity    | boolean (IsDeleted)| Soft delete flag         |
| Notify    | NotificationJob  | **byte** ✅       | Multiple states (0-4)     |
| Instrument| Instrument       | **byte** ✅ NEW   | Multiple states (0-3)     |
| Instrument| InstrumentRun    | **byte** ✅ NEW   | Multiple states (0-2)     |

✅ Instrument service giờ đã đồng nhất với Notify service trong việc sử dụng `byte` cho status!

---

## 📚 File quan trọng

- **SQL Script**: `Instrument.Infrastructure/Migrations/Manual_Migration_Status_To_Enum.sql` 🔥
- **Enum Definitions**: `Instrument.Domain/Enums/`

---

## ⚠️ Lưu ý quan trọng

1. **Backup database** trước khi chạy SQL script ⚠️
2. **Đọc kỹ script** trước khi execute
3. Script có **TRANSACTION** (COMMIT/ROLLBACK) - an toàn
4. Nếu có giá trị không hợp lệ, sẽ **default về 0**
5. JSON response sẽ trả về **số** thay vì string
6. **Frontend cần update** để handle số thay vì string

---

## 🎉 Kết quả

✅ Build thành công  
✅ Tất cả enum đã được tạo  
✅ Tất cả entity, DTO, service, controller đã được update  
✅ SQL script thủ công đã sẵn sàng  
✅ Code đã clean (không có EF migration file)  

**Next Step:** 
1. Chạy SQL script `Manual_Migration_Status_To_Enum.sql` trong SSMS
2. Deploy code mới
3. Test API

**Status:** Ready to migrate! 🚀
