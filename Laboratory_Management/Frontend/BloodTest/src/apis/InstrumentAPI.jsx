import api from "../configs/axios";

// Generate instrument code: INSTRUMENT + 10 random digits
const generateInstrumentCode = () => {
  let digits = "";
  for (let i = 0; i < 10; i++) digits += Math.floor(Math.random() * 10);
  return `INSTRUMENT${digits}`;
};

const INSTRUMENT_BASE = "instrument/api/instruments";

const unwrap = (response) => response?.data ?? response;

export const getInstruments = async (page, pageSize) => {
  const res = await api.get(INSTRUMENT_BASE, { page, pageSize });
  return unwrap(res);
};

export const getInstrumentById = async (id) => {
  const res = await api.get(`${INSTRUMENT_BASE}/${id}`);
  return unwrap(res);
};

export const getInstrumentByCode = async (code) => {
  const res = await api.get(`${INSTRUMENT_BASE}/${code}`);
  return unwrap(res);
};

export const createInstrument = async (payload) => {
  const res = await api.post(INSTRUMENT_BASE, payload);
  return unwrap(res);
};

export const updateInstrumentByCode = async (code, payload) => {
  const res = await api.put(`${INSTRUMENT_BASE}/${code}`, payload);
  return unwrap(res);
};

export const deleteInstrumentByCode = async (code) => {
  const res = await api.delete(`${INSTRUMENT_BASE}/${code}`);
  return unwrap(res);
};

export const startInstrumentRun = async (bookingId) => {
  const instrumentCode = generateInstrumentCode();
  const payload = { bookingId, instrumentCode };
  const res = await api.post("instrument/api/instrument/runs/start", payload);
  return res?.data ?? {};
};

export default {
  getInstruments,
  getInstrumentById,
  getInstrumentByCode,
  createInstrument,
  updateInstrumentByCode,
  deleteInstrumentByCode,
  startInstrumentRun,
};
