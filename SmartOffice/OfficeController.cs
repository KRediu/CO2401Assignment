using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDDAssignment.Interfaces;

namespace TDDAssignment
{
    public class OfficeController
    {
        private string officeID;
        private string currentState;
        private string previousState; // Keeps track of the last normal state
        private readonly ILightManager lightManager;
        private readonly IFireAlarmManager fireAlarmManager;
        private readonly IDoorManager doorManager;
        private readonly IWebService webService;
        private readonly IEmailService emailService;

        // Constructor for default initialization
        public OfficeController(string officeID)
        {
            SetOfficeID(officeID);
            InitializeState();
        }

        // LVL2: Constructor with initial state validation
        public OfficeController(string id, string startState)
        {
            SetOfficeID(id);
            string lowerState = startState.Trim().ToLower();
            string[] validStates = { "open", "closed", "out of hours" };

            if (!Array.Exists(validStates, state => state == lowerState))
            {
                throw new ArgumentException("Argument Exception: OfficeController can only be initialised to the following states 'open', 'closed', 'out of hours'");
            }
            currentState = lowerState;
        }

        // LVL2: Constructor with dependency injection
        public OfficeController(string id, ILightManager iLightManager, IFireAlarmManager iFireAlarmManager, IDoorManager iDoorManager, IWebService iWebService, IEmailService iEmailService)
        {
            SetOfficeID(id);
            InitializeState();
            lightManager = iLightManager;
            fireAlarmManager = iFireAlarmManager;
            doorManager = iDoorManager;
            webService = iWebService;
            emailService = iEmailService;
        }

        /* PRIVATE METHODS */
        private void InitializeState()
        {
            currentState = "out of hours";
        }

        /* SETTERS */
        public void SetOfficeID(string id)
        {
            officeID = id.ToLower();
        }

        public bool SetCurrentState(string newState)
        {
            if (newState == null || string.IsNullOrWhiteSpace(newState))
            {
                return false;
            }

            string lowerNewState = newState.Trim().ToLower();
            string lowerCurrentState = GetCurrentState().ToLower();
            string[] validStates = { "open", "closed", "out of hours", "fire alarm", "fire drill" };

            if (!Array.Exists(validStates, state => state == lowerNewState))
            {
                return false;
            }

            switch (lowerNewState)
            {
                case "open":
                    if (lowerCurrentState == "out of hours" || lowerCurrentState == "open" || lowerCurrentState == "fire alarm" || lowerCurrentState == "fire drill")
                    {
                        if (doorManager != null && !doorManager.OpenAllDoors())
                        {
                            return false;
                        }
                        currentState = "open";
                        return true;
                    }
                    break;
                case "out of hours":
                    if (lowerCurrentState == "open" || lowerCurrentState == "closed" || lowerCurrentState == "fire alarm" || lowerCurrentState == "out of hours")
                    {
                        currentState = "out of hours";
                        return true;
                    }
                    break;
                case "closed":
                    if (lowerCurrentState == "closed" || lowerCurrentState == "out of hours" || lowerCurrentState == "fire alarm")
                    {
                        currentState = "closed";
                        return true;
                    }
                    else return false;
                case "fire alarm":
                    if (lowerCurrentState == "open" || lowerCurrentState == "closed" || lowerCurrentState == "out of hours" || lowerCurrentState == "fire alarm")
                    {
                        previousState = currentState;
                        currentState = "fire alarm";
                        return true;
                    }
                    break;
                case "fire drill":
                    if (lowerCurrentState == "open" || lowerCurrentState == "fire drill" || lowerCurrentState == "out of hours")
                    {
                        previousState = currentState;
                        currentState = "fire drill";
                        return true;
                    }
                    break;
                default:
                    if (lowerCurrentState == "fire alarm" || lowerCurrentState == "fire drill")
                    {
                        if (lowerNewState == previousState && previousState != "fire drill")
                        {
                            currentState = previousState;
                            return true;
                        }
                        return false;
                    }
                    break;
            }
            return false;
        }

        /* LVL2 FUNCTIONALITY */
        public string GetStatusReport()
        {
            if (lightManager == null || fireAlarmManager == null || doorManager == null)
            {
                throw new InvalidOperationException("OfficeController is missing dependencies");
            }
            return lightManager.GetStatus() + doorManager.GetStatus() + fireAlarmManager.GetStatus();
        }

        /* GETTERS */
        public string GetOfficeID()
        {
            return officeID;
        }

        public string GetCurrentState()
        {
            return currentState;
        }
    }
}