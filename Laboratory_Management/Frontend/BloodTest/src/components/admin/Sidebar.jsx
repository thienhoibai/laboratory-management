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
  { path: "/admin/dashboard", icon: <FiHome />, name: "Dashboard" },
  { path: "/admin/users", icon: <FiUsers />, name: "Users" },
  { path: "/admin/roles", icon: <FiBriefcase />, name: "Roles" },
  { path: "/admin/instruments", icon: <FiPackage />, name: "Instruments" },
  { path: "/admin/reagents", icon: <FiPackage />, name: "Reagents" },
  { path: "/admin/patients", icon: <FiUsers />, name: "Patients" },
  { path: "/admin/test-orders", icon: <FiFileText />, name: "Test Orders" },
  { path: "/admin/event-log", icon: <FiList />, name: "Event Log" },
  { path: "/admin/reports", icon: <FiBarChart2 />, name: "Reports" },
  { path: "/admin/settings", icon: <FiSettings />, name: "Settings" },
];

const Sidebar = () => {
  const location = useLocation();

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="sidebar-logo">
          {/* Có thể thêm <img/> logo ở đây */}
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
