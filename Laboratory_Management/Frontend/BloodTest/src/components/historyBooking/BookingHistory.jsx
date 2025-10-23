import React, { useState } from "react";
import "./BookingHistory.css";

const MOCK_BOOKINGS = [
  {
    id: "APT004",
    dateLabel: "Thứ Hai, 25 tháng 3, 2024",
    time: "14:00",
    createdAt: "09:45 22 tháng 3, 2024",
    status: "pending", // pending | confirmed | completed | cancelled
    price: 200000,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Xét nghiệm nước tiểu",
    resultReady: false,
  },
  {
    id: "APT002",
    dateLabel: "Thứ Tư, 20 tháng 3, 2024",
    time: "10:30",
    createdAt: "16:20 11 tháng 3, 2024",
    status: "confirmed",
    price: 200000,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Xét nghiệm tổng quát",
    resultReady: false,
  },
  {
    id: "APT001",
    dateLabel: "Chủ Nhật, 10 tháng 3, 2024",
    time: "09:00",
    createdAt: "14:30 10 tháng 3, 2024",
    status: "completed",
    price: 2500000,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Gói xét nghiệm tổng quát",
    resultReady: true,
  },
  {
    id: "APT005",
    dateLabel: "Thứ Bảy, 10 tháng 2, 2024",
    time: "11:00",
    createdAt: "13:30 8 tháng 2, 2024",
    status: "cancelled",
    price: 0,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Xét nghiệm nhanh",
    resultReady: false,
  },
];

