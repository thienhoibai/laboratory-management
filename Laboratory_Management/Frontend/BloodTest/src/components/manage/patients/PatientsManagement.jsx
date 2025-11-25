import React, { useEffect, useState } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./PatientsManagement.css";
import api from "../../../configs/axios";
import { setAuthToken } from "../../../utils/auth";

const PatientsManagement = () => {
  const [patients, setPatients] = useState([]);
  const [showModal, setShowModal] = useState(false);
  const [modalMode, setModalMode] = useState("add"); // "add" or "edit"
  // eslint-disable-next-line no-unused-vars
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [formData, setFormData] = useState({
    fullName: "",
    phone: "",
    email: "",
    address: "",
    idNumber: "",
    insuranceNumber: "",
    gender: "Nam",
    dateOfBirth: "",
  });

  // TODO: Gọi API để lấy danh sách bệnh nhân
  useEffect(() => {
    fetchPatients();
  }, []);

  const fetchPatients = async () => {
    try {
      const token = localStorage.getItem("accessToken");
      setAuthToken(token);
      const response = await api.get("patient/v1/patients/mine");
      console.log("Fetched patients:", response.data);
      if (response.status === 200 && response.data) {
        // Xử lý dữ liệu trả về - có thể là response.data.data hoặc response.data
        const patientData = Array.isArray(response.data.data)
          ? response.data.data
          : Array.isArray(response.data)
          ? response.data
          : [];
        setPatients(patientData);
      }
    } catch (error) {
      console.error("Error fetching patients:", error);
      setPatients([]); // Set về mảng rỗng khi có lỗi
    }
  };

  const handleOpenAddModal = () => {
    setModalMode("add");
    setSelectedPatient(null);
    setFormData({
      fullName: "",
      phone: "",
      email: "",
      address: "",
      idNumber: "",
      insuranceNumber: "",
      gender: "Nam",
      dateOfBirth: "",
    });
    setShowModal(true);
  };

  const handleOpenEditModal = (patient) => {
    setModalMode("edit");
    setSelectedPatient(patient);
    setFormData({
      fullName: patient.fullName || "",
      phone: patient.phone || "",
      email: patient.email || "",
      address: patient.address || "",
      idNumber: patient.idNumber || "",
      insuranceNumber: patient.insuranceNumber || "",
      gender: patient.gender || "Nam",
      dateOfBirth: patient.dateOfBirth || "",
    });
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedPatient(null);
    setFormData({
      fullName: "",
      phone: "",
      email: "",
      address: "",
      idNumber: "",
      insuranceNumber: "",
      gender: "Nam",
      dateOfBirth: "",
    });
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // TODO: Gọi API để thêm/sửa bệnh nhân
    if (modalMode === "add") {
      // const response = await api.post('/patients', formData);
      console.log("Add patient:", formData);
    } else {
      // const response = await api.put(`/patients/${selectedPatient.id}`, formData);
      console.log("Update patient:", formData);
    }

    handleCloseModal();
    // fetchPatients(); // Reload danh sách
  };

  const handleDelete = async (patientId) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa bệnh nhân này?")) {
      // TODO: Gọi API để xóa bệnh nhân
      // await api.delete(`/patients/${patientId}`);
      console.log("Delete patient:", patientId);
      // fetchPatients(); // Reload danh sách
    }
  };

  return (
    <AdminLayout pageTitle="Quản lý bệnh nhân">
      <div className="patients-management">
        <div className="patients-header">
          <button className="btn-add-patient" onClick={handleOpenAddModal}>
            <svg
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Thêm bệnh nhân
          </button>
        </div>

        <div className="patients-table-wrapper">
          <table className="patients-table">
            <thead>
              <tr>
                <th>Họ và tên</th>
                <th>SĐT</th>
                <th>Email</th>
                <th>Địa chỉ</th>
                <th>Số CCCD</th>
                <th>Số BHYT</th>
                <th>Giới tính</th>
                <th>Ngày sinh</th>
                <th>Hành động</th>
              </tr>
            </thead>
            <tbody>
              {patients && patients.length > 0 ? (
                patients.map((patient) => (
                  <tr key={patient.id}>
                    <td className="patient-name">{patient.fullName}</td>
                    <td>{patient.phone}</td>
                    <td className="patient-email">{patient.email}</td>
                    <td>{patient.address}</td>
                    <td>{patient.idNumber}</td>
                    <td>{patient.insuranceNumber}</td>
                    <td>{patient.gender}</td>
                    <td>{patient.dateOfBirth}</td>
                    <td>
                      <div className="action-buttons">
                        <button
                          className="btn-edit"
                          onClick={() => handleOpenEditModal(patient)}
                          title="Chỉnh sửa"
                        >
                          <svg
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                            <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                          </svg>
                        </button>
                        <button
                          className="btn-delete"
                          onClick={() => handleDelete(patient.id)}
                          title="Xóa"
                        >
                          <svg
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <polyline points="3 6 5 6 21 6" />
                            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                            <line x1="10" y1="11" x2="10" y2="17" />
                            <line x1="14" y1="11" x2="14" y2="17" />
                          </svg>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="9" className="empty-state">
                    Chưa có dữ liệu bệnh nhân. Vui lòng gọi API để tải dữ liệu.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Modal thêm/sửa bệnh nhân */}
        {showModal && (
          <div className="modal-overlay" onClick={handleCloseModal}>
            <div
              className="modal-content patients-modal"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="modal-header">
                <div>
                  <h2 className="modal-title">
                    {modalMode === "add"
                      ? "Thêm bệnh nhân mới"
                      : "Chỉnh sửa bệnh nhân"}
                  </h2>
                  <p className="modal-subtitle">
                    {modalMode === "add"
                      ? "Điền thông tin bệnh nhân mới"
                      : "Cập nhật thông tin bệnh nhân"}
                  </p>
                </div>
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

              <form onSubmit={handleSubmit} className="modal-body">
                <div className="form-group">
                  <label>
                    Họ và tên <span className="required">*</span>
                  </label>
                  <input
                    type="text"
                    name="fullName"
                    value={formData.fullName}
                    onChange={handleInputChange}
                    required
                    placeholder="Nhập họ và tên"
                  />
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>
                      Số điện thoại <span className="required">*</span>
                    </label>
                    <input
                      type="tel"
                      name="phone"
                      value={formData.phone}
                      onChange={handleInputChange}
                      required
                      placeholder="0901234567"
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Email <span className="required">*</span>
                    </label>
                    <input
                      type="email"
                      name="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      required
                      placeholder="nguyen@email.com"
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>
                    Địa chỉ <span className="required">*</span>
                  </label>
                  <input
                    type="text"
                    name="address"
                    value={formData.address}
                    onChange={handleInputChange}
                    required
                    placeholder="123 Đường Lê Lợi, Quận 1, TP.HCM"
                  />
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>
                      Số CCCD <span className="required">*</span>
                    </label>
                    <input
                      type="text"
                      name="idNumber"
                      value={formData.idNumber}
                      onChange={handleInputChange}
                      required
                      placeholder="123456789012"
                    />
                  </div>
                  <div className="form-group">
                    <label>Số BHYT</label>
                    <input
                      type="text"
                      name="insuranceNumber"
                      value={formData.insuranceNumber}
                      onChange={handleInputChange}
                      placeholder="BH123456"
                    />
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>
                      Giới tính <span className="required">*</span>
                    </label>
                    <select
                      name="gender"
                      value={formData.gender}
                      onChange={handleInputChange}
                      required
                    >
                      <option value="Nam">Nam</option>
                      <option value="Nữ">Nữ</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <label>
                      Ngày sinh <span className="required">*</span>
                    </label>
                    <input
                      type="date"
                      name="dateOfBirth"
                      value={formData.dateOfBirth}
                      onChange={handleInputChange}
                      required
                    />
                  </div>
                </div>

                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn-cancel"
                    onClick={handleCloseModal}
                  >
                    Hủy
                  </button>
                  <button type="submit" className="btn-save">
                    {modalMode === "add" ? "Thêm mới" : "Cập nhật"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
};

export default PatientsManagement;
