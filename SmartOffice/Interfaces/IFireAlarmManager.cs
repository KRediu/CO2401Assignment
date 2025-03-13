namespace TDDAssignment.Interfaces
{
    public interface IFireAlarmManager // Fire alarm manager interface
    {
        string GetStatus(); // status log method, returns strings of OK or FAULT

        // alarm set method
        bool SetAlarm(bool isActive);
    }
}