// src/pages/blog/BlogPage.jsx
import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import Navbar from "../../components/navbar/Navbar";
import Footer from "../../components/footer/Footer";
import BlogService from "../../services/BlogService";
import "./BlogPage.css";

export default function BlogPage() {
  const [selectedCategory, setSelectedCategory] = useState("Tất cả");
  const [blogPosts, setBlogPosts] = useState([]);
  const [categories, setCategories] = useState(["Tất cả"]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: "smooth" });
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);

      // Load blogs đã duyệt và categories song song
      const [blogsData, categoriesData] = await Promise.all([
        BlogService.getApprovedBlogs(1, 100),
        BlogService.getCategories(),
      ]);

      setBlogPosts(blogsData);

      // Tạo danh sách categories
      const categoryNames = [
        "Tất cả",
        ...categoriesData.map((cat) => cat.name),
      ];
      setCategories(categoryNames);

      setError(null);
    } catch (err) {
      console.error("Error loading blog page data:", err);
      setError("Không thể tải dữ liệu. Vui lòng thử lại sau.");
    } finally {
      setLoading(false);
    }
  };

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

        {loading ? (
          <div className="blog-page-loading">
            <p>Đang tải dữ liệu...</p>
          </div>
        ) : error ? (
          <div className="blog-page-error">
            <p>{error}</p>
          </div>
        ) : (
          <>
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

            {filteredPosts.length === 0 ? (
              <div className="blog-page-empty">
                <p>Không có bài viết nào trong danh mục này.</p>
              </div>
            ) : (
              <div className="blog-posts-grid">
                {filteredPosts.map((post) => (
                  <Link
                    to={`/blog/${post.id}`}
                    key={post.id}
                    className="blog-post-card"
                  >
                    <img
                      src={post.img || post.thumbnailUrl || post.imageUrl}
                      alt={post.title}
                      className="blog-post-img"
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }}
                    />
                    <div className="blog-post-content">
                      <div className="blog-post-meta">
                        <span className="blog-post-tag">
                          {post.tag || post.category}
                        </span>
                        <span className="blog-post-date">{post.date}</span>
                      </div>
                      <h3 className="blog-post-title">{post.title}</h3>
                      <p className="blog-post-desc">{post.desc}</p>
                      <div className="blog-post-author">
                        Tác giả: {post.author}
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </>
        )}
      </div>
      <Footer />
    </div>
  );
}
