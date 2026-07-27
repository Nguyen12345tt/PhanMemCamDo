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
    public class PawnContractsApiController : ControllerBase
    {
        private readonly PawnShopDbContext _context;

        public PawnContractsApiController(PawnShopDbContext context)
        {
            _context = context;
        }

        // GET: api/PawnContractsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PawnContract>>> GetPawnContracts()
        {
            return await _context.PawnContracts.ToListAsync();
        }

        // GET: api/PawnContractsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PawnContract>> GetPawnContract(int id)
        {
            var pawnContract = await _context.PawnContracts.FindAsync(id);

            if (pawnContract == null)
            {
                return NotFound();
            }

            return pawnContract;
        }

        // PUT: api/PawnContractsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPawnContract(int id, PawnContract pawnContract)
        {
            if (id != pawnContract.Id)
            {
                return BadRequest();
            }

            _context.Entry(pawnContract).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PawnContractExists(id))
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

        // POST: api/PawnContractsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PawnContract>> PostPawnContract(PawnContract pawnContract)
        {
            _context.PawnContracts.Add(pawnContract);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPawnContract", new { id = pawnContract.Id }, pawnContract);
        }

        // DELETE: api/PawnContractsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePawnContract(int id)
        {
            var pawnContract = await _context.PawnContracts.FindAsync(id);
            if (pawnContract == null)
            {
                return NotFound();
            }

            _context.PawnContracts.Remove(pawnContract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PawnContractExists(int id)
        {
            return _context.PawnContracts.Any(e => e.Id == id);
        }
    }
}
