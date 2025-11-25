import {
  getAllBundles as getAllBundlesAPI,
  getAllBundlesWithActive,
  getBundleById as getBundleByIdAPI,
  createBundle as createBundleAPI,
  updateBundle as updateBundleAPI,
  deleteBundle as deleteBundleAPI,
  getCatalogsOfBundle as getCatalogsOfBundleAPI,
  addCatalogsToBundle as addCatalogsToBundleAPI,
  removeCatalogsFromBundle as removeCatalogsFromBundleAPI,
  getAllCatalogs as getAllCatalogsAPI,
  getCatalogById as getCatalogByIdAPI,
  createCatalog as createCatalogAPI,
  updateCatalog as updateCatalogAPI,
  addParametersToCatalog as addParametersToCatalogAPI,
  removeParametersFromCatalog as removeParametersFromCatalogAPI,
  deleteCatalogParameter as deleteCatalogParameterAPI,
  getAllParameters as getAllParametersAPI,
  getParameterById as getParameterByIdAPI,
  createParameter as createParameterAPI,
  bookingService as bookingServiceAPI,
} from "../apis/TestOrderServiceAPI.jsx";

// ==================== Helper Functions ====================
export const extractItemsAndMeta = (response, fallbackQuery = {}) => {
  const data = response?.data;
  let items = [];

  if (Array.isArray(data)) {
    items = data;
  } else if (Array.isArray(data?.data)) {
    items = data.data;
  } else if (Array.isArray(data?.items)) {
    items = data.items;
  } else if (data) {
    items = Array.isArray(data) ? data : data.items || [];
  }

  const meta = data?.meta || {
    totalItems: data?.totalItems ?? items.length ?? 0,
    page: fallbackQuery.page ?? 1,
    pageSize: fallbackQuery.pageSize ?? items.length ?? 0,
  };

  return { items, meta };
};

// ==================== Bundle Service ====================
export const getAllBundles = async (params = {}) => {
  // Gọi API để lấy bundles từ CatalogBundle
  const catalogBundleResponse = await getAllBundlesAPI(params);
  const catalogBundles = extractItemsAndMeta(catalogBundleResponse, params);

  // Lấy thông tin isActive từ TestBundle API để merge
  try {
    const testBundleResponse = await getAllBundlesWithActive(params);
    const testBundles = extractItemsAndMeta(testBundleResponse, params);

    // Merge: lấy catalogs từ CatalogBundle và isActive từ TestBundle
    const mergedBundles = catalogBundles.items.map((catalogBundle) => {
      const testBundle = testBundles.items.find(
        (tb) =>
          (tb.bundleId ?? tb.id) ===
          (catalogBundle.bundleId ?? catalogBundle.id)
      );
      return {
        ...catalogBundle,
        isActive: testBundle?.isActive ?? catalogBundle.isActive ?? true,
      };
    });

    return {
      items: mergedBundles,
      meta: catalogBundles.meta,
    };
  } catch (error) {
    // Nếu TestBundle API lỗi, chỉ trả về dữ liệu từ CatalogBundle
    console.warn(
      "Could not fetch isActive from TestBundle, using CatalogBundle data only:",
      error
    );
    return catalogBundles;
  }
};

export const getBundleById = async (id) => {
  if (!id) throw new Error("Bundle ID is required");
  try {
    const response = await getBundleByIdAPI(id);
    if (response?.status === 204 || !response?.data) {
      console.warn(
        `[Service] getBundleById returned 204 or no data for id: ${id}`
      );
      return null;
    }
    const data = response?.data;
    return data?.data || data;
  } catch (error) {
    if (error.response?.status === 204 || error.response?.status === 404) {
      console.warn(
        `[Service] getBundleById: Bundle not found or no content for id: ${id}`
      );
      return null;
    }
    throw error;
  }
};

