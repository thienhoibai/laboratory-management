import React, { useState, useMemo, useEffect } from "react";
import AdminLayout from "../../../components/admin/layout/AdminLayout";
import {
  FiCalendar,
  FiSearch,
  FiEdit2,
  FiChevronLeft,
  FiChevronRight,
  FiX,
  FiCheck,
} from "react-icons/fi";
import {
  mockAppointments,
  appointmentStatuses,
  timeSlots,
  getAppointmentCountBySlot,
  getStatusInfo,
} from "../../../data/appointment";
import "./AdminAppointmentSchedulePage.css";
import api from "../../../configs/axios";
import { CgLayoutGrid } from "react-icons/cg";

const AdminAppointmentSchedulePage = () => {
  const [selectedDate, setSelectedDate] = useState(new Date()); // Ngày hiện tại
  const [currentMonth, setCurrentMonth] = useState(new Date()); // Tháng hiện tại cho calendar picker
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [statusDropdownOpen, setStatusDropdownOpen] = useState(false);
  const [editingStatus, setEditingStatus] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [isDatePickerOpen, setIsDatePickerOpen] = useState(false);
  const itemsPerPage = 7;

  const [Booking, SetBookings] = useState([]);
  const [checkingInId, setCheckingInId] = useState(null); // track in-flight checkin

  // Format date to YYYY-MM-DD
  const formatDate = (date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  // Get next 7 days from today
  const getNext7Days = () => {
    const days = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    for (let i = 0; i < 7; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() + i);
      days.push(date);
    }
    return days;
  };

  const next7Days = getNext7Days();

  // Get all days in current month for calendar picker
  const getCalendarDays = () => {
    const year = currentMonth.getFullYear();
    const month = currentMonth.getMonth();

    const firstDay = new Date(year, month, 1);
    // const lastDay = new Date(year, month + 1, 0);
    const startDate = new Date(firstDay);

    // Start from Monday of the week containing the 1st
    startDate.setDate(firstDay.getDate() - ((firstDay.getDay() + 6) % 7));

    const days = [];
    const currentDate = new Date(startDate);

    // Get 6 weeks (42 days) to fill the calendar
    for (let i = 0; i < 42; i++) {
      days.push(new Date(currentDate));
      currentDate.setDate(currentDate.getDate() + 1);
    }

    return days;
  };

  const calendarDays = getCalendarDays();

  // Filter appointments
  const filteredAppointments = useMemo(() => {
    const selectedDateStr = formatDate(selectedDate);
    let filtered = mockAppointments.filter(
      (apt) => apt.appointmentDate === selectedDateStr
    );

    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (apt) =>
          apt.patientName.toLowerCase().includes(query) ||
          apt.email.toLowerCase().includes(query) ||
          apt.phone.includes(query) ||
          apt.bookingCode.toLowerCase().includes(query)
      );
    }

    return filtered;
  }, [selectedDate, searchQuery]);

  // Pagination
  const totalPages = Math.ceil(filteredAppointments.length / itemsPerPage);
  // const paginatedAppointments = filteredAppointments.slice(
  //   (currentPage - 1) * itemsPerPage,
  //   currentPage * itemsPerPage
  // );

  // Handle date selection
  const handleDateSelect = (date) => {
    setSelectedDate(date);
    setCurrentPage(1);
    setIsDatePickerOpen(false);
  };

  // Handle month navigation in calendar picker
  const handlePrevMonth = () => {
    setCurrentMonth(
      new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1, 1)
    );
  };

  const handleNextMonth = () => {
    setCurrentMonth(
      new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1, 1)
    );
  };

  // Handle navigation
  const handlePrevDay = () => {
    const newDate = new Date(selectedDate);
    newDate.setDate(selectedDate.getDate() - 1);

    // Không cho chọn ngày trước hôm nay
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (newDate >= today) {
      setSelectedDate(newDate);
    }
  };

  const handleNextDay = () => {
    const newDate = new Date(selectedDate);
    newDate.setDate(selectedDate.getDate() + 1);

    // Không cho chọn ngày sau 7 ngày
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 6);
    maxDate.setHours(23, 59, 59, 999);

    if (newDate <= maxDate) {
      setSelectedDate(newDate);
    }
  };

  // Handle edit appointment
  const handleEditAppointment = (appointment) => {
    setSelectedAppointment(appointment);
    setEditingStatus(appointment.status);
    setIsModalOpen(true);
  };

  // Handle close modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAppointment(null);
    setEditingStatus("");
    setStatusDropdownOpen(false);
  };

  // Handle save changes
  const handleSaveChanges = () => {
    // Trong thực tế, gọi API để cập nhật
    console.log("Cập nhật trạng thái:", editingStatus);
    handleCloseModal();
  };

  // Format Vietnamese day name
  const getVietnameseDayName = (date) => {
    const days = ["CN", "Th 2", "Th 3", "Th 4", "Th 5", "Th 6", "Th 7"];
    return days[date.getDay()];
  };

  // Format month/year for calendar picker
  const formatCalendarMonthYear = (date) => {
    const months = [
      "Tháng 1",
      "Tháng 2",
      "Tháng 3",
      "Tháng 4",
      "Tháng 5",
      "Tháng 6",
      "Tháng 7",
      "Tháng 8",
      "Tháng 9",
      "Tháng 10",
      "Tháng 11",
      "Tháng 12",
    ];
    return `${months[date.getMonth()]} ${date.getFullYear()}`;
  };

  // Check if date is in current month
  const isInCurrentMonth = (date) => {
    return date.getMonth() === currentMonth.getMonth();
  };

  // Check if date is today
  const isToday = (date) => {
    const today = new Date();
    return formatDate(date) === formatDate(today);
  };
  const formatMonthYear = (date) => {
    const days = [
      "Chủ Nhật",
      "Thứ Hai",
      "Thứ Ba",
      "Thứ Tư",
      "Thứ Năm",
      "Thứ Sáu",
      "Thứ Bảy",
    ];
    const months = [
      "tháng 1",
      "tháng 2",
      "tháng 3",
      "tháng 4",
      "tháng 5",
      "tháng 6",
      "tháng 7",
      "tháng 8",
      "tháng 9",
      "tháng 10",
      "tháng 11",
      "tháng 12",
    ];
    return `${days[date.getDay()]}, ${date.getDate()} ${
      months[date.getMonth()]
    }, ${date.getFullYear()}`;
  };

  // Check if can go prev/next
  const canGoPrev = () => {
    const prevDate = new Date(selectedDate);
    prevDate.setDate(selectedDate.getDate() - 1);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return prevDate >= today;
  };

  const canGoNext = () => {
    const nextDate = new Date(selectedDate);
    nextDate.setDate(selectedDate.getDate() + 1);
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 6);
    return nextDate <= maxDate;
  };

  const fetchAPI = async () => {
    const response = await api.get(`testorder/api/Booking/info?pageNumber=3`);
    const data = response.data;
    if (response.status >= 200 && response.status < 300) {
      SetBookings(data);
      console.log(data);
    }
  };
  useEffect(() => {
    fetchAPI();
  }, []);

  const handleCheckin = async (bookingId) => {
    try {
      setCheckingInId(bookingId);
      const response = await api.put(
        `testorder/api/Booking/check-in?bookingId=${bookingId}`
      );
      if (response.status >= 200 && response.status < 300) {
        await fetchAPI(); // refresh list after success
      }
    } catch (err) {
      console.error("Check-in failed:", err);
    } finally {
      setCheckingInId(null);
    }
  };

  return (
    <AdminLayout
      pageTitle="Quản lý lịch xét nghiệm"
      breadcrumbs={[
        { name: "Tổng quan", link: "/admin/dashboard" },
        { name: "Quản lý gói xét nghiệm" },
      ]}
    >
      <div className="appointment-schedule-container">
        <div className="appointment-header">
          <h1>Quản lý lịch xét nghiệm</h1>
          <p>Quản lý lịch hẹn xét nghiệm của bệnh nhân</p>
        </div>

        <div className="appointment-content">
          {/* Calendar Section */}
          <div className="calendar-section">
            <div className="calendar-header">
              <div className="calendar-title">
                <button
                  className="calendar-icon-button"
                  onClick={() => setIsDatePickerOpen(!isDatePickerOpen)}
                  title="Chọn ngày"
                >
                  <FiCalendar />
                </button>
                <span>Chọn ngày</span>
              </div>
              <div className="calendar-nav">
                <button onClick={handlePrevDay} disabled={!canGoPrev()}>
                  <FiChevronLeft size={20} />
                </button>
                <span className="calendar-current-date">
                  {formatMonthYear(selectedDate)}
                </span>
                <button onClick={handleNextDay} disabled={!canGoNext()}>
                  <FiChevronRight size={20} />
                </button>
              </div>
            </div>

            {/* Date Picker Dropdown */}
            {isDatePickerOpen && (
              <div className="date-picker-dropdown">
                <div className="date-picker-header">
                  <button onClick={handlePrevMonth}>
                    <FiChevronLeft size={20} />
                  </button>
                  <span>{formatCalendarMonthYear(currentMonth)}</span>
                  <button onClick={handleNextMonth}>
                    <FiChevronRight size={20} />
                  </button>
                </div>

                <div className="date-picker-weekdays">
                  <div>T2</div>
                  <div>T3</div>
                  <div>T4</div>
                  <div>T5</div>
                  <div>T6</div>
                  <div>T7</div>
                  <div>CN</div>
                </div>

                <div className="date-picker-calendar">
                  {calendarDays.map((day, index) => {
                    const isSelected =
                      formatDate(day) === formatDate(selectedDate);
                    const inMonth = isInCurrentMonth(day);
                    const today = isToday(day);

                    return (
                      <div
                        key={index}
                        className={`date-picker-calendar-day ${
                          isSelected ? "selected" : ""
                        } ${!inMonth ? "other-month" : ""} ${
                          today ? "today" : ""
                        }`}
                        onClick={() => handleDateSelect(day)}
                      >
                        {day.getDate()}
                      </div>
                    );
                  })}
                </div>

                <div className="date-picker-footer">
                  <button
                    onClick={() => {
                      setSelectedDate(new Date());
                      setCurrentMonth(new Date());
                    }}
                  >
                    Hôm nay
                  </button>
                  <button onClick={() => setIsDatePickerOpen(false)}>
                    Đóng
                  </button>
                </div>
              </div>
            )}

            <div className="calendar-grid">
              {next7Days.map((day, index) => {
                const isSelected = formatDate(day) === formatDate(selectedDate);
                return (
                  <div
                    key={index}
                    className={`calendar-day ${isSelected ? "selected" : ""}`}
                    onClick={() => handleDateSelect(day)}
                  >
                    <div className="day-label">{getVietnameseDayName(day)}</div>
                    <div className="day-number">{day.getDate()}</div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Search Section */}
          <div className="search-section">
            <div className="search-box">
              <FiSearch size={18} />
              <input
                type="text"
                placeholder="Tìm kiếm theo tên, email, số điện thoại hoặc mã đặt lịch..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>

          {/* Appointments Table */}
          <div className="appointments-table-container">
            <table className="appointments-table">
              <thead>
                <tr>
                  <th>Họ và tên</th>
                  <th>Email</th>
                  <th>Số điện thoại</th>
                  <th>Mã đặt lịch</th>
                  <th>Ngày Khám</th>
                  <th>Giờ Khám</th>
                  <th>Trạng thái</th>
                  <th>Thao Tác</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {Booking.length > 0 ? (
                  Booking.map((appointment) => {
                    return (
                      <tr key={appointment.bookingId}>
                        <td>{appointment.patientName}</td>
                        <td>{appointment.patientEmail}</td>
                        <td>{appointment.patientPhoneNumber}</td>
                        <td>{appointment.bookingCode}</td>
                        <td>{appointment.slotInfo.appointmentDate}</td>
                        <td>{appointment.slotInfo.timeBlock}</td>
                        <td>{appointment.status}</td>
                        <td>
                          <button
                            onClick={() => handleCheckin(appointment.bookingId)}
                            disabled={checkingInId === appointment.bookingId}
                          >
                            {checkingInId === appointment.bookingId
                              ? "Đang check in..."
                              : "Check In"}
                          </button>
                        </td>
                        <td>
                          <button
                            className="action-button"
                            onClick={() => handleEditAppointment(appointment)}
                          >
                            <FiEdit2 size={18} />
                          </button>
                        </td>
                      </tr>
                    );
                  })
                ) : (
                  <tr>
                    <td
                      colSpan="7"
                      style={{ textAlign: "center", padding: "40px" }}
                    >
                      Không có lịch hẹn nào trong ngày này
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {filteredAppointments.length > 0 && (
            <div className="pagination">
              <div className="pagination-info">
                Hiện thị {(currentPage - 1) * itemsPerPage + 1} đến{" "}
                {Math.min(
                  currentPage * itemsPerPage,
                  filteredAppointments.length
                )}{" "}
                trong {filteredAppointments.length} lịch hẹn
              </div>
              <div className="pagination-controls">
                <button
                  className="pagination-button"
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                >
                  Trước
                </button>
                {Array.from({ length: totalPages }, (_, i) => i + 1).map(
                  (page) => (
                    <button
                      key={page}
                      className={`pagination-button ${
                        currentPage === page ? "active" : ""
                      }`}
                      onClick={() => setCurrentPage(page)}
                    >
                      {page}
                    </button>
                  )
                )}
                <button
                  className="pagination-button"
                  onClick={() =>
                    setCurrentPage((p) => Math.min(totalPages, p + 1))
                  }
                  disabled={currentPage === totalPages}
                >
                  Sau
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Edit Modal */}
      {isModalOpen && selectedAppointment && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div
            className="appointment-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <div>
                <h2>Chỉnh sửa trạng thái lịch hẹn</h2>
                <p className="modal-subtitle">
                  Cập nhật trạng thái cho lịch hẹn của{" "}
                  {selectedAppointment.patientName}
                </p>
              </div>
              <button className="modal-close" onClick={handleCloseModal}>
                <FiX size={24} />
              </button>
            </div>

            <div className="modal-body">
              <div className="modal-info-1">
                <div className="modal-info-grid">
                  <div className="modal-info-item">
                    <span className="modal-info-label">Mã đặt lịch</span>
                    <span className="modal-info-value">
                      {selectedAppointment.bookingCode}
                    </span>
                  </div>
                  <div className="modal-info-item">
                    <span className="modal-info-label">Giờ hẹn hiện tại</span>
                    <span className="modal-info-value">
                      {selectedAppointment.slotInfo.timeBlock}
                    </span>
                  </div>
                </div>

                <div className="modal-section">
                  <h3 className="modal-section-title">Trạng thái</h3>
                  <div className="status-dropdown">
                    <button
                      className="status-dropdown-button"
                      onClick={() => setStatusDropdownOpen(!statusDropdownOpen)}
                    >
                      <span>{getStatusInfo(editingStatus).label}</span>
                      <FiChevronRight
                        style={{
                          transform: statusDropdownOpen
                            ? "rotate(90deg)"
                            : "rotate(0deg)",
                          transition: "transform 0.2s",
                        }}
                      />
                    </button>
                    {statusDropdownOpen && (
                      <div className="status-dropdown-menu">
                        {appointmentStatuses.map((status) => (
                          <div
                            key={status.value}
                            className={`status-dropdown-item ${
                              editingStatus === status.value ? "selected" : ""
                            }`}
                            onClick={() => {
                              setEditingStatus(status.value);
                              setStatusDropdownOpen(false);
                            }}
                          >
                            {editingStatus === status.value && (
                              <FiCheck size={16} />
                            )}
                            <span>{status.label}</span>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              </div>

              <div className="modal-section">
                <h3 className="modal-section-title">
                  Đổi lịch trong ngày (tùy chọn)
                </h3>
                <div className="time-slots-grid">
                  {timeSlots.map((slot) => {
                    const count = getAppointmentCountBySlot(
                      selectedAppointment.appointmentDate,
                      slot.time
                    );
                    const isDisabled = count >= slot.capacity;
                    const isSelected =
                      selectedAppointment.appointmentTime === slot.time;

                    return (
                      <div
                        key={slot.time}
                        className={`time-slot-card ${
                          isDisabled ? "disabled" : ""
                        } ${isSelected ? "selected" : ""}`}
                      >
                        <div className="time-slot-time">{slot.time}</div>
                        <div className="time-slot-capacity">
                          {count}/{slot.capacity}
                        </div>
                      </div>
                    );
                  })}
                </div>
                <div className="reschedule-note">Sẽ đổi lịch sang 10:30</div>
              </div>
            </div>

            <div className="modal-footer">
              <button
                className="modal-button cancel"
                onClick={handleCloseModal}
              >
                Hủy
              </button>
              <button
                className="modal-button primary"
                onClick={handleSaveChanges}
              >
                Lưu thay đổi
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default AdminAppointmentSchedulePage;
