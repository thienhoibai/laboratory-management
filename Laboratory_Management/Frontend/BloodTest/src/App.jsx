import "./App.css";
import LoginPage from "./page/login/Login";
import RegisterPage from "./page/register/Register";
import HomePage from "./page/home/Home";
import BookingPage from "./page/booking/Booking";
import ResultsPage from "./page/results/Results";
import HistoryPage from "./page/history/History";
import ProfilePage from "./components/profile/ProfilePage";
import UserManagementPage from "./page/admin/user/UserManagementPage";
import DashboardPage from "./page/admin/dashboard/DashboardPage";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css"; // Import CSS

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
      path: "results",
      element: <ResultsPage />,
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
      path: "admin/dashboard",
      element: <DashboardPage />,
    },
    {
      path: "admin/users",
      element: <UserManagementPage />,
    },
  ]);

  return (
    <>
      <RouterProvider router={router} />
      <ToastContainer
        position="top-right"
        autoClose={3000}
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
