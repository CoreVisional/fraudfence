using FraudFence.Data;
using FraudFence.EntityModels.Models;
using FraudFence.Service.Common;

namespace FraudFence.Service
{
    public class ArticleService : BaseService<Article>
    {
        public ArticleService(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task UpdateAsync(Article entity)
        {
            var _article = GetById(entity.Id);

            if (_article == null)
            {
                return;
            }

            _article.Title = entity.Title;
            _article.ScamCategoryId = entity.ScamCategoryId;
            _article.Content = entity.Content;
            _article.LastModified = entity.LastModified;

            _context.Articles.Update(_article);

            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAsync(Article entity)
        {
            bool isScheduled = _context.Newsletters
                .Where(n => n.SentAt == null)
                .SelectMany(n => n.Articles)
                .Any(a => a.Id == entity.Id);

            if (isScheduled)
                throw new InvalidOperationException("Article is scheduled in a pending newsletter.");

            await DisableAsync(entity);
        }
    }
}
