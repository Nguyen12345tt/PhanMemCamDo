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
    public class NotificationsApiController : ControllerBase
    {
        private readonly PawnShopDbContext _context;

        public NotificationsApiController(PawnShopDbContext context)
        {
            _context = context;
        }

        // GET: api/NotificationsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await _context.Notifications.ToListAsync();
        }

        // GET: api/NotificationsApi/unread
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            await AutoCheckContractNotifications();

            var unreadList = await _context.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(20)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    CreatedDate = n.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications.CountAsync(n => !n.IsRead);

            return Ok(new {
                unreadCount = unreadCount,
                items = unreadList
            });
        }

        // POST: api/NotificationsApi/mark-as-read/5
        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // POST: api/NotificationsApi/mark-all-read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unreadItems = await _context.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var item in unreadItems)
            {
                item.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task AutoCheckContractNotifications()
        {
            var now = DateTime.Now;
            var threeDaysLater = now.AddDays(3);

            // 1. Kiểm tra hợp đồng quá hạn
            var overdueContracts = await _context.PawnContracts
                .Include(p => p.Customer)
                .Where(p => p.Status == Models.Enums.ContractStatus.Active && p.EndDate < now)
                .ToListAsync();

            foreach (var item in overdueContracts)
            {
                string title = $"⚠️ Hợp đồng {item.ContractCode} đã quá hạn!";
                bool exists = await _context.Notifications.AnyAsync(n => n.Title == title);
                if (!exists)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = title,
                        Message = $"Hợp đồng {item.ContractCode} (Khách: {item.Customer?.FullName}) đã quá hạn từ ngày {item.EndDate:dd/MM/yyyy}. Cần xử lý đóng lãi hoặc thanh lý!",
                        IsRead = false,
                        CreatedDate = now
                    });
                }
            }

            // 2. Kiểm tra hợp đồng sắp đến hạn (trong 3 ngày tới)
            var dueSoonContracts = await _context.PawnContracts
                .Include(p => p.Customer)
                .Where(p => p.Status == Models.Enums.ContractStatus.Active && p.EndDate >= now && p.EndDate <= threeDaysLater)
                .ToListAsync();

            foreach (var item in dueSoonContracts)
            {
                string title = $"⏰ Hợp đồng {item.ContractCode} sắp đến hạn!";
                bool exists = await _context.Notifications.AnyAsync(n => n.Title == title);
                if (!exists)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Title = title,
                        Message = $"Hợp đồng {item.ContractCode} (Khách: {item.Customer?.FullName}) sẽ hết hạn vào ngày {item.EndDate:dd/MM/yyyy}.",
                        IsRead = false,
                        CreatedDate = now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // GET: api/NotificationsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

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

            _context.Entry(notification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNotification", new { id = notification.Id }, notification);
        }

        // DELETE: api/NotificationsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}
