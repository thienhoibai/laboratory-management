// src/components/home/BlogSection.jsx
import React from "react";
import "./BlogSection.css";

const blogs = [
  {
    tag: "Hướng dẫn",
    time: "5 ngày trước",
    title: "Chuẩn bị gì trước khi xét nghiệm máu?",
    desc: "Hướng dẫn chi tiết về cách chuẩn bị trước khi đi xét nghiệm máu để đảm bảo kết quả chính xác nhất.",
    img: "https://umcclinic.com.vn/Data/Sites/1/media/l%E1%BA%A5y-m%C3%A1u-x%C3%A9t-nghi%E1%BB%87m/t%E1%BA%A1i-v%E1%BB%8B-tr%C3%AD-l%E1%BA%A5y-m%C3%A1u-x%C3%A9t-nghi%E1%BB%87m-khi-kim-ti%C3%AAm-%C4%91i-v%C3%A0o-c%C3%B3-th%E1%BB%83-b%E1%BB%8B-b%E1%BA%A7m-t%C3%ADm.jpg",
  },
  {
    tag: "Sức khỏe",
    time: "1 tuần trước",
    title: "Chế độ ăn uống ảnh hưởng đến kết quả xét nghiệm",
    desc: "Tìm hiểu về mối liên hệ giữa chế độ ăn uống và các chỉ số xét nghiệm máu quan trọng.",
    img: "https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=600&q=80",
  },
  {
    tag: "Kiến thức",
    time: "2 tuần trước",
    title: "Hiểu rõ các chỉ số trong kết quả xét nghiệm",
    desc: "Giải thích chi tiết về ý nghĩa của các chỉ số thường gặp trong phiếu kết quả xét nghiệm máu.",
    img: "https://cdn.nhathuoclongchau.com.vn/unsafe/800x0/https://cms-prod.s3-sgn09.fptcloud.com/cac_chi_so_trong_xet_nghiem_mau_la_gi_va_co_y_nghia_nhu_the_nao3_355070912b.jpg",
  },
];

export default function BlogSection() {
  return (
    <div className="blog-section-bg">
      <div className="blog-section-header">
        <div className="blog-title-group">
          <span className="blog-badge">Blog & Tin tức</span>
          <h2 className="blog-title"></h2>
        </div>
        <p className="blog-desc">
        <h2 className="blog-title">Kiến thức sức khỏe</h2>
        </p>
        <a href="/blog" className="blog-viewall">
          Xem tất cả &rarr;
        </a>
      </div>
      <div className="blog-cards">
        {blogs.map((blog, idx) => (
          <div className="blog-card" key={idx}>
            <img src={blog.img} alt={blog.title} className="blog-img" />
            <div className="blog-card-content">
              <div className="blog-meta">
                <span className="blog-tag">{blog.tag}</span>
                <span className="blog-time">{blog.time}</span>
              </div>
              <div className="blog-card-title">{blog.title}</div>
              <div className="blog-card-desc">{blog.desc}</div>
              <a href="/blog" className="blog-readmore">
                Đọc thêm &rarr;
              </a>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
