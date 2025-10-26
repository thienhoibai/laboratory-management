import React, { useEffect, useState } from "react";
import AdminLayout from "../../../components/admin/layout/AdminLayout.jsx";
import {
  FiSearch,
  FiPlus,
  FiEye,
  FiTrash2,
  FiChevronDown,
  FiX,
} from "react-icons/fi";
import { Pagination } from "antd";
import api from "../../../configs/axios.js";
import { setAuthToken } from "../../../utils/auth.js";
import { toast } from "react-toastify";
const endPoint = "iam/api/Users";

const getRoleClass = (role) => {
  switch (role) {
    case "Doctor":
      return "role-doctor";
    case "Lab Technician":
      return "role-technician";
    case "Receptionist":
      return "role-receptionist";
    case "Lab Manager":
      return "role-manager";
    case "Admin":
      return "role-manager";
    case "Manager":
      return "role-manager";
    case "Staff":
      return "role-receptionist";
    case "Patient":
      return "role-doctor";
    case "Customer":
      return "role-technician";
    default:
      return "";
  }
};

const UserManagementPage = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Users Management" },
  ];
  const token = localStorage.getItem("accessToken");
  const [users, setUsers] = useState([]);
  const [total, setTotal] = useState(0);

  // Filter states
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const [type, setType] = useState("");
  const [status, setStatus] = useState("");
  const [sortBy, setSortBy] = useState("UpdatedAt");
  const [sortDir, setSortDir] = useState("desc");
  const [searchDebounce, setSearchDebounce] = useState("");

  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    username: "",
    password: "",
    confirmPassword: "",
    roleId: "",
  });
  const [formErrors, setFormErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const roleMapping = {
    Admin: 1,
    Manager: 2,
    Staff: 3,
    Customer: 5,
  };

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearchDebounce(search);
    }, 500);
    return () => clearTimeout(timer);
  }, [search]);

  useEffect(() => {
    if (token) setAuthToken(token);
    fetchUsers();
  }, [page, pageSize, searchDebounce, role, type, status, sortBy, sortDir]);

  const fetchUsers = async () => {
    try {
      const params = new URLSearchParams();
      params.append("page", page);
      params.append("pageSize", pageSize);
      if (searchDebounce) params.append("search", searchDebounce);
      if (role) params.append("role", role);
      if (type) params.append("type", type);
      if (status)
        params.append("isActive", status === "active" ? "true" : "false");

      // Backend expects lowercase sortBy based on C# code
      if (sortBy) {
        const sortByLower = sortBy.toLowerCase();
        params.append("sortBy", sortByLower);
      }
      if (sortDir) params.append("sortDir", sortDir.toLowerCase());

      console.log("Request params:", params.toString());

      const response = await api.get(`${endPoint}?${params.toString()}`);

      if (response.status === 200) {
        const res = response.data;
        const usersList = res.data || [];
        const meta = res.meta || {};

        console.log("Response:", { meta, usersCount: usersList.length });

        setUsers(usersList);
        setTotal(meta.totalItems || 0);
      }
    } catch (error) {
      console.error("Error fetching users:", error);
      console.error("Error response:", error.response?.data);
    }
  };

  const handleSearchChange = (e) => {
    setSearch(e.target.value);
    setPage(1);
  };

  const handleRoleChange = (e) => {
    setRole(e.target.value);
    setPage(1);
  };

  const handleTypeChange = (e) => {
    setType(e.target.value);
    setPage(1);
  };

  const handleStatusChange = (e) => {
    const value = e.target.value;
    setStatus(value);
    setPage(1);
  };

  const handleSort = (field) => {
    // Map frontend field names to backend field names
    const fieldMapping = {
      name: "name",
      role: "role",
      createdat: "createdat",
      updatedat: "updatedat",
      status: "status",
    };

    const backendField = fieldMapping[field] || field;

    if (sortBy.toLowerCase() === backendField) {
      // Toggle direction if same field
      setSortDir(sortDir === "asc" ? "desc" : "asc");
    } else {
      // New field, start with asc
      setSortBy(backendField);
      setSortDir("asc");
    }
    setPage(1);
  };

  const getSortIcon = (field) => {
    const fieldMapping = {
      name: "name",
      role: "role",
      createdat: "createdat",
      updatedat: "updatedat",
      status: "status",
    };

    const backendField = fieldMapping[field] || field;
    if (sortBy.toLowerCase() !== backendField) return null;
    return sortDir === "asc" ? " ↑" : " ↓";
  };

  const handlePageChange = (newPage, newPageSize) => {
    setPage(newPage);
    if (newPageSize !== pageSize) {
      setPageSize(newPageSize);
    }
  };

  // Modal handlers
  const handleOpenModal = () => {
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setFormData({
      username: "",
      password: "",
      confirmPassword: "",
      roleId: "",
    });
    setFormErrors({});
  };

  const handleFormChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    if (formErrors[name]) {
      setFormErrors((prev) => ({
        ...prev,
        [name]: "",
      }));
    }
  };

  const validateForm = () => {
    const errors = {};

    if (!formData.username.trim()) {
      errors.username = "Tên đăng nhập là bắt buộc";
    }

    if (!formData.password) {
      errors.password = "Mật khẩu là bắt buộc";
    } else if (formData.password.length < 6) {
      errors.password = "Mật khẩu phải có ít nhất 6 ký tự";
    }

    if (!formData.confirmPassword) {
      errors.confirmPassword = "Vui lòng xác nhận mật khẩu";
    } else if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = "Mật khẩu xác nhận không khớp";
    }

    if (!formData.roleId) {
      errors.roleId = "Vui lòng chọn vai trò";
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmitUser = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      const requestData = {
        username: formData.username.trim(),
        password: formData.password,
        roleId: parseInt(formData.roleId),
      };

      const response = await api.post(endPoint, requestData);

      if (response.status === 200 || response.status === 201) {
        toast("Thêm tài khoản thành công!");
        handleCloseModal();
        fetchUsers();
      }
    } catch (error) {
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        "Có lỗi xảy ra khi thêm tài khoản";
      toast.error(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AdminLayout pageTitle="Users Management" breadcrumbs={breadcrumbs}>
      <div className="admin-content-card">
        <div className="admin-table-controls">
          <div className="admin-filters-row">
            <div className="admin-search-bar">
              <FiSearch />
              <input
                type="text"
                placeholder="Search by name or email..."
                value={search}
                onChange={handleSearchChange}
              />
            </div>

            <div className="admin-filter-group">
              <select
                className="admin-filter-select"
                value={role}
                onChange={handleRoleChange}
              >
                <option value="">All Roles</option>
                <option value="Admin">Admin</option>
                <option value="Manager">Manager</option>
                <option value="Staff">Staff</option>
                <option value="Patient">Patient</option>
                <option value="Customer">Customer</option>
              </select>
              <FiChevronDown className="admin-select-icon" />
            </div>

            <div className="admin-filter-group">
              <select
                className="admin-filter-select"
                value={type}
                onChange={handleTypeChange}
              >
                <option value="">All Types</option>
                <option value="internal">Internal</option>
                <option value="external">External</option>
              </select>
              <FiChevronDown className="admin-select-icon" />
            </div>

            <div className="admin-filter-group">
              <select
                className="admin-filter-select"
                value={status}
                onChange={handleStatusChange}
              >
                <option value="">All Status</option>
                <option value="active">Active</option>
                <option value="inactive">Inactive</option>
              </select>
              <FiChevronDown className="admin-select-icon" />
            </div>
          </div>

          <button className="admin-add-button" onClick={handleOpenModal}>
            <FiPlus /> Add User
          </button>
        </div>

        <div className="admin-table">
          <div className="admin-table-header">
            <span className="sortable" onClick={() => handleSort("name")}>
              Họ Và Tên{getSortIcon("name")}
            </span>
            <span>Email</span>
            <span className="sortable" onClick={() => handleSort("role")}>
              Role{getSortIcon("role")}
            </span>
            <span>Lần Đăng Nhập Cuối Cùng</span>
            <span className="sortable" onClick={() => handleSort("createdat")}>
              Ngày Tạo{getSortIcon("createdat")}
            </span>
            {/* <span className="sortable" onClick={() => handleSort("updatedat")}>
              Cập Nhật{getSortIcon("updatedat")}
            </span> */}
            <span className="sortable" onClick={() => handleSort("status")}>
              Trạng Thái{getSortIcon("status")}
            </span>
            <span>Hành Động</span>
          </div>
          {users.map((user, index) => (
            <div className="admin-table-row" key={index}>
              <span>{user.fullName}</span>
              <span>{user.email}</span>
              <span>
                <div className={`admin-badge ${getRoleClass(user.role)}`}>
                  {user.roles}
                </div>
              </span>
              <span>{user.lastLoginAt || "N/A"}</span>
              <span>{new Date(user.createdAt).toLocaleDateString()}</span>
              {/* <span>
                {user.updatedAt
                  ? new Date(user.updatedAt).toLocaleDateString()
                  : "N/A"}
              </span> */}
              <span>
                <div
                  className={`admin-badge-status ${
                    user.isActive ? "status-active" : "status-inactive"
                  }`}
                >
                  {user.isActive ? "Active" : "Inactive"}
                </div>
              </span>
              <span className="admin-table-actions">
                <FiEye />
                <FiTrash2 />
              </span>
            </div>
          ))}
        </div>

        {users.length === 0 && (
          <div className="admin-no-data">
            <p>Không tìm thấy người dùng nào</p>
          </div>
        )}

        <div className="admin-pagination">
          <Pagination
            current={page}
            pageSize={pageSize}
            total={total}
            onChange={handlePageChange}
            showSizeChanger
            showQuickJumper
            showTotal={(total, range) =>
              total > 0
                ? `${range[0]}-${range[1]} của ${total} dữ liệu`
                : "0 dữ liệu"
            }
            pageSizeOptions={["5", "10", "20", "50", "100"]}
            locale={{
              items_per_page: "/ trang",
              jump_to: "Đến",
              jump_to_confirm: "xác nhận",
              page: "",
              prev_page: "Trang trước",
              next_page: "Trang sau",
              prev_5: "5 trang trước",
              next_5: "5 trang sau",
              prev_3: "3 trang trước",
              next_3: "3 trang sau",
            }}
          />
        </div>
      </div>

      {/* Add User Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-container" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Thêm Tài Khoản Mới</h2>
              <button className="modal-close-btn" onClick={handleCloseModal}>
                <FiX />
              </button>
            </div>

            <form onSubmit={handleSubmitUser} className="modal-form">
              <div className="form-group">
                <label htmlFor="username">
                  Tên Đăng Nhập <span className="required">*</span>
                </label>
                <input
                  type="text"
                  id="username"
                  name="username"
                  value={formData.username}
                  onChange={handleFormChange}
                  className={formErrors.username ? "input-error" : ""}
                  placeholder="Nhập tên đăng nhập"
                />
                {formErrors.username && (
                  <span className="error-message">{formErrors.username}</span>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="password">
                  Mật Khẩu <span className="required">*</span>
                </label>
                <input
                  type="password"
                  id="password"
                  name="password"
                  value={formData.password}
                  onChange={handleFormChange}
                  className={formErrors.password ? "input-error" : ""}
                  placeholder="Nhập mật khẩu"
                />
                {formErrors.password && (
                  <span className="error-message">{formErrors.password}</span>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="confirmPassword">
                  Xác Nhận Mật Khẩu <span className="required">*</span>
                </label>
                <input
                  type="password"
                  id="confirmPassword"
                  name="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleFormChange}
                  className={formErrors.confirmPassword ? "input-error" : ""}
                  placeholder="Nhập lại mật khẩu"
                />
                {formErrors.confirmPassword && (
                  <span className="error-message">
                    {formErrors.confirmPassword}
                  </span>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="roleId">
                  Vai Trò <span className="required">*</span>
                </label>
                <select
                  id="roleId"
                  name="roleId"
                  value={formData.roleId}
                  onChange={handleFormChange}
                  className={formErrors.roleId ? "input-error" : ""}
                >
                  <option value="">Chọn vai trò</option>
                  {Object.entries(roleMapping).map(([roleName, roleValue]) => (
                    <option key={roleValue} value={roleValue}>
                      {roleName}
                    </option>
                  ))}
                </select>
                {formErrors.roleId && (
                  <span className="error-message">{formErrors.roleId}</span>
                )}
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn-cancel"
                  onClick={handleCloseModal}
                  disabled={isSubmitting}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="btn-submit"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? "Đang xử lý..." : "Thêm Tài Khoản"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default UserManagementPage;
