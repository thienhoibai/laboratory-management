import React from "react";
import "./BookingHeader.css";

function BookingHeader({ title, subtitle, currentStep }) {
  const steps = [
    { id: 1, label: "Chọn gói", name: "Chọn gói" },
    { id: 2, label: "Chọn giờ", name: "Chọn giờ" },
    { id: 3, label: "Xác nhận", name: "Xác nhận" },
    { id: 4, label: "Thanh toán", name: "Thanh toán" },
    { id: 5, label: "Thông tin", name: "Thông tin" },
  ];

  return (
    <div className="booking-header">
      <div className="booking-title-section">
        <h1 className="booking-title">{title}</h1>
        <p className="booking-subtitle">{subtitle}</p>
      </div>

      <div className="booking-steps">
        {steps.map((step, index) => (
          <React.Fragment key={step.id}>
            <div className={`step ${currentStep >= step.id ? "active" : ""}`}>
              <div className="step-circle">
                <span className="step-number">{step.id}</span>
              </div>
              <span className="step-label">{step.name}</span>
            </div>
            {index < steps.length - 1 && (
              <div className="step-arrow">
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                  <path
                    d="M6 12L10 8L6 4"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </div>
            )}
          </React.Fragment>
        ))}
      </div>
    </div>
  );
}

export default BookingHeader;
