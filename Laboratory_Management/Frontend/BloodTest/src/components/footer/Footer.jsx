import React from "react";
import "./Footer.css";

function Footer() {
  return (
    <footer className="footer-bg">
      <div className="footer-container">
        <div className="footer-col">
          <div className="footer-logo">
            <img
              src="/logo.png"
              alt="HemaLink Logo"
              className="footer-logo-img"
            />
            <span className="footer-logo-text">HemaLink</span>
          </div>
          <p className="footer-desc">
            Kết nối bệnh nhân và chẩn đoán, mang đến dịch vụ y tế chất lượng và
            đáng tin cậy.
          </p>
        </div>
        <div className="footer-col">
          <div className="footer-title">Dịch vụ</div>
          <ul>
            <li>Xét nghiệm máu</li>
            <li>Xét nghiệm sinh hóa</li>
            <li>Xét nghiệm vi sinh</li>
            <li>Tư vấn sức khỏe</li>
          </ul>
        </div>
        <div className="footer-col">
          <div className="footer-title">Về chúng tôi</div>
          <ul>
            <li>Giới thiệu</li>
            <li>Đội ngũ bác sĩ</li>
            <li>Blog</li>
            <li>Liên hệ</li>
          </ul>
        </div>
        <div className="footer-col">
          <div className="footer-title">Liên hệ</div>
          <ul>
            <li>Email: info@hemalink.vn</li>
            <li>Hotline: 1900 xxxx</li>
            <li>Địa chỉ: TP. Hồ Chí Minh</li>
          </ul>
        </div>
      </div>
      <div className="footer-bottom">
        © 2025 HemaLink. Tất cả quyền được bảo lưu.
      </div>
    </footer>
  );
}

export default Footer;