export default function BookingHistory() {
  const [expanded, setExpanded] = useState({});

  const toggle = (id) => {
    setExpanded((s) => ({ ...s, [id]: !s[id] }));
  };

  const statusLabel = (status) => {
    switch (status) {
      case "pending":
        return { text: "Chờ xác nhận", className: "badge-yellow" };
      case "confirmed":
        return { text: "Đã xác nhận", className: "badge-blue" };
      case "completed":
        return { text: "Hoàn thành", className: "badge-green" };
      case "cancelled":
        return { text: "Đã hủy", className: "badge-red" };
      default:
        return { text: status, className: "" };
    }
  };

  return (
    <div className="booking-history-page">
      <div className="booking-history-header">
        <h1>Lịch sử đặt lịch</h1>
        <p>Xem tất cả các lịch hẹn đã đặt</p>
      </div>

      <div className="booking-filters">
        <div className="filters-title">
          <img src="src\assets\icon\Fillter.svg" alt="Filters" />
          <strong>Bộ lọc</strong>
        </div>
        <span style={{ color: "#737373", fontSize: "15px" }}>
          Lọc lịch hẹn theo ngày và trạng thái
        </span>
        <div className="filter-row">
          <div className="filter-item">
            <label htmlFor="from-date" className="filter-label">
              Từ ngày
            </label>
            <input type="date" id="from-date" className="filter-date-input" />
          </div>
          <div className="filter-item">
            <label htmlFor="to-date" className="filter-label">
              Đến ngày
            </label>
            <input type="date" id="to-date" className="filter-date-input" />
          </div>
          <div className="filter-item">
            <label htmlFor="status" className="filter-label">
              Trạng thái
            </label>
            <select id="status" className="filter-status-select">
              <option value="">Tất cả</option>
              <option value="pending">Chờ xác nhận</option>
              <option value="confirmed">Đã xác nhận</option>
              <option value="completed">Hoàn thành</option>
              <option value="cancelled">Hủy</option>
            </select>
          </div>
        </div>
      </div>

      <div className="booking-list">
        {MOCK_BOOKINGS.map((b) => {
          const s = statusLabel(b.status);
          const isExpanded = !!expanded[b.id];
          return (
            <div key={b.id} className={`booking-card booking-${b.status}`}>
              <div className="booking-card-header">
                <div className="booking-code">Mã đặt lịch: {b.id}</div>
                <div className="booking-main">
                  <img src="src\assets\icon\Calender.svg" alt="Calender" />
                  <div className="booking-date&time">
                    <div className="booking-date">{b.dateLabel}</div>
                    <div className="booking-time">Giờ hẹn: {b.time}</div>
                  </div>
                  <div className={`booking-badge ${s.className}`}>{s.text}</div>
                </div>
                <div className="booking-created">
                  Đặt lịch ngày: lúc {b.createdAt}
                </div>
                <div className="booking-actions">
                  <button
                    className="btn-dropdown"
                    onClick={() => toggle(b.id)}
                    aria-expanded={isExpanded}
                  >
                    {isExpanded ? "Ẩn chi tiết" : "Xem chi tiết"}
                  </button>
                </div>
              </div>

              {/* always render body, toggle classes for animation */}
              <div
                className={`booking-card-body ${
                  isExpanded ? "open" : "closed"
                }`}
                aria-hidden={!isExpanded}
              >
                {/* unified expanded grid: always show Personal / Details / Payment */}
                <div className="completed-layout expanded-grid">
                  <div className="left-col">
                    <div className="section">
                      <h4>Thông tin cá nhân</h4>
                      <div className="info-row">
                        <div className="info-row-1">
                          <img src="src\assets\icon\User.svg" alt="User" />
                          <span className="label">Họ và tên</span>
                        </div>
                        <span className="value">{b.patient.name}</span>
                      </div>
                      <div className="info-row">
                        <div className="info-row-1">
                          <img src="src\assets\icon\Mail.svg" alt="Email" />
                          <span className="label">Email</span>
                        </div>
                        <span className="value">{b.patient.email}</span>
                      </div>
                      <div className="info-row">
                        <div className="info-row-1">
                          <img src="src\assets\icon\Phone.svg" alt="Phone" />{" "}
                          <span className="label">Điện thoại</span>
                        </div>
                        <span className="value">{b.patient.phone}</span>
                      </div>
                    </div>
                  </div>

                  <div className="middle-col">
                    <div className="section">
                      <h4>Chi tiết đặt lịch</h4>
                      <div className="info-row">
                        <div className="info-row-1">
                          <img
                            src="src\assets\icon\Document_Gray.svg"
                            alt="Document"
                          />
                          <span className="label">Gói</span>
                        </div>
                        <span className="value">{b.service}</span>
                      </div>
                      {/* <div className="info-row">
                            <span className="label">Ngày hẹn</span>
                            <span className="value">
                              {b.dateLabel} — {b.time}
                            </span>
                          </div>
                          <div className="info-row small">
                            <span className="label">Đặt lúc</span>
                            <span className="value">{b.createdAt}</span>
                          </div> */}
                    </div>
                  </div>

                  <div className="right-col">
                    <div className="section payment">
                      <h4>Thông tin thanh toán</h4>
                      <div className="info-row">
                        <div className="info-row-1">
                          <img src="src\assets\icon\Pay.svg" alt="Pay" />
                          <span className="label">Hình thức</span>
                        </div>
                        <span className="value">Tiền mặt</span>
                      </div>
                      <div className="info-row">
                        <div className="info-row-1">
                          <img src="src\assets\icon\Pay.svg" alt="Pay" />
                          <span className="label">Tổng tiền</span>
                        </div>
                        <span className="value">
                          {b.price.toLocaleString("vi-VN")} ₫
                        </span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* result area: only this differs by status */}
                <h3>Kết quả xét nghiệm</h3>
                <div className="result-area" style={{ marginTop: 20 }}>
                  {b.status === "completed" && b.resultReady ? (
                    <div className="result-box ready">
                      <img
                        src="src\assets\icon\Document_Border.svg"
                        alt="Document_Borders"
                      />{" "}
                      <br />
                      <strong style={{ fontSize: "18px" }}>
                        Kết quả xét nghiệm đã sẵn sàng
                      </strong>
                      <p
                        style={{
                          fontSize: "13px",
                          marginTop: "10px",
                          color: "#737373",
                        }}
                      >
                        Có 27 chỉ số xét nghiệm trong gói này
                      </p>
                      <button className="btn-primary-history-booking">
                        <img src="src\assets\icon\Document_white.svg" alt="" />
                        Xem chi tiết kết quả xét nghiệm
                      </button>
                    </div>
                  ) : b.status === "cancelled" ? (
                    <div className="result-box cancelled">
                      <strong>Không có kết quả xét nghiệm</strong>
                      <p>Lịch hẹn đã bị hủy, không có kết quả.</p>
                    </div>
                  ) : (
                    <div
                      className={`result-box ${
                        b.status === "pending" || b.status === "confirmed"
                          ? "yellow"
                          : "normal"
                      }`}
                    >
                      <p>Chưa có kết quả xét nghiệm</p>
                      {(b.status === "pending" || b.status === "confirmed") && (
                        <small>
                          Kết quả sẽ được cập nhật sau khi hoàn tất lấy mẫu.
                        </small>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
