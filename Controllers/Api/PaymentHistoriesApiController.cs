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
    public class PaymentHistoriesApiController : ControllerBase
    {
        private readonly PawnShopDbContext _context;

        public PaymentHistoriesApiController(PawnShopDbContext context)
        {
            _context = context;
        }

        // GET: api/PaymentHistoriesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentHistory>>> GetPaymentHistories()
        {
            return await _context.PaymentHistories.ToListAsync();
        }

        // GET: api/PaymentHistoriesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentHistory>> GetPaymentHistory(int id)
        {
            var paymentHistory = await _context.PaymentHistories.FindAsync(id);

            if (paymentHistory == null)
            {
                return NotFound();
            }

            return paymentHistory;
        }

        // PUT: api/PaymentHistoriesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentHistory(int id, PaymentHistory paymentHistory)
        {
            if (id != paymentHistory.Id)
            {
                return BadRequest();
            }

            _context.Entry(paymentHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentHistoryExists(id))
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

        // POST: api/PaymentHistoriesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PaymentHistory>> PostPaymentHistory(PaymentHistory paymentHistory)
        {
            _context.PaymentHistories.Add(paymentHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaymentHistory", new { id = paymentHistory.Id }, paymentHistory);
        }

        // DELETE: api/PaymentHistoriesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentHistory(int id)
        {
            var paymentHistory = await _context.PaymentHistories.FindAsync(id);
            if (paymentHistory == null)
            {
                return NotFound();
            }

            _context.PaymentHistories.Remove(paymentHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentHistoryExists(int id)
        {
            return _context.PaymentHistories.Any(e => e.Id == id);
        }
    }
}
