import React, { useState } from "react";
import "./PackageSelection.css";

function PackageSelection({ selectedPackage, onPackageSelect }) {
  const [searchTerm, setSearchTerm] = useState("");

  const packages = [
    {
      id: 1,
      title: "Xét nghiệm tổng quát",
      description:
        "Gói xét nghiệm cơ bản bao gồm công thức máu, đường huyết, mỡ máu",
      price: "450.000₫",
      duration: "30 phút",
      isPopular: true,
      includes: ["Công thức máu", "Đường huyết", "Mỡ máu", "Chức năng gan"],
    },
    {
      id: 2,
      title: "Xét nghiệm sinh hóa",
      description:
        "Đánh giá chức năng gan, thận và các chỉ số sinh hóa quan trọng",
      price: "650.000₫",
      duration: "45 phút",
      isPopular: false,
      includes: [
        "Chức năng gan",
        "Chức năng thận",
        "Điện giải đồ",
        "Protein máu",
      ],
    },
    {
      id: 3,
      title: "Xét nghiệm toàn diện",
      description: "Gói xét nghiệm đầy đủ nhất cho sức khỏe tổng thể",
      price: "1.200.000₫",
      duration: "60 phút",
      isPopular: false,
      includes: [
        "Tất cả xét nghiệm cơ bản",
        "Hormone",
        "Vitamin",
        "Dấu ấn ung thư",
      ],
    },
  ];

  const filteredPackages = packages.filter(
    (pkg) =>
      pkg.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      pkg.description.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="package-selection">
      <div className="search-container">
        <div className="search-input-wrapper">
          <svg
            className="search-icon"
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
          >
            <path
              d="M9 17A8 8 0 1 0 9 1a8 8 0 0 0 0 16zM19 19l-4.35-4.35"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
          <input
            type="text"
            placeholder="Tìm kiếm gói xét nghiệm..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
        </div>
      </div>

      <div className="packages-grid">
        {filteredPackages.map((pkg) => (
          <div
            key={pkg.id}
            className={`package-card ${
              selectedPackage === pkg.id ? "selected" : ""
            }`}
            onClick={() => onPackageSelect(pkg.id)}
          >
            {pkg.isPopular && <div className="popular-badge">Phổ biến</div>}

            <div className="package-header">
              <h3 className="package-title">{pkg.title}</h3>
              <p className="package-description">{pkg.description}</p>
            </div>

            <div className="package-price">
              <span className="price-amount">{pkg.price}</span>
              <div className="package-duration">
                <svg
                  className="clock-icon"
                  width="16"
                  height="16"
                  viewBox="0 0 16 16"
                  fill="none"
                >
                  <circle
                    cx="8"
                    cy="8"
                    r="7"
                    stroke="currentColor"
                    strokeWidth="1.5"
                  />
                  <path
                    d="M8 4v4l3 3"
                    stroke="currentColor"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
                <span>{pkg.duration}</span>
              </div>
            </div>

            <div className="package-includes">
              <h4 className="includes-title">Bao gồm:</h4>
              <ul className="includes-list">
                {pkg.includes.map((item, index) => (
                  <li key={index} className="includes-item">
                    <svg
                      className="check-icon"
                      width="16"
                      height="16"
                      viewBox="0 0 16 16"
                      fill="none"
                    >
                      <path
                        d="M13.5 4.5L6 12L2.5 8.5"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default PackageSelection;
