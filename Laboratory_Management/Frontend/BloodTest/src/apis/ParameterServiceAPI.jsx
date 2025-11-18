/* eslint-disable react-refresh/only-export-components */
import api from "../configs/axios";

const PARAMETER_BASE = "testorder/api/TestParameter";

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
    items = Array.isArray(data) ? data : data.items || [];
  }

  const meta = data?.meta || {
    totalItems: data?.totalItems ?? items.length ?? 0,
    page: fallbackQuery.page ?? 1,
    pageSize: fallbackQuery.pageSize ?? items.length ?? 0,
  };

  return { items, meta };
};

export const getAllParameters = async (params = {}) => {
  const response = await api.get(PARAMETER_BASE, { params });
  return extractItemsAndMeta(response, params);
};

export const getParameterById = async (id) => {
  if (!id) throw new Error("Parameter ID is required");
  const response = await api.get(`${PARAMETER_BASE}/${id}`);
  const data = response?.data;
  if (data?.data) return data.data;
  return data;
};

export const createParameter = async (payload) => {
  if (!payload) throw new Error("Payload is required");
  const response = await api.post(PARAMETER_BASE, payload);
  return response?.data;
};

const ParameterServiceAPI = {
  getAllParameters,
  getParameterById,
  createParameter,
};

export default ParameterServiceAPI;
