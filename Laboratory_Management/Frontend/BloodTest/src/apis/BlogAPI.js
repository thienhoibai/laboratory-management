import api from "../configs/axios";

/**
 * Blog API Service
 * Handles all HTTP requests related to blog posts
 */

const BlogAPI = {
  /**
   * Get all blog posts
   * @returns {Promise} Array of blog posts
   */
  getAllBlogs: async () => {
    try {
      const response = await api.get("/api/BlogPost");
      return response.data;
    } catch (error) {
      console.error("Error fetching blogs:", error);
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
      const response = await api.get(`/api/BlogPost/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching blog ${id}:`, error);
      throw error;
    }
  },

  /**
   * Create a new blog post
   * @param {Object} blogData - Blog post data
   * @returns {Promise} Created blog post
   */
  createBlog: async (blogData) => {
    try {
      const response = await api.post("/api/BlogPost", blogData);
      return response.data;
    } catch (error) {
      console.error("Error creating blog:", error);
      throw error;
    }
  },

  /**
   * Update an existing blog post
   * @param {number} id - Blog post ID
   * @param {Object} blogData - Updated blog post data
   * @returns {Promise} Updated blog post
   */
  updateBlog: async (id, blogData) => {
    try {
      const response = await api.put(`/api/BlogPost/${id}`, blogData);
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
      const response = await api.delete(`/api/BlogPost/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error deleting blog ${id}:`, error);
      throw error;
    }
  },
};

export default BlogAPI;
