// Utility functions for authentication
export const setAuthToken = (token) => {
  localStorage.setItem('authToken', token);
};

export const getAuthToken = () => {
  return localStorage.getItem('authToken');
};

export const removeAuthToken = () => {
  localStorage.removeItem('authToken');
};

// User data functions
export const setUserData = (userData) => {
  localStorage.setItem('user', JSON.stringify(userData));
  // Also set auth token for compatibility
  setAuthToken('user-logged-in');
};

export const getUserData = () => {
  const userData = localStorage.getItem('user');
  return userData ? JSON.parse(userData) : null;
};

export const removeUserData = () => {
  localStorage.removeItem('user');
  removeAuthToken();
};

export const isAuthenticated = () => {
  const userData = getUserData();
  const token = getAuthToken();
  return !!(userData && token);
};

// Demo function to simulate login
export const demoLogin = () => {
  const demoUser = {
    fullname: "Nguyễn Văn An",
    patientId: "550e8400",
    gender: "Nam",
    age: 40,
    email: "nguyenvanan@email.com",
    phone: "0912345678",
    birthday: "15 tháng 3, 1985",
    address: "123 Đường Lê Lợi, Quận 1, TP.HCM",
    idCard: "079085001234",
    healthInsurance: "BH-2024-001234",
    registrationDate: "15 tháng 1, 2024"
  };
  setUserData(demoUser);
  window.location.reload(); // Reload to update login status
};

// Demo function to simulate logout
export const demoLogout = () => {
  removeUserData();
  window.location.reload(); // Reload to update login status
};

