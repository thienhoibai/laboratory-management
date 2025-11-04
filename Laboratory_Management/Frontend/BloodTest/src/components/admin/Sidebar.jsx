import React from "react";
import { Link, useLocation } from "react-router-dom";
import {
  FiHome,
  FiUsers,
  FiBriefcase,
  FiPackage,
  FiFileText,
  FiList,
  FiBarChart2,
  FiSettings,
} from "react-icons/fi";
import "./layout/AdminLayout.css"; // Sidebar cũng dùng chung CSS này

const menuItems = [
  { path: "/admin/dashboard", icon: <FiHome />, name: "Tổng quan" },
  { path: "/admin/users", icon: <FiUsers />, name: "Quản lý người dùng" },
  { path: "/admin/patients", icon: <FiUsers />, name: "Quản lý bệnh nhân" },
  { path: "/admin/roles", icon: <FiBriefcase />, name: "Phân quyền" },
  { path: "/admin/instruments", icon: <FiPackage />, name: "Thiết bị" },
  { path: "/admin/reagents", icon: <FiPackage />, name: "Hóa chất" },
  { path: "/admin/test-orders", icon: <FiFileText />, name: "Đơn xét nghiệm" },
  { path: "/admin/event-log", icon: <FiList />, name: "Nhật ký" },
  { path: "/admin/reports", icon: <FiBarChart2 />, name: "Báo cáo" },
  { path: "/admin/settings", icon: <FiSettings />, name: "Cài đặt" },
];

const Sidebar = () => {
  const location = useLocation();

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="sidebar-logo">
          <img src="/logo.png" alt="HemaLink Logo" />
        </div>
        <div className="sidebar-title">
          <h3>HemaLink</h3>
          <p>Blood Test System</p>
        </div>
      </div>
      <nav className="sidebar-nav">
        <ul>
          {menuItems.map((item) => (
            <li key={item.path}>
              <Link
                to={item.path}
                className={
                  location.pathname.startsWith(item.path) ? "active" : ""
                }
              >
                {item.icon}
                <span>{item.name}</span>
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  );
};

export default Sidebar; // Dòng này rất quan trọng
