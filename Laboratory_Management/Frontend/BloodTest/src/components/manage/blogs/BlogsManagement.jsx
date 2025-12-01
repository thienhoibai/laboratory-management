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
  FiUpload,
  FiImage,
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
    imageFile: null, // File object for upload
  });
  const [imagePreview, setImagePreview] = useState(""); // Preview URL
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

  const token = localStorage.getItem("accessToken");
  const decode = jwtDecode(token);
  let role = null;
  role = decode["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

  // Load data on mount and when filter changes
  useEffect(() => {
    loadBlogs();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  useEffect(() => {
    // Only trigger search when user stops typing (debounce effect)
    if (search.trim() !== "") {
      loadBlogs();
    } else {
      // Clear search, reload with current filter
      loadBlogs();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  useEffect(() => {
    loadCategories();
  }, []);

  // Cleanup preview URL on unmount or when modal closes
  useEffect(() => {
    return () => {
      if (imagePreview && imagePreview.startsWith("blob:")) {
        URL.revokeObjectURL(imagePreview);
      }
    };
  }, [imagePreview]);

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

      // Always apply status filter (except for "all")
      if (filter !== "all") {
        params.status = statusMap[filter];
      }

      // Add search parameter if search term exists
      if (search && search.trim() !== "") {
        params.search = search.trim();
      }
      if (role === "Manager" || role === "Admin") {
        const blogsData = await BlogService.getAllBlogs(params);
        setBlogs(blogsData);
      } else if (role === "Staff") {
        const decode = jwtDecode(token);
        let id = null;
        id = decode["sub"];
        params.authorId = id;
        const blogsData = await BlogService.getAllBlogs(params);
        setBlogs(blogsData);
      }
    } catch (error) {
("Error loading blogs:", error);
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
("Error loading categories:", error);
      toast.error("Không thể tải danh mục. Vui lòng thử lại!");
    } finally {
      setCategoriesLoading(false);
    }
  };

  // API handles both status filter and search
  // Just slice for pagination
  const displayedBlogs = blogs.slice((page - 1) * pageSize, page * pageSize);

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
      imageFile: null,
    });
    setImagePreview("");
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
      imageFile: null,
    });
    setImagePreview("");
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
    const existingImageUrl = blog.img || blog.thumbnailUrl || "";
    setFormData({
      title: blog.title || "",
      category: blog.category || "",
      categoryId:
        blog.categoryId !== undefined && blog.categoryId !== null
          ? String(blog.categoryId)
          : "",
      status: blog.status || "pending",
      content: blog.content || "",
      img: existingImageUrl,
      imageFile: null, // Reset file when editing
    });
    setImagePreview(existingImageUrl); // Show existing image as preview
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

  // Handle file selection
  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) {
      setFormData((prev) => ({ ...prev, imageFile: null }));
      setImagePreview(isEditMode ? formData.img : "");
      return;
    }

    // Validate file type
    if (!file.type.startsWith("image/")) {
      setFormErrors((prev) => ({
        ...prev,
        img: "Vui lòng chọn file ảnh hợp lệ",
      }));
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      setFormErrors((prev) => ({
        ...prev,
        img: "Kích thước file không được vượt quá 5MB",
      }));
      return;
    }

    // Create preview URL
    const previewUrl = URL.createObjectURL(file);
    setImagePreview(previewUrl);
    setFormData((prev) => ({ ...prev, imageFile: file }));
    
    // Clear error
    if (formErrors.img) {
      setFormErrors((prev) => ({
        ...prev,
        img: "",
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
    // Validate image: require file for create, optional for edit
    if (!isEditMode && !formData.imageFile) {
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

      // Prepare data with imageFile
      const submitData = {
        title: formData.title.trim(),
        category: trimmedCategoryName,
        content: formData.content.trim(),
        tag: trimmedCategoryName,
        authorId: id,
        imageFile: formData.imageFile, // Include file if exists
      };

      if (hasValidCategoryId) {
        submitData.categoryId = categoryIdNumber;
      }

      if (isEditMode) {
        if (editingBlogId === null || editingBlogId === undefined) {
          throw new Error("Không tìm thấy ID bài viết để cập nhật");
        }
        await BlogService.updateBlog(editingBlogId, submitData);
        toast.success("Cập nhật bài viết thành công!");
      } else {
        await BlogService.createBlog(submitData);
        toast.success("Tạo bài viết mới thành công!");
      }

      closeModal();
      loadBlogs();
    } catch (error) {
("Error saving blog:", error);
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
("Error deleting blog:", error);
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
("Error approving blog:", error);
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
("Error rejecting blog:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi hủy bài viết. Vui lòng thử lại!"
      );
    }
  };

  // View detail handlers
  const openViewDetailModal = async (blog) => {
    const blogId = resolveBlogId(blog);
    if (!blogId && blogId !== 0) {
      toast.error("Không xác định được ID bài viết để xem chi tiết");
      return;
    }
    
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      
      // Fetch fresh blog data from API to ensure we have the latest imagePath
      const blogDetail = await BlogService.getBlogById(blogId);
      
      // Check if blogDetail is valid
      if (!blogDetail || Object.keys(blogDetail).length === 0) {
        setViewingBlog(blog);
        setIsViewDetailOpen(true);
        return;
      }
      
      setViewingBlog(blogDetail);
      setIsViewDetailOpen(true);
    } catch (error) {
("Error loading blog detail:", error);
      // Fallback to using blog from list if API call fails
      setViewingBlog(blog);
      setIsViewDetailOpen(true);
      toast.warning("Không thể tải chi tiết bài viết. Hiển thị thông tin từ danh sách.");
    }
  };

  const closeViewDetailModal = () => {
    setIsViewDetailOpen(false);
    setViewingBlog(null);
  };

  const gridCols =
    role === "Admin" || role === "Manager"
      ? "2fr 1fr 1fr 1fr 1fr 0.8fr 1.5fr" // Có cột Tác giả
      : "1fr 1fr 0.75fr 0.75fr 0.75fr 1fr"; // Không có cột Tác giả

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
          {(role === "Staff" || role === "Admin") && (
            <button className="blogs-create-button" onClick={openCreateModal}>
              <FiPlus /> Tạo bài viết mới
            </button>
          )}
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
            <div
              className="blogs-table-header"
              style={{
                display: "grid",
                gridTemplateColumns: gridCols,
              }}
            >
              <span>Tiêu đề</span>

              {(role === "Admin" || role === "Manager") && <span>Tác giả</span>}
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
                <div
                  className="blogs-table-row"
                  key={blog.id}
                  style={{
                    display: "grid",
                    gridTemplateColumns: gridCols,
                  }}
                >
                  <span className="blogs-table-title">{blog.title}</span>

                  {(role === "Admin" || role === "Manager") && (
                    <span>{blog.author || ""}</span>
                  )}

                  <span>{blog.category}</span>
                  <span>{formatDate1(blog.createdDate) || "Chưa có"}</span>
                  <span>
                    {formatDate1(blog.updatedDate) || "Chưa cập nhật"}
                  </span>
                  <span>{getStatusTag(blog.status)}</span>
                  <span className="blogs-table-actions">
                    {(role === "Manager" || role === "Admin") && (
                      <>
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
                      </>
                    )}
                    <button
                      className="blogs-action-btn view-btn"
                      onClick={() => openViewDetailModal(blog)}
                      title="Xem chi tiết"
                    >
                      <FiEye />
                    </button>
                    {(role === "Staff" || role === "Admin") && (
                      <button
                        className="blogs-action-btn edit-btn"
                        onClick={() => openEditModal(blog)}
                        title="Chỉnh sửa"
                      >
                        <FiEdit />
                      </button>
                    )}
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
              total={blogs.length}
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
              {/* Image Upload */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">
                  <FiImage /> Ảnh bài viết {!isEditMode && <span className="required-star">*</span>}
                </label>
                {!imagePreview ? (
                  <div className="blogs-image-upload-area">
                    <input
                      type="file"
                      name="imageFile"
                      id="imageFile"
                      accept="image/*"
                      onChange={handleFileChange}
                      className="blogs-file-input-hidden"
                    />
                    <label htmlFor="imageFile" className="blogs-image-upload-label">
                      <div className="blogs-upload-icon">
                        <FiUpload />
                      </div>
                      <div className="blogs-upload-text">
                        <span className="blogs-upload-primary">Nhấp để tải ảnh lên</span>
                        <span className="blogs-upload-secondary">
                          hoặc kéo thả ảnh vào đây
                        </span>
                      </div>
                      <span className="blogs-upload-hint">
                        JPG, PNG (tối đa 5MB)
                      </span>
                    </label>
                  </div>
                ) : (
                  <div className="blogs-image-preview-wrapper">
                    <div className="blogs-image-preview">
                      <img src={imagePreview} alt="Preview" />
                    </div>
                    <button
                      type="button"
                      className="blogs-image-change-btn"
                      onClick={() => document.getElementById("imageFile")?.click()}
                    >
                      <FiUpload /> Thay đổi ảnh
                    </button>
                    <input
                      type="file"
                      name="imageFile"
                      id="imageFile"
                      accept="image/*"
                      onChange={handleFileChange}
                      className="blogs-file-input-hidden"
                    />
                  </div>
                )}
                {formErrors.img && (
                  <span className="blogs-form-error">{formErrors.img}</span>
                )}
              </div>

              {/* Title */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">
                  Tiêu đề bài viết <span className="required-star">*</span>
                </label>
                <input
                  type="text"
                  name="title"
                  value={formData.title}
                  onChange={handleInputChange}
                  className={`blogs-form-input ${
                    formErrors.title ? "error" : ""
                  }`}
                  placeholder="Nhập tiêu đề bài viết..."
                />
                {formErrors.title && (
                  <span className="blogs-form-error">{formErrors.title}</span>
                )}
              </div>

              {/* Category */}
              <div className="blogs-form-group">
                <label className="blogs-form-label">
                  Danh mục <span className="required-star">*</span>
                </label>
                <select
                  name="categoryId"
                  value={formData.categoryId}
                  onChange={handleCategorySelect}
                  className={`blogs-form-input blogs-form-select ${
                    formErrors.categoryId ? "error" : ""
                  }`}
                  disabled={categoriesLoading}
                >
                  <option value="">
                    {categoriesLoading
                      ? "Đang tải danh mục..."
                      : "Chọn danh mục..."}
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
                <label className="blogs-form-label">
                  Nội dung <span className="required-star">*</span>
                </label>
                <textarea
                  name="content"
                  value={formData.content}
                  onChange={handleInputChange}
                  className={`blogs-form-textarea ${
                    formErrors.content ? "error" : ""
                  }`}
                  placeholder="Nhập nội dung bài viết..."
                  rows={8}
                />
                <div className="blogs-textarea-footer">
                  <span className="blogs-char-count">
                    {formData.content.length} ký tự
                  </span>
                </div>
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
                  {isSubmitting ? (
                    <>
                      <span className="blogs-spinner"></span>
                      Đang xử lý...
                    </>
                  ) : isEditMode ? (
                    <>
                      <FiCheck /> Cập nhật bài viết
                    </>
                  ) : (
                    <>
                      <FiPlus /> Tạo bài viết
                    </>
                  )}
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
              {(() => {
                // Get image URL - check all possible fields including imagePath
                const imageUrl = viewingBlog.img || 
                                viewingBlog.thumbnailUrl || 
                                viewingBlog.imageUrl ||
                                (viewingBlog.imagePath ? BlogService.buildImageUrl(viewingBlog.imagePath) : null);
                
                return imageUrl ? (
                  <div className="blogs-view-image">
                    <img 
                      src={imageUrl} 
                      alt={viewingBlog.title || "Blog image"}
                      onError={(e) => {
                        const img = e.target;
                        const container = img.closest('.blogs-view-image');
                        if (container) {
                          img.style.display = 'none';
                          const errorDiv = container.querySelector('.blogs-image-error');
                          if (errorDiv) {
                            errorDiv.classList.add('show');
                          }
                        }
                      }}
                    />
                    <div className="blogs-image-error">
                      <FiImage />
                      <span>Không thể tải hình ảnh</span>
                    </div>
                  </div>
                ) : null;
              })()}

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
