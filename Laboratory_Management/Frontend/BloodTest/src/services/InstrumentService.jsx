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

const buildInstrumentPayload = (
  { code, name, status, reagentStatus, imageFile },
  isUpdate = false
) => {
  // Nếu có file ảnh, sử dụng FormData
  if (imageFile instanceof File) {
    const formData = new FormData();
    // Chỉ gửi code khi tạo mới, không gửi khi update (vì đã có trong URL)
    // Backend expect PascalCase: InstrumentCode, Name, Status, ReagentStatus, Image
    if (code && !isUpdate) {
      formData.append("InstrumentCode", code);
    }
    if (name) {
      formData.append("Name", name);
    }
    if (typeof status !== "undefined") {
      // Backend expect enum (số), gửi số trực tiếp
      formData.append("Status", status);
    }
    if (typeof reagentStatus !== "undefined") {
      // Backend expect enum (số), gửi số trực tiếp
      formData.append("ReagentStatus", reagentStatus);
    }
    formData.append("Image", imageFile); // Backend expect "Image" not "imageFile"

    // Debug: Log FormData contents
    console.log("FormData contents:");
    for (let pair of formData.entries()) {
      console.log(
        pair[0] +
          ": " +
          (pair[1] instanceof File ? `File(${pair[1].name})` : pair[1])
      );
    }

    return formData;
  }

  // Nếu không có file, sử dụng JSON
  const payload = {};
  // Chỉ gửi code khi tạo mới, không gửi khi update (vì đã có trong URL)
  if (code && !isUpdate) payload.InstrumentCode = code;
  if (name) payload.Name = name;
  if (typeof status !== "undefined") payload.Status = status;
  if (typeof reagentStatus !== "undefined")
    payload.ReagentStatus = reagentStatus;
  if (typeof imageFile === "string" && imageFile) {
    payload.ImagePath = imageFile;
  }

  return payload;
};

const InstrumentService = {
  async list(params = {}) {
    try {
      const data = await getInstruments(params);
      const items = normalizeListResponse(data).map(mapInstrumentShape);

      // Trả về cả dữ liệu và thông tin phân trang
      return {
        items,
        totalPages: data?.totalPages || 1,
        totalElements: data?.totalElements || items.length,
        currentPage: data?.currentPage || params.page || 1,
        pageSize: data?.pageSize || params.pageSize || 10,
      };
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
      const jsonPayload = buildInstrumentPayload(payload);
      return await createInstrument(jsonPayload);
    } catch (error) {
      throw formatError(error);
    }
  },

  async update(code, payload) {
    try {
      // Update endpoint chỉ nhận JSON với ImagePath (string), không hỗ trợ file upload
      // Nếu có file mới, cần upload riêng trước hoặc bỏ qua
      // eslint-disable-next-line no-unused-vars
      const { imageFile, ...restPayload } = payload;
      const jsonPayload = buildInstrumentPayload(
        { ...restPayload, code },
        true
      );
      return await updateInstrumentByCode(code, jsonPayload);
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
