namespace TDDAssignment.Interfaces
{
    public interface IDoorManager // Door manager interface
    {
        string GetStatus(); // status log method, returns strings of OK or FAULT

        // open/lock all doors methods
        bool OpenAllDoors(); 
        bool LockAllDoors();

        // Individual open/lock method, unused
        bool OpenDoor(int id);
        bool LockDoor(int id);
    }
}