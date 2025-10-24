import React, { useState } from "react";
import "./PackageSelection.css";
import { catalog } from "../../data/catalog"; // <-- sử dụng dữ liệu chung

function CatalogSelection({ setPackageMode, onContinue }) {
  const [selectedIds, setSelectedIds] = useState(new Set());

  const toggle = (id) => {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    setSelectedIds(next);
  };

  const parsePrice = (s) => Number(String(s).replace(/[^\d]/g, "")) || 0;
  const total = Array.from(selectedIds).reduce((sum, id) => {
    const item = catalog.find((c) => c.id === id);
    return sum + (item ? parsePrice(item.price) : 0);
  }, 0);

  const selectedItemsArray = Array.from(selectedIds).map((id) =>
    catalog.find((c) => c.id === id)
  );

  return (
    <div className="package-selection">
      {/* Header: nút quay về Chọn gói có sẵn (không reload trang) */}
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
            className="btn-primary-1"
            onClick={() => setPackageMode && setPackageMode("preset")}
          >
            Chọn gói có sẵn
          </button>
          <button className="btn-secondary active">Tùy chỉnh xét nghiệm</button>
        </div>
      </div>

      <div className="packages-grid" style={{ marginTop: 8 }}>
        {catalog.map((item) => (
          <div
            key={item.id}
            className="package-card"
            style={{ minHeight: 110, padding: 14 }}
          >
            <label
              style={{
                display: "flex",
                gap: 12,
                alignItems: "flex-start",
                cursor: "pointer",
              }}
            >
              <input
                type="checkbox"
                checked={selectedIds.has(item.id)}
                onChange={() => toggle(item.id)}
                style={{ marginTop: 6 }}
              />
              <div style={{ width: "100%" }}>
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 12,
                  }}
                >
                  <div style={{ fontWeight: 700, color: "#0f172a" }}>
                    {item.name}
                  </div>
                  <div style={{ color: "#3b82f6", fontWeight: 800 }}>
                    {item.price}đ
                  </div>
                </div>
                <div
                  style={{
                    marginTop: 6,
                    color: "#6b7280",
                    fontSize: 13,
                  }}
                >
                  {item.description}
                </div>
              </div>
            </label>
          </div>
        ))}
      </div>

      {/* Summary bên dưới */}
      <div className="selection-summary" style={{ justifyContent: "flex-end" }}>
        <div
          style={{
            display: "flex",
            gap: 12,
            alignItems: "center",
          }}
        >
          <div style={{ color: "#6b7280" }}>{selectedIds.size} xét nghiệm</div>
          <div style={{ fontWeight: 800, color: "#3b82f6" }}>
            {total.toLocaleString()}₫
          </div>
          <button
            className="summary-continue"
            onClick={() => {
              onContinue &&
                onContinue({
                  source: "catalog",
                  items: selectedItemsArray,
                  total,
                });
            }}
            disabled={selectedIds.size === 0}
          >
            Tiếp tục ({selectedIds.size} xét nghiệm)
          </button>
        </div>
      </div>
    </div>
  );
}

export default CatalogSelection;
