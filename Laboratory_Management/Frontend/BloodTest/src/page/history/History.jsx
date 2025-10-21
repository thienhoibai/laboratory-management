import React from "react";
import BookingNavbar from "../../components/booking/BookingNavbar";
import "./History.css";

function History() {
  return (
    <div className="history-container">
      <BookingNavbar />
      <div className="history-content">
        <div className="history-header">
          <h1 className="history-title">Lịch sử</h1>
          <p className="history-subtitle">
            Xem lịch sử giao dịch và xét nghiệm của bạn
          </p>
        </div>

        <div className="history-placeholder">
          <div className="placeholder-icon">
            <svg width="64" height="64" viewBox="0 0 64 64" fill="none">
              <circle
                cx="32"
                cy="32"
                r="30"
                stroke="#2563eb"
                strokeWidth="2"
                fill="#f0f7ff"
              />
              <path
                d="M32 16V32L40 40"
                stroke="#2563eb"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </div>
          <h2 className="placeholder-title">Trang đang được phát triển</h2>
          <p className="placeholder-description">
            Trang lịch sử sẽ sớm được hoàn thiện. Bạn sẽ có thể xem lịch sử giao
            dịch và xét nghiệm tại đây.
          </p>
          <div className="placeholder-features">
            <div className="feature-item">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path
                  d="M16.5 6.5L7.5 15.5L3.5 11.5"
                  stroke="#10b981"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              <span>Lịch sử giao dịch</span>
            </div>
            <div className="feature-item">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path
                  d="M16.5 6.5L7.5 15.5L3.5 11.5"
                  stroke="#10b981"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              <span>Lịch sử xét nghiệm</span>
            </div>
            <div className="feature-item">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path
                  d="M16.5 6.5L7.5 15.5L3.5 11.5"
                  stroke="#10b981"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              <span>Thống kê chi tiết</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default History;
