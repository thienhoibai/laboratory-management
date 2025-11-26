import React, { useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import AdminLayout from "../../admin/layout/AdminLayout";
import InstrumentService from "../../../services/InstrumentService";
import "./InstrumentsManagement.css";

const machineStatusOptions = [
  { value: "ACTIVE", label: "Đang hoạt động", badgeClass: "status-active" },
  { value: "OFF", label: "Đang tắt", badgeClass: "status-off" },
  { value: "ERROR", label: "Đang lỗi", badgeClass: "status-error" },
  {
    value: "MAINTENANCE",
    label: "Đang bảo trì",
    badgeClass: "status-maintenance",
  },
];

const reagentStatusOptions = [
  { value: "FULL", label: "Đầy đủ", badgeClass: "pill-full" },
  { value: "LOW", label: "Sắp hết", badgeClass: "pill-low" },
  { value: "EMPTY", label: "Đã hết", badgeClass: "pill-empty" },
];

const defaultForm = {
  code: "",
  name: "",
  machineStatus: machineStatusOptions[0].value,
  reagentStatus: reagentStatusOptions[0].value,
  imageData: "",
  imageName: "",
};

const InstrumentsManagement = () => {
  const breadcrumbs = [
    { name: "Phòng xét nghiệm", link: "/dashboard" },
    { name: "Thiết bị" },
  ];

  const [instruments, setInstruments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [modalMode, setModalMode] = useState("create");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formState, setFormState] = useState(defaultForm);
  const [formError, setFormError] = useState("");
  const [imagePreview, setImagePreview] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const tableData = useMemo(() => instruments ?? [], [instruments]);
  const getInstrumentCode = (instrument) =>
    instrument?.code ?? instrument?.instrumentCode ?? "";

  useEffect(() => {
    loadInstruments();
  }, []);

  const loadInstruments = async () => {
    setLoading(true);
    setError("");
    try {
      const data = await InstrumentService.list();
      setInstruments(data);
    } catch (err) {
      setError(err?.message || "Không thể tải danh sách thiết bị.");
    } finally {
      setLoading(false);
    }
  };

  const openCreateModal = () => {
    setModalMode("create");
    setFormState(defaultForm);
    setImagePreview("");
    setFormError("");
    setIsModalOpen(true);
  };

  const openEditModal = (instrument) => {
    setModalMode("edit");
    const resolvedCode = getInstrumentCode(instrument);
    setFormState({
      code: resolvedCode,
      name: instrument?.name ?? "",
      machineStatus: instrument?.machineStatus ?? machineStatusOptions[0].value,
      reagentStatus: instrument?.reagentStatus ?? reagentStatusOptions[0].value,
      imageData: instrument?.imageData ?? "",
      imageName: instrument?.imageName ?? "",
    });
    setImagePreview(instrument?.imageUrl || instrument?.imageData || "");
    setFormError("");
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSubmitting(false);
  };

  const handleFormChange = (field, value) => {
    setFormState((prev) => ({ ...prev, [field]: value }));
  };

  const handleFileChange = (file) => {
    if (!file) {
      setImagePreview("");
      setFormState((prev) => ({ ...prev, imageData: "", imageName: "" }));
      return;
    }
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result);
      setFormState((prev) => ({
        ...prev,
        imageData: reader.result,
        imageName: file.name,
      }));
    };
    reader.readAsDataURL(file);
  };

  const validateForm = () => {
    if (!formState.code.trim() || !formState.name.trim()) {
      setFormError("Vui lòng nhập đầy đủ mã máy và tên máy.");
      return false;
    }
    setFormError("");
    return true;
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!validateForm()) return;
    setSubmitting(true);
    try {
      const payload = {
        code: formState.code.trim(),
        name: formState.name.trim(),
        machineStatus: formState.machineStatus,
        reagentStatus: formState.reagentStatus,
        imageData: formState.imageData,
      };

      if (modalMode === "create") {
        await InstrumentService.create(payload);
      } else {
        await InstrumentService.update(formState.code, payload);
      }
      closeModal();
      loadInstruments();
    } catch (err) {
      setFormError(err?.message || "Không thể lưu thiết bị.");
      setSubmitting(false);
    }
  };

  const handleDeleteClick = (instrument) => {
    setDeleteTarget(instrument);
    setIsDeleteModalOpen(true);
  };

  const closeDeleteModal = () => {
    setIsDeleteModalOpen(false);
    setDeleteTarget(null);
    setDeleteLoading(false);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const code = getInstrumentCode(deleteTarget);
    setDeleteLoading(true);
    try {
      await InstrumentService.remove(code);
      closeDeleteModal();
      loadInstruments();
    } catch (err) {
      setError(err?.message || "Không thể xóa thiết bị.");
      setDeleteLoading(false);
    }
  };

  const resolveMachineStatus = (value) =>
    machineStatusOptions.find((option) => option.value === value) ||
    machineStatusOptions[0];

  const resolveReagentStatus = (value) =>
    reagentStatusOptions.find((option) => option.value === value) ||
    reagentStatusOptions[0];

  return (
    <AdminLayout pageTitle="Instruments Management" breadcrumbs={breadcrumbs}>
      <div className="instruments-management">
        <div className="instruments-header">
          <div>
            <h2>Danh sách thiết bị</h2>
            <p>Theo dõi trạng thái vận hành và thuốc thử cho từng máy.</p>
          </div>
          <button className="primary-btn" onClick={openCreateModal}>
            + Thêm thiết bị
          </button>
        </div>

        <div className="instruments-card">
          {loading && <p className="state-text">Đang tải danh sách...</p>}
          {!loading && error && <p className="error-text">{error}</p>}
          {!loading && !error && tableData.length === 0 && (
            <p className="state-text">Chưa có thiết bị nào.</p>
          )}

          {!loading && !error && tableData.length > 0 && (
            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Mã máy</th>
                    <th>Tên máy</th>
                    <th>Trạng thái máy</th>
                    <th>Trạng thái thuốc</th>
                    <th>Hành động</th>
                  </tr>
                </thead>
                <tbody>
                  {tableData.map((instrument) => {
                    const code = getInstrumentCode(instrument);
                    const machineStatus = resolveMachineStatus(
                      instrument.machineStatus
                    );
                    const reagentStatus = resolveReagentStatus(
                      instrument.reagentStatus
                    );
                    return (
                      <tr key={code}>
                        <td className="code-cell">{code}</td>
                        <td>{instrument.name}</td>
                        <td>
                          <span
                            className={`status-pill ${machineStatus.badgeClass}`}
                          >
                            {machineStatus.label}
                          </span>
                        </td>
                        <td>
                          <span
                            className={`status-pill ${reagentStatus.badgeClass}`}
                          >
                            {reagentStatus.label}
                          </span>
                        </td>
                        <td>
                          <div className="action-buttons">
                            <button
                              className="ghost-btn"
                              onClick={() => openEditModal(instrument)}
                            >
                              Sửa
                            </button>
                            <button
                              className="danger-btn"
                              onClick={() => handleDeleteClick(instrument)}
                              disabled={
                                deleteLoading &&
                                code === getInstrumentCode(deleteTarget)
                              }
                            >
                              {deleteLoading &&
                              code === getInstrumentCode(deleteTarget)
                                ? "Đang xóa..."
                                : "Xóa"}
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {isModalOpen && (
        <InstrumentModal
          mode={modalMode}
          isOpen={isModalOpen}
          onClose={closeModal}
          onSubmit={handleSubmit}
          onFieldChange={handleFormChange}
          onFileChange={handleFileChange}
          formState={formState}
          formError={formError}
          submitting={submitting}
          imagePreview={imagePreview}
        />
      )}
      {isDeleteModalOpen && deleteTarget && (
        <DeleteConfirmModal
          isOpen={isDeleteModalOpen}
          onClose={closeDeleteModal}
          onConfirm={confirmDelete}
          loading={deleteLoading}
          code={getInstrumentCode(deleteTarget)}
          name={deleteTarget?.name}
        />
      )}
    </AdminLayout>
  );
};

const InstrumentModal = ({
  mode,
  isOpen,
  onClose,
  onSubmit,
  onFieldChange,
  onFileChange,
  formState,
  formError,
  submitting,
  imagePreview,
}) => {
  if (!isOpen) return null;

  const handleFileInput = (event) => {
    const file = event.target.files?.[0];
    onFileChange(file);
  };

  const title = mode === "create" ? "Thêm thiết bị mới" : "Chỉnh sửa thiết bị";
  const submitLabel = mode === "create" ? "Thêm" : "Cập nhật";

  const modalContent = (
    <div className="instrument-modal-backdrop" role="dialog" aria-modal="true">
      <div className="instrument-modal">
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="close-btn" onClick={onClose} aria-label="Đóng">
            ×
          </button>
        </div>
        <form className="modal-form" onSubmit={onSubmit}>
          <div className="modal-body">
            <div className="form-stack">
              <div className="form-field">
                <label htmlFor="instrument-code">Mã máy</label>
                <input
                  id="instrument-code"
                  type="text"
                  name="code"
                  placeholder="VD: EQ001"
                  value={formState.code}
                  onChange={(e) => onFieldChange("code", e.target.value)}
                  disabled={mode === "edit"}
                />
              </div>
              <div className="form-field">
                <label htmlFor="instrument-name">Tên máy</label>
                <input
                  id="instrument-name"
                  type="text"
                  name="name"
                  placeholder="VD: Máy xét nghiệm tự động"
                  value={formState.name}
                  onChange={(e) => onFieldChange("name", e.target.value)}
                />
              </div>

              <div className="form-field upload-field">
                <label>Hình ảnh thiết bị</label>
                <div className="upload-box">
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleFileInput}
                  />
                  <span>⬆ Chọn ảnh</span>
                </div>
                {imagePreview && (
                  <div className="image-preview">
                    <img src={imagePreview} alt="Xem trước thiết bị" />
                    <button
                      type="button"
                      className="ghost-btn"
                      onClick={() => onFileChange(null)}
                    >
                      Xóa ảnh
                    </button>
                  </div>
                )}
              </div>

              <div className="form-row">
                <div className="form-field">
                  <label htmlFor="machine-status">Trạng thái máy</label>
                  <select
                    id="machine-status"
                    value={formState.machineStatus}
                    onChange={(e) =>
                      onFieldChange("machineStatus", e.target.value)
                    }
                  >
                    {machineStatusOptions.map((status) => (
                      <option key={status.value} value={status.value}>
                        {status.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-field">
                  <label htmlFor="reagent-status">Trạng thái thuốc</label>
                  <select
                    id="reagent-status"
                    value={formState.reagentStatus}
                    onChange={(e) =>
                      onFieldChange("reagentStatus", e.target.value)
                    }
                  >
                    {reagentStatusOptions.map((status) => (
                      <option key={status.value} value={status.value}>
                        {status.label}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              {formError && (
                <p className="error-text form-error">{formError}</p>
              )}
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="ghost-btn" onClick={onClose}>
              Hủy
            </button>
            <button type="submit" className="primary-btn" disabled={submitting}>
              {submitting ? "Đang lưu..." : submitLabel}
            </button>
          </div>
        </form>
      </div>
    </div>
  );

  return createPortal(modalContent, document.body);
};

const DeleteConfirmModal = ({
  isOpen,
  onClose,
  onConfirm,
  loading,
  code,
  name,
}) => {
  if (!isOpen) return null;
  const modalContent = (
    <div className="instrument-modal-backdrop" role="dialog" aria-modal="true">
      <div className="instrument-modal confirm-modal">
        <div className="modal-header">
          <h3>Xóa thiết bị</h3>
          <button className="close-btn" onClick={onClose} aria-label="Đóng">
            ×
          </button>
        </div>
        <div className="modal-body">
          <p>
            Bạn có chắc chắn muốn xóa thiết bị{" "}
            <strong>
              {code} - {name}
            </strong>
            ? Thao tác này không thể hoàn tác.
          </p>
        </div>
        <div className="modal-footer">
          <button type="button" className="ghost-btn" onClick={onClose}>
            Hủy
          </button>
          <button
            type="button"
            className="danger-btn"
            onClick={onConfirm}
            disabled={loading}
          >
            {loading ? "Đang xóa..." : "Xóa thiết bị"}
          </button>
        </div>
      </div>
    </div>
  );

  return createPortal(modalContent, document.body);
};

export default InstrumentsManagement;
