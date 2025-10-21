import React from "react";
import BookingNavbar from "../../components/booking/BookingNavbar";
import "./Results.css";

function Results() {
  return (
    <div className="results-container">
      <BookingNavbar />
      <div className="results-content">
        <div className="results-header">
          <h1 className="results-title">Kết quả xét nghiệm</h1>
          <p className="results-subtitle">
            Xem và tải xuống kết quả xét nghiệm của bạn
          </p>
        </div>

        <div className="results-placeholder">
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
                d="M32 20C36.4 20 40 23.6 40 28V32H44C45.1 32 46 32.9 46 34V50C46 51.1 45.1 52 44 52H20C18.9 52 18 51.1 18 50V34C18 32.9 18.9 32 20 32H24V28C24 23.6 27.6 20 32 20Z"
                stroke="#2563eb"
                strokeWidth="2"
                fill="none"
              />
              <path
                d="M32 40V44"
                stroke="#2563eb"
                strokeWidth="2"
                strokeLinecap="round"
              />
              <circle cx="32" cy="46" r="2" fill="#2563eb" />
            </svg>
          </div>
          <h2 className="placeholder-title">Trang đang được phát triển</h2>
          <p className="placeholder-description">
            Trang kết quả xét nghiệm sẽ sớm được hoàn thiện. Bạn sẽ có thể xem
            và tải xuống kết quả xét nghiệm tại đây.
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
              <span>Xem kết quả chi tiết</span>
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
              <span>Tải xuống PDF</span>
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
              <span>Chia sẻ với bác sĩ</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Results;
