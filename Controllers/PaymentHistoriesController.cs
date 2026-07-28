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
                            if (paymentHistory.Amount > contract.PawnAmount)
                            {
                                ModelState.AddModelError("Amount", $"Số tiền trả bớt gốc ({paymentHistory.Amount:N0}đ) không được vượt quá số tiền nợ gốc hiện tại ({contract.PawnAmount:N0}đ)!");
                                transaction.Rollback();
                                ViewData["PawnContractId"] = new SelectList(context.PawnContracts.Where(c => c.Status == ContractStatus.Active), "Id", "ContractCode", paymentHistory.PawnContractId);
                                return View(paymentHistory);
                            }

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
                        else if (paymentHistory.PaymentType == PaymentType.Liquidation)
                        {
                            contract.PawnAmount = 0;
                            contract.Status = ContractStatus.Liquidated;
                            context.Update(contract);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "🎉 Thu tiền khách hàng thành công!";
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

        // 4. API LẤY CHI TIẾT HỢP ĐỒNG BẰNG AJAX
        [HttpGet]
        public async Task<IActionResult> GetContractDetail(int id)
        {
            var contract = await context.PawnContracts
                .Include(c => c.Customer)
                .Include(c => c.Asset)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound();

            var interestPerMonth = contract.PawnAmount * contract.InterestRate / 100m;

            return Json(new
            {
                id = contract.Id,
                code = contract.ContractCode,
                customerName = contract.Customer?.FullName ?? "N/A",
                phone = contract.Customer?.PhoneNumber ?? "N/A",
                assetName = contract.Asset?.AssetName ?? "N/A",
                pawnAmount = contract.PawnAmount,
                interestRate = contract.InterestRate,
                startDate = contract.StartDate.ToString("dd/MM/yyyy"),
                endDate = contract.EndDate.ToString("dd/MM/yyyy"),
                endDateIso = contract.EndDate.ToString("yyyy-MM-ddTHH:mm"),
                suggestedInterest = Math.Round(interestPerMonth, 0)
            });
        }

        private static string GetEnumName(PaymentType type)
        {
            return type switch
            {
                PaymentType.Interest => "Lãi",
                PaymentType.Principal => "Trả bớt gốc",
                PaymentType.Redeem => "Chuộc đồ",
                PaymentType.Liquidation => "Thanh lý",
                _ => "Khác"
            };
        }
    }
}