using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Controllers
{
    // Controller này chịu trách nhiệm hiển thị Nhật Ký Hoạt Động
    public class ActionLogsController(PawnShopDbContext context) : Controller
    {
        // 1. TRANG DANH SÁCH NHẬT KÝ
        // Đường dẫn chạy: /ActionLogs
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách log từ Database
            // OrderByDescending: Sắp xếp ngày mới nhất lên đầu
            var logs = await context.ActionLogs
                                    .OrderByDescending(x => x.Timestamp)
                                    .ToListAsync();

            return View(logs);
        }
    }
}