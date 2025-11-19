import api from "../configs/axios";

const URL = "patient/v1/patients";

export const PatientServiceAPI = {
  CreateProfile: async (data) => {
    return await api.post(URL, data);
  },
  Profile: async () => {
    return await api.get(`${URL}/me`);
  },
  GetProfileByPatientId: async (patientId) => {
    return await api.get(`${URL}/${patientId}`);
  },
  GetMedicalRecords: async (page, pageSize) => {
    return await api.get(`${URL}/mine?page=${page}&pageSize=${pageSize}`);
  },
  UpdateProfile: async (patientId, data) => {
    return await api.put(`${URL}/${patientId}`, data);
  },
  AddNewProfile: async (data) => {
    return await api.post(`${URL}`, data);
  },
};
