using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsApiController(PawnShopDbContext context) : ControllerBase
    {
        // GET: api/NotificationsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await context.Notifications.ToListAsync();
        }

        // GET: api/NotificationsApi/unread
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            await AutoCheckContractNotifications();

            var unreadList = await context.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(20)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.ContractId,
                    Url = !string.IsNullOrEmpty(n.Url) ? n.Url : (n.ContractId.HasValue ? $"/PawnContracts/Details/{n.ContractId}" : "/PawnContracts"),
                    CreatedDate = n.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            var unreadCount = await context.Notifications.CountAsync(n => !n.IsRead);

            return Ok(new {
                unreadCount = unreadCount,
                items = unreadList
            });
        }

        // POST: api/NotificationsApi/mark-as-read/5
        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync();
            }
            return Ok();
        }

        // POST: api/NotificationsApi/mark-all-read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unreadItems = await context.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var item in unreadItems)
            {
                item.IsRead = true;
            }
            await context.SaveChangesAsync();
            return Ok();
        }

        private async Task AutoCheckContractNotifications()
        {
            var today = DateTime.Today;
            var now = DateTime.Now;

            // 1. Kiểm tra hợp đồng quá hạn (EndDate.Date < today)
            var overdueContracts = await context.PawnContracts
                .Include(p => p.Customer)
                .Where(p => p.Status == Models.Enums.ContractStatus.Active && p.EndDate.Date < today)
                .ToListAsync();

            foreach (var item in overdueContracts)
            {
                string title = $"⚠️ Hợp đồng {item.ContractCode} đã quá hạn!";
                bool exists = await context.Notifications.AnyAsync(n => n.Title == title);
                if (!exists)
                {
                    context.Notifications.Add(new Notification
                    {
                        Title = title,
                        Message = $"Hợp đồng {item.ContractCode} (Khách: {item.Customer?.FullName}) đã quá hạn từ ngày {item.EndDate:dd/MM/yyyy}. Cần xử lý đóng lãi hoặc thanh lý!",
                        ContractId = item.Id,
                        Url = $"/PawnContracts/Details/{item.Id}",
                        IsRead = false,
                        CreatedDate = now
                    });
                }
            }

            // 2. Kiểm tra hợp đồng sắp đến hạn (trong vòng 3 ngày tới: today <= EndDate.Date <= today + 3 ngày)
            var dueSoonContracts = await context.PawnContracts
                .Include(p => p.Customer)
                .Where(p => p.Status == Models.Enums.ContractStatus.Active && p.EndDate.Date >= today && p.EndDate.Date <= today.AddDays(3))
                .ToListAsync();

            foreach (var item in dueSoonContracts)
            {
                int daysLeft = (item.EndDate.Date - today).Days;
                string dueStr = daysLeft == 0 ? "hôm nay" : $"sau {daysLeft} ngày nữa ({item.EndDate:dd/MM/yyyy})";
                string title = $"⏰ Hợp đồng {item.ContractCode} sắp hết hạn ({dueStr})!";
                
                bool exists = await context.Notifications.AnyAsync(n => n.Title == title);
                if (!exists)
                {
                    context.Notifications.Add(new Notification
                    {
                        Title = title,
                        Message = $"Hợp đồng {item.ContractCode} của khách hàng {item.Customer?.FullName} sẽ hết hạn {dueStr}. Vui lòng nhắc khách đóng lãi hoặc gia hạn!",
                        ContractId = item.Id,
                        Url = $"/PawnContracts/Details/{item.Id}",
                        IsRead = false,
                        CreatedDate = now
                    });
                }
            }

            // 3. Nếu chưa có thông báo nào trong hệ thống, tự động tạo 1 thông báo mẫu chào mừng
            if (!await context.Notifications.AnyAsync())
            {
                context.Notifications.Add(new Notification
                {
                    Title = "🎉 Chào mừng đến với Phần Mềm Cầm Đồ!",
                    Message = "Hệ thống đã kích hoạt tính năng thông báo tự động. Các hợp đồng sắp hết hạn và quá hạn sẽ hiển thị tại đây.",
                    Url = "/PawnContracts",
                    IsRead = false,
                    CreatedDate = now
                });
            }

            await context.SaveChangesAsync();
        }

        // GET: api/NotificationsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        // PUT: api/NotificationsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotification(int id, Notification notification)
        {
            if (id != notification.Id)
            {
                return BadRequest();
            }

            context.Entry(notification).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/NotificationsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetNotification", new { id = notification.Id }, notification);
        }

        // DELETE: api/NotificationsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool NotificationExists(int id)
        {
            return context.Notifications.Any(e => e.Id == id);
        }
    }
}
