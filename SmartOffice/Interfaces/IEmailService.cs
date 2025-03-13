namespace TDDAssignment.Interfaces
{
    public interface IEmailService // email service interface
    {
        // e-mail sending method, needs an address, subject, and message
        void SendMail(string emailAddress, string subject, string message);
    }
}