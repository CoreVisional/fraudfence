namespace FraudFence.EntityModels.Dto.Newsletter
{
    public record CreateNewsletterDTO(string Subject, string? IntroText, DateTime ScheduledAt, List<int> SelectedArticleIds);
}
