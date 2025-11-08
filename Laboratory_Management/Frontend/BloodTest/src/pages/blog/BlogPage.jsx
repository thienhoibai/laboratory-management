// src/pages/blog/BlogPage.jsx
import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import Navbar from "../../components/navbar/Navbar";
import Footer from "../../components/footer/Footer";
import { blogPosts, categories } from "../../data/blog";
import "./BlogPage.css";

export default function BlogPage() {
  const [selectedCategory, setSelectedCategory] = useState("Tất cả");

  // Scroll to top when component mounts
  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: "smooth" });
  }, []);

  const filteredPosts =
    selectedCategory === "Tất cả"
      ? blogPosts
      : blogPosts.filter(
          (post) =>
            post.category === selectedCategory || post.tag === selectedCategory
        );

  return (
    <div className="blog-page">
      <Navbar />
      <div className="blog-page-content">
        <div className="blog-page-header">
          <h1 className="blog-page-title">Kiến thức sức khỏe</h1>
          <p className="blog-page-subtitle">
            Cập nhật thông tin y tế và sức khỏe mới nhất từ các chuyên gia
          </p>
        </div>

        <div className="blog-category-filters">
          {categories.map((category) => (
            <button
              key={category}
              className={`category-filter-btn ${
                selectedCategory === category ? "active" : ""
              }`}
              onClick={() => setSelectedCategory(category)}
            >
              {category}
            </button>
          ))}
        </div>

        <div className="blog-posts-grid">
          {filteredPosts.map((post) => (
            <Link
              to={`/blog/${post.id}`}
              key={post.id}
              className="blog-post-card"
            >
              <img src={post.img} alt={post.title} className="blog-post-img" />
              <div className="blog-post-content">
                <div className="blog-post-meta">
                  <span className="blog-post-tag">{post.tag}</span>
                  <span className="blog-post-date">{post.date}</span>
                </div>
                <h3 className="blog-post-title">{post.title}</h3>
                <p className="blog-post-desc">{post.desc}</p>
                <div className="blog-post-author">Tác giả: {post.author}</div>
              </div>
            </Link>
          ))}
        </div>
      </div>
      <Footer />
    </div>
  );
}
