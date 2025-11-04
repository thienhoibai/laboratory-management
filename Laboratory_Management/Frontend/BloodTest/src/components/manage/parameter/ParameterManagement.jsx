import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./ParameterManagement.css";

const ParameterManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Parameter Management" },
  ];

  return (
    <AdminLayout pageTitle="Parameter Management" breadcrumbs={breadcrumbs}>
      <div className="parameter-management-content">
        <p>Parameter Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default ParameterManagement;
