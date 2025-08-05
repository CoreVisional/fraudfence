using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FraudFence.Service
{
    public class ExternalAgencyService : BaseService<ExternalAgency>
    {
        public ExternalAgencyService(ApplicationDbContext context) : base(context) { }

        public override async Task UpdateAsync(ExternalAgency entity)
        {
            var inDb = await _context.ExternalAgencies.FindAsync(entity.Id);
            if (inDb == null) return;
            inDb.Name = entity.Name;
            inDb.Email = entity.Email;
            inDb.Phone = entity.Phone;
            await _context.SaveChangesAsync();
        }

        public async Task<List<ExternalAgency>> GetAllAsync(bool getDisabled = false)
        {
            return await GetAll(getDisabled).ToListAsync();
        }

        public async Task<ExternalAgency?> GetByIdAsync(int id)
        {
            return await _context.ExternalAgencies.FindAsync(id);
        }
    }
} 