namespace FraudFence.EntityModels.Dto.Newsletter
{
    public record NewsletterDTO(int Id, string Subject, string? IntroText, DateTime ScheduledAt, DateTime? SentAt, bool IsDisabled);
}
