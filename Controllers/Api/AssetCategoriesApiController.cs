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
    public class AssetCategoriesApiController : ControllerBase
    {
        private readonly PawnShopDbContext _context;

        public AssetCategoriesApiController(PawnShopDbContext context)
        {
            _context = context;
        }

        // GET: api/AssetCategoriesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetCategory>>> GetAssetCategories()
        {
            return await _context.AssetCategories.ToListAsync();
        }

        // GET: api/AssetCategoriesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetCategory>> GetAssetCategory(int id)
        {
            var assetCategory = await _context.AssetCategories.FindAsync(id);

            if (assetCategory == null)
            {
                return NotFound();
            }

            return assetCategory;
        }

        // PUT: api/AssetCategoriesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetCategory(int id, AssetCategory assetCategory)
        {
            if (id != assetCategory.Id)
            {
                return BadRequest();
            }

            _context.Entry(assetCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetCategoryExists(id))
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

        // POST: api/AssetCategoriesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AssetCategory>> PostAssetCategory(AssetCategory assetCategory)
        {
            _context.AssetCategories.Add(assetCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAssetCategory", new { id = assetCategory.Id }, assetCategory);
        }

        // DELETE: api/AssetCategoriesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetCategory(int id)
        {
            var assetCategory = await _context.AssetCategories.FindAsync(id);
            if (assetCategory == null)
            {
                return NotFound();
            }

            _context.AssetCategories.Remove(assetCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssetCategoryExists(int id)
        {
            return _context.AssetCategories.Any(e => e.Id == id);
        }
    }
}
