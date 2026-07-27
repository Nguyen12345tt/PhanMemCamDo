using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Controllers
{
    public class PaymentHistoriesController(PawnShopDbContext context) : Controller
    {
        // 1. DANH SÁCH LỊCH SỬ (INDEX)
        public async Task<IActionResult> Index(int? contractId)
        {
            var query = context.PaymentHistories
                .Include(p => p.PawnContract)
                    // 👇 SỬA LỖI Ở ĐÂY: Thêm dấu '!' vào sau chữ 'c'
                    .ThenInclude(c => c!.Customer)
                .AsQueryable();

            if (contractId != null)
            {
                query = query.Where(x => x.PawnContractId == contractId);
                ViewBag.ContractId = contractId;
            }

            return View(await query.OrderByDescending(x => x.PaymentDate).ToListAsync());
        }

        // 2. HIỆN FORM THU TIỀN (GET)
        public async Task<IActionResult> Create(int? contractId)
        {
            if (contractId == null)
            {
                ViewData["PawnContractId"] = new SelectList(context.PawnContracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .Include(c => c.Customer), "Id", "ContractCode");
            }
            else
            {
                var contract = await context.PawnContracts
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.Id == contractId);

                if (contract != null)
                {
                    ViewBag.SelectedContract = contract;
                    
                    ViewBag.SuggestedAmount = contract.PawnAmount * contract.InterestRate / 100;
                }
            }

            return View();
        }

        // 3. XỬ LÝ LƯU TIỀN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentHistory paymentHistory, int? AutoExtendDays)
        {
            ModelState.Remove("PawnContract");

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    // A. LƯU LỊCH SỬ
                    paymentHistory.PaymentDate = DateTime.Now;
                    context.Add(paymentHistory);
                    await context.SaveChangesAsync();

                    // B. LƯU SỔ QUỸ
                    var contract = await context.PawnContracts.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == paymentHistory.PawnContractId);

                    var cashFlow = new CashFlow
                    {
                        Date = DateTime.Now,
                        Amount = paymentHistory.Amount,
                        // 👇 Đảm bảo dùng đúng Income (khớp với file Enum bác đã tạo)
                        FlowType = CashFlowType.Income,
                        Description = $"Thu tiền {GetEnumName(paymentHistory.PaymentType)} HĐ {contract?.ContractCode} - Khách {contract?.Customer?.FullName}",
                        UserName = "Admin"
                    };
                    context.CashFlows.Add(cashFlow);

                    // C. CẬP NHẬT HỢP ĐỒNG
                    if (contract != null)
                    {
                        if (paymentHistory.PaymentType == PaymentType.Interest && AutoExtendDays > 0)
                        {
                            contract.EndDate = contract.EndDate.AddDays(AutoExtendDays.Value);
                            context.Update(contract);
                        }
                        else if (paymentHistory.PaymentType == PaymentType.Principal)
                        {
                            contract.PawnAmount -= paymentHistory.Amount;

                            if (contract.PawnAmount <= 0)
                            {
                                contract.PawnAmount = 0;
                                
                                contract.Status = ContractStatus.Redeemed;
                            }
                            context.Update(contract);
                        }
                        else if (paymentHistory.PaymentType == PaymentType.Redeem)
                        {
                            contract.PawnAmount = 0;
                            contract.Status = ContractStatus.Redeemed;
                            context.Update(contract);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                }
            }

            ViewData["PawnContractId"] = new SelectList(context.PawnContracts.Where(c => c.Status == ContractStatus.Active), "Id", "ContractCode", paymentHistory.PawnContractId);
            return View(paymentHistory);
        }

        // 👇 Đã thêm 'static' để tối ưu code (hết cảnh báo vàng)
        private static string GetEnumName(PaymentType type)
        {
            return type switch
            {
                PaymentType.Interest => "Lãi",
                PaymentType.Principal => "Gốc",
                PaymentType.Redeem => "Chuộc",
                _ => "Khác"
            };
        }
    }
}