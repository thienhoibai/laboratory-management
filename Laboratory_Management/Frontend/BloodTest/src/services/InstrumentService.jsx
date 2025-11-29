import {
  createInstrument,
  deleteInstrumentByCode,
  getInstrumentByCode,
  getInstrumentById,
  getInstruments,
  updateInstrumentByCode,
} from "../apis/InstrumentAPI.jsx";

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

/**
 * Helper function to build full image URL for instrument
 * @param {string} imagePath - Image path from API (can be relative or absolute)
 * @returns {string} Full image URL
 */
const buildInstrumentImageUrl = (imagePath) => {
  if (!imagePath || imagePath.trim() === "") return "";

  const trimmedPath = imagePath.trim();
  const baseURL = "http://localhost:8080";

  // If already a full URL (starts with http:// or https://), return as is
  if (trimmedPath.startsWith("http://") || trimmedPath.startsWith("https://")) {
    return trimmedPath;
  }

  // If starts with /, it's a relative path from root
  if (trimmedPath.startsWith("/")) {
    return `${baseURL}${trimmedPath}`;
  }

  // If path starts with "Images/", append directly to /instrument/
  // Example: "Images/f4a44ab4-81cf-47de-9555-d67ccf02fbb1.jpg"
  // Result: http://localhost:8080/instrument/Images/f4a44ab4-81cf-47de-9555-d67ccf02fbb1.jpg
  if (trimmedPath.startsWith("Images/")) {
    // Build URL: http://localhost:8080/instrument/Images/...
    return `${baseURL}/instrument/${trimmedPath}`;
  }

  // Otherwise, assume it's just a filename and try common paths
  return `${baseURL}/instrument/Images/${trimmedPath}`;
};

const mapInstrumentShape = (instrument = {}) => {
  const code =
    instrument.code ??
    instrument.instrumentCode ??
    instrument.InstrumentCode ??
    "";
  const machineStatus =
    instrument.machineStatus ??
    instrument.status ??
    instrument.Status ??
    instrument.MachineStatus ??
    "ACTIVE";

  // Lấy reagentStatus từ nhiều nguồn có thể
  const rawReagentStatus =
    instrument.reagentStatus ??
    instrument.reagent_status ??
    instrument.ReagentStatus ??
    instrument.Reagent_Status;

  // Debug: Log để kiểm tra dữ liệu từ API
  if (rawReagentStatus === undefined || rawReagentStatus === null) {
    console.log("⚠️ ReagentStatus not found in instrument data:", {
      code,
      allKeys: Object.keys(instrument),
      instrument,
    });
  }

  const reagentStatus = rawReagentStatus ?? "FULL";

  // Get image path from various possible fields
  const rawImagePath =
    instrument.imagePath ||
    instrument.ImagePath ||
    instrument.imageUrl ||
    instrument.ImageUrl ||
    "";
  const imageUrl = buildInstrumentImageUrl(rawImagePath);

  // Debug: Log image URL for troubleshooting (only if image exists)
  if (rawImagePath) {
    console.log("📸 Instrument Image Debug:", {
      code,
      name: instrument.name || instrument.Name,
      rawImagePath,
      builtImageUrl: imageUrl,
      allImageFields: {
        imagePath: instrument.imagePath,
        ImagePath: instrument.ImagePath,
        imageUrl: instrument.imageUrl,
        ImageUrl: instrument.ImageUrl,
      },
    });
  }

  console.log("🔍 Mapped instrument:", {
    code,
    originalReagentStatus: rawReagentStatus,
    mappedReagentStatus: reagentStatus,
    allFields: {
      reagentStatus: instrument.reagentStatus,
      reagent_status: instrument.reagent_status,
      ReagentStatus: instrument.ReagentStatus,
      Reagent_Status: instrument.Reagent_Status,
    },
  });

  return {
    ...instrument,
    code,
    instrumentCode: code,
    machineStatus,
    status: machineStatus,
    reagentStatus,
    imageUrl: imageUrl, // Add built image URL
    imagePath: rawImagePath, // Keep original path for reference
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
    if (typeof reagentStatus !== "undefined" && reagentStatus !== null) {
      // Backend expect enum (số), gửi dưới dạng string để đảm bảo backend nhận được
      // Một số backend yêu cầu string thay vì number trong FormData
      formData.append("ReagentStatus", String(reagentStatus));
      console.log("📤 Sending ReagentStatus in FormData:", {
        original: reagentStatus,
        type: typeof reagentStatus,
        asString: String(reagentStatus),
      });
    } else {
      console.warn(
        "⚠️ ReagentStatus is undefined or null, not sending to backend"
      );
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
  if (typeof reagentStatus !== "undefined" && reagentStatus !== null) {
    payload.ReagentStatus = reagentStatus;
    console.log("📤 Sending ReagentStatus in JSON:", {
      original: reagentStatus,
      type: typeof reagentStatus,
      value: payload.ReagentStatus,
    });
  } else {
    console.warn(
      "⚠️ ReagentStatus is undefined or null, not sending to backend"
    );
  }
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
