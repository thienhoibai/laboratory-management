import React, { useState, useEffect } from "react";
import { useDispatch } from "react-redux";
import { setPatient } from "../../data/patientSlice";
import { useNavigate } from "react-router-dom";
import "./ProfilePage.css";
import { setAuthToken } from "../../utils/auth";
import api from "../../configs/axios";
import { toast } from "react-toastify";
import { Pagination, Modal, Form, Input, Select, DatePicker } from "antd";
import dayjs from "dayjs";
import {
  parseDateToInput,
  calculateAge,
  formatDateTime,
} from "../../utils/formatDate";

// Change password feature removed:
// This ProfilePage does not include any "change password" UI, state or API calls.
// If a change-password feature is added later, keep it in a separate component/modal
// and do not couple password changes with profile data updates.

// ===== CONSTANTS & UTILS =====
const initialFormData = {
  fullName: "",
  gender: "",
  dateOfBirth: "",
  phoneNumber: "",
  email: "",
  address: "",
  identityCard: "",
  healthInsurance: "",
};

// ===== MAIN PROFILE PAGE COMPONENT =====
export default function ProfilePage() {
  const dispatch = useDispatch();

  // ===== STATE =====
  const [userData, setUserData] = useState([]);
  const [activeTab, setActiveTab] = useState("personal");
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState(initialFormData);
  const [errors, setErrors] = useState({});
  const [medicalRecords, setMedicalRecords] = useState([]);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createForm] = Form.useForm();
  const [isCreating, setIsCreating] = useState(false);

  const [totalRecords, setTotalRecords] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(2);

  const navigate = useNavigate();

  // ===== HOOKS =====
  useEffect(() => {
    fetchProfile();
    fetchMedicalRecords(page, pageSize);
  }, [page, pageSize]);

  // ===== API CALLS =====
  const fetchProfile = async () => {
    try {
      const token = localStorage.getItem("accessToken");
      setAuthToken(token);

      const check = await api.get(`patient/v1/patients/me`);
      if (!check.data || check.data.succeeded === false || !check.data.data) {
        navigate("/create-profile");
        return;
      }

      const patient = check.data.data;
      const patientId = patient.patientId;

      if (!patientId) {
        navigate("/create-profile");
        return;
      }

      const response = await api.get(`patient/v1/patients/${patientId}`);
      const data = response.data;
      if (response.status === 200 && response.data) {
        setUserData(data);

        dispatch(
          setPatient({
            patientId: data.patientId,
            fullName: data.fullName,
            phone: data.phone,
            email: data.email,
          })
        );
      } else {
        navigate("/create-profile");
      }
    } catch (error) {
      toast.error(error);
      navigate("/create-profile");
    }
  };

  // Lấy danh sách hồ sơ bệnh án cá nhân (phân trang)
  const fetchMedicalRecords = async (page, pageSize) => {
    try {
      const token = localStorage.getItem("accessToken");
      setAuthToken(token);
      const response = await api.get(
        `patient/v1/patients/mine?page=${page}&pageSize=${pageSize}`
      );
      if (response.status >= 200 && response.status < 300) {
        // Đúng cấu trúc response: lấy từ response.data.items và response.data.total
        setMedicalRecords(response.data.items || [null]);
        setTotalRecords(response.data.total || 0);
      }
    } catch (error) {
      toast.error(error);
      setMedicalRecords([]);
      setTotalRecords(0);
    }
  };

  // ===== FORM UTILS =====

  const getInitials = (name) => {
    if (!name) return "";
    return name
      .split(" ")
      .map((word) => word[0])
      .join("")
      .toUpperCase();
  };

  // ===== HANDLERS =====
  const handleOpenModal = () => {
    setFormData({
      fullName: userData.fullName || "",
      // Sửa lại để form hiển thị đúng giá trị value của select
      gender: userData.gender === 1 ? "1" : userData.gender === 0 ? "0" : "",
      dateOfBirth: parseDateToInput(userData.dateOfBirth),
      phoneNumber: userData.phone || "",
      email: userData.email || "",
      address: userData.address || "",
      identityCard: userData.idNumber || "",
      healthInsurance: userData.insuranceNumber || "",
    });
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setErrors({});
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: "" }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    if (!formData.fullName.trim())
      newErrors.fullName = "Vui lòng nhập họ và tên";
    if (!formData.phoneNumber.trim())
      newErrors.phoneNumber = "Vui lòng nhập số điện thoại";
    else if (!/^0[3-9]\d{8}$/.test(formData.phoneNumber))
      newErrors.phoneNumber = "Số điện thoại không hợp lệ";
    if (!formData.email.trim()) newErrors.email = "Vui lòng nhập email";
    else if (!/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(formData.email))
      newErrors.email = "Email không hợp lệ";
    if (!formData.address.trim()) newErrors.address = "Vui lòng nhập địa chỉ";
    if (!formData.identityCard.trim())
      newErrors.identityCard = "Vui lòng nhập số CMND/CCCD";
    if (!formData.dateOfBirth.trim())
      newErrors.dateOfBirth = "Vui lòng nhập ngày sinh";
    else {
      // Validate date format YYYY-MM-DD
      const dateRegex = /^\d{4}-\d{2}-\d{2}$/;
      if (!dateRegex.test(formData.dateOfBirth)) {
        newErrors.dateOfBirth = "Ngày sinh phải có định dạng YYYY-MM-DD";
      } else {
        const [year, month, day] = formData.dateOfBirth.split("-");
        const date = new Date(year, month - 1, day);
        if (
          date.getMonth() !== month - 1 ||
          date.getDate() !== parseInt(day) ||
          date.getFullYear() !== parseInt(year)
        ) {
          newErrors.dateOfBirth = "Ngày sinh không hợp lệ";
        } else {
          const today = new Date();
          if (date > today) {
            newErrors.dateOfBirth =
              "Ngày sinh không thể là ngày trong tương lai";
          }
        }
      }
    }
    return newErrors;
  };

  const handleSave = async () => {
    const newErrors = validateForm();
    setErrors(newErrors);

    if (Object.keys(newErrors).length === 0) {
      const data = {
        fullName: formData.fullName,
        dateOfBirth: formData.dateOfBirth,
        gender: formData.gender === "1" ? 1 : formData.gender === "0" ? 0 : 2,
        phone: formData.phoneNumber,
        email: formData.email,
        address: formData.address,
        idNumber: formData.identityCard,
        insuranceNumber: formData.healthInsurance,
      };

      try {
        const token = localStorage.getItem("accessToken");
        setAuthToken(token);

        await api.put(`patient/v1/patients/${userData.patientId}`, data);

        // Fetch lại profile từ API để cập nhật giao diện
        await fetchProfile();

        setShowModal(false);
        toast.success("Cập nhật thông tin thành công!");
      } catch (error) {
        toast.error(error.data || "Cập nhật thất bại!");
      }
    }
  };

  // Thêm hồ sơ bệnh án mới
  const handleCreateMedicalRecord = async (values) => {
    setIsCreating(true);
    try {
      const token = localStorage.getItem("accessToken");
      setAuthToken(token);
      const data = {
        fullName: values.fullName,
        dateOfBirth: dayjs(values.dateOfBirth).format("YYYY-MM-DD"),
        gender: values.gender,
        phone: values.phoneNumber,
        email: values.email,
        address: values.address,
        idNumber: values.identityCard,
        insuranceNumber: values.healthInsurance,
        createdBy: "user",
        createdAt: new Date().toISOString(),
      };
      const response = await api.post("patient/v1/patients", data);
      if (response.status >= 200 && response.status < 300) {
        toast.success("Thêm hồ sơ bệnh án thành công!");
        setShowCreateModal(false);
        createForm.resetFields();
        fetchMedicalRecords(page, pageSize);
      }
    } catch (error) {
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi thêm hồ sơ. Vui lòng thử lại!"
      );
    } finally {
      setIsCreating(false);
    }
  };

  // ===== RENDER MAIN UI =====
  return (
    <div className="profile-page">
      {/* ===== HEADER SECTION ===== */}
      <div className="profile-header">
        <div className="profile-header-content">
          <div className="profile-avatar-section">
            <div className="profile-avatar">
              {getInitials(userData.fullName)}
            </div>
            <div className="profile-info">
              <h1 className="profile-name">{userData.fullName}</h1>
              <p className="profile-patient-id">
                Mã bệnh nhân: {userData.patientId}
              </p>
              <div className="profile-tags">
                <span className="profile-tag">
                  {userData.gender == 1 ? "Nam" : "Nữ"}
                </span>
                <span className="profile-tag">
                  {typeof userData.age !== "undefined"
                    ? userData.age
                    : calculateAge(parseDateToInput(userData.dateOfBirth))}{" "}
                  Tuổi
                </span>
              </div>
            </div>
          </div>
          <div className="profile-header-actions">
            <button className="profile-back-btn" onClick={() => navigate("/")}>
              <svg
                className="back-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                style={{ position: "relative", top: "2px" }}
              >
                <path d="M19 12H5" />
                <path d="M12 19l-7-7 7-7" />
              </svg>
              Quay về trang chủ
            </button>
            <button
              className="profile-history-btn"
              onClick={() => navigate("/history")}
            >
              <svg
                className="clock-icon"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <circle cx="12" cy="12" r="10" />
                <polyline points="12,6 12,12 16,14" />
              </svg>
              Lịch sử đặt lịch
            </button>
          </div>
        </div>
      </div>

      {/* ===== MAIN CONTENT ===== */}
      <div className="profile-content">
        {/* ===== TABS ===== */}
        <div className="profile-tabs">
          <button
            className={`profile-tab ${
              activeTab === "personal" ? "active" : ""
            }`}
            onClick={() => setActiveTab("personal")}
          >
            <svg
              className="tab-icon"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
            Thông tin cá nhân
          </button>
          <button
            className={`profile-tab ${activeTab === "medical" ? "active" : ""}`}
            onClick={() => setActiveTab("medical")}
          >
            <svg
              className="tab-icon"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
              <polyline points="14,2 14,8 20,8" />
              <line x1="16" y1="13" x2="8" y2="13" />
              <line x1="16" y1="17" x2="8" y2="17" />
              <polyline points="10,9 9,9 8,9" />
            </svg>
            Hồ sơ bệnh án
          </button>
        </div>

        {/* ===== TAB: PERSONAL INFO ===== */}
        {activeTab === "personal" && (
          <div className="profile-tab-content">
            <div className="profile-section">
              <div className="profile-Title">
                <div>
                  <h2 className="profile-section-title">Thông tin cá nhân</h2>
                  <p className="profile-section-subtitle">
                    Thông tin chi tiết về bệnh nhân
                  </p>
                </div>
              </div>

              <div className="profile-info-grid">
                <div className="profile-info-column">
                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                      <circle cx="12" cy="7" r="4" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Họ và tên</span>
                      <span className="info-value">{userData.fullName}</span>
                    </div>
                  </div>

                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                      <line x1="16" y1="2" x2="16" y2="6" />
                      <line x1="8" y1="2" x2="8" y2="6" />
                      <line x1="3" y1="10" x2="21" y2="10" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Ngày sinh</span>
                      <span className="info-value">{userData.dateOfBirth}</span>
                    </div>
                  </div>

                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Số điện thoại</span>
                      <span className="info-value">{userData.phone}</span>
                    </div>
                  </div>

                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                      <polyline points="22,6 12,13 2,6" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Email</span>
                      <span className="info-value">{userData.email}</span>
                    </div>
                  </div>
                </div>

                <div className="profile-info-column">
                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                      <circle cx="12" cy="10" r="3" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Địa chỉ</span>
                      <span className="info-value">{userData.address}</span>
                    </div>
                  </div>

                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <rect x="2" y="3" width="20" height="14" rx="2" ry="2" />
                      <line x1="8" y1="21" x2="16" y2="21" />
                      <line x1="12" y1="17" x2="12" y2="21" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Số CMND/CCCD</span>
                      <span className="info-value">{userData.idNumber}</span>
                    </div>
                  </div>

                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Số bảo hiểm y tế</span>
                      <span className="info-value">
                        {userData.insuranceNumber}
                      </span>
                    </div>
                  </div>

                  <div className="profile-info-item">
                    <svg
                      className="info-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                      <line x1="16" y1="2" x2="16" y2="6" />
                      <line x1="8" y1="2" x2="8" y2="6" />
                      <line x1="3" y1="10" x2="21" y2="10" />
                    </svg>
                    <div className="info-content">
                      <span className="info-label">Ngày đăng ký</span>
                      <span className="info-value">
                        {formatDateTime(userData.createdAt)}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <button className="profile-update-btn" onClick={handleOpenModal}>
                <svg
                  className="update-icon"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                </svg>
                Cập nhật
              </button>
            </div>
          </div>
        )}

        {/* ===== TAB: MEDICAL RECORDS ===== */}
        {activeTab === "medical" && (
          <div className="profile-tab-content">
            {/* Khu vực hồ sơ bệnh án cá nhân (chỉ hiển thị profile hiện tại) */}
            <div className="profile-section personal">
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  marginBottom: 18,
                }}
              >
                <div>
                  <h2 className="profile-section-title personal">
                    Hồ sơ bệnh án cá nhân
                  </h2>
                  <p className="profile-section-subtitle">
                    Thông tin chi tiết về hồ sơ bệnh án của bạn
                  </p>
                </div>
                <button
                  className="profile-update-btn"
                  onClick={() => setShowCreateModal(true)}
                  style={{ marginBottom: 0 }}
                >
                  + Thêm hồ sơ bệnh án
                </button>
              </div>
              <div className="medical-records-list">
                <div className="medical-record-card">
                  <div className="medical-record-header">
                    <h3 className="medical-record-title">
                      Hồ sơ bệnh án #M123
                    </h3>
                    <span className="medical-record-status">
                      {userData.status || "Đang hoạt động"}
                    </span>
                  </div>
                  <div className="medical-record-dates">
                    <span className="medical-record-date">
                      Ngày tạo: {formatDateTime(userData.createdAt)}
                    </span>
                    <span className="medical-record-separator">•</span>
                    <span className="medical-record-date">
                      Cập nhật lần cuối: {formatDateTime(userData.updatedAt)}
                    </span>
                  </div>
                  <div className="medical-record-patient-info">
                    <h4 className="medical-record-patient-title">
                      Thông tin bệnh nhân
                    </h4>
                    <div className="medical-record-patient-details">
                      <div className="medical-record-patient-column">
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Họ tên:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.fullName}
                          </span>
                        </div>
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Ngày sinh:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.dateOfBirth}
                          </span>
                        </div>
                      </div>
                      <div className="medical-record-patient-column">
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Mã BN:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.patientId}
                          </span>
                        </div>
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Giới tính:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.gender === 1 ? "Nam" : "Nữ"}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                  <button
                    className="medical-record-view-btn"
                    onClick={() => navigate(`/medical-record`)}
                  >
                    <svg
                      className="view-icon"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                    >
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                    Xem chi tiết hồ sơ bệnh án
                  </button>
                </div>
              </div>
            </div>
            {/* Khu vực hồ sơ bệnh án của người khác (list từ API, loại bỏ hồ sơ cá nhân nếu trùng patientId) */}
            <div className="profile-section others" style={{ marginTop: 32 }}>
              <h2 className="profile-section-title others">
                Hồ sơ bệnh án của người khác
              </h2>
              <p className="profile-section-subtitle">
                Danh sách hồ sơ bệnh án của người khác
              </p>
              <div className="medical-records-list">
                {medicalRecords.filter(
                  (record) => record.patientId !== userData.patientId
                ).length === 0 ? (
                  <div style={{ color: "#888", marginBottom: 16 }}>
                    Không có hồ sơ bệnh án nào.
                  </div>
                ) : (
                  medicalRecords
                    .filter((record) => record.patientId !== userData.patientId)
                    .map((record) => (
                      <div
                        className="medical-record-card"
                        key={record.patientId}
                      >
                        <div className="medical-record-header">
                          <h3 className="medical-record-title">
                            Hồ sơ bệnh án #{record.patientId}
                          </h3>
                          <span className="medical-record-status">
                            {record.status || "Đang hoạt động"}
                          </span>
                        </div>
                        <div className="medical-record-dates">
                          <span className="medical-record-date">
                            Ngày tạo: {formatDateTime(record.createdAt)}
                          </span>
                          <span className="medical-record-separator">•</span>
                          <span className="medical-record-date">
                            Cập nhật lần cuối:{" "}
                            {formatDateTime(record.updatedAt)}
                          </span>
                        </div>
                        <div className="medical-record-patient-info">
                          <h4 className="medical-record-patient-title">
                            Thông tin bệnh nhân
                          </h4>
                          <div className="medical-record-patient-details">
                            <div className="medical-record-patient-column">
                              <div className="medical-record-patient-item">
                                <span className="medical-record-patient-label">
                                  Họ tên:
                                </span>
                                <span className="medical-record-patient-value">
                                  {record.fullName}
                                </span>
                              </div>
                              <div className="medical-record-patient-item">
                                <span className="medical-record-patient-label">
                                  Ngày sinh:
                                </span>
                                <span className="medical-record-patient-value">
                                  {record.dateOfBirth}
                                </span>
                              </div>
                            </div>
                            <div className="medical-record-patient-column">
                              <div className="medical-record-patient-item">
                                <span className="medical-record-patient-label">
                                  Mã BN:
                                </span>
                                <span className="medical-record-patient-value">
                                  {record.patientId}
                                </span>
                              </div>
                              <div className="medical-record-patient-item">
                                <span className="medical-record-patient-label">
                                  Giới tính:
                                </span>
                                <span className="medical-record-patient-value">
                                  {record.gender === 1 ? "Nam" : "Nữ"}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div
                          style={{
                            display: "flex",
                            gap: 8,
                            justifyContent: "flex-end",
                          }}
                        >
                          <button
                            className="medical-record-view-btn"
                            onClick={() =>
                              navigate(`/medical-record/${record.patientId}`)
                            }
                          >
                            <svg
                              className="view-icon"
                              viewBox="0 0 24 24"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2"
                            >
                              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                              <circle cx="12" cy="12" r="3" />
                            </svg>
                            Xem chi tiết hồ sơ bệnh án
                          </button>
                          <button
                            className="medical-record-delete-btn"
                            style={{
                              background: "#ef4444",
                              color: "white",
                              border: "none",
                              borderRadius: "8px",
                              padding: "12px 20px",
                              fontWeight: 600,
                              fontSize: "14px",
                              cursor: "pointer",
                              display: "flex",
                              alignItems: "center",
                              gap: "8px",
                              transition: "all 0.2s",
                            }}
                            onClick={async () => {
                              if (
                                window.confirm(
                                  "Bạn có chắc muốn xóa hồ sơ này?"
                                )
                              ) {
                                try {
                                  const token =
                                    localStorage.getItem("accessToken");
                                  setAuthToken(token);
                                  await api.delete(
                                    `patient/v1/patients/${record.patientId}`
                                  );
                                  toast.success(
                                    "Xóa hồ sơ bệnh án thành công!"
                                  );
                                  fetchMedicalRecords(page, pageSize);
                                } catch (error) {
                                  toast.error(
                                    error.response?.data?.message ||
                                      "Xóa hồ sơ thất bại!"
                                  );
                                }
                              }
                            }}
                          >
                            <svg
                              style={{ width: 16, height: 16 }}
                              viewBox="0 0 24 24"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2"
                            >
                              <polyline points="3 6 5 6 21 6" />
                              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m5 0V4a2 2 0 0 1 2-2h0a2 2 0 0 1 2 2v2" />
                              <line x1="10" y1="11" x2="10" y2="17" />
                              <line x1="14" y1="11" x2="14" y2="17" />
                            </svg>
                            Xóa
                          </button>
                        </div>
                      </div>
                    ))
                )}
                <div style={{ textAlign: "center", marginTop: 16 }}>
                  <Pagination
                    current={page}
                    pageSize={pageSize}
                    total={totalRecords}
                    onChange={(p, ps) => {
                      setPage(p);
                      setPageSize(ps);
                    }}
                    showSizeChanger
                  />
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* ===== UPDATE MODAL ===== */}
      {showModal && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header-1">
              <h2 className="modal-title">Cập nhật thông tin bệnh nhân</h2>
              <p className="modal-subtitle">
                Chỉnh sửa thông tin cá nhân của bệnh nhân
              </p>
              <button className="modal-close" onClick={handleCloseModal}>
                <svg
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <line x1="18" y1="6" x2="6" y2="18" />
                  <line x1="6" y1="6" x2="18" y2="18" />
                </svg>
              </button>
            </div>

            <div className="modal-body-1">
              <div className="form-row-1">
                <div className="form-group">
                  <label>Họ và tên</label>
                  <input
                    type="text"
                    name="fullName"
                    value={formData.fullName}
                    onChange={handleInputChange}
                    className={errors.fullName ? "error" : ""}
                  />
                  {errors.fullName && (
                    <span className="error-text">{errors.fullName}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Giới tính</label>
                  <select
                    name="gender"
                    value={formData.gender}
                    onChange={handleInputChange}
                  >
                    <option value="">Chọn giới tính</option>
                    <option value="1">Nam</option>
                    <option value="0">Nữ</option>
                  </select>
                </div>
              </div>

              <div className="form-row-1">
                <div className="form-group">
                  <label>Ngày sinh</label>
                  <input
                    type="date"
                    name="dateOfBirth"
                    value={formData.dateOfBirth}
                    onChange={handleInputChange}
                    className={errors.dateOfBirth ? "error" : ""}
                  />
                  {errors.dateOfBirth && (
                    <span className="error-text">{errors.dateOfBirth}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Số điện thoại</label>
                  <input
                    type="text"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleInputChange}
                    className={errors.phoneNumber ? "error" : ""}
                  />
                  {errors.phoneNumber && (
                    <span className="error-text">{errors.phoneNumber}</span>
                  )}
                </div>
              </div>
              <div className="form-row-1">
                <div className="form-group">
                  <label>Email</label>
                  <input
                    type="email"
                    name="email"
                    value={formData.email}
                    onChange={handleInputChange}
                    className={errors.email ? "error" : ""}
                  />
                  {errors.email && (
                    <span className="error-text">{errors.email}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Địa chỉ</label>
                  <input
                    type="text"
                    name="address"
                    value={formData.address}
                    onChange={handleInputChange}
                    className={errors.address ? "error" : ""}
                  />
                  {errors.address && (
                    <span className="error-text">{errors.address}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Số CMND/CCCD</label>
                  <input
                    type="text"
                    name="identityCard"
                    value={formData.identityCard}
                    onChange={handleInputChange}
                    className={errors.identityCard ? "error" : ""}
                  />
                  {errors.identityCard && (
                    <span className="error-text">{errors.identityCard}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Số bảo hiểm y tế</label>
                  <input
                    type="text"
                    name="healthInsurance"
                    value={formData.healthInsurance}
                    onChange={handleInputChange}
                  />
                </div>
              </div>
            </div>

            <div className="modal-footer">
              <button className="btn-cancel" onClick={handleCloseModal}>
                Hủy
              </button>
              <button className="btn-save" onClick={handleSave}>
                Lưu thay đổi
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ===== CREATE MEDICAL RECORD MODAL ===== */}
      <Modal
        open={showCreateModal}
        title="Thêm hồ sơ bệnh án"
        onCancel={() => setShowCreateModal(false)}
        footer={null}
        destroyOnClose
      >
        <Form
          form={createForm}
          layout="vertical"
          onFinish={handleCreateMedicalRecord}
        >
          <Form.Item
            label="Họ và tên"
            name="fullName"
            rules={[
              { required: true, message: "Họ và tên là bắt buộc" },
              {
                pattern: /^[a-zA-ZÀ-ỹ\s]+$/,
                message: "Chỉ được nhập chữ cái, không số hoặc ký tự đặc biệt!",
              },
            ]}
          >
            <Input placeholder="Nhập họ và tên" />
          </Form.Item>
          <Form.Item
            label="Ngày sinh"
            name="dateOfBirth"
            rules={[
              { required: true, message: "Ngày sinh là bắt buộc" },
              {
                validator: (_, value) => {
                  if (!value) return Promise.resolve();
                  if (value.isAfter(dayjs(), "day")) {
                    return Promise.reject("Ngày sinh không được ở tương lai!");
                  }
                  return Promise.resolve();
                },
              },
            ]}
          >
            <DatePicker
              format="YYYY-MM-DD"
              style={{ width: "100%" }}
              placeholder="Chọn ngày sinh"
              disabledDate={(current) =>
                current && current > dayjs().endOf("day")
              }
            />
          </Form.Item>
          <Form.Item
            label="Giới tính"
            name="gender"
            rules={[{ required: true, message: "Vui lòng chọn giới tính" }]}
          >
            <Select placeholder="Chọn giới tính">
              <Option value="1">Nam</Option>
              <Option value="0">Nữ</Option>
            </Select>
          </Form.Item>
          <Form.Item
            label="Số điện thoại"
            name="phoneNumber"
            rules={[
              { required: true, message: "Số điện thoại là bắt buộc" },
              {
                pattern: /^0\d{9}$/,
                message:
                  "Số điện thoại phải bắt đầu bằng 0 và gồm đúng 10 chữ số!",
              },
            ]}
          >
            <Input placeholder="Nhập số điện thoại" maxLength={10} />
          </Form.Item>
          <Form.Item
            label="Email"
            name="email"
            rules={[
              { required: true, message: "Email là bắt buộc" },
              { type: "email", message: "Email không hợp lệ" },
            ]}
          >
            <Input placeholder="Nhập email" />
          </Form.Item>
          <Form.Item
            label="CCCD/CMND"
            name="identityCard"
            rules={[
              { required: true, message: "CCCD/CMND là bắt buộc" },
              {
                pattern: /^\d{9}$|^\d{12}$/,
                message: "CCCD/CMND phải có 9 hoặc 12 chữ số hợp lệ!",
              },
            ]}
          >
            <Input placeholder="Nhập số CCCD/CMND" maxLength={12} />
          </Form.Item>
          <Form.Item
            label="Địa chỉ"
            name="address"
            rules={[{ required: true, message: "Địa chỉ là bắt buộc" }]}
          >
            <Input placeholder="Nhập địa chỉ" />
          </Form.Item>
          <Form.Item label="Số thẻ BHYT" name="healthInsurance">
            <Input placeholder="Nhập số thẻ BHYT (nếu có)" />
          </Form.Item>
          <div
            style={{
              display: "flex",
              justifyContent: "flex-end",
              gap: 12,
              marginTop: 16,
            }}
          >
            <button
              type="button"
              className="btn-cancel"
              onClick={() => setShowCreateModal(false)}
              disabled={isCreating}
            >
              Hủy
            </button>
            <button type="submit" className="btn-save" disabled={isCreating}>
              {isCreating ? "Đang xử lý..." : "Thêm hồ sơ"}
            </button>
          </div>
        </Form>
      </Modal>
    </div>
  );
}
