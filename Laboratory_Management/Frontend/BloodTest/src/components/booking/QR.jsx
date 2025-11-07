import React from "react";
import "./QR.css";

function QR({ selectedItems, selectedDateTime, onConfirmPaid, loading }) {
  return (
    <div className="qr-container">
      <div className="qr-card">
        <h2>Quét mã QR để thanh toán</h2>
        <div className="qr-image">
          {/* Giả lập mã QR */}
          <img
            src="https://api.qrserver.com/v1/create-qr-code/?size=220x220&data=ThanhToanXetNghiem"
            alt="QR Code"
          />
        </div>
        <div className="qr-info">
          <p>
            Vui lòng sử dụng ứng dụng ngân hàng hoặc ví điện tử để quét mã QR và
            hoàn tất thanh toán.
          </p>
        </div>
        <button
          className="btn-qr-confirm"
          onClick={onConfirmPaid}
          disabled={loading}
        >
          {loading ? (
            <span className="qr-loading">
              <span className="spinner" /> Đang xác nhận thanh toán...
            </span>
          ) : (
            "Xác nhận đã thanh toán"
          )}
        </button>
      </div>
    </div>
  );
}

export default QR;
