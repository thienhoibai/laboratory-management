import api from "../configs/axios";
import { setAuthToken } from "../utils/auth";

// Generate instrument code: INSTRUMENT + 10 random digits
const generateInstrumentCode = () => {
  let digits = "";
  for (let i = 0; i < 10; i++) digits += Math.floor(Math.random() * 10);
  return `INSTRUMENT${digits}`;
};

const INSTRUMENT_BASE = "instrument/api/instruments";

const unwrap = (response) => response?.data ?? response;

// Helper function to set auth token before API calls
const ensureAuth = () => {
  const token = localStorage.getItem("accessToken");
  if (token) setAuthToken(token);
};

export const getInstruments = async (params = {}) => {
  ensureAuth();
  const res = await api.get(INSTRUMENT_BASE, { params });
  return unwrap(res);
};

export const getInstrumentById = async (id) => {
  ensureAuth();
  const res = await api.get(`${INSTRUMENT_BASE}/${id}`);
  return unwrap(res);
};

export const getInstrumentByCode = async (code) => {
  ensureAuth();
  const res = await api.get(`${INSTRUMENT_BASE}/${code}`);
  return unwrap(res);
};

export const createInstrument = async (payload) => {
  ensureAuth();
  console.log(
    "Sending payload:",
    payload instanceof FormData ? "FormData" : "JSON"
  );
  if (payload instanceof FormData) {
    // Debug: Log FormData để kiểm tra
    console.log("FormData entries:");
    for (let pair of payload.entries()) {
      console.log(
        `  ${pair[0]}:`,
        pair[1] instanceof File
          ? `File(${pair[1].name}, ${pair[1].size} bytes, type: ${pair[1].type})`
          : pair[1]
      );
    }
  } else {
    console.log("JSON payload:", payload);
  }

  try {
    // Axios tự động xử lý FormData, không cần config đặc biệt
    // Nó sẽ tự động set Content-Type với boundary
    const res = await api.post(INSTRUMENT_BASE, payload);
    console.log("✅ Success! Response from backend:", res.data);

    // Kiểm tra xem ReagentStatus có khớp với giá trị đã gửi không
    if (payload instanceof FormData) {
      const sentReagentStatus = payload.get("ReagentStatus");
      const receivedReagentStatus =
        res.data?.reagentStatus ?? res.data?.ReagentStatus;
      if (sentReagentStatus !== null && sentReagentStatus !== undefined) {
        console.log("🔍 ReagentStatus check:", {
          sent: sentReagentStatus,
          received: receivedReagentStatus,
          match: String(sentReagentStatus) === String(receivedReagentStatus),
        });
        if (String(sentReagentStatus) !== String(receivedReagentStatus)) {
          console.warn(
            "⚠️ WARNING: ReagentStatus mismatch! Sent:",
            sentReagentStatus,
            "but received:",
            receivedReagentStatus
          );
        }
      }
    }

    return unwrap(res);
  } catch (error) {
    console.error("❌ Error creating instrument:", error);
    if (error.response) {
      console.error("Error response data:", error.response.data);
      console.error("Error status:", error.response.status);
      console.error("Error headers:", error.response.headers);
    } else if (error.request) {
      console.error("No response received:", error.request);
    } else {
      console.error("Error message:", error.message);
    }
    throw error;
  }
};

export const updateInstrumentByCode = async (code, payload) => {
  ensureAuth();
  // Nếu payload là FormData, không cần set Content-Type (browser sẽ tự động set với boundary)
  const res = await api.put(`${INSTRUMENT_BASE}/${code}`, payload);
  return unwrap(res);
};

export const deleteInstrumentByCode = async (code) => {
  ensureAuth();
  const res = await api.delete(`${INSTRUMENT_BASE}/${code}`);
  return unwrap(res);
};

export const startInstrumentRun = async (bookingId) => {
  ensureAuth();
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
