import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { Pagination, Spin } from "antd";
import "./BookingHistory.css";
import api from "../../configs/axios";
import { formatDate,formatTime } from "../../utils/formatDate";
import { useSearchParams } from "react-router-dom";
import { bookingService } from "../../services/bookingService";

const endPoint = "testorder/api/Booking/patient";
const endPoint1 = "testorder/api/TestBundle";
const MOCK_BOOKINGS = [
  {
    id: "APT004",
    dateLabel: "Thứ Hai, 25 tháng 3, 2024",
    time: "14:00",
    createdAt: "09:45 22 tháng 3, 2024",
    status: "pending", // pending | confirmed | completed | cancelled
    price: 200000,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Xét nghiệm nước tiểu",
    resultReady: false,
  },
  {
    id: "APT002",
    dateLabel: "Thứ Tư, 20 tháng 3, 2024",
    time: "10:30",
    createdAt: "16:20 11 tháng 3, 2024",
    status: "confirmed",
    price: 200000,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Xét nghiệm tổng quát",
    resultReady: false,
  },
  {
    id: "APT001",
    dateLabel: "Chủ Nhật, 10 tháng 3, 2024",
    time: "09:00",
    createdAt: "14:30 10 tháng 3, 2024",
    status: "completed",
    price: 2500000,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Gói xét nghiệm tổng quát",
    resultReady: true,
  },
  {
    id: "APT005",
    dateLabel: "Thứ Bảy, 10 tháng 2, 2024",
    time: "11:00",
    createdAt: "13:30 8 tháng 2, 2024",
    status: "cancelled",
    price: 0,
    patient: {
      name: "Nguyễn Văn An",
      email: "nguyenvanan@email.com",
      phone: "0912345678",
    },
    service: "Xét nghiệm nhanh",
    resultReady: false,
  },
];

