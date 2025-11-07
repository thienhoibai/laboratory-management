import React, { useState } from "react";
import { Modal, Form, Input, Button } from "antd";
import { setAuthToken } from "../../utils/auth";
import api from "../../configs/axios";
import { toast } from "react-toastify";

const URL = "iam/api/Auth/change-password";

const ChangePasswordModal = ({ open, onClose }) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const token = localStorage.getItem("accessToken");
  const passwordPattern =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

  const handleSubmit = async (values) => {
    setLoading(true);
    setAuthToken(token);
    try {
      const response = await api.post(URL, {
        currentPassword: values.oldPassword,
        newPassword: values.newPassword,
      });

      if (response.status >= 200 && response.status < 300) {
        toast.success(response?.data?.message || "Đổi mật khẩu thành công!");
        form.resetFields();
        onClose();
      }
    } catch (err) {
      const serverMsg =
        (typeof err?.response?.data === "string" && err.response.data) ||
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        err?.message ||
        "Đổi mật khẩu thất bại.";
      toast.error(serverMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title="Đổi mật khẩu"
      open={open}
      onCancel={() => {
        form.resetFields();
        onClose();
      }}
      footer={null}
      className="cp-modal"
    >
      <Form form={form} layout="vertical" onFinish={handleSubmit}>
        <Form.Item
          label="Mật khẩu cũ"
          name="oldPassword"
          rules={[{ required: true, message: "Vui lòng nhập mật khẩu cũ" }]}
        >
          <Input.Password placeholder="Mật khẩu cũ" size="large" />
        </Form.Item>

        <Form.Item
          label="Mật khẩu mới"
          name="newPassword"
          rules={[
            { required: true, message: "Vui lòng nhập mật khẩu mới" },
            {
              validator: (_, value) => {
                if (!value) return Promise.reject();
                return passwordPattern.test(value)
                  ? Promise.resolve()
                  : Promise.reject(
                      new Error(
                        "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt"
                      )
                    );
              },
            },
          ]}
        >
          <Input.Password placeholder="Mật khẩu mới" size="large" />
        </Form.Item>
        <div className="cp-password-hint">
          Mật khẩu cần tối thiểu 8 ký tự và phải bao gồm chữ hoa, chữ thường,
          chữ số và ký tự đặc biệt.
        </div>

        <Form.Item
          label="Xác nhận mật khẩu mới"
          name="confirmPassword"
          dependencies={["newPassword"]}
          rules={[
            { required: true, message: "Vui lòng xác nhận mật khẩu mới" },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue("newPassword") === value) {
                  return Promise.resolve();
                }
                return Promise.reject(
                  new Error("Mật khẩu xác nhận không khớp")
                );
              },
            }),
          ]}
        >
          <Input.Password placeholder="Xác nhận mật khẩu mới" size="large" />
        </Form.Item>

        <Form.Item>
          <div className="cp-actions">
            <Button
              onClick={() => {
                form.resetFields();
                onClose();
              }}
              style={{ marginRight: 8 }}
            >
              Hủy
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              Lưu
            </Button>
          </div>
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default ChangePasswordModal;
