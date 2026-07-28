using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Models.Enums;
using PhanMemCamDo.Services;

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

            // Ưu tiên lấy các thông báo chưa đọc, nếu không có thì lấy 5 thông báo gần nhất
            var unreadQuery = context.Notifications.Where(n => !n.IsRead);
            var unreadCount = await unreadQuery.CountAsync();

            List<Notification> rawList;
            if (unreadCount > 0)
            {
                rawList = await unreadQuery.OrderByDescending(n => n.CreatedDate).Take(20).ToListAsync();
            }
            else
            {
                rawList = await context.Notifications.OrderByDescending(n => n.CreatedDate).Take(5).ToListAsync();
            }

            var unreadList = rawList.Select(n => {
                string url = "/PawnContracts";
                if (!string.IsNullOrEmpty(n.Url))
                {
                    url = n.Url;
                }
                else if (n.ContractId.HasValue)
                {
                    url = $"/PawnContracts/Details/{n.ContractId}";
                }
                else if (!string.IsNullOrEmpty(n.Title))
                {
                    var match = NotificationRegex.MatchTitle(n.Title);
                    if (match.Success)
                    {
                        url = $"/PawnContracts?searchString={match.Value}";
                    }
                }

                return new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    Url = url,
                    CreatedDate = n.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                };
            }).ToList();

            return Ok(new {
                unreadCount,
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
                .Where(p => p.Status == ContractStatus.Active && p.EndDate.Date < today)
                .ToListAsync();

            foreach (var item in overdueContracts)
            {
                string code = item.ContractCode ?? "";
                string title = $"⚠️ Hợp đồng {item.ContractCode} đã quá hạn!";
                string message = $"Hợp đồng {item.ContractCode} (Khách: {item.Customer?.FullName}) đã quá hạn từ ngày {item.EndDate:dd/MM/yyyy}. Cần xử lý đóng lãi hoặc thanh lý!";

                var existing = await context.Notifications.FirstOrDefaultAsync(n => n.Title != null && code != "" && n.Title.Contains(code));
                if (existing != null)
                {
                    existing.Title = title;
                    existing.Message = message;
                }
                else
                {
                    context.Notifications.Add(new Notification
                    {
                        Title = title,
                        Message = message,
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
                .Where(p => p.Status == ContractStatus.Active && p.EndDate.Date >= today && p.EndDate.Date <= today.AddDays(3))
                .ToListAsync();

            foreach (var item in dueSoonContracts)
            {
                string code = item.ContractCode ?? "";
                int daysLeft = (item.EndDate.Date - today).Days;
                string dueStr = daysLeft == 0 ? "hôm nay" : $"sau {daysLeft} ngày nữa ({item.EndDate:dd/MM/yyyy})";
                string title = $"⏰ Hợp đồng {item.ContractCode} sắp hết hạn ({dueStr})!";
                string message = $"Hợp đồng {item.ContractCode} của khách hàng {item.Customer?.FullName} sẽ hết hạn {dueStr}. Vui lòng nhắc khách đóng lãi hoặc gia hạn!";

                var existing = await context.Notifications.FirstOrDefaultAsync(n => n.Title != null && code != "" && n.Title.Contains(code));
                if (existing != null)
                {
                    existing.Title = title;
                    existing.Message = message;
                }
                else
                {
                    context.Notifications.Add(new Notification
                    {
                        Title = title,
                        Message = message,
                        ContractId = item.Id,
                        Url = $"/PawnContracts/Details/{item.Id}",
                        IsRead = false,
                        CreatedDate = now
                    });
                }
            }

            // 3. Dọn dẹp các thông báo trùng lặp cũ của cùng một hợp đồng
            var allNotifications = await context.Notifications.ToListAsync();
            var grouped = allNotifications
                .Where(n => !string.IsNullOrEmpty(n.Title))
                .GroupBy(n => {
                    var match = NotificationRegex.MatchTitle(n.Title!);
                    return match.Success ? match.Value : n.Title!;
                })
                .Where(g => g.Count() > 1);

            foreach (var group in grouped)
            {
                var duplicates = group.OrderByDescending(n => n.CreatedDate).Skip(1);
                context.Notifications.RemoveRange(duplicates);
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
