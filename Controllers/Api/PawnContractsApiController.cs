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
    public class PawnContractsApiController(PawnShopDbContext context) : ControllerBase
    {
        // GET: api/PawnContractsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PawnContract>>> GetPawnContracts()
        {
            return await context.PawnContracts.ToListAsync();
        }

        // GET: api/PawnContractsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PawnContract>> GetPawnContract(int id)
        {
            var pawnContract = await context.PawnContracts.FindAsync(id);

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

            context.Entry(pawnContract).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
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
            context.PawnContracts.Add(pawnContract);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetPawnContract", new { id = pawnContract.Id }, pawnContract);
        }

        // DELETE: api/PawnContractsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePawnContract(int id)
        {
            var pawnContract = await context.PawnContracts.FindAsync(id);
            if (pawnContract == null)
            {
                return NotFound();
            }

            context.PawnContracts.Remove(pawnContract);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool PawnContractExists(int id)
        {
            return context.PawnContracts.Any(e => e.Id == id);
        }
    }
}
