import React, { useState, useMemo, useEffect } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import { FiSearch, FiEdit2, FiTrash2, FiPlus, FiX } from "react-icons/fi";
import api from "../../../configs/axios";
import { setAuthToken } from "../../../utils/auth";
import { toast } from "react-toastify";
import "./ParameterManagement.css";

const endPoint = "testorder/api/TestParameter";

const ParameterManagement = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const [parameters, setParameters] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState("create"); // 'create' or 'edit'
  // eslint-disable-next-line no-unused-vars
  const [selectedParameter, setSelectedParameter] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Form state
  const [formData, setFormData] = useState({
    parameterName: "",
    referenceRange: "",
    unit: "",
  });

  // Fetch parameters from API
  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (token) setAuthToken(token);
    fetchParameters();
  }, []);

  const fetchParameters = async () => {
    setIsLoading(true);
    try {
      const response = await api.get(endPoint);
      if (response.status === 200) {
        const data = response.data;
        // Handle different response structures
        const parametersList = Array.isArray(data)
          ? data
          : data.data || data.items || [];
        setParameters(parametersList);
      }
    } catch (error) {
      console.error("Error fetching parameters:", error);
      toast.error("Không thể tải danh sách chỉ số xét nghiệm");
    } finally {
      setIsLoading(false);
    }
  };

  // Filter parameters
  const filteredParameters = useMemo(() => {
    if (!searchQuery) return parameters;
    const query = searchQuery.toLowerCase();
    return parameters.filter(
      (param) =>
        param.parameterName?.toLowerCase().includes(query) ||
        param.unit?.toLowerCase().includes(query)
    );
  }, [parameters, searchQuery]);

  // Handle open modal for create
  const handleOpenCreateModal = () => {
    setModalMode("create");
    setFormData({
      parameterName: "",
      referenceRange: "",
      unit: "",
    });
    setSelectedParameter(null);
    setIsModalOpen(true);
  };

  // Handle open modal for edit
  const handleOpenEditModal = (param) => {
    setModalMode("edit");
    setFormData({
      parameterName: param.parameterName || "",
      referenceRange: param.referenceRange || "",
      unit: param.unit || "",
    });
    setSelectedParameter(param);
    setIsModalOpen(true);
  };

  // Handle close modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setFormData({
      parameterName: "",
      referenceRange: "",
      unit: "",
    });
    setSelectedParameter(null);
  };

  // Handle form input change
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  // Handle save parameter
  const handleSaveParameter = async () => {
    if (!formData.parameterName || !formData.referenceRange || !formData.unit) {
      toast.error("Vui lòng điền đầy đủ thông tin!");
      return;
    }

    // Validate referenceRange format (should be like "7 - 56" or "7-56")
    const rangePattern = /^\s*(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)\s*$/;
    const match = formData.referenceRange.trim().match(rangePattern);
    if (!match) {
      toast.error("Khoảng tham chiếu không đúng định dạng! (VD: 7 - 56)");
      return;
    }

    const min = parseFloat(match[1]);
    const max = parseFloat(match[2]);
    if (min >= max) {
      toast.error("Giá trị tối thiểu phải nhỏ hơn giá trị tối đa!");
      return;
    }

    setIsSubmitting(true);

    try {
      if (modalMode === "create") {
        // Check if parameterName already exists
        if (
          parameters.find((p) => p.parameterName === formData.parameterName)
        ) {
          toast.error("Tên chỉ số đã tồn tại!");
          setIsSubmitting(false);
          return;
        }

        const payload = {
          parameterName: formData.parameterName.trim(),
          referenceRange: formData.referenceRange.trim(),
          unit: formData.unit.trim(),
        };

        const response = await api.post(endPoint, payload);

        if (response.status === 200 || response.status === 201) {
          toast.success("Thêm chỉ số xét nghiệm thành công!");
          await fetchParameters(); // Refresh the list
          handleCloseModal();
        }
      } else {
        // Edit mode - will be implemented later if needed
        toast.info("Chức năng chỉnh sửa đang được phát triển");
        setIsSubmitting(false);
      }
    } catch (error) {
      console.error("Error saving parameter:", error);
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "Không thể lưu chỉ số xét nghiệm";
      toast.error(errorMessage);
      setIsSubmitting(false);
    }
  };

  // Handle delete parameter
  const handleDeleteParameter = (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa chỉ số xét nghiệm này?")) {
      setParameters(parameters.filter((param) => param.parameterId !== id));
    }
  };

  return (
    <AdminLayout
      pageTitle="Quản lý chỉ số xét nghiệm"
      breadcrumbs={[
        { name: "Tổng quan", link: "/admin/dashboard" },
        { name: "Quản lý chỉ số xét nghiệm" },
      ]}
    >
      <div className="parameters-container">
        <div className="parameters-header">
          <div className="parameters-header-left">
            <h1>Quản lý chỉ số xét nghiệm</h1>
            <p>Quản lý các chỉ số và thông số xét nghiệm</p>
          </div>
          <button
            className="add-parameter-button"
            onClick={handleOpenCreateModal}
          >
            <FiPlus size={20} />
            <span>Thêm chỉ số xét nghiệm</span>
          </button>
        </div>

        <div className="parameters-content">
          {/* Search Section */}
          <div className="search-section">
            <div className="search-box">
              <FiSearch size={18} />
              <input
                type="text"
                placeholder="Tìm kiếm theo tên chỉ số hoặc đơn vị..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>

          {/* Parameters Table */}
          <div className="parameters-table-container">
            {isLoading ? (
              <div className="loading-container">
                <div className="loading-spinner"></div>
                <p>Đang tải dữ liệu...</p>
              </div>
            ) : (
              <table className="parameters-table">
                <thead>
                  <tr>
                    <th>Tên chỉ số</th>
                    <th>Khoảng tham chiếu</th>
                    <th>Đơn vị</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredParameters.length > 0 ? (
                    filteredParameters.map((param) => (
                      <tr key={param.parameterId}>
                        <td>
                          <span className="parameter-name">
                            {param.parameterName}
                          </span>
                        </td>
                        <td>
                          <span className="normal-range">
                            {param.referenceRange}
                          </span>
                        </td>
                        <td>{param.unit}</td>
                        <td>
                          <div className="action-buttons">
                            <button
                              className="action-button edit"
                              onClick={() => handleOpenEditModal(param)}
                              title="Chỉnh sửa"
                            >
                              <FiEdit2 size={18} />
                            </button>
                            <button
                              className="action-button delete"
                              onClick={() =>
                                handleDeleteParameter(param.parameterId)
                              }
                              title="Xóa"
                            >
                              <FiTrash2 size={18} />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td
                        colSpan="4"
                        style={{ textAlign: "center", padding: "40px" }}
                      >
                        {searchQuery
                          ? "Không tìm thấy chỉ số xét nghiệm nào"
                          : "Chưa có chỉ số xét nghiệm nào"}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>

      {/* Parameter Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="parameter-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>
                {modalMode === "create"
                  ? "Thêm chỉ số xét nghiệm"
                  : "Chỉnh sửa chỉ số xét nghiệm"}
              </h2>
              <button className="modal-close" onClick={handleCloseModal}>
                <FiX size={20} />
              </button>
            </div>

            <div className="modal-body">
              {/* Row 1: Parameter Name - full width */}
              <div className="form-group">
                <label className="form-label">Tên chỉ số</label>
                <input
                  type="text"
                  name="parameterName"
                  className="form-input"
                  placeholder="VD: ALT (SGPT)"
                  value={formData.parameterName}
                  onChange={handleInputChange}
                />
              </div>

              {/* Row 2: Reference Range and Unit - 2 columns */}
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Khoảng tham chiếu</label>
                  <input
                    type="text"
                    name="referenceRange"
                    className="form-input"
                    placeholder="VD: 7 - 56"
                    value={formData.referenceRange}
                    onChange={handleInputChange}
                  />
                  <small
                    style={{
                      color: "#6b7280",
                      fontSize: "12px",
                      marginTop: "4px",
                      display: "block",
                    }}
                  >
                    Định dạng: số - số (VD: 7 - 56)
                  </small>
                </div>

                <div className="form-group">
                  <label className="form-label">Đơn vị</label>
                  <input
                    type="text"
                    name="unit"
                    className="form-input"
                    placeholder="VD: U/L"
                    value={formData.unit}
                    onChange={handleInputChange}
                  />
                </div>
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
                onClick={handleSaveParameter}
                disabled={isSubmitting}
              >
                {isSubmitting
                  ? "Đang xử lý..."
                  : modalMode === "create"
                  ? "Thêm mới"
                  : "Cập nhật"}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default ParameterManagement;
