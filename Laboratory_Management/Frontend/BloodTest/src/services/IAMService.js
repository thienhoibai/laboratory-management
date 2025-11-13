import React, { useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Form } from "antd";
import { IAMServiceAPI } from "../apis/IAMServiceAPI";
import { toast } from "react-toastify";
import { jwtDecode } from "jwt-decode";
import { setUserData } from "../utils/auth";

export const useResetPassword = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");

  const onFinish = async (values) => {
    if (!token) {
      toast.error("Thiếu token đặt lại mật khẩu.");
      return;
    }
    setLoading(true);
    try {
      const response = await IAMServiceAPI.ResetPasswordAPI(
        token,
        values.password
      );
      if (response.status >= 200 && response.status < 300) {
        const msg = "Đặt lại mật khẩu thành công.";
        toast.success(msg);
        navigate("/login");
      }
    } catch (err) {
      console.log("Server response:", err.response);

      const serverMsg =
        (typeof err?.response?.data === "string" && err.response.data) ||
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        "Không thể đặt lại mật khẩu.";
      toast.error(serverMsg);
    } finally {
      setLoading(false);
    }
  };

  return { onFinish, loading, token };
};

export const useForgotPassword = () => {
  const [loading, setLoading] = useState(false);

  const onFinish = async (values) => {
    setLoading(true);
    try {
      const response = await IAMServiceAPI.ForgotPasswordAPI(values.email);

      if (response.status >= 200 && response.status < 300) {
        toast.success(
          response?.message ||
            "Yêu cầu đổi mật khẩu đã được gửi về Email, vui lòng kiểm tra lại Email!!"
        );
      }
    } catch (err) {
      const serverMsg =
        (typeof err?.response?.data === "string" && err.response.data) ||
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        "Gửi yêu cầu thất bại. Vui lòng thử lại.";
      toast.error(serverMsg);
    } finally {
      setLoading(false);
    }
  };
  return { onFinish, loading };
};

