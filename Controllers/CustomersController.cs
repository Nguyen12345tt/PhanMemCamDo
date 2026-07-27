using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Controllers
{
    public class CustomersController(PawnShopDbContext context) : Controller
    {
        // 1. DANH SÁCH KHÁCH HÀNG (Có tìm kiếm)
        public async Task<IActionResult> Index(string searchString)
        {
            var customers = from c in context.Customers select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(s => 
                    s.FullName != null && s.FullName.Contains(searchString) ||
                    s.PhoneNumber != null && s.PhoneNumber.Contains(searchString) ||
                    s.IdentityCard != null && s.IdentityCard.Contains(searchString));
            }

            return View(await customers.OrderByDescending(x => x.Id).ToListAsync());
        }

        // 2. TẠO KHÁCH MỚI (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. XỬ LÝ TẠO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng CCCD
                if (await context.Customers.AnyAsync(c => c.IdentityCard == customer.IdentityCard))
                {
                    ModelState.AddModelError("IdentityCard", "Số CCCD này đã tồn tại trong hệ thống!");
                    return View(customer);
                }

                context.Add(customer);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 4. SỬA THÔNG TIN (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // 5. XỬ LÝ SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(customer);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 6. XEM LỊCH SỬ CẦM ĐỒ CỦA KHÁCH (Hay dùng)
        public async Task<IActionResult> History(int? id)
        {
            if (id == null) return NotFound();

            var customer = await context.Customers
                .Include(c => c.PawnContracts) // Load danh sách hợp đồng
                .ThenInclude(h => h.Asset)     // Load tiếp thông tin tài sản trong hợp đồng
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        private bool CustomerExists(int id)
        {
            return context.Customers.Any(e => e.Id == id);
        }

        // 7. XÓA KHÁCH HÀNG (POST) - BẢO MẬT TUYỆT ĐỐI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Load khách kèm theo Hợp đồng để kiểm tra
            var customer = await context.Customers
                .Include(c => c.PawnContracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer != null)
            {
                // 🔒 ĐIỀU KIỆN TIÊN QUYẾT: Nếu có hợp đồng -> CẤM XÓA VĨNH VIỄN
                if (customer.PawnContracts.Count > 0)
                {
                    TempData["NotificationType"] = "error";
                    TempData["NotificationMessage"] = $"⛔ CẤM XÓA: Khách '{customer.FullName}' đang có dữ liệu hợp đồng!";
                    return RedirectToAction(nameof(Index));
                }

                // Nếu sạch sẽ -> Cho xóa
                context.Customers.Remove(customer);
                await context.SaveChangesAsync();

                TempData["NotificationType"] = "success";
                TempData["NotificationMessage"] = $"✅ Đã xóa vĩnh viễn khách hàng: {customer.FullName}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}