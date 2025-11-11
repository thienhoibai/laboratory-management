import api from "../configs/axios";

const URL = "patient/v1/patients";

export const PatientServiceAPI = {
  CreateProfile: async (data) => {
    return await api.post(URL, data);
  },
};
