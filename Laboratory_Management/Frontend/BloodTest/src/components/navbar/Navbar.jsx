import React, { useState, useEffect } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { getUserData, logoutUser } from "../../utils/auth";
// import api from "../../configs/axios";
import "./Navbar.css";
import api from "../../configs/axios";
import { setAuthToken } from "../../utils/auth";
// import { usePermission } from "../../utils/permission";
import ChangePasswordModal from "../profile/ChangePassword";

function Navbar() {
  // const { can } = usePermission();

  const [user, setUser] = useState(null);
  const [openChange, setOpenChange] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const userData = getUserData();
    setUser(userData);
  }, []);

  const handleLogout = async () => {
    await logoutUser();
    setUser(null);
    navigate("/login");
  };

  const handleProfileClick = async () => {
    try {
      const token = localStorage.getItem("accessToken");
      setAuthToken(token);
      const response = await api.get(`patient/v1/patients/me`);

      if (response.data && response.data.succeeded === true) {
        navigate("/profile");
      } else {
        navigate("/create-profile");
      }
    } catch (error) {
      console.error("Error checking patient profile:", error);
      navigate("/create-profile");
    }
  };

  const handleNavClick = (e, sectionId) => {
    e.preventDefault();

    // Tính toán offset của navbar (72px theo CSS)
    const navbarHeight = 72;

    // Nếu đang ở trang chủ, scroll đến section
    if (location.pathname === "/") {
      const element = document.getElementById(sectionId);
      if (element) {
        const elementPosition = element.getBoundingClientRect().top;
        const offsetPosition =
          elementPosition + window.pageYOffset - navbarHeight;

        window.scrollTo({
          top: offsetPosition,
          behavior: "smooth",
        });
      }
    } else {
      // Nếu không ở trang chủ, điều hướng về trang chủ với hash
      navigate(`/#${sectionId}`, { replace: false });
      // Sau khi navigate, scroll đến section
      setTimeout(() => {
        const element = document.getElementById(sectionId);
        if (element) {
          const elementPosition = element.getBoundingClientRect().top;
          const offsetPosition =
            elementPosition + window.pageYOffset - navbarHeight;

          window.scrollTo({
            top: offsetPosition,
            behavior: "smooth",
          });
        }
      }, 200);
    }
  };

  const handleLogoClick = (e) => {
    e.preventDefault();
    navigate("/");
    // Scroll to top when going to home
    setTimeout(() => {
      window.scrollTo({ top: 0, behavior: "smooth" });
    }, 100);
  };

  // Handle scroll to section when navigating from another page with hash
  useEffect(() => {
    if (location.pathname === "/" && location.hash) {
      const sectionId = location.hash.substring(1);
      const navbarHeight = 72;
      setTimeout(() => {
        const element = document.getElementById(sectionId);
        if (element) {
          const elementPosition = element.getBoundingClientRect().top;
          const offsetPosition =
            elementPosition + window.pageYOffset - navbarHeight;

          window.scrollTo({
            top: offsetPosition,
            behavior: "smooth",
          });
        }
      }, 200);
    }
  }, [location]);

  // Track active section on scroll
  const [activeSection, setActiveSection] = useState("hero");
  useEffect(() => {
    if (location.pathname !== "/") {
      setActiveSection("");
      return;
    }

    const sections = ["hero", "services", "equipments", "blog", "bundles"];
    const handleScroll = () => {
      const scrollPosition = window.scrollY + 150;

      // Check if at top of page
      if (window.scrollY < 100) {
        setActiveSection("hero");
        return;
      }

      // Find the section currently in view
      for (let i = sections.length - 1; i >= 0; i--) {
        const section = document.getElementById(sections[i]);
        if (section) {
          const offsetTop = section.offsetTop;
          const offsetHeight = section.offsetHeight;
          if (
            scrollPosition >= offsetTop &&
            scrollPosition < offsetTop + offsetHeight
          ) {
            setActiveSection(sections[i]);
            break;
          }
        }
      }
    };

    window.addEventListener("scroll", handleScroll, { passive: true });
    handleScroll(); // Initial check
    return () => window.removeEventListener("scroll", handleScroll);
  }, [location.pathname]);

  return (
    <header className="navbar">
      <div className="navbar-inner">
        <Link
          to="/"
          className="navbar-logo"
          onClick={handleLogoClick}
          style={{
            textDecoration: "none",
            display: "flex",
            alignItems: "center",
            gap: "8px",
          }}
        >
          <span className="navbar-logo-icon">
            <img
              src="/logo.png"
              alt="HemaLink Logo"
              className="navbar-logo-img"
            />
          </span>
          <span className="navbar-logo-text">HemaLink</span>
        </Link>
        <nav className="navbar-menu">
          <a
            href="#hero"
            className={
              location.pathname === "/" && activeSection === "hero"
                ? "active"
                : ""
            }
            onClick={(e) => handleNavClick(e, "hero")}
          >
            Trang chủ
          </a>
          <a
            href="#services"
            className={
              location.pathname === "/" && activeSection === "services"
                ? "active"
                : ""
            }
            onClick={(e) => handleNavClick(e, "services")}
          >
            Dịch vụ
          </a>
          <a
            href="#equipments"
            className={
              location.pathname === "/" && activeSection === "equipments"
                ? "active"
                : ""
            }
            onClick={(e) => handleNavClick(e, "equipments")}
          >
            Thiết bị
          </a>
          <a
            href="#blog"
            className={
              location.pathname === "/" && activeSection === "blog"
                ? "active"
                : ""
            }
            onClick={(e) => handleNavClick(e, "blog")}
          >
            Blog
          </a>
          <a
            href="#bundles"
            className={
              location.pathname === "/" && activeSection === "bundles"
                ? "active"
                : ""
            }
            onClick={(e) => handleNavClick(e, "bundles")}
          >
            Gói xét nghiệm
          </a>
          <Link
            to="/booking"
            className={`navbar-link ${
              location.pathname === "/booking" ? "active" : ""
            }`}
          >
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
              <div className="navbar-avatar-dropdown" tabIndex={0}>
                <div
                  className="navbar-avatar-icon"
                  role="button"
                  aria-haspopup="true"
                  style={{ cursor: "pointer" }}
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

                <div className="avatar-menu" role="menu">
                  <button
                    className="avatar-menu-item"
                    onClick={handleProfileClick}
                    type="button"
                  >
                    Thông tin cá nhân
                  </button>
                  <button
                    className="avatar-menu-item"
                    onClick={() => setOpenChange(true)}
                    type="button"
                  >
                    Đổi mật khẩu
                  </button>
                  <button className="navbar-logout" onClick={handleLogout}>
                    Đăng Xuất
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Render change-password modal */}
      <ChangePasswordModal
        open={openChange}
        onClose={() => setOpenChange(false)}
      />
    </header>
  );
}

export default Navbar;
