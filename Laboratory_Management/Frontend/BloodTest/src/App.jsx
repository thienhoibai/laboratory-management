import "./App.css";
import LoginPage from "./pages/login/Login";
import RegisterPage from "./pages/register/Register";
import HomePage from "./pages/home/Home";
import BookingPage from "./pages/booking/Booking";
import HistoryPage from "./pages/history/History";
import ProfilePage from "./components/profile/ProfilePage";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
// import React from "react";
// import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Booking from "./pages/booking/Booking";
import CatalogSelection from "./components/booking/CatalogSelection";

import DashboardPage from "./pages/admin/dashboard/DashboardPage";
import PatientManagement from "./pages/admin/patient/PatientManagement";
import AdminDashboardPage from "./pages/admin/dashboard/AdminDashboardPage";
import AdminUsersPage from "./pages/admin/user/AdminUsersPage";
import AdminRolesPage from "./pages/admin/roles/AdminRolesPage";
import AdminInstrumentsPage from "./pages/admin/instruments/AdminInstrumentsPage";
import AdminReagentsPage from "./pages/admin/reagents/AdminReagentsPage";
import AdminBlogsPage from "./pages/admin/blogs/AdminBlogsPage";
import AdminPatientsPage from "./pages/admin/patients/AdminPatientsPage";
import AdminTestOrdersPage from "./pages/admin/test-orders/AdminTestOrdersPage";
import AdminPackagesPage from "./pages/admin/packages/AdminPackagesPage";
import AdminCatalogsPage from "./pages/admin/catalogs/AdminCatalogsPage";
import AdminParameterPage from "./pages/admin/parameter/AdminParameterPage";
import AdminReportsPage from "./pages/admin/reports/AdminReportsPage";
import AdminAppointmentSchedulePage from "./pages/admin/appointment-schedule/AdminAppointmentSchedulePage";

import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css"; // Import CSS
import MedicalRecord from "./pages/medical-record/MedicalRecord";
import ForgotPass from "./components/authen-form/ForgotPassword";
import ResetPass from "./components/authen-form/ResetPassword/ResetPassword";
import AppointmentSchedule from "./pages/lab-staff/appointment-schedule/appointment-schedule";
import LabStaffDashboard from "./pages/lab-staff/dashboard/dashboard";
import LabStaffLanding from "./pages/lab-staff/landing/LabStaffLanding";
import CreatePatient from "./components/profile/CreateProfile";
import BlogPage from "./pages/blog/BlogPage";
import BlogDetailPage from "./pages/blog/BlogDetailPage";

function App() {
  const router = createBrowserRouter([
    {
      path: "/",
      element: <HomePage />,
    },
    {
      path: "login",
      element: <LoginPage />,
    },
    {
      path: "register",
      element: <RegisterPage />,
    },
    {
      path: "booking",
      element: <BookingPage />,
    },
    {
      path: "history",
      element: <HistoryPage />,
    },
    {
      path: "profile",
      element: <ProfilePage />,
    },
    {
      path: "/booking",
      element: <Booking />,
    },
    {
      path: "/booking/catalog",
      element: <CatalogSelection />,
    },
    {
      path: "/dashboard",
      element: <AdminDashboardPage />,
    },
    {
      path: "/appointment-schedule",
      element: <AdminAppointmentSchedulePage />,
    },
    {
      path: "/users",
      element: <AdminUsersPage />,
    },
    {
      path: "/roles",
      element: <AdminRolesPage />,
    },
    {
      path: "/instruments",
      element: <AdminInstrumentsPage />,
    },
    {
      path: "/reagents",
      element: <AdminReagentsPage />,
    },
    {
      path: "/blogs",
      element: <AdminBlogsPage />,
    },
    {
      path: "/patients",
      element: <AdminPatientsPage />,
    },
    {
      path: "/test-orders",
      element: <AdminTestOrdersPage />,
    },
    {
      path: "/packages",
      element: <AdminPackagesPage />,
    },
    {
      path: "/catalogs",
      element: <AdminCatalogsPage />,
    },
    {
      path: "/parameter",
      element: <AdminParameterPage />,
    },
    {
      path: "/reports",
      element: <AdminReportsPage />,
    },
    {
      path: "admin/patients",
      element: <PatientManagement />,
    },
    {
      path: "medical-record",
      element: <MedicalRecord />,
    },
    {
      path: "forgot-password",
      element: <ForgotPass />,
    },
    {
      path: "reset-password",
      element: <ResetPass />,
    },
    {
      path: "lab-staff",
      element: <LabStaffLanding />,
    },
    {
      path: "lab-staff/dashboard",
      element: <LabStaffDashboard />,
    },
    {
      path: "lab-staff/appointment-schedule",
      element: <AppointmentSchedule />,
    },
    {
      path: "create-profile",
      element: <CreatePatient />,
    },
    {
      path: "blog",
      element: <BlogPage />,
    },
    {
      path: "blog/:id",
      element: <BlogDetailPage />,
    },
  ]);

  return (
    <>
      <RouterProvider router={router} />
      <ToastContainer
        position="bottom-center"
        autoClose={2000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="light"
      />
    </>
  );
}

export default App;
