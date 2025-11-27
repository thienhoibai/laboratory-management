import {
  createInstrument,
  deleteInstrumentByCode,
  getInstrumentByCode,
  getInstrumentById,
  getInstruments,
  updateInstrumentByCode,
} from "../apis/InstrumentAPI";

const formatError = (error) => {
  if (!error) return new Error("Đã có lỗi không xác định.");
  const message =
    error?.response?.data?.message ||
    error?.message ||
    "Không thể thực hiện thao tác. Vui lòng thử lại.";
  return new Error(message);
};

const normalizeListResponse = (payload) => {
  if (!payload) return [];
  if (Array.isArray(payload)) return payload;

  const candidateKeys = ["items", "content", "data", "results"];
  for (const key of candidateKeys) {
    const value = payload?.[key];
    if (Array.isArray(value)) return value;
  }

  // Fall back to the first array value inside the object, if any
  const firstArrayValue = Object.values(payload || {}).find(Array.isArray);
  return Array.isArray(firstArrayValue) ? firstArrayValue : [];
};

const mapInstrumentShape = (instrument = {}) => {
  const code = instrument.code ?? instrument.instrumentCode ?? "";
  const machineStatus =
    instrument.machineStatus ?? instrument.status ?? "ACTIVE";
  const reagentStatus =
    instrument.reagentStatus ?? instrument.reagent_status ?? "FULL";
  return {
    ...instrument,
    code,
    instrumentCode: code,
    machineStatus,
    status: machineStatus,
    reagentStatus,
  };
};

const InstrumentService = {
  async list() {
    try {
      const data = await getInstruments();
      return normalizeListResponse(data).map(mapInstrumentShape);
    } catch (error) {
      throw formatError(error);
    }
  },

  async getById(id) {
    try {
      return await getInstrumentById(id);
    } catch (error) {
      throw formatError(error);
    }
  },

  async getByCode(code) {
    try {
      return await getInstrumentByCode(code);
    } catch (error) {
      throw formatError(error);
    }
  },

  async create(payload) {
    try {
      return await createInstrument(payload);
    } catch (error) {
      throw formatError(error);
    }
  },

  async update(code, payload) {
    try {
      return await updateInstrumentByCode(code, payload);
    } catch (error) {
      throw formatError(error);
    }
  },

  async remove(code) {
    try {
      return await deleteInstrumentByCode(code);
    } catch (error) {
      throw formatError(error);
    }
  },
};

export default InstrumentService;
