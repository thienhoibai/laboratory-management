import React, { useState } from "react";
import "./TestResultDetail.css";

export default function TestResultDetail({ test, onClose, inline = false }) {
  const [expandedSections, setExpandedSections] = useState({});

  // Mock test result data based on the images
  const testResults = {
    general: [
      {
        name: "Xét nghiệm tế bào máu",
        description: "Kiểm tra số lượng tế bào máu toàn phần",
        tests: [
          {
            name: "Hồng Cầu (RBC)",
            value: "4.8",
            unit: "T/dL",
            referenceRange: "4.5-5.5",
            status: "normal",
          },
          {
            name: "Bạch cầu",
            value: "7.2",
            unit: "G/L",
            referenceRange: "4-10",
            status: "normal",
          },
          {
            name: "Hemoglobin",
            value: "14.5",
            unit: "g/dL",
            referenceRange: "13.5-17.5",
            status: "normal",
          },
          {
            name: "Hematocrit",
            value: "42",
            unit: "%",
            referenceRange: "40-50",
            status: "normal",
          },
          {
            name: "Tiểu Cầu",
            value: "250",
            unit: "nghìn/µL",
            referenceRange: "150-400",
            status: "normal",
          },
          {
            name: "MCV",
            value: "88",
            unit: "fL",
            referenceRange: "80-100",
            status: "normal",
          },
          {
            name: "MCH",
            value: "30",
            unit: "pg",
            referenceRange: "27-33",
            status: "normal",
          },
          {
            name: "MCHC",
            value: "34",
            unit: "g/dL",
            referenceRange: "32-36",
            status: "normal",
          },
        ],
      },
      {
        name: "Xét nghiệm chức năng thận",
        description: "Kiểm tra chức năng hoạt động của thận",
        tests: [
          {
            name: "AST (SGOT)",
            value: "25",
            unit: "U/L",
            referenceRange: "<40",
            status: "normal",
          },
          {
            name: "ALT (SGPT)",
            value: "28",
            unit: "U/L",
            referenceRange: "<40",
            status: "normal",
          },
          {
            name: "Bilirubin toàn phần",
            value: "8.5",
            unit: "µmol/L",
            referenceRange: "3.4-20.5",
            status: "normal",
          },
          {
            name: "Albumin",
            value: "4.2",
            unit: "g/dL",
            referenceRange: "3.5-5.5",
            status: "normal",
          },
          {
            name: "Protein toàn phần",
            value: "7.5",
            unit: "g/dL",
            referenceRange: "6.0-8.3",
            status: "normal",
          },
        ],
      },
      {
        name: "Xét nghiệm lipid máu",
        description: "Kiểm tra các chỉ số mỡ máu",
        tests: [
          {
            name: "Cholesterol toàn phần",
            value: "180",
            unit: "mg/dL",
            referenceRange: "<200",
            status: "normal",
          },
          {
            name: "Triglycerid",
            value: "140",
            unit: "mg/dL",
            referenceRange: "<150",
            status: "normal",
          },
          {
            name: "HDL-C",
            value: "55",
            unit: "mg/dL",
            referenceRange: ">40",
            status: "normal",
          },
          {
            name: "LDL-C",
            value: "100",
            unit: "mg/dL",
            referenceRange: "<130",
            status: "normal",
          },
        ],
      },
      {
        name: "Xét nghiệm đường huyết",
        description: "Kiểm tra chỉ số đường trong máu",
        tests: [
          {
            name: "Glucose lúc đói",
            value: "95",
            unit: "mg/dL",
            referenceRange: "70-100",
            status: "normal",
          },
          {
            name: "HbA1c",
            value: "5.5",
            unit: "%",
            referenceRange: "<5.7",
            status: "normal",
          },
        ],
      },
      {
        name: "Xét nghiệm nước tiểu",
        description: "Phân tích thành phần nước tiểu",
        tests: [
          {
            name: "Màu sắc",
            value: "Vàng nhạt",
            unit: "",
            referenceRange: "Vàng nhạt",
            status: "normal",
          },
          {
            name: "Tỷ trọng",
            value: "1.015",
            unit: "",
            referenceRange: "1.010-1.030",
            status: "normal",
          },
          {
            name: "pH",
            value: "6.0",
            unit: "",
            referenceRange: "5.0-7.0",
            status: "normal",
          },
          {
            name: "Protein",
            value: "Âm tính",
            unit: "",
            referenceRange: "Âm tính",
            status: "normal",
          },
          {
            name: "Glucose",
            value: "Âm tính",
            unit: "",
            referenceRange: "Âm tính",
            status: "normal",
          },
          {
            name: "Bilirubin",
            value: "Âm tính",
            unit: "",
            referenceRange: "Âm tính",
            status: "normal",
          },
          {
            name: "Ketone",
            value: "Âm tính",
            unit: "",
            referenceRange: "Âm tính",
            status: "normal",
          },
        ],
      },
      {
        name: "Xét nghiệm chức năng giáp",
        description: "Kiểm tra hoạt động của tuyến giáp",
        tests: [
          {
            name: "TSH",
            value: "2.5",
            unit: "µIU/mL",
            referenceRange: "0.4-4.0",
            status: "normal",
          },
          {
            name: "T4",
            value: "1.2",
            unit: "ng/dL",
            referenceRange: "0.8-1.8",
            status: "normal",
          },
          {
            name: "T3",
            value: "1.1",
            unit: "ng/mL",
            referenceRange: "0.8-2.0",
            status: "normal",
          },
        ],
      },
    ],
  };

  const toggleSection = (index) => {
    setExpandedSections((prev) => ({
      ...prev,
      [index]: !prev[index],
    }));
  };

  const getStatusBadge = (status) => {
    if (status === "normal") {
      return <span className="status-badge normal">Bình thường</span>;
    } else if (status === "high") {
      return <span className="status-badge high">Cao</span>;
    } else if (status === "low") {
      return <span className="status-badge low">Thấp</span>;
    }
    return null;
  };

  return (
    <div className={`test-result-detail ${inline ? "inline-mode" : ""}`}>
      {/* Header - Only show if not inline */}
      {!inline && (
        <div className="test-result-header">
          <div className="breadcrumb">
            <button onClick={onClose} className="breadcrumb-link">
              Quay lại
            </button>
          </div>
          <h1 className="page-title">{test.title}</h1>
          <p className="page-subtitle">
            Ngày xét nghiệm: {test.date} • Khám tại: {test.location}
          </p>
        </div>
      )}

      {/* Test Results */}
      <div className="test-results-container">
        <div className="results-header">
          <h2 className="section-title">Kết quả xét nghiệm</h2>
        </div>

        {testResults.general.map((section, index) => (
          <div key={index} className="result-section">
            <button
              className="section-header"
              onClick={() => toggleSection(index)}
            >
              <div className="section-header-left">
                <div className="section-icon">
                  <svg
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                  >
                    <path d="M9 11l3 3L22 4" />
                    <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
                  </svg>
                </div>
                <div className="section-info">
                  <h3 className="section-name">{section.name}</h3>
                  <p className="section-description">{section.description}</p>
                </div>
              </div>
              <svg
                className={`expand-icon ${
                  expandedSections[index] ? "expanded" : ""
                }`}
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              >
                <polyline points="6 9 12 15 18 9" />
              </svg>
            </button>

            {expandedSections[index] && (
              <div className="section-content">
                <table className="results-table">
                  <thead>
                    <tr>
                      <th>Tên xét nghiệm</th>
                      <th>Kết quả</th>
                      <th>Đơn vị</th>
                      <th>Giá trị tham chiếu</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {section.tests.map((testItem, testIndex) => (
                      <tr key={testIndex}>
                        <td className="test-name">{testItem.name}</td>
                        <td className="test-value">{testItem.value}</td>
                        <td className="test-unit">{testItem.unit}</td>
                        <td className="test-reference">
                          {testItem.referenceRange}
                        </td>
                        <td className="test-status">
                          {getStatusBadge(testItem.status)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Footer Note */}
      <div className="result-footer">
        <div className="footer-note">
          <svg
            className="note-icon"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="16" x2="12" y2="12" />
            <line x1="12" y1="8" x2="12.01" y2="8" />
          </svg>
          <p>
            Lưu ý: Kết quả xét nghiệm chỉ mang tính chất tham khảo. Vui lòng
            tham khảo ý kiến bác sĩ để được tư vấn và điều trị phù hợp.
          </p>
        </div>
      </div>
    </div>
  );
}
