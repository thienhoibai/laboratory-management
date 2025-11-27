import React, { useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import AdminLayout from "../../admin/layout/AdminLayout";
import InstrumentService from "../../../services/InstrumentService";
import "./InstrumentsManagement.css";

const MACHINE_STATUS_OPTIONS = [
  {
    key: "ACTIVE",
    rawValues: ["ACTIVE", 0],
    label: "Đang hoạt động",
    badge: "status-active",
  },
  { key: "OFF", rawValues: ["OFF", 1], label: "Đang tắt", badge: "status-off" },
  {
    key: "ERROR",
    rawValues: ["ERROR", 2],
    label: "Đang lỗi",
    badge: "status-error",
  },
  {
    key: "MAINTENANCE",
    rawValues: ["MAINTENANCE", 3],
    label: "Đang bảo trì",
    badge: "status-maintenance",
  },
];

const REAGENT_STATUS_OPTIONS = [
  { key: "FULL", rawValues: ["FULL", 0], label: "Đầy đủ", badge: "pill-full" },
  { key: "LOW", rawValues: ["LOW", 1], label: "Sắp hết", badge: "pill-low" },
  {
    key: "EMPTY",
    rawValues: ["EMPTY", 2],
    label: "Đã hết",
    badge: "pill-empty",
  },
];

const getStatusKey = (options, rawValue) => {
  const matched =
    options.find(
      (option) =>
        option.key === rawValue ||
        option.rawValues.some((val) => val === rawValue)
    ) ?? options[0];
  return matched.key;
};

const getStatusView = (options, rawValue) => {
  return (
    options.find(
      (option) =>
        option.key === rawValue ||
        option.rawValues.some((val) => val === rawValue)
    ) ?? options[0]
  );
};

const EMPTY_FORM = {
  code: "",
  name: "",
  machineStatus: MACHINE_STATUS_OPTIONS[0].key,
  reagentStatus: REAGENT_STATUS_OPTIONS[0].key,
  imageData: "",
};

const mapInstrumentToForm = (instrument) => ({
  code: instrument.code ?? instrument.instrumentCode ?? "",
  name: instrument.name ?? "",
  machineStatus: getStatusKey(
    MACHINE_STATUS_OPTIONS,
    instrument.machineStatus ?? instrument.status
  ),
  reagentStatus: getStatusKey(
    REAGENT_STATUS_OPTIONS,
    instrument.reagentStatus ?? instrument.reagent_status
  ),
  imageData: instrument.imageData ?? "",
});

const buildPayloadFromForm = (formState) => ({
  code: formState.code.trim(),
  name: formState.name.trim(),
  machineStatus: formState.machineStatus,
  reagentStatus: formState.reagentStatus,
  imageData: formState.imageData,
});

const pickInstrumentCode = (instrument) =>
  instrument?.code ?? instrument?.instrumentCode ?? "";

const InstrumentsManagement = () => {
  const breadcrumbs = [
    { name: "Phòng xét nghiệm", link: "/dashboard" },
    { name: "Thiết bị" },
  ];

  const [instruments, setInstruments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState("create"); // create | edit
  const [formState, setFormState] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [imagePreview, setImagePreview] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const tableData = useMemo(() => instruments ?? [], [instruments]);

  useEffect(() => {
    fetchInstruments();
  }, []);

  const fetchInstruments = async () => {
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

  const resetForm = () => {
    setFormState(EMPTY_FORM);
    setImagePreview("");
    setFormError("");
  };

  const openCreateModal = () => {
    resetForm();
    setModalMode("create");
    setIsModalOpen(true);
  };

  const openEditModal = (instrument) => {
    setModalMode("edit");
    setFormState(mapInstrumentToForm(instrument));
    setImagePreview(instrument.imageUrl || instrument.imageData || "");
    setFormError("");
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSubmitting(false);
    resetForm();
  };

  const handleFormChange = (field, value) => {
    setFormState((prev) => ({ ...prev, [field]: value }));
  };

  const handleFileChange = (file) => {
    if (!file) {
      setImagePreview("");
      setFormState((prev) => ({ ...prev, imageData: "" }));
      return;
    }
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result);
      setFormState((prev) => ({ ...prev, imageData: reader.result }));
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
      const payload = buildPayloadFromForm(formState);
      if (modalMode === "create") {
        await InstrumentService.create(payload);
      } else {
        await InstrumentService.update(formState.code, payload);
      }
      closeModal();
      fetchInstruments();
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
    const code = pickInstrumentCode(deleteTarget);
    setDeleteLoading(true);
    try {
      await InstrumentService.remove(code);
      closeDeleteModal();
      fetchInstruments();
    } catch (err) {
      setError(err?.message || "Không thể xóa thiết bị.");
      setDeleteLoading(false);
    }
  };

  const getMachineStatusDisplay = (instrument) =>
    getStatusView(
      MACHINE_STATUS_OPTIONS,
      instrument.machineStatus ?? instrument.status
    );

  const getReagentStatusDisplay = (instrument) =>
    getStatusView(
      REAGENT_STATUS_OPTIONS,
      instrument.reagentStatus ?? instrument.reagent_status
    );

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
                    const code = pickInstrumentCode(instrument);
                    const machineStatus = getMachineStatusDisplay(instrument);
                    const reagentStatus = getReagentStatusDisplay(instrument);
                    return (
                      <tr key={code}>
                        <td className="code-cell">{code}</td>
                        <td>{instrument.name}</td>
                        <td>
                          <span
                            className={`status-pill ${machineStatus.badge}`}
                          >
                            {machineStatus.label}
                          </span>
                        </td>
                        <td>
                          <span
                            className={`status-pill ${reagentStatus.badge}`}
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
                                code === pickInstrumentCode(deleteTarget)
                              }
                            >
                              {deleteLoading &&
                              code === pickInstrumentCode(deleteTarget)
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
          code={pickInstrumentCode(deleteTarget)}
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
                    {MACHINE_STATUS_OPTIONS.map((status) => (
                      <option key={status.key} value={status.key}>
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
                    {REAGENT_STATUS_OPTIONS.map((status) => (
                      <option key={status.key} value={status.key}>
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