export const useLoginWithPassword = () => {
  const navigate = useNavigate();
  const onFinish = async (values) => {
    try {
      const response = await IAMServiceAPI.LoginWithPassword(
        values.username,
        values.password
      );

      const data = response?.data.data || {};
      const decode = jwtDecode(data.accessToken);
      const role =
        decode["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      const perm = decode["perm"];

      if (response.status === 200) {
        localStorage.setItem("accessToken", data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);
        localStorage.setItem("expiresAt", data.expiresAt);
        localStorage.setItem("permissions", JSON.stringify(perm) || []);
        setUserData(data);
        if (role === "Customer" || role === "Patient") {
          toast.success("Đăng nhập thành công!");
          navigate("/");
        } else if (role === "Admin" || role === "Manager" || role === "Staff") {
          toast.success("Đăng nhập thành công!");
          navigate("/dashboard");
        }
      }
    } catch (error) {
      if (error.response?.status === 423) {
        toast.error("Tài Khoản Của Bạn Đã Bị Khóa!!");
        return;
      }
      const serverMsg =
        (typeof error?.response?.data === "string" && error.response.data) ||
        error?.response?.data?.message ||
        error?.response?.data?.error ||
        "";
      toast.error(
        typeof serverMsg === "string" && serverMsg.trim()
          ? serverMsg
          : "Tên đăng nhập hoặc mật khẩu không chính xác!"
      );
    }
  };
  return { onFinish };
};

export const useLoginWithGoogle = () => {
  const navigate = useNavigate();
  const handleGoogleLoginSuccess = async (credentialResponse) => {
    try {
      const idToken = credentialResponse.credential;
      if (!idToken) {
        toast.error("Không nhận được token từ Google");
        return;
      }

      // Gửi idToken sang backend để xác thực
      const response = await IAMServiceAPI.LoginWithGoogle(idToken);

      const data = response?.data.data || {};
      const decode = jwtDecode(data.accessToken);
      const role =
        decode["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      const perm = decode["perm"];

      if (response.status <= 200 && response.status < 300) {
        localStorage.setItem("accessToken", data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);
        localStorage.setItem("expiresAt", data.expiresAt);
        localStorage.setItem("permissions", JSON.stringify(perm) || []);
        setUserData(data);
        if (role === "Customer" || role === "Patient") {
          toast.success("Đăng nhập thành công!");
          navigate("/");
        } else if (role === "Admin" || role === "Manager" || role === "Staff") {
          toast.success("Đăng nhập thành công!");
          navigate("/dashboard");
        }
      } else {
        toast.error("Không nhận được access token từ server");
      }
    } catch (error) {
      console.error("Google login error:", error);
      toast.error("Đăng nhập bằng Google thất bại!");
    }
  };
  return { handleGoogleLoginSuccess };
};

export const useRegister = () => {
  const navigate = useNavigate();
  const [apiErrors, setApiErrors] = useState({});
  const [form] = Form.useForm();
  const onFinish = async (values) => {
    setApiErrors({}); // Clear previous errors

    try {
      const payload = {
        username: values.UserName,
        email: values.email,
        password: values.password,
        confirmPassword: values.confirmPassword,
        fullName: values.fullname,
      };

      const response = await IAMServiceAPI.Register(payload);
      if (response && response.status >= 200 && response.status < 300) {
        toast.success("Đăng ký thành công!");
        navigate("/login");
      } else {
        toast.error("Đăng ký không thành công!");
      }
    } catch (error) {
      console.error("Registration error:", error);

      // Handle API error response
      if (error.response?.data) {
        const errorData = error.response.data;
        const newErrors = {};

        // Check if error has detail field (like DUPLICATE_EMAIL)
        if (errorData.detail) {
          const detail = errorData.detail.toLowerCase();

          // Map error to specific field based on detail content or code
          if (
            errorData.code === "DUPLICATE_EMAIL" ||
            detail.includes("email")
          ) {
            newErrors.email = errorData.detail;
          } else if (
            detail.includes("username") ||
            detail.includes("tên đăng nhập")
          ) {
            newErrors.UserName = errorData.detail;
          } else if (
            detail.includes("password") ||
            detail.includes("mật khẩu")
          ) {
            newErrors.password = errorData.detail;
          } else if (detail.includes("fullname") || detail.includes("họ tên")) {
            newErrors.fullname = errorData.detail;
          } else {
            // Default to email field if can't determine
            newErrors.email = errorData.detail;
          }
        }
        // Check for errors array
        else if (errorData.errors && Array.isArray(errorData.errors)) {
          errorData.errors.forEach((err) => {
            if (err.field) {
              const fieldName = err.field.toLowerCase();
              // Map backend field names to form field names
              const fieldMap = {
                username: "UserName",
                email: "email",
                password: "password",
                confirmpassword: "confirmPassword",
                fullname: "fullname",
              };
              const mappedField = fieldMap[fieldName] || err.field;
              newErrors[mappedField] = err.message || err;
            }
          });
        }
        // Check for validation errors object
        else if (errorData.errors && typeof errorData.errors === "object") {
          Object.keys(errorData.errors).forEach((field) => {
            const fieldName = field.toLowerCase();
            const errorMessages = errorData.errors[field];
            const fieldMap = {
              username: "UserName",
              email: "email",
              password: "password",
              confirmpassword: "confirmPassword",
              fullname: "fullname",
            };
            const mappedField = fieldMap[fieldName] || field;
            newErrors[mappedField] = Array.isArray(errorMessages)
              ? errorMessages.join(", ")
              : errorMessages;
          });
        }
        // Fallback to message
        else if (errorData.message) {
          newErrors.email = errorData.message;
        }

        if (Object.keys(newErrors).length > 0) {
          setApiErrors(newErrors);
          // Set form fields errors programmatically
          const formErrors = Object.keys(newErrors).map((field) => ({
            name: field,
            errors: [newErrors[field]],
          }));
          form.setFields(formErrors);
        } else {
          toast.error("Đăng ký không thành công, vui lòng thử lại sau!");
        }
      } else {
        toast.error("Đăng ký không thành công, vui lòng thử lại sau!");
      }
    }
  };
  return { onFinish, apiErrors, form, setApiErrors };
};
