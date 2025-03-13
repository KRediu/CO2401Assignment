namespace TDDAssignment.Interfaces
{
    public interface IWebService // Web service interface
    {
        // transition logging method, unused
        void LogStateChange(string message);

        // fault logging method
        void LogEngineerRequired(string message);

        // fire alarm logging method
        void LogFireAlarm(string message);
    }
}