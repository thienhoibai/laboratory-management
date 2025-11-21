import React from "react";
// import { useNavigate } from "react-router-dom"; // Tạm thời bỏ
// import { FiLogOut } from "react-icons/fi"; // Tạm thời bỏ
import "./layout/AdminLayout.css";
import { useNavigate } from "react-router-dom";
import { logoutUser } from "../../utils/auth";
import { toast } from "react-toastify";
import { setAuthToken } from "../../utils/auth";
import { jwtDecode } from "jwt-decode";

const AdminHeader = ({ pageTitle, breadcrumbs }) => {
  const token = localStorage.getItem("accessToken");
  const navigate = useNavigate();
  const deocde = jwtDecode(token);
  let role = ["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

  const handleLogout = async () => {
    setAuthToken(token);
    await logoutUser();
    toast.success("Đăng Xuất Thành Công!");
    navigate("/login");
  };

  return (
    <header className="admin-header">
      <div className="admin-breadcrumbs">
        {breadcrumbs &&
          breadcrumbs.map((crumb, index) => (
            <span key={index}>
              {crumb.link ? <a href={crumb.link}>{crumb.name}</a> : crumb.name}
              {index < breadcrumbs.length - 1 && " › "}
            </span>
          ))}
      </div>
      <div className="admin-user-profile">
        <span>Administrator</span>
        <div className="admin-avatar">AD</div>

        <button
          onClick={handleLogout}
          className="admin-logout-button"
          title="Đăng xuất"
        >
          Đăng xuất
        </button>
      </div>
    </header>
  );
};

export default AdminHeader;
