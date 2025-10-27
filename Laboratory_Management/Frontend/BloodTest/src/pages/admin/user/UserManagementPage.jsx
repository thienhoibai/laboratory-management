import React, { useEffect, useState } from "react";
import AdminLayout from "../../../components/admin/layout/AdminLayout.jsx";
import {
  FiSearch,
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiChevronDown,
  FiX,
  FiEye,
  FiEyeOff,
  FiAlertTriangle,
} from "react-icons/fi";
import { Pagination } from "antd";
import api from "../../../configs/axios.js";
import { setAuthToken } from "../../../utils/auth.js";
import { toast } from "react-toastify";
const endPoint = "http://localhost:8080/iam/api/Users";

// Removed old getRoleClass mapping; using inline color styles per role instead

const getRoleStyle = (role) => {
  const name = String(role || "").toLowerCase();
  const styles = {
    admin: { backgroundColor: "#db1f1fff", color: "#fff" },
    manager: { backgroundColor: "#9b59b6", color: "#fff" },
    staff: { backgroundColor: "#16a085", color: "#fff" },
    patient: { backgroundColor: "#10c3c9ff", color: "#fff" },
    customer: { backgroundColor: "#ffac30ff", color: "#fff" },
  };
  return styles[name] || { backgroundColor: "#95a5a6", color: "#fff" };
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
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  // Delete modal states
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [userToDelete, setUserToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const roleMapping = {
    Admin: 0,
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
  }, [page, pageSize, searchDebounce, role, status, sortBy, sortDir]);

  const fetchUsers = async () => {
    try {
      const params = new URLSearchParams();
      params.append("page", page);
      params.append("pageSize", pageSize);
      if (searchDebounce) params.append("search", searchDebounce);
      if (role) params.append("role", role);
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

  // Delete handlers
  const openDeleteModal = (user) => {
    setUserToDelete(user);
    setIsDeleteOpen(true);
  };

  const closeDeleteModal = () => {
    setIsDeleteOpen(false);
    setUserToDelete(null);
    setIsDeleting(false);
  };

  const confirmDelete = async () => {
    if (!userToDelete) return;
    const id =
      userToDelete?.id ??
      userToDelete?.userId ??
      userToDelete?.uuid ??
      userToDelete?.Id;
    if (!id) {
      toast.error("Không tìm thấy ID người dùng để xóa");
      return;
    }
    setIsDeleting(true);
    try {
      const response = await api.delete(`${endPoint}/${id}`);
      if (response.status === 200 || response.status === 204) {
        toast("Xóa người dùng thành công!");
        closeDeleteModal();
        fetchUsers();
      }
    } catch (error) {
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        "Có lỗi xảy ra khi xóa tài khoản";
      toast.error(errorMessage);
    } finally {
      setIsDeleting(false);
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
    } else if (formData.username.length < 4) {
      errors.username = "Tên đăng nhập phải có ít nhất 4 ký tự";
    } else if (!/^[a-zA-Z0-9_.-]+$/.test(formData.username)) {
      errors.username = "Chỉ cho phép chữ, số và các ký tự _ . -";
    }

    if (!formData.password) {
      errors.password = "Mật khẩu là bắt buộc";
    } else {
      const pwd = formData.password;
      if (pwd.length < 8) {
        errors.password = "Mật khẩu phải có ít nhất 8 ký tự";
      } else if (!/[A-Z]/.test(pwd) || !/[a-z]/.test(pwd) || !/[0-9]/.test(pwd) || !/[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?`~]/.test(pwd)) {
        errors.password = "Mật khẩu phải gồm chữ hoa, chữ thường, số và ký tự đặc biệt";
      }
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

    const requestData = {
      username: formData.username.trim(),
      password: formData.password,
      roleId: parseInt(formData.roleId),
    };

    try {
      const response = await api.post(endPoint, requestData, {
        headers: { "Content-Type": "application/json" },
      });

      if (response.status === 200 || response.status === 201) {
        toast("Thêm tài khoản thành công!");
        handleCloseModal();
        fetchUsers();
      }
    } catch (error) {
      const data = error.response?.data;
      let detail =
        (typeof data?.detail === "string" && data.detail) ||
        data?.message;
      if (!detail && data?.errors) {
        if (Array.isArray(data.errors)) {
          detail = data.errors.join("; ");
        } else if (typeof data.errors === "object") {
          detail = Object.entries(data.errors)
            .map(([k, v]) => `${k}: ${Array.isArray(v) ? v.join(", ") : v}`)
            .join("; ");
        }
      }
      const errorMessage = detail || "Có lỗi xảy ra khi thêm tài khoản";
      toast.error(errorMessage);
      console.error("Add User failed", { requestData, response: data });

      // Map backend field errors to form fields if available
      if (data?.errors && typeof data.errors === "object") {
        const be = data.errors;
        const next = { ...formErrors };
        const getMsg = (val) => (Array.isArray(val) ? val[0] : (val || ""));
        // common keys used by backends: Username, username, Password, password, RoleId, roleId
        if (be.Username || be.username) next.username = getMsg(be.Username || be.username);
        if (be.Password || be.password) next.password = getMsg(be.Password || be.password);
        if (be.RoleId || be.roleId) next.roleId = getMsg(be.RoleId || be.roleId);
        // generic message fallback
        if (!next.username && !next.password && !next.roleId && errorMessage) {
          next.username = errorMessage;
        }
        setFormErrors(next);
      }
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
                placeholder="Tìm kiếm bằng tên hoặc email"
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
                value={status}
                onChange={handleStatusChange}
              >
                <option value="">Trạng thái</option>
                <option value="active">Hoạt động</option>
                <option value="inactive">Không hoạt động</option>
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
                <div
                  className="admin-badge"
                  style={getRoleStyle(user.role || user.roles)}
                >
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
                  className={`admin-badge ${
                    user.isActive ? "status-active" : "status-inactive"
                  }`}
                >
                  {user.isActive ? "Hoạt động" : "Không hoạt động"}
                </div>
              </span>
              <span className="admin-table-actions">
                <FiEdit2 style={{ cursor: "pointer" }} />
                <FiTrash2
                  onClick={() => openDeleteModal(user)}
                  style={{ cursor: "pointer" }}
                />
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
                <div style={{ position: "relative" }}>
                  <input
                    type={showPassword ? "text" : "password"}
                    id="password"
                    name="password"
                    value={formData.password}
                    onChange={handleFormChange}
                    className={formErrors.password ? "input-error" : ""}
                    placeholder="Nhập mật khẩu"
                    style={{ paddingRight: 36 }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                    title={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                    style={{
                      position: "absolute",
                      right: 6,
                      top: "50%",
                      transform: "translateY(-50%)",
                      background: "#f5f5f5",
                      border: "1px solid #d9d9d9",
                      borderRadius: 6,
                      cursor: "pointer",
                      width: 32,
                      height: 32,
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      padding: 0,
                    }}
                  >
                    {showPassword ? <FiEyeOff size={18} color="#333" /> : <FiEye size={18} color="#333" />}
                  </button>
                </div>
                {formErrors.password && (
                  <span className="error-message">{formErrors.password}</span>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="confirmPassword">
                  Xác Nhận Mật Khẩu <span className="required">*</span>
                </label>
                <div style={{ position: "relative" }}>
                  <input
                    type={showConfirmPassword ? "text" : "password"}
                    id="confirmPassword"
                    name="confirmPassword"
                    value={formData.confirmPassword}
                    onChange={handleFormChange}
                    className={formErrors.confirmPassword ? "input-error" : ""}
                    placeholder="Nhập lại mật khẩu"
                    style={{ paddingRight: 36 }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword((v) => !v)}
                    aria-label={showConfirmPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                    title={showConfirmPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                    style={{
                      position: "absolute",
                      right: 6,
                      top: "50%",
                      transform: "translateY(-50%)",
                      background: "#f5f5f5",
                      border: "1px solid #d9d9d9",
                      borderRadius: 6,
                      cursor: "pointer",
                      width: 32,
                      height: 32,
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      padding: 0,
                    }}
                  >
                    {showConfirmPassword ? <FiEyeOff size={18} color="#333" /> : <FiEye size={18} color="#333" />}
                  </button>
                </div>
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

      {/* Delete Confirm Modal */}
      {isDeleteOpen && (
        <div className="modal-overlay" onClick={closeDeleteModal}>
          <div className="modal-container" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header" style={{ padding: "16px 20px", borderBottom: "1px solid #eee" }}>
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <FiAlertTriangle style={{ color: "#e74c3c" }} />
                <h2 style={{ margin: 0 }}>Xác nhận xóa người dùng</h2>
              </div>
              <button className="modal-close-btn" onClick={closeDeleteModal}>
                <FiX />
              </button>
            </div>
            <div className="modal-content" style={{ padding: "16px 20px" }}>
              <p style={{ marginTop: 4, marginBottom: 0, lineHeight: 1.5 }}>
                Bạn có chắc chắn muốn xóa người dùng
                {" "}
                <strong>
                  {userToDelete?.fullName || userToDelete?.username || "này"}
                </strong>
                ? Hành động này không thể hoàn tác.
              </p>
            </div>
            <div
              className="modal-actions"
              style={{
                display: "flex",
                gap: 8,
                justifyContent: "flex-end",
                padding: "12px 20px",
                borderTop: "1px solid #eee",
              }}
            >
              <button
                type="button"
                className="btn-cancel"
                onClick={closeDeleteModal}
                disabled={isDeleting}
              >
                Hủy
              </button>
              <button
                type="button"
                className="btn-submit"
                onClick={confirmDelete}
                disabled={isDeleting}
                style={{ backgroundColor: "#e74c3c" }}
              >
                {isDeleting ? "Đang xóa..." : "Xóa"}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default UserManagementPage;
