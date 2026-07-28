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
    public class SystemConfigsApiController(PawnShopDbContext context) : ControllerBase
    {
        // GET: api/SystemConfigsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemConfig>>> GetSystemConfigs()
        {
            return await context.SystemConfigs.ToListAsync();
        }

        // GET: api/SystemConfigsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SystemConfig>> GetSystemConfig(int id)
        {
            var systemConfig = await context.SystemConfigs.FindAsync(id);

            if (systemConfig == null)
            {
                return NotFound();
            }

            return systemConfig;
        }

        // PUT: api/SystemConfigsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSystemConfig(int id, SystemConfig systemConfig)
        {
            if (id != systemConfig.Id)
            {
                return BadRequest();
            }

            context.Entry(systemConfig).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemConfigExists(id))
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

        // POST: api/SystemConfigsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SystemConfig>> PostSystemConfig(SystemConfig systemConfig)
        {
            context.SystemConfigs.Add(systemConfig);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetSystemConfig", new { id = systemConfig.Id }, systemConfig);
        }

        // DELETE: api/SystemConfigsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystemConfig(int id)
        {
            var systemConfig = await context.SystemConfigs.FindAsync(id);
            if (systemConfig == null)
            {
                return NotFound();
            }

            context.SystemConfigs.Remove(systemConfig);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool SystemConfigExists(int id)
        {
            return context.SystemConfigs.Any(e => e.Id == id);
        }
    }
}
