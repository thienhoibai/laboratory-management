import React, { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import SuccessBooking from "../../components/booking/SuccessBooking";
import Navbar from "../../components/navbar/Navbar";
import { bookingService } from "../../services/TestOrderService.jsx";
import { toast } from "react-toastify";

export default function SuccessBookingPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [bookingData, setBookingData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBookingData = async () => {
      // Lấy bookingId từ URL params
      const bookingId = searchParams.get("bookingId");

      if (!bookingId) {
        toast.error("Không tìm thấy mã đơn hàng");
        navigate("/booking");
        return;
      }

      try {
        const booking = (await bookingService.getBookingById(bookingId)).data;
        // console.log(booking.testCatalogs);
        let testInfo = null;
        if (booking.bundleId) {
          testInfo = await bookingService.getTestBundle(booking.bundleId);
        } else if (booking.testCatalogs && booking.testCatalogs.length > 0) {
          const catalogPromises = booking.testCatalogs.map((catalogId) =>
            bookingService.getTestCatalog(catalogId)
          );
          testInfo = await Promise.all(catalogPromises);
        }

        setBookingData({
          ...booking,
          testInfo,
        });
      } catch (error) {
        toast.error(error || "Không thể tải thông tin đơn hàng");
        navigate("/booking");
      } finally {
        setLoading(false);
      }
    };

    fetchBookingData();
  }, [searchParams, navigate]);

  const handleNewBooking = () => {
    navigate("/booking");
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="spinner"></div>
        <p>Đang tải thông tin đơn hàng...</p>
      </div>
    );
  }

  if (!bookingData) {
    return null;
  }

  // Format dữ liệu để truyền vào SuccessBooking component
  const selectedItems = {
    total: 0, // Có thể tính từ testInfo nếu API trả về price
    source: bookingData.bundleId ? "package" : "catalog",
    package: bookingData.bundleId
      ? {
          name: bookingData.testInfo?.bundleName,
          title: bookingData.testInfo?.bundleName,
        }
      : null,
    items: !bookingData.bundleId
      ? bookingData.testInfo?.map((test) => ({
          testName: test.testName,
        }))
      : null,
  };

  const selectedDateTime = {
    date: bookingData.slotInfo?.appointmentDate,
    time: bookingData.slotInfo?.timeBlock,
  };

  const paymentResult = {
    orderCode: bookingData.bookingCode,
    amount: 0, // Có thể tính từ testInfo
  };

  return (
    <div className="booking-container">
      <Navbar />
      <SuccessBooking
        paymentResult={paymentResult}
        selectedItems={selectedItems}
        selectedDateTime={selectedDateTime}
        onNewBooking={handleNewBooking}
        bookingData={bookingData}
      />
    </div>
  );
}
