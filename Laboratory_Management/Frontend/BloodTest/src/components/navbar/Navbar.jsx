import React, { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { getUserData, removeUserData } from "../../utils/auth";
import "./Navbar.css";

function Navbar() {
  const [user, setUser] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const userData = getUserData();
    setUser(userData);
  }, []);

  const handleLogout = () => {
    removeUserData();
    setUser(null);
    navigate("/login");
  };

  return (
    <header className="navbar">
      <div className="navbar-inner">
        <div className="navbar-logo">
          <span className="navbar-logo-icon">
            <img
              src="/logo.png"
              alt="HemaLink Logo"
              className="navbar-logo-img"
            />
          </span>
          <span className="navbar-logo-text">HemaLink</span>
        </div>
        <nav className="navbar-menu">
          <a href="#hero" className="active">
            Trang chủ
          </a>
          <a href="#services">Dịch vụ</a>
          <a href="#equipments">Thiết bị</a>
          <a href="#blog">Blog</a>
          <Link to="/booking" className="navbar-link">
            Đặt Lịch
          </Link>
        </nav>
        <div className="navbar-actions">
          {!user ? (
            <>
              <a href="/login" className="navbar-login">
                Đăng nhập
              </a>
              <a href="/register" className="navbar-register">
                Đăng ký
              </a>
            </>
          ) : (
            <div className="navbar-user-info">
              <div className="navbar-notification">
                <svg
                  className="notification-icon"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
                  <path d="M13.73 21a2 2 0 0 1-3.46 0" />
                </svg>
              </div>
              <div
                className="navbar-avatar-dropdown"
                onClick={() => navigate("/profile")}
              >
                <svg
                  className="user-icon"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                  <circle cx="12" cy="7" r="4" />
                </svg>
              </div>
              <button className="navbar-logout" onClick={handleLogout}>
                Đăng Xuất
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}

export default Navbar;
