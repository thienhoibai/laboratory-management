CREATE DATABASE BlogServiceDB;
GO
USE BlogServiceDB;
GO

--------------------------------------------------
-- TẠO BẢNG
--------------------------------------------------

CREATE TABLE Category (
  CategoryId INT IDENTITY(1,1) PRIMARY KEY,
  CategoryName NVARCHAR(100) NOT NULL,
  Description NVARCHAR(500),
  CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE BlogPost(
 PostId INT IDENTITY(1,1) PRIMARY KEY,
 Title NVARCHAR(255),
 Content NVARCHAR(max),
 AuthorId UNIQUEIDENTIFIER,
 CategoryId INT FOREIGN KEY REFERENCES Category(CategoryId),
 CreatedDate DATETIME DEFAULT GETDATE(),
 UpdatedDate DATETIME NULL,
 IsPublished BIT DEFAULT 0,
 IsApproved BIT DEFAULT 0,
 ThumbnailUrl NVARCHAR(500),
 Status INT DEFAULT 0
);

CREATE TABLE Tag(
 TagId INT IDENTITY(1,1) PRIMARY KEY,
 TagName NVARCHAR(50)
);

CREATE TABLE BlogPostTag(
 PostId INT FOREIGN KEY REFERENCES BlogPost(PostId),
 TagId INT FOREIGN KEY REFERENCES Tag(TagId),
 PRIMARY KEY (PostId, TagId)
);

CREATE TABLE Comment(
 CommentId INT PRIMARY KEY IDENTITY(1,1),
 PostId INT FOREIGN KEY REFERENCES BlogPost(PostId),
 UserId UNIQUEIDENTIFIER,
 Content NVARCHAR(max),
 CreatedDate DATETIME DEFAULT GETDATE(),
 IsUpdated BIT DEFAULT 0
);
 ALTER TABLE BlogPost

--------------------------------------------------
-- THÊM DỮ LIỆU
--------------------------------------------------

-- Category (5)
INSERT INTO Category (CategoryName, Description)
VALUES 
(N'Xét nghiệm máu cơ bản', N'Bài viết về xét nghiệm tổng quát'),
(N'Xét nghiệm chuyên sâu', N'Các xét nghiệm chuyên ngành'),
(N'Hướng dẫn & Kinh nghiệm', N'Kinh nghiệm và lưu ý khi xét nghiệm'),
(N'Tầm soát bệnh', N'Xét nghiệm phát hiện bệnh sớm'),
(N'Phân tích chỉ số', N'Giải thích các chỉ số sinh hóa trong máu');


-- BlogPost (5)
INSERT INTO BlogPost (Title, Content, AuthorId, CategoryId, IsPublished, IsApproved, ThumbnailUrl, Status)
VALUES
(N'Xét nghiệm máu tổng quát gồm những gì?',
 N'Bài viết mô tả các chỉ số CBC, HGB, WBC,...',
 NEWID(), 1, 1, 1, N'https://img.com/t1.jpg', 1),

(N'Tầm soát ung thư máu bằng xét nghiệm có chính xác không?',
 N'Tìm hiểu vai trò xét nghiệm máu trong phát hiện ung thư máu.',
 NEWID(), 2, 1, 0, N'https://img.com/t2.jpg', 0),

(N'Cần nhịn ăn bao lâu trước khi xét nghiệm máu?',
 N'Nhiều xét nghiệm yêu cầu nhịn 8–12 giờ để kết quả chuẩn.',
 NEWID(), 3, 1, 1, N'https://img.com/t3.jpg', 1),

(N'Xét nghiệm đường huyết HbA1c là gì?',
 N'Chỉ số HbA1c cho biết lượng đường trung bình trong 3 tháng.',
 NEWID(), 4, 1, 1, N'https://img.com/t4.jpg', 1),

(N'Men gan tăng có nguy hiểm không?',
 N'Xét nghiệm SGOT, SGPT đánh giá chức năng gan.',
 NEWID(), 5, 1, 1, N'https://img.com/t5.jpg', 2);


-- Tag
INSERT INTO Tag (TagName)
VALUES
(N'Xét nghiệm'),
(N'Sức khỏe'),
(N'Ung thư'),
(N'Sinh hóa'),
(N'Hướng dẫn'),
(N'Gan'),
(N'Đường huyết');


-- BlogPostTag (gắn tag cho 5 bài)
INSERT INTO BlogPostTag (PostId, TagId) VALUES
(1, 1), (1, 2), (1, 4),
(2, 1), (2, 3),
(3, 1), (3, 5),
(4, 1), (4, 7),
(5, 1), (5, 6);


-- Comment (5) - UserId là GUID
INSERT INTO Comment (PostId, UserId, Content)
VALUES
(1, NEWID(), N'Bài này rất hữu ích!'),
(1, NEWID(), N'Tôi vừa đi xét nghiệm, đúng như mô tả.'),
(2, NEWID(), N'Hy vọng thêm nhiều thông tin hơn.'),
(4, NEWID(), N'Tôi bị tiểu đường, chỉ số HbA1c rất quan trọng.'),
(5, NEWID(), N'Lúc xét nghiệm men gan của tôi tăng cao, rất lo lắng.');
