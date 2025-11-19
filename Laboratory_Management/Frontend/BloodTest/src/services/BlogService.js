import BlogAPI from "../apis/BlogAPI";

/**
 * Blog Service
 * Business logic layer for blog operations
 * Transforms data between API and UI format
 */

const BlogService = {
  /**
   * Transform API blog data to UI format
   * @param {Object} apiBlog - Blog data from API
   * @returns {Object} Transformed blog object for UI
   */
  transformBlogFromAPI: (apiBlog) => {
    // Map status: 0 = Chờ duyệt, 1 = Đã duyệt, 2 = Đã hủy
    let statusText = "pending";
    if (apiBlog.status === 0) statusText = "pending";
    else if (apiBlog.status === 1) statusText = "approved";
    else if (apiBlog.status === 2) statusText = "rejected";

    return {
      id: apiBlog.postId,
      title: apiBlog.title || "",
      author: apiBlog.authorId || "Unknown",
      category: apiBlog.category?.categoryName || "",
      categoryId: apiBlog.categoryId,
      status: statusText,
      statusCode: apiBlog.status,
      isPublished: apiBlog.isPublished,
      isApproved: apiBlog.isApproved,
      content: apiBlog.content || "",
      img: apiBlog.thumbnailUrl || "",
      thumbnailUrl: apiBlog.thumbnailUrl || "",
      createdDate: apiBlog.createdDate
        ? new Date(apiBlog.createdDate).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
          })
        : "",
      updatedDate: apiBlog.updatedDate
        ? new Date(apiBlog.updatedDate).toLocaleDateString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
          })
        : "",
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
    return {
      title: uiBlog.title,
      author: uiBlog.author,
      category: uiBlog.category,
      tag: uiBlog.tag || uiBlog.category,
      status: uiBlog.status,
      content: uiBlog.content,
      imageUrl: uiBlog.img,
    };
  },

  /**
   * Get all blogs
   * @returns {Promise<Array>} Array of blogs in UI format
   */
  getAllBlogs: async () => {
    try {
      const apiBlogs = await BlogAPI.getAllBlogs();

      // Handle different response structures
      let blogsArray = apiBlogs;

      // If response has $values property (C# serialization)
      if (apiBlogs && apiBlogs.$values) {
        blogsArray = apiBlogs.$values;
      }
      // If response has data property
      else if (apiBlogs && apiBlogs.data) {
        blogsArray = apiBlogs.data;
      }
      // If response is not an array, return empty array
      else if (!Array.isArray(apiBlogs)) {
        console.warn("API response is not an array:", apiBlogs);
        return [];
      }

      return blogsArray.map((blog) => BlogService.transformBlogFromAPI(blog));
    } catch (error) {
      console.error("BlogService - Error getting all blogs:", error);
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
