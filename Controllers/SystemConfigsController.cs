using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Models.ViewModels;
using System.Globalization;

namespace PhanMemCamDo.Controllers
{
    public class SystemConfigsController(PawnShopDbContext context) : Controller
    {
        // GET: /SystemConfigs
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "⛔ Bạn không có quyền truy cập vào Cấu Hình Tiệm. Tính năng này chỉ dành cho Quản Lý (Admin)!";
                return RedirectToAction("Index", "Home");
            }

            await EnsureDefaultConfigsExist();

            var configs = await context.SystemConfigs.ToListAsync();

            var vm = new ShopConfigViewModel
            {
                TenCuaHang = GetConfigValue(configs, "TenCuaHang", "Cầm Đồ Xịn"),
                SoDienThoai = GetConfigValue(configs, "SoDienThoai", "0976543123"),
                DiaChi = GetConfigValue(configs, "DiaChi", "123 Nguyễn Trãi, Thanh Xuân, Hà Nội"),
                LaiSuatMacDinh = parseDecimal(GetConfigValue(configs, "LaiSuatMacDinh", "3"), 3.0m),
                HeSoPhatQuaHan = parseDecimal(GetConfigValue(configs, "HeSoPhatQuaHan", "1.5"), 1.5m),
                ThoiHanCanhBao = parseInt(GetConfigValue(configs, "ThoiHanCanhBao", "3"), 3)
            };

            HttpContext.Session.SetString("StoreName", vm.TenCuaHang);
            return View(vm);
        }

        // POST: /SystemConfigs/SaveConfig
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveConfig(ShopConfigViewModel model)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "⛔ Bạn không có quyền thực hiện thao tác này!";
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            await SaveOrUpdateKey("TenCuaHang", model.TenCuaHang, "Tên hiển thị cửa hàng / tiệm cầm đồ");
            await SaveOrUpdateKey("SoDienThoai", model.SoDienThoai, "Số điện thoại hotline liên hệ");
            await SaveOrUpdateKey("DiaChi", model.DiaChi, "Địa chỉ cửa hàng");
            await SaveOrUpdateKey("LaiSuatMacDinh", model.LaiSuatMacDinh.ToString(CultureInfo.InvariantCulture), "Lãi suất mặc định gợi ý khi tạo HĐ (%/tháng)");
            await SaveOrUpdateKey("HeSoPhatQuaHan", model.HeSoPhatQuaHan.ToString(CultureInfo.InvariantCulture), "Hệ số phạt trễ hạn (VD: 1.5)");
            await SaveOrUpdateKey("ThoiHanCanhBao", model.ThoiHanCanhBao.ToString(), "Số ngày cảnh báo đếm ngược hợp đồng sắp hết hạn");

            // Lưu nhật ký thao tác
            var username = HttpContext.Session.GetString("Username") ?? "Hệ thống";
            context.ActionLogs.Add(new ActionLog
            {
                ActionName = "CẤU HÌNH TIỆM",
                EntityName = "SystemConfig",
                Description = $"{username} đã cập nhật cấu hình tiệm cầm đồ '{model.TenCuaHang}'",
                Timestamp = DateTime.Now,
                UserName = username
            });

            await context.SaveChangesAsync();

            // Cập nhật Session ngay lập tức cho Layout Navbar
            HttpContext.Session.SetString("StoreName", model.TenCuaHang);

            TempData["SuccessMessage"] = "🎉 Cập nhật cấu hình cửa hàng thành công! Tất cả thiết lập mới đã được áp dụng.";
            return RedirectToAction(nameof(Index));
        }

        private async Task EnsureDefaultConfigsExist()
        {
            bool hasChanges = false;

            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == "TenCuaHang"))
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "TenCuaHang", ConfigValue = "Cầm Đồ Xịn", Description = "Tên hiển thị cửa hàng / tiệm cầm đồ" });
                hasChanges = true;
            }
            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == "SoDienThoai"))
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "SoDienThoai", ConfigValue = "0976543123", Description = "Số điện thoại hotline liên hệ" });
                hasChanges = true;
            }
            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == "DiaChi"))
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "DiaChi", ConfigValue = "123 Nguyễn Trãi, Thanh Xuân, Hà Nội", Description = "Địa chỉ cửa hàng" });
                hasChanges = true;
            }
            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == "LaiSuatMacDinh"))
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "LaiSuatMacDinh", ConfigValue = "3", Description = "Lãi suất mặc định gợi ý khi tạo HĐ (%/tháng)" });
                hasChanges = true;
            }
            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == "HeSoPhatQuaHan"))
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "HeSoPhatQuaHan", ConfigValue = "1.5", Description = "Hệ số phạt trễ hạn (VD: 1.5)" });
                hasChanges = true;
            }
            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == "ThoiHanCanhBao"))
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "ThoiHanCanhBao", ConfigValue = "3", Description = "Số ngày cảnh báo đếm ngược hợp đồng sắp hết hạn" });
                hasChanges = true;
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
            }
        }

        private async Task SaveOrUpdateKey(string key, string value, string description)
        {
            var config = await context.SystemConfigs.FirstOrDefaultAsync(x => x.ConfigKey == key);
            if (config != null)
            {
                config.ConfigValue = value;
                config.Description = description;
            }
            else
            {
                context.SystemConfigs.Add(new SystemConfig
                {
                    ConfigKey = key,
                    ConfigValue = value,
                    Description = description
                });
            }
        }

        private static string GetConfigValue(List<SystemConfig> list, string key, string defaultValue)
        {
            var item = list.FirstOrDefault(x => x.ConfigKey == key);
            return !string.IsNullOrEmpty(item?.ConfigValue) ? item.ConfigValue : defaultValue;
        }

        private static decimal parseDecimal(string input, decimal defaultValue)
        {
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        private static int parseInt(string input, int defaultValue)
        {
            return int.TryParse(input, out var result) ? result : defaultValue;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }
    }
}