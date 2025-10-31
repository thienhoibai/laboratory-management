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

import UserManagementPage from "./pages/admin/user/UserManagementPage";
import DashboardPage from "./pages/admin/dashboard/DashboardPage";

import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css"; // Import CSS
import MedicalRecord from "./page/medical-record/MedicalRecord";
import ForgotPass from "./components/authen-form/ForgotPassword";
import ResetPass from "./components/authen-form/ResetPassword/ResetPassword";
import AppointmentSchedule from "./pages/lab-staff/appointment-schedule/appointment-schedule";
import LabStaffDashboard from "./pages/lab-staff/dashboard/dashboard";
import LabStaffLanding from "./pages/lab-staff/landing/LabStaffLanding";
import CreatePatient from "./components/profile/CreateProfile";

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
      path: "admin/dashboard",
      element: <DashboardPage />,
    },
    {
      path: "admin/users",
      element: <UserManagementPage />,
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
