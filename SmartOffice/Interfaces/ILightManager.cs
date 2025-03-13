namespace TDDAssignment.Interfaces
{
    public interface ILightManager // Light manager interface
    {
        string GetStatus(); // status log method, returns strings of OK or FAULT

        // individual light set method, unused
        bool SetLight(bool isON, int id);

        // all lights set method
        bool SetAllLights(bool isON);
    }
}