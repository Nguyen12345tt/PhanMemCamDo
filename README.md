# 🏪 Phần Mềm Quản Lý Cầm Đồ - Cầm Đồ Xịn

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-10.0.10-blue?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC292B?style=for-the-badge&logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap)

Ứng dụng Web quản lý tiệm cầm đồ chuyên nghiệp, hiện đại và tối ưu quy trình vận hành. Được xây dựng trên nền tảng **ASP.NET Core MVC** kết hợp với **Entity Framework Core 10**, **C# 12 Primary Constructors**, **Source-Generated Regex** và hệ thống **Thông báo thời gian thực 🔔**.

---

## 🚀 Hướng Dẫn Chạy Ứng Dụng Từ A đến Z

Dưới đây là các bước chi tiết để cài đặt và khởi chạy dự án từ đầu:

### 📋 Bước 1: Chuẩn Bị Môi Trường
Trước khi bắt đầu, hãy chắc chắn rằng máy tính của bạn đã cài đặt:
1. **.NET 10 SDK** (hoặc mới hơn): Kiểm tra bằng lệnh `dotnet --version` trong Terminal.
2. **SQL Server** hoặc **SQL Server Express / LocalDB** (Đi kèm mặc định khi cài Visual Studio).
3. **Git**: Kiểm tra bằng lệnh `git --version`.

---

### 📥 Bước 2: Tải Mã Nguồn Về Máy
Mở Terminal / PowerShell và chạy lệnh clone repository từ GitHub:
```bash
git clone https://github.com/Nguyen12345tt/PhanMemCamDo.git
```

Di chuyển vào thư mục của dự án:
```bash
cd PhanMemCamDo
```

---

### ⚙️ Bước 3: Cấu Hình Chuỗi Kết Nối CSDL (Database)
Ứng dụng được cấu hình đa môi trường linh hoạt cho **mọi máy tính**:

1. **Khi mang dự án sang máy tính khác (Dùng SQL LocalDB mặc định)**:
   File `appsettings.json` đã được cấu hình mặc định sử dụng **SQL LocalDB** (có sẵn trên mọi máy Windows có Visual Studio / .NET SDK, chạy được ngay mà không cần cài đặt hay chỉnh sửa thêm):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PawnShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

2. **Khi chạy trên máy có cài đặt SQL Server riêng (như `.\MSSQLSERVER01` hay `.\SQLEXPRESS`)**:
   Bạn cấu hình chuỗi kết nối riêng trong file `appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.\\MSSQLSERVER01;Database=PawnShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```
   *(Thay `.\\MSSQLSERVER01` thành tên Server Instance hiển thị trong phần Object Explorer trên máy của bạn).*

---

### 📦 Bước 4: Khôi Phục Các Gói Thư Viện (Restore Packages)
Chạy lệnh khôi phục toàn bộ các thư viện NuGet cần thiết cho dự án:
```bash
dotnet restore
```

---

### 🗄️ Bước 5: Khởi Tạo / Cập Nhật Database (Database Update)
Hệ thống hỗ trợ 2 cách khởi tạo CSDL:

* **Cách 1 (Mặc định - Tự Động)**: Hệ thống sử dụng `context.Database.EnsureCreated()` trong `DbInitializer.cs`. CSDL `PawnShopDB` cùng toàn bộ các bảng và dữ liệu mẫu sẽ **tự động được tạo ngay lần đầu khởi chạy** mà không bắt buộc phải gõ lệnh migration.
* **Cách 2 (Thủ Công qua EF Core CLI)**: Nếu bạn muốn tự cập nhật CSDL bằng Migration:
  ```bash
  dotnet ef database update
  ```
  *(Lưu ý: Nếu máy bạn chưa cài đặt công cụ EF Core CLI, chạy lệnh `dotnet tool install --global dotnet-ef` để cài đặt).*

---

### ▶️ Bước 6: Khởi Chạy Ứng Dụng (Run Project)
Chạy lệnh khởi chạy ứng dụng:
```bash
dotnet run
```

---

