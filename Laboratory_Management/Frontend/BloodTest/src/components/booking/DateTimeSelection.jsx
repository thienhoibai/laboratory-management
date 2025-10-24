import React, { useState, useMemo } from "react";
import "./DateTimeSelection.css";
import { IoMdTime } from "react-icons/io";
import { CiCalendar } from "react-icons/ci";

function DateTimeSelection({ onBack, onContinue }) {
  // tạo danh sách 30 ngày bắt đầu từ hôm nay
  const days = useMemo(() => {
    const arr = [];
    const today = new Date();
    for (let i = 1; i < 31; i++) {
      const d = new Date(today);
      d.setDate(today.getDate() + i);
      // set giờ về giữa trưa để tránh vấn đề timezone khi parse ISO trên client khác múi giờ
      d.setHours(12, 0, 0, 0);
      arr.push(d);
    }
    return arr;
  }, []);

  const morningSlots = [
    "07:00",
    "07:30",
    "08:00",
    "08:30",
    "09:00",
    "09:30",
    "10:00",
    "10:30",
    "11:00",
  ];
  const afternoonSlots = [
    "13:00",
    "13:30",
    "14:00",
    "14:30",
    "15:00",
    "15:30",
    "16:00",
    "16:30",
    "17:00",
  ];

  const [selectedDate, setSelectedDate] = useState(null);
  const [selectedTime, setSelectedTime] = useState(null);

  const formatDayLabel = (d) => {
    const opts = { weekday: "short", day: "numeric", month: "numeric" };
    return d.toLocaleDateString("vi-VN", opts);
  };

  return (
    <div className="dt-selection">
      <div className="dt-location-card">
        <div>Phòng khám Xét nghiệm Y tế</div>
        <div className="dt-location-sub">123 Nguyễn Huệ, Quận 1, TP.HCM</div>
      </div>
      <h4 className="dt-sub ">
        <CiCalendar style={{ color: "#2563eb", fontWeight: "bolder" }} />
        <span className="dt-title">Chọn ngày (trong vòng 30 ngày)</span>
      </h4>
      <div className="dt-day-grid">
        {days.map((d) => {
          const key = d.toISOString(); // ISO with time (no timezone shift issues)
          const isSelected = selectedDate === key;
          return (
            <button
              key={key}
              className={`dt-day ${isSelected ? "selected" : ""}`}
              onClick={() => {
                setSelectedDate(key);
                setSelectedTime(null);
              }}
              type="button"
            >
              {formatDayLabel(d)}
            </button>
          );
        })}
      </div>
      <h4 className="dt-sub ">
        <IoMdTime style={{ color: "#2563eb", fontWeight: "bolder" }} />
        <span className="dt-title">Chọn giờ</span>
      </h4>
      <div className="dt-slots">
        <h4>Ca sáng (07:00 - 11:00)</h4>
        <div className="dt-slot-row">
          {morningSlots.map((t) => (
            <button
              key={t}
              className={`dt-slot ${selectedTime === t ? "selected" : ""}`}
              onClick={() => setSelectedTime(t)}
              disabled={!selectedDate}
              type="button"
            >
              {t}
            </button>
          ))}
        </div>

        <h4>Ca chiều (13:00 - 17:00)</h4>
        <div className="dt-slot-row">
          {afternoonSlots.map((t) => (
            <button
              key={t}
              className={`dt-slot ${selectedTime === t ? "selected" : ""}`}
              onClick={() => setSelectedTime(t)}
              disabled={!selectedDate}
              type="button"
            >
              {t}
            </button>
          ))}
        </div>
      </div>

      <div className="dt-actions">
        <button className="dt-back" onClick={() => onBack && onBack()}>
          Quay lại
        </button>
        <button
          className={`dt-continue ${
            selectedDate && selectedTime ? "enabled" : ""
          }`}
          disabled={!selectedDate || !selectedTime}
          onClick={() =>
            onContinue &&
            onContinue({
              // pass full ISO date (with time set midday) to avoid timezone shift later
              date: selectedDate,
              time: selectedTime,
            })
          }
        >
          Tiếp tục
        </button>
      </div>
    </div>
  );
}

export default DateTimeSelection;
