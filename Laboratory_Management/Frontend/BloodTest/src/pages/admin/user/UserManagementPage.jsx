import React from "react";
import AdminLayout from "../../../components/admin/layout/AdminLayout.jsx";
import { FiSearch, FiPlus, FiEye, FiTrash2 } from "react-icons/fi";

// Dữ liệu giả lập
const users = [
  {
    name: "Dr. Sarah Johnson",
    email: "sarah.johnson@lab.com",
    phone: "+1 (555) 123-4567",
    role: "Doctor",
    status: "Active",
  },
  {
    name: "Michael Chen",
    email: "michael.chen@lab.com",
    phone: "+1 (555) 234-5678",
    role: "Lab Technician",
    status: "Active",
  },
  {
    name: "Emily Rodriguez",
    email: "emily.rodriguez@lab.com",
    phone: "+1 (555) 345-6789",
    role: "Receptionist",
    status: "Inactive",
  },
  {
    name: "David Thompson",
    email: "david.thompson@lab.com",
    phone: "+1 (555) 456-7890",
    role: "Lab Manager",
    status: "Active",
  },
];

const getRoleClass = (role) => {
  switch (role) {
    case "Doctor":
      return "role-doctor";
    case "Lab Technician":
      return "role-technician";
    case "Receptionist":
      return "role-receptionist";
    case "Lab Manager":
      return "role-manager";
    default:
      return "";
  }
};

const UserManagementPage = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Users Management" },
  ];

  return (
    <AdminLayout pageTitle="Users Management" breadcrumbs={breadcrumbs}>
      <div className="admin-content-card">
        <div className="admin-table-controls">
          <div className="admin-search-bar">
            <FiSearch />
            <input type="text" placeholder="Search" />
          </div>
          <button className="admin-add-button">
            <FiPlus /> Add User
          </button>
        </div>
        <div className="admin-table">
          <div className="admin-table-header">
            <span>Full Name</span>
            <span>Email</span>
            <span>Phone</span>
            <span>Role</span>
            <span>Status</span>
            <span>Actions</span>
          </div>
          {users.map((user, index) => (
            <div className="admin-table-row" key={index}>
              <span>{user.name}</span>
              <span>{user.email}</span>
              <span>{user.phone}</span>
              <span>
                <div className={`admin-badge ${getRoleClass(user.role)}`}>
                  {user.role}
                </div>
              </span>
              <span>
                <div
                  className={`admin-badge ${
                    user.status === "Active"
                      ? "status-active"
                      : "status-inactive"
                  }`}
                >
                  {user.status}
                </div>
              </span>
              <span className="admin-table-actions">
                <FiEye />
                <FiTrash2 />
              </span>
            </div>
          ))}
        </div>
      </div>
    </AdminLayout>
  );
};

export default UserManagementPage;
