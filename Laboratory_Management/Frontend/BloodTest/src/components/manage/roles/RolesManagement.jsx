import React, { useEffect, useState, useMemo } from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import {
  FiSearch,
  FiEdit2,
  FiCheck,
  FiX,
  FiChevronDown,
  FiChevronUp,
  FiShield,
} from "react-icons/fi";
import { Pagination } from "antd";
import { setAuthToken } from "../../../utils/auth";
import { toast } from "react-toastify";
import {
  getRoles,
  getPermissionGroups,
  getRolePermissions,
  updateRolePermissions,
  patchRolePermissionsByModule,
} from "../../../services/IAMService.jsx";
import "./RolesManagement.css";

const getRoleId = (role) => role?.id ?? role?.roleId ?? role?.Id ?? null;

const RolesManagement = () => {
  const breadcrumbs = [
    { name: "Tổng quan", link: "/admin/dashboard" },
    { name: "Quyền truy cập" },
  ];

  // States
  const [roles, setRoles] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [searchInput, setSearchInput] = useState("");
  const [searchDebounce, setSearchDebounce] = useState("");

  // Store permissions for each role
  const [rolePermissionsMap, setRolePermissionsMap] = useState({});
  const [loadingPermissions, setLoadingPermissions] = useState({});

  // Permission groups
  const [permissionGroups, setPermissionGroups] = useState([]);
  const [permissionGroupsLoading, setPermissionGroupsLoading] = useState(false);

  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState(null);
  const [rolePermissions, setRolePermissions] = useState([]);
  const [isSaving, setIsSaving] = useState(false);
  const [isDetailLoading, setIsDetailLoading] = useState(false);
  const [expandedModules, setExpandedModules] = useState(new Set());

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (token) setAuthToken(token);
    // Load permission groups first, then roles will load permissions
    fetchPermissionGroups();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearchDebounce(searchInput.trim());
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    fetchRoles();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize, searchDebounce]);

  // Helper functions
  const getPermissionId = (permission) =>
    permission?.key ??
    permission?.id ??
    permission?.permissionId ??
    permission?.Id ??
    null;

  const getPermissionLabel = (permission) =>
    permission?.label ??
    permission?.name ??
    permission?.permissionName ??
    permission?.key ??
    "-";

  const comparePermissionIds = (id1, id2) => {
    if (!id1 || !id2) return false;
    return String(id1) === String(id2);
  };

  const findPermissionInGroups = (permissionId) => {
    if (!permissionId) return null;
    for (const group of permissionGroups) {
      if (Array.isArray(group.permissions)) {
        const found = group.permissions.find((p) =>
          comparePermissionIds(getPermissionId(p), permissionId)
        );
        if (found) return found;
      }
    }
    return null;
  };

  const fetchPermissionGroups = async () => {
    setPermissionGroupsLoading(true);
    try {
      const groups = await getPermissionGroups();
      setPermissionGroups(Array.isArray(groups) ? groups : []);
    } catch (error) {
      console.error("Error loading permission groups:", error);
      toast.error("Không thể tải nhóm quyền");
    } finally {
      setPermissionGroupsLoading(false);
    }
  };

  const fetchRoles = async () => {
    setIsLoading(true);
    try {
      const params = { page, pageSize };
      if (searchDebounce) params.search = searchDebounce;
      const { items, meta } = await getRoles(params);
      const rolesList = items || [];
      setRoles(rolesList);
      setTotal(meta?.totalItems ?? rolesList.length ?? 0);

      // Fetch permissions for each role
      fetchRolesPermissions(rolesList);
    } catch (error) {
      console.error("Error fetching roles:", error);
      toast.error("Không thể tải danh sách vai trò");
    } finally {
      setIsLoading(false);
    }
  };

  const fetchRolesPermissions = async (rolesList) => {
    if (!rolesList || rolesList.length === 0) return;

    // Set loading state for all roles
    const loadingMap = {};
    rolesList.forEach((role) => {
      const roleId = getRoleId(role);
      if (roleId) loadingMap[roleId] = true;
    });
    setLoadingPermissions(loadingMap);

    try {
      // Fetch permissions for all roles in parallel
      const permissionPromises = rolesList.map(async (role) => {
        const roleId = getRoleId(role);
        if (!roleId) return { roleId: null, permissions: [] };

        try {
          const permissions = await getRolePermissions(roleId);
          const permissionsArray = Array.isArray(permissions)
            ? permissions
            : [];

          // Map permissions to get labels from permission groups
          const mappedPermissions = permissionsArray.map((perm) => {
            if (typeof perm === "string") {
              // Try to find in permission groups
              const found = findPermissionInGroups(perm);
              return found || { key: perm, label: perm };
            }
            // If it's an object, check if we have more info in permission groups
            const permKey = getPermissionId(perm);
            if (permKey) {
              const found = findPermissionInGroups(permKey);
              if (found && found.label) {
                return { ...perm, label: found.label };
              }
            }
            return perm;
          });

          return { roleId, permissions: mappedPermissions };
        } catch (error) {
          console.error(
            `Error fetching permissions for role ${roleId}:`,
            error
          );
          return { roleId, permissions: [] };
        }
      });

      const results = await Promise.all(permissionPromises);

      // Update permissions map
      const newPermissionsMap = { ...rolePermissionsMap };
      results.forEach(({ roleId, permissions }) => {
        if (roleId) {
          newPermissionsMap[roleId] = permissions;
        }
      });
      setRolePermissionsMap(newPermissionsMap);
    } catch (error) {
      console.error("Error fetching roles permissions:", error);
    } finally {
      // Clear loading state
      setLoadingPermissions({});
    }
  };

  const handleOpenEditModal = async (role) => {
    const roleId = getRoleId(role);
    if (!roleId) return;

    setSelectedRole(role);
    setIsModalOpen(true);
    setIsDetailLoading(true);
    setExpandedModules(new Set());

    try {
      const permissions = await getRolePermissions(roleId);
      const permissionsArray = Array.isArray(permissions) ? permissions : [];

      // Nếu API trả về array of strings (permission keys), cần map với permission groups
      // Nếu API trả về array of objects, giữ nguyên
      const mappedPermissions = permissionsArray.map((perm) => {
        // Nếu là string, tìm trong permission groups
        if (typeof perm === "string") {
          const found = findPermissionById(perm);
          return found || { key: perm, label: perm };
        }
        // Nếu là object, giữ nguyên
        return perm;
      });

      setRolePermissions(mappedPermissions);
    } catch (error) {
      console.error("Error loading role permissions:", error);
      toast.error("Không thể tải quyền của vai trò");
      setRolePermissions([]);
    } finally {
      setIsDetailLoading(false);
    }
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedRole(null);
    setRolePermissions([]);
    setExpandedModules(new Set());
    setIsSaving(false);
    setIsDetailLoading(false);
  };

  const togglePermission = (permissionId) => {
    setRolePermissions((prev) => {
      const exists = prev.some((p) =>
        comparePermissionIds(getPermissionId(p), permissionId)
      );
      if (exists) {
        return prev.filter(
          (p) => !comparePermissionIds(getPermissionId(p), permissionId)
        );
      } else {
        // Tìm permission từ groups để thêm vào
        const permission = findPermissionById(permissionId);
        if (permission) {
          return [...prev, permission];
        }
        return prev;
      }
    });
  };

  const findPermissionById = (permissionId) => {
    return findPermissionInGroups(permissionId);
  };

  const handleSavePermissions = async () => {
    if (!selectedRole) return;
    const roleId = getRoleId(selectedRole);
    if (!roleId) return;

    setIsSaving(true);
    try {
      // Lấy permission keys (hoặc IDs) từ selected permissions
      const permissionKeys = rolePermissions
        .map((p) => getPermissionId(p))
        .filter(Boolean);

      // Sử dụng PUT để cập nhật toàn bộ quyền
      // API có thể expect array of permission keys hoặc IDs
      await updateRolePermissions(roleId, permissionKeys);
      toast.success("Cập nhật quyền thành công!");

      // Update permissions map for this role
      const newPermissionsMap = { ...rolePermissionsMap };
      newPermissionsMap[roleId] = rolePermissions;
      setRolePermissionsMap(newPermissionsMap);

      fetchRoles();
      handleCloseModal();
    } catch (error) {
      console.error("Error updating role permissions:", error);
      const message =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "Không thể cập nhật quyền";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  // Hàm để cập nhật quyền theo module cụ thể (sử dụng PATCH)
  const handleUpdateModulePermissions = async (moduleName) => {
    if (!selectedRole || isSaving) return;
    const roleId = getRoleId(selectedRole);
    if (!roleId) return;

    const modulePerms = groupedPermissions[moduleName]?.permissions || [];
    const modulePermissionKeys = modulePerms
      .map((p) => getPermissionId(p))
      .filter(Boolean);
    const selectedKeys = rolePermissions
      .map((p) => getPermissionId(p))
      .filter(Boolean);

    // Lấy các permission keys của module đang được chọn
    const moduleSelectedKeys = modulePermissionKeys.filter((key) =>
      selectedKeys.some((sk) => comparePermissionIds(sk, key))
    );

    setIsSaving(true);
    try {
      // Sử dụng PATCH để cập nhật quyền theo module
      // API expect module name và array of permission keys
      await patchRolePermissionsByModule(
        roleId,
        moduleName,
        moduleSelectedKeys
      );
      toast.success(
        `Đã cập nhật quyền module ${
          groupedPermissions[moduleName]?.moduleLabel || moduleName
        }`
      );
      // Refresh permissions in modal and update map
      const permissions = await getRolePermissions(roleId);
      const permissionsArray = Array.isArray(permissions) ? permissions : [];

      // Map permissions to get labels
      const mappedPermissions = permissionsArray.map((perm) => {
        if (typeof perm === "string") {
          const found = findPermissionInGroups(perm);
          return found || { key: perm, label: perm };
        }
        const permKey = getPermissionId(perm);
        if (permKey) {
          const found = findPermissionInGroups(permKey);
          if (found && found.label) {
            return { ...perm, label: found.label };
          }
        }
        return perm;
      });

      setRolePermissions(mappedPermissions);

      // Update permissions map
      const newPermissionsMap = { ...rolePermissionsMap };
      newPermissionsMap[roleId] = mappedPermissions;
      setRolePermissionsMap(newPermissionsMap);
    } catch (error) {
      console.error("Error updating module permissions:", error);
      const message =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "Không thể cập nhật quyền module";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleToggleModule = (moduleName) => {
    setExpandedModules((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(moduleName)) {
        newSet.delete(moduleName);
      } else {
        newSet.add(moduleName);
      }
      return newSet;
    });
  };

  const toggleModulePermissions = (moduleName, permissions) => {
    const permissionKeys = permissions
      .map((p) => getPermissionId(p))
      .filter(Boolean);
    const selectedKeys = rolePermissions
      .map((p) => getPermissionId(p))
      .filter(Boolean);

    const allSelected = permissionKeys.every((key) =>
      selectedKeys.some((sk) => comparePermissionIds(sk, key))
    );

    if (allSelected) {
      // Bỏ chọn tất cả
      setRolePermissions((prev) =>
        prev.filter(
          (p) =>
            !permissionKeys.some((pk) =>
              comparePermissionIds(getPermissionId(p), pk)
            )
        )
      );
    } else {
      // Chọn tất cả
      const toAdd = permissions.filter(
        (p) =>
          !selectedKeys.some((sk) =>
            comparePermissionIds(getPermissionId(p), sk)
          )
      );
      setRolePermissions((prev) => [...prev, ...toAdd]);
    }
  };

  const filteredRoles = useMemo(() => {
    if (!searchDebounce) return roles;
    const query = searchDebounce.toLowerCase();
    return roles.filter(
      (role) =>
        role.name?.toLowerCase().includes(query) ||
        role.description?.toLowerCase().includes(query)
    );
  }, [roles, searchDebounce]);

  // Group permissions by module
  // Response structure: [{ module, label, permissions: [{ key, label }] }]
  const groupedPermissions = useMemo(() => {
    const groups = {};
    permissionGroups.forEach((group) => {
      // Sử dụng module từ response, fallback về moduleName hoặc label
      const moduleName =
        group.module || group.moduleName || group.label || "Khác";
      const moduleLabel = group.label || group.module || moduleName;

      if (!groups[moduleName]) {
        groups[moduleName] = {
          moduleName,
          moduleLabel,
          permissions: [],
        };
      }
      if (Array.isArray(group.permissions)) {
        groups[moduleName].permissions.push(...group.permissions);
      }
    });
    return groups;
  }, [permissionGroups]);

  const isPermissionSelected = (permissionId) => {
    return rolePermissions.some((p) =>
      comparePermissionIds(getPermissionId(p), permissionId)
    );
  };

  const getModuleSelectedCount = (moduleName) => {
    const modulePerms = groupedPermissions[moduleName]?.permissions || [];
    return modulePerms.filter((p) => isPermissionSelected(getPermissionId(p)))
      .length;
  };

  return (
    <AdminLayout pageTitle="Quản lý quyền truy cập" breadcrumbs={breadcrumbs}>
      <div className="roles-container">
        <div className="roles-header">
          <div className="roles-header-left">
            <h1>Quản lý quyền truy cập</h1>
            <p>Quản lý vai trò và quyền truy cập của người dùng</p>
          </div>
        </div>

        <div className="roles-content">
          <div className="roles-controls">
            <div className="search-section">
              <div className="search-box">
                <FiSearch size={18} />
                <input
                  type="text"
                  placeholder="Tìm kiếm vai trò..."
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                />
              </div>
            </div>
            <div className="page-info">
              <span>
                Hiển thị {filteredRoles.length} / {total || 0} vai trò
              </span>
            </div>
          </div>

          <div className="roles-table-container">
            <table className="roles-table">
              <thead>
                <tr>
                  <th>Vai trò</th>
                  <th>Các quyền</th>
                  <th>Số quyền</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan="4" style={{ padding: "40px" }}>
                      <div className="loading-container">
                        <div className="loading-spinner"></div>
                        <p>Đang tải dữ liệu...</p>
                      </div>
                    </td>
                  </tr>
                ) : filteredRoles.length > 0 ? (
                  filteredRoles.map((role) => (
                    <tr key={getRoleId(role)}>
                      <td>
                        <div className="role-name-cell">
                          <FiShield size={18} className="role-icon" />
                          <span className="role-name">
                            {role.name || role.roleName || "-"}
                          </span>
                        </div>
                      </td>
                      <td>
                        <div className="role-permissions-cell">
                          {loadingPermissions[getRoleId(role)] ? (
                            <span className="loading-permissions">
                              Đang tải...
                            </span>
                          ) : (
                            (() => {
                              const roleId = getRoleId(role);
                              const permissions =
                                rolePermissionsMap[roleId] || [];
                              const permissionCount = permissions.length;

                              if (permissionCount === 0) {
                                return (
                                  <span className="no-permissions">
                                    Chưa có quyền nào
                                  </span>
                                );
                              }

                              // Hiển thị tối đa 3 quyền đầu tiên, còn lại hiển thị số lượng
                              const displayPermissions = permissions.slice(
                                0,
                                3
                              );
                              const remainingCount =
                                permissionCount - displayPermissions.length;

                              return (
                                <div className="permissions-list-inline">
                                  {displayPermissions.map((perm, index) => (
                                    <span
                                      key={getPermissionId(perm) || index}
                                      className="permission-tag"
                                    >
                                      {getPermissionLabel(perm)}
                                    </span>
                                  ))}
                                  {remainingCount > 0 && (
                                    <span className="permission-more">
                                      +{remainingCount} quyền khác
                                    </span>
                                  )}
                                </div>
                              );
                            })()
                          )}
                        </div>
                      </td>
                      <td>
                        <span className="permission-count">
                          {(() => {
                            const roleId = getRoleId(role);
                            const permissions = rolePermissionsMap[roleId];
                            if (permissions !== undefined) {
                              return permissions.length;
                            }
                            return (
                              role.permissionCount ??
                              role.permissions?.length ??
                              0
                            );
                          })()}
                        </span>
                      </td>
                      <td>
                        <button
                          className="action-button edit"
                          onClick={() => handleOpenEditModal(role)}
                          title="Chỉnh sửa quyền"
                        >
                          <FiEdit2 size={18} />
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td
                      colSpan="4"
                      style={{ textAlign: "center", padding: 40 }}
                    >
                      {searchDebounce
                        ? "Không tìm thấy vai trò nào"
                        : "Chưa có vai trò nào"}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {total > pageSize && (
            <div className="pagination-container">
              <Pagination
                current={page}
                pageSize={pageSize}
                total={total}
                onChange={(newPage, newPageSize) => {
                  setPage(newPage);
                  setPageSize(newPageSize);
                }}
                showSizeChanger
                showTotal={(total) => `Tổng ${total} vai trò`}
                pageSizeOptions={["5", "10", "20", "50"]}
              />
            </div>
          )}
        </div>

        {/* Edit Permissions Modal */}
        {isModalOpen && selectedRole && (
          <div className="modal-overlay" onClick={handleCloseModal}>
            <div className="roles-modal" onClick={(e) => e.stopPropagation()}>
              <div className="modal-header">
                <h2>
                  Chỉnh sửa quyền: {selectedRole.name || selectedRole.roleName}
                </h2>
                <button className="modal-close" onClick={handleCloseModal}>
                  <FiX size={20} />
                </button>
              </div>

              <div className="modal-body">
                {isDetailLoading ? (
                  <div className="loading-container">
                    <div className="loading-spinner"></div>
                    <p>Đang tải quyền...</p>
                  </div>
                ) : (
                  <div className="permissions-selector">
                    {Object.keys(groupedPermissions).length > 0 ? (
                      Object.entries(groupedPermissions).map(
                        ([moduleName, group]) => {
                          const isExpanded = expandedModules.has(moduleName);
                          const selectedCount =
                            getModuleSelectedCount(moduleName);
                          const totalCount = group.permissions.length;
                          const allSelected =
                            selectedCount === totalCount && totalCount > 0;

                          return (
                            <div key={moduleName} className="permission-module">
                              <div
                                className="module-header"
                                onClick={() => handleToggleModule(moduleName)}
                              >
                                <div className="module-header-left">
                                  {isExpanded ? (
                                    <FiChevronUp size={20} />
                                  ) : (
                                    <FiChevronDown size={20} />
                                  )}
                                  <span className="module-name">
                                    {group.moduleLabel || moduleName}
                                  </span>
                                  <span className="module-count">
                                    ({selectedCount}/{totalCount})
                                  </span>
                                </div>
                                <div className="module-actions">
                                  <button
                                    className="select-all-btn"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      toggleModulePermissions(
                                        moduleName,
                                        group.permissions
                                      );
                                    }}
                                    disabled={isSaving}
                                  >
                                    {allSelected
                                      ? "Bỏ chọn tất cả"
                                      : "Chọn tất cả"}
                                  </button>
                                  <button
                                    className="save-module-btn"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleUpdateModulePermissions(moduleName);
                                    }}
                                    disabled={isSaving}
                                    title="Lưu quyền module này"
                                  >
                                    Lưu
                                  </button>
                                </div>
                              </div>

                              {isExpanded && (
                                <div className="permissions-list">
                                  {group.permissions.length > 0 ? (
                                    group.permissions.map((permission) => {
                                      const permId =
                                        getPermissionId(permission);
                                      const isSelected =
                                        isPermissionSelected(permId);

                                      return (
                                        <div
                                          key={permId}
                                          className={`permission-item ${
                                            isSelected ? "selected" : ""
                                          }`}
                                          onClick={() =>
                                            togglePermission(permId)
                                          }
                                        >
                                          <div className="permission-checkbox">
                                            {isSelected ? (
                                              <FiCheck
                                                size={16}
                                                className="check-icon"
                                              />
                                            ) : (
                                              <div className="empty-checkbox" />
                                            )}
                                          </div>
                                          <div className="permission-info">
                                            <span className="permission-name">
                                              {getPermissionLabel(permission)}
                                            </span>
                                            {permission.description && (
                                              <span className="permission-desc">
                                                {permission.description}
                                              </span>
                                            )}
                                            {permission.key &&
                                              permission.key !==
                                                getPermissionLabel(
                                                  permission
                                                ) && (
                                                <span className="permission-key">
                                                  {permission.key}
                                                </span>
                                              )}
                                          </div>
                                        </div>
                                      );
                                    })
                                  ) : (
                                    <p className="empty-text">
                                      Không có quyền nào
                                    </p>
                                  )}
                                </div>
                              )}
                            </div>
                          );
                        }
                      )
                    ) : (
                      <div className="loading-container">
                        {permissionGroupsLoading ? (
                          <>
                            <div className="loading-spinner"></div>
                            <p>Đang tải nhóm quyền...</p>
                          </>
                        ) : (
                          <p className="empty-text">Không có nhóm quyền nào</p>
                        )}
                      </div>
                    )}
                  </div>
                )}
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
                  onClick={handleSavePermissions}
                  disabled={isSaving || isDetailLoading}
                >
                  {isSaving ? "Đang lưu..." : "Lưu thay đổi"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
};

export default RolesManagement;
