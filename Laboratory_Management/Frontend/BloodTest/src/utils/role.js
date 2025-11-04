// src/utils/role.js
// Utility functions for role and permission management

import { jwtDecode } from "jwt-decode";

// Define permissions for each role
const rolePermissions = {
  Admin: [
    "dashboard",
    "users",
    "roles",
    "instruments",
    "reagents",
    "blogs",
    "patients",
    "test-orders",
    "packages",
    "catalogs",
    "parameter",
    "reports",
  ],
  Manager: [
    "dashboard",
    "users",
    "instruments",
    "reagents",
    "blogs",
    "patients",
    "catalogs",
    "parameter",
  ],
  Staff: [
    "instruments",
    "reagents",
    "blogs",
    "patients",
    "test-orders",
    "packages",
  ],
};

// Get current user role from token
export const getCurrentUserRole = () => {
  try {
    const token = localStorage.getItem("accessToken");
    if (!token) return null;

    const decoded = jwtDecode(token);
    const role =
      decoded[
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
      ] || decoded.role;

    return role || null;
  } catch (error) {
    console.error("Error decoding token:", error);
    return null;
  }
};

// Check if user has permission to access a feature
export const hasPermission = (feature) => {
  const role = getCurrentUserRole();
  if (!role) return false;

  const permissions = rolePermissions[role] || [];
  return permissions.includes(feature);
};

// Get all permissions for current user
export const getUserPermissions = () => {
  const role = getCurrentUserRole();
  if (!role) return [];

  return rolePermissions[role] || [];
};

// Check if user is admin
export const isAdmin = () => {
  return getCurrentUserRole() === "Admin";
};

// Check if user is manager
export const isManager = () => {
  return getCurrentUserRole() === "Manager";
};

// Check if user is staff
export const isStaff = () => {
  return getCurrentUserRole() === "Staff";
};

// Check if user can access management pages
export const canAccessManagement = () => {
  const role = getCurrentUserRole();
  return role === "Admin" || role === "Manager" || role === "Staff";
};

// Get menu items based on user role
export const getMenuItems = () => {
  const permissions = getUserPermissions();

  const allMenuItems = [
    { path: "/admin/dashboard", icon: "dashboard", name: "Tổng quan", permission: "dashboard" },
    { path: "/admin/users", icon: "users", name: "Người dùng", permission: "users" },
    { path: "/admin/roles", icon: "roles", name: "Vai trò", permission: "roles" },
    { path: "/admin/instruments", icon: "instruments", name: "Thiết bị", permission: "instruments" },
    { path: "/admin/reagents", icon: "reagents", name: "Thuốc thử", permission: "reagents" },
    { path: "/admin/blogs", icon: "blogs", name: "Blog", permission: "blogs" },
    { path: "/admin/patients", icon: "patients", name: "Bệnh nhân", permission: "patients" },
    { path: "/admin/test-orders", icon: "test-orders", name: "Đơn xét nghiệm", permission: "test-orders" },
    { path: "/admin/packages", icon: "packages", name: "Gói xét nghiệm", permission: "packages" },
    { path: "/admin/catalogs", icon: "catalogs", name: "Danh mục", permission: "catalogs" },
    { path: "/admin/parameter", icon: "parameter", name: "Tham số", permission: "parameter" },
    { path: "/admin/reports", icon: "reports", name: "Báo cáo", permission: "reports" },
  ];

  return allMenuItems.filter((item) => permissions.includes(item.permission));
};

export default {
  getCurrentUserRole,
  hasPermission,
  getUserPermissions,
  isAdmin,
  isManager,
  isStaff,
  canAccessManagement,
  getMenuItems,
};

