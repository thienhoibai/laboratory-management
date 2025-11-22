import api from "../configs/axios";

export const bookingService = {
  // Lấy thông tin booking theo ID
  getBookingById: async (bookingId) => {
    const response = await api.get(
      `testorder/api/Booking?bookingId=${bookingId}`
    );
    return response;
  },

  // Lấy thông tin test catalog
  getTestCatalog: async (catalogId) => {
    const response = await api.get(`testorder/api/TestCatalog/${catalogId}`);
    return response;
  },

  // Lấy thông tin test bundle
  getTestBundle: async (bundleId) => {
    const response = await api.get(`testorder/api/TestBundle/${bundleId}`);
    return response;
  },

  // Tạo VNPay URL
  createVnPayUrl: async (bookingId, amount) => {
    const response = await api.post(`testorder/api/Payment/vnpay-url`, {
      bookingId,
      amount,
    });
    return response;
  },

  // Lấy thông tin số lượng booking của các appointment slots
  getAppointmentSlotCounts: async () => {
    const response = await api.get(
      `testorder/api/AppointmentSlot/count-all?pageNumber=1&pageSize=10000`
    );
    return response;
  },
};

export const ManagerAppointmentSchedule = {};
