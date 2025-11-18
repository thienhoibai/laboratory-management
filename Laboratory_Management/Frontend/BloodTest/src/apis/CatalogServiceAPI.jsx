/* eslint-disable react-refresh/only-export-components */
import api from "../configs/axios";

const CATALOG_BASE = "testorder/api/TestCatalog";

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

export const getAllCatalogs = async (params = {}) => {
  const response = await api.get(CATALOG_BASE, { params });
  return extractItemsAndMeta(response, params);
};

export const getCatalogById = async (id) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await api.get(`${CATALOG_BASE}/${id}`);
  return response?.data?.data || response?.data;
};

export const createCatalog = async (payload) => {
  const response = await api.post(CATALOG_BASE, payload);
  return response?.data?.data || response?.data;
};

export const updateCatalog = async (id, payload) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await api.put(`${CATALOG_BASE}/${id}`, payload);
  return response?.data?.data || response?.data;
};

export const updateCatalogParameters = async (id, parameterIds = []) => {
  if (!id) throw new Error("Catalog ID is required");
  const response = await api.put(
    `${CATALOG_BASE}/${id}/parameters`,
    parameterIds
  );
  return response?.data?.data || response?.data;
};

const CatalogServiceAPI = {
  getAllCatalogs,
  getCatalogById,
  createCatalog,
  updateCatalog,
  updateCatalogParameters,
};

export default CatalogServiceAPI;

