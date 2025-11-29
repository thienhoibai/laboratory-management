// src/components/home/EquipmentSection.jsx
import React, { useEffect, useState } from "react";
import "./EquipmentSection.css";
import InstrumentService from "../../services/InstrumentService";
import { setAuthToken } from "../../utils/auth";

const API_BASE_URL = "http://localhost:8080/";

const STATUS_LABELS = {
  0: "Đang hoạt động",
  1: "Đang tắt",
  2: "Đang lỗi",
  3: "Đang bảo trì",
  ACTIVE: "Đang hoạt động",
  OFF: "Đang tắt",
  ERROR: "Đang lỗi",
  MAINTENANCE: "Đang bảo trì",
};

export default function EquipmentSection() {
  const [equipments, setEquipments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentIndex, setCurrentIndex] = useState(0);
  const itemsPerPage = 3;

  useEffect(() => {
    fetchEquipments();
  }, []);

  const fetchEquipments = async () => {
    try {
      // Thử lấy token nếu có (user đã login)
      const token = localStorage.getItem("accessToken");
      if (token) {
        setAuthToken(token);
        const response = await InstrumentService.list({
          pageSize: 100,
          page: 1,
        });
        const items = response.items || [];
        console.log("Fetched equipments:", items);
        // Log image data for debugging
        items.forEach((item, index) => {
          console.log(`Equipment ${index}:`, {
            name: item.name,
            code: item.code,
            imageUrl: item.imageUrl,
            imagePath: item.imagePath,
            imageData: item.imageData ? "Base64 data present" : "No base64",
            fullItem: item,
          });
        });
        setEquipments(items);
      } else {
        // Nếu chưa login, không hiển thị dữ liệu
        setEquipments([]);
      }
    } catch (error) {
      console.error("Error fetching equipments:", error);
      // Nếu có lỗi (401, 403...), không hiển thị dữ liệu mẫu
      setEquipments([]);
    } finally {
      setLoading(false);
    }
  };

  const getStatusLabel = (status) => {
    return STATUS_LABELS[status] || "Không xác định";
  };

  const getStatusClass = (status) => {
    if (status === 0 || status === "ACTIVE") return "status-active";
    if (status === 1 || status === "OFF") return "status-off";
    if (status === 2 || status === "ERROR") return "status-error";
    if (status === 3 || status === "MAINTENANCE") return "status-maintenance";
    return "status-unknown";
  };

  const getImageUrl = (instrument) => {
    // Nếu có imageData (base64), sử dụng trực tiếp
    if (instrument.imageData) {
      // Kiểm tra xem đã có prefix data: chưa
      if (instrument.imageData.startsWith("data:image")) {
        return instrument.imageData;
      }
      // Nếu chưa có prefix, thêm vào
      return `data:image/jpeg;base64,${instrument.imageData}`;
    }

    // Lấy code và fileName
    const code = instrument.code || instrument.instrumentCode;
    const fileName = instrument.imageUrl || instrument.imagePath;

    // Nếu có imageUrl/imagePath là tên file
    if (fileName) {
      // Nếu là URL đầy đủ (bắt đầu bằng http/https), dùng trực tiếp
      if (fileName.startsWith("http://") || fileName.startsWith("https://")) {
        return fileName;
      }

      // Nếu là tên file (có extension), thử các endpoint
      if (fileName.match(/\.(jpg|jpeg|png|gif|webp)$/i)) {
        // Encode tên file để xử lý khoảng trắng và ký tự đặc biệt
        const encodedFileName = encodeURIComponent(fileName);

        // Ưu tiên 1: Endpoint API theo code (nếu có)
        if (code) {
          return `${API_BASE_URL}instrument/api/instruments/${code}/image`;
        }

        // Ưu tiên 2: Thử các endpoint static files phổ biến
        const possibleEndpoints = [
          `Images/${encodedFileName}`, // Encode để xử lý khoảng trắng
          `Images/${fileName}`, // Không encode (thử cả hai)
          `BlogService.Presentation/Images/${encodedFileName}`,
          `BlogService.Presentation/Images/${fileName}`,
          `instrument/api/instruments/image/${encodedFileName}`,
          `instrument/api/instruments/image/${fileName}`,
        ];

        // Trả về endpoint đầu tiên
        return `${API_BASE_URL}${possibleEndpoints[0]}`;
      }

      // Nếu không phải tên file, ghép với baseURL
      return `${API_BASE_URL}${fileName}`;
    }

    // Nếu chỉ có code, thử endpoint API
    if (code) {
      return `${API_BASE_URL}instrument/api/instruments/${code}/image`;
    }

    return "";
  };

  const getVisibleEquipments = () => {
    const startIndex = currentIndex;
    const endIndex = startIndex + itemsPerPage;
    return equipments.slice(startIndex, endIndex);
  };

  const handlePrevious = () => {
    setCurrentIndex((prev) => Math.max(0, prev - 1));
  };

  const handleNext = () => {
    setCurrentIndex((prev) =>
      Math.min(equipments.length - itemsPerPage, prev + 1)
    );
  };

  const canGoPrevious = currentIndex > 0;
  const canGoNext = currentIndex < equipments.length - itemsPerPage;

  if (loading) {
    return (
      <div className="equipment-section-bg" id="equipments">
        <span className="equipment-badge">Thiết bị y tế</span>
        <h2 className="equipment-title">Thiết bị y tế hiện đại</h2>
        <p className="equipment-desc">
          Trang bị công nghệ tiên tiến giúp chẩn đoán và điều trị chính xác, an
          toàn.
        </p>
        <div className="equipment-cards">
          <p style={{ textAlign: "center", width: "100%", color: "#6b7280" }}>
            Đang tải dữ liệu thiết bị...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="equipment-section-bg" id="equipments">
      <span className="equipment-badge">Thiết bị y tế</span>
      <h2 className="equipment-title">Thiết bị y tế hiện đại</h2>
      <p className="equipment-desc">
        Trang bị công nghệ tiên tiến giúp chẩn đoán và điều trị chính xác, an
        toàn.
      </p>
      <div className="equipment-carousel-container">
        {equipments.length > itemsPerPage && (
          <button
            className={`equipment-nav-btn equipment-nav-btn-left ${
              !canGoPrevious ? "disabled" : ""
            }`}
            onClick={handlePrevious}
            disabled={!canGoPrevious}
            aria-label="Thiết bị trước"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M15 18l-6-6 6-6" />
            </svg>
          </button>
        )}
        <div className="equipment-cards">
          {equipments.length === 0 ? (
            <p style={{ textAlign: "center", width: "100%", color: "#6b7280" }}>
              Chưa có thiết bị nào.
            </p>
          ) : (
            getVisibleEquipments().map((item) => (
              <div className="equipment-card" key={item.code || item.id}>
                <div className="equipment-img-bg">
                  {getImageUrl(item) ? (
                    <img
                      src={getImageUrl(item)}
                      alt={item.name}
                      className="equipment-img"
                      onError={(e) => {
                        console.error(
                          "Failed to load image:",
                          getImageUrl(item),
                          item
                        );
                        e.target.style.display = "none";
                        // Hiển thị placeholder khi ảnh lỗi
                        const placeholder =
                          e.target.parentElement.querySelector(
                            ".equipment-img-placeholder"
                          );
                        if (placeholder) {
                          placeholder.style.display = "flex";
                        }
                      }}
                    />
                  ) : null}
                  <div
                    className="equipment-img-placeholder"
                    style={{ display: getImageUrl(item) ? "none" : "flex" }}
                  >
                    <svg
                      width="80"
                      height="80"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.5"
                    >
                      <rect
                        x="3"
                        y="3"
                        width="18"
                        height="18"
                        rx="2"
                        ry="2"
                        stroke="#cbd5e0"
                      />
                      <circle cx="8.5" cy="8.5" r="1.5" fill="#cbd5e0" />
                      <path
                        d="M21 15l-5-5L5 21"
                        stroke="#cbd5e0"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                    <span
                      style={{
                        marginTop: "8px",
                        color: "#9ca3af",
                        fontSize: "0.875rem",
                      }}
                    >
                      Không có ảnh
                    </span>
                  </div>
                </div>
                <div className="equipment-info">
                  <div className="equipment-header">
                    <span className="equipment-name">{item.name}</span>
                  </div>
                  <p className="equipment-card-desc">
                    Mã thiết bị: {item.code || item.instrumentCode}
                  </p>
                  <div
                    className={`equipment-category ${getStatusClass(
                      item.status || item.machineStatus
                    )}`}
                  >
                    <span className="equipment-status-text">
                      {getStatusLabel(item.status || item.machineStatus)}
                    </span>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
        {equipments.length > itemsPerPage && (
          <button
            className={`equipment-nav-btn equipment-nav-btn-right ${
              !canGoNext ? "disabled" : ""
            }`}
            onClick={handleNext}
            disabled={!canGoNext}
            aria-label="Thiết bị tiếp theo"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M9 18l6-6-6-6" />
            </svg>
          </button>
        )}
      </div>
    </div>
  );
}
