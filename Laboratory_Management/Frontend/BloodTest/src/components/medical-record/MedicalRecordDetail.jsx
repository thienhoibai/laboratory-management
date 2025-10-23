import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./MedicalRecordDetail.css";
import TestResultDetail from "./TestResultDetail";

export default function MedicalRecordDetail() {
  const navigate = useNavigate();
  const [expandedTests, setExpandedTests] = useState({});

  // Mock data
  const patientInfo = {
    recordId: "MR001",
    fullname: "Nguyễn Văn An",
    patientId: "550e8400",
    gender: "Nam",
    birthday: "5 tháng 3, 1985 (40 tuổi)",
    phone: "0912345678",
    email: "nguyenvanan@email.com",
    address: "123 Đường Lê Lợi, Quận 1, TP.HCM",
    idCard: "079085001234",
  };

  const appointmentHistory = [
    {
      id: 1,
      title: "Xét nghiệm máu",
      date: "20 tháng 9, 2024",
      location: "Phòng khám Vinmec, 123 Lê Văn Việt, Tân Phú",
      status: "completed",
    },
    {
      id: 2,
      title: "Gói xét nghiệm tổng quát",
      date: "10 tháng 8, 2024",
      location: "Phòng khám Vinmec, 123 Lê Văn Việt, Tân Phú",
      status: "completed",
    },
    {
      id: 3,
      title: "Xét nghiệm tim mạch",
      date: "25 tháng 6, 2024",
      location: "Phòng khám Vinmec, 123 Lê Văn Việt, Tân Phú",
      status: "completed",
    },
    {
      id: 4,
      title: "Xét nghiệm nước tiểu",
      date: "17 tháng 5, 2024",
      location: "Phòng khám Vinmec, 123 Lê Văn Việt, Tân Phú",
      status: "completed",
    },
    {
      id: 5,
      title: "Xét nghiệm chức năng gan",
      date: "12 tháng 3, 2024",
      location: "Phòng khám Vinmec, 123 Lê Văn Việt, Tân Phú",
      status: "completed",
    },
  ];

  const toggleTestDetail = (testId) => {
    setExpandedTests((prev) => ({
      ...prev,
      [testId]: !prev[testId],
    }));
  };

  return (
    <div className="medical-record-detail">
      {/* Header */}
      <div className="medical-record-header">
        <div className="breadcrumb">
          <button
            onClick={() => navigate("/profile")}
            className="breadcrumb-link"
          >
            Quay lại hồ sơ cá nhân
          </button>
        </div>
        <h1 className="page-title">Chi tiết hồ sơ bệnh án</h1>
        <p className="page-subtitle">Hồ sơ bệnh án #{patientInfo.recordId}</p>
      </div>

      {/* Patient Info Section */}
      <div className="patient-info-section">
        <h2 className="section-title">Thông tin bệnh nhân</h2>
        <p className="section-subtitle">Thông tin chi tiết của bệnh nhân</p>

        <div className="patient-info-grid">
          <div className="patient-info-item">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                <circle cx="12" cy="7" r="4" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Họ và tên</span>
              <span className="info-value">{patientInfo.fullname}</span>
            </div>
          </div>

          <div className="patient-info-item">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                <line x1="16" y1="2" x2="16" y2="6" />
                <line x1="8" y1="2" x2="8" y2="6" />
                <line x1="3" y1="10" x2="21" y2="10" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Ngày sinh</span>
              <span className="info-value">{patientInfo.birthday}</span>
            </div>
          </div>

          <div className="patient-info-item">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <circle cx="12" cy="12" r="10" />
                <path d="M12 6v6l4 2" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Mã bệnh nhân</span>
              <span className="info-value">{patientInfo.patientId}</span>
            </div>
          </div>

          <div className="patient-info-item">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Giới tính</span>
              <span className="info-value">{patientInfo.gender}</span>
            </div>
          </div>

          <div className="patient-info-item">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Số điện thoại</span>
              <span className="info-value">{patientInfo.phone}</span>
            </div>
          </div>

          <div className="patient-info-item">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                <polyline points="22,6 12,13 2,6" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Email</span>
              <span className="info-value">{patientInfo.email}</span>
            </div>
          </div>

          <div className="patient-info-item full-width">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                <circle cx="12" cy="10" r="3" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Địa chỉ</span>
              <span className="info-value">{patientInfo.address}</span>
            </div>
          </div>

          <div className="patient-info-item full-width">
            <div className="info-icon-wrapper">
              <svg
                className="info-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <rect x="2" y="3" width="20" height="14" rx="2" ry="2" />
                <line x1="8" y1="21" x2="16" y2="21" />
                <line x1="12" y1="17" x2="12" y2="21" />
              </svg>
            </div>
            <div className="info-content">
              <span className="info-label">Số CMND/CCCD</span>
              <span className="info-value">{patientInfo.idCard}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Appointment History Section */}
      <div className="appointment-history-section">
        <h2 className="section-title">Lịch sử xét nghiệm</h2>
        <p className="section-subtitle">
          Các xét nghiệm đã thực hiện trong quá khứ (chỉ xem)
        </p>

        <div className="filter-bar">
          <div className="filter-item">
            <svg
              className="filter-icon"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
            </svg>
            <span>Bộ lọc</span>
            <span className="filter-note">
              Lọc xét nghiệm theo ngày trước ngày tháng năm
            </span>
          </div>
          <div className="filter-dates">
            <div className="date-picker">
              <label>Từ ngày</label>
              <input type="text" placeholder="mm/dd/yyyy" />
            </div>
            <div className="date-picker">
              <label>Đến ngày</label>
              <input type="text" placeholder="mm/dd/yyyy" />
            </div>
            <button className="filter-reset">
              <span>Tất cả</span>
            </button>
          </div>
        </div>

        <div className="appointment-list">
          {appointmentHistory.map((appointment) => (
            <div key={appointment.id} className="appointment-card-wrapper">
              <div className="appointment-card">
                <div className="appointment-icon">
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                    <polyline points="14,2 14,8 20,8" />
                    <line x1="16" y1="13" x2="8" y2="13" />
                    <line x1="16" y1="17" x2="8" y2="17" />
                    <polyline points="10,9 9,9 8,9" />
                  </svg>
                </div>
                <div className="appointment-content">
                  <h3 className="appointment-title">{appointment.title}</h3>
                  <div className="appointment-info">
                    <div className="appointment-date">
                      <svg
                        className="date-icon"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <rect
                          x="3"
                          y="4"
                          width="18"
                          height="18"
                          rx="2"
                          ry="2"
                        />
                        <line x1="16" y1="2" x2="16" y2="6" />
                        <line x1="8" y1="2" x2="8" y2="6" />
                        <line x1="3" y1="10" x2="21" y2="10" />
                      </svg>
                      <span>Ngày xét nghiệm: {appointment.date}</span>
                    </div>
                    <div className="appointment-location">
                      <span>Khám tại: {appointment.location}</span>
                    </div>
                  </div>
                </div>
                <div className="appointment-actions">
                  <button
                    className={`view-detail-btn ${
                      expandedTests[appointment.id] ? "active" : ""
                    }`}
                    onClick={() => toggleTestDetail(appointment.id)}
                  >
                    {expandedTests[appointment.id] ? "Ẩn" : "Xem"} chi tiết kết
                    quả
                    <svg
                      className={`chevron-icon ${
                        expandedTests[appointment.id] ? "expanded" : ""
                      }`}
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <polyline points="6 9 12 15 18 9" />
                    </svg>
                  </button>
                </div>
              </div>
              {expandedTests[appointment.id] && (
                <div className="test-detail-dropdown">
                  <TestResultDetail test={appointment} inline={true} />
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
