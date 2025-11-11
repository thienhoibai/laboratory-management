import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Form } from "antd";
import { toast } from "react-toastify";
import { setAuthToken } from "../utils/auth";
import dayjs from "dayjs";
import { PatientServiceAPI } from "../apis/PatientServiceAPI";

export const useCreatePatient = () => {
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleOpenModal = () => setIsModalOpen(true);
  const handleCloseModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleSubmit = async (values) => {
    setIsSubmitting(true);
    try {
      const token = localStorage.getItem("accessToken");
      setAuthToken(token);

      const data = {
        fullName: values.fullName,
        dateOfBirth: dayjs(values.dateOfBirth).format("YYYY-MM-DD"),
        gender: values.gender,
        phone: values.phoneNumber,
        email: values.email,
        address: values.address,
        idNumber: values.identityCard,
        insuranceNumber: values.healthInsurance,
        createdBy: "user",
        createdAt: new Date().toISOString(),
      };

      const response = await PatientServiceAPI.CreateProfile(data);
      if (response.status >= 200 && response.status < 300) {
        toast.success("Tạo hồ sơ bệnh nhân thành công!");
        handleCloseModal();
        navigate("/profile");
      }
    } catch (error) {
      console.error("Error creating profile:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi tạo hồ sơ. Vui lòng thử lại!"
      );
    } finally {
      setIsSubmitting(false);
    }
  };
  return {
    handleSubmit,
    handleOpenModal,
    handleCloseModal,
    isModalOpen,
    isSubmitting,
    form,
  };
};
