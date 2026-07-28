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
    public class CashFlowsApiController(PawnShopDbContext context) : ControllerBase
    {
        // GET: api/CashFlowsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CashFlow>>> GetCashFlows()
        {
            return await context.CashFlows.ToListAsync();
        }

        // GET: api/CashFlowsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CashFlow>> GetCashFlow(int id)
        {
            var cashFlow = await context.CashFlows.FindAsync(id);

            if (cashFlow == null)
            {
                return NotFound();
            }

            return cashFlow;
        }

        // PUT: api/CashFlowsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCashFlow(int id, CashFlow cashFlow)
        {
            if (id != cashFlow.Id)
            {
                return BadRequest();
            }

            context.Entry(cashFlow).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CashFlowExists(id))
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

        // POST: api/CashFlowsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CashFlow>> PostCashFlow(CashFlow cashFlow)
        {
            context.CashFlows.Add(cashFlow);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetCashFlow", new { id = cashFlow.Id }, cashFlow);
        }

        // DELETE: api/CashFlowsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCashFlow(int id)
        {
            var cashFlow = await context.CashFlows.FindAsync(id);
            if (cashFlow == null)
            {
                return NotFound();
            }

            context.CashFlows.Remove(cashFlow);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool CashFlowExists(int id)
        {
            return context.CashFlows.Any(e => e.Id == id);
        }
    }
}
