using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Controllers
{
    public class AssetsController(PawnShopDbContext context) : Controller
    {
        // 1. DANH SÁCH (CÓ TÌM KIẾM)
        public async Task<IActionResult> Index(string searchString)
        {
            var assets = from a in context.Assets select a;

            if (!string.IsNullOrEmpty(searchString))
            {
                assets = assets.Where(s => 
                    (s.AssetName != null && s.AssetName.Contains(searchString) ||
                    s.Description != null && s.Description.Contains(searchString)));
            }

            return View(await assets.ToListAsync());
        }

        // 2. TẠO MỚI (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. TẠO MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (ModelState.IsValid)
            {
                context.Add(asset);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // 4. SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var asset = await context.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        // 5. SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Asset asset)
        {
            if (id != asset.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(asset);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // 6. XÓA (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var asset = await context.Assets.FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        // 7. XÁC NHẬN XÓA (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await context.Assets.FindAsync(id);
            if (asset != null)
            {
                try
                {
                    context.Assets.Remove(asset);
                    await context.SaveChangesAsync();
                    // Báo thành công nếu tài sản ko bị ràng buộc khóa ngoại
                    TempData["SuccessMessage"] = "Đã xóa tài sản thành công!";
                }
                catch (DbUpdateException)
                {
                    // 👇 Lưu lỗi vào TempData nếu không thể xóa do ràng buộc khóa ngoại
                    TempData["ErrorMessage"] = "⛔ Không thể xóa: Tài sản này đang nằm trong Hợp đồng cầm đồ.";
                }
            }
            // 👇 Luôn quay về trang Index (Dù xóa được hay không)
            return RedirectToAction(nameof(Index));
        }

        private bool AssetExists(int id)
        {
            return context.Assets.Any(e => e.Id == id);
        }
    }
}