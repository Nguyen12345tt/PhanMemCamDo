using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Data
{
    public class PawnShopDbContext(DbContextOptions<PawnShopDbContext> options) : DbContext(options)
    {
        // --- NHÓM 1: CORE ---
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<PawnContract> PawnContracts { get; set; }

        // --- NHÓM 2: TIỀN NONG ---
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<CashFlow> CashFlows { get; set; }

        // --- NHÓM 3: QUẢN TRỊ ---
        public DbSet<User> Users { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }

        // --- NHÓM 4: TIỆN ÍCH (AssetCategories thay cho AssetImages) ---
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình kiểu số tiền (Decimal)
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18, 0)"); // Lưu số nguyên (VND)
            }

            // 2. SEED DATA: Tài khoản Admin
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Password = "123456", // Chưa mã hóa (for demo only)
                FullName = "Quản Trị Viên",
                Role = UserRole.Admin,
                Email = "admin@example.com",
                PhoneNumber = "0123456789"
            });

            // 3. SEED DATA: Khách hàng mẫu
            modelBuilder.Entity<Customer>().HasData(new Customer
            {
                Id = 1,
                FullName = "Khách Hàng Test",
                IdentityCard = "001234567890",
                PhoneNumber = "0909000111",
                Address = "Hà Nội"
            });


            // 4. SEED DATA: Danh mục tài sản (QUAN TRỌNG: Cài lại Win vẫn còn)
            modelBuilder.Entity<AssetCategory>().HasData(
                new AssetCategory { Id = 1, Name = "Xe Máy" },
                new AssetCategory { Id = 2, Name = "Ô Tô" },
                new AssetCategory { Id = 3, Name = "Điện Thoại" },
                new AssetCategory { Id = 4, Name = "Máy Tính / Laptop" },
                new AssetCategory { Id = 5, Name = "Trang Sức / Vàng" },
                new AssetCategory { Id = 6, Name = "Đồng Hồ" },
                new AssetCategory { Id = 7, Name = "Giấy Tờ Xe / Nhà Đất" },
                new AssetCategory { Id = 8, Name = "Khác" }
            );

            // 5. QUAN HỆ RÀNG BUỘC (Để an toàn dữ liệu)
            modelBuilder.Entity<PawnContract>()
                .HasOne(c => c.Customer).WithMany(c => c.PawnContracts)
                .HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PawnContract>()
                .HasOne(c => c.Asset).WithMany(c => c.PawnContracts)
                .HasForeignKey(c => c.AssetId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}