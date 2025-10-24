import React, { useState } from "react";
import "./Payment.css";

export default function Payment({
  selectedItems,
  selectedDateTime,
  onBack,
  onProceed, // legacy prop
  onConfirmRequested, // new prop Booking passes to open modal
}) {
  console.log("Payment props:", { onConfirmRequested });
  const [form, setForm] = useState({
    fullName: "Tuấn Lê",
    email: "email@example.com",
    phone: "0123321132",
    paymentMethod: "credit",
    cardNumber: "",
    exp: "",
    cvv: "",
  });

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleProceed = (e) => {
    // ensure no form submit
    if (e && e.preventDefault) e.preventDefault();
    // prefer Booking's onConfirmRequested to show modal
    if (typeof onConfirmRequested === "function") {
      onConfirmRequested();
      return;
    }
    // fallback to legacy onProceed if provided
    if (typeof onProceed === "function") onProceed();
  };

  return (
    <div className="payment-container">
      <h2 className="payment-title">Thanh toán</h2>
      <p className="payment-sub">
        Hoàn tất thông tin và thanh toán để xác nhận lịch hẹn
      </p>

      <div className="payment-layout">
        {/* LEFT COLUMN */}
        <div className="payment-left">
          {/* Thông tin cá nhân */}
          <div className="card info-card">
            <h3>Thông tin cá nhân</h3>
            <div className="form-grid">
              <label>
                Họ và tên<span>*</span>
                <input
                  name="fullName"
                  value={form.fullName}
                  onChange={handleChange}
                />
              </label>
              <div className="info-card-1">
                <label>
                  Email<span>*</span>
                  <input
                    type="email"
                    name="email"
                    value={form.email}
                    onChange={handleChange}
                  />
                </label>
                <label>
                  Số điện thoại<span>*</span>
                  <input
                    name="phone"
                    value={form.phone}
                    onChange={handleChange}
                  />
                </label>
              </div>
            </div>
          </div>

          {/* Phương thức thanh toán */}
          <div className="card method-card">
            <h3>Phương thức thanh toán</h3>
            <div className="payment-methods">
              <label className="method-item">
                <input
                  type="radio"
                  name="paymentMethod"
                  value="credit"
                  checked={form.paymentMethod === "credit"}
                  onChange={handleChange}
                />
                <img src="src\assets\icon\ATM.svg" />
                <span>Thẻ tín dụng/ghi nợ</span>
              </label>

              <label className="method-item">
                <input
                  type="radio"
                  name="paymentMethod"
                  value="momo"
                  checked={form.paymentMethod === "momo"}
                  onChange={handleChange}
                />
                <img src="src\assets\icon\Momo.svg" />
                <span>Ví MoMo</span>
              </label>

              <label className="method-item">
                <input
                  type="radio"
                  name="paymentMethod"
                  value="bank"
                  checked={form.paymentMethod === "bank"}
                  onChange={handleChange}
                />
                <img src="src\assets\icon\bank.svg" />
                <span>Chuyển khoản ngân hàng</span>
              </label>
            </div>

            {/* Thông tin thẻ */}
            {form.paymentMethod === "credit" && (
              <div className="card-inputs">
                <label>
                  Số thẻ<span>*</span>
                  <input
                    name="cardNumber"
                    placeholder="1234 5678 9012 3456"
                    value={form.cardNumber}
                    onChange={handleChange}
                  />
                </label>
                <div className="small-row">
                  <label>
                    Ngày hết hạn<span>*</span>
                    <input
                      name="exp"
                      placeholder="MM/YY"
                      value={form.exp}
                      onChange={handleChange}
                    />
                  </label>
                  <label>
                    CVV<span>*</span>
                    <input
                      name="cvv"
                      placeholder="123"
                      value={form.cvv}
                      onChange={handleChange}
                    />
                  </label>
                </div>
              </div>
            )}
          </div>

          <button className="btn-back" onClick={onBack}>
            Quay lại
          </button>
        </div>

        {/* RIGHT COLUMN */}
        <div className="payment-right">
          <div className="card summary-card">
            <h3>Tóm tắt đơn hàng</h3>
            <div className="summary-line">
              <span>Gói xét nghiệm:</span>
              <strong>
                {selectedItems?.package?.name || "Xét nghiệm tổng quát"}
              </strong>
            </div>
            <div className="summary-line">
              <span>Địa điểm:</span>
              <strong>Phòng khám Xét nghiệm Y tế</strong>
            </div>
            <div className="summary-line">
              <span>Ngày:</span>
              <strong>
                {selectedDateTime?.date
                  ? new Date(selectedDateTime.date).toLocaleDateString(
                      "vi-VN",
                      {
                        weekday: "long",
                        day: "2-digit",
                        month: "2-digit",
                        year: "numeric",
                      }
                    )
                  : "Thứ 6, 17/10/2025"}
              </strong>
            </div>
            <div className="summary-line">
              <span>Giờ:</span>
              <strong>{selectedDateTime?.time || "08:00"}</strong>
            </div>

            <hr />

            <div className="summary-line">
              <span>Tạm tính:</span>
              <span>
                {(selectedItems?.total || 300000).toLocaleString("vi-VN")}đ
              </span>
            </div>
            <div className="summary-line">
              <span>Phí dịch vụ:</span>
              <span>0đ</span>
            </div>

            <div className="summary-total">
              <strong>Tổng cộng:</strong>
              <strong className="price-total">
                {(selectedItems?.total || 300000).toLocaleString("vi-VN")}đ
              </strong>
            </div>

            {/* ensure button is explicit non-submit and identifiable by fallback listener */}
            <button
              type="button"
              className="btn-confirm"
              data-action="confirm-payment"
              onClick={handleProceed}
            >
              Xác nhận thanh toán
            </button>

            <p className="secure-note">
              Thông tin của bạn được bảo mật và mã hóa theo tiêu chuẩn quốc tế
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
