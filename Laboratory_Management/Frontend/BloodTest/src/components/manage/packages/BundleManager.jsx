import React, { useEffect, useMemo, useState } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import { Pagination } from "antd";
import { FiPlus, FiSearch, FiEdit2, FiTrash2, FiX } from "react-icons/fi";
import { toast } from "react-toastify";
import { setAuthToken } from "../../../utils/auth";
import {
  getAllBundles,
  createBundle,
  updateBundle,
  deleteBundle,
  addCatalogsToBundle,
  removeCatalogsFromBundle,
  getCatalogsOfBundle,
  getAllCatalogs,
} from "../../../services/TestOrderService.jsx";
import "./BundleManager.css";

const getCatalogId = (catalog) =>
  catalog?.catalogId ?? catalog?.id ?? catalog?.Id ?? null;

// Helper để so sánh ID một cách an toàn (convert về string để so sánh)
const compareCatalogIds = (id1, id2) => {
  if (!id1 || !id2) return false;
  return String(id1) === String(id2);
};

const getBundleId = (bundle) =>
  bundle?.bundleId ?? bundle?.id ?? bundle?.Id ?? null;

// Helper function để chuẩn hóa giá trị isActive
const normalizeIsActive = (bundle) => {
  // Kiểm tra nhiều field name có thể có
  const activeValue = bundle?.isActive ?? bundle?.active ?? bundle?.status;

  // Xử lý các trường hợp: boolean, string, number
  if (typeof activeValue === "boolean") {
    return activeValue;
  }
  if (typeof activeValue === "string") {
    return activeValue.toLowerCase() === "true" || activeValue === "1";
  }
  if (typeof activeValue === "number") {
    return activeValue === 1 || activeValue > 0;
  }
  // Mặc định là true nếu không có giá trị
  return true;
};

