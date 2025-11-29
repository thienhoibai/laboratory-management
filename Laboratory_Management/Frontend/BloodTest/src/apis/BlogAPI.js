import api from "../configs/axios";

/**
 * Blog API Service
 * Handles all HTTP requests related to blog posts and categories
 */

const BlogAPI = {
  /**
   * Get all blog posts
   * @param {Object} params - Query parameters
   * @param {number} params.page - Page number
   * @param {number} params.pageSize - Page size
   * @param {number} params.status - Status filter (0: pending, 1: approved, 2: rejected)
   * @param {string} params.search - Search term for filtering blogs
   * @param {string} params.authorId - Author ID filter
   * @returns {Promise} Array of blog posts
   */
  getAllBlogs: async (params = {}) => {
    try {
      const { page = 1, pageSize = 100, status, authorId, search } = params;
      let url = `blog/api/BlogPost?page=${page}&pageSize=${pageSize}`;
      
      if (status !== undefined && status !== null) {
        url += `&status=${status}`;
      }
      
      if (authorId) {
        url += `&authorId=${authorId}`;
      }
      
      if (search && search.trim() !== "") {
        url += `&search=${encodeURIComponent(search.trim())}`;
      }
      
      const response = await api.get(url);
      return response.data;
    } catch (error) {
      console.error("Error fetching blogs:", error);
      throw error;
    }
  },

  /**
   * Get approved blog posts (for public display)
   * @param {number} page - Page number
   * @param {number} pageSize - Page size
   * @returns {Promise} Array of approved blog posts
   */
  getApprovedBlogs: async (page = 1, pageSize = 100) => {
    try {
      // Use getAllBlogs with status=1 (approved)
      return await BlogAPI.getAllBlogs({ page, pageSize, status: 1 });
    } catch (error) {
      console.error("Error fetching approved blogs:", error);
      throw error;
    }
  },

  /**
   * Get a single blog post by ID
   * @param {number} id - Blog post ID
   * @returns {Promise} Blog post object
   */
  getBlogById: async (id) => {
    try {
      const response = await api.get(`blog/api/BlogPost/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Create a new blog post
   * @param {FormData|Object} blogData - Blog post data (FormData for file upload or Object for JSON)
   * @returns {Promise} Created blog post
   */
  createBlog: async (blogData) => {
    try {
      // Axios tự động xử lý FormData, không cần set Content-Type
      const response = await api.post("blog/api/BlogPost", blogData);
      return response.data;
    } catch (error) {
      console.error("Error creating blog:", error);
      throw error;
    }
  },

  /**
   * Update an existing blog post
   * @param {number} id - Blog post ID
   * @param {FormData|Object} blogData - Updated blog post data (FormData for file upload or Object for JSON)
   * @returns {Promise} Updated blog post
   */
  updateBlog: async (id, blogData) => {
    try {
      // Axios tự động xử lý FormData, không cần set Content-Type
      const response = await api.put(`blog/api/BlogPost/${id}`, blogData);
      return response.data;
    } catch (error) {
      console.error(`Error updating blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Delete a blog post
   * @param {number} id - Blog post ID
   * @returns {Promise} Delete confirmation
   */
  deleteBlog: async (id) => {
    try {
      const response = await api.delete(`blog/api/BlogPost/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error deleting blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Approve a blog post
   * @param {number} id - Blog post ID
   * @param {number} status - Status code (0, 1, or 2)
   * @returns {Promise} Approve confirmation
   */
  approveBlog: async (id, status = 1) => {
    try {
      const response = await api.put(`blog/api/BlogPost/status/${id}`, {
        status: status,
      });
      return response.data;
    } catch (error) {
      console.error(`Error approving blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Reject a blog post
   * @param {number} id - Blog post ID
   * @param {number} status - Status code (0, 1, or 2)
   * @returns {Promise} Reject confirmation
   */
  rejectBlog: async (id, status = 2) => {
    try {
      const response = await api.put(`blog/api/BlogPost/status/${id}`, {
        status: status,
      });
      return response.data;
    } catch (error) {
      console.error(`Error rejecting blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Get all categories
   * @returns {Promise<Array|Object>} Category response data
   */
  getAllCategories: async () => {
    try {
      const response = await api.get("blog/api/Category");
      return response.data;
    } catch (error) {
      console.error("Error fetching categories:", error);
      throw error;
    }
  },

  /**
   * Get blog image by image path
   * @param {string} imagePath - Image path from blog post
   * @returns {Promise<Blob>} Image blob
   */
  getBlogImage: async (imagePath) => {
    try {
      // Try different possible endpoints
      // If backend has a specific endpoint for images
      const response = await api.get(`blog/api/BlogPost/image/${imagePath}`, {
        responseType: 'blob'
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching blog image:", error);
      throw error;
    }
  },
};

export default BlogAPI;
