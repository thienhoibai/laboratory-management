import React from "react";
import "./PackageSelection.css";
import { catalog } from "../../data/catalog"; // <-- import catalog chung

function PackageSelection({
  selectedPackage,
  onPackageSelect,
  onContinue,
  setPackageMode,
  mode,
}) {
  const packages = [
    {
      id: 1,
      title: "Xét nghiệm tổng quát",
      description:
        "Gói xét nghiệm cơ bản bao gồm công thức máu, đường huyết, chức năng gan",
      price: "450.000₫",
      duration: "30 phút",
      isPopular: true,
      includes: [1, 2, 3],
    },
    {
      id: 2,
      title: "Xét nghiệm sinh hóa",
      description:
        "Đánh giá chức năng gan, thận và các chỉ số sinh hóa quan trọng",
      price: "650.000₫",
      duration: "45 phút",
      isPopular: false,
      includes: [3, 4, 5, 6],
    },
    {
      id: 3,
      title: "Xét nghiệm toàn diện",
      description: "Gói xét nghiệm đầy đủ nhất cho sức khỏe tổng thể",
      price: "1.200.000₫",
      duration: "60 phút",
      isPopular: false,
      includes: [1, 2, 3, 4, 5, 6, 7],
    },
  ];

  // helper chuyển "1.200.000₫" -> 1200000 (number)
  const parsePrice = (priceStr) =>
    Number(String(priceStr).replace(/[^\d]/g, "")) || 0;

  const selectedPkg = packages.find((p) => p.id === selectedPackage) || null;
  const testsCount = selectedPkg ? selectedPkg.includes.length : 0;

  // Tổng tiền: dùng trực tiếp giá của gói (nếu có), fallback parse từ chuỗi
  const totalPriceNumber = selectedPkg
    ? typeof selectedPkg.price === "number"
      ? selectedPkg.price
      : parsePrice(selectedPkg.price)
    : 0;

  const formattedTotal = totalPriceNumber
    ? totalPriceNumber.toLocaleString("vi-VN") + "₫"
    : "0₫";

  return (
    <div className="package-selection">
      {/* Header: nút chuyển chế độ giống CatalogSelection */}
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          marginBottom: 12,
        }}
      >
        <div style={{ display: "flex", gap: 12 }}>
          <button
            className={`btn-primary-1 ${mode === "preset" ? "active" : ""}`}
            onClick={() => setPackageMode && setPackageMode("preset")}
            type="button"
          >
            Chọn gói có sẵn
          </button>
          <button
            className={`btn-secondary ${mode === "custom" ? "active" : ""}`}
            onClick={() => setPackageMode && setPackageMode("custom")}
            type="button"
          >
            Tùy chỉnh xét nghiệm
          </button>
        </div>
      </div>

      <div className="packages-grid">
        {packages.map((pkg) => (
          <div
            key={pkg.id}
            className={`package-card ${
              selectedPackage === pkg.id ? "selected" : ""
            }`}
            onClick={() => onPackageSelect(pkg.id)}
            role="button"
            tabIndex={0}
            onKeyPress={(e) => {
              if (e.key === "Enter") onPackageSelect(pkg.id);
            }}
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
                  aria-hidden="true"
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
                {pkg.includes.map((itemId, index) => {
                  const c = catalog.find((it) => it.id === itemId);
                  return (
                    <li key={index} className="includes-item">
                      <svg
                        className="check-icon"
                        width="16"
                        height="16"
                        viewBox="0 0 16 16"
                        fill="none"
                        aria-hidden="true"
                      >
                        <path
                          d="M13.5 4.5L6 12L2.5 8.5"
                          stroke="currentColor"
                          strokeWidth="2"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </svg>
                      <span>
                        {c ? c.name : itemId}
                        {c && c.price ? ` — ${c.price.toLocaleString()}₫` : ""}
                      </span>
                    </li>
                  );
                })}
              </ul>
            </div>
          </div>
        ))}
      </div>

      {/* Summary bar below packages (giống ảnh) */}
      <div className="selection-summary">
        <div className="summary-left">
          <div className="summary-title">Gói đã chọn</div>
          <div className="summary-sub">
            {selectedPkg ? selectedPkg.title : "Chưa chọn gói"}
          </div>
        </div>

        <div className="summary-right">
          <div className="summary-price">{formattedTotal}</div>
          <button
            className="summary-continue"
            onClick={() =>
              onContinue &&
              onContinue({
                source: "package",
                package: selectedPkg,
                total: totalPriceNumber,
              })
            }
            disabled={!selectedPkg}
          >
            Tiếp tục {selectedPkg && `(${testsCount} xét nghiệm)`}
          </button>
        </div>
      </div>
    </div>
  );
}

export default PackageSelection;
