import BlogAPI from "../apis/BlogAPI";
import { formatDate1 } from "../utils/formatDate";
import { getUserById } from "./IAMService.jsx";
import { setAuthToken } from "../utils/auth";

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
   * Helper function to build full image URL
   * @param {string} imagePath - Image path from API (can be relative or absolute)
   * @returns {string} Full image URL
   */
  buildImageUrl: (imagePath) => {
    if (!imagePath || imagePath.trim() === "") return "";
    
    const trimmedPath = imagePath.trim();
    const baseURL = "http://localhost:8080";
    
    // If already a full URL (starts with http:// or https://), return as is
    if (trimmedPath.startsWith("http://") || trimmedPath.startsWith("https://")) {
      return trimmedPath;
    }
    
    // If starts with /, it's a relative path from root
    if (trimmedPath.startsWith("/")) {
      return `${baseURL}${trimmedPath}`;
    }
    
    // If path starts with "Images/", append directly to /blog/
    // Example: "Images/f4a44ab4-81cf-47de-9555-d67ccf02fbb1.jpg"
    // Result: http://localhost:8080/blog/Images/f4a44ab4-81cf-47de-9555-d67ccf02fbb1.jpg
    if (trimmedPath.startsWith("Images/")) {
      // Build URL: http://localhost:8080/blog/Images/...
      return `${baseURL}/blog/${trimmedPath}`;
    }
    
    // Otherwise, assume it's just a filename and try common paths
    return `${baseURL}/blog/api/BlogPost/Images/${trimmedPath}`;
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
    
    // Get image URL from various possible fields (including imagePath from API)
    const rawImageUrl = apiBlog.imagePath || apiBlog.imageUrl || apiBlog.thumbnailUrl || apiBlog.img || apiBlog.image || "";
    const imageUrl = BlogService.buildImageUrl(rawImageUrl);
    
    // Debug: Log image URL for troubleshooting (only if image exists)
    if (rawImageUrl) {
      console.log("📸 Blog Image Debug:", {
        blogId: apiBlog.blogPostId || apiBlog.postId || apiBlog.id,
        title: apiBlog.title,
        rawImageUrl,
        builtImageUrl: imageUrl,
        allImageFields: {
          imagePath: apiBlog.imagePath,
          imageUrl: apiBlog.imageUrl,
          thumbnailUrl: apiBlog.thumbnailUrl,
          img: apiBlog.img,
          image: apiBlog.image,
        },
      });
    }
    
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
      img: imageUrl,
      thumbnailUrl: imageUrl,
      imageUrl: imageUrl,
      createdDate: apiBlog.createdDate ? formatDate1(apiBlog.createdDate) : "",
      updatedDate: apiBlog.updatedDate ? formatDate1(apiBlog.updatedDate) : "",
      date: apiBlog.createdDate
        ? formatDate1(apiBlog.createdDate)
        : formatDate1(new Date().toISOString()),
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
   * Build FormData for blog API (backend requires multipart/form-data)
   * @param {Object} uiBlog - Blog data from UI
   * @returns {FormData} FormData with blog fields
   */
  buildBlogPayload: (uiBlog) => {
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

    const formData = new FormData();
    
    // Backend expect: Title, Content, CategoryId, AuthorId, Image
    if (uiBlog.title) {
      formData.append("Title", uiBlog.title);
    }
    if (uiBlog.content) {
      formData.append("Content", uiBlog.content);
    }
    if (categoryId !== undefined && categoryId !== null) {
      formData.append("CategoryId", categoryId);
    }
    if (authorId) {
      formData.append("AuthorId", authorId);
    }
    
    // Only append Image if there's a file
    // For update without new image, backend will keep existing image
    if (uiBlog.imageFile instanceof File) {
      formData.append("Image", uiBlog.imageFile);
    }

    return formData;
  },

  /**
   * Transform UI blog data to API format (deprecated - use buildBlogPayload instead)
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
   * Enrich blog with author full name from IAM service
   * @param {Object} blog - Blog object
   * @returns {Promise<Object>} Blog with author full name
   */
  enrichBlogWithAuthor: async (blog) => {
    if (blog.authorId) {
      try {
        const userData = await getUserById(blog.authorId);

        if (userData) {
          // Handle different field names for full name
          const fullName =
            userData.fullName ||
            userData.FullName ||
            userData.name ||
            userData.Name;

          if (fullName) {
            return { ...blog, author: fullName };
          }
        }
      } catch (error) {
        console.error(`Error fetching author for blog ${blog.id}:`, error);
      }
    }
    return blog;
  },

  /**
   * Get all blogs
   * @param {Object} params - Query parameters
   * @param {number} params.status - Status filter (0: pending, 1: approved, 2: rejected)
   * @param {string} params.authorId - Author ID filter
   * @returns {Promise<Array>} Array of blogs in UI format
   */
  getAllBlogs: async (params = {}) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiResponse = await BlogAPI.getAllBlogs(params);
      const apiBlogs = BlogService.extractBlogList(apiResponse);
      const transformedBlogs = apiBlogs.map((blog) =>
        BlogService.transformBlogFromAPI(blog)
      );

      // Enrich blogs with author names in parallel
      const enrichedBlogs = await Promise.all(
        transformedBlogs.map((blog) => BlogService.enrichBlogWithAuthor(blog))
      );

      return enrichedBlogs;
    } catch (error) {
      console.error("BlogService - Error getting all blogs:", error);
      throw error;
    }
  },

  getAllBlogsById: async (authorId) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiResponse = await BlogAPI.getAllBlogs({ authorId });
      if (apiResponse.status >= 200 && apiResponse.status < 300) {
        const apiBlogs = BlogService.extractBlogList(apiResponse);
        const transformedBlogs = apiBlogs.map((blog) =>
          BlogService.transformBlogFromAPI(blog)
        );

        // Enrich blogs with author names in parallel
        const enrichedBlogs = await Promise.all(
          transformedBlogs.map((blog) => BlogService.enrichBlogWithAuthor(blog))
        );
        return enrichedBlogs;
      }
    } catch (error) {
      console.error("BlogService - Error getting all blogs:", error);
      throw error;
    }
  },

  /**
   * Get approved blogs (for public display)
   * @param {number} page - Page number
   * @param {number} pageSize - Page size
   * @returns {Promise<Array>} Array of approved blogs in UI format
   */
  getApprovedBlogs: async (page = 1, pageSize = 100) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiResponse = await BlogAPI.getApprovedBlogs(page, pageSize);
      const apiBlogs = BlogService.extractBlogList(apiResponse);
      const transformedBlogs = apiBlogs.map((blog) =>
        BlogService.transformBlogFromAPI(blog)
      );

      // Enrich blogs with author names in parallel
      const enrichedBlogs = await Promise.all(
        transformedBlogs.map((blog) => BlogService.enrichBlogWithAuthor(blog))
      );
      
      // Sort by blogPostId/id descending (newest first - highest ID)
      enrichedBlogs.sort((a, b) => {
        const idA = a.id || 0;
        const idB = b.id || 0;
        return idB - idA;
      });

      // Slice to exact pageSize to ensure correct number of blogs
      return enrichedBlogs.slice(0, pageSize);
    } catch (error) {
      console.error("BlogService - Error getting approved blogs:", error);
      throw error;
    }
  },

  /**
   * Get all categories
   * @returns {Promise<Array>} Array of categories
   */
  getCategories: async () => {
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiResponse = await BlogAPI.getAllCategories();
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
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiBlog = await BlogAPI.getBlogById(id);
      
      console.log("🔍 Raw API Blog Response:", {
        id,
        apiBlog,
        hasData: !!apiBlog,
        imagePath: apiBlog?.imagePath,
      });
      
      if (!apiBlog) {
        throw new Error(`Blog with ID ${id} not found`);
      }
      
      const transformedBlog = BlogService.transformBlogFromAPI(apiBlog);
      
      // Try to enrich with author, but don't fail if it errors
      let enrichedBlog = transformedBlog;
      try {
        enrichedBlog = await BlogService.enrichBlogWithAuthor(transformedBlog);
      } catch (authorError) {
        console.warn("⚠️ Could not enrich blog with author:", authorError);
        // Continue with transformed blog without author enrichment
      }
      
      return enrichedBlog;
    } catch (error) {
      console.error(`BlogService - Error getting blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Create new blog
   * @param {Object} blogData - Blog data from UI (may contain imageFile)
   * @returns {Promise<Object>} Created blog in UI format
   */
  createBlog: async (blogData) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiData = BlogService.buildBlogPayload(blogData);
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
   * @param {Object} blogData - Updated blog data from UI (may contain imageFile)
   * @returns {Promise<Object>} Updated blog in UI format
   */
  updateBlog: async (id, blogData) => {
    try {
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
      const apiData = BlogService.buildBlogPayload(blogData);
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
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
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
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
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
      const token = localStorage.getItem("accessToken");
      if (token) setAuthToken(token);
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
