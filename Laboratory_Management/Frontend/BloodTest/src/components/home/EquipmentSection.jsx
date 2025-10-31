// src/components/home/EquipmentSection.jsx
import React from "react";
import "./EquipmentSection.css";

const equipments = [
  {
    name: "Erba H360 Analyzer",
    description:
      "Máy phân tích Erba H360 với giao diện trực quan đảm bảo kết quả nhất quán.",
    category: "Phân tích sinh hóa",
    rating: 4.9,
    img: "https://img.medicalexpo.com/images_me/photo-g/68387-16287442.webp", // Link ảnh mới
  },
  {
    name: "Mindray System",
    description:
      "Mindray cho phép phân tích đồng thời nhiều mẫu, giúp giảm thời gian chờ đợi của bạn.",
    category: "Xét nghiệm huyết học",
    rating: 4.8,
    img: "https://www.mindray.com/content/xpace/en/products/anesthesia/a7.thumb.1280.1280.png", // Link ảnh mới
  },
  {
    name: "Sysmex XN-1000",
    description:
      "Công nghệ Nhật Bản từ Sysmex XN-1000 mang lại độ chính xác gần như hoàn hảo cho các thông số huyết học.",
    category: "Xét nghiệm huyết học nâng cao",
    rating: 5.0,
    img: "https://kimhung.vn/wp-content/uploads/2020/09/Xn1000.png", // Link ảnh mới
  },
];

export default function EquipmentSection() {
  return (
    <div className="equipment-section-bg" id="equipments">
      <span className="equipment-badge">Thiết bị y tế</span>
      <h2 className="equipment-title">Thiết bị y tế hiện đại</h2>
      <p className="equipment-desc">
        Trang bị công nghệ tiên tiến giúp chẩn đoán và điều trị chính xác, an
        toàn.
      </p>
      <div className="equipment-cards">
        {equipments.map((item, idx) => (
          <div className="equipment-card" key={idx}>
            <div className="equipment-img-bg">
              <img src={item.img} alt={item.name} className="equipment-img" />
            </div>
            <div className="equipment-info">
              <div className="equipment-header">
                <span className="equipment-name">{item.name}</span>
                <span className="equipment-rating">★ {item.rating}</span>
              </div>
              <p className="equipment-card-desc">{item.description}</p>
              <div className="equipment-category">
                <span className="equipment-category-icon">♡</span>
                {item.category}
              </div>
              <button className="equipment-btn">Xem chi tiết</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
