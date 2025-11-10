import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./ReagentsManagement.css";

const ReagentsManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Reagents Management" },
  ];

  return (
    <AdminLayout pageTitle="Reagents Management" breadcrumbs={breadcrumbs}>
      <div className="reagents-management-content">
        <p>Reagents Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default ReagentsManagement;
