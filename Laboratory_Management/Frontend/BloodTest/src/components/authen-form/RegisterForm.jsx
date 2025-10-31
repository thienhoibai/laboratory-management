import React, { useState, useEffect } from "react";
import { Button, Form, Input, Card } from "antd";
import { Link, useNavigate } from "react-router-dom";
import "./register.css";
import api from "../../configs/axios";
import { toast } from "react-toastify";

const URL = "iam/api/Auth/register";

function RegisterForm() {
  const navigate = useNavigate();
  const [apiErrors, setApiErrors] = useState({});
  const [form] = Form.useForm();

  // Auto-hide errors after 5 seconds
  useEffect(() => {
    if (Object.keys(apiErrors).length > 0) {
      const timer = setTimeout(() => {
        setApiErrors({});
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [apiErrors]);

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

      const response = await api.post(URL, payload);
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

  return (
    <div className="auth-register-container">
      <div className="auth-register-left">
        <div className="auth-register-left-content">
          <h1 className="auth-register-welcome-title">HemaLink</h1>
          <p className="auth-register-welcome-subtitle">
            Phòng khám hiện đại, kết nối chẩn đoán và điều trị, mang đến dịch vụ
            y tế chất lượng và đáng tin cậy.
          </p>

          <div className="auth-register-features">
            <div className="auth-register-feature-item">
              <div className="auth-register-feature-icon">🧪</div>
              <div className="auth-register-feature-text">
                <h3>Xét nghiệm nhanh chóng</h3>
                <p>Hệ thống hiện đại, kết quả chính xác và kịp thời</p>
              </div>
            </div>

            <div className="auth-register-feature-item">
              <div className="auth-register-feature-icon">📋</div>
              <div className="auth-register-feature-text">
                <h3>Quản lý hồ sơ dễ dàng</h3>
                <p>Theo dõi lịch sử khám chữa bệnh mọi lúc, mọi nơi</p>
              </div>
            </div>

            <div className="auth-register-feature-item">
              <div className="auth-register-feature-icon">👨‍⚕️</div>
              <div className="auth-register-feature-text">
                <h3>Đội ngũ bác sĩ giàu kinh nghiệm</h3>
                <p>Tư vấn tận tình, hỗ trợ chuyên sâu cho từng bệnh nhân</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Right Side - Register Form */}
      <div className="auth-register-right">
        <Card
          className="auth-register-card"
          title="Đăng ký tài khoản"
          extra={
            <Link to="/" className="auth-register-back-home">
              Quay về trang chủ
            </Link>
          }
        >
          <Form
            form={form}
            name="register"
            layout="vertical"
            onFinish={onFinish}
            autoComplete="off"
            className="auth-register-form"
          >
            <div className="auth-register-form-row">
              <div className="auth-register-form-item">
                <Form.Item
                  label="Họ và tên"
                  name="fullname"
                  validateStatus={apiErrors.fullname ? "error" : ""}
                  help={apiErrors.fullname}
                  rules={[{ required: true, message: "Vui lòng nhập họ tên!" }]}
                >
                  <Input
                    placeholder="Nhập họ và tên"
                    size="large"
                    className="placeholder"
                    onChange={() => {
                      if (apiErrors.fullname) {
                        const newErrors = { ...apiErrors };
                        delete newErrors.fullname;
                        setApiErrors(newErrors);
                      }
                    }}
                  />
                </Form.Item>
              </div>
              <div className="auth-register-form-item">
                <Form.Item
                  label="Email"
                  name="email"
                  validateStatus={apiErrors.email ? "error" : ""}
                  help={apiErrors.email}
                  rules={[
                    { required: true, message: "Vui lòng nhập email!" },
                    { type: "email", message: "Email không hợp lệ!" },
                  ]}
                >
                  <Input
                    placeholder="Nhập email"
                    size="large"
                    onChange={() => {
                      if (apiErrors.email) {
                        const newErrors = { ...apiErrors };
                        delete newErrors.email;
                        setApiErrors(newErrors);
                      }
                    }}
                  />
                </Form.Item>
              </div>
            </div>

            <div className="auth-register-form-row">
              <div className="auth-register-form-item">
                <Form.Item
                  label="Tên đăng nhập"
                  name="UserName"
                  validateStatus={apiErrors.UserName ? "error" : ""}
                  help={apiErrors.UserName}
                  rules={[
                    { required: true, message: "Vui lòng nhập tên người dùng" },
                  ]}
                >
                  <Input
                    placeholder="Nhập tên người dùng"
                    size="large"
                    onChange={() => {
                      if (apiErrors.UserName) {
                        const newErrors = { ...apiErrors };
                        delete newErrors.UserName;
                        setApiErrors(newErrors);
                      }
                    }}
                  />
                </Form.Item>
              </div>
              <div className="auth-register-form-item">
                <Form.Item
                  label="Mật khẩu"
                  name="password"
                  validateStatus={apiErrors.password ? "error" : ""}
                  help={apiErrors.password}
                  rules={[
                    { required: true, message: "Vui lòng nhập mật khẩu!" },
                    { min: 6, message: "Mật khẩu ít nhất 6 ký tự" },
                  ]}
                  hasFeedback
                >
                  <Input.Password
                    placeholder="Nhập mật khẩu"
                    size="large"
                    onChange={() => {
                      if (apiErrors.password) {
                        const newErrors = { ...apiErrors };
                        delete newErrors.password;
                        setApiErrors(newErrors);
                      }
                    }}
                  />
                </Form.Item>
              </div>
            </div>

            <Form.Item
              label="Xác nhận mật khẩu"
              name="confirmPassword"
              validateStatus={apiErrors.confirmPassword ? "error" : ""}
              help={apiErrors.confirmPassword}
              dependencies={["password"]}
              hasFeedback
              rules={[
                { required: true, message: "Vui lòng xác nhận mật khẩu!" },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue("password") === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error("Mật khẩu không khớp!"));
                  },
                }),
              ]}
            >
              <Input.Password
                placeholder="Nhập lại mật khẩu"
                size="large"
                onChange={() => {
                  if (apiErrors.confirmPassword) {
                    const newErrors = { ...apiErrors };
                    delete newErrors.confirmPassword;
                    setApiErrors(newErrors);
                  }
                }}
              />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                size="large"
                block
                className="auth-register-button"
              >
                Đăng ký ngay
              </Button>
            </Form.Item>

            <div className="auth-register-login-link">
              Đã có tài khoản?
              <Link to="/login" className="auth-register-login-link-a">
                Đăng nhập ngay
              </Link>
            </div>
          </Form>
        </Card>
      </div>
    </div>
  );
}

export default RegisterForm;