### 🌐 Bước 7: Truy Cập Ứng Dụng
Mở trình duyệt Web (Chrome, Edge, Firefox...) và truy cập vào đường dẫn:
👉 **[http://localhost:5135](http://localhost:5135)**

---

## 📖 Hướng Dẫn Sử Dụng Chức Năng Cốt Lõi

### 1. 📄 Lập Hợp Đồng Cầm Đồ Mới
- Điều hướng tới **Hợp Đồng** ➡️ **Tạo Hợp Đồng Mới**.
- **Mã hợp đồng**: Tự động gợi ý mã tiếp theo (`HD0001`, `HD0002`...).
- **Quy đổi tiền thông minh**: Gõ số tiền ngắn gọn `< 1000` (Ví dụ: gõ `512` ➡️ tự động chuyển thành `512.000 đ`, gõ `5` ➡️ tự động chuyển thành `5.000.000 đ`).
- **Hình thức lãi**: Chọn Lãi theo **Ngày**, **Tuần** hoặc **Tháng**. Hệ thống tự động tính lãi chính xác qua `PawnCalculator`.

### 2. 🔔 Hệ Thống Thông Báo Thời Gian Thực
- Biểu tượng **Chuông 🔔** trên thanh Menu hiển thị số lượng hợp đồng **Sắp đến hạn (trong 3 ngày)** và **Quá hạn**.
- Click vào dòng thông báo ➡️ Tự động đánh dấu **Đã đọc** và mở ngay trang **Chi tiết hợp đồng** đó.

### 3. 💵 Đóng Lãi & Chuộc Đồ
- Truy cập chi tiết hợp đồng ➡️ Bấm **Đóng Lãi** hoặc **Chuộc Đồ**.
- Số tiền thu được sẽ tự động hạch toán trực tiếp vào **Sổ Quỹ Thu** (`CashFlows`).

### 4. ⚖️ Thanh Lý Tài Sản Quá Hạn
- Đối với hợp đồng quá hạn, bấm **Thanh Lý Tài Sản**.
- **Tính Lãi/Lỗ thời gian thực**: Ngay khi nhập giá bán thực tế, giao diện JS tự động phản hồi khoản Lãi thêm hoặc Thâm hụt tiền so với điểm hòa vốn (Gốc + Lãi dồn tích).
- Lưu lại lịch sử thao tác vào **Nhật ký hệ thống** (`ActionLogs`).

---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

* **Core Backend**: .NET 10, ASP.NET Core MVC, C# 12 (Primary Constructors, GeneratedRegex)
* **ORM & Database**: Entity Framework Core 10.0.10, SQL Server / SQL LocalDB
* **Frontend**: HTML5, Vanilla CSS, JavaScript (ES6+), Bootstrap 5.3, FontAwesome 6, SweetAlert2
* **Package Upgrade**: Swashbuckle 10.2.3, EF Core 10.0.10

---

## 📁 Cấu Trúc Dự Án

```
PhanMemCamDo/
├── Controllers/         # Các Controller xử lý logic giao diện (PawnContracts, Customers...)
│   └── Api/             # Các API Controller (NotificationsApi, ActionLogsApi...)
├── Data/                # DbContext & Seed Data (PawnShopDbContext.cs)
├── Models/              # Entities, Enums và ViewModels
│   ├── Entities/        # Entity CSDL (PawnContract, Asset, Customer, Notification...)
│   ├── Enums/           # Enum trạng thái (ContractStatus, PaymentType, InterestType...)
│   └── ViewModels/      # ViewModels hỗ trợ giao diện
├── Services/            # Service nghiệp vụ (PawnCalculator.cs, NotificationRegex.cs)
├── Views/               # Giao diện CSHTML (PawnContracts, Assets, Shared...)
└── wwwroot/             # Tài nguyên tĩnh CSS, JS, Bootstrap, JQuery
```

---

## 📜 Giấy Phép & Đóng Góp
Dự án được phát triển cho mục đích quản lý tiệm cầm đồ và nghiên cứu/học tập công nghệ ASP.NET Core.

Mọi góp ý hoặc yêu cầu tính năng mới vui lòng tạo **Issue** hoặc **Pull Request** tại GitHub Repository:
👉 **[https://github.com/Nguyen12345tt/PhanMemCamDo](https://github.com/Nguyen12345tt/PhanMemCamDo)**
