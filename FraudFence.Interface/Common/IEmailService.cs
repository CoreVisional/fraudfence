namespace FraudFence.Interface.Common
{
    public interface IEmailService
    {
        Task SendEmail(string subject, string[] sendTo, string body);
    }
}
