import api from "../configs/axios";

const URL = "iam/api/Auth/";
const URL_Google = "iam/v1/auth/";
export const IAMServiceAPI = {
  ResetPasswordAPI: async (token, newPassword) => {
    return await api.post(`${URL}reset-password`, {
      token: token,
      newPassword: newPassword,
    });
  },

  ForgotPasswordAPI: async (email) => {
    return await api.post(`${URL}forgot-password`, {
      UsernameOrEmail: email,
    });
  },

  LoginWithPassword: async (username, password) => {
    return await api.post(`${URL}login`, {
      username: username,
      password: password,
    });
  },

  LoginWithGoogle: async (idToken) => {
    return await api.post(`${URL_Google}google`, { idToken });
  },

  Register: async (payLoad) => {
    return await api.post(`${URL}register`, payLoad);
  },

  ChangePassword: async (currentPassword, newPassword) => {
    return await api.post(`${URL}change-password`, {
      currentPassword: currentPassword,
      newPassword: newPassword,
    });
  },
};
