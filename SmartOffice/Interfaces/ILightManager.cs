namespace TDDAssignment.Interfaces
{
    public interface ILightManager
    {
        string GetStatus();

        bool SetLight(bool isON, int id);

        bool SetAllLights(bool isON);
    }
}