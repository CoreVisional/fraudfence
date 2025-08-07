using FraudFence.Data;
using FraudFence.EntityModels.Common;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Service.Common
{
    public abstract class BaseService<T> where T : BaseEntity<int>
    {
        protected readonly ApplicationDbContext _context;

        protected BaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual IQueryable<T> GetAll(bool getDisabled = false)
        {
            var set = _context.Set<T>().AsQueryable();

            return getDisabled ? set : set.Where(e => !e.IsDisabled);
        }

        public virtual T? GetById(int id)
        {
            return _context.Set<T>().FirstOrDefault(x => x.Id == id);
        }

        public virtual async Task AddAsync(T entity)
        {
            _context.Add(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var toRemove = await _context.Set<T>().Where(e => ids.Contains(e.Id)).ToListAsync();

            if (toRemove.Count > 0)
            {
                _context.RemoveRange(toRemove);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task DisableAsync(T entity)
        {
            var inDb = await _context.Set<T>().FindAsync(entity.Id);

            if (inDb is null || inDb.IsDisabled) return;

            inDb.IsDisabled = true;

            await _context.SaveChangesAsync();
        }

        public virtual async Task EnableAsync(T entity)
        {
            var inDb = await _context.Set<T>().FindAsync(entity.Id);

            if (inDb is null || !inDb.IsDisabled) return;

            inDb.IsDisabled = false;

            await _context.SaveChangesAsync();
        }

        public virtual async Task DisableRangeAsync(IEnumerable<int> ids)
        {
            var batch = await _context.Set<T>().Where(e => ids.Contains(e.Id) && !e.IsDisabled).ToListAsync();

            if (batch.Count == 0) return;

            batch.ForEach(e => e.IsDisabled = true);

            _context.UpdateRange(batch);

            await _context.SaveChangesAsync();
        }

        public virtual async Task EnableRangeAsync(IEnumerable<int> ids)
        {
            var batch = await _context.Set<T>().Where(e => ids.Contains(e.Id) && e.IsDisabled).ToListAsync();

            if (batch.Count == 0) return;

            batch.ForEach(e => e.IsDisabled = false);

            _context.UpdateRange(batch);

            await _context.SaveChangesAsync();
        }

        public abstract Task UpdateAsync(T entity);
    }
}
