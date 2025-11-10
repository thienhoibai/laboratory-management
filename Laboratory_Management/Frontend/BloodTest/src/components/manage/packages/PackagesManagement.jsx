import React, { useState, useMemo, useEffect } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import { FiSearch, FiEdit2, FiTrash2, FiPlus, FiX } from "react-icons/fi";
import { availableTests } from "../../../data/package";
import api from "../../../configs/axios";
import "./PackagesManagement.css";

const PackagesManagement = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const [packages, setPackages] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState("create"); // 'create' or 'edit'
  const [selectedPackage, setSelectedPackage] = useState(null);
  const [loading, setLoading] = useState(false);

  // Form state
  const [formData, setFormData] = useState({
    name: "",
    price: "",
    description: "",
    tests: [],
  });

  // Filter packages based on search query
  const filteredPackages = useMemo(() => {
    if (!searchQuery) return packages;
    const query = searchQuery.toLowerCase();
    return packages.filter((pkg) => pkg.name.toLowerCase().includes(query));
  }, [packages, searchQuery]);

  // Fetch packages from API
  useEffect(() => {
    fetchPackages();
  }, []);

  const fetchPackages = async () => {
    try {
      setLoading(true);
      const response = await api.get("testorder/api/CatalogBundle");

      // Transform API data to match component format
      const transformedData = response.data.map((item) => ({
        id: item.bundleId,
        name: item.bundleName,
        price: item.price,
        description: item.description || "",
        testCount: item.catalogs?.length || 0,
        status: "active", // Default status, adjust if API provides status
        tests:
          item.catalogs?.map((test) => ({
            id: test.catalogId,
            name: test.testName,
            category: test.description || "",
            price: test.price || 0,
          })) || [],
        createdAt: new Date().toISOString().split("T")[0],
      }));

      setPackages(transformedData);
    } catch (error) {
      console.error("Lỗi khi tải danh sách gói xét nghiệm:", error);
      alert("Không thể tải danh sách gói xét nghiệm. Vui lòng thử lại!");
    } finally {
      setLoading(false);
    }
  };

  // Handle open modal for create
  const handleOpenCreateModal = () => {
    setModalMode("create");
    setFormData({
      name: "",
      price: "",
      description: "",
      tests: [],
    });
    setSelectedPackage(null);
    setIsModalOpen(true);
  };

  // Handle open modal for edit
  const handleOpenEditModal = (pkg) => {
    setModalMode("edit");
    setFormData({
      name: pkg.name,
      price: pkg.price,
      description: pkg.description,
      tests: [...pkg.tests],
    });
    setSelectedPackage(pkg);
    setIsModalOpen(true);
  };

  // Handle close modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setFormData({
      name: "",
      price: "",
      description: "",
      tests: [],
    });
    setSelectedPackage(null);
  };

  // Handle form input change
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  // Handle add test to package
  const handleAddTest = (test) => {
    if (formData.tests.find((t) => t.id === test.id)) return;
    setFormData((prev) => ({
      ...prev,
      tests: [...prev.tests, test],
    }));
  };

  // Handle remove test from package
  const handleRemoveTest = (testId) => {
    setFormData((prev) => ({
      ...prev,
      tests: prev.tests.filter((t) => t.id !== testId),
    }));
  };

  // Calculate total test price
  const calculateTotalTestPrice = () => {
    return formData.tests.reduce((total, test) => total + test.price, 0);
  };

  // Handle save package
  const handleSavePackage = () => {
    if (!formData.name || !formData.price || formData.tests.length === 0) {
      alert("Vui lòng điền đầy đủ thông tin và chọn ít nhất một xét nghiệm!");
      return;
    }

    if (modalMode === "create") {
      const newPackage = {
        id: packages.length + 1,
        name: formData.name,
        price: parseInt(formData.price),
        description: formData.description,
        testCount: formData.tests.length,
        status: "active",
        tests: formData.tests,
        createdAt: new Date().toISOString().split("T")[0],
      };
      setPackages([...packages, newPackage]);
    } else {
      setPackages(
        packages.map((pkg) =>
          pkg.id === selectedPackage.id
            ? {
                ...pkg,
                name: formData.name,
                price: parseInt(formData.price),
                description: formData.description,
                testCount: formData.tests.length,
                tests: formData.tests,
              }
            : pkg
        )
      );
    }

    handleCloseModal();
  };

  // Handle delete package
  const handleDeletePackage = (id) => {
    if (window.confirm("Bạn có chắc chắn muốn xóa gói xét nghiệm này?")) {
      setPackages(packages.filter((pkg) => pkg.id !== id));
    }
  };

  // Format price
  const formatPrice = (price) => {
    return new Intl.NumberFormat("vi-VN").format(price) + " đ";
  };

  return (
    <AdminLayout
      pageTitle="Quản lý gói xét nghiệm"
      breadcrumbs={[
        { name: "Tổng quan", link: "/admin/dashboard" },
        { name: "Quản lý gói xét nghiệm" },
      ]}
    >
      <div className="packages-container">
        <div className="packages-header">
          <div className="packages-header-left">
            <h1>Quản lý gói xét nghiệm</h1>
            <p>Quản lý các gói xét nghiệm của trung tâm</p>
          </div>
          <button
            className="add-package-button"
            onClick={handleOpenCreateModal}
          >
            <FiPlus size={20} />
            <span>Thêm gói mới</span>
          </button>
        </div>

        <div className="packages-content">
          {/* Search Section */}
          <div className="search-section">
            <div className="search-box">
              <FiSearch size={18} />
              <input
                type="text"
                placeholder="Tìm kiếm gói xét nghiệm..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>

          {/* Packages Table */}
          <div className="packages-table-container">
            {loading ? (
              <div style={{ textAlign: "center", padding: "40px" }}>
                <p>Đang tải dữ liệu...</p>
              </div>
            ) : (
              <table className="packages-table">
                <thead>
                  <tr>
                    <th>Tên gói</th>
                    <th>Giá tiền</th>
                    <th>Số lượng xét nghiệm</th>
                    <th>Trạng thái</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredPackages.length > 0 ? (
                    filteredPackages.map((pkg) => (
                      <tr key={pkg.id}>
                        <td>
                          <span className="package-name">{pkg.name}</span>
                        </td>
                        <td>
                          <span className="package-price">
                            {formatPrice(pkg.price)}
                          </span>
                        </td>
                        <td>{pkg.testCount} xét nghiệm</td>
                        <td>
                          <span className={`status-badge ${pkg.status}`}>
                            {pkg.status === "active" ? "Hoạt động" : "Tạm dừng"}
                          </span>
                        </td>
                        <td>
                          <div className="action-buttons">
                            <button
                              className="action-button edit"
                              onClick={() => handleOpenEditModal(pkg)}
                              title="Chỉnh sửa"
                            >
                              <FiEdit2 size={18} />
                            </button>
                            <button
                              className="action-button delete"
                              onClick={() => handleDeletePackage(pkg.id)}
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
                        colSpan="5"
                        style={{ textAlign: "center", padding: "40px" }}
                      >
                        Không tìm thấy gói xét nghiệm nào
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>

      {/* Package Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="package-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-header-left">
                <h2>
                  {modalMode === "create"
                    ? "Thêm gói xét nghiệm mới"
                    : "Chỉnh sửa gói xét nghiệm"}
                </h2>
                <p className="modal-subtitle">
                  {modalMode === "create"
                    ? "Điền thông tin và chọn các xét nghiệm cho gói"
                    : "Điều chỉnh thông tin và chọn các xét nghiệm cho gói"}
                </p>
              </div>
              <button className="modal-close" onClick={handleCloseModal}>
                <FiX size={24} />
              </button>
            </div>

            <div className="modal-body">
              {/* Left Side - Package Info */}
              <div className="modal-left">
                <div className="form-section">
                  <h3 className="form-section-title">Thông tin gói</h3>
                  <p
                    style={{
                      fontSize: "13px",
                      color: "#666",
                      marginBottom: "16px",
                    }}
                  >
                    Nhập các thông tin cơ bản của gói xét nghiệm
                  </p>

                  <div className="form-group">
                    <label className="form-label">Tên gói xét nghiệm</label>
                    <input
                      type="text"
                      name="name"
                      className="form-input"
                      placeholder="VD: Gói xét nghiệm tổng quát"
                      value={formData.name}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Giá gói (đ)</label>
                    <input
                      type="number"
                      name="price"
                      className="form-input"
                      placeholder="450000"
                      value={formData.price}
                      onChange={handleInputChange}
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Mô tả</label>
                    <textarea
                      name="description"
                      className="form-textarea"
                      placeholder="Mô tả chi tiết về gói xét nghiệm..."
                      value={formData.description}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>

                <div className="form-section">
                  <div className="selected-tests-header">
                    <h3 className="form-section-title">
                      Các xét nghiệm đã chọn
                    </h3>
                    <div className="selected-tests-info">
                      {formData.tests.length} xét nghiệm • Tổng giá trị:{" "}
                      <strong>{formatPrice(calculateTotalTestPrice())}</strong>
                    </div>
                  </div>

                  <div className="selected-tests-list">
                    {formData.tests.length > 0 ? (
                      formData.tests.map((test) => (
                        <div key={test.id} className="selected-test-item">
                          <div className="test-info">
                            <div className="test-name">{test.name}</div>
                            <div className="test-category">{test.category}</div>
                          </div>
                          <span className="test-price">
                            {formatPrice(test.price)}
                          </span>
                          <button
                            className="remove-test-button"
                            onClick={() => handleRemoveTest(test.id)}
                          >
                            <FiX size={16} />
                          </button>
                        </div>
                      ))
                    ) : (
                      <div className="empty-tests">
                        Chưa có xét nghiệm nào được chọn
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Right Side - Available Tests */}
              <div className="modal-right">
                <div className="form-section">
                  <h3 className="form-section-title">Danh mục xét nghiệm</h3>
                  <p
                    style={{
                      fontSize: "13px",
                      color: "#666",
                      marginBottom: "16px",
                    }}
                  >
                    Chọn các xét nghiệm để thêm vào gói
                  </p>

                  <div className="available-tests-list">
                    {availableTests.map((test) => {
                      const isAdded = formData.tests.find(
                        (t) => t.id === test.id
                      );
                      return (
                        <div key={test.id} className="available-test-item">
                          <div className="test-info">
                            <div className="test-name">{test.name}</div>
                            <div className="test-category">{test.category}</div>
                          </div>
                          <span className="test-price">
                            {formatPrice(test.price)}
                          </span>
                          <button
                            className="add-test-button"
                            onClick={() => handleAddTest(test)}
                            disabled={isAdded}
                          >
                            <FiPlus size={14} />
                            {isAdded ? "Đã thêm" : "Thêm"}
                          </button>
                        </div>
                      );
                    })}
                  </div>
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
                onClick={handleSavePackage}
              >
                {modalMode === "create" ? "Tạo gói mới" : "Cập nhật gói"}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default PackagesManagement;
