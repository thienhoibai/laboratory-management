import React, { useState, useEffect } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import {
  FiSearch,
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiBook,
  FiEye,
  FiTrendingUp,
  FiMessageCircle,
  FiX,
  FiUpload,
  FiImage,
} from "react-icons/fi";
import { Pagination } from "antd";
import { toast } from "react-toastify";
import { categories } from "../../../data/blog";
import BlogService from "../../../services/BlogService";
import { setAuthToken } from "../../../utils/auth";
import "./BlogsManagement.css";

const BlogsManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Quản lý Blog" },
  ];

  const [filter, setFilter] = useState("all");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Blogs state
  const [blogs, setBlogs] = useState([]);
  const [loading, setLoading] = useState(false);

  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [editingBlogId, setEditingBlogId] = useState(null);
  const [formData, setFormData] = useState({
    title: "",
    author: "",
    category: "",
    status: "draft",
    content: "",
    img: "",
    imgFile: null,
  });
  const [formErrors, setFormErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Delete modal states
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [blogToDelete, setBlogToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Load blogs on mount
  useEffect(() => {
    loadBlogs();
  }, []);

  const loadBlogs = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      const blogsData = await BlogService.getAllBlogs();
      setBlogs(blogsData);
    } catch (error) {
      console.error("Error loading blogs:", error);
      toast.error("Không thể tải danh sách bài viết. Vui lòng thử lại!");
    } finally {
      setLoading(false);
    }
  };

  const filteredBlogs = blogs.filter((blog) => {
    if (filter === "all") {
      return true;
    }
    return blog.status === filter;
  });

  const searchedBlogs = filteredBlogs.filter((blog) =>
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
      approved: { text: "Đã phê duyệt", class: "status-approved" },
      pending: { text: "Chờ phê duyệt", class: "status-pending" },
      draft: { text: "Nháp", class: "status-draft" },
    };
    const statusInfo = statusMap[status] || statusMap.draft;
    return (
      <span className={`blogs-status-tag ${statusInfo.class}`}>
        {statusInfo.text}
      </span>
    );
  };

  // Modal handlers
  const openCreateModal = () => {
    setIsEditMode(false);
    setEditingBlogId(null);
    setFormData({
      title: "",
      author: "",
      category: "",
      status: "draft",
      content: "",
      img: "",
      imgFile: null,
    });
    setFormErrors({});
    setIsModalOpen(true);
  };

  const openEditModal = (blog) => {
    setIsEditMode(true);
    setEditingBlogId(blog.id);
    setFormData({
      title: blog.title || "",
      author: blog.author || "",
      category: blog.category || blog.tag || "",
      status: blog.status || "draft",
      content: blog.content || "",
      img: blog.img || "",
      imgFile: null,
    });
    setFormErrors({});
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setIsEditMode(false);
    setEditingBlogId(null);
    setFormData({
      title: "",
      author: "",
      category: "",
      status: "draft",
      content: "",
      img: "",
      imgFile: null,
    });
    setFormErrors({});
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

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        // 5MB limit
        toast.error("Kích thước ảnh không được vượt quá 5MB");
        return;
      }
      if (!file.type.startsWith("image/")) {
        toast.error("File phải là hình ảnh");
        return;
      }
      const reader = new FileReader();
      reader.onloadend = () => {
        setFormData((prev) => ({
          ...prev,
          imgFile: file,
          img: reader.result,
        }));
      };
      reader.readAsDataURL(file);
    }
  };

  // Form validation
  const validateForm = () => {
    const errors = {};
    if (!formData.title.trim()) {
      errors.title = "Tiêu đề bài viết không được để trống";
    }
    if (!formData.author.trim()) {
      errors.author = "Tác giả không được để trống";
    }
    if (!formData.category.trim()) {
      errors.category = "Danh mục không được để trống";
    }
    if (!formData.content.trim()) {
      errors.content = "Nội dung bài viết không được để trống";
    }
    if (!isEditMode && !formData.img && !formData.imgFile) {
      errors.img = "Vui lòng chọn ảnh bài viết";
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
      if (token) setAuthToken(token);

      // Prepare data
      const submitData = {
        title: formData.title.trim(),
        author: formData.author.trim(),
        category: formData.category.trim(),
        status: formData.status,
        content: formData.content.trim(),
        img: formData.img,
        tag: formData.category.trim(),
      };

      if (isEditMode) {
        // Update blog
        await BlogService.updateBlog(editingBlogId, submitData);
        toast.success("Cập nhật bài viết thành công!");
      } else {
        // Create blog
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

    setIsDeleting(true);
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);

      await BlogService.deleteBlog(blogToDelete.id);
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
                  Đã phê duyệt
                </button>
                <button
                  className={`blogs-filter-tab ${
                    filter === "pending" ? "active" : ""
                  }`}
                  onClick={() => setFilter("pending")}
                >
                  Chờ phê duyệt
                </button>
                <button
                  className={`blogs-filter-tab ${
                    filter === "draft" ? "active" : ""
                  }`}
                  onClick={() => setFilter("draft")}
                >
                  Nháp
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
              <span>Ngày đăng</span>
              <span>Trạng thái</span>
              <span>Lượt xem</span>
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
                  <span>{blog.author}</span>
                  <span>{blog.date}</span>
                  <span>{getStatusTag(blog.status)}</span>
                  <span>{blog.views}</span>
                  <span className="blogs-table-actions">
                    <FiEdit2
                      onClick={() => openEditModal(blog)}
                      style={{ cursor: "pointer" }}
                      title="Chỉnh sửa"
                    />
                    <FiTrash2
                      onClick={() => openDeleteModal(blog)}
                      style={{ cursor: "pointer" }}
                      title="Xóa"
                    />
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
              pageSizeOptions={["10", "20", "50", "100"]}
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
                    ? "Cập nhật thông tin bài viết blog"
                    : "Viết một bài viết blog mới cho trang web"}
                </p>
              </div>
              <button className="blogs-modal-close" onClick={closeModal}>
                <FiX />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="blogs-modal-form">
              {/* Image Upload */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Ảnh bài viết</label>
                <div className="blogs-image-upload">
                  {formData.img ? (
                    <div className="blogs-image-preview">
                      <img src={formData.img} alt="Preview" />
                      <button
                        type="button"
                        className="blogs-image-remove"
                        onClick={() =>
                          setFormData((prev) => ({
                            ...prev,
                            img: "",
                            imgFile: null,
                          }))
                        }
                      >
                        <FiX />
                      </button>
                    </div>
                  ) : (
                    <label className="blogs-image-upload-button">
                      <FiUpload />
                      <span>Chọn ảnh bài viết</span>
                      <input
                        type="file"
                        accept="image/*"
                        onChange={handleImageChange}
                        style={{ display: "none" }}
                      />
                    </label>
                  )}
                </div>
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

              {/* Author */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Tác giả</label>
                <input
                  type="text"
                  name="author"
                  value={formData.author}
                  onChange={handleInputChange}
                  className={`blogs-form-input ${
                    formErrors.author ? "error" : ""
                  }`}
                  placeholder="Nhập tên tác giả"
                />
                {formErrors.author && (
                  <span className="blogs-form-error">{formErrors.author}</span>
                )}
              </div>

              {/* Category */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Danh mục</label>
                <select
                  name="category"
                  value={formData.category}
                  onChange={handleInputChange}
                  className={`blogs-form-input ${
                    formErrors.category ? "error" : ""
                  }`}
                >
                  <option value="">Chọn danh mục</option>
                  {categories
                    .filter((cat) => cat !== "Tất cả")
                    .map((cat) => (
                      <option key={cat} value={cat}>
                        {cat}
                      </option>
                    ))}
                </select>
                {formErrors.category && (
                  <span className="blogs-form-error">
                    {formErrors.category}
                  </span>
                )}
              </div>

              {/* Status */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">Trạng thái</label>
                <select
                  name="status"
                  value={formData.status}
                  onChange={handleInputChange}
                  className="blogs-form-input"
                >
                  <option value="draft">Nháp</option>
                  <option value="pending">Chờ phê duyệt</option>
                  <option value="approved">Đã phê duyệt</option>
                </select>
              </div>

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
                  {isSubmitting ? "Đang lưu..." : "Lưu thay đổi"}
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
    </AdminLayout>
  );
};

export default BlogsManagement;
