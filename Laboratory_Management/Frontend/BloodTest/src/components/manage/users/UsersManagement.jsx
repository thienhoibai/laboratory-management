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
    email: "",
    roleId: "",
  });
  const [formErrors, setFormErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Delete modal states
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [userToDelete, setUserToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Edit modal states
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [userToEdit, setUserToEdit] = useState(null);
  const [editFormData, setEditFormData] = useState({
    email: "",
    fullName: "",
    phone: "",
  });
  const [editFormErrors, setEditFormErrors] = useState({});
  const [isUpdating, setIsUpdating] = useState(false);

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
    // eslint-disable-next-line react-hooks/exhaustive-deps
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
      email: "",
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

    if (!formData.email.trim()) {
      errors.email = "Email là bắt buộc";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email.trim())) {
      errors.email = "Email không hợp lệ";
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
      email: formData.email.trim(),
      roleId: parseInt(formData.roleId),
    };

    try {
      const response = await api.post(endPoint, requestData);

      if (response.status === 200 || response.status === 201) {
        toast.success(
          "Thêm tài khoản thành công! Mật khẩu đã được gửi đến email của người dùng."
        );
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
        if (be.Email || be.email) next.email = getMsg(be.Email || be.email);
        if (be.RoleId || be.roleId)
          next.roleId = getMsg(be.RoleId || be.roleId);
        if (!next.username && !next.email && !next.roleId && errorMessage) {
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
      if (response?.data?.data.status === "locked" || response.status === 200) {
        // Update local state immediately for instant UI feedback
        setUsers((prevUsers) =>
          prevUsers.map((u) => {
            const userId = u?.id ?? u?.userId ?? u?.uuid ?? u?.Id;
            if (userId === id) {
              return { ...u, isActive: false, status: "locked" };
            }
            return u;
          })
        );
        toast.success("Tài khoản đã bị khóa!");
        // Then refresh from server to ensure consistency
        fetchUsers();
      } else {
        toast.info("Thao tác thành công!");
        fetchUsers();
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
      const response = await api.post(`iam/api/Users/${id}/unlock`);
      if (
        response?.data?.data.status === "unlocked" ||
        response.status === 200
      ) {
        // Update local state immediately for instant UI feedback
        setUsers((prevUsers) =>
          prevUsers.map((u) => {
            const userId = u?.id ?? u?.userId ?? u?.uuid ?? u?.Id;
            if (userId === id) {
              return { ...u, isActive: true, status: "unlocked" };
            }
            return u;
          })
        );
        toast.success("Tài khoản đã được mở khóa!");
        // Then refresh from server to ensure consistency
        fetchUsers();
      } else {
        toast.info("Thao tác thành công!");
        fetchUsers();
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

  // Edit User functions
  const handleOpenEditModal = (user) => {
    setUserToEdit(user);
    setEditFormData({
      email: user.email || "",
      fullName: user.fullName || "",
      phone: user.phone || "",
    });
    setEditFormErrors({});
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setIsEditModalOpen(false);
    setUserToEdit(null);
    setEditFormData({
      email: "",
      fullName: "",
      phone: "",
    });
    setEditFormErrors({});
  };

  const handleEditFormChange = (e) => {
    const { name, value } = e.target;
    setEditFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    if (editFormErrors[name]) {
      setEditFormErrors((prev) => ({
        ...prev,
        [name]: "",
      }));
    }
  };

  const validateEditForm = () => {
    const errors = {};

    if (!editFormData.email.trim()) {
      errors.email = "Email là bắt buộc";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(editFormData.email.trim())) {
      errors.email = "Email không hợp lệ";
    }

    if (!editFormData.fullName.trim()) {
      errors.fullName = "Họ và tên là bắt buộc";
    }

    if (
      editFormData.phone &&
      !/^[0-9+\-\s()]+$/.test(editFormData.phone.trim())
    ) {
      errors.phone = "Số điện thoại không hợp lệ";
    }

    setEditFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleUpdateUser = async () => {
    if (!validateEditForm()) {
      return;
    }

    if (!userToEdit) {
      toast.error("Không tìm thấy thông tin người dùng để chỉnh sửa");
      return;
    }

    const id =
      userToEdit?.id ??
      userToEdit?.userId ??
      userToEdit?.uuid ??
      userToEdit?.Id;
    if (!id) {
      toast.error("Không tìm thấy ID người dùng để chỉnh sửa");
      return;
    }

    setIsUpdating(true);

    const requestData = {
      email: editFormData.email.trim(),
      fullName: editFormData.fullName.trim(),
      phone: editFormData.phone.trim() || "",
    };

    try {
      const response = await api.put(`iam/api/Users/${id}`, requestData);

      if (response.status === 200 || response.status === 204) {
        toast.success("Cập nhật thông tin người dùng thành công!");
        handleCloseEditModal();
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
      const errorMessage =
        detail || "Có lỗi xảy ra khi cập nhật thông tin người dùng";
      toast.error(errorMessage);
      console.error("Update User failed", { requestData, response: data });

      if (data?.errors && typeof data.errors === "object") {
        const be = data.errors;
        const next = { ...editFormErrors };
        const getMsg = (val) => (Array.isArray(val) ? val[0] : val || "");
        if (be.Email || be.email) next.email = getMsg(be.Email || be.email);
        if (be.FullName || be.fullName)
          next.fullName = getMsg(be.FullName || be.fullName);
        if (be.Phone || be.phone) next.phone = getMsg(be.Phone || be.phone);
        if (!next.email && !next.fullName && !next.phone && errorMessage) {
          next.email = errorMessage;
        }
        setEditFormErrors(next);
      }
    } finally {
      setIsUpdating(false);
    }
  };

  return (
    <AdminLayout pageTitle="Quản lý người dùng" breadcrumbs={breadcrumbs}>
      <div className="users-container">
        <div className="users-header">
          <div className="users-header-left">
            <h1>Quản lý người dùng</h1>
            <p>Quản lý tài khoản và quyền truy cập của người dùng</p>
          </div>
          <button className="add-user-button" onClick={handleOpenModal}>
            <FiPlus size={20} />
            <span>Thêm người dùng</span>
          </button>
        </div>

        <div className="users-content">
          {/* Filters Section */}
          <div className="users-filters-section">
            <div className="search-section">
              <div className="search-box">
                <FiSearch size={18} />
                <input
                  type="text"
                  placeholder="Tìm kiếm bằng tên hoặc email..."
                  value={search}
                  onChange={handleSearchChange}
                />
              </div>
            </div>

            <div className="filter-group">
              <select
                className="filter-select"
                value={role}
                onChange={handleRoleChange}
              >
                <option value="">Tất cả vai trò</option>
                <option value="Admin">Admin</option>
                <option value="Manager">Manager</option>
                <option value="Staff">Staff</option>
                <option value="Patient">Patient</option>
                <option value="Customer">Customer</option>
              </select>
              <FiChevronDown className="select-icon" />
            </div>

            <div className="filter-group">
              <select
                className="filter-select"
                value={status}
                onChange={handleStatusChange}
              >
                <option value="">Tất cả trạng thái</option>
                <option value="active">Hoạt động</option>
                <option value="inactive">Không hoạt động</option>
              </select>
              <FiChevronDown className="select-icon" />
            </div>
          </div>

          {/* Users Table */}
          <div className="users-table-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th
                    className="sortable"
                    onClick={() => handleSort("name")}
                    style={{ cursor: "pointer" }}
                  >
                    Họ và tên{getSortIcon("name")}
                  </th>
                  <th>Email</th>
                  <th
                    className="sortable"
                    onClick={() => handleSort("role")}
                    style={{ cursor: "pointer" }}
                  >
                    Vai trò{getSortIcon("role")}
                  </th>
                  <th>Lần đăng nhập cuối</th>
                  <th
                    className="sortable"
                    onClick={() => handleSort("createdat")}
                    style={{ cursor: "pointer" }}
                  >
                    Ngày tạo{getSortIcon("createdat")}
                  </th>
                  <th
                    className="sortable"
                    onClick={() => handleSort("status")}
                    style={{ cursor: "pointer" }}
                  >
                    Trạng thái{getSortIcon("status")}
                  </th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {users.length > 0 ? (
                  users.map((user, index) => (
                    <tr key={user.id || user.userId || user.uuid || index}>
                      <td>
                        <span className="user-name">
                          {user.fullName || "-"}
                        </span>
                      </td>
                      <td>{user.email || "-"}</td>
                      <td>
                        <span
                          className="role-badge"
                          style={getRoleStyle(user.role || user.roles)}
                        >
                          {user.roles || user.role || "-"}
                        </span>
                      </td>
                      <td>{user.lastLoginAt || "N/A"}</td>
                      <td>
                        {user.createdAt
                          ? new Date(user.createdAt).toLocaleDateString("vi-VN")
                          : "-"}
                      </td>
                      <td>
                        <span
                          className={`status-badge ${
                            user.isActive ? "active" : "inactive"
                          }`}
                        >
                          {user.isActive ? "Hoạt động" : "Không hoạt động"}
                        </span>
                      </td>
                      <td>
                        <div className="action-buttons">
                          <button
                            className="action-button edit"
                            onClick={() => handleOpenEditModal(user)}
                            title="Chỉnh sửa"
                          >
                            <FiEdit2 size={18} />
                          </button>
                          <button
                            className="action-button delete"
                            onClick={() => openDeleteModal(user)}
                            title="Xóa"
                          >
                            <FiTrash2 size={18} />
                          </button>
                          <button
                            className={`action-button lock-toggle ${
                              user.isActive ? "unlocked" : "locked"
                            }`}
                            onClick={() =>
                              user.isActive
                                ? handleLockUser(user)
                                : handleUnlockUser(user)
                            }
                            disabled={
                              lockLoadingId ===
                              (user.id ?? user.userId ?? user.uuid ?? user.Id)
                            }
                            title={
                              user.isActive
                                ? "Khóa tài khoản"
                                : "Mở khóa tài khoản"
                            }
                          >
                            {user.isActive ? (
                              <FiUnlock size={18} />
                            ) : (
                              <FiLock size={18} />
                            )}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td
                      colSpan="7"
                      style={{ textAlign: "center", padding: "40px" }}
                    >
                      Không tìm thấy người dùng nào
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
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
      </div>

      {/* Add User Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="user-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Thêm tài khoản mới</h2>
              <button className="modal-close" onClick={handleCloseModal}>
                <FiX size={20} />
              </button>
            </div>

            <div className="modal-body">
              <div className="form-group">
                <label className="form-label">
                  Tên đăng nhập <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <input
                  type="text"
                  name="username"
                  className={`form-input ${
                    formErrors.username ? "input-error" : ""
                  }`}
                  placeholder="VD: john_doe"
                  value={formData.username}
                  onChange={handleFormChange}
                />
                {formErrors.username && (
                  <span className="error-message">{formErrors.username}</span>
                )}
              </div>

              <div className="form-group">
                <label className="form-label">
                  Email <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <input
                  type="email"
                  name="email"
                  className={`form-input ${
                    formErrors.email ? "input-error" : ""
                  }`}
                  placeholder="VD: user@example.com"
                  value={formData.email}
                  onChange={handleFormChange}
                />
                {formErrors.email && (
                  <span className="error-message">{formErrors.email}</span>
                )}
                <small
                  style={{
                    color: "#6b7280",
                    fontSize: "12px",
                    marginTop: "4px",
                    display: "block",
                  }}
                >
                  Mật khẩu sẽ được gửi đến email này để xác thực
                </small>
              </div>

              <div className="form-group">
                <label className="form-label">
                  Vai trò <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <select
                  name="roleId"
                  className={`form-input ${
                    formErrors.roleId ? "input-error" : ""
                  }`}
                  value={formData.roleId}
                  onChange={handleFormChange}
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
            </div>

            <div className="modal-footer">
              <button
                type="button"
                className="modal-button cancel"
                onClick={handleCloseModal}
                disabled={isSubmitting}
              >
                Hủy
              </button>
              <button
                type="button"
                className="modal-button primary"
                onClick={handleSubmitUser}
                disabled={isSubmitting}
              >
                {isSubmitting ? "Đang xử lý..." : "Thêm mới"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit User Modal */}
      {isEditModalOpen && (
        <div className="modal-overlay" onClick={handleCloseEditModal}>
          <div className="user-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Chỉnh sửa thông tin người dùng</h2>
              <button className="modal-close" onClick={handleCloseEditModal}>
                <FiX size={20} />
              </button>
            </div>

            <div className="modal-body">
              <div className="form-group">
                <label className="form-label">
                  Email <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <input
                  type="email"
                  name="email"
                  className={`form-input ${
                    editFormErrors.email ? "input-error" : ""
                  }`}
                  placeholder="VD: user@example.com"
                  value={editFormData.email}
                  onChange={handleEditFormChange}
                />
                {editFormErrors.email && (
                  <span className="error-message">{editFormErrors.email}</span>
                )}
              </div>

              <div className="form-group">
                <label className="form-label">
                  Họ và tên <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <input
                  type="text"
                  name="fullName"
                  className={`form-input ${
                    editFormErrors.fullName ? "input-error" : ""
                  }`}
                  placeholder="VD: Nguyễn Văn A"
                  value={editFormData.fullName}
                  onChange={handleEditFormChange}
                />
                {editFormErrors.fullName && (
                  <span className="error-message">
                    {editFormErrors.fullName}
                  </span>
                )}
              </div>

              <div className="form-group">
                <label className="form-label">Số điện thoại</label>
                <input
                  type="tel"
                  name="phone"
                  className={`form-input ${
                    editFormErrors.phone ? "input-error" : ""
                  }`}
                  placeholder="VD: 0123456789"
                  value={editFormData.phone}
                  onChange={handleEditFormChange}
                />
                {editFormErrors.phone && (
                  <span className="error-message">{editFormErrors.phone}</span>
                )}
              </div>
            </div>

            <div className="modal-footer">
              <button
                type="button"
                className="modal-button cancel"
                onClick={handleCloseEditModal}
                disabled={isUpdating}
              >
                Hủy
              </button>
              <button
                type="button"
                className="modal-button primary"
                onClick={handleUpdateUser}
                disabled={isUpdating}
              >
                {isUpdating ? "Đang cập nhật..." : "Cập nhật"}
              </button>
            </div>
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
