import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./PackagesManagement.css";

const PackagesManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Packages Management" },
  ];

  return (
    <AdminLayout pageTitle="Packages Management" breadcrumbs={breadcrumbs}>
      <div className="packages-management-content">
        <p>Packages Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default PackagesManagement;
