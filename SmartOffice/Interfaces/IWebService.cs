namespace TDDAssignment.Interfaces
{
    public interface IWebService
    {
        void LogStateChange(string message);

        void LogEngineerRequired(string message);

        void LogFireAlarm(string message);
    }
}