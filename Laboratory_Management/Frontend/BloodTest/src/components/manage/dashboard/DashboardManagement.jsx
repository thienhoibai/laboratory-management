import React from "react";
import AdminLayout from "../../admin/layout/AdminLayout";
import { Card, Col, Row } from "antd";
import { FiPackage, FiCalendar, FiUsers, FiTrendingUp } from "react-icons/fi";
import "./DashboardManagement.css";

const dashboardData = {
  kpis: [
    {
      title: "Tổng số gói xét nghiệm",
      value: 24,
      change: "+3 gói mới",
      icon: <FiPackage />,
      color: "#3b82f6",
    },
    {
      title: "Lịch hẹn hôm nay",
      value: 48,
      change: "+12% so với hôm qua",
      icon: <FiCalendar />,
      color: "#10b981",
    },
    {
      title: "Khách hàng mới",
      value: 156,
      change: "+23 tuần này",
      icon: <FiUsers />,
      color: "#8b5cf6",
    },
    {
      title: "Doanh thu tháng này",
      value: "₫45.2M",
      change: "+18% so với tháng trước",
      icon: <FiTrendingUp />,
      color: "#f59e0b",
    },
  ],
  weeklyAppointments: [
    { day: "T2", value: 32 },
    { day: "T3", value: 45 },
    { day: "T4", value: 38 },
    { day: "T5", value: 52 },
    { day: "T6", value: 48 },
    { day: "T7", value: 35 },
    { day: "CN", value: 28 },
  ],
  monthlyRevenue: [
    { month: "11", value: 32 },
    { month: "12", value: 35 },
    { month: "13", value: 33 },
    { month: "14", value: 40 },
    { month: "15", value: 43 },
    { month: "16", value: 46 },
  ],
  popularPackages: [
    { name: "Xét nghiệm tổng quát", bookings: 145, change: "+12%" },
    { name: "Xét nghiệm sinh hóa", bookings: 98, change: "+8%" },
    { name: "Xét nghiệm toàn diện", bookings: 76, change: "+15%" },
    { name: "Xét nghiệm hormone", bookings: 54, change: "+5%" },
  ],
  inventoryAlerts: [
    { item: "Ống nghiệm máu", current: 45, minimum: 100, level: "urgent" },
    { item: "Kim tiêm", current: 120, minimum: 200, level: "warning" },
    { item: "Găng tay y tế", current: 180, minimum: 300, level: "warning" },
  ],
};

