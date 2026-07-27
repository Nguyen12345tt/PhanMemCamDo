using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models;
using PhanMemCamDo.Models.Enums;
using PhanMemCamDo.Models.ViewModels;
using System.Diagnostics;

namespace PhanMemCamDo.Controllers
{
    public class HomeController(PawnShopDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            // Nếu chưa đăng nhập thì bắt đăng nhập ngay
            // if (HttpContext.Session.GetString("Username") == null) return RedirectToAction("Login", "Account");

            // --- TÍNH TOÁN SỐ LIỆU THỐNG KÊ ---
            var viewModel = new DashboardVM
            {
                // 1. Đếm tổng khách
                TotalCustomers = await context.Customers.CountAsync(),

                // 2. Đếm hợp đồng đang hoạt động
                ActiveContracts = await context.PawnContracts
                                        .CountAsync(c => c.Status == ContractStatus.Active),

                // 3. Tổng tiền đang "thả gà ra đuổi" (Tiền gốc đang vay)
                TotalLoanAmount = await context.PawnContracts
                                        .Where(c => c.Status == ContractStatus.Active)
                                        .SumAsync(c => c.PawnAmount),

                // 4. Tính tổng doanh thu/lãi
                TotalProfit = await context.PaymentHistories
                                        .Where(p => p.PaymentType == PaymentType.Interest)
                                        .SumAsync(p => p.Amount),

                // 5. Đếm hợp đồng sắp hết hạn trong 3 ngày tới
                ContractsDueSoon = await context.PawnContracts
                                         .CountAsync(c => c.Status == ContractStatus.Active
                                                          && c.EndDate <= DateTime.Now.AddDays(3)),

                // 6. Lấy 5 hợp đồng mới nhất để hiện ra bảng
                RecentContracts = await context.PawnContracts
                                        .Include(c => c.Customer) // Kèm tên khách
                                        .Include(c => c.Asset)    // Kèm tên đồ
                                        .OrderByDescending(c => c.StartDate) // Mới nhất lên đầu
                                        .Take(5)
                                        .ToListAsync()
            };

            ViewBag.ActiveCount = viewModel.ActiveContracts;
            ViewBag.RedeemedCount = await context.PawnContracts.CountAsync(c => c.Status == ContractStatus.Redeemed);
            ViewBag.OverdueCount = await context.PawnContracts.CountAsync(c => c.Status == ContractStatus.Overdue);
            ViewBag.LiquidatedCount = await context.PawnContracts.CountAsync(c => c.Status == ContractStatus.Liquidated);

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}