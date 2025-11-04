import React, { useEffect, useState } from "react";
import AdminLayout from "../../admin/layout/AdminLayout.jsx";
import {
  FiSearch,
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiChevronDown,
  FiX,
  FiAlertTriangle,
  FiLock,
  FiUnlock,
} from "react-icons/fi";
import { Pagination } from "antd";
import api from "../../../configs/axios.js";
import { setAuthToken } from "../../../utils/auth.js";
import { toast } from "react-toastify";
import "./UsersManagement.css";

const endPoint = "http://localhost:8080/iam/api/Users";

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

const UsersManagement = () => {
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

  // Delete modal states
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [userToDelete, setUserToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [lockLoadingId, setLockLoadingId] = useState(null);

  const roleMapping = {
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

        usersList.forEach((u) => {
          console.log(
            `User ${u.fullName || u.username || u.id}: status = ${u.status}`
          );
        });

        setUsers(usersList);
        setTotal(meta.totalItems || 0);
      }
    } catch (error) {
      console.error("Error fetching users:", error);
      console.error("Error response:", error.response?.data);
    }
  };

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
    const fieldMapping = {
      name: "name",
      role: "role",
      createdat: "createdat",
      updatedat: "updatedat",
      status: "status",
    };

    const backendField = fieldMapping[field] || field;

    if (sortBy.toLowerCase() === backendField) {
      setSortDir(sortDir === "asc" ? "desc" : "asc");
    } else {
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
      } else if (
        !/[A-Z]/.test(pwd) ||
        !/[a-z]/.test(pwd) ||
        !/[0-9]/.test(pwd) ||
        !/[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?`~]/.test(pwd)
      ) {
        errors.password =
          "Mật khẩu phải gồm chữ hoa, chữ thường, số và ký tự đặc biệt";
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
      const response = await api.post(endPoint, requestData);

      if (response.status === 200 || response.status === 201) {
        toast("Thêm tài khoản thành công!");
        handleCloseModal();
        fetchUsers();
      }
    } catch (error) {
      const data = error.response?.data;
      let detail =
        (typeof data?.detail === "string" && data.detail) || data?.message;
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

      if (data?.errors && typeof data.errors === "object") {
        const be = data.errors;
        const next = { ...formErrors };
        const getMsg = (val) => (Array.isArray(val) ? val[0] : val || "");
        if (be.Username || be.username)
          next.username = getMsg(be.Username || be.username);
        if (be.Password || be.password)
          next.password = getMsg(be.Password || be.password);
        if (be.RoleId || be.roleId)
          next.roleId = getMsg(be.RoleId || be.roleId);
        if (!next.username && !next.password && !next.roleId && errorMessage) {
          next.username = errorMessage;
        }
        setFormErrors(next);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleLockUser = async (user) => {
    const id = user?.id ?? user?.userId ?? user?.uuid ?? user?.Id;
    if (!id) {
      toast.error("Không tìm thấy ID người dùng để thao tác");
      return;
    }
    setLockLoadingId(id);
    try {
      const response = await api.post(`iam/api/Users/${id}/lock`);
      if (response?.data?.data.status === "locked") {
        toast.success("Tài khoản đã bị khóa!");
      } else {
        toast.info("Thao tác thành công!");
      }
    } catch (error) {
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        "Có lỗi xảy ra khi thao tác tài khoản";
      toast.error(errorMessage);
    } finally {
      setLockLoadingId(null);
    }
  };

  const handleUnlockUser = async (user) => {
    const id = user?.id ?? user?.userId ?? user?.uuid ?? user?.Id;
    if (!id) {
      toast.error("Không tìm thấy ID người dùng để thao tác");
      return;
    }
    setLockLoadingId(id);
    try {
      const response = await api.post(`iam/api/Users/${id}/ `);
      if (response?.data?.data.status === "unlocked") {
        toast.success("Tài khoản đã được mở khóa!");
      } else {
        toast.info("Thao tác thành công!");
      }
    } catch (error) {
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        "Có lỗi xảy ra khi thao tác tài khoản";
      toast.error(errorMessage);
    } finally {
      setLockLoadingId(null);
    }
  };

  return (
    <AdminLayout pageTitle="Users Management" breadcrumbs={breadcrumbs}>
      <div className="users-management-content">
        <div className="users-table-controls">
          <div className="users-filters-row">
            <div className="users-search-bar">
              <FiSearch />
              <input
                type="text"
                placeholder="Tìm kiếm bằng tên hoặc email"
                value={search}
                onChange={handleSearchChange}
              />
            </div>

            <div className="users-filter-group">
              <select
                className="users-filter-select"
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
              <FiChevronDown className="users-select-icon" />
            </div>

            <div className="users-filter-group">
              <select
                className="users-filter-select"
                value={status}
                onChange={handleStatusChange}
              >
                <option value="">Trạng thái</option>
                <option value="active">Hoạt động</option>
                <option value="inactive">Không hoạt động</option>
              </select>
              <FiChevronDown className="users-select-icon" />
            </div>
          </div>

          <button className="users-add-button" onClick={handleOpenModal}>
            <FiPlus /> Add User
          </button>
        </div>

        <div className="users-table">
          <div className="users-table-header">
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
            <span className="sortable" onClick={() => handleSort("status")}>
              Trạng Thái{getSortIcon("status")}
            </span>
            <span>Hành Động</span>
          </div>
          {users.map((user, index) => (
            <div className="users-table-row" key={index}>
              <span>{user.fullName}</span>
              <span>{user.email}</span>
              <span>
                <div
                  className="users-badge"
                  style={getRoleStyle(user.role || user.roles)}
                >
                  {user.roles}
                </div>
              </span>
              <span>{user.lastLoginAt || "N/A"}</span>
              <span>{new Date(user.createdAt).toLocaleDateString()}</span>
              <span>
                <div
                  className={`users-badge-status ${
                    user.isActive ? "status-active" : "status-inactive"
                  }`}
                >
                  {user.isActive ? "Hoạt động" : "Không hoạt động"}
                </div>
              </span>
              <span className="users-table-actions">
                <FiEdit2 style={{ cursor: "pointer" }} />
                <FiTrash2
                  onClick={() => openDeleteModal(user)}
                  style={{ cursor: "pointer" }}
                />
                <FiLock
                  title="Khóa tài khoản"
                  style={{
                    cursor:
                      user.status === "locked" ||
                      lockLoadingId ===
                        (user.id ?? user.userId ?? user.uuid ?? user.Id)
                        ? "not-allowed"
                        : "pointer",
                    color: user.status === "locked" ? "#bdbdbd" : "#e74c3c",
                    filter:
                      user.status === "locked"
                        ? "grayscale(60%) brightness(0.8)"
                        : "drop-shadow(0 0 4px #e74c3c)",
                    opacity:
                      lockLoadingId ===
                      (user.id ?? user.userId ?? user.uuid ?? user.Id)
                        ? 0.6
                        : 1,
                    transition: "filter 0.2s, color 0.2s",
                  }}
                  onClick={() =>
                    user.status === "locked" || lockLoadingId
                      ? null
                      : handleLockUser(user)
                  }
                />
                <FiUnlock
                  title="Mở khóa tài khoản"
                  style={{
                    cursor:
                      user.status === "unlocked" ||
                      lockLoadingId ===
                        (user.id ?? user.userId ?? user.uuid ?? user.Id)
                        ? "not-allowed"
                        : "pointer",
                    color: user.status === "unlocked" ? "#bdbdbd" : "#198754",
                    filter:
                      user.status === "unlocked"
                        ? "grayscale(60%) brightness(0.8)"
                        : "drop-shadow(0 0 4px #198754)",
                    opacity:
                      lockLoadingId ===
                      (user.id ?? user.userId ?? user.uuid ?? user.Id)
                        ? 0.6
                        : 1,
                    transition: "filter 0.2s, color 0.2s",
                  }}
                  onClick={() =>
                    user.status === "unlocked" || lockLoadingId
                      ? null
                      : handleUnlockUser(user)
                  }
                />
              </span>
            </div>
          ))}
        </div>

        {users.length === 0 && (
          <div className="users-no-data">
            <p>Không tìm thấy người dùng nào</p>
          </div>
        )}

        <div className="users-pagination">
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
        <div className="users-modal-overlay" onClick={handleCloseModal}>
          <div
            className="users-modal-container"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="users-modal-header">
              <h2>Thêm Tài Khoản Mới</h2>
              <button
                className="users-modal-close-btn"
                onClick={handleCloseModal}
              >
                <FiX />
              </button>
            </div>

            <form onSubmit={handleSubmitUser} className="users-modal-form">
              <div className="users-form-group">
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

              <div className="users-form-group">
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

              <div className="users-form-group">
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

              <div className="users-form-group">
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

              <div className="users-modal-actions">
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
        <div className="users-modal-overlay" onClick={closeDeleteModal}>
          <div
            className="users-modal-container"
            onClick={(e) => e.stopPropagation()}
          >
            <div
              className="users-modal-header"
              style={{ padding: "16px 20px", borderBottom: "1px solid #eee" }}
            >
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <FiAlertTriangle style={{ color: "#e74c3c" }} />
                <h2 style={{ margin: 0 }}>Xác nhận xóa người dùng</h2>
              </div>
              <button
                className="users-modal-close-btn"
                onClick={closeDeleteModal}
              >
                <FiX />
              </button>
            </div>
            <div
              className="users-modal-content"
              style={{ padding: "16px 20px" }}
            >
              <p style={{ marginTop: 4, marginBottom: 0, lineHeight: 1.5 }}>
                Bạn có chắc chắn muốn xóa người dùng{" "}
                <strong>
                  {userToDelete?.fullName || userToDelete?.username || "này"}
                </strong>
                ? Hành động này không thể hoàn tác.
              </p>
            </div>
            <div
              className="users-modal-actions"
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

export default UsersManagement;
