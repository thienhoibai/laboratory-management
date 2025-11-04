// src/components/home/BlogSection.jsx
import React from "react";
import { Link } from "react-router-dom";
import "./BlogSection.css";
import { blogPosts } from "../../data/blog";

// Lấy 3 bài blog đầu tiên để hiển thị trên homepage
const blogs = blogPosts.slice(0, 3);

export default function BlogSection() {
  return (
    <div className="blog-section-bg">
      <div className="blog-section-header">
        <span className="blog-badge">Blog & Tin tức</span>
        <div className="blog-title-group">
          <h2 className="blog-title">Kiến thức sức khỏe</h2>
          <p className="blog-desc">
            Cập nhật thông tin y tế và lời khuyên từ chuyên gia
          </p>
        </div>
        <Link to="/blog" className="blog-viewall">
          Xem tất cả &rarr;
        </Link>
      </div>
      <div className="blog-cards">
        {blogs.map((blog) => (
          <Link to={`/blog/${blog.id}`} key={blog.id} className="blog-card">
            <img src={blog.img} alt={blog.title} className="blog-img" />
            <div className="blog-card-content">
              <div className="blog-meta">
                <span className="blog-tag">{blog.tag}</span>
                <span className="blog-time">{blog.time}</span>
              </div>
              <div className="blog-card-title">{blog.title}</div>
              <div className="blog-card-desc">{blog.desc}</div>
              <span className="blog-readmore">Đọc thêm &rarr;</span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
