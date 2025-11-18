/* eslint-disable react-refresh/only-export-components */
import api from "../configs/axios";

const BUNDLE_BASE = "testorder/api/TestBundle";
const CATALOG_BUNDLE_BASE = "testorder/api/CatalogBundle";

const extractItemsAndMeta = (response, fallbackQuery = {}) => {
  const data = response?.data;
  let items = [];

  if (Array.isArray(data)) {
    items = data;
  } else if (Array.isArray(data?.data)) {
    items = data.data;
  } else if (Array.isArray(data?.items)) {
    items = data.items;
  } else if (data) {
    items = data.items || [];
  }

  const meta = data?.meta || {
    totalItems: data?.totalItems ?? items.length ?? 0,
    page: fallbackQuery.page ?? 1,
    pageSize: fallbackQuery.pageSize ?? items.length ?? 0,
  };

  return { items, meta };
};

export const getAllBundles = async (params = {}) => {
  // Sử dụng CatalogBundle API để lấy bundles kèm catalogs
  const catalogBundleResponse = await api.get(CATALOG_BUNDLE_BASE, { params });
  const catalogBundles = extractItemsAndMeta(catalogBundleResponse, params);

  // Lấy thông tin isActive từ TestBundle API để merge
  try {
    const testBundleResponse = await api.get(BUNDLE_BASE, { params });
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
  const response = await api.get(`${BUNDLE_BASE}/${id}`);
  return response?.data?.data || response?.data;
};

export const createBundle = async (payload) => {
  const response = await api.post(BUNDLE_BASE, payload);
  return response?.data?.data || response?.data;
};

export const updateBundle = async (id, payload) => {
  if (!id) throw new Error("Bundle ID is required");
  const response = await api.put(`${BUNDLE_BASE}/${id}`, payload);
  return response?.data?.data || response?.data;
};

export const deleteBundle = async (id) => {
  if (!id) throw new Error("Bundle ID is required");
  const response = await api.delete(`${BUNDLE_BASE}/${id}`);
  return response?.data?.data || response?.data;
};

export const getCatalogsOfBundle = async (bundleId) => {
  if (!bundleId) throw new Error("Bundle ID is required");
  const response = await api.get(`${CATALOG_BUNDLE_BASE}/${bundleId}`);
  return response?.data?.data || response?.data || [];
};

export const addCatalogsToBundle = async (bundleId, catalogIds = []) => {
  if (!bundleId) throw new Error("Bundle ID is required");
  const payload = {
    bundleId,
    catalogId: catalogIds,
  };
  const response = await api.post(CATALOG_BUNDLE_BASE, payload);
  return response?.data?.data || response?.data;
};

export const removeCatalogsFromBundle = async (bundleId, catalogIds = []) => {
  if (!bundleId) throw new Error("Bundle ID is required");
  const payload = {
    bundleId,
    catalogId: catalogIds,
  };
  const response = await api.delete(`${CATALOG_BUNDLE_BASE}/${bundleId}`, {
    data: payload,
  });
  return response?.data?.data || response?.data;
};

const BundleServiceAPI = {
  getAllBundles,
  getBundleById,
  createBundle,
  updateBundle,
  deleteBundle,
  getCatalogsOfBundle,
  addCatalogsToBundle,
  removeCatalogsFromBundle,
};

export default BundleServiceAPI;
