import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import "./CatalogsManagement.css";

const CatalogsManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Catalogs Management" },
  ];

  return (
    <AdminLayout pageTitle="Catalogs Management" breadcrumbs={breadcrumbs}>
      <div className="catalogs-management-content">
        <p>Catalogs Management - Coming soon</p>
      </div>
    </AdminLayout>
  );
};

export default CatalogsManagement;