export const createBundle = async (payload) => {
  try {
    const response = await createBundleAPI(payload);

    // Xử lý trường hợp 204 No Content - API thành công nhưng không trả về data
    if (response?.status === 204) {
      console.log(
        "[Service] createBundle returned 204 No Content - success but no data"
      );

      // Thử lấy bundleId từ Location header nếu có
      const location =
        response?.headers?.location || response?.headers?.Location;
      if (location) {
        const bundleIdMatch = location.match(/\/(\d+)$/);
        if (bundleIdMatch) {
          const bundleId = parseInt(bundleIdMatch[1]);
          return { id: bundleId, bundleId: bundleId };
        }
      }

      // Nếu không có Location header, trả về null để component xử lý
      return null;
    }

    const data = response?.data;
    return data?.data || data;
  } catch (error) {
    // Xử lý trường hợp 204 trong error response
    if (error.response?.status === 204) {
      console.log(
        "[Service] createBundle error response 204 - treating as success"
      );
      const location =
        error.response?.headers?.location || error.response?.headers?.Location;
      if (location) {
        const bundleIdMatch = location.match(/\/(\d+)$/);
        if (bundleIdMatch) {
          const bundleId = parseInt(bundleIdMatch[1]);
          return { id: bundleId, bundleId: bundleId };
        }
      }
      return null;
    }
    throw error;
  }
};

export const updateBundle = async (id, payload) => {
  if (!id) throw new Error("Bundle ID is required");
  const response = await updateBundleAPI(id, payload);
  const data = response?.data;
  return data?.data || data;
};

export const deleteBundle = async (id) => {
  if (!id) throw new Error("Bundle ID is required");
  const response = await deleteBundleAPI(id);
  const data = response?.data;
  return data?.data || data;
};

export const getCatalogsOfBundle = async (bundleId) => {
  if (!bundleId) throw new Error("Bundle ID is required");
  console.log(
    `[Service] Calling getCatalogsOfBundle for bundleId: ${bundleId}`
  );

  try {
    const response = await getCatalogsOfBundleAPI(bundleId);
    console.log(
      `[Service] Response from getCatalogsOfBundle:`,
      response?.status,
      response?.data
    );

    // Xử lý trường hợp 204 No Content - bundle không có catalogs
    if (response?.status === 204 || !response?.data) {
      console.log(
        `[Service] getCatalogsOfBundle returned 204 or no data for bundleId: ${bundleId}`
      );
      return null;
    }

    // Response trả về là array: [{bundleId, bundleName, description, price, catalogs: [...]}]
    const data = response?.data;

    // Nếu là array, trả về phần tử đầu tiên (chứa thông tin bundle và catalogs)
    if (Array.isArray(data) && data.length > 0) {
      return data[0];
    }
    // Nếu là object trực tiếp
    else if (data && typeof data === "object") {
      // Thử các trường hợp nested
      if (Array.isArray(data.data) && data.data.length > 0) {
        return data.data[0];
      } else if (data.data && typeof data.data === "object") {
        return data.data;
      }
      return data;
    }

    return null;
  } catch (error) {
    // Xử lý trường hợp 204, 404 hoặc các lỗi khác
    if (error.response?.status === 204) {
      console.log(
        `[Service] getCatalogsOfBundle: 204 No Content for bundleId: ${bundleId}`
      );
      return null;
    }
    if (error.response?.status === 404) {
      console.log(
        `[Service] getCatalogsOfBundle: 404 Not Found for bundleId: ${bundleId}`
      );
      return null;
    }
    console.error(
      `[Service] Error getting catalogs of bundle ${bundleId}:`,
      error
    );
    throw error;
  }
};

export const addCatalogsToBundle = async (bundleId, catalogIds = []) => {
  if (!bundleId) throw new Error("Bundle ID is required");
  const response = await addCatalogsToBundleAPI(bundleId, catalogIds);
  const data = response?.data;
  return data?.data || data;
};

export const removeCatalogsFromBundle = async (bundleId, catalogIds = []) => {
  if (!bundleId) throw new Error("Bundle ID is required");
  const response = await removeCatalogsFromBundleAPI(bundleId, catalogIds);
  const data = response?.data;
  return data?.data || data;
};

