SELECT TOP (1000) [PermissionId]
      ,[Name]
      ,[Description]
      ,[CreatedAt]
      ,[UpdatedAt]
  FROM [LabIAM].[dbo].[Permissions]
Delete From Permissions
INSERT INTO Permissions (Name, Description) VALUES
-- IAM / USER & ROLE
('User.List',                N'Xem danh sách tài khoản'),
('User.View',                N'Xem chi tiết tài khoản'),
('User.Create',              N'Tạo tài khoản'),
('User.Update',              N'Cập nhật tài khoản'),
('User.Delete',              N'Xóa tài khoản'),
('Role.List',                N'Xem danh sách role'),
('Role.View',                N'Xem chi tiết role'),
('Role.Create',              N'Tạo role'),
('Role.Update',              N'Cập nhật role (bao gồm gán quyền)'),
('Role.Delete',              N'Xóa role'),
('User.Manage',              N'Truy cập màn RBAC (quản trị quyền)'),

-- PATIENT
('Patient.List',             N'Xem danh sách bệnh nhân'),
('Patient.View',             N'Xem chi tiết bệnh nhân'),
('Patient.Create',           N'Tạo bệnh nhân'),
('Patient.Update',           N'Cập nhật bệnh nhân'),
('Patient.Delete',           N'Xóa bệnh nhân'),
('Patient.Search',           N'Tìm kiếm bệnh nhân'),

-- BOOKING
('Booking.List',             N'Xem danh sách đặt lịch'),
('Booking.View',             N'Xem chi tiết lịch hẹn'),
('Booking.Create',           N'Tạo lịch hẹn'),
('Booking.View.Own',         N'Bệnh nhân xem lịch của chính mình'),
('Booking.Update.CheckIn',   N'Cập nhật check-in lịch hẹn'),
('Booking.Update.CheckOut',  N'Cập nhật check-out lịch hẹn'),

-- PAYMENT
('Payment.List',             N'Xem danh sách hóa đơn'),
('Payment.ByBooking.View',   N'Xem hóa đơn theo lịch hẹn'),
('Payment.View',             N'Xem chi tiết hóa đơn'),
('Payment.Create',           N'Tạo hóa đơn thanh toán'),

-- APPOINTMENTSLOT
('AppointmentSlot.List',         N'Xem danh sách khung giờ hẹn'),
('AppointmentSlot.ByDate.View',  N'Xem khung giờ theo ngày'),
('AppointmentSlot.CountAll.View',N'Xem tổng số slot đã đặt'),
('AppointmentSlot.CountByDate.View', N'Xem slot trong 1 ngày cụ thể'),

-- TESTCATALOG
('TestCatalog.List',         N'Xem danh sách danh mục xét nghiệm'),
('TestCatalog.Create',       N'Tạo danh mục xét nghiệm'),
('TestCatalog.View',         N'Xem chi tiết danh mục xét nghiệm'),
('TestCatalog.Update',       N'Cập nhật danh mục xét nghiệm'),
('TestCatalog.UpdateParameter', N'Cập nhật tham số trong danh mục xét nghiệm'),
('TestCatalog.DeleteParameter', N'Xóa tham số trong danh mục xét nghiệm'),
('TestCatalog.Delete',       N'Xóa danh mục xét nghiệm'),

-- TESTBUNDLE
('TestBundle.List',          N'Xem danh sách gói xét nghiệm'),
('TestBundle.Create',        N'Tạo gói xét nghiệm'),
('TestBundle.View',          N'Xem chi tiết gói xét nghiệm'),
('TestBundle.Update',        N'Cập nhật gói xét nghiệm'),
('TestBundle.Delete',        N'Xóa gói xét nghiệm'),

-- TESTPARAMETER
('TestParameter.List',       N'Xem danh sách thông số xét nghiệm'),
('TestParameter.Create',     N'Tạo thông số xét nghiệm'),
('TestParameter.View',       N'Xem chi tiết thông số xét nghiệm'),

