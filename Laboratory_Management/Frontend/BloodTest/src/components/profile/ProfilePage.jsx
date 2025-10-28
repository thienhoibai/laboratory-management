import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getUserData, setUserData as saveUserData } from "../../utils/auth";
import { Card, Button, Modal, Form, Input } from "antd";
import { toast } from "react-toastify";
import api from "../../configs/axios";
import "./ProfilePage.css";
import "./ChangePassword.css";
import { setAuthToken } from "../../utils/auth";
import { jwtDecode } from "jwt-decode";

const URL = "iam/api/Auth/change-password";

const mockUserData = {
  fullname: "Nguyễn Văn An",
  patientId: "550e8400",
  gender: "Nam",
  age: 40,
  email: "nguyenvanan@email.com",
  phone: "0912345678",
  birthday: "15 tháng 3, 1985",
  address: "123 Đường Lê Lợi, Quận 1, TP.HCM",
  idCard: "079085001234",
  healthInsurance: "BH-2024-001234",
  registrationDate: "15 tháng 1, 2024",
};

const initialFormData = {
  fullname: "",
  gender: "",
  birthday: "",
  phone: "",
  email: "",
  address: "",
  idCard: "",
  healthInsurance: "",
};

const ChangePasswordModal = ({ open, onClose }) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const token = localStorage.getItem("accessToken");
  const passwordPattern =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

  const handleSubmit = async (values) => {
    setLoading(true);
    setAuthToken(token);
    try {
      const response = await api.post(URL, {
        currentPassword: values.oldPassword,
        newPassword: values.newPassword,
      });

      if (response.status >= 200 && response.status < 300) {
        toast.success(response?.data?.message || "Đổi mật khẩu thành công!");
        form.resetFields();
        onClose();
      }
    } catch (err) {
      const serverMsg =
        (typeof err?.response?.data === "string" && err.response.data) ||
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        err?.message ||
        "Đổi mật khẩu thất bại.";
      toast.error(serverMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title="Đổi mật khẩu"
      open={open}
      onCancel={() => {
        form.resetFields();
        onClose();
      }}
      footer={null}
      className="cp-modal"
    >
      <Form form={form} layout="vertical" onFinish={handleSubmit}>
        <Form.Item
          label="Mật khẩu cũ"
          name="oldPassword"
          rules={[{ required: true, message: "Vui lòng nhập mật khẩu cũ" }]}
        >
          <Input.Password placeholder="Mật khẩu cũ" size="large" />
        </Form.Item>

        <Form.Item
          label="Mật khẩu mới"
          name="newPassword"
          rules={[
            { required: true, message: "Vui lòng nhập mật khẩu mới" },
            {
              validator: (_, value) => {
                if (!value) return Promise.reject();
                return passwordPattern.test(value)
                  ? Promise.resolve()
                  : Promise.reject(
                      new Error(
                        "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt"
                      )
                    );
              },
            },
          ]}
        >
          <Input.Password placeholder="Mật khẩu mới" size="large" />
        </Form.Item>
        <div className="cp-password-hint">
          Mật khẩu cần tối thiểu 8 ký tự và phải bao gồm chữ hoa, chữ thường,
          chữ số và ký tự đặc biệt.
        </div>

        <Form.Item
          label="Xác nhận mật khẩu mới"
          name="confirmPassword"
          dependencies={["newPassword"]}
          rules={[
            { required: true, message: "Vui lòng xác nhận mật khẩu mới" },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue("newPassword") === value) {
                  return Promise.resolve();
                }
                return Promise.reject(
                  new Error("Mật khẩu xác nhận không khớp")
                );
              },
            }),
          ]}
        >
          <Input.Password placeholder="Xác nhận mật khẩu mới" size="large" />
        </Form.Item>

        <Form.Item>
          <div className="cp-actions">
            <Button
              onClick={() => {
                form.resetFields();
                onClose();
              }}
              style={{ marginRight: 8 }}
            >
              Hủy
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              Lưu
            </Button>
          </div>
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default function ProfilePage() {
  const [userData, setUserData] = useState(mockUserData);
  const [activeTab, setActiveTab] = useState("personal");
  const [showModal, setShowModal] = useState(false);
  const [showMedicalRecordModal, setShowMedicalRecordModal] = useState(false);
  const [formData, setFormData] = useState(initialFormData);
  const [errors, setErrors] = useState({});
  const [openChange, setOpenChange] = useState(false);
  const navigate = useNavigate();
  const token = localStorage.getItem("accessToken");
  useEffect(() => {
    setAuthToken(token);
    // Load user data from localStorage or API
    const getProfile = async () => {
      let id = null;
      const decode = jwtDecode(token);
      id = decode.sub;
      const response = await api.get(`patient/v1/patients/${id}`);
      if (response.status === 200) {
        console.log(response.data);
      }
    };
    getProfile();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const parseBirthdayToInput = (birthday) => {
    if (!birthday) return "";

    // Parse from "DD tháng MM, YYYY" to "MM/DD/YYYY"
    const match = birthday.match(/(\d+) tháng (\d+), (\d+)/);
    if (match) {
      const [, day, month, year] = match;
      return `${month.padStart(2, "0")}/${day.padStart(2, "0")}/${year}`;
    }

    return "";
  };

  const handleOpenModal = () => {
    setFormData({
      fullname: userData.fullname,
      gender: userData.gender,
      birthday: parseBirthdayToInput(userData.birthday),
      phone: userData.phone,
      email: userData.email,
      address: userData.address,
      idCard: userData.idCard,
      healthInsurance: userData.healthInsurance,
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

    if (!formData.fullname.trim()) {
      newErrors.fullname = "Vui lòng nhập họ và tên";
    }

    if (!formData.phone.trim()) {
      newErrors.phone = "Vui lòng nhập số điện thoại";
    } else if (!/^0[3-9]\d{8}$/.test(formData.phone)) {
      newErrors.phone = "Số điện thoại không hợp lệ";
    }

    if (!formData.email.trim()) {
      newErrors.email = "Vui lòng nhập email";
    } else if (!/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(formData.email)) {
      newErrors.email = "Email không hợp lệ";
    }

    if (!formData.address.trim()) {
      newErrors.address = "Vui lòng nhập địa chỉ";
    }

    if (!formData.idCard.trim()) {
      newErrors.idCard = "Vui lòng nhập số CMND/CCCD";
    }

    if (!formData.healthInsurance.trim()) {
      newErrors.healthInsurance = "Vui lòng nhập số bảo hiểm y tế";
    }

    if (!formData.birthday.trim()) {
      newErrors.birthday = "Vui lòng nhập ngày sinh";
    } else {
      // Validate date format MM/DD/YYYY
      const dateRegex = /^(0[1-9]|1[0-2])\/(0[1-9]|[12][0-9]|3[01])\/\d{4}$/;
      if (!dateRegex.test(formData.birthday)) {
        newErrors.birthday = "Ngày sinh phải có định dạng MM/DD/YYYY";
      } else {
        // Check if date is valid
        const [month, day, year] = formData.birthday.split("/");
        const date = new Date(year, month - 1, day);
        if (
          date.getMonth() !== month - 1 ||
          date.getDate() !== parseInt(day) ||
          date.getFullYear() !== parseInt(year)
        ) {
          newErrors.birthday = "Ngày sinh không hợp lệ";
        } else {
          // Check if date is not in the future
          const today = new Date();
          if (date > today) {
            newErrors.birthday = "Ngày sinh không thể là ngày trong tương lai";
          }
        }
      }
    }

    return newErrors;
  };

  const calculateAge = (birthday) => {
    if (!birthday) return 0;

    // Parse birthday from MM/DD/YYYY format
    const [month, day, year] = birthday.split("/");
    const birthDate = new Date(year, month - 1, day);
    const today = new Date();

    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (
      monthDiff < 0 ||
      (monthDiff === 0 && today.getDate() < birthDate.getDate())
    ) {
      age--;
    }

    return age;
  };

  const formatBirthday = (birthday) => {
    if (!birthday) return "";

    // Parse from MM/DD/YYYY to "DD tháng MM, YYYY"
    const [month, day, year] = birthday.split("/");
    const monthNames = [
      "1",
      "2",
      "3",
      "4",
      "5",
      "6",
      "7",
      "8",
      "9",
      "10",
      "11",
      "12",
    ];

    return `${day} tháng ${monthNames[parseInt(month) - 1]}, ${year}`;
  };

  const handleSave = async () => {
    const newErrors = validateForm();
    setErrors(newErrors);

    if (Object.keys(newErrors).length !== 0) {
      return;
    }

    // Prepare data for backend
    const toISODate = (mmddyyyy) => {
      // expects MM/DD/YYYY -> returns YYYY-MM-DD
      const parts = mmddyyyy.split("/");
      if (parts.length !== 3) return null;
      const [mm, dd, yyyy] = parts;
      return `${yyyy.padStart(4, "0")}-${mm.padStart(2, "0")}-${dd.padStart(
        2,
        "0"
      )}`;
    };

    const mapGender = (g) => {
      if (!g) return 0;
      const lower = String(g).toLowerCase();
      if (lower.includes("nam") || lower === "male") return 1;
      if (lower.includes("nữ") || lower === "nu" || lower === "female")
        return 2;
      return 0;
    };

    // Get current user id from saved user data (if available)
    const savedUser = getUserData() || {};
    const userId =
      savedUser.userId ||
      savedUser.id ||
      savedUser.user_id ||
      savedUser.userIdFromToken ||
      null;

    // Build payload according to required JSON
    const payload = {
      fullName: formData.fullname,
      dateOfBirth: toISODate(formData.birthday),
      gender: mapGender(formData.gender),
      phone: formData.phone,
      email: formData.email,
      address: formData.address,
      idNumber: formData.idCard,
      insuranceNumber: formData.healthInsurance,
      userId: userId,
      createdChannel: "mobile_app",
    };

    // Basic validation for date conversion
    if (!payload.dateOfBirth) {
      toast.error(
        "Ngày sinh không hợp lệ. Vui lòng kiểm tra định dạng MM/DD/YYYY."
      );
      return;
    }

    try {
      // send token header and call API
      const token = localStorage.getItem("accessToken");
      let id = null;
      const decoded = jwtDecode(token);
      id = decoded.sub;
      console.log(id);

      setAuthToken(token);

      // Use PUT to update patient info (adjust to POST if your backend expects)
      const response = await api.put(`patient/v1/patients${id}`, payload);

      if (response?.status >= 200 && response?.status < 300) {
        const age = calculateAge(formData.birthday);
        const formattedBirthday = formatBirthday(formData.birthday);

        const updatedUser = {
          ...userData,
          fullname: formData.fullname,
          gender: formData.gender,
          phone: formData.phone,
          email: formData.email,
          address: formData.address,
          idCard: formData.idCard,
          healthInsurance: formData.healthInsurance,
          birthday: formattedBirthday,
          age: age,
        };

        setUserData(updatedUser); // update component state
        saveUserData(updatedUser); // persist to localStorage
        setShowModal(false);
        toast.success(
          response?.data?.message || "Cập nhật thông tin thành công!"
        );
      } else {
        const msg = response?.data?.message || "Cập nhật thất bại.";
        toast.error(msg);
      }
    } catch (err) {
      const serverMsg =
        (typeof err?.response?.data === "string" && err.response.data) ||
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        err?.message ||
        "Cập nhật thất bại.";
      toast.error(serverMsg);
    }
  };

  const getInitials = (name) => {
    return name
      .split(" ")
      .map((word) => word[0])
      .join("")
      .toUpperCase();
  };

  return (
    <div className="profile-page">
      {/* Header Section */}
      <div className="profile-header">
        <div className="profile-header-content">
          <div className="profile-avatar-section">
            <div className="profile-avatar">
              {getInitials(userData.fullname)}
            </div>
            <div className="profile-info">
              <h1 className="profile-name">{userData.fullname}</h1>
              <p className="profile-patient-id">
                Mã bệnh nhân: {userData.patientId}
              </p>
              <div className="profile-tags">
                <span className="profile-tag">{userData.gender}</span>
                <span className="profile-tag">{userData.age} tuổi</span>
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

      {/* Main Content */}
      <div className="profile-content">
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

        {activeTab === "personal" && (
          <div className="profile-tab-content">
            <div className="profile-section">
              <div className="profile-title">
                <div>
                  <h2 className="profile-section-title">Thông tin cá nhân</h2>
                  <p className="profile-section-subtitle">
                    Thông tin chi tiết về bệnh nhân
                  </p>
                </div>
                <Button
                  type="default"
                  onClick={() => setOpenChange(true)}
                  className="btn-change-password"
                >
                  Đổi mật khẩu
                </Button>
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
                      <span className="info-value">{userData.fullname}</span>
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
                      <span className="info-value">
                        {userData.birthday} ({userData.age} tuổi)
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
                      <span className="info-value">{userData.idCard}</span>
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
                        {userData.healthInsurance}
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
                        {userData.registrationDate}
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

        {activeTab === "medical" && (
          <div className="profile-tab-content">
            <div className="profile-section">
              <div className="medical-record-header-section">
                <div>
                  <h2 className="profile-section-title">Hồ sơ bệnh án</h2>
                  <p className="profile-section-subtitle">
                    Hồ sơ bệnh án của bệnh nhân (chỉ xem)
                  </p>
                </div>
                <button
                  className="create-record-btn"
                  onClick={() => setShowMedicalRecordModal(true)}
                >
                  <svg
                    className="plus-icon"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <line x1="12" y1="5" x2="12" y2="19" />
                    <line x1="5" y1="12" x2="19" y2="12" />
                  </svg>
                  Tạo hồ sơ mới
                </button>
              </div>

              <div className="medical-records-list">
                <div className="medical-record-card">
                  <div className="medical-record-header">
                    <h3 className="medical-record-title">
                      Hồ sơ bệnh án #MR001
                    </h3>
                    <span className="medical-record-status">
                      Đang hoạt động
                    </span>
                  </div>

                  <div className="medical-record-dates">
                    <span className="medical-record-date">
                      Ngày tạo: 15/1/2024
                    </span>
                    <span className="medical-record-separator">•</span>
                    <span className="medical-record-date">
                      Cập nhật: 10/3/2024
                    </span>
                  </div>

                  <div className="medical-record-patient-info">
                    <div className="medical-record-patient-details">
                      <div className="medical-record-patient-column">
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Họ tên:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.fullname}
                          </span>
                        </div>
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Ngày sinh:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.birthday}
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
                            {userData.gender}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <button
                    className="medical-record-view-btn"
                    onClick={() => navigate("/medical-record")}
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
                    Xem chi tiết
                  </button>
                </div>

                <div className="medical-record-card">
                  <div className="medical-record-header">
                    <h3 className="medical-record-title">
                      Hồ sơ bệnh án #MR002
                    </h3>
                    <span className="medical-record-status status-archived">
                      Lưu trữ
                    </span>
                  </div>

                  <div className="medical-record-dates">
                    <span className="medical-record-date">
                      Ngày tạo: 20/12/2023
                    </span>
                    <span className="medical-record-separator">•</span>
                    <span className="medical-record-date">
                      Cập nhật: 5/2/2024
                    </span>
                  </div>

                  <div className="medical-record-patient-info">
                    <div className="medical-record-patient-details">
                      <div className="medical-record-patient-column">
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Họ tên:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.fullname}
                          </span>
                        </div>
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Ngày sinh:
                          </span>
                          <span className="medical-record-patient-value">
                            15/3/1985
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
                            {userData.gender}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <button
                    className="medical-record-view-btn"
                    onClick={() => navigate("/medical-record")}
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
                    Xem chi tiết
                  </button>
                </div>

                <div className="medical-record-card">
                  <div className="medical-record-header">
                    <h3 className="medical-record-title">
                      Hồ sơ bệnh án #MR003
                    </h3>
                    <span className="medical-record-status status-archived">
                      Lưu trữ
                    </span>
                  </div>

                  <div className="medical-record-dates">
                    <span className="medical-record-date">
                      Ngày tạo: 10/10/2023
                    </span>
                    <span className="medical-record-separator">•</span>
                    <span className="medical-record-date">
                      Cập nhật: 15/11/2023
                    </span>
                  </div>

                  <div className="medical-record-patient-info">
                    <div className="medical-record-patient-details">
                      <div className="medical-record-patient-column">
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Họ tên:
                          </span>
                          <span className="medical-record-patient-value">
                            {userData.fullname}
                          </span>
                        </div>
                        <div className="medical-record-patient-item">
                          <span className="medical-record-patient-label">
                            Ngày sinh:
                          </span>
                          <span className="medical-record-patient-value">
                            15/3/1985
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
                            {userData.gender}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <button
                    className="medical-record-view-btn"
                    onClick={() => navigate("/medical-record")}
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
                    Xem chi tiết
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Update Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
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

            <div className="modal-body">
              <div className="form-row">
                <div className="form-group">
                  <label>Họ và tên</label>
                  <input
                    type="text"
                    name="fullname"
                    value={formData.fullname}
                    onChange={handleInputChange}
                    className={errors.fullname ? "error" : ""}
                  />
                  {errors.fullname && (
                    <span className="error-text">{errors.fullname}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Giới tính</label>
                  <select
                    name="gender"
                    value={formData.gender}
                    onChange={handleInputChange}
                  >
                    <option value="Nam">Nam</option>
                    <option value="Nữ">Nữ</option>
                    <option value="Khác">Khác</option>
                  </select>
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Ngày sinh</label>
                  <div className="date-input-wrapper">
                    <input
                      type="text"
                      name="birthday"
                      value={formData.birthday}
                      onChange={handleInputChange}
                      placeholder="MM/DD/YYYY"
                      className={errors.birthday ? "error" : ""}
                    />
                    <svg
                      className="calendar-icon"
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
                  </div>
                  {errors.birthday && (
                    <span className="error-text">{errors.birthday}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Số điện thoại</label>
                  <input
                    type="text"
                    name="phone"
                    value={formData.phone}
                    onChange={handleInputChange}
                    className={errors.phone ? "error" : ""}
                  />
                  {errors.phone && (
                    <span className="error-text">{errors.phone}</span>
                  )}
                </div>
              </div>

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

              <div className="form-row">
                <div className="form-group">
                  <label>Số CMND/CCCD</label>
                  <input
                    type="text"
                    name="idCard"
                    value={formData.idCard}
                    onChange={handleInputChange}
                    className={errors.idCard ? "error" : ""}
                  />
                  {errors.idCard && (
                    <span className="error-text">{errors.idCard}</span>
                  )}
                </div>

                <div className="form-group">
                  <label>Số bảo hiểm y tế</label>
                  <input
                    type="text"
                    name="healthInsurance"
                    value={formData.healthInsurance}
                    onChange={handleInputChange}
                    className={errors.healthInsurance ? "error" : ""}
                  />
                  {errors.healthInsurance && (
                    <span className="error-text">{errors.healthInsurance}</span>
                  )}
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

      <ChangePasswordModal
        open={openChange}
        onClose={() => setOpenChange(false)}
      />

      {/* Create Medical Record Modal */}
      {showMedicalRecordModal && (
        <div
          className="modal-overlay"
          onClick={() => setShowMedicalRecordModal(false)}
        >
          <div
            className="modal-content medical-record-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2 className="modal-title">Tạo hồ sơ bệnh án mới</h2>
              <p className="modal-subtitle">
                Điền thông tin để tạo hồ sơ bệnh án mới
              </p>
              <button
                className="modal-close"
                onClick={() => setShowMedicalRecordModal(false)}
              >
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

            <div className="modal-body">
              <div className="form-group">
                <label>Họ và tên bệnh nhân</label>
                <input
                  type="text"
                  value={userData.fullname}
                  disabled
                  className="disabled-input"
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Mã bệnh nhân</label>
                  <input
                    type="text"
                    value={userData.patientId}
                    disabled
                    className="disabled-input"
                  />
                </div>

                <div className="form-group">
                  <label>Giới tính</label>
                  <input
                    type="text"
                    value={userData.gender}
                    disabled
                    className="disabled-input"
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Ngày sinh</label>
                  <input
                    type="text"
                    value={userData.birthday}
                    disabled
                    className="disabled-input"
                  />
                </div>

                <div className="form-group">
                  <label>Số điện thoại</label>
                  <input
                    type="text"
                    value={userData.phone}
                    disabled
                    className="disabled-input"
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Lý do tạo hồ sơ</label>
                <textarea
                  rows="4"
                  placeholder="Nhập lý do tạo hồ sơ bệnh án..."
                  className="form-textarea"
                />
              </div>

              <div className="form-group">
                <label>Ghi chú (tùy chọn)</label>
                <textarea
                  rows="3"
                  placeholder="Thêm ghi chú nếu cần..."
                  className="form-textarea"
                />
              </div>
            </div>

            <div className="modal-footer">
              <button
                className="btn-cancel"
                onClick={() => setShowMedicalRecordModal(false)}
              >
                Hủy
              </button>
              <button
                className="btn-save"
                onClick={() => {
                  toast.success("Tạo hồ sơ bệnh án thành công!");
                  setShowMedicalRecordModal(false);
                }}
              >
                Tạo hồ sơ
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
