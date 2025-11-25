import api from "../configs/axios";

// Generate instrument code: INSTRUMENT + 10 random digits
const generateInstrumentCode = () => {
  let digits = "";
  for (let i = 0; i < 10; i++) digits += Math.floor(Math.random() * 10);
  return `INSTRUMENT${digits}`;
};

export const startInstrumentRun = async (bookingId) => {
  const instrumentCode = generateInstrumentCode();
  const payload = { bookingId, instrumentCode };
  const res = await api.post("instrument/api/instrument/runs/start", payload);
  return res?.data ?? {};
};

export default {
  startInstrumentRun,
};
