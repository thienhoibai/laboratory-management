
USE master;
ALTER DATABASE BlogServiceDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE BlogServiceDB;
use BlogServiceDB
CREATE TABLE Category (
  CategoryId INT IDENTITY(1,1) PRIMARY KEY,
  CategoryName NVARCHAR(100) NOT NULL,
  Description NVARCHAR(500),
  CreatedDate DATETIME,
);
CREATE TABLE BlogPost(
 PostId INT IDENTITY(1,1) PRIMARY KEY,
 Title NVARCHAR(255),
 Content NVARCHAR (max),
 AuthorId UniqueIdentifier,
 CategoryId INT FOREIGN KEY REFERENCES Category(CategoryId),
 CreatedDate DATETIME DEFAULT GETDATE(),
 UpdatedDate DATETIME NULL,
 IsPublished BIT DEFAULT 0,
 IsApproved BIT DEFAULT 0,
 ThumbnailUrl NVARCHAR(500),
 Status INT DEFAULT 0  -- 0 = Chờ duyệt, 1 = Đã duyệt, 2 = Đã hủy

CREATE TABLE Tag(
TagId INT IDENTITY(1,1) PRIMARY KEY,
TagName NVARCHAR(50),
);
CREATE TABLE BlogPostTag(
PostId INT FOREIGN KEY REFERENCES BlogPost(PostId),
TagId INT FOREIGN KEY REFERENCES Tag(TagId)
PRIMARY KEY (PostId, TagId)
);
CREATE TABLE Comment(
CommentId INT PRIMARY KEY IDENTITY(1,1),
PostId INT FOREIGN KEY REFERENCES BlogPost(PostId),
UserId INT,
Content NVARCHAR(max),
CreatedDate DATETIME,
IsUpdated BIT
);
 ALTER TABLE BlogPost



select * from BlogPost



CREATE DATABASE BlogServiceDB;
GO

