import React, { useState, useEffect } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import {
  FiSearch,
  FiPlus,
  FiTrash2,
  FiBook,
  FiEye,
  FiTrendingUp,
  FiMessageCircle,
  FiX,
  FiCheck,
  FiXCircle,
  FiEdit,
} from "react-icons/fi";
import { Pagination } from "antd";
import { toast } from "react-toastify";
import BlogService from "../../../services/BlogService";
import { setAuthToken } from "../../../utils/auth";
import "./BlogsManagement.css";
import { jwtDecode } from "jwt-decode";
import { formatDate1 } from "../../../utils/formatDate";

const resolveBlogId = (blogOrId) => {
  if (blogOrId == null) return null;
  if (typeof blogOrId === "number" || typeof blogOrId === "string") {
    return blogOrId;
  }
  return (
    blogOrId.id ??
    blogOrId.postId ??
    blogOrId.blogPostId ??
    blogOrId.blogId ??
    null
  );
};

const BlogsManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Quản lý Blog" },
  ];

  const [filter, setFilter] = useState("all");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);

  // Blogs state
  const [blogs, setBlogs] = useState([]);
  const [loading, setLoading] = useState(false);

  const [categories, setCategories] = useState([]);
  const [categoriesLoading, setCategoriesLoading] = useState(false);

  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    title: "",
    category: "",
    categoryId: "",
    content: "",
    img: "",
  });
  const [formErrors, setFormErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [editingBlogId, setEditingBlogId] = useState(null);

  // Delete modal states
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [blogToDelete, setBlogToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // View detail modal states
  const [isViewDetailOpen, setIsViewDetailOpen] = useState(false);
  const [viewingBlog, setViewingBlog] = useState(null);

  // Load data on mount and when filter changes
  useEffect(() => {
    loadBlogs();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  useEffect(() => {
    loadCategories();
  }, []);

  const loadBlogs = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      // Map filter to status code
      const statusMap = {
        all: undefined,
        pending: 0,
        approved: 1,
        rejected: 2,
      };

      const params = {};
      if (filter !== "all") {
        params.status = statusMap[filter];
      }

      const blogsData = await BlogService.getAllBlogs(params);
      setBlogs(blogsData);
    } catch (error) {
      console.error("Error loading blogs:", error);
      toast.error("Không thể tải danh sách bài viết. Vui lòng thử lại!");
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    setCategoriesLoading(true);
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      const categoriesData = await BlogService.getCategories();
      setCategories(categoriesData);
    } catch (error) {
      console.error("Error loading categories:", error);
      toast.error("Không thể tải danh mục. Vui lòng thử lại!");
    } finally {
      setCategoriesLoading(false);
    }
  };

  // Frontend search only (API already filtered by status)
  const searchedBlogs = blogs.filter((blog) =>
    blog.title.toLowerCase().includes(search.toLowerCase())
  );

  const displayedBlogs = searchedBlogs.slice(
    (page - 1) * pageSize,
    page * pageSize
  );

  // Statistics
  const stats = {
    total: blogs.length,
    approved: blogs.filter((b) => b.status === "approved").length,
    views: blogs.reduce((sum, b) => sum + b.views, 0),
    comments: blogs.reduce((sum, b) => sum + (b.comments || 0), 0),
  };

  const getStatusTag = (status) => {
    const statusMap = {
      approved: { text: "Đã duyệt", class: "status-approved" },
      pending: { text: "Chờ duyệt", class: "status-pending" },
      rejected: { text: "Đã hủy", class: "status-rejected" },
    };
    const statusInfo = statusMap[status] || statusMap.pending;
    return (
      <span className={`blogs-status-tag ${statusInfo.class}`}>
        {statusInfo.text}
      </span>
    );
  };

  // Modal handlers
  const openCreateModal = () => {
    setFormData({
      title: "",
      category: "",
      categoryId: "",
      content: "",
      img: "",
    });
    setFormErrors({});
    setIsEditMode(false);
    setEditingBlogId(null);
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setFormData({
      title: "",
      category: "",
      categoryId: "",
      content: "",
      img: "",
    });
    setFormErrors({});
    setIsEditMode(false);
    setEditingBlogId(null);
  };

  const openEditModal = (blog) => {
    const resolvedId = resolveBlogId(blog);
    if (!resolvedId && resolvedId !== 0) {
      toast.error("Không xác định được ID bài viết để chỉnh sửa");
      return;
    }
    setFormData({
      title: blog.title || "",
      category: blog.category || "",
      categoryId:
        blog.categoryId !== undefined && blog.categoryId !== null
          ? String(blog.categoryId)
          : "",
      status: blog.status || "pending",
      content: blog.content || "",
      img: blog.img || blog.thumbnailUrl || "",
    });
    setFormErrors({});
    setIsEditMode(true);
    setEditingBlogId(resolvedId);
    setIsModalOpen(true);
  };

  // Form handlers
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    // Clear error when user types
    if (formErrors[name]) {
      setFormErrors((prev) => ({
        ...prev,
        [name]: "",
      }));
    }
  };

  const handleCategorySelect = (e) => {
    const selectedId = e.target.value;
    const selectedCategory = categories.find(
      (category) => String(category.categoryId) === selectedId
    );

    setFormData((prev) => ({
      ...prev,
      categoryId: selectedId,
      category: selectedCategory?.name || "",
    }));

    if (formErrors.categoryId) {
      setFormErrors((prev) => ({
        ...prev,
        categoryId: "",
      }));
    }
  };

  // Form validation
  const validateForm = () => {
    const errors = {};
    if (!formData.title.trim()) {
      errors.title = "Tiêu đề bài viết không được để trống";
    }
    if (!formData.categoryId) {
      errors.categoryId = "Danh mục không được để trống";
    }
    if (formData.categoryId && Number.isNaN(Number(formData.categoryId))) {
      errors.categoryId = "Category ID phải là số";
    }
    if (!formData.content.trim()) {
      errors.content = "Nội dung bài viết không được để trống";
    }
    if (!formData.img) {
      errors.img = "Vui lòng nhập đường dẫn ảnh bài viết";
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Submit form
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    try {
      const token = localStorage.getItem("accessToken");
      const decode = jwtDecode(token);
      let id = null;
      id = decode["sub"];

      if (token) setAuthToken(token);

      const selectedCategory = categories.find(
        (category) =>
          String(category.categoryId) === String(formData.categoryId)
      );
      const categoryName = selectedCategory?.name || formData.category || "";
      const trimmedCategoryName = categoryName.trim();
      const categoryIdNumber = Number(formData.categoryId);
      const hasValidCategoryId = !Number.isNaN(categoryIdNumber);

      // Prepare data
      const submitData = {
        title: formData.title.trim(),
        category: trimmedCategoryName,
        content: formData.content.trim(),
        img: formData.img,
        tag: trimmedCategoryName,
        authorId: id,
      };

      if (hasValidCategoryId) {
        submitData.categoryId = categoryIdNumber;
      }

      if (isEditMode) {
        if (editingBlogId === null || editingBlogId === undefined) {
          throw new Error("Không tìm thấy ID bài viết để cập nhật");
        }
        submitData.thumbnailUrl = formData.img;
        await BlogService.updateBlog(editingBlogId, submitData);
        toast.success("Cập nhật bài viết thành công!");
      } else {
        await BlogService.createBlog(submitData);
        toast.success("Tạo bài viết mới thành công!");
      }

      closeModal();
      loadBlogs();
    } catch (error) {
      console.error("Error saving blog:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi lưu bài viết. Vui lòng thử lại!"
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  // Delete handlers
  const openDeleteModal = (blog) => {
    setBlogToDelete(blog);
    setIsDeleteOpen(true);
  };

  const closeDeleteModal = () => {
    setIsDeleteOpen(false);
    setBlogToDelete(null);
    setIsDeleting(false);
  };

  const confirmDelete = async () => {
    if (!blogToDelete) return;
    const blogId = resolveBlogId(blogToDelete);
    if (!blogId && blogId !== 0) {
      toast.error("Không xác định được ID bài viết để xóa");
      return;
    }

    setIsDeleting(true);
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      await BlogService.deleteBlog(blogId);
      toast.success("Xóa bài viết thành công!");
      closeDeleteModal();
      loadBlogs();
    } catch (error) {
      console.error("Error deleting blog:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi xóa bài viết. Vui lòng thử lại!"
      );
    } finally {
      setIsDeleting(false);
    }
  };

  // Approve blog handler
  const handleApproveBlog = async (blog) => {
    const blogId = resolveBlogId(blog);
    if (!blogId && blogId !== 0) {
      toast.error("Không xác định được ID bài viết để duyệt");
      return;
    }
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      await BlogService.approveBlog(blogId);
      toast.success("Đã duyệt bài viết thành công!");
      loadBlogs();
    } catch (error) {
      console.error("Error approving blog:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi duyệt bài viết. Vui lòng thử lại!"
      );
    }
  };

  // Reject blog handler
  const handleRejectBlog = async (blog) => {
    const blogId = resolveBlogId(blog);
    if (!blogId && blogId !== 0) {
      toast.error("Không xác định được ID bài viết để hủy");
      return;
    }
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      await BlogService.rejectBlog(blogId);
      toast.success("Đã hủy bài viết thành công!");
      loadBlogs();
    } catch (error) {
      console.error("Error rejecting blog:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi hủy bài viết. Vui lòng thử lại!"
      );
    }
  };

  // View detail handlers
  const openViewDetailModal = (blog) => {
    setViewingBlog(blog);
    setIsViewDetailOpen(true);
  };

  const closeViewDetailModal = () => {
    setIsViewDetailOpen(false);
    setViewingBlog(null);
  };

  return (
    <AdminLayout pageTitle="Quản lý Blog" breadcrumbs={breadcrumbs}>
      <div className="blogs-management-content">
        <div className="blogs-header">
          <div>
            <h1 className="blogs-title">Quản lý Blog</h1>
            <p className="blogs-subtitle">
              Quản lý, phê duyệt và xuất bản các bài viết blog
            </p>
          </div>
          <button className="blogs-create-button" onClick={openCreateModal}>
            <FiPlus /> Tạo bài viết mới
          </button>
        </div>

        {/* Statistics Cards */}
        <div className="blogs-stats-cards">
          <div className="blogs-stat-card">
            <div className="blogs-stat-icon" style={{ color: "#3b82f6" }}>
              <FiBook />
            </div>
            <div className="blogs-stat-content">
              <div className="blogs-stat-title">Tổng bài viết</div>
              <div className="blogs-stat-value">{stats.total}</div>
              <div className="blogs-stat-change">+3 bài mới</div>
            </div>
          </div>

          <div className="blogs-stat-card">
            <div className="blogs-stat-icon" style={{ color: "#10b981" }}>
              <FiEye />
            </div>
            <div className="blogs-stat-content">
              <div className="blogs-stat-title">Bài đã phê duyệt</div>
              <div className="blogs-stat-value">{stats.approved}</div>
              <div className="blogs-stat-change">Đang hoạt động</div>
            </div>
          </div>

          <div className="blogs-stat-card">
            <div className="blogs-stat-icon" style={{ color: "#8b5cf6" }}>
              <FiTrendingUp />
            </div>
            <div className="blogs-stat-content">
              <div className="blogs-stat-title">Tổng lượt xem</div>
              <div className="blogs-stat-value">
                {(stats.views / 1000).toFixed(1)}K
              </div>
              <div className="blogs-stat-change">+15% tuần này</div>
            </div>
          </div>

          <div className="blogs-stat-card">
            <div className="blogs-stat-icon" style={{ color: "#f59e0b" }}>
              <FiMessageCircle />
            </div>
            <div className="blogs-stat-content">
              <div className="blogs-stat-title">Bình luận</div>
              <div className="blogs-stat-value">{stats.comments}</div>
              <div className="blogs-stat-change">+23 bình luận mới</div>
            </div>
          </div>
        </div>

        {/* Blog List Section */}
        <div className="blogs-list-section">
          <div className="blogs-list-header">
            <h2 className="blogs-list-title">Danh sách bài viết</h2>
            <div className="blogs-list-controls">
              <div className="blogs-filter-tabs">
                <button
                  className={`blogs-filter-tab ${
                    filter === "all" ? "active" : ""
                  }`}
                  onClick={() => setFilter("all")}
                >
                  Tất cả
                </button>
                <button
                  className={`blogs-filter-tab ${
                    filter === "approved" ? "active" : ""
                  }`}
                  onClick={() => setFilter("approved")}
                >
                  Đã duyệt
                </button>
                <button
                  className={`blogs-filter-tab ${
                    filter === "pending" ? "active" : ""
                  }`}
                  onClick={() => setFilter("pending")}
                >
                  Chờ duyệt
                </button>
                <button
                  className={`blogs-filter-tab ${
                    filter === "rejected" ? "active" : ""
                  }`}
                  onClick={() => setFilter("rejected")}
                >
                  Đã hủy
                </button>
              </div>
              <div className="blogs-search-bar">
                <FiSearch />
                <input
                  type="text"
                  placeholder="Tìm kiếm bài viết..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
            </div>
          </div>

          <div className="blogs-table">
            <div className="blogs-table-header">
              <span>Tiêu đề</span>
              <span>Tác giả</span>
              <span>Danh mục</span>
              <span>Ngày tạo</span>
              <span>Ngày cập nhật</span>
              <span>Trạng thái</span>
              <span>Thao tác</span>
            </div>
            {loading ? (
              <div style={{ textAlign: "center", padding: "40px" }}>
                <p>Đang tải dữ liệu...</p>
              </div>
            ) : displayedBlogs.length > 0 ? (
              displayedBlogs.map((blog) => (
                <div className="blogs-table-row" key={blog.id}>
                  <span className="blogs-table-title">{blog.title}</span>
                  <span>{blog.author || "Unknown"}</span>
                  <span>{blog.category}</span>
                  <span>{formatDate1(blog.createdDate) || "Chưa có"}</span>
                  <span>
                    {formatDate1(blog.updatedDate) || "Chưa Cập Nhật"}
                  </span>
                  <span>{getStatusTag(blog.status)}</span>
                  <span className="blogs-table-actions">
                    <button
                      className={`blogs-action-btn approve-btn ${
                        blog.status === "approved" ? "disabled" : ""
                      }`}
                      onClick={() => handleApproveBlog(blog)}
                      title="Duyệt bài"
                      disabled={blog.status === "approved"}
                    >
                      <FiCheck />
                    </button>
                    <button
                      className={`blogs-action-btn reject-btn ${
                        blog.status === "rejected" ? "disabled" : ""
                      }`}
                      onClick={() => handleRejectBlog(blog)}
                      title="Hủy bài"
                      disabled={blog.status === "rejected"}
                    >
                      <FiXCircle />
                    </button>
                    <button
                      className="blogs-action-btn edit-btn"
                      onClick={() => openEditModal(blog)}
                      title="Chỉnh sửa"
                    >
                      <FiEdit />
                    </button>
                    <button
                      className="blogs-action-btn view-btn"
                      onClick={() => openViewDetailModal(blog)}
                      title="Xem chi tiết"
                    >
                      <FiEye />
                    </button>
                    <button
                      className="blogs-action-btn delete-btn"
                      onClick={() => openDeleteModal(blog)}
                      title="Xóa"
                    >
                      <FiTrash2 />
                    </button>
                  </span>
                </div>
              ))
            ) : (
              <div className="blogs-no-data">
                <p>Không tìm thấy bài viết nào</p>
              </div>
            )}
          </div>

          <div className="blogs-pagination">
            <Pagination
              current={page}
              pageSize={pageSize}
              total={searchedBlogs.length}
              onChange={(newPage, newPageSize) => {
                setPage(newPage);
                if (newPageSize !== pageSize) {
                  setPageSize(newPageSize);
                }
              }}
              showSizeChanger
              showQuickJumper
              showTotal={(total, range) =>
                total > 0
                  ? `${range[0]}-${range[1]} của ${total} bài viết`
                  : "0 bài viết"
              }
              pageSizeOptions={["5", "10", "20", "50", "100"]}
            />
          </div>
        </div>
      </div>

      {/* Create/Edit Modal */}
      {isModalOpen && (
        <div className="blogs-modal-overlay" onClick={closeModal}>
          <div
            className="blogs-modal-content"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="blogs-modal-header">
              <div>
                <h2 className="blogs-modal-title">
                  {isEditMode ? "Chỉnh sửa bài viết" : "Tạo bài viết mới"}
                </h2>
                <p className="blogs-modal-subtitle">
                  {isEditMode
                    ? "Cập nhật nội dung cho bài viết hiện có"
                    : "Viết một bài viết blog mới cho trang web"}
                </p>
              </div>
              <button className="blogs-modal-close" onClick={closeModal}>
                <FiX />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="blogs-modal-form">
              {/* Image URL */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">
                  Ảnh bài viết (Image URL)
                </label>
                <input
                  type="url"
                  name="img"
                  value={formData.img}
                  onChange={handleInputChange}
                  className={`blogs-form-input ${
                    formErrors.img ? "error" : ""
                  }`}
                  placeholder="Dán đường dẫn ảnh (https://...)"
                />
                {formData.img && (
                  <div className="blogs-image-preview">
                    <img src={formData.img} alt="Preview" />
                    <button
                      type="button"
                      className="blogs-image-remove"
                      onClick={() =>
                        setFormData((prev) => ({
                          ...prev,
                          img: "",
                        }))
                      }
                    >
                      <FiX />
                    </button>
                  </div>
                )}
                {formErrors.img && (
                  <span className="blogs-form-error">{formErrors.img}</span>
                )}
              </div>

              {/* Title */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Tiêu đề bài viết</label>
                <input
                  type="text"
                  name="title"
                  value={formData.title}
                  onChange={handleInputChange}
                  className={`blogs-form-input ${
                    formErrors.title ? "error" : ""
                  }`}
                  placeholder="Nhập tiêu đề bài viết"
                />
                {formErrors.title && (
                  <span className="blogs-form-error">{formErrors.title}</span>
                )}
              </div>

              {/* Category */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Danh mục</label>
                <select
                  name="categoryId"
                  value={formData.categoryId}
                  onChange={handleCategorySelect}
                  className={`blogs-form-input ${
                    formErrors.categoryId ? "error" : ""
                  }`}
                  disabled={categoriesLoading}
                >
                  <option value="">
                    {categoriesLoading
                      ? "Đang tải danh mục..."
                      : "Chọn danh mục"}
                  </option>
                  {!categoriesLoading &&
                    formData.categoryId &&
                    !categories.some(
                      (category) =>
                        String(category.categoryId) ===
                        String(formData.categoryId)
                    ) && (
                      <option value={formData.categoryId}>
                        {formData.category || "Danh mục hiện tại"}
                      </option>
                    )}
                  {categories.map((category) => (
                    <option
                      key={category.id ?? category.categoryId}
                      value={category.categoryId}
                    >
                      {category.name || category.categoryName}
                    </option>
                  ))}
                </select>
                {formErrors.categoryId && (
                  <span className="blogs-form-error">
                    {formErrors.categoryId}
                  </span>
                )}
              </div>

              {/* Status field now hidden for both create and edit */}

              {/* Content */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Nội dung</label>
                <textarea
                  name="content"
                  value={formData.content}
                  onChange={handleInputChange}
                  className={`blogs-form-textarea ${
                    formErrors.content ? "error" : ""
                  }`}
                  placeholder="Nhập nội dung bài viết"
                  rows={10}
                />
                {formErrors.content && (
                  <span className="blogs-form-error">{formErrors.content}</span>
                )}
              </div>

              {/* Form Actions */}
              <div className="blogs-modal-actions">
                <button
                  type="button"
                  className="blogs-modal-cancel"
                  onClick={closeModal}
                  disabled={isSubmitting}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="blogs-modal-submit"
                  disabled={isSubmitting}
                >
                  {isSubmitting
                    ? "Đang lưu..."
                    : isEditMode
                    ? "Cập nhật"
                    : "Lưu thay đổi"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {isDeleteOpen && (
        <div className="blogs-modal-overlay" onClick={closeDeleteModal}>
          <div
            className="blogs-delete-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="blogs-delete-header">
              <h3>Xác nhận xóa</h3>
            </div>
            <div className="blogs-delete-content">
              <p>
                Bạn có chắc chắn muốn xóa bài viết "
                <strong>{blogToDelete?.title}</strong>" không?
              </p>
              <p className="blogs-delete-warning">
                Hành động này không thể hoàn tác.
              </p>
            </div>
            <div className="blogs-delete-actions">
              <button
                className="blogs-delete-cancel"
                onClick={closeDeleteModal}
                disabled={isDeleting}
              >
                Hủy
              </button>
              <button
                className="blogs-delete-confirm"
                onClick={confirmDelete}
                disabled={isDeleting}
              >
                {isDeleting ? "Đang xóa..." : "Xóa"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* View Detail Modal */}
      {isViewDetailOpen && viewingBlog && (
        <div className="blogs-modal-overlay" onClick={closeViewDetailModal}>
          <div
            className="blogs-view-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="blogs-modal-header">
              <div>
                <h2 className="blogs-modal-title">Chi tiết bài viết</h2>
                <p className="blogs-modal-subtitle">
                  Xem thông tin chi tiết của bài viết
                </p>
              </div>
              <button
                className="blogs-modal-close"
                onClick={closeViewDetailModal}
              >
                <FiX />
              </button>
            </div>

            <div className="blogs-view-content">
              {viewingBlog.img && (
                <div className="blogs-view-image">
                  <img src={viewingBlog.img} alt={viewingBlog.title} />
                </div>
              )}

              <div className="blogs-view-info">
                <div className="blogs-view-row">
                  <label>Tiêu đề:</label>
                  <span>{viewingBlog.title}</span>
                </div>

                <div className="blogs-view-row">
                  <label>Tác giả:</label>
                  <span>{viewingBlog.author}</span>
                </div>

                <div className="blogs-view-row">
                  <label>Danh mục:</label>
                  <span>{viewingBlog.category}</span>
                </div>

                <div className="blogs-view-row">
                  <label>Trạng thái:</label>
                  <span>{getStatusTag(viewingBlog.status)}</span>
                </div>

                <div className="blogs-view-row">
                  <label>Ngày tạo:</label>
                  <span>
                    {formatDate1(viewingBlog.createdDate) || "Chưa có"}
                  </span>
                </div>

                <div className="blogs-view-row">
                  <label>Ngày cập nhật:</label>
                  <span>
                    {formatDate1(viewingBlog.updatedDate) || "Chưa cập nhật"}
                  </span>
                </div>

                <div className="blogs-view-row blogs-view-content-section">
                  <label>Nội dung:</label>
                  <div className="blogs-view-content-text">
                    {viewingBlog.content}
                  </div>
                </div>
              </div>

              <div className="blogs-view-actions">
                <button
                  className="blogs-modal-cancel"
                  onClick={closeViewDetailModal}
                >
                  Đóng
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default BlogsManagement;
