namespace TDDAssignment.Interfaces
{
    public interface IEmailService
    {
        void SendMail(string emailAddress, string subject, string message);
    }
}