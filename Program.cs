using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. KHAI BÁO DỊCH VỤ (SERVICES) ---

// Thêm dịch vụ Session (Để lưu trạng thái đăng nhập)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Đăng nhập giữ trong 60 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm dịch vụ truy cập HttpContext (Để gọi Session từ View/Controller dễ hơn)
builder.Services.AddHttpContextAccessor();

// Thêm Controllers và Views
builder.Services.AddControllersWithViews();

// Thêm swagger để test API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kết nối SQL Server
builder.Services.AddDbContext<PawnShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm dịch vụ tính lãi cầm đồ
builder.Services.AddScoped<PhanMemCamDo.Services.PawnCalculator>();

var app = builder.Build();

// Tự động khởi tạo Database nếu chưa tồn tại
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PawnShopDbContext>();
    dbContext.Database.EnsureCreated();
}

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhanMemCamDo API v1"));
}

app.UseHttpsRedirection();

// Xử lý file tĩnh (CSS/JS)
app.UseRouting();

app.UseAuthorization();

// --- QUAN TRỌNG: KÍCH HOẠT SESSION ---
// (Phải đặt SAU UseRouting và TRƯỚC MapControllerRoute)
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();