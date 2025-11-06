import React, { useState, useMemo } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import { FiSearch, FiEdit2, FiTrash2, FiPlus, FiX } from "react-icons/fi";
import { mockParameters } from "../../../data/parameter";
import "./ParameterManagement.css";

const ParameterManagement = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const [parameters, setParameters] = useState(mockParameters);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState("create"); // 'create' or 'edit'
  const [selectedParameter, setSelectedParameter] = useState(null);

  // Form state
  const [formData, setFormData] = useState({
    code: "",
    name: "",
    normalRangeMin: "",
    normalRangeMax: "",
    unit: "",
    description: "",
  });

  // Filter parameters
  const filteredParameters = useMemo(() => {
    if (!searchQuery) return parameters;
    const query = searchQuery.toLowerCase();
    return parameters.filter(
      (param) =>
        param.code.toLowerCase().includes(query) ||
        param.name.toLowerCase().includes(query)
    );
  }, [parameters, searchQuery]);

  // Handle open modal for create
  const handleOpenCreateModal = () => {
    setModalMode("create");
    setFormData({
      code: "",
      name: "",
      normalRangeMin: "",
      normalRangeMax: "",
      unit: "",
      description: "",
    });
    setSelectedParameter(null);
    setIsModalOpen(true);
  };

  // Handle open modal for edit
  const handleOpenEditModal = (param) => {
    setModalMode("edit");
    setFormData({
      code: param.code,
      name: param.name,
      normalRangeMin: param.normalRangeMin,
      normalRangeMax: param.normalRangeMax,
      unit: param.unit,
      description: param.description,
    });
    setSelectedParameter(param);
    setIsModalOpen(true);
  };

  // Handle close modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setFormData({
      code: "",
      name: "",
      normalRangeMin: "",
      normalRangeMax: "",
      unit: "",
      description: "",
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
  const handleSaveParameter = () => {
    if (
      !formData.code ||
      !formData.name ||
      !formData.normalRangeMin ||
      !formData.normalRangeMax ||
      !formData.unit
    ) {
      alert("Vui lòng điền đầy đủ thông tin!");
      return;
    }

    if (
      parseFloat(formData.normalRangeMin) >= parseFloat(formData.normalRangeMax)
    ) {
      alert("Giá trị tối thiểu phải nhỏ hơn giá trị tối đa!");
      return;
    }

    if (modalMode === "create") {
      // Check if code already exists
      if (parameters.find((p) => p.code === formData.code)) {
        alert("Mã chỉ số đã tồn tại!");
        return;
      }

      const newParameter = {
        id: parameters.length + 1,
        code: formData.code,
        name: formData.name,
        normalRangeMin: parseFloat(formData.normalRangeMin),
        normalRangeMax: parseFloat(formData.normalRangeMax),
        unit: formData.unit,
        description: formData.description,
        createdAt: new Date().toISOString().split("T")[0],
      };
      setParameters([...parameters, newParameter]);
    } else {
      setParameters(
        parameters.map((param) =>
          param.id === selectedParameter.id
            ? {
                ...param,
                code: formData.code,
                name: formData.name,
                normalRangeMin: parseFloat(formData.normalRangeMin),
                normalRangeMax: parseFloat(formData.normalRangeMax),
                unit: formData.unit,
                description: formData.description,
              }
            : param
        )
      );
    }

    handleCloseModal();
  };

  // Handle delete parameter
  const handleDeleteParameter = (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa chỉ số xét nghiệm này?")) {
      setParameters(parameters.filter((param) => param.id !== id));
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
                placeholder="Tìm kiếm theo mã hoặc tên chỉ số..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>

          {/* Parameters Table */}
          <div className="parameters-table-container">
            <table className="parameters-table">
              <thead>
                <tr>
                  <th>Mã</th>
                  <th>Tên chỉ số</th>
                  <th>Khoảng bình thường</th>
                  <th>Đơn vị</th>
                  <th>Mô tả</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredParameters.length > 0 ? (
                  filteredParameters.map((param) => (
                    <tr key={param.id}>
                      <td>
                        <span className="parameter-code">{param.code}</span>
                      </td>
                      <td>
                        <span className="parameter-name">{param.name}</span>
                      </td>
                      <td>
                        <span className="normal-range">
                          {param.normalRangeMin}-{param.normalRangeMax}
                        </span>
                      </td>
                      <td>{param.unit}</td>
                      <td>{param.description}</td>
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
                            onClick={() => handleDeleteParameter(param.id)}
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
                      colSpan="6"
                      style={{ textAlign: "center", padding: "40px" }}
                    >
                      Không tìm thấy chỉ số xét nghiệm nào
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
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
              {/* Row 1: Code and Unit - 2 columns */}
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Mã chỉ số</label>
                  <input
                    type="text"
                    name="code"
                    className="form-input"
                    placeholder="VD: WBC"
                    value={formData.code}
                    onChange={handleInputChange}
                    disabled={modalMode === "edit"}
                    style={
                      modalMode === "edit"
                        ? { background: "#f5f5f5", cursor: "not-allowed" }
                        : {}
                    }
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Đơn vị</label>
                  <input
                    type="text"
                    name="unit"
                    className="form-input"
                    placeholder="VD: 10^3/μL"
                    value={formData.unit}
                    onChange={handleInputChange}
                  />
                </div>
              </div>

              {/* Row 2: Name - full width */}
              <div className="form-group">
                <label className="form-label">Tên chỉ số</label>
                <input
                  type="text"
                  name="name"
                  className="form-input"
                  placeholder="VD: White Blood Cell"
                  value={formData.name}
                  onChange={handleInputChange}
                />
              </div>

              {/* Row 3: Normal Range - 2 columns */}
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">Giá trị tối thiểu</label>
                  <input
                    type="number"
                    name="normalRangeMin"
                    className="form-input"
                    placeholder="VD: 4.5"
                    step="0.1"
                    value={formData.normalRangeMin}
                    onChange={handleInputChange}
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Giá trị tối đa</label>
                  <input
                    type="number"
                    name="normalRangeMax"
                    className="form-input"
                    placeholder="VD: 11"
                    step="0.1"
                    value={formData.normalRangeMax}
                    onChange={handleInputChange}
                  />
                </div>
              </div>

              {/* Row 4: Description - full width */}
              <div className="form-group">
                <label className="form-label">Mô tả</label>
                <textarea
                  name="description"
                  className="form-textarea"
                  placeholder="VD: Số lượng tế bào máu trắng"
                  value={formData.description}
                  onChange={handleInputChange}
                />
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
              >
                {modalMode === "create" ? "Thêm mới" : "Cập nhật"}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default ParameterManagement;
