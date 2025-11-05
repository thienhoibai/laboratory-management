// src/data/package.js

// Mock data cho danh sách các xét nghiệm có thể chọn
export const availableTests = [
  {
    id: 1,
    name: "Xét nghiệm máu tổng quát",
    category: "Huyết học",
    price: 50000,
  },
  {
    id: 2,
    name: "Xét nghiệm đường huyết",
    category: "Sinh hóa",
    price: 30000,
  },
  {
    id: 3,
    name: "Xét nghiệm chức năng gan",
    category: "Sinh hóa",
    price: 120000,
  },
  {
    id: 4,
    name: "Xét nghiệm chức năng thận",
    category: "Sinh hóa",
    price: 100000,
  },
  {
    id: 5,
    name: "Xét nghiệm lipid máu",
    category: "Sinh hóa",
    price: 80000,
  },
  {
    id: 6,
    name: "Xét nghiệm nước tiểu",
    category: "Vi sinh",
    price: 40000,
  },
  {
    id: 7,
    name: "Điện giải đồ",
    category: "Sinh hóa",
    price: 70000,
  },
  {
    id: 8,
    name: "Xét nghiệm HbA1c",
    category: "Sinh hóa",
    price: 90000,
  },
  {
    id: 9,
    name: "Xét nghiệm TSH",
    category: "Nội tiết",
    price: 85000,
  },
  {
    id: 10,
    name: "Xét nghiệm PSA",
    category: "Ung thư",
    price: 95000,
  },
];

// Mock data cho gói xét nghiệm
export const mockPackages = [
  {
    id: 1,
    name: "Gói xét nghiệm tổng quát",
    price: 450000,
    description: "Gói xét nghiệm toàn diện cho sức khỏe tổng quát",
    testCount: 12,
    status: "active",
    tests: [
      { id: 1, name: "Xét nghiệm máu tổng quát", category: "Huyết học", price: 50000 },
      { id: 2, name: "Xét nghiệm đường huyết", category: "Sinh hóa", price: 30000 },
      { id: 3, name: "Xét nghiệm chức năng gan", category: "Sinh hóa", price: 120000 },
      { id: 4, name: "Xét nghiệm chức năng thận", category: "Sinh hóa", price: 100000 },
      { id: 5, name: "Xét nghiệm lipid máu", category: "Sinh hóa", price: 80000 },
      { id: 6, name: "Xét nghiệm nước tiểu", category: "Vi sinh", price: 40000 },
    ],
    createdAt: "2025-01-15",
  },
  {
    id: 2,
    name: "Gói xét nghiệm tiêu chuẩn",
    price: 650000,
    description: "Gói xét nghiệm cơ bản với các chỉ số quan trọng",
    testCount: 18,
    status: "active",
    tests: [
      { id: 1, name: "Xét nghiệm máu tổng quát", category: "Huyết học", price: 50000 },
      { id: 3, name: "Xét nghiệm chức năng gan", category: "Sinh hóa", price: 120000 },
      { id: 4, name: "Xét nghiệm chức năng thận", category: "Sinh hóa", price: 100000 },
      { id: 5, name: "Xét nghiệm lipid máu", category: "Sinh hóa", price: 80000 },
      { id: 8, name: "Xét nghiệm HbA1c", category: "Sinh hóa", price: 90000 },
    ],
    createdAt: "2025-02-20",
  },
  {
    id: 3,
    name: "Gói xét nghiệm cao cấp",
    price: 1200000,
    description: "Gói xét nghiệm toàn diện nhất với nhiều xét nghiệm chuyên sâu",
    testCount: 25,
    status: "active",
    tests: [
      { id: 1, name: "Xét nghiệm máu tổng quát", category: "Huyết học", price: 50000 },
      { id: 2, name: "Xét nghiệm đường huyết", category: "Sinh hóa", price: 30000 },
      { id: 3, name: "Xét nghiệm chức năng gan", category: "Sinh hóa", price: 120000 },
      { id: 4, name: "Xét nghiệm chức năng thận", category: "Sinh hóa", price: 100000 },
      { id: 5, name: "Xét nghiệm lipid máu", category: "Sinh hóa", price: 80000 },
      { id: 7, name: "Điện giải đồ", category: "Sinh hóa", price: 70000 },
      { id: 8, name: "Xét nghiệm HbA1c", category: "Sinh hóa", price: 90000 },
      { id: 9, name: "Xét nghiệm TSH", category: "Nội tiết", price: 85000 },
    ],
    createdAt: "2025-03-10",
  },
  {
    id: 4,
    name: "Gói xét nghiệm tim mạch",
    price: 850000,
    description: "Gói xét nghiệm chuyên sâu về tim mạch và mạch máu",
    testCount: 15,
    status: "inactive",
    tests: [
      { id: 1, name: "Xét nghiệm máu tổng quát", category: "Huyết học", price: 50000 },
      { id: 5, name: "Xét nghiệm lipid máu", category: "Sinh hóa", price: 80000 },
      { id: 7, name: "Điện giải đồ", category: "Sinh hóa", price: 70000 },
    ],
    createdAt: "2025-04-05",
  },
];

// Hàm helper để lấy package theo ID
export const getPackageById = (id) => {
  return mockPackages.find((pkg) => pkg.id === id);
};

// Hàm helper để tính tổng giá trị thực của các xét nghiệm trong gói
export const calculateTotalTestPrice = (tests) => {
  return tests.reduce((total, test) => total + test.price, 0);
};
