namespace TDDAssignment.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string emailAddress, string subject, string message);
    }
}