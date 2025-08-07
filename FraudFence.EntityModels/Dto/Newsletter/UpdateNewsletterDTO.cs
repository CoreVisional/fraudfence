namespace FraudFence.EntityModels.Dto.Newsletter
{
    public record UpdateNewsletterDTO(string Subject, string? IntroText, DateTime ScheduledAt, List<int> SelectedArticleIds);
}