-- CATALOGBUNDLE
('CatalogBundle.List',       N'Xem danh sách danh mục gói xét nghiệm'),
('CatalogBundle.Create',     N'Tạo danh mục gói xét nghiệm'),
('CatalogBundle.View',       N'Xem chi tiết danh mục gói xét nghiệm'),
('CatalogBundle.Delete',     N'Xóa danh mục gói xét nghiệm'),

-- BLOGPOST
('BlogPost.List',            N'Xem danh sách bài viết'),
('BlogPost.Create',          N'Tạo bài viết'),
('BlogPost.View',            N'Xem chi tiết bài viết'),
('BlogPost.Update',          N'Cập nhật bài viết'),
('BlogPost.Delete',          N'Xóa bài viết'),
('BlogPost.Approved.View',   N'Xem bài viết đã được duyệt'),
('BlogPost.Status.Update',   N'Cập nhật trạng thái bài viết'),

-- COMMENT
('Comment.Post.View',        N'Xem bình luận theo bài viết'),
('Comment.View',             N'Xem chi tiết bình luận'),
('Comment.Create',           N'Thêm bình luận'),
('Comment.Update',           N'Cập nhật bình luận'),
('Comment.Delete',           N'Xóa bình luận'),
('Comment.Search',           N'Tìm kiếm bình luận'),

-- BLOGCATEGORY
('BlogCategory.List',        N'Xem danh mục bài viết'),
('BlogCategory.Create',      N'Tạo danh mục bài viết'),
('BlogCategory.View',        N'Xem chi tiết danh mục bài viết'),
('BlogCategory.Update',      N'Cập nhật danh mục bài viết'),
('BlogCategory.Delete',      N'Xóa danh mục bài viết'),

-- TAG
('Tag.List',                 N'Xem danh sách nhãn'),
('Tag.View',                 N'Xem chi tiết nhãn'),
('Tag.Create',               N'Tạo nhãn'),
('Tag.Update',               N'Cập nhật nhãn'),
('Tag.Delete',               N'Xóa nhãn'),

-- BLOGTAG
('BlogTag.BlogPost.View',    N'Xem tag theo bài viết'),
('BlogTag.Tag.View',         N'Xem bài viết theo tag'),
('BlogTag.Create',           N'Gán tag cho bài viết'),
('BlogTag.Delete',           N'Xóa tag khỏi bài viết'),

-- INSTRUMENT
('Instrument.List',          N'Xem danh sách máy xét nghiệm'),
('Instrument.View',          N'Xem chi tiết máy xét nghiệm'),
('Instrument.Create',        N'Tạo máy xét nghiệm'),
('Instrument.Update',        N'Cập nhật máy xét nghiệm'),
('Instrument.Delete',        N'Xóa máy xét nghiệm'),
('Instrument.Maintain',      N'Bảo trì máy xét nghiệm'),

-- LOG
('Log.List',                 N'Xem danh sách log'),
('Log.View',                 N'Xem chi tiết log'),
('Log.Export',               N'Xuất log'),
('Log.Delete',               N'Xóa log'),

-- SCHEDULE
('Schedule.List',            N'Xem danh sách lịch'),
('Schedule.View',            N'Xem chi tiết lịch'),
('Schedule.Create',          N'Tạo lịch'),
('Schedule.Update',          N'Cập nhật lịch'),
('Schedule.Delete',          N'Xóa lịch'),

-- TESTPACKAGE
('TestPackage.List',         N'Xem danh sách gói xét nghiệm (TestPackage)'),
('TestPackage.View',         N'Xem chi tiết gói xét nghiệm (TestPackage)'),
('TestPackage.Create',       N'Tạo gói xét nghiệm (TestPackage)'),
('TestPackage.Update',       N'Cập nhật gói xét nghiệm (TestPackage)'),
('TestPackage.Delete',       N'Xóa gói xét nghiệm (TestPackage)'),

-- NOTIFICATION
('Notification.List',        N'Xem danh sách thông báo'),
('Notification.View',        N'Xem chi tiết thông báo'),
('Notification.Send',        N'Gửi thông báo'),
('Notification.Delete',      N'Xóa thông báo'),

-- REPORT
('Report.View',              N'Xem báo cáo'),
('Report.Export',            N'Xuất báo cáo'),
('Report.Dashboard',         N'Xem dashboard');
