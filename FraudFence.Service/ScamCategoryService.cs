using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FraudFence.Service
{
    public class ScamCategoryService : BaseService<ScamCategory>
    {
        public ScamCategoryService(ApplicationDbContext context) : base(context) { }

        public override async Task UpdateAsync(ScamCategory entity)
        {
            var inDb = await _context.ScamCategories.FindAsync(entity.Id);
            if (inDb == null) return;
            inDb.Name = entity.Name;
            inDb.ParentCategoryId = entity.ParentCategoryId;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ScamCategory>> GetAllWithParentAsync(string search = null, bool? showDisabled = null)
        {
            var query = _context.ScamCategories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));
            if (showDisabled.HasValue)
                query = query.Where(c => c.IsDisabled == showDisabled.Value);
            return await query.Include(c => c.ParentCategory).OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<ScamCategory>> GetAllAsync(bool getDisabled = false)
        {
            return await GetAll(getDisabled).ToListAsync();
        }

        public async Task<ScamCategory?> GetByIdAsync(int id)
        {
            return await _context.ScamCategories.FindAsync(id);
        }
    }
} 