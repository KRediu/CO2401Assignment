namespace TDDAssignment.Interfaces
{
    public interface IDoorManager
    {
        string GetStatus();

        bool OpenAllDoors();
        bool LockAllDoors();

        bool OpenDoor(int id);
        bool LockDoor(int id);
    }
}