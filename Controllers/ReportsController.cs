using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Enums;
using System.Globalization;

namespace PhanMemCamDo.Controllers
{
    public class ReportsController(PawnShopDbContext context) : Controller
    {
        // GET: /Reports
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string range = "this_month")
        {
            var now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, 1);
            DateTime end = now;

            if (range == "today")
            {
                start = now.Date;
                end = now.Date.AddDays(1).AddTicks(-1);
            }
            else if (range == "last_month")
            {
                var prevMonth = now.AddMonths(-1);
                start = new DateTime(prevMonth.Year, prevMonth.Month, 1);
                end = start.AddMonths(1).AddTicks(-1);
            }
            else if (range == "this_year")
            {
                start = new DateTime(now.Year, 1, 1);
                end = new DateTime(now.Year, 12, 31, 23, 59, 59);
            }
            else if (fromDate.HasValue && toDate.HasValue)
            {
                start = fromDate.Value.Date;
                end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                range = "custom";
            }

            // 1. Thống kê Hợp Đồng
            var activeContracts = await context.PawnContracts
                .Include(c => c.Asset)
                .ThenInclude(a => a!.AssetCategory)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            decimal tongVonDangChoVay = activeContracts.Sum(c => c.PawnAmount);
            int tongHopDongActive = activeContracts.Count;

            // 2. Thống kê Lịch Sử Thu Tiền trong khoảng thời gian chọn
            var paymentsInRange = await context.PaymentHistories
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
                .ToListAsync();

            decimal tongLaiDaThu = paymentsInRange.Where(p => p.PaymentType == PaymentType.Interest).Sum(p => p.Amount);
            decimal tongGocDaThu = paymentsInRange.Where(p => p.PaymentType == PaymentType.Principal).Sum(p => p.Amount);
            decimal tongThuChuocDo = paymentsInRange.Where(p => p.PaymentType == PaymentType.Redeem).Sum(p => p.Amount);
            decimal tongThuThanhLy = paymentsInRange.Where(p => p.PaymentType == PaymentType.Liquidation).Sum(p => p.Amount);

            // 3. Phân bổ vốn theo danh mục tài sản
            var baoCaoDanhMuc = activeContracts
                .GroupBy(c => c.Asset?.AssetCategory?.Name ?? "Khác")
                .Select(g => new DanhMucBaoCaoItem
                {
                    TenLoai = g.Key,
                    SoLuong = g.Count(),
                    TongTien = g.Sum(c => c.PawnAmount),
                    TiLeVon = tongVonDangChoVay > 0 ? Math.Round((g.Sum(c => c.PawnAmount) / tongVonDangChoVay) * 100, 1) : 0
                })
                .OrderByDescending(x => x.TongTien)
                .ToList();

            // 4. Doanh thu tiền lãi 6 tháng gần nhất (Dùng cho Biểu đồ cột)
            var monthlyInterestChartData = new List<MonthlyRevenueItem>();
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var mStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                var mEnd = mStart.AddMonths(1).AddTicks(-1);

                var mInterest = await context.PaymentHistories
                    .Where(p => p.PaymentType == PaymentType.Interest && p.PaymentDate >= mStart && p.PaymentDate <= mEnd)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                monthlyInterestChartData.Add(new MonthlyRevenueItem
                {
                    MonthLabel = $"Tháng {targetMonth.Month}/{targetMonth.Year}",
                    TotalRevenue = mInterest
                });
            }

            // 5. Tổng số hợp đồng theo các trạng thái
            ViewBag.CountActive = activeContracts.Count;
            ViewBag.CountOverdue = await context.PawnContracts.CountAsync(c => c.Status == ContractStatus.Overdue);
            ViewBag.CountRedeemed = await context.PawnContracts.CountAsync(c => c.Status == ContractStatus.Redeemed);
            ViewBag.CountLiquidated = await context.PawnContracts.CountAsync(c => c.Status == ContractStatus.Liquidated);

            ViewBag.FromDate = start.ToString("yyyy-MM-dd");
            ViewBag.ToDate = end.ToString("yyyy-MM-dd");
            ViewBag.Range = range;

            ViewBag.TongVonDangChoVay = tongVonDangChoVay;
            ViewBag.TongLaiDaThu = tongLaiDaThu;
            ViewBag.TongGocDaThu = tongGocDaThu;
            ViewBag.TongThuChuocDo = tongThuChuocDo;
            ViewBag.TongThuThanhLy = tongThuThanhLy;

            ViewBag.MonthlyInterestChart = monthlyInterestChartData;

            return View(baoCaoDanhMuc);
        }
    }

    public class DanhMucBaoCaoItem
    {
        public string ?TenLoai { get; set; }
        public int SoLuong { get; set; }
        public decimal TongTien { get; set; }
        public decimal TiLeVon { get; set; }
    }

    public class MonthlyRevenueItem
    {
        public string ?MonthLabel { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}