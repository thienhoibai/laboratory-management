import React, { useState } from "react";
import { Card, Form, Input, Button } from "antd";
import { Link } from "react-router-dom";
import { toast } from "react-toastify";
import api from "../../configs/axios";
import "./ForgotPassword.css";

const URL = "iam/api/Auth/forgot-password";
const ForgotPassword = () => {
  const [loading, setLoading] = useState(false);

  const onFinish = async (values) => {
    setLoading(true);
    try {
      const response = await api.post(URL, {
        usernameOrEmail: values.email,
      });

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

  return (
    <div className="fp-container">
      <div className="fp-card-wrapper">
        <Card title="Quên mật khẩu" className="fp-card">
          <p className="fp-subtitle">
            Nhập email hoặc username bạn đã đăng ký. Chúng tôi sẽ gửi hướng dẫn
            đặt lại mật khẩu.
          </p>

          <Form name="forgot" layout="vertical" onFinish={onFinish}>
            <Form.Item
              label="Bạn Có thể Nhập Email or Username"
              name="email"
              rules={[{ required: true, message: "Vui lòng nhập email!" }]}
            >
              <Input placeholder="you@example.com or you" size="large" />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                block
                loading={loading}
                size="large"
              >
                Gửi yêu cầu
              </Button>
            </Form.Item>

            <div className="fp-actions">
              <Link to="/login" className="fp-link">
                Quay lại đăng nhập
              </Link>
              <Link to="/register" className="fp-link">
                Đăng ký tài khoản
              </Link>
            </div>
          </Form>
        </Card>
      </div>
    </div>
  );
};

export default ForgotPassword;