const BundleManager = () => {
  const [bundles, setBundles] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [total, setTotal] = useState(0);
  const [searchInput, setSearchInput] = useState("");
  const [searchDebounce, setSearchDebounce] = useState("");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState("create");
  const [selectedBundle, setSelectedBundle] = useState(null);
  const [formData, setFormData] = useState({
    bundleName: "",
    description: "",
    price: "",
    isActive: true,
  });
  const [isSaving, setIsSaving] = useState(false);
  const [isDetailLoading, setIsDetailLoading] = useState(false);
  const [initialCatalogIds, setInitialCatalogIds] = useState([]);

  const [availableCatalogs, setAvailableCatalogs] = useState([]);
  const [catalogSearch, setCatalogSearch] = useState("");
  const [selectedCatalogs, setSelectedCatalogs] = useState([]);
  const [catalogsLoading, setCatalogsLoading] = useState(false);

  // Delete confirmation modal
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [bundleToDelete, setBundleToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (token) setAuthToken(token);
    preloadCatalogs();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearchDebounce(searchInput.trim());
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    fetchBundles();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize, searchDebounce]);

  const preloadCatalogs = async () => {
    setCatalogsLoading(true);
    try {
      const { items } = await getAllCatalogs({ page: 1, pageSize: 1000 });
      setAvailableCatalogs(items || []);
    } catch (error) {
      console.error("Error loading catalogs:", error);
      toast.error("Không thể tải danh mục xét nghiệm");
    } finally {
      setCatalogsLoading(false);
    }
  };

  const fetchBundles = async () => {
    setIsLoading(true);
    try {
      const query = { page, pageSize };
      if (searchDebounce) query.search = searchDebounce;
      const { items, meta } = await getAllBundles(query);
      // Chuẩn hóa isActive cho tất cả bundles
      const normalizedBundles = (items || []).map((bundle) => ({
        ...bundle,
        isActive: normalizeIsActive(bundle),
      }));
      setBundles(normalizedBundles);
      setTotal(meta?.totalItems ?? items?.length ?? 0);
    } catch (error) {
      console.error("Error fetching bundles:", error);
      toast.error("Không thể tải danh sách gói xét nghiệm");
    } finally {
      setIsLoading(false);
    }
  };

  const handlePageChange = (newPage, newPageSize) => {
    if (newPageSize !== pageSize) {
      setPageSize(newPageSize);
      setPage(1);
    } else {
      setPage(newPage);
    }
  };

  const handleOpenCreateModal = () => {
    setModalMode("create");
    setSelectedBundle(null);
    setFormData({
      bundleName: "",
      description: "",
      price: "",
      isActive: true,
    });
    setSelectedCatalogs([]);
    setInitialCatalogIds([]);
    setCatalogSearch("");
    setIsModalOpen(true);
  };

  const handleOpenEditModal = async (bundle) => {
    const bundleId = getBundleId(bundle);
    if (!bundleId) return;
    setModalMode("edit");
    setSelectedBundle(bundle);
    setIsModalOpen(true);
    setCatalogSearch("");
    setIsDetailLoading(true);

    try {
      // Đảm bảo có danh sách catalogs đầy đủ để map - luôn load lại để đảm bảo có dữ liệu mới nhất
      let catalogsToMap = availableCatalogs;
      if (catalogsToMap.length === 0) {
        const { items } = await getAllCatalogs({ page: 1, pageSize: 1000 });
        catalogsToMap = items || [];
        setAvailableCatalogs(catalogsToMap);
      }

      // Chỉ gọi API GET CatalogBundle/{bundleId} để lấy thông tin bundle và catalogs
      // Response trả về: [{bundleId, bundleName, description, price, catalogs: [...]}]
      const bundleData = await getCatalogsOfBundle(bundleId);
      console.log(
        "[BundleManager] API Response from getCatalogsOfBundle:",
        bundleData
      );

      // Xử lý trường hợp 204 No Content hoặc null
      if (!bundleData) {
        console.log("[BundleManager] No data from API, using bundle data");
        setFormData({
          bundleName: bundle.bundleName || "",
          description: bundle.description || "",
          price: bundle.price ?? "",
          isActive: bundle.isActive ?? true,
        });
        setSelectedCatalogs([]);
        setInitialCatalogIds([]);
        setIsDetailLoading(false);
        return;
      }

      // Lấy thông tin bundle từ response
      const bundleInfo = bundleData;
      console.log("[BundleManager] Bundle info:", bundleInfo);

      // Set form data từ API response
      setFormData({
        bundleName: bundleInfo.bundleName || bundle.bundleName || "",
        description: bundleInfo.description || bundle.description || "",
        price: bundleInfo.price ?? bundle.price ?? "",
        isActive: bundle.isActive ?? true, // Lấy từ bundle hiện tại vì API không trả về isActive
      });

      // Lấy danh sách catalogs từ response
      const catalogs = bundleInfo.catalogs || [];
      console.log(
        "[BundleManager] Catalogs from API:",
        catalogs,
        "count:",
        catalogs.length
      );
      console.log(
        "[BundleManager] availableCatalogs count:",
        catalogsToMap.length
      );

      // Normalize catalogs - đảm bảo là array
      let normalized = Array.isArray(catalogs) ? catalogs : [];
      console.log("[BundleManager] Normalized catalogs:", normalized);
      console.log("[BundleManager] Normalized count:", normalized.length);

      // Map catalogs từ API
      // API trả về catalogs đã có đầy đủ thông tin: {catalogId, testName, description, price}
      const mappedCatalogs = normalized
        .map((catalog, index) => {
          console.log(`[BundleManager] Processing catalog ${index}:`, catalog);

          if (!catalog || typeof catalog !== "object") {
            console.log(`[BundleManager] Invalid catalog, skipping`);
            return null;
          }

          const catalogId = getCatalogId(catalog);
          console.log(
            `[BundleManager] Extracted catalogId:`,
            catalogId,
            "from catalog:",
            catalog
          );

          if (!catalogId) {
            console.log(`[BundleManager] No catalogId found, skipping`);
            return null;
          }

          // Ưu tiên tìm trong availableCatalogs để có đầy đủ thông tin nhất
          const fullCatalog = catalogsToMap.find((ac) => {
            const acId = getCatalogId(ac);
            const match = compareCatalogIds(acId, catalogId);
            if (match) {
              console.log(
                `[BundleManager] Match found in availableCatalogs:`,
                acId,
                "===",
                catalogId
              );
            }
            return match;
          });

          // Nếu tìm thấy trong availableCatalogs, dùng nó
          if (fullCatalog) {
            console.log(
              `[BundleManager] Using fullCatalog from availableCatalogs:`,
              getCatalogId(fullCatalog)
            );
            return fullCatalog;
          }
          // Nếu không tìm thấy, dùng catalog từ API (đã có đầy đủ thông tin)
          else if (catalog.testName || catalog.catalogName) {
            console.log(
              `[BundleManager] Using catalog from API directly:`,
              catalog.testName || catalog.catalogName
            );
            return catalog;
          }

          console.log(
            `[BundleManager] Catalog not found and no name, skipping. Catalog:`,
            catalog
          );
          return null;
        })
        .filter((catalog) => {
          const isValid = catalog != null && getCatalogId(catalog) != null;
          if (!isValid && catalog) {
            console.log(
              `[BundleManager] Filtering out invalid catalog:`,
              catalog
            );
          }
          return isValid;
        });

      console.log("[BundleManager] Final mappedCatalogs:", mappedCatalogs);
      console.log(
        "[BundleManager] mappedCatalogs count:",
        mappedCatalogs.length
      );
      console.log(
        "[BundleManager] Setting selectedCatalogs to:",
        mappedCatalogs.map((c) => ({
          id: getCatalogId(c),
          name: c.testName || c.catalogName,
        }))
      );

      // Set selected catalogs để hiển thị trong UI
      setSelectedCatalogs(mappedCatalogs);
      setInitialCatalogIds(
        mappedCatalogs.map((catalog) => getCatalogId(catalog)).filter(Boolean)
      );
    } catch (error) {
      console.error("Error loading bundle detail:", error);
      toast.error("Không thể tải thông tin gói xét nghiệm");
      setFormData({
        bundleName: bundle.bundleName || "",
        description: bundle.description || "",
        price: bundle.price ?? "",
        isActive: bundle.isActive ?? true,
      });
      const fallbackCatalogs = bundle.catalogs || [];
      // Đảm bảo có danh sách catalogs đầy đủ để map
      let fallbackCatalogsToMap = availableCatalogs;
      if (fallbackCatalogsToMap.length === 0) {
        try {
          const { items } = await getAllCatalogs({ page: 1, pageSize: 1000 });
          fallbackCatalogsToMap = items || [];
        } catch (err) {
          console.error("Error loading catalogs for fallback:", err);
        }
      }

      // Map fallback catalogs với availableCatalogs
      const mappedFallbackCatalogs = fallbackCatalogs
        .map((catalog) => {
          const catalogId = getCatalogId(catalog);
          const fullCatalog = fallbackCatalogsToMap.find((ac) =>
            compareCatalogIds(getCatalogId(ac), catalogId)
          );
          return fullCatalog || catalog;
        })
        .filter((catalog) => getCatalogId(catalog) != null);

      setSelectedCatalogs(mappedFallbackCatalogs);
      setInitialCatalogIds(
        mappedFallbackCatalogs
          .map((catalog) => getCatalogId(catalog))
          .filter(Boolean)
      );
    } finally {
      setIsDetailLoading(false);
    }
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedBundle(null);
    setIsSaving(false);
    setSelectedCatalogs([]);
    setCatalogSearch("");
    setInitialCatalogIds([]);
    setFormData({
      bundleName: "",
      description: "",
      price: "",
      isActive: true,
    });
  };

  const handleFormChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleClearAllCatalogs = () => {
    if (selectedCatalogs.length === 0) return;
    setSelectedCatalogs([]);
  };

  const handleToggleCatalog = (catalog) => {
    const catalogId = getCatalogId(catalog);
    if (!catalogId) return;
    const exists = selectedCatalogs.some((item) =>
      compareCatalogIds(getCatalogId(item), catalogId)
    );

    if (exists) {
      // Uncheck: Xóa catalog khỏi danh sách (sẽ xử lý API khi bấm lưu)
      setSelectedCatalogs((prev) =>
        prev.filter((item) => !compareCatalogIds(getCatalogId(item), catalogId))
      );
    } else {
      // Check: Thêm catalog vào danh sách
      setSelectedCatalogs((prev) => [...prev, catalog]);
    }
  };

  const filteredCatalogs = useMemo(() => {
    if (!catalogSearch) return availableCatalogs;
    const query = catalogSearch.toLowerCase();
    return availableCatalogs.filter((catalog) => {
      const name = catalog.testName || catalog.catalogName || "";
      const desc = catalog.description || "";
      return (
        name.toLowerCase().includes(query) || desc.toLowerCase().includes(query)
      );
    });
  }, [availableCatalogs, catalogSearch]);

  const validateForm = () => {
    if (!formData.bundleName.trim()) {
      toast.error("Tên gói xét nghiệm là bắt buộc");
      return false;
    }
    if (formData.price === "" || isNaN(Number(formData.price))) {
      toast.error("Vui lòng nhập giá hợp lệ");
      return false;
    }
    if (selectedCatalogs.length === 0) {
      toast.error("Vui lòng chọn ít nhất một danh mục xét nghiệm");
      return false;
    }
    return true;
  };

  const handleSaveBundle = async () => {
    if (!validateForm()) return;
    setIsSaving(true);
    try {
      let bundleId = getBundleId(selectedBundle);
      const basePayload = {
        bundleName: formData.bundleName.trim(),
        description: formData.description.trim(),
        price: Number(formData.price) || 0,
      };

      if (modalMode === "create") {
        const payload = {
          ...basePayload,
          isActive: Boolean(formData.isActive),
        };
        const created = await createBundle(payload);
        bundleId = getBundleId(created);

        // Nếu không lấy được bundleId từ response (204 No Content),
        // tìm bundle mới tạo theo tên từ danh sách bundles
        if (!bundleId) {
          console.log(
            "[BundleManager] No bundleId from create response, searching by name..."
          );

          // Chỉ gọi API một lần để tìm bundle mới tạo
          try {
            const searchResult = await getAllBundles({
              page: 1,
              pageSize: 100,
              search: formData.bundleName.trim(),
            });

            // Tìm bundle mới tạo theo tên chính xác
            const newBundle = searchResult.items?.find(
              (b) => b.bundleName === formData.bundleName.trim()
            );

            if (newBundle) {
              bundleId = getBundleId(newBundle);
              console.log(
                "[BundleManager] Found new bundle by name:",
                bundleId
              );
            } else {
              // Nếu không tìm thấy với search, thử tìm trong tất cả bundles
              const allBundles = await getAllBundles({
                page: 1,
                pageSize: 100,
              });
              const foundBundle = allBundles.items?.find(
                (b) => b.bundleName === formData.bundleName.trim()
              );
              if (foundBundle) {
                bundleId = getBundleId(foundBundle);
                console.log(
                  "[BundleManager] Found new bundle in all bundles:",
                  bundleId
                );
              }
            }
          } catch (searchError) {
            console.error(
              "[BundleManager] Error searching for new bundle:",
              searchError
            );
          }
        }

        if (!bundleId) {
          throw new Error(
            "Không xác định được ID của gói sau khi tạo. Vui lòng kiểm tra lại."
          );
        }
      } else if (bundleId) {
        await updateBundle(bundleId, basePayload);
      }

      const currentCatalogIds = selectedCatalogs
        .map((catalog) => getCatalogId(catalog))
        .filter(Boolean);

      if (modalMode === "create") {
        if (currentCatalogIds.length) {
          await addCatalogsToBundle(bundleId, currentCatalogIds);
        }
      } else {
        const initialIdStrings = initialCatalogIds.map((id) => String(id));
        const currentIdStrings = currentCatalogIds.map((id) => String(id));

        const toAdd = currentCatalogIds.filter(
          (id) => !initialIdStrings.includes(String(id))
        );
        const toRemove = initialCatalogIds.filter(
          (id) => !currentIdStrings.includes(String(id))
        );

        if (toRemove.length) {
          await removeCatalogsFromBundle(bundleId, toRemove);
        }
        if (toAdd.length) {
          await addCatalogsToBundle(bundleId, toAdd);
        }
      }

      toast.success(
        modalMode === "create"
          ? "Tạo gói xét nghiệm thành công!"
          : "Cập nhật gói xét nghiệm thành công!"
      );
      await fetchBundles();
      handleCloseModal();
    } catch (error) {
      console.error("Error saving bundle:", error);
      const message =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "Có lỗi xảy ra khi lưu gói xét nghiệm";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteBundle = (bundle) => {
    // Mở modal xác nhận xóa
    setBundleToDelete(bundle);
    setIsDeleteModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!bundleToDelete) return;
    const bundleId = getBundleId(bundleToDelete);
    if (!bundleId) return;

    setIsDeleting(true);
    try {
      await deleteBundle(bundleId);
      toast.success("Đã xóa gói xét nghiệm");
      fetchBundles();
      setIsDeleteModalOpen(false);
      setBundleToDelete(null);
    } catch (error) {
      console.error("Error deleting bundle:", error);
      const message =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "Không thể xóa gói xét nghiệm";
      toast.error(message);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCancelDelete = () => {
    setIsDeleteModalOpen(false);
    setBundleToDelete(null);
  };

  const formatCurrency = (value) => {
    if (value === null || value === undefined) return "-";
    return new Intl.NumberFormat("vi-VN").format(value) + " đ";
  };

  return (
    <AdminLayout
      pageTitle="Quản lý gói xét nghiệm"
      breadcrumbs={[
        { name: "Tổng quan", link: "/admin/dashboard" },
        { name: "Quản lý gói xét nghiệm" },
      ]}
    >
      <div className="bundle-container">
        <div className="bundle-header">
          <div className="bundle-header-left">
            <h1>Quản lý gói xét nghiệm</h1>
            <p>Theo dõi và cấu hình các gói xét nghiệm của trung tâm</p>
          </div>
          <button className="bundle-add-button" onClick={handleOpenCreateModal}>
            <FiPlus size={20} />
            <span>Thêm gói mới</span>
          </button>
        </div>

        <div className="bundle-content">
          <div className="bundle-controls">
            <div className="search-section">
              <div className="search-box">
                <FiSearch size={18} />
                <input
                  type="text"
                  placeholder="Tìm kiếm theo tên hoặc mô tả..."
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                />
              </div>
            </div>
            <div className="page-info">
              Hiển thị {bundles.length} / {total || 0} gói
            </div>
          </div>

          <div className="bundle-table-container">
            <table className="bundle-table">
              <thead>
                <tr>
                  <th>Tên gói</th>
                  <th>Mô tả</th>
                  <th>Danh mục xét nghiệm</th>
                  <th>Giá</th>
                  <th>Trạng thái</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan="6">
                      <div className="loading-container">
                        <div className="loading-spinner"></div>
                        <p>Đang tải dữ liệu...</p>
                      </div>
                    </td>
                  </tr>
                ) : bundles.length > 0 ? (
                  bundles.map((bundle) => (
                    <tr key={getBundleId(bundle)}>
                      <td>
                        <div className="bundle-name">{bundle.bundleName}</div>
                        <div className="bundle-meta">
                          {bundle.catalogs?.length || 0} danh mục
                        </div>
                      </td>
                      <td>
                        {bundle.description ? (
                          <span className="bundle-desc">
                            {bundle.description}
                          </span>
                        ) : (
                          <span className="bundle-desc empty">
                            Chưa có mô tả
                          </span>
                        )}
                      </td>
                      <td>
                        <div className="catalog-tags">
                          {bundle.catalogs && bundle.catalogs.length > 0 ? (
                            bundle.catalogs.slice(0, 3).map((catalog) => (
                              <span
                                className="catalog-tag"
                                key={getCatalogId(catalog)}
                              >
                                {catalog.testName || catalog.catalogName}
                              </span>
                            ))
                          ) : (
                            <span className="catalog-tag empty">
                              Chưa có danh mục
                            </span>
                          )}
                          {bundle.catalogs && bundle.catalogs.length > 3 && (
                            <span className="catalog-tag more">
                              +{bundle.catalogs.length - 3}
                            </span>
                          )}
                        </div>
                      </td>
                      <td>
                        <span className="bundle-price">
                          {formatCurrency(bundle.price)}
                        </span>
                      </td>
                      <td>
                        <span
                          className={`status-badge ${
                            bundle.isActive ? "active" : "inactive"
                          }`}
                        >
                          {bundle.isActive ? "Hoạt động" : "Tạm dừng"}
                        </span>
                      </td>
                      <td>
                        <div className="action-buttons">
                          <button
                            className="action-button edit"
                            onClick={() => handleOpenEditModal(bundle)}
                          >
                            <FiEdit2 size={16} />
                          </button>
                          <button
                            className="action-button delete"
                            onClick={() => handleDeleteBundle(bundle)}
                          >
                            <FiTrash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td
                      colSpan="6"
                      style={{ textAlign: "center", padding: 40 }}
                    >
                      {searchDebounce
                        ? "Không tìm thấy gói xét nghiệm nào"
                        : "Chưa có gói xét nghiệm nào"}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <div className="bundle-pagination">
            <Pagination
              current={page}
              pageSize={pageSize}
              total={total}
              onChange={handlePageChange}
              showSizeChanger
              pageSizeOptions={["5", "10", "20", "50", "100"]}
              showTotal={(tot, range) =>
                tot > 0 ? `${range[0]}-${range[1]} của ${tot} gói` : "0 gói"
              }
            />
          </div>
        </div>
      </div>

      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="bundle-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <h2>
                  {modalMode === "create"
                    ? "Thêm gói xét nghiệm"
                    : "Chỉnh sửa gói"}
                </h2>
                <p>
                  {modalMode === "create"
                    ? "Tạo mới gói và chọn danh mục xét nghiệm phù hợp"
                    : "Điều chỉnh thông tin và danh mục của gói"}
                </p>
              </div>
              <button className="modal-close" onClick={handleCloseModal}>
                <FiX size={20} />
              </button>
            </div>

            <div className="modal-body">
              {isDetailLoading && (
                <p className="form-hint">Đang tải dữ liệu gói xét nghiệm...</p>
              )}
              <div className="bundle-form-grid">
                <div className="bundle-form-left">
                  <div className="form-section">
                    <label className="form-label">Tên gói</label>
                    <input
                      type="text"
                      name="bundleName"
                      className="form-input"
                      placeholder="VD: Gói khám tổng quát"
                      value={formData.bundleName}
                      onChange={handleFormChange}
                      disabled={isSaving}
                    />
                  </div>
                  <div className="form-section grid-2">
                    <div className="form-group">
                      <label className="form-label">Giá (VNĐ)</label>
                      <input
                        type="number"
                        name="price"
                        className="form-input"
                        placeholder="450000"
                        value={formData.price}
                        onChange={handleFormChange}
                        disabled={isSaving}
                      />
                    </div>
                    <div className="form-group switch-group">
                      <label className="form-label">Trạng thái</label>
                      <div className="switch-container">
                        <label className="switch">
                          <input
                            type="checkbox"
                            name="isActive"
                            checked={formData.isActive}
                            onChange={handleFormChange}
                            disabled={isSaving}
                          />
                          <span className="slider" />
                        </label>
                        <span
                          className={`switch-text ${
                            formData.isActive ? "active" : "inactive"
                          }`}
                        >
                          {formData.isActive ? "Hoạt động" : "Tạm dừng"}
                        </span>
                      </div>
                    </div>
                  </div>
                  <div className="form-section">
                    <label className="form-label">Mô tả</label>
                    <textarea
                      name="description"
                      className="form-textarea"
                      rows={4}
                      placeholder="Nhập mô tả cho gói xét nghiệm..."
                      value={formData.description}
                      onChange={handleFormChange}
                      disabled={isSaving}
                    />
                  </div>
                  <div className="form-section selected-summary">
                    <div className="selected-summary-header">
                      <h3>Danh mục đã chọn ({selectedCatalogs.length})</h3>
                      <button
                        type="button"
                        className="clear-btn"
                        onClick={handleClearAllCatalogs}
                        disabled={isSaving || selectedCatalogs.length === 0}
                      >
                        Xóa tất cả
                      </button>
                    </div>
                    <div className="selected-summary-list">
                      {selectedCatalogs.length > 0 ? (
                        selectedCatalogs.map((catalog) => (
                          <div
                            key={getCatalogId(catalog)}
                            className="selected-summary-item"
                          >
                            <span>
                              {catalog.testName || catalog.catalogName || "-"}
                            </span>
                            <button
                              onClick={() => handleToggleCatalog(catalog)}
                              disabled={isSaving}
                            >
                              <FiX size={14} />
                            </button>
                          </div>
                        ))
                      ) : (
                        <p className="empty-text">
                          Chưa có danh mục nào được chọn
                        </p>
                      )}
                    </div>
                  </div>
                </div>

                <div className="bundle-form-right">
                  <div className="parameter-selector">
                    <div className="parameter-selector-header">
                      <div>
                        <h4>Danh mục xét nghiệm</h4>
                        <p>Chọn các danh mục thuộc gói</p>
                      </div>
                      <div className="search-box compact">
                        <FiSearch size={16} />
                        <input
                          type="text"
                          placeholder="Tìm kiếm danh mục..."
                          value={catalogSearch}
                          onChange={(e) => setCatalogSearch(e.target.value)}
                        />
                      </div>
                    </div>
                    <div className="parameters-selection">
                      {catalogsLoading ? (
                        <div className="loading-container small">
                          <div className="loading-spinner"></div>
                          <p>Đang tải danh mục...</p>
                        </div>
                      ) : filteredCatalogs.length > 0 ? (
                        filteredCatalogs.map((catalog) => {
                          const catalogId = getCatalogId(catalog);
                          const isSelected = selectedCatalogs.some((item) =>
                            compareCatalogIds(getCatalogId(item), catalogId)
                          );

                          return (
                            <div
                              key={catalogId}
                              className={`parameter-item ${
                                isSelected ? "selected" : ""
                              }`}
                              onClick={() => handleToggleCatalog(catalog)}
                            >
                              <input
                                type="checkbox"
                                checked={isSelected}
                                readOnly
                              />
                              <div className="parameter-info">
                                <span className="parameter-title">
                                  {catalog.testName || catalog.catalogName}
                                </span>
                                <span className="parameter-meta">
                                  {catalog.description || "Không có mô tả"}
                                </span>
                              </div>
                            </div>
                          );
                        })
                      ) : (
                        <p className="empty-text">
                          Không có danh mục nào phù hợp
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="modal-footer">
              <button
                className="modal-button cancel"
                onClick={handleCloseModal}
                disabled={isSaving}
              >
                Hủy
              </button>
              <button
                className="modal-button primary"
                onClick={handleSaveBundle}
                disabled={isSaving || isDetailLoading}
              >
                {isSaving
                  ? "Đang xử lý..."
                  : modalMode === "create"
                  ? "Tạo gói mới"
                  : "Cập nhật gói"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {isDeleteModalOpen && bundleToDelete && (
        <div className="modal-overlay" onClick={handleCancelDelete}>
          <div
            className="bundle-modal delete-confirm-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2>Xác nhận xóa</h2>
              <button
                className="modal-close"
                onClick={handleCancelDelete}
                disabled={isDeleting}
              >
                <FiX size={20} />
              </button>
            </div>

            <div className="modal-body">
              <div className="delete-confirm-content">
                <div className="delete-confirm-icon">⚠️</div>
                <p className="delete-confirm-message">
                  Bạn có chắc chắn muốn xóa{" "}
                  <strong className="delete-bundle-name">
                    {bundleToDelete.bundleName ||
                      `ID: ${bundleToDelete.bundleId}`}
                  </strong>
                  ?
                </p>
                <p className="delete-confirm-warning">
                  Hành động này không thể hoàn tác
                </p>
              </div>
            </div>

            <div className="modal-footer">
              <button
                className="modal-button cancel"
                onClick={handleCancelDelete}
                disabled={isDeleting}
              >
                Hủy
              </button>
              <button
                className="modal-button primary delete-button"
                onClick={handleConfirmDelete}
                disabled={isDeleting}
              >
                {isDeleting ? "Đang xóa..." : "Xác nhận xóa"}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};

export default BundleManager;
