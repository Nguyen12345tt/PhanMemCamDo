using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Controllers
{
    public class SystemConfigsController(PawnShopDbContext context) : Controller
    {
        // 1. DANH SÁCH CẤU HÌNH
        public async Task<IActionResult> Index()
        {
            // Nếu chưa có cấu hình nào thì tự tạo mẫu (Seed Data thủ công)
            if (!context.SystemConfigs.Any())
            {
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "LaiSuatMacDinh", ConfigValue = "3", Description = "Lãi suất mặc định (%/tháng)" });
                context.SystemConfigs.Add(new SystemConfig { ConfigKey = "TenCuaHang", ConfigValue = "Cầm Đồ Số 1", Description = "Tên hiển thị trên web" });
                await context.SaveChangesAsync();
            }

            return View(await context.SystemConfigs.ToListAsync());
        }

        // 2. SỬA CẤU HÌNH (GET) - Hiện Form popup hoặc trang riêng đều được
        // Ở đây làm trang riêng cho dễ
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var config = await context.SystemConfigs.FindAsync(id);
            if (config == null) return NotFound();
            return View(config);
        }

        // 3. LƯU SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SystemConfig config)
        {
            if (id != config.Id) return NotFound();

            if (ModelState.IsValid)
            {
                context.Update(config);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(config);
        }
    }
}