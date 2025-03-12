namespace TDDAssignment.Interfaces
{
    public interface IFireAlarmManager
    {
        string GetStatus();

        bool SetAlarm(bool isActive);
    }
}