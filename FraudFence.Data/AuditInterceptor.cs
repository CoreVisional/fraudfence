using FraudFence.EntityModels.common;
using FraudFence.Interface.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FraudFence.Data
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContext _userContext;

        public AuditInterceptor(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventData.Context);

            var _now = DateTime.Now;
            var _userId = _userContext.Id;

            foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = _now;
                    entry.Entity.CreatedBy = _userId;
                }

                entry.Entity.LastModified = _now;
                entry.Entity.ModifiedBy = _userId;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
