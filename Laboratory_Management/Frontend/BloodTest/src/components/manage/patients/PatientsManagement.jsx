import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./PatientsManagement.css";

const PatientsManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Patients Management" },
  ];

  return (
    <AdminLayout pageTitle="Patients Management" breadcrumbs={breadcrumbs}>
      <div className="patients-management-content">
        <p>Patients Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default PatientsManagement;
