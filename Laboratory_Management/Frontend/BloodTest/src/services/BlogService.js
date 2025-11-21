import BlogAPI from "../apis/BlogAPI";
import CategoryAPI from "../apis/CategoryAPI";

/**
 * Blog Service
 * Business logic layer for blog operations
 * Transforms data between API and UI format
 */

const BlogService = {
  STATUS_TEXT_BY_CODE: {
    0: "pending",
    1: "approved",
    2: "rejected",
  },
  STATUS_CODE_BY_TEXT: {
    pending: 0,
    approved: 1,
    rejected: 2,
  },

  mapStatusFromAPI(status) {
    if (typeof status === "number") {
      return BlogService.STATUS_TEXT_BY_CODE[status] || "pending";
    }
    if (typeof status === "string" && status.trim() !== "") {
      const normalized = status.trim().toLowerCase();
      if (
        Object.prototype.hasOwnProperty.call(
          BlogService.STATUS_CODE_BY_TEXT,
          normalized
        )
      ) {
        return normalized;
      }
      const parsed = Number(normalized);
      if (!Number.isNaN(parsed)) {
        return BlogService.STATUS_TEXT_BY_CODE[parsed] || "pending";
      }
    }
    return "pending";
  },

  mapStatusToAPI(status) {
    if (typeof status === "number") {
      return status;
    }
    if (typeof status === "string") {
      const normalized = status.trim().toLowerCase();
      if (
        Object.prototype.hasOwnProperty.call(
          BlogService.STATUS_CODE_BY_TEXT,
          normalized
        )
      ) {
        return BlogService.STATUS_CODE_BY_TEXT[normalized];
      }
      const parsed = Number(normalized);
      if (!Number.isNaN(parsed)) {
        return parsed;
      }
    }
    return 0;
  },

  /**
   * Transform API blog data to UI format
   * @param {Object} apiBlog - Blog data from API
   * @returns {Object} Transformed blog object for UI
   */
  transformBlogFromAPI: (apiBlog) => {
    const categoryName =
      apiBlog.category?.categoryName ||
      apiBlog.categoryName ||
      apiBlog.tag ||
      "";
    const status = BlogService.mapStatusFromAPI(apiBlog.status);
    return {
      id: apiBlog.blogPostId || apiBlog.postId || apiBlog.id,
      title: apiBlog.title || "",
      author: apiBlog.author || apiBlog.authorName || "Unknown",
      authorId: apiBlog.authorId || apiBlog.author?.id || "",
      categoryId: apiBlog.categoryId || apiBlog.category?.categoryId || null,
      category: categoryName,
      tag: apiBlog.tag || categoryName,
      status,
      content: apiBlog.content || "",
      img: apiBlog.imageUrl || apiBlog.thumbnailUrl || apiBlog.img || "",
      thumbnailUrl: apiBlog.thumbnailUrl || apiBlog.imageUrl || "",
      createdDate: apiBlog.createdDate || apiBlog.createdAt || "",
      updatedDate: apiBlog.updatedDate || apiBlog.updatedAt || "",
      date: apiBlog.createdDate
        ? new Date(apiBlog.createdDate).toLocaleDateString("vi-VN", {
            day: "numeric",
            month: "long",
            year: "numeric",
          })
        : new Date().toLocaleDateString("vi-VN", {
            day: "numeric",
            month: "long",
            year: "numeric",
          }),
      fullDate: apiBlog.createdDate
        ? new Date(apiBlog.createdDate).toLocaleDateString("vi-VN", {
            weekday: "long",
            day: "numeric",
            month: "long",
            year: "numeric",
          })
        : "",
      time: apiBlog.createdDate
        ? formatTimeAgo(new Date(apiBlog.createdDate))
        : "Vừa xong",
      views: apiBlog.views || 0,
      comments: apiBlog.comments || 0,
      desc:
        apiBlog.description ||
        (apiBlog.content ? apiBlog.content.substring(0, 100) + "..." : ""),
    };
  },

  /**
   * Transform UI blog data to API format
   * @param {Object} uiBlog - Blog data from UI
   * @returns {Object} Transformed blog object for API
   */
  transformBlogToAPI: (uiBlog) => {
    const status = BlogService.mapStatusToAPI(uiBlog.status);
    const hasCategoryId =
      uiBlog.categoryId !== undefined &&
      uiBlog.categoryId !== null &&
      uiBlog.categoryId !== "" &&
      !Number.isNaN(Number(uiBlog.categoryId));
    const categoryId = hasCategoryId ? Number(uiBlog.categoryId) : undefined;
    const authorId =
      uiBlog.authorId && uiBlog.authorId.trim() !== ""
        ? uiBlog.authorId.trim()
        : undefined;
    const thumbnailUrl = uiBlog.thumbnailUrl || uiBlog.img || "";

    const payload = {
      title: uiBlog.title,
      author: uiBlog.author,
      category: uiBlog.category,
      tag: uiBlog.tag || uiBlog.category,
      status,
      content: uiBlog.content,
      imageUrl: uiBlog.img,
    };

    if (thumbnailUrl) {
      payload.thumbnailUrl = thumbnailUrl;
    }
    if (authorId) {
      payload.authorId = authorId;
    }
    if (hasCategoryId && categoryId !== undefined && categoryId !== null) {
      payload.categoryId = categoryId;
    }
    if (uiBlog.updatedDate) {
      payload.updatedDate = uiBlog.updatedDate;
    }

    return payload;
  },

  /**
   * Transform API category data to UI format
   * @param {Object} apiCategory
   * @returns {{id:number|string, categoryId:number|string, name:string, description:string}}
   */
  transformCategoryFromAPI: (apiCategory) => {
    if (!apiCategory) {
      return {
        id: null,
        categoryId: null,
        name: "",
        description: "",
      };
    }
    return {
      id: apiCategory.categoryId || apiCategory.id,
      categoryId: apiCategory.categoryId || apiCategory.id,
      name: apiCategory.categoryName || apiCategory.name || "",
      description: apiCategory.description || "",
    };
  },

  /**
   * Normalize list responses coming from API
   * @param {*} apiResponse
   * @returns {Array}
   */
  extractBlogList: (apiResponse) => {
    if (!apiResponse) return [];
    if (Array.isArray(apiResponse)) return apiResponse;
    if (Array.isArray(apiResponse.items)) return apiResponse.items;
    if (Array.isArray(apiResponse.data)) return apiResponse.data;
    if (apiResponse.data && Array.isArray(apiResponse.data.items)) {
      return apiResponse.data.items;
    }
    return [];
  },

  /**
   * Normalize category list responses coming from API
   * @param {*} apiResponse
   * @returns {Array}
   */
  extractCategoryList: (apiResponse) => {
    if (!apiResponse) return [];
    if (Array.isArray(apiResponse)) return apiResponse;
    if (Array.isArray(apiResponse.items)) return apiResponse.items;
    if (Array.isArray(apiResponse.data)) return apiResponse.data;
    if (apiResponse.data && Array.isArray(apiResponse.data.items)) {
      return apiResponse.data.items;
    }
    return [];
  },

  /**
   * Get all blogs
   * @returns {Promise<Array>} Array of blogs in UI format
   */
  getAllBlogs: async () => {
    try {
      const apiResponse = await BlogAPI.getAllBlogs();
      const apiBlogs = BlogService.extractBlogList(apiResponse);
      return apiBlogs.map((blog) => BlogService.transformBlogFromAPI(blog));
    } catch (error) {
      console.error("BlogService - Error getting all blogs:", error);
      throw error;
    }
  },

  /**
   * Get all categories
   * @returns {Promise<Array>} Array of categories
   */
  getCategories: async () => {
    try {
      const apiResponse = await CategoryAPI.getAllCategories();
      const apiCategories = BlogService.extractCategoryList(apiResponse);
      return apiCategories.map((category) =>
        BlogService.transformCategoryFromAPI(category)
      );
    } catch (error) {
      console.error("BlogService - Error getting categories:", error);
      throw error;
    }
  },

  /**
   * Get blog by ID
   * @param {number} id - Blog ID
   * @returns {Promise<Object>} Blog object in UI format
   */
  getBlogById: async (id) => {
    try {
      const apiBlog = await BlogAPI.getBlogById(id);
      return BlogService.transformBlogFromAPI(apiBlog);
    } catch (error) {
      console.error(`BlogService - Error getting blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Create new blog
   * @param {Object} blogData - Blog data from UI
   * @returns {Promise<Object>} Created blog in UI format
   */
  createBlog: async (blogData) => {
    try {
      const apiData = BlogService.transformBlogToAPI(blogData);
      const createdBlog = await BlogAPI.createBlog(apiData);
      return BlogService.transformBlogFromAPI(createdBlog);
    } catch (error) {
      console.error("BlogService - Error creating blog:", error);
      throw error;
    }
  },

  /**
   * Update blog
   * @param {number} id - Blog ID
   * @param {Object} blogData - Updated blog data from UI
   * @returns {Promise<Object>} Updated blog in UI format
   */
  updateBlog: async (id, blogData) => {
    try {
      const apiData = BlogService.transformBlogToAPI(blogData);
      const updatedBlog = await BlogAPI.updateBlog(id, apiData);
      return BlogService.transformBlogFromAPI(updatedBlog);
    } catch (error) {
      console.error(`BlogService - Error updating blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Delete blog
   * @param {number} id - Blog ID
   * @returns {Promise} Delete confirmation
   */
  deleteBlog: async (id) => {
    try {
      return await BlogAPI.deleteBlog(id);
    } catch (error) {
      console.error(`BlogService - Error deleting blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Approve blog
   * @param {number} id - Blog ID
   * @returns {Promise} Approve confirmation
   */
  approveBlog: async (id) => {
    try {
      return await BlogAPI.approveBlog(id);
    } catch (error) {
      console.error(`BlogService - Error approving blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Reject blog
   * @param {number} id - Blog ID
   * @returns {Promise} Reject confirmation
   */
  rejectBlog: async (id) => {
    try {
      return await BlogAPI.rejectBlog(id);
    } catch (error) {
      console.error(`BlogService - Error rejecting blog ${id}:`, error);
      throw error;
    }
  },
};

/**
 * Format time ago helper function
 * @param {Date} date - Date to format
 * @returns {string} Formatted time string
 */
function formatTimeAgo(date) {
  const now = new Date();
  const diffInSeconds = Math.floor((now - date) / 1000);

  if (diffInSeconds < 60) return "Vừa xong";
  if (diffInSeconds < 3600)
    return `${Math.floor(diffInSeconds / 60)} phút trước`;
  if (diffInSeconds < 86400)
    return `${Math.floor(diffInSeconds / 3600)} giờ trước`;
  if (diffInSeconds < 604800)
    return `${Math.floor(diffInSeconds / 86400)} ngày trước`;
  if (diffInSeconds < 2592000)
    return `${Math.floor(diffInSeconds / 604800)} tuần trước`;
  return date.toLocaleDateString("vi-VN", {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
}

export default BlogService;
