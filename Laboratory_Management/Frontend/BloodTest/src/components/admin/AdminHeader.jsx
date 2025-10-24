import React from "react";
// import { useNavigate } from "react-router-dom"; // Tạm thời bỏ
// import { FiLogOut } from "react-icons/fi"; // Tạm thời bỏ
import "./layout/AdminLayout.css";

const AdminHeader = ({ pageTitle, breadcrumbs }) => {
  // const navigate = useNavigate(); // Tạm thời bỏ

  const handleLogout = () => {
    // Tạm thời chỉ thông báo
    alert("Logging out...");
    // localStorage.removeItem("user");
    // localStorage.removeItem("token");
    // navigate("/login");
  };

  return (
    <header className="admin-header">
      <div className="admin-breadcrumbs">
        {breadcrumbs && breadcrumbs.map((crumb, index) => (
          <span key={index}>
            {crumb.link ? <a href={crumb.link}>{crumb.name}</a> : crumb.name}
            {index < breadcrumbs.length - 1 && " › "}
          </span>
        ))}
      </div>
      <div className="admin-user-profile">
        <span>Administrator</span>
        <div className="admin-avatar">AD</div>
        {/* Nút đăng xuất tạm thời không dùng icon */}
        <button onClick={handleLogout} className="admin-logout-button" title="Đăng xuất">
          Logout
        </button>
      </div>
    </header>
  );
};

export default AdminHeader;