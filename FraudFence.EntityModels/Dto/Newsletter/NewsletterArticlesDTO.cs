using FraudFence.EntityModels.Dto.Article;

namespace FraudFence.EntityModels.Dto.Newsletter
{
    public record NewsletterArticlesDTO(
        int Id,
        string Subject,
        string? IntroText,
        DateTime ScheduledAt,
        DateTime? SentAt,
        bool IsDisabled,
        IReadOnlyList<ArticleDTO> Articles
    );
}
