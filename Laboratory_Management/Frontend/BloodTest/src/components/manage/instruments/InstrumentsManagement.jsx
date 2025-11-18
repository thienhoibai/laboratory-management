import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./InstrumentsManagement.css";

const InstrumentsManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Instruments Management" },
  ];

  return (
    <AdminLayout pageTitle="Instruments Management" breadcrumbs={breadcrumbs}>
      <div className="instruments-management-content">
        <p>Instruments Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default InstrumentsManagement;
