// src/pages/blog/BlogDetailPage.jsx
import React, { useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import Navbar from "../../components/navbar/Navbar";
import Footer from "../../components/footer/Footer";
import { blogPosts } from "../../data/blog";
import "./BlogDetailPage.css";

export default function BlogDetailPage() {
  const { id } = useParams();
  const post = blogPosts.find((p) => p.id === parseInt(id));

  // Scroll to top when component mounts or id changes
  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: "smooth" });
  }, [id]);

  if (!post) {
    return (
      <div className="blog-detail-page">
        <Navbar />
        <div className="blog-detail-content">
          <h1>Bài viết không tồn tại</h1>
          <Link to="/blog">Quay lại trang blog</Link>
        </div>
        <Footer />
      </div>
    );
  }

  // Lấy 2 bài viết liên quan (không bao gồm bài hiện tại)
  const relatedPosts = blogPosts.filter((p) => p.id !== post.id).slice(0, 2);

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
