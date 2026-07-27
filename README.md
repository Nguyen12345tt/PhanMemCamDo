# 🏪 Phần Mềm Quản Lý Cầm Đồ - Cầm Đồ Xịn

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-10.0-blue?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC292B?style=for-the-badge&logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap)

Ứng dụng Web quản lý tiệm cầm đồ chuyên nghiệp, hiện đại và tối ưu quy trình vận hành. Được xây dựng trên nền tảng **ASP.NET Core MVC** kết hợp với **Entity Framework Core**, **Bootstrap 5** và **SweetAlert2**.

---

## 🔥 Các Chức Năng Nổi Bật

### 1. 📄 Quản Lý Hợp Đồng Cầm Đồ
* **Tự động sinh mã hợp đồng chuẩn**: Mã thứ tự ngắn gọn (`HD0001`, `HD0002`...) tự động gợi ý khi lập hợp đồng mới.
* **Tính toán tiền lãi tự động**: Hỗ trợ nhiều kiểu tính lãi (ngày/tuần/tháng) với công thức chính xác qua `PawnCalculator`.
* **Trạng thái hợp đồng đa dạng**: Đang hoạt động, Sắp đến hạn, Quá hạn cầm, Đã chuộc, Đã thanh lý.
* **Bộ lọc thông minh**: Lọc nhanh hợp đồng theo trạng thái (*Tất cả, Đang hoạt động, Quá hạn, Đã thanh lý, Đã chuộc*).

### 2. ⚖️ Quy Trình Thanh Lý Tài Sản Quá Hạn (Asset Liquidation)
* **Tính Lãi/Lỗ thời gian thực (Real-time JS)**: Tự động tính toán điểm hòa vốn (Gốc + Lãi dồn tích). Ngay khi nhập giá bán thực tế, hệ thống phản hồi kết quả Lãi thêm hoặc Thâm hụt tiền ngay lập tức.
* **Hạch toán sổ quỹ tự động**: Số tiền bán thanh lý tự động cộng trực tiếp vào **Sổ Quỹ Thu** (`CashFlows`).
* **Lưu nhật ký vết thao tác**: Ghi nhận lịch sử thanh lý vào nhật ký hệ thống (`ActionLogs`).

### 3. 📦 Kho Tài Sản Cầm Cố
* Phân loại tài sản theo danh mục (*Xe máy, Ô tô, Điện thoại, Laptop, Trang sức/Vàng, Đồng hồ, Giấy tờ...*).
* Quản lý thông tin chi tiết, tình trạng, mô tả tài sản cầm cố.

### 4. 👥 Quản Lý Khách Hàng
* Lưu trữ thông tin khách hàng (Họ tên, SĐT, CCCD/CMND, Địa chỉ).
* Tự động nhận diện khách quen khi lập hợp đồng cầm thêm.

### 5. 💰 Sổ Quỹ Thu / Chi (Cash Flows)
* Theo dõi tổng thu nhập (Tiền đóng lãi, tiền chuộc đồ, tiền bán thanh lý) và tổng chi tiêu (Cho vay/cầm đồ).
* Tổng quan số dư quỹ tiền mặt hiện có tại cửa hàng.

### 6. 📊 Báo Cáo & Nhật Ký Hệ Thống
* Thống kê tổng vốn đang cho vay, lãi dự kiến thu được.
* Nhật ký hành động ghi vết chi tiết các thao tác Thêm / Sửa / Xóa / Thanh lý của nhân viên.

---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

* **Backend**: .NET 10, ASP.NET Core MVC, C#
* **ORM & Database**: Entity Framework Core, SQL Server / SQL LocalDB
* **Frontend**: HTML5, CSS3, JavaScript (ES6+), Bootstrap 5, FontAwesome 6, SweetAlert2
* **Session & Auth**: ASP.NET Core Session Management

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Ứng Dụng

### 1. Yêu Cầu Hệ Thống
* [.NET 10 SDK](https://dotnet.microsoft.com/download) hoặc mới hơn
* SQL Server hoặc SQL Server Express / LocalDB (Đi kèm Visual Studio)

### 2. Tải Mã Nguồn
```bash
git clone https://github.com/Nguyen12345tt/PhanMemCamDo.git
cd PhanMemCamDo
```

### 3. Cấu Hình Chuỗi Kết Nối Database
File `appsettings.json` đã được cấu hình mặc định chạy với **SQL LocalDB**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PawnShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### 4. Chạy Ứng Dụng
Mở Terminal tại thư mục dự án và chạy câu lệnh:
```bash
dotnet run --launch-profile http
```

Sau khi ứng dụng khởi chạy thành công, mở trình duyệt và truy cập:
👉 **http://localhost:5135**

> 💡 **Ghi chú**: Database `PawnShopDB` cùng các bảng và dữ liệu mẫu sẽ được tự động tạo ngay lần đầu tiên ứng dụng khởi chạy (`EnsureCreated`).

---

## 📁 Cấu Trúc Dự Án

```
PhanMemCamDo/
├── Controllers/         # Các Controller xử lý logic (PawnContracts, Customers, Assets...)
│   └── Api/             # Các API Controller truy xuất dữ liệu
├── Data/                # DbContext & Seed Data (PawnShopDbContext.cs)
├── Models/              # Entities, Enums và ViewModels
│   ├── Entities/        # Bảng dữ liệu (PawnContract, Asset, Customer, CashFlow...)
│   ├── Enums/           # Enum trạng thái (ContractStatus, PaymentType, InterestType...)
│   └── ViewModels/      # Model dành riêng cho các View
├── Services/            # Service nghiệp vụ (PawnCalculator.cs - Tính lãi)
├── Views/               # Giao diện CSHTML (PawnContracts, Assets, Shared...)
└── wwwroot/             # File tĩnh CSS, JS, Bootstrap, JQuery
```

---

## 📜 Giấy Phép & Đóng Góp
Dự án được phát triển cho mục đích quản lý tiệm cầm đồ và học tập/nghiên cứu công nghệ ASP.NET Core MVC.

Mọi góp ý hoặc yêu cầu tính năng mới vui lòng tạo **Issue** hoặc **Pull Request** tại GitHub Repository!
