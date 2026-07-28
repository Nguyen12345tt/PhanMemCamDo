using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Services;
using PhanMemCamDo.Models.Enums;

namespace PhanMemCamDo.Controllers
{
    public class PawnContractsController : Controller
    {
        private readonly PawnShopDbContext context;
        // 1. THÊM MỚI: Khai báo biến để chứa cái máy tính tiền
        private readonly PawnCalculator _pawnCalculator;

        // 2. SỬA ĐỔI: Nhận cái máy tính tiền (PawnCalculator) từ hệ thống truyền vào
        public PawnContractsController(PawnShopDbContext context, PawnCalculator pawnCalculator)
        {
            this.context = context;
            _pawnCalculator = pawnCalculator;
        }

        // 1. DANH SÁCH HỢP ĐỒNG (INDEX)
        public async Task<IActionResult> Index(string? searchString, string? statusFilter)
        {
            var contracts = context.PawnContracts
                .Include(p => p.Asset)
                    .ThenInclude(a => a!.AssetCategory)
                .Include(p => p.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                contracts = contracts.Where(s =>
                    (s.ContractCode != null && s.ContractCode.Contains(searchString ?? "")) ||
                    (s.Customer != null && s.Customer.FullName != null && s.Customer.FullName.Contains(searchString ?? ""))
                );
            }

            ViewBag.VonDangVay = await contracts
                .Where(c => c.Status == ContractStatus.Active)
                .SumAsync(c => c.PawnAmount);

            ViewBag.DangChay = await contracts
                .CountAsync(c => c.Status == ContractStatus.Active);

            var today = DateTime.Now.Date;
            var threeDaysLater = today.AddDays(3);
            ViewBag.SapDenHan = await contracts
                .CountAsync(c => c.Status == ContractStatus.Active
                                 && c.EndDate >= today
                                 && c.EndDate <= threeDaysLater);

            ViewBag.QuaHan = await contracts
                .CountAsync(c => c.Status == ContractStatus.Active && c.EndDate < today);

            ViewBag.DaThanhLy = await contracts
                .CountAsync(c => c.Status == ContractStatus.Liquidated);

            ViewBag.LaiDuKien = await contracts
                .Where(c => c.Status == ContractStatus.Active)
                .SumAsync(c => (c.PawnAmount) * (c.InterestRate) / 100);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter.ToLower())
                {
                    case "active":
                        contracts = contracts.Where(c => c.Status == ContractStatus.Active);
                        break;
                    case "near":
                        contracts = contracts.Where(c => c.Status == ContractStatus.Active && c.EndDate >= today && c.EndDate <= threeDaysLater);
                        break;
                    case "overdue":
                        contracts = contracts.Where(c => c.Status == ContractStatus.Active && c.EndDate < today);
                        break;
                    case "liquidated":
                        contracts = contracts.Where(c => c.Status == ContractStatus.Liquidated);
                        break;
                    case "redeemed":
                        contracts = contracts.Where(c => c.Status == ContractStatus.Redeemed);
                        break;
                }
            }

            ViewBag.CurrentStatusFilter = statusFilter;

