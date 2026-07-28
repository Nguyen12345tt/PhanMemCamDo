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
    public class ActionLogsApiController(PawnShopDbContext context) : ControllerBase
    {
        // GET: api/ActionLogsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActionLog>>> GetActionLogs()
        {
            return await context.ActionLogs.ToListAsync();
        }

        // GET: api/ActionLogsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActionLog>> GetActionLog(int id)
        {
            var actionLog = await context.ActionLogs.FindAsync(id);

            if (actionLog == null)
            {
                return NotFound();
            }

            return actionLog;
        }

        // PUT: api/ActionLogsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActionLog(int id, ActionLog actionLog)
        {
            if (id != actionLog.Id)
            {
                return BadRequest();
            }

            context.Entry(actionLog).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActionLogExists(id))
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

        // POST: api/ActionLogsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ActionLog>> PostActionLog(ActionLog actionLog)
        {
            context.ActionLogs.Add(actionLog);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetActionLog", new { id = actionLog.Id }, actionLog);
        }

        // DELETE: api/ActionLogsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActionLog(int id)
        {
            var actionLog = await context.ActionLogs.FindAsync(id);
            if (actionLog == null)
            {
                return NotFound();
            }

            context.ActionLogs.Remove(actionLog);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ActionLogExists(int id)
        {
            return context.ActionLogs.Any(e => e.Id == id);
        }
    }
}
