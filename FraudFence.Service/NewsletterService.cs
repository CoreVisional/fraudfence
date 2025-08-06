using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;

namespace FraudFence.Service
{
    public class NewsletterService : BaseService<Newsletter>
    {
        public NewsletterService(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task UpdateAsync(Newsletter entity)
        {
            var _newsletter = GetById(entity.Id);

            if (_newsletter == null)
            {
                return;
            }

            _newsletter.Subject = entity.Subject;
            _newsletter.IntroText = entity.IntroText;
            _newsletter.ScheduledAt = entity.ScheduledAt;
            _newsletter.LastModified = entity.LastModified;

            _context.Newsletters.Update(_newsletter);

            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Newsletter entity)
        {
            if (entity.SentAt.HasValue) throw new InvalidOperationException("Cannot delete a newsletter that has already been sent.");

            await DisableAsync(entity);
        }
    }
}