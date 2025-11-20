// src/pages/blog/BlogDetailPage.jsx
import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import Navbar from "../../components/navbar/Navbar";
import Footer from "../../components/footer/Footer";
import BlogService from "../../services/BlogService";
import "./BlogDetailPage.css";

export default function BlogDetailPage() {
  const { id } = useParams();
  const [post, setPost] = useState(null);
  const [relatedPosts, setRelatedPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: "smooth" });
    loadBlogDetail();
  }, [id]);

  const loadBlogDetail = async () => {
    try {
      setLoading(true);
      setError(null);

      // Gọi API để lấy chi tiết bài blog theo ID
      const blogData = await BlogService.getBlogById(id);
      setPost(blogData);

      // Lấy danh sách các bài blog khác để hiển thị bài viết liên quan
      const allBlogs = await BlogService.getAllBlogs();
      const related = allBlogs
        .filter(
          (blog) => blog.id !== parseInt(id) && blog.status === "approved"
        )
        .slice(0, 2);
      setRelatedPosts(related);
    } catch (err) {
      console.error("Error loading blog detail:", err);
      setError("Không thể tải bài viết. Vui lòng thử lại sau.");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="blog-detail-page">
        <Navbar />
        <div className="blog-detail-content">
          <div className="blog-detail-loading">
            <p>Đang tải bài viết...</p>
          </div>
        </div>
        <Footer />
      </div>
    );
  }

  if (error || !post) {
    return (
      <div className="blog-detail-page">
        <Navbar />
        <div className="blog-detail-content">
          <div className="blog-detail-error">
            <h1>{error || "Bài viết không tồn tại"}</h1>
            <Link to="/blog" className="back-to-blog-btn">
              Quay lại trang blog
            </Link>
          </div>
        </div>
        <Footer />
      </div>
    );
  }

  return (
    <div className="blog-detail-page">
      <Navbar />
      <div className="blog-detail-content">
        <div className="blog-detail-breadcrumb">
          <Link to="/">Trang chủ</Link>
          <span> / </span>
          <Link to="/blog">Blog</Link>
          <span> / </span>
          <span>{post.title}</span>
        </div>

        <article className="blog-detail-article">
          <h1 className="blog-detail-title">{post.title}</h1>

          <div className="blog-detail-meta">
            <span className="blog-detail-author">Tác giả: {post.author}</span>
            <span className="blog-detail-date">{post.fullDate}</span>
          </div>

          <img src={post.img} alt={post.title} className="blog-detail-img" />

          <div
            className="blog-detail-body"
            dangerouslySetInnerHTML={{ __html: post.content }}
          />

          <div className="blog-detail-share">
            <span className="blog-detail-share-label">Chia sẻ bài viết:</span>
            <div className="blog-detail-share-buttons">
              <a
                href={`https://www.facebook.com/sharer/sharer.php?u=${window.location.href}`}
                target="_blank"
                rel="noopener noreferrer"
                className="share-btn facebook"
              >
                Facebook
              </a>
              <a
                href={`https://twitter.com/intent/tweet?url=${window.location.href}&text=${post.title}`}
                target="_blank"
                rel="noopener noreferrer"
                className="share-btn twitter"
              >
                Twitter
              </a>
              <a
                href={`https://www.linkedin.com/sharing/share-offsite/?url=${window.location.href}`}
                target="_blank"
                rel="noopener noreferrer"
                className="share-btn linkedin"
              >
                LinkedIn
              </a>
            </div>
          </div>
        </article>

        {relatedPosts.length > 0 && (
          <section className="blog-related-posts">
            <h2 className="blog-related-title">Bài viết liên quan</h2>
            <div className="blog-related-grid">
              {relatedPosts.map((relatedPost) => (
                <Link
                  to={`/blog/${relatedPost.id}`}
                  key={relatedPost.id}
                  className="blog-related-card"
                >
                  <img
                    src={relatedPost.img}
                    alt={relatedPost.title}
                    className="blog-related-img"
                  />
                  <div className="blog-related-content">
                    <div className="blog-related-date">{relatedPost.date}</div>
                    <h3 className="blog-related-card-title">
                      {relatedPost.title}
                    </h3>
                  </div>
                </Link>
              ))}
            </div>
          </section>
        )}
      </div>
      <Footer />
    </div>
  );
}