export default function BookingHistory() {
  const [expanded, setExpanded] = useState({});
  const [BookingHistory, setBookingHistory] = useState([]);
  const [Package, setPackage] = useState([]);
  const [Catalog, setCatalog] = useState([]);
  const [searchParams] = useSearchParams();
  const patientId = searchParams.get("patientId");

  // pagination & loading
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalRecords, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchAPi = async () => {
      try {
        setLoading(true);
        const response = await api.get(
          `${endPoint}?patientId=${patientId}&pageNumber=${page}&pageSize=${pageSize}`
        );
        const data = response.data;
        if (response.status >= 200 && response.status < 300) {
          if (Array.isArray(data)) {
            setBookingHistory(data);
            setTotal(data.length);
            console.log(data);
          } else if (data?.items) {
            setBookingHistory(data.items);
            setTotal(
              data.total ?? data.totalCount ?? data.pagination?.total ?? 0
            );
          } else {
            setBookingHistory(data);
            setTotal((data && data.length) || 0);
          }
        }

        const bundleId =
          (Array.isArray(data)
            ? data?.[0]?.bundleId
            : data?.items?.[0]?.bundleId) || data?.bundleId;

        if (bundleId) {
          const response2 = await api.get(`${endPoint1}/${bundleId}`);
          const pkg = response2.data;
          if (response2.status >= 200 && response2.status < 300) {
            setPackage(pkg);
          }
        } else {
          setPackage([]);
        }
        setLoading(false);
      } catch (err) {
        setLoading(false);
        console.error("Lỗi khi fetch API:", err);
      }
    };

    if (patientId) fetchAPi();
  }, [patientId, page, pageSize]);

  const toggle = (bookingCode) => {
    setExpanded((s) => ({ ...s, [bookingCode]: !s[bookingCode] }));
  };

  const statusLabel = (status) => {
    switch (String(status).toLowerCase()) {
      case "pending":
        return { text: "Chờ xác nhận", className: "badge-yellow" };
      case "confirmed":
        return { text: "Đã xác nhận", className: "badge-blue" };
      case "completed":
        return { text: "Hoàn thành", className: "badge-green" };
      case "cancelled":
        return { text: "Đã hủy", className: "badge-red" };
      default:
        return { text: status, className: "" };
    }
  };

  return (
    <div className="booking-history-page">
      <div className="booking-history-header">
        <h1>Lịch sử đặt lịch</h1>
        <p>Xem tất cả các lịch hẹn đã đặt</p>
      </div>

      <div className="booking-filters">
        <div className="filters-title">
          <img src="src\assets\icon\Fillter.svg" alt="Filters" />
          <strong>Bộ lọc</strong>
        </div>
        <span style={{ color: "#737373", fontSize: "15px" }}>
          Lọc lịch hẹn theo ngày và trạng thái
        </span>
        <div className="filter-row">
          <div style={{ display: "flex", gap: "16px" }}>
            <div className="filter-item">
              <label htmlFor="from-date" className="filter-label">
                Từ ngày
              </label>
              <input type="date" id="from-date" className="filter-date-input" />
            </div>
            <div className="filter-item">
              <label htmlFor="to-date" className="filter-label">
                Đến ngày
              </label>
              <input type="date" id="to-date" className="filter-date-input" />
            </div>
          </div>
          <div className="filter-item">
            <label htmlFor="status" className="filter-label">
              Trạng thái
            </label>
            <select id="status" className="filter-status-select">
              <option value="">Tất cả</option>
              <option value="pending">Chờ xác nhận</option>
              <option value="confirmed">Đã xác nhận</option>
              <option value="completed">Hoàn thành</option>
              <option value="cancelled">Hủy</option>
            </select>
          </div>
        </div>
      </div>

      <div className="booking-list">
        {loading ? (
          <div style={{ textAlign: "center", padding: 24 }}>
            <Spin size="large" />
          </div>
        ) : (
          BookingHistory.map((b) => {
            const s = statusLabel(b.status);
            const isExpanded = !!expanded[b.bookingCode];
            return (
              <div
                key={b.bookingCode}
                className={`booking-card booking-${String(
                  b.status
                ).toLowerCase()}`}
              >
                <div className="booking-card-header">
                  <div className="booking-code">
                    Mã đặt lịch: {b.bookingCode}
                  </div>
                  <div className="booking-main">
                    <img src="src\assets\icon\Calender.svg" alt="Calender" />
                    <div className="booking-date&time">
                      <div className="booking-date">{b.RunDate}</div>
                      <div className="booking-time">Ngày/Giờ hẹn: {formatDate(b.slotInfo.appointmentDate)} /  {formatTime(b.slotInfo.timeBlock)}</div>
                    </div>
                    <div className={`booking-badge ${s.className}`}>
                      {s.text}
                    </div>
                  </div>
                  <div className="booking-created">
                    Đặt lịch ngày: {formatDate(b.createdDate)}
                  </div>
                  <div className="booking-actions">
                    <button
                      className="btn-dropdown"
                      onClick={() => toggle(b.bookingCode)}
                      aria-expanded={isExpanded}
                    >
                      {isExpanded ? "Ẩn chi tiết" : "Xem chi tiết"}
                    </button>
                  </div>
                </div>

                <div
                  className={`booking-card-body ${
                    isExpanded ? "open" : "closed"
                  }`}
                  aria-hidden={!isExpanded}
                >
                  <div className="completed-layout expanded-grid">
                    <div className="left-col">
                      <div className="section">
                        <h4>Thông tin cá nhân</h4>
                        <div className="info-row">
                          <div className="info-row-1">
                            <img src="src\assets\icon\User.svg" alt="User" />
                            <span className="label">Họ và tên</span>
                          </div>
                          <span className="value">{b.patientName}</span>
                        </div>
                        <div className="info-row">
                          <div className="info-row-1">
                            <img src="src\assets\icon\Mail.svg" alt="Email" />
                            <span className="label">Email</span>
                          </div>
                          <span className="value">{b.patientEmail}</span>
                        </div>
                        <div className="info-row">
                          <div className="info-row-1">
                            <img src="src\assets\icon\Phone.svg" alt="Phone" />{" "}
                            <span className="label">Điện thoại</span>
                          </div>
                          <span className="value">{b.patientPhoneNumber}</span>
                        </div>
                      </div>
                    </div>

                    <div className="middle-col">
                      <div className="section">
                        <h4>Chi tiết đặt lịch</h4>
                        <div className="info-row">
                          <div className="info-row-1">
                            <img
                              src="src\assets\icon\Document_Gray.svg"
                              alt="Document"
                            />
                            <span className="label">Gói</span>
                          </div>
                          <span className="value">
                            {b.bundleId
                              ? Package?.bundleName || `Gói #${b.bundleId}`
                              : "Không có gói"}
                          </span>
                        </div>
                        {/* Nếu có thêm thông tin về dịch vụ hoặc catalog, có thể hiển thị ở đây */}
                      </div>
                    </div>

                    <div className="right-col">
                      <div className="section payment">
                        <h4>Thông tin thanh toán</h4>
                        <div className="info-row">
                          <div className="info-row-1">
                            <img src="src\assets\icon\Pay.svg" alt="Pay" />
                            <span className="label">Hình thức</span>
                          </div>
                          <span className="value">Tiền mặt</span>
                        </div>
                        {/* Nếu có trường price thì hiển thị, nếu không thì bỏ qua */}
                        {typeof b.price === "number" && (
                          <div className="info-row">
                            <div className="info-row-1">
                              <img src="src\assets\icon\Pay.svg" alt="Pay" />
                              <span className="label">Tổng tiền</span>
                            </div>
                            <span className="value">
                              {b.price.toLocaleString("vi-VN")} ₫
                            </span>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* result area: chỉ hiển thị nếu có status completed/cancelled */}
                  <h3>Kết quả xét nghiệm</h3>
                  <div className="result-area" style={{ marginTop: 20 }}>
                    {String(b.status).toLowerCase() === "completed" ? (
                      <div className="result-box ready">
                        <img
                          src="src\assets\icon\Document_Border.svg"
                          alt="Document_Borders"
                        />{" "}
                        <br />
                        <strong style={{ fontSize: "18px" }}>
                          Kết quả xét nghiệm đã sẵn sàng
                        </strong>
                        <p
                          style={{
                            fontSize: "13px",
                            marginTop: "10px",
                            color: "#737373",
                          }}
                        >
                          Vui lòng liên hệ phòng khám để nhận kết quả.
                        </p>
                        <button className="btn-primary-history-booking">
                          <img
                            src="src\assets\icon\Document_white.svg"
                            alt=""
                          />
                          Xem chi tiết kết quả xét nghiệm
                        </button>
                      </div>
                    ) : String(b.status).toLowerCase() === "cancelled" ? (
                      <div className="result-box cancelled">
                        <strong>Không có kết quả xét nghiệm</strong>
                        <p>Lịch hẹn đã bị hủy, không có kết quả.</p>
                      </div>
                    ) : (
                      <div
                        className={`result-box ${
                          String(b.status).toLowerCase() === "pending" ||
                          String(b.status).toLowerCase() === "confirmed"
                            ? "yellow"
                            : "normal"
                        }`}
                      >
                        <p>Chưa có kết quả xét nghiệm</p>
                        {(String(b.status).toLowerCase() === "pending" ||
                          String(b.status).toLowerCase() === "confirmed") && (
                          <small>
                            Kết quả sẽ được cập nhật sau khi hoàn tất lấy mẫu.
                          </small>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })
        )}
        {/* Pagination */}
        <div style={{ textAlign: "center", marginTop: 16 }}>
          <Pagination
            current={page}
            pageSize={pageSize}
            total={totalRecords}
            onChange={(p, ps) => {
              setPage(p);
              if (ps !== pageSize) {
                setPageSize(ps);
                setPage(1); // reset to first when pageSize changes
              }
            }}
            showSizeChanger
            pageSizeOptions={[5, 10, 20, 50]}
          />
        </div>
      </div>
    </div>
  );
}
