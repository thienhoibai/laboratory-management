import api from "../configs/axios";

/**
 * Category API Service
 * Handles HTTP requests related to blog categories
 */
const CategoryAPI = {
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
};

export default CategoryAPI;

