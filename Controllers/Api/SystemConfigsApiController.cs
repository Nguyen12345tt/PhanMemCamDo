using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanMemCamDo.Data;
using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Controllers_Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemConfigsApiController : ControllerBase
    {
        private readonly PawnShopDbContext _context;

        public SystemConfigsApiController(PawnShopDbContext context)
        {
            _context = context;
        }

        // GET: api/SystemConfigsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemConfig>>> GetSystemConfigs()
        {
            return await _context.SystemConfigs.ToListAsync();
        }

        // GET: api/SystemConfigsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SystemConfig>> GetSystemConfig(int id)
        {
            var systemConfig = await _context.SystemConfigs.FindAsync(id);

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

            _context.Entry(systemConfig).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
            _context.SystemConfigs.Add(systemConfig);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSystemConfig", new { id = systemConfig.Id }, systemConfig);
        }

        // DELETE: api/SystemConfigsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystemConfig(int id)
        {
            var systemConfig = await _context.SystemConfigs.FindAsync(id);
            if (systemConfig == null)
            {
                return NotFound();
            }

            _context.SystemConfigs.Remove(systemConfig);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SystemConfigExists(int id)
        {
            return _context.SystemConfigs.Any(e => e.Id == id);
        }
    }
}