            var resultList = await contracts
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return View(resultList);
        }

        // 2. XEM CHI TIẾT (DETAILS) - ĐÃ CẬP NHẬT TÍNH LÃI
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pawnContract = await context.PawnContracts
                .Include(p => p.Asset)
                    .ThenInclude(a => a!.AssetCategory)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pawnContract == null) return NotFound();

            // --- 3. GỌI SERVICE TÍNH TIỀN ---
            // Tính tổng tiền phải trả (Gốc + Lãi + Phạt) tính đến thời điểm hiện tại (DateTime.Now)
            decimal tongTien = _pawnCalculator.CalculateTotalPayment(pawnContract, DateTime.Now);

            // Đẩy ra View để hiển thị
            ViewBag.TongTienPhaiTra = tongTien;
            // --------------------------------

            return View(pawnContract);
        }

        // 3. TẠO HỢP ĐỒNG MỚI (GET)
        public async Task<IActionResult> Create(int? customerId)
        {
            if (customerId != null)
            {
                var khachQuen = await context.Customers.FindAsync(customerId);
                if (khachQuen != null) ViewBag.KhachQuen = khachQuen;
            }

            ViewData["CustomerId"] = new SelectList(context.Customers, "Id", "FullName");
            ViewData["AssetCategoryId"] = new SelectList(context.AssetCategories, "Id", "Name");

            var configLaiSuat = await context.SystemConfigs.FirstOrDefaultAsync(x => x.ConfigKey == "LaiSuatMacDinh");
            ViewBag.LaiSuatGoiY = configLaiSuat?.ConfigValue ?? "3";

            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var autoCode = await GenerateNewContractCode();
            var model = new PawnContract
            {
                ContractCode = autoCode,
                StartDate = startDate,
                EndDate = startDate.AddMonths(1)
            };

            return View(model);
        }

        // 4. XỬ LÝ TẠO HỢP ĐỒNG (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PawnContract pawnContract,
                                                string TenTaiSan, string MoTaTaiSan,
                                                string TenKhach, string SDT, string CCCD, string DiaChi,
                                                int AssetCategoryId)
        {
            if (string.IsNullOrWhiteSpace(pawnContract.ContractCode))
            {
                pawnContract.ContractCode = await GenerateNewContractCode();
            }
            ModelState.Remove("ContractCode");

            ModelState.Remove("Customer");
            ModelState.Remove("Asset");
            ModelState.Remove("CustomerId");
            ModelState.Remove("AssetId");

            if (string.IsNullOrWhiteSpace(CCCD) || !System.Text.RegularExpressions.Regex.IsMatch(CCCD.Trim(), @"^\d{12}$"))
            {
                ModelState.AddModelError("CCCD", "Số CCCD/CMND là bắt buộc và phải đúng 12 chữ số!");
            }

            if (string.IsNullOrWhiteSpace(SDT) || !System.Text.RegularExpressions.Regex.IsMatch(SDT.Trim(), @"^\d{10}$"))
            {
                ModelState.AddModelError("SDT", "Số điện thoại là bắt buộc và phải đúng 10 chữ số!");
            }

            if (string.IsNullOrWhiteSpace(TenKhach))
            {
                ModelState.AddModelError("TenKhach", "Họ tên khách hàng là bắt buộc!");
            }

            if (pawnContract.PawnAmount <= 0)
            {
                ModelState.AddModelError("PawnAmount", "Số tiền cầm là bắt buộc và phải lớn hơn 0!");
            }

            if (!ModelState.IsValid)
            {
                return Content("Lỗi nhập liệu: " + string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            try
            {
                var existingCustomer = await context.Customers.FirstOrDefaultAsync(c => c.IdentityCard == CCCD);
                if (existingCustomer != null)
                {
                    pawnContract.CustomerId = existingCustomer.Id;
                    existingCustomer.FullName = TenKhach;
                    existingCustomer.PhoneNumber = SDT;
                    if (!string.IsNullOrWhiteSpace(DiaChi))
                    {
                        existingCustomer.Address = DiaChi;
                    }
                    context.Update(existingCustomer);
                }
                else
                {
                    var newCustomer = new Customer
                    {
                        FullName = TenKhach,
                        PhoneNumber = SDT,
                        IdentityCard = CCCD,
                        Address = string.IsNullOrWhiteSpace(DiaChi) ? "Chưa cập nhật" : DiaChi
                    };
                    context.Customers.Add(newCustomer);
                    await context.SaveChangesAsync();
                    pawnContract.CustomerId = newCustomer.Id;
                }

                var newAsset = new Asset { AssetName = string.IsNullOrEmpty(TenTaiSan) ? "Tài sản chưa tên" : TenTaiSan, Description = MoTaTaiSan ?? "", AssetCategoryId = AssetCategoryId };
                context.Assets.Add(newAsset);
                await context.SaveChangesAsync();
                pawnContract.AssetId = newAsset.Id;

                if (pawnContract.EndDate == DateTime.MinValue || pawnContract.EndDate <= pawnContract.StartDate)
                    pawnContract.EndDate = pawnContract.StartDate.AddMonths(1);

                pawnContract.Status = ContractStatus.Active;
                context.Add(pawnContract);
                await context.SaveChangesAsync();

                await GhiNhatKy("TẠO MỚI", $"Tạo HĐ {pawnContract.ContractCode} - Khách: {TenKhach} - Số tiền: {pawnContract.PawnAmount:N0}đ");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content($"🔥 LỖI HỆ THỐNG: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // 5. SỬA HỢP ĐỒNG (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pawnContract = await context.PawnContracts.Include(a => a.Asset).FirstOrDefaultAsync(x => x.Id == id);
            if (pawnContract == null) return NotFound();

            ViewData["CustomerId"] = new SelectList(context.Customers, "Id", "FullName", pawnContract.CustomerId);
            ViewData["AssetCategoryId"] = new SelectList(context.AssetCategories, "Id", "Name", pawnContract.Asset?.AssetCategoryId);
            return View(pawnContract);
        }

        // 6. XỬ LÝ SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PawnContract pawnContract, int AssetCategoryId, string TenTaiSan, string MoTaTaiSan)
        {
            if (id != pawnContract.Id) return NotFound();
            ModelState.Remove("Customer");
            ModelState.Remove("Asset");

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(pawnContract);
                    var assetToUpdate = await context.Assets.FindAsync(pawnContract.AssetId);
                    if (assetToUpdate != null)
                    {
                        assetToUpdate.AssetName = TenTaiSan;
                        assetToUpdate.Description = MoTaTaiSan;
                        assetToUpdate.AssetCategoryId = AssetCategoryId;
                        context.Update(assetToUpdate);
                    }
                    await context.SaveChangesAsync();
                    await GhiNhatKy("CẬP NHẬT", $"Sửa HĐ {pawnContract.ContractCode}. Trạng thái mới: {pawnContract.Status}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PawnContractExists(pawnContract.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pawnContract);
        }

        // 7. XÓA HỢP ĐỒNG (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var pawnContract = await context.PawnContracts.Include(p => p.Asset).Include(p => p.Customer).FirstOrDefaultAsync(m => m.Id == id);
            if (pawnContract == null) return NotFound();
            return View(pawnContract);
        }

        // 8. XÁC NHẬN XÓA (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pawnContract = await context.PawnContracts.Include(p => p.Customer).FirstOrDefaultAsync(m => m.Id == id);
            if (pawnContract != null)
            {
                await GhiNhatKy("XÓA HỢP ĐỒNG", $"Đã xóa HĐ {pawnContract.ContractCode} của khách {pawnContract.Customer?.FullName}");
                context.PawnContracts.Remove(pawnContract);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 9. THANH LÝ TÀI SẢN (GET)
        public async Task<IActionResult> Liquidate(int? id)
        {
            if (id == null) return NotFound();

            var pawnContract = await context.PawnContracts
                .Include(p => p.Asset)
                    .ThenInclude(a => a!.AssetCategory)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pawnContract == null) return NotFound();

            if (pawnContract.Status == ContractStatus.Liquidated)
            {
                TempData["ErrorMessage"] = "Hợp đồng này đã được thanh lý trước đó!";
                return RedirectToAction(nameof(Details), new { id = pawnContract.Id });
            }

            decimal tongTienPhaiTra = _pawnCalculator.CalculateTotalPayment(pawnContract, DateTime.Now);
            decimal tienLaiTichLuy = tongTienPhaiTra - pawnContract.PawnAmount;

            ViewBag.TongTienPhaiTra = tongTienPhaiTra;
            ViewBag.TienLaiTichLuy = tienLaiTichLuy;

            return View(pawnContract);
        }

        // 10. XỬ LÝ THANH LÝ TÀI SẢN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiquidateConfirmed(int id, decimal sellingPrice, string? note)
        {
            var pawnContract = await context.PawnContracts
                .Include(p => p.Asset)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pawnContract == null) return NotFound();

            if (pawnContract.Status == ContractStatus.Liquidated)
            {
                TempData["ErrorMessage"] = "Hợp đồng này đã được thanh lý trước đó!";
                return RedirectToAction(nameof(Index));
            }

            if (sellingPrice < 0)
            {
                TempData["ErrorMessage"] = "Số tiền bán thanh lý không hợp lệ!";
                return RedirectToAction(nameof(Liquidate), new { id = id });
            }

            // 1. Cập nhật trạng thái hợp đồng thành Đã Thanh Lý
            pawnContract.Status = ContractStatus.Liquidated;
            context.Update(pawnContract);

            // 2. Thêm lịch sử thanh toán
            var paymentHistory = new PaymentHistory
            {
                PawnContractId = pawnContract.Id,
                Amount = sellingPrice,
                PaymentDate = DateTime.Now,
                PaymentType = PaymentType.Liquidation,
                Note = string.IsNullOrEmpty(note)
                    ? $"Thanh lý tài sản: {pawnContract.Asset?.AssetName}"
                    : $"Thanh lý tài sản: {pawnContract.Asset?.AssetName} - Ghi chú: {note}"
            };
            context.PaymentHistories.Add(paymentHistory);

            // 3. Hạch toán vào Sổ Quỹ (CashFlows - Thu)
            var cashFlow = new CashFlow
            {
                Date = DateTime.Now,
                Amount = sellingPrice,
                FlowType = CashFlowType.Income,
                Description = $"Thu tiền thanh lý tài sản HĐ {pawnContract.ContractCode} ({pawnContract.Asset?.AssetName}) - Khách: {pawnContract.Customer?.FullName}. Giá gốc: {pawnContract.PawnAmount:N0}đ, Giá bán: {sellingPrice:N0}đ",
                UserName = HttpContext.Session.GetString("Username") ?? "Admin"
            };
            context.CashFlows.Add(cashFlow);

            await context.SaveChangesAsync();

            // 4. Ghi Nhật Ký Hệ Thống
            await GhiNhatKy("THANH LÝ TÀI SẢN", $"Thanh lý tài sản HĐ {pawnContract.ContractCode} với số tiền {sellingPrice:N0}đ. Gốc: {pawnContract.PawnAmount:N0}đ");

            TempData["SuccessMessage"] = $"Đã thanh lý tài sản thành công! Hợp đồng {pawnContract.ContractCode} được cập nhật trạng thái Đã Thanh Lý.";
            return RedirectToAction(nameof(Index));
        }

        // KIỂM TRA HỢP ĐỒNG TỒN TẠI
        private bool PawnContractExists(int id)
        {
            return context.PawnContracts.Any(e => e.Id == id);
        }

        // TỰ ĐỘNG TẠO MÃ HỢP ĐỒNG NGẮN GỌN (HD0001, HD0002...)
        private async Task<string> GenerateNewContractCode()
        {
            var count = await context.PawnContracts.CountAsync();
            int nextId = count + 1;
            string newCode = $"HD{nextId:D4}";
            while (await context.PawnContracts.AnyAsync(c => c.ContractCode == newCode))
            {
                nextId++;
                newCode = $"HD{nextId:D4}";
            }
            return newCode;
        }

        // GHI NHẬT KÝ HÀNH ĐỘNG
        private async Task GhiNhatKy(string hanhDong, string chiTiet)
        {
            var log = new ActionLog { ActionName = hanhDong, Description = chiTiet, EntityName = "Hợp Đồng", Timestamp = DateTime.Now, UserName = "Admin" };
            context.ActionLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}