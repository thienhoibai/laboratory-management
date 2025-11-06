import React, { useState, useMemo } from "react";
import AdminLayout from "../../../components/admin/layout/AdminLayout";
import { FiSearch, FiEdit2, FiTrash2, FiPlus, FiX } from "react-icons/fi";
import { mockCatalogs, availableParameters } from "../../../data/catalogTest";
import "./AdminCatalogsPage.css";

const AdminCatalogsPage = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const [catalogs, setCatalogs] = useState(mockCatalogs);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState("create"); // 'create' or 'edit'
  const [selectedCatalog, setSelectedCatalog] = useState(null);

  // Form state
  const [formData, setFormData] = useState({
    name: "",
    category: "",
    price: "",
    status: "Hoạt động",
    parameters: [],
    description: "",
  });

  // Filter catalogs
  const filteredCatalogs = useMemo(() => {
    if (!searchQuery) return catalogs;
    const query = searchQuery.toLowerCase();
    return catalogs.filter(
      (catalog) =>
        catalog.name.toLowerCase().includes(query) ||
        catalog.category.toLowerCase().includes(query)
    );
  }, [catalogs, searchQuery]);

  // Handle open modal for create
  const handleOpenCreateModal = () => {
    setModalMode("create");
    setFormData({
      name: "",
      category: "",
      price: "",
      status: "Hoạt động",
      parameters: [],
      description: "",
    });
    setSelectedCatalog(null);
    setIsModalOpen(true);
  };

  // Handle open modal for edit
  const handleOpenEditModal = (catalog) => {
    setModalMode("edit");
    setFormData({
      name: catalog.name,
      category: catalog.category,
      price: catalog.price,
      status: catalog.status,
      parameters: [...catalog.parameters],
      description: catalog.description,
    });
    setSelectedCatalog(catalog);
    setIsModalOpen(true);
  };

  // Handle close modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setFormData({
      name: "",
      category: "",
      price: "",
      status: "Hoạt động",
      parameters: [],
      description: "",
    });
    setSelectedCatalog(null);
  };

  // Handle form input change
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  // Handle add parameter
  const handleAddParameter = (parameter) => {
    setFormData((prev) => ({
      ...prev,
      parameters: [...prev.parameters, parameter],
    }));
  };

  // Handle remove parameter
  const handleRemoveParameter = (parameterId) => {
    setFormData((prev) => ({
      ...prev,
      parameters: prev.parameters.filter((p) => p.id !== parameterId),
    }));
  };

  // Handle save catalog
  const handleSaveCatalog = () => {
    if (!formData.name || !formData.category || !formData.price) {
      alert("Vui lòng điền đầy đủ thông tin!");
      return;
    }

    if (formData.parameters.length === 0) {
      alert("Vui lòng chọn ít nhất một chỉ số xét nghiệm!");
      return;
    }

    if (modalMode === "create") {
      const newCatalog = {
        id: catalogs.length + 1,
        name: formData.name,
        category: formData.category,
        price: parseInt(formData.price),
        status: formData.status,
        parameters: formData.parameters,
        description: formData.description,
      };
      setCatalogs([...catalogs, newCatalog]);
    } else {
      setCatalogs(
        catalogs.map((catalog) =>
          catalog.id === selectedCatalog.id
            ? {
                ...catalog,
                name: formData.name,
                category: formData.category,
                price: parseInt(formData.price),
                status: formData.status,
                parameters: formData.parameters,
                description: formData.description,
              }
            : catalog
        )
      );
    }

    handleCloseModal();
  };

  // Handle delete catalog
  const handleDeleteCatalog = (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa mục xét nghiệm này?")) {
      setCatalogs(catalogs.filter((catalog) => catalog.id !== id));
    }
  };

  return (
    <AdminLayout
      pageTitle="Quản lý mục xét nghiệm"
      breadcrumbs={[
        { name: "Tổng quan", link: "/admin/dashboard" },
        { name: "Quản lý mục xét nghiệm" },
      ]}
    >
      <div className="catalogs-container">
        <div className="catalogs-header">
          <div className="catalogs-header-left">
            <h1>Quản lý mục xét nghiệm</h1>
            <p>Quản lý các danh mục xét nghiệm và chỉ số</p>
          </div>
          <button
            className="add-catalog-button"
            onClick={handleOpenCreateModal}
          >
            <FiPlus size={20} />
            <span>Thêm mục xét nghiệm</span>
          </button>
        </div>

        <div className="catalogs-content">
          {/* Search Section */}
          <div className="search-section">
            <div className="search-box">
              <FiSearch size={18} />
              <input
                type="text"
                placeholder="Tìm kiếm theo tên hoặc danh mục..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>

          {/* Catalogs Table */}
          <div className="catalogs-table-container">
            <table className="catalogs-table">
              <thead>
                <tr>
                  <th>Tên mục xét nghiệm</th>
                  <th>Danh mục</th>
                  <th>Chỉ số xét nghiệm</th>
                  <th>Giá</th>
                  <th>Trạng thái</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredCatalogs.length > 0 ? (
                  filteredCatalogs.map((catalog) => (
                    <tr key={catalog.id}>
                      <td>
                        <span className="catalog-name">{catalog.name}</span>
                      </td>
                      <td>{catalog.category}</td>
                      <td>
                        <div className="parameters-tags">
                          {catalog.parameters.map((param) => (
                            <span key={param.id} className="parameter-tag">
                              {param.code}
                            </span>
                          ))}
                        </div>
                      </td>
                      <td>
                        <span className="catalog-price">
                          {catalog.price.toLocaleString()} đ
                        </span>
                      </td>
                      <td>
                        <span
                          className={`status-badge ${
                            catalog.status === "Hoạt động"
                              ? "active"
                              : "inactive"
                          }`}
                        >
                          {catalog.status}
                        </span>
                      </td>
                      <td>
                        <div className="action-buttons">
                          <button
                            className="action-button edit"
                            onClick={() => handleOpenEditModal(catalog)}
                            title="Chỉnh sửa"
                          >
                            <FiEdit2 size={18} />
                          </button>
                          <button
                            className="action-button delete"
                            onClick={() => handleDeleteCatalog(catalog.id)}
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
                      Không tìm thấy mục xét nghiệm nào
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Catalog Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="catalog-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>
                {modalMode === "create"
                  ? "Thêm mục xét nghiệm"
                  : "Chỉnh sửa mục xét nghiệm"}
              </h2>
              <button className="modal-close" onClick={handleCloseModal}>
                <FiX size={20} />
              </button>
            </div>

            <div className="modal-body">
              {/* Basic Info Section */}
              <div className="form-section">
                <div className="form-row">
                  <div className="form-group">
                    <label className="form-label">Tên mục xét nghiệm</label>
                    <input
                      type="text"
                      name="name"
                      className="form-input"
                      placeholder="VD: Xét nghiệm máu toàn bộ"
                      value={formData.name}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Danh mục</label>
                    <input
                      type="text"
                      name="category"
                      className="form-input"
                      placeholder="VD: Máu"
                      value={formData.category}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label className="form-label">Giá (VNĐ)</label>
                    <input
                      type="number"
                      name="price"
                      className="form-input"
                      placeholder="150000"
                      value={formData.price}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Trạng thái</label>
                    <select
                      name="status"
                      className="form-input"
                      value={formData.status}
                      onChange={handleInputChange}
                    >
                      <option value="Hoạt động">Hoạt động</option>
                      <option value="Ngừng hoạt động">Ngừng hoạt động</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Parameters Selection Section */}
              <div className="form-section">
                <label className="form-label">Chọn chỉ số xét nghiệm</label>
                <div className="parameters-selection">
                  {availableParameters.map((param) => {
                    const isSelected = formData.parameters.some(
                      (p) => p.id === param.id
                    );
                    return (
                      <div
                        key={param.id}
                        className={`parameter-item ${
                          isSelected ? "selected" : ""
                        }`}
                        onClick={() => {
                          if (isSelected) {
                            handleRemoveParameter(param.id);
                          } else {
                            handleAddParameter(param);
                          }
                        }}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => {}}
                        />
                        <div className="parameter-info">
                          <span className="parameter-code">{param.code}</span>
                          <span className="parameter-name">{param.name}</span>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Selected Parameters Display */}
              {formData.parameters.length > 0 && (
                <div className="form-section">
                  <label className="form-label">Các chỉ số đã chọn</label>
                  <div className="selected-parameters">
                    {formData.parameters.map((param) => (
                      <div key={param.id} className="selected-parameter-tag">
                        <span>
                          {param.code} - {param.name}
                        </span>
                        <button
                          className="remove-tag-btn"
                          onClick={() => handleRemoveParameter(param.id)}
                        >
                          <FiX size={14} />
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Description */}
              <div className="form-section">
                <div className="form-group">
                  <label className="form-label">Mô tả</label>
                  <textarea
                    name="description"
                    className="form-textarea"
                    placeholder="Xét nghiệm đầm bào toàn bộ bao gồm các chỉ số"
                    value={formData.description}
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
                onClick={handleSaveCatalog}
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

export default AdminCatalogsPage;
