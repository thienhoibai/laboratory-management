import React from "react";
import "./AcceptInfo.css";
import { CiCalendar } from "react-icons/ci";
import { IoMdTime } from "react-icons/io";
import { HiOutlineLocationMarker } from "react-icons/hi";
import { catalog } from "../../data/catalog"; // <-- import catalog chung

function AcceptInfo({ selectedItems, selectedDateTime, onBack, onProceed }) {
  // selectedItems: { source:'package', package: {...}, total } OR { source:'catalog', items:[{name,price}], total }

  const parsePrice = (priceStr) =>
    Number(String(priceStr).replace(/[^\d]/g, "")) || 0;

  let headerTitle = "Xét nghiệm đã chọn";
  let itemList = [];
  let total = 0;

  if (!selectedItems) {
    itemList = [];
    total = 0;
  } else if (selectedItems.source === "package") {
    const pkg = selectedItems.package || null;
    headerTitle = pkg ? pkg.title : headerTitle;

    // nếu pkg.includes là mảng id (số) -> map từ catalog để lấy name + price
    if (pkg && Array.isArray(pkg.includes)) {
      const first = pkg.includes[0];
      if (typeof first === "number") {
        itemList = pkg.includes
          .map((id) => {
            const c = catalog.find((it) => it.id === id);
            if (!c) return { name: String(id), price: null };
            return { name: c.name, price: c.price || null };
          })
          .filter(Boolean);
      } else {
        // includes là tên chuỗi
        itemList = pkg.includes.map((name) => ({ name, price: null }));
      }
    } else {
      itemList = [];
    }

    // tổng ưu tiên dùng selectedItems.total, fallback tính từ catalog nếu có id, else parse chuỗi price gói
    if (
      typeof selectedItems.total === "number" &&
      !Number.isNaN(selectedItems.total)
    ) {
      total = selectedItems.total;
    } else {
      // try sum catalog prices when includes are ids
      if (
        pkg &&
        Array.isArray(pkg.includes) &&
        typeof pkg.includes[0] === "number"
      ) {
        total = pkg.includes.reduce((s, id) => {
          const c = catalog.find((it) => it.id === id);
          return s + (c && typeof c.price === "number" ? c.price : 0);
        }, 0);
      } else {
        total = selectedItems.total || parsePrice(pkg?.price || "0");
      }
    }
  } else if (selectedItems.source === "catalog") {
    itemList = selectedItems.items || [];
    total =
      selectedItems.total || itemList.reduce((s, it) => s + (it.price || 0), 0);
  }

  const formattedTotal = total ? total.toLocaleString("vi-VN") + "₫" : "0₫";
  const formatDateLabel = (isoDate) => {
    if (!isoDate) return "";
    try {
      const d = new Date(isoDate);
      return d.toLocaleDateString("vi-VN", {
        weekday: "long",
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
    } catch {
      return isoDate;
    }
  };

  return (
    <div className="accept-info">
      <h2>Xác nhận thông tin</h2>
      <p className="accept-sub">
        Vui lòng kiểm tra lại thông tin trước khi thanh toán
      </p>

      <div className="card">
        <div className="card-title">Xét Nghiệm Đã Chọn</div>
        <div className="card-title-header">{headerTitle}</div>
        <ul className="selected-list">
          {itemList.map((it, idx) => (
            <li key={idx} className="selected-item">
              <div className="item-left">
                <img src="src/assets/icon/SVG_margin.svg" />
                <span className="item-name">{it.name}</span>
              </div>
              <div className="item-price">
                {it.price ? it.price.toLocaleString("vi-VN") + "₫" : ""}
              </div>
            </li>
          ))}
        </ul>
      </div>

      <div className="card">
        <div className="card-title">Địa điểm và thời gian</div>
        <div className="location">
          <div className="location-main">
            <HiOutlineLocationMarker className="loc-icon" />
            <div>
              <div className="loc-title">Phòng khám Xét nghiệm Y tế</div>
              <div className="location-sub">123 Nguyễn Huệ, Quận 1, TP.HCM</div>
            </div>
          </div>
          <div className="location-time">
            {selectedDateTime ? (
              <>
                <div className="time-row">
                  <CiCalendar className="time-icon" />
                  <div className="loc-title">Ngày khám</div>
                </div>
                <div className="value">
                  {" "}
                  {formatDateLabel(selectedDateTime.date)}
                </div>

                <div className="time-row">
                  <IoMdTime className="time-icon" />
                  <div className="loc-title">Giờ khám</div>
                </div>
                <div className="value">Giờ: {selectedDateTime.time}</div>
              </>
            ) : (
              <div className="no-dt">Chưa chọn ngày giờ</div>
            )}
          </div>
        </div>
      </div>

      <div className="card total-card">
        <div>
          <div className="total-label">Tổng chi phí</div>
          <div className="total-sub">Tạm tính</div>
        </div>
        <div className="total-amount">{formattedTotal}</div>
      </div>

      <div className="note-card">
        <div className="note-title">Lưu ý quan trọng</div>
        <ul>
          <li>Vui lòng đến trước giờ hẹn 15 phút để làm thủ tục.</li>
          <li>
            Nhịn ăn 8-12 tiếng trước khi xét nghiệm (nếu cần theo hướng dẫn).
          </li>
          <li>Mang theo CMND/CCCD khi đến làm thủ thuật.</li>
        </ul>
      </div>

      <div className="accept-actions">
        <button className="btn-back" onClick={() => onBack && onBack()}>
          Quay lại
        </button>
        <button
          className="btn-proceed"
          onClick={() => onProceed && onProceed()}
        >
          Tiếp tục thanh toán
        </button>
      </div>
    </div>
  );
}

export default AcceptInfo;
