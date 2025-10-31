import React, { useState } from "react";
import { Card, Form, Input, Button } from "antd";
import { useNavigate, Link, useSearchParams } from "react-router-dom";
import { toast } from "react-toastify";
import api from "../../../configs/axios";
import "./ResetPassword.css";

const URL = "iam/api/Auth/reset-password";

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const onFinish = async (values) => {
    console.log(token);
    console.log(values.password);
    if (!token) {
      toast.error("Thiếu token đặt lại mật khẩu.");
      return;
    }
    setLoading(true);
    try {
      const response = await api.post(URL, {
        token: token,
        newPassword: values.password,
      });
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

  return (
    <div className="rp-container">
      <div className="rp-card-wrapper">
        <Card title="Đặt lại mật khẩu" className="rp-card">
          {!token ? (
            <div className="rp-no-token">
              <p>Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.</p>
              <Link to="/forgot-password">
                Yêu cầu gửi lại email đặt lại mật khẩu
              </Link>
            </div>
          ) : (
            <>
              <p className="rp-subtitle">
                Nhập mật khẩu mới cho tài khoản của bạn.
              </p>
              <Form name="reset" layout="vertical" onFinish={onFinish}>
                <Form.Item
                  name="password"
                  label="Mật khẩu mới"
                  rules={[
                    { required: true, message: "Vui lòng nhập mật khẩu mới" },
                    { min: 6, message: "Mật khẩu tối thiểu 6 ký tự" },
                  ]}
                >
                  <Input.Password placeholder="Mật khẩu mới" size="large" />
                </Form.Item>

                <Form.Item
                  name="confirm"
                  label="Xác nhận mật khẩu"
                  dependencies={["password"]}
                  rules={[
                    { required: true, message: "Vui lòng xác nhận mật khẩu" },
                    ({ getFieldValue }) => ({
                      validator(_, value) {
                        if (!value || getFieldValue("password") === value) {
                          return Promise.resolve();
                        }
                        return Promise.reject(
                          new Error("Mật khẩu xác nhận không khớp")
                        );
                      },
                    }),
                  ]}
                >
                  <Input.Password
                    placeholder="Xác nhận mật khẩu"
                    size="large"
                  />
                </Form.Item>

                <Form.Item>
                  <Button
                    type="primary"
                    htmlType="submit"
                    block
                    loading={loading}
                  >
                    Đặt lại mật khẩu
                  </Button>
                </Form.Item>

                <div className="rp-actions">
                  <Link to="/login" className="rp-link">
                    Quay lại đăng nhập
                  </Link>
                </div>
              </Form>
            </>
          )}
        </Card>
      </div>
    </div>
  );
};

export default ResetPassword;
