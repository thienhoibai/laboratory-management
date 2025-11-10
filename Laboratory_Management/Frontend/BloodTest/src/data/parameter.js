// src/data/parameter.js

// Mock data cho chỉ số xét nghiệm
export const mockParameters = [
  {
    id: 1,
    code: "WBC",
    name: "White Blood Cell",
    normalRangeMin: 4.5,
    normalRangeMax: 11,
    unit: "10^3/μL",
    description: "Số lượng tế bào máu trắng",
    createdAt: "2025-01-15",
  },
  {
    id: 2,
    code: "RBC",
    name: "Red Blood Cell",
    normalRangeMin: 4.5,
    normalRangeMax: 5.9,
    unit: "10^6/μL",
    description: "Số lượng tế bào máu đỏ",
    createdAt: "2025-01-15",
  },
  {
    id: 3,
    code: "GLU",
    name: "Glucose",
    normalRangeMin: 70,
    normalRangeMax: 100,
    unit: "mg/dL",
    description: "Nồng độ đường huyết",
    createdAt: "2025-01-16",
  },
  {
    id: 4,
    code: "CHOL",
    name: "Cholesterol",
    normalRangeMin: 0,
    normalRangeMax: 200,
    unit: "mg/dL",
    description: "Cholesterol toàn phần",
    createdAt: "2025-01-16",
  },
  {
    id: 5,
    code: "HGB",
    name: "Hemoglobin",
    normalRangeMin: 12,
    normalRangeMax: 16,
    unit: "g/dL",
    description: "Nồng độ hemoglobin",
    createdAt: "2025-01-17",
  },
];

// Hàm helper để lấy parameter theo ID
export const getParameterById = (id) => {
  return mockParameters.find((param) => param.id === id);
};

// Hàm helper để lấy parameter theo code
export const getParameterByCode = (code) => {
  return mockParameters.find((param) => param.code === code);
};
