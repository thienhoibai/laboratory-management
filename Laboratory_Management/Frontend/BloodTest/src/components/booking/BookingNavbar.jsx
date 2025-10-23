import React, { useState, useEffect } from "react";
import { Link, useLocation } from "react-router-dom";
import { isAuthenticated, demoLogout } from "../../utils/auth";
import "./BookingNavbar.css";

function BookingNavbar() {
  const [loggedIn, setLoggedIn] = useState(false);
  const location = useLocation();

  useEffect(() => {
    setLoggedIn(isAuthenticated());
  }, []);

  const handleLogout = () => {
    demoLogout();
    setLoggedIn(false);
  };

  const isActive = (path) => {
    return location.pathname === path;
  };

  return (
    <header className="booking-navbar">
      <div className="booking-navbar-inner">
        <div className="booking-navbar-logo">
          <span className="booking-navbar-logo-icon">
            <img
              src="/logo.png"
              alt="HemaLink Logo"
              className="booking-navbar-logo-img"
            />
          </span>
          <span className="booking-navbar-logo-text">HemaLink</span>
        </div>

        <nav className="booking-navbar-menu">
          <Link
            to="/"
            className={`booking-navbar-link ${isActive("/") ? "active" : ""}`}
          >
            Trang chủ
          </Link>
          <Link
            to="/booking"
            className={`booking-navbar-link ${
              isActive("/booking") ? "active" : ""
            }`}
          >
            Đặt lịch
          </Link>
          <Link
            to="/results"
            className={`booking-navbar-link ${
              isActive("/results") ? "active" : ""
            }`}
          >
            Kết quả
          </Link>
          <Link
            to="/history"
            className={`booking-navbar-link ${
              isActive("/history") ? "active" : ""
            }`}
          >
            Lịch sử
          </Link>
        </nav>

        <div className="booking-navbar-actions">
          {loggedIn ? (
            <>
              <span className="booking-navbar-user">Xin chào!</span>
              <button onClick={handleLogout} className="booking-navbar-logout">
                Đăng xuất
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="booking-navbar-login">
                Đăng nhập
              </Link>
              <Link to="/register" className="booking-navbar-register">
                Đăng ký
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

export default BookingNavbar;
