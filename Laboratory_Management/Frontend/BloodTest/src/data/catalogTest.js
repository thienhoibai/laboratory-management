// Mock data for test catalogs (mục xét nghiệm) with parameters

// Available test parameters (from parameter.js)
export const availableParameters = [
  {
    id: 1,
    code: "WBC",
    name: "White Blood Cell",
    unit: "10^3/μL",
    normalRangeMin: 4.5,
    normalRangeMax: 11,
  },
  {
    id: 2,
    code: "RBC",
    name: "Red Blood Cell",
    unit: "10^6/μL",
    normalRangeMin: 4.5,
    normalRangeMax: 5.9,
  },
  {
    id: 3,
    code: "GLU",
    name: "Glucose",
    unit: "mg/dL",
    normalRangeMin: 70,
    normalRangeMax: 100,
  },
  {
    id: 4,
    code: "CHOL",
    name: "Cholesterol",
    unit: "mg/dL",
    normalRangeMin: 125,
    normalRangeMax: 200,
  },
  {
    id: 5,
    code: "HGB",
    name: "Hemoglobin",
    unit: "g/dL",
    normalRangeMin: 13.5,
    normalRangeMax: 17.5,
  },
  {
    id: 6,
    code: "PLT",
    name: "Platelet",
    unit: "10^3/μL",
    normalRangeMin: 150,
    normalRangeMax: 400,
  },
  {
    id: 7,
    code: "ALT",
    name: "Alanine Aminotransferase",
    unit: "U/L",
    normalRangeMin: 7,
    normalRangeMax: 56,
  },
  {
    id: 8,
    code: "AST",
    name: "Aspartate Aminotransferase",
    unit: "U/L",
    normalRangeMin: 10,
    normalRangeMax: 40,
  },
  {
    id: 9,
    code: "CREA",
    name: "Creatinine",
    unit: "mg/dL",
    normalRangeMin: 0.7,
    normalRangeMax: 1.3,
  },
  {
    id: 10,
    code: "UREA",
    name: "Urea",
    unit: "mg/dL",
    normalRangeMin: 15,
    normalRangeMax: 45,
  },
];

// Mock test catalogs
export const mockCatalogs = [
  {
    id: 1,
    name: "Xét nghiệm máu toàn bộ",
    category: "Máu",
    price: 150000,
    status: "Hoạt động",
    parameters: [
      { id: 1, code: "WBC", name: "White Blood Cell" },
      { id: 2, code: "RBC", name: "Red Blood Cell" },
      { id: 5, code: "HGB", name: "Hemoglobin" },
    ],
    description: "Xét nghiệm đầm bào toàn bộ bao gồm các chỉ số",
  },
  {
    id: 2,
    name: "Xét nghiệm chỉ số hô mô",
    category: "Hóa sinh",
    price: 120000,
    status: "Hoạt động",
    parameters: [
      { id: 3, code: "GLU", name: "Glucose" },
      { id: 4, code: "CHOL", name: "Cholesterol" },
    ],
    description: "Xét nghiệm đầm bào toàn bộ bao gồm các chỉ số",
  },
];

// Helper functions
export const getCatalogById = (id) => {
  return mockCatalogs.find((catalog) => catalog.id === id);
};

export const getAvailableParametersNotInCatalog = (catalogParameters) => {
  const catalogParameterIds = catalogParameters.map((p) => p.id);
  return availableParameters.filter(
    (param) => !catalogParameterIds.includes(param.id)
  );
};
