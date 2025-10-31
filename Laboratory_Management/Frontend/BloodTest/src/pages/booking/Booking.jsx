import React, { useState, useEffect } from "react";
import { isAuthenticated } from "../../utils/auth"; // <-- login check util (returns boolean)
import BookingNavbar from "../../components/booking/BookingNavbar";
import BookingHeader from "../../components/booking/BookingHeader";
import PackageSelection from "../../components/booking/PackageSelection";
import CatalogSelection from "../../components/booking/CatalogSelection";
import DateTimeSelection from "../../components/booking/DateTimeSelection";
import AcceptInfo from "../../components/booking/AcceptInfo";
import LoginRequirement from "../../components/booking/LoginRequirement";
import Payment from "../../components/booking/Payment";
import Success from "../../components/booking/SuccessBooking";
import "./Booking.css";

function Booking() {
  const [selectedPackage, setSelectedPackage] = useState(null);
  const [selectedItems, setSelectedItems] = useState(null); // payload from Package/Catalog continue
  const [selectedDateTime, setSelectedDateTime] = useState(null); // { date, time }
  const [currentStep, setCurrentStep] = useState(1);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [packageMode, setPackageMode] = useState("preset");
  // modal state for payment confirmation warning
  const [showWarningModal, setShowWarningModal] = useState(false);
  // store payment result to show in Success
  const [paymentResult, setPaymentResult] = useState(null);

  // Reset booking to initial state (bắt đầu lại bước 1)
  const handleNewBooking = () => {
    setSelectedPackage(null);
    setSelectedItems(null);
    setSelectedDateTime(null);
    setPaymentResult(null);
    setPackageMode("preset");
    setCurrentStep(1);
    // nếu cần, cuộn lên top hoặc focus UI ở đây
  };

  // Kiểm tra trạng thái đăng nhập
  useEffect(() => {
    const checkLoginStatus = () => {
      // CHECKPOINT: đây là nơi gọi isAuthenticated() để update trạng thái isLoggedIn
      // Nếu muốn debug, console.log(isAuthenticated()) tại đây.
      const loggedIn = isAuthenticated();
      setIsLoggedIn(loggedIn);
    };

    checkLoginStatus();

    // Lắng nghe sự thay đổi trong localStorage
    window.addEventListener("storage", checkLoginStatus);
    return () => window.removeEventListener("storage", checkLoginStatus);
  }, []);

  const handlePackageSelect = (packageId) => {
    setSelectedPackage(packageId);
  };

  // Modified: handleContinue nhận payload từ PackageSelection hoặc CatalogSelection
  const handleContinueFromSelection = (payload) => {
    // payload: { source: 'package'|'catalog', package: {...} } OR { source: 'catalog', items: [...] }
    setSelectedItems(payload);
    // chuyển sang chọn ngày & giờ
    setCurrentStep(2);
  };

  // DateTimeSelection tiếp tục -> chuyển sang AcceptInfo
  const handleContinueFromDateTime = (dateTime) => {
    setSelectedDateTime(dateTime);
    setCurrentStep(3);
  };

  // AcceptInfo tiếp tục (chỉ chuyển nội bộ sang Payment step)
  const handleProceedPayment = () => {
    setCurrentStep(4);
    // Payment step will show and Payment should trigger the warning modal
  };

  // Payment hoàn tất -> chuyển sang Success
  const handlePaymentFinish = (result) => {
    // store result then go to success step
    setPaymentResult(result || { status: "success", time: Date.now() });
    setCurrentStep(5);
  };

  // back handlers
  const backToSelection = () => setCurrentStep(1);
  const backToDateTime = () => setCurrentStep(2);
  const backFromPayment = () => setCurrentStep(3);

  // Called by Payment component when user presses "Xác nhận thanh toán"
  const handleRequestConfirm = () => {
    setShowWarningModal(true);
  };

  // Fallback: if Payment doesn't call prop, detect clicks on .btn-confirm inside payment container
  useEffect(() => {
    const clickHandler = (e) => {
      if (currentStep !== 4) return;
      // detect by class OR data-action attribute
      const btn =
        e.target.closest(".btn-confirm") ||
        e.target.closest('[data-action="confirm-payment"]');
      const inPayment = e.target.closest(".payment-container");
      if (btn && inPayment && !btn.disabled) {
        e.preventDefault();
        setShowWarningModal(true);
      }
    };
    document.addEventListener("click", clickHandler);
    return () => document.removeEventListener("click", clickHandler);
  }, [currentStep]);

  const handleModalCancel = () => setShowWarningModal(false);
  const handleModalConfirm = () => {
    setShowWarningModal(false);
    // simulate/perform payment here (or call API) then mark success
    const result = { status: "success", confirmedAt: Date.now() };
    // store and go to success
    handlePaymentFinish(result);
  };

  // Nếu chưa đăng nhập, hiển thị yêu cầu đăng nhập
  if (!isLoggedIn) {
    // EARLY RETURN: khi isLoggedIn === false, component sẽ return LoginRequirement và không render phần booking
    return (
      <div className="booking-container">
        <BookingNavbar />
        <LoginRequirement />
      </div>
    );
  }
  const steps = [
    { id: 1, label: "Chọn xét nghiệm", name: "Chọn xét nghiệm" },
    { id: 2, label: "Chọn giờ", name: "Chọn giờ" },
    { id: 3, label: "Xác nhận", name: "Xác nhận" },
    { id: 4, label: "Thanh toán", name: "Thanh toán" },
    { id: 5, label: "Thành công", name: "Thành công" },
  ];
  // Nếu đã đăng nhập, hiển thị trang đặt lịch
  return (
    <div className="booking-container">
      {currentStep !== 5}
      <BookingNavbar />
      <div className="booking-content">
        {currentStep !== 5 && (
          <BookingHeader
            title="Đặt lịch xét nghiệm"
            subtitle="Chọn gói xét nghiệm, địa điểm và thời gian phù hợp với bạn"
            currentStep={currentStep}
            steps={steps}
          />
        )}

        {/* Render nội bộ không reload trang theo currentStep */}
        {currentStep === 1 && (
          <>
            {packageMode === "preset" ? (
              <PackageSelection
                selectedPackage={selectedPackage}
                onPackageSelect={handlePackageSelect}
                onContinue={(payload) => handleContinueFromSelection(payload)}
                setPackageMode={setPackageMode}
                mode={packageMode}
              />
            ) : (
              <CatalogSelection
                setPackageMode={setPackageMode}
                onContinue={(payload) => handleContinueFromSelection(payload)}
              />
            )}
          </>
        )}

        {currentStep === 2 && (
          <DateTimeSelection
            onBack={backToSelection}
            onContinue={(dt) => handleContinueFromDateTime(dt)}
          />
        )}

        {currentStep === 3 && (
          <AcceptInfo
            selectedItems={selectedItems}
            selectedDateTime={selectedDateTime}
            onBack={backToDateTime}
            onProceed={handleProceedPayment}
          />
        )}
        {currentStep === 4 && (
          <Payment
            selectedItems={selectedItems}
            selectedDateTime={selectedDateTime}
            onBack={backFromPayment}
            onFinish={(result) => handlePaymentFinish(result)}
            // Payment should call this prop when user clicks its confirm button
            onConfirmRequested={handleRequestConfirm}
          />
        )}
        {currentStep === 5 && (
          <Success
            paymentResult={paymentResult}
            selectedItems={selectedItems}
            selectedDateTime={selectedDateTime}
            onNewBooking={handleNewBooking}
          />
        )}

        {/* Modal warning / confirmation rendered at Booking level */}
        {showWarningModal && (
          <div className="modal-overlay" role="dialog" aria-modal="true">
            <div className="modal-dialog">
              <div className="modal-title">Chính sách hoàn tiền</div>
              <div className="modal-body">
                <p>
                  <strong>Lưu ý quan trọng:</strong>
                </p>
                <ul>
                  <li>
                    Thanh toán sẽ <strong>không được hoàn</strong> nếu hủy trong
                    vòng 24 giờ trước lịch hẹn.
                  </li>
                  <li>
                    Nếu lịch hẹn vào thứ 7 hoặc chủ nhật, sẽ không được hoàn
                    tiền khi hủy.
                  </li>
                </ul>
                <p>Bạn có chắc chắn muốn tiếp tục thanh toán?</p>
              </div>
              <div className="modal-actions">
                <button
                  className="btn-modal-cancel"
                  onClick={handleModalCancel}
                >
                  Hủy
                </button>
                <button
                  className="btn-modal-confirm"
                  onClick={handleModalConfirm}
                >
                  Xác nhận
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default Booking;
