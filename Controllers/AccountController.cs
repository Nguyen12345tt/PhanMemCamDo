using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Models.ViewModels;
//using System.Security.Cryptography; // Để mã hóa mật khẩu
using System.Text;

namespace PhanMemCamDo.Controllers
{
    public class AccountController(PawnShopDbContext context) : Controller
    {
        // 1. Hiện Form Đăng Ký (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 2. Xử lý khi bấm nút Đăng Ký (POST)
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Username đã có người dùng chưa
                bool isExist = await context.Users.AnyAsync(u => u.Username == model.Username);
                if (isExist)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã có người dùng!");
                    return View(model);
                }

                // Tạo người dùng mới
                var newUser = new User
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Password = model.Password,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    // MÃ HÓA MẬT KHẨU (An toàn tuyệt đối)
                    //PasswordHash = HashPassword(model.Password ?? ""),
                    Role = model.Role, // Lưu đúng vai trò do người dùng chọn (Admin / Staff)
                    IsActive = true
                };

                context.Users.Add(newUser);
                await context.SaveChangesAsync(); // Lưu vào SQL

                TempData["SuccessMessage"] = "Tạo tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // 3. Hiện Form Đăng Nhập (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 4. Xử lý Đăng Nhập (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập không tồn tại!");
                    return View(model);
                }

                if (user.Password != model.Password)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng!");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động!");
                    return View(model);
                }

                // Lưu thông tin người dùng vào Session
                HttpContext.Session.SetString("Username", user.Username ?? "");
                HttpContext.Session.SetString("FullName", user.FullName ?? user.Username ?? "");
                HttpContext.Session.SetString("Role", user.Role.ToString());

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // 5. Đăng Xuất (Thêm mới)
        public IActionResult Logout()
        {
            // Xóa sạch bộ nhớ phiên làm việc
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            // 1. Kiểm tra đăng nhập
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // 2. Lấy thông tin user từ Database
            var user = context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null) return NotFound();

            return View(user);
        }

        // 6. Cập nhật thông tin cá nhân (GET)
        [HttpGet]
        public IActionResult EditProfile()
        {
            // 1. Kiểm tra đăng nhập
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }
            // 2. Lấy thông tin user từ Database
            var user = context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // 2. Lấy thông tin user từ Database
            var userInDb = context.Users.FirstOrDefault(u => u.Username == username);

            if (userInDb != null)
            {
                // Cập nhật các trường thông tin cá nhân
                userInDb.FullName = model.FullName;
                userInDb.Email = model.Email;
                userInDb.PhoneNumber = model.PhoneNumber;
                // Lưu thay đổi vào Database
                context.Users.Update(userInDb);
                await context.SaveChangesAsync();

                // Thông báo thành công
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction("Profile");
            }

            TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
            return View("Profile", model);
        }

        // 7. Đổi mật khẩu trong profile bằng thông báo (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // 1. Kiểm tra dữ liệu đầu vào (ModelState)
            // Nếu dữ liệu không hợp lệ (ví dụ: để trống), ta KHÔNG được return View(model)
            // mà phải quay về Profile và báo lỗi để mở lại Modal.
            if (!ModelState.IsValid)
            {
                // Lấy lỗi đầu tiên để hiển thị
                var message = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .FirstOrDefault();

                TempData["PassError"] = message ?? "Vui lòng nhập đầy đủ thông tin!";
                TempData["KeepOpen"] = true; // Tín hiệu mở lại Modal
                return RedirectToAction("Profile"); // <--- QUAN TRỌNG: Quay về Profile
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Kiểm tra mật khẩu cũ
            if (user.Password != model.OldPassword)
            {
                TempData["PassError"] = "Mật khẩu hiện tại không đúng!";
                TempData["KeepOpen"] = true;
                return RedirectToAction("Profile"); // <--- Quay về Profile
            }

            // 3. Kiểm tra mật khẩu xác nhận
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["PassError"] = "Mật khẩu xác nhận không khớp!";
                TempData["KeepOpen"] = true;
                return RedirectToAction("Profile"); // <--- Quay về Profile
            }

            // 4. Lưu thay đổi
            user.Password = model.NewPassword;
            context.Users.Update(user);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}