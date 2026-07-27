using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Controllers
{
    public class ReportsController(PawnShopDbContext context) : Controller
    {
        // Trang báo cáo tổng quan theo danh mục
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả hợp đồng ĐANG HOẠT ĐỘNG (Active)
            var data = await context.PawnContracts
                .Include(c => c.Asset)
                .ThenInclude(a => a!.AssetCategory)
                .Where(c => c.Status == ContractStatus.Active) // Chỉ tính cái đang cầm
                .ToListAsync();

            // Thực hiện gom nhóm (Group By) theo Tên Loại
            var baoCao = data
                .GroupBy(c => c.Asset?.AssetCategory?.Name ?? "Khác") // Nếu null thì gom vào "Khác"
                .Select(g => new BaoCaoViewModel
                {
                    TenLoai = g.Key,
                    SoLuong = g.Count(),
                    TongTien = g.Sum(c => c.PawnAmount),
                    TiLeVon = 0 // Tí tính sau
                })
                .OrderByDescending(x => x.TongTien) // Cái nào nhiều tiền xếp trên
                .ToList();

            // Tính % vốn chiếm dụng (Để vẽ biểu đồ hoặc nhìn cho dễ)
            decimal tongVonCaCuaHang = baoCao.Sum(x => x.TongTien);
            if (tongVonCaCuaHang > 0)
            {
                foreach (var item in baoCao)
                {
                    item.TiLeVon = Math.Round((item.TongTien / tongVonCaCuaHang) * 100, 2);
                }
            }

            ViewBag.TongVon = tongVonCaCuaHang;
            ViewBag.TongDon = baoCao.Sum(x => x.SoLuong);

            return View(baoCao);
        }
    }

    // Class nhỏ để chứa dữ liệu hiển thị (DTO)
    public class BaoCaoViewModel
    {
        public string ?TenLoai { get; set; }
        public int SoLuong { get; set; }
        public decimal TongTien { get; set; }
        public decimal TiLeVon { get; set; } // Chiếm bao nhiêu % vốn
    }
}