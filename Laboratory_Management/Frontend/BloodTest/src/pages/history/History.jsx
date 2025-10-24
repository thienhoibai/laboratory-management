import React from "react";
import BookingNavbar from "../../components/booking/BookingNavbar";
import BookingHistory from "../../components/historyBooking/BookingHistory";
import "./History.css";

function History() {
  return (
    <div className="history-container">
      <BookingNavbar />
      <div className="history-content">
        <BookingHistory />
      </div>
    </div>
  );
}

export default History;
