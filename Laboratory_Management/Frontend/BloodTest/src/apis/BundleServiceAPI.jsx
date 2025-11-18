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
  const response = await api.get(BUNDLE_BASE, { params });
  return extractItemsAndMeta(response, params);
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