const DashboardManagement = () => {
  const breadcrumbs = [
    { name: "Laboratory", link: "#" },
    { name: "Tổng quan" },
  ];

  const {
    kpis,
    weeklyAppointments,
    monthlyRevenue,
    popularPackages,
    inventoryAlerts,
  } = dashboardData;

  const maxAppointment = Math.max(...weeklyAppointments.map((a) => a.value));
  const maxRevenue = Math.max(...monthlyRevenue.map((r) => r.value));

  return (
    <AdminLayout pageTitle="Tổng quan" breadcrumbs={breadcrumbs}>
      <div className="dashboard-management-content">
        <div className="dashboard-header">
          <h1 className="dashboard-title">Tổng quan</h1>
          <p className="dashboard-subtitle">
            Theo dõi hoạt động và hiệu suất của trung tâm xét nghiệm
          </p>
        </div>

        {/* KPI Cards */}
        <div className="dashboard-kpi-cards">
          {kpis.map((kpi, index) => (
            <Card key={index} className="dashboard-kpi-card">
              <div className="dashboard-kpi-icon" style={{ color: kpi.color }}>
                {kpi.icon}
              </div>
              <div className="dashboard-kpi-content">
                <div className="dashboard-kpi-title">{kpi.title}</div>
                <div className="dashboard-kpi-value">{kpi.value}</div>
                <div className="dashboard-kpi-change">{kpi.change}</div>
              </div>
            </Card>
          ))}
        </div>

        {/* Charts Section */}
        <div className="dashboard-charts-section">
          <Card className="dashboard-chart-card">
            <h3 className="dashboard-chart-title">Lịch hẹn trong tuần</h3>
            <div className="dashboard-bar-chart">
              <div className="chart-y-axis">
                {[60, 45, 30, 15, 0].map((val) => (
                  <div key={val} className="y-axis-label">
                    {val}
                  </div>
                ))}
              </div>
              <div className="chart-content">
                <div className="chart-bars">
                  {weeklyAppointments.map((appointment, index) => {
                    const height = (appointment.value / maxAppointment) * 100;
                    return (
                      <div key={index} className="chart-bar-container">
                        <div
                          className="chart-bar"
                          style={{ height: `${height}%` }}
                          title={`${appointment.day}: ${appointment.value}`}
                        >
                          <span className="chart-bar-value">
                            {appointment.value}
                          </span>
                        </div>
                        <div className="chart-x-axis-label">
                          {appointment.day}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          </Card>

          <Card className="dashboard-chart-card">
            <h3 className="dashboard-chart-title">Doanh thu 6 tháng gần đây</h3>
            <div className="dashboard-line-chart">
              <div className="chart-y-axis">
                {[60, 45, 30, 15, 0].map((val) => (
                  <div key={val} className="y-axis-label">
                    {val}
                  </div>
                ))}
              </div>
              <div className="chart-content">
                <svg
                  className="line-chart-svg"
                  viewBox="0 0 300 200"
                  preserveAspectRatio="none"
                >
                  <polyline
                    fill="none"
                    stroke="#3b82f6"
                    strokeWidth="3"
                    points={monthlyRevenue
                      .map(
                        (r, i) =>
                          `${(i * 300) / (monthlyRevenue.length - 1)},${
                            200 - (r.value / maxRevenue) * 200
                          }`
                      )
                      .join(" ")}
                  />
                  {monthlyRevenue.map((r, i) => {
                    const x = (i * 300) / (monthlyRevenue.length - 1);
                    const y = 200 - (r.value / maxRevenue) * 200;
                    return (
                      <circle
                        key={i}
                        cx={x}
                        cy={y}
                        r="4"
                        fill="#3b82f6"
                        className="chart-point"
                      />
                    );
                  })}
                </svg>
                <div className="chart-x-axis-labels">
                  {monthlyRevenue.map((r, index) => (
                    <div key={index} className="x-axis-label">
                      {r.month}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </Card>
        </div>

        {/* Bottom Lists Section */}
        <div className="dashboard-lists-section">
          <Card className="dashboard-list-card">
            <div className="dashboard-list-header">
              <div
                className="dashboard-list-icon"
                style={{ backgroundColor: "#3b82f6" }}
              >
                <FiPackage />
              </div>
              <h3 className="dashboard-list-title">Gói xét nghiệm phổ biến</h3>
            </div>
            <div className="dashboard-list-items">
              {popularPackages.map((pkg, index) => (
                <div key={index} className="dashboard-list-item">
                  <div className="list-item-content">
                    <div className="list-item-name">{pkg.name}</div>
                    <div className="list-item-details">
                      {pkg.bookings} lượt đặt
                    </div>
                  </div>
                  <div className="list-item-change positive">{pkg.change}</div>
                </div>
              ))}
            </div>
          </Card>

          <Card className="dashboard-list-card">
            <div className="dashboard-list-header">
              <div
                className="dashboard-list-icon alert"
                style={{ backgroundColor: "#ef4444" }}
              >
                <FiTrendingUp />
              </div>
              <h3 className="dashboard-list-title">Cảnh báo tồn kho</h3>
            </div>
            <div className="dashboard-list-items">
              {inventoryAlerts.map((alert, index) => (
                <div key={index} className="dashboard-list-item">
                  <div className="list-item-content">
                    <div className="list-item-name">{alert.item}</div>
                    <div className="list-item-details">
                      Còn {alert.current} / Tối thiểu {alert.minimum}
                    </div>
                  </div>
                  <div
                    className={`list-item-alert ${
                      alert.level === "urgent" ? "urgent" : "warning"
                    }`}
                  >
                    {alert.level === "urgent" ? "Nguy cấp" : "Cảnh báo"}
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </AdminLayout>
  );
};

export default DashboardManagement;
