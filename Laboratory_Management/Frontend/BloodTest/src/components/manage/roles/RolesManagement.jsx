import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./RolesManagement.css";

const RolesManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Roles Management" },
  ];

  return (
    <AdminLayout pageTitle="Roles Management" breadcrumbs={breadcrumbs}>
      <div className="roles-management-content">
        <p>Roles Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default RolesManagement;
