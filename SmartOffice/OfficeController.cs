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
        private string officeID; // initialization ID for controller
        private string currentState; // office state variable
        private string previousState; // Keeps track of the last normal state - unused, only here to represent History node

        // mock object declarations - set readonly for dependency injection
        private readonly ILightManager lightManager;
        private readonly IFireAlarmManager fireAlarmManager;
        private readonly IDoorManager doorManager;
        private readonly IWebService webService;
        private readonly IEmailService emailService;

        // Constructor for default initialization - L1R1
        public OfficeController(string officeID)
        {
            SetOfficeID(officeID);
            currentState = "out of hours"; // initial state
        }

        // Constructor with initial state validation - L2R1
        public OfficeController(string id, string startState)
        {
            SetOfficeID(id);

            string lowerState = startState.Trim().ToLower(); // sets given state, cuts unnecessary whitespaces and sets to lowercase
            string[] validStates = { "open", "closed", "out of hours" }; // valid initial state list

            if (!Array.Exists(validStates, state => state == lowerState)) // If given state does not match any valid initial state, throw exception
            {
                throw new ArgumentException("Argument Exception: OfficeController can only be initialised to the following states 'open', 'closed', 'out of hours'");
            }
            currentState = lowerState; // sets state to given one
        }

        // Constructor with dependency injection - L2R2
        public OfficeController(string id, ILightManager iLightManager, IFireAlarmManager iFireAlarmManager, IDoorManager iDoorManager, IWebService iWebService, IEmailService iEmailService)
        {
            SetOfficeID(id);
            currentState = "out of hours";

            // initializes dependencies
            lightManager = iLightManager;
            fireAlarmManager = iFireAlarmManager;
            doorManager = iDoorManager;
            webService = iWebService;
            emailService = iEmailService;
        }

        // Setters

        // Lowercases and sets controller ID
        public void SetOfficeID(string id)
        {
            officeID = id.ToLower();
        }

        // Sets office controller state and transitions
        public bool SetCurrentState(string newState)
        {
            if (newState == null || string.IsNullOrWhiteSpace(newState)) // failsafe for null/empty strings or null value
            {
                return false;
            }

            string lowerNewState = newState.Trim().ToLower(); // lowercases input and trims unnecessary whitespaces
            string lowerCurrentState = GetCurrentState().ToLower(); // lowercases current state

            string[] validStates = { "open", "closed", "out of hours", "fire alarm", "fire drill" }; // valid state array

            if (!Array.Exists(validStates, state => state == lowerNewState)) // if given input doesn't match any valid state, return with failure
            {
                return false;
            }

            // switch for valid states
            switch (lowerNewState)
            {
                case "open": // open state
                    if (lowerCurrentState == "out of hours" || lowerCurrentState == "open" || lowerCurrentState == "fire alarm" || lowerCurrentState == "fire drill") // valid states to transition from, and self
                    {
                        // dependency call
                        if (doorManager != null && !doorManager.OpenAllDoors()) // transition only succeeds upon successful call to OpenAllDoors()
                        {
                            return false;
                        }

                        currentState = "open";
                        return true; // sets state, returns
                    }
                    break;
                case "out of hours": // out of hours state - default state
                    if (lowerCurrentState == "open" || lowerCurrentState == "closed" || lowerCurrentState == "fire alarm" || lowerCurrentState == "out of hours") // valid states to transition from, and self
                    {
                        currentState = "out of hours";
                        return true; // sets state, returns
                    }
                    break;
                case "closed": // closed state
                    if (lowerCurrentState == "closed" || lowerCurrentState == "out of hours" || lowerCurrentState == "fire alarm") // valid states to transition from, and self
                    {
                        // calls to dependencies - function proceeds even if no light manager is present
                        if (doorManager != null && !doorManager.LockAllDoors()) // transition only succeeds upon successful call to LockAllDoors()
                        {
                            return false;
                        }
                        lightManager?.SetAllLights(false);

                        currentState = "closed";
                        return true; // sets state, returns
                    }
                    else return false;
                case "fire alarm": // fire alarm state
                    if (lowerCurrentState == "open" || lowerCurrentState == "closed" || lowerCurrentState == "out of hours" || lowerCurrentState == "fire alarm") // valid states to transition from, and self
                    {
                        previousState = currentState; // saves previous state

                        // calls to dependencies - function will proceed regardless if the dependencies actually exist
                        doorManager?.OpenAllDoors(); // attempts to open all doors
                        fireAlarmManager?.SetAlarm(true); // attempts to set alarm on
                        lightManager?.SetAllLights(true); // attempts to set all lights on

                        try // exception handling for web service
                        {
                            webService.LogFireAlarm("fire alarm"); // attempts to log alarm
                        }
                        catch (Exception ex) // if there is any failure for logging, send an email to address below with exception message
                        {
                            // If LogFireAlarm throws an exception, send an email with the exception message.
                            emailService?.SendMail("citycouncil@preston.gov.uk", "failed to log alarm", ex.Message);
                        }

                        currentState = "fire alarm";
                        return true; // sets state, returns
                    }
                    break;
                case "fire drill": // fire drill state
                    if (lowerCurrentState == "open" || lowerCurrentState == "closed" || lowerCurrentState == "fire drill" || lowerCurrentState == "out of hours") // valid states to transition from, and self
                    {
                        previousState = currentState; // saves previous state

                        currentState = "fire drill";
                        return true; // sets state, returns
                    }
                    break;
                default: // failsafe
                    return false;
            }
            return false;
        }

        // Dependency status report function - gets individual status messages and combines into one
        public string GetStatusReport()
        {
            if (lightManager == null || fireAlarmManager == null || doorManager == null) // failure if basic manager dependencies do not exist, throw exception
            {
                throw new InvalidOperationException("OfficeController is missing dependencies");
            }

            // Initialize a list to collect faults
            var faults = new List<string>();

            // Check for faults and null strings, if any faults, add corresponding device names to the list
            if (lightManager.GetStatus() != null && lightManager.GetStatus().Contains("FAULT"))
            {
                faults.Add("Lights");
            }

            if (fireAlarmManager.GetStatus() != null && fireAlarmManager.GetStatus().Contains("FAULT"))
            {
                faults.Add("FireAlarm");
            }

            if (doorManager.GetStatus() != null && doorManager.GetStatus().Contains("FAULT"))
            {
                faults.Add("Doors");
            }

            // If faults exist, log them to the WebService
            if (faults.Count > 0)
            {
                string faultString = string.Join(",", faults) + ","; // Comma-separated fault list
                webService.LogEngineerRequired(faultString); // Log the faults
            }

            return lightManager.GetStatus() + doorManager.GetStatus() + fireAlarmManager.GetStatus(); // return conjoined messages
        }

        // Getters

        // Office ID getter function
        public string GetOfficeID()
        {
            return officeID;
        }

        // Office state getter function
        public string GetCurrentState()
        {
            return currentState;
        }
    }
}