// ==================== Catalog Service ====================
export const getAllCatalogs = async (params = {}) => {
  // Đảm bảo luôn có page và pageSize - logic xử lý params
  const queryParams = {
    page: params.page || 1,
    pageSize: params.pageSize || 10,
    ...params,
  };

  const response = await getAllCatalogsAPI(queryParams);
  const data = response?.data;

  // Xử lý response mới với catalogDTOs
  let items = [];

  if (Array.isArray(data?.catalogDTOs)) {
    items = data.catalogDTOs;
  } else if (Array.isArray(data)) {
    items = data;
  } else if (Array.isArray(data?.data)) {
    items = data.data;
  } else if (Array.isArray(data?.items)) {
    items = data.items;
  }

  // Meta từ response mới
  const meta = data?.meta || {
    totalItems: data?.totalItems ?? items.length ?? 0,
    page: data?.page ?? queryParams.page ?? 1,
    pageSize: data?.pageSize ?? queryParams.pageSize ?? items.length ?? 0,
    totalPages:
      data?.totalPages ??
      Math.ceil(
        (data?.totalItems ?? items.length) /
          (data?.pageSize ?? queryParams.pageSize)
      ),
  };

  return { items, meta };
};

export const getCatalogById = async (id) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await getCatalogByIdAPI(id);
  const data = response?.data;
  return data?.data || data;
};

export const createCatalog = async (payload) => {
  const response = await createCatalogAPI(payload);
  const data = response?.data;
  return data?.data || data;
};

export const updateCatalog = async (id, payload) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await updateCatalogAPI(id, payload);
  const data = response?.data;
  return data?.data || data;
};

export const addParametersToCatalog = async (id, parameterIds = []) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await addParametersToCatalogAPI(id, parameterIds);
  const data = response?.data;
  return data?.data || data;
};

export const removeParametersFromCatalog = async (id, parameterIds = []) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await removeParametersFromCatalogAPI(id, parameterIds);
  const data = response?.data;
  return data?.data || data;
};

export const deleteCatalogParameter = async (catalogId, parameterId) => {
  if (!catalogId) throw new Error("Catalog ID is required");
  if (!parameterId) throw new Error("Parameter ID is required");
  const response = await deleteCatalogParameterAPI(catalogId, parameterId);
  const data = response?.data;
  return data?.data || data;
};

// ==================== Parameter Service ====================
export const getAllParameters = async (params = {}) => {
  const response = await getAllParametersAPI(params);
  return extractItemsAndMeta(response, params);
};

export const getParameterById = async (id) => {
  if (!id) throw new Error("Parameter ID is required");
  const response = await getParameterByIdAPI(id);
  const data = response?.data;
  if (data?.data) return data.data;
  return data;
};

export const createParameter = async (payload) => {
  if (!payload) throw new Error("Payload is required");
  const response = await createParameterAPI(payload);
  return response?.data;
};

// ==================== Booking Service ====================
export const bookingService = {
  // Lấy thông tin booking theo ID
  getBookingById: async (bookingId) => {
    const response = await bookingServiceAPI.getBookingById(bookingId);
    return response?.data || response;
  },

  // Lấy thông tin test catalog
  getTestCatalog: async (catalogId) => {
    const response = await bookingServiceAPI.getTestCatalog(catalogId);
    return response?.data || response;
  },

  // Lấy thông tin test bundle
  getTestBundle: async (bundleId) => {
    const response = await bookingServiceAPI.getTestBundle(bundleId);
    return response?.data || response;
  },

  // Tạo VNPay URL
  createVnPayUrl: async (bookingId, amount) => {
    const response = await bookingServiceAPI.createVnPayUrl(bookingId, amount);
    return response?.data || response;
  },

  // Lấy thông tin số lượng booking của các appointment slots
  getAppointmentSlotCounts: async () => {
    const response = await bookingServiceAPI.getAppointmentSlotCounts();
    return response?.data || response;
  },
};

export const ManagerAppointmentSchedule = {};
