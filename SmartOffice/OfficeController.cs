using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDDAssignment
{
    public class OfficeController
    {
        private string officeID;
        private string currentState;
        private string previousState; // Keeps track of the last normal state

        // Constructor
        public OfficeController(string officeID)
        {
            SetOfficeID(officeID);
            InitializeState();
        }

        /* PRIVATE METHODS */

        // Initializes the state to "out of hours"
        private void InitializeState()
        {
            currentState = "out of hours";
        }

        /* SETTERS */

        // Setter for office ID
        public void SetOfficeID(string id)
        {
            officeID = id.ToLower(); // ensure that the capitalism works
        }

        // Setter for the state
        public bool SetCurrentState(string newState)
        {
            if (newState == null || string.IsNullOrWhiteSpace(newState))
            {
                return false; // Prevent null from causing an exception
            }

            string lowerNewState = newState.Trim().ToLower();
            string lowerCurrentState = GetCurrentState().ToLower();

            // Valid states
            string[] validStates = { "open", "closed", "out of hours", "fire alarm", "fire drill" };

            if (!Array.Exists(validStates, state => state == lowerNewState))
            {
                return false; // Invalid state
            }

            // Handle state transitions
            switch (lowerNewState)
            {
                case "open":
                    if (lowerCurrentState == "out of hours" || lowerCurrentState == "open" || lowerCurrentState == "fire alarm" || lowerCurrentState == "fire drill")
                    {
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
                        previousState = currentState; // Save last normal state
                        currentState = "fire alarm";
                        return true;
                    }
                    break;

                case "fire drill":
                    if (lowerCurrentState == "open" || lowerCurrentState == "fire drill" || lowerCurrentState == "out of hours")
                    {
                        previousState = currentState; // Save last normal state
                        currentState = "fire drill";
                        return true;
                    }
                    break;

                default:
                    // Handle returning from fire alarm or fire drill
                    if (lowerCurrentState == "fire alarm" || lowerCurrentState == "fire drill")
                    {
                        if (lowerNewState == previousState && previousState != "fire drill") // Restrict fire drill returns
                        {
                            currentState = previousState;
                            return true;
                        }
                        return false; // Prevent invalid transitions
                    }
                    break;
            }

            return false; // If no valid transition was found
        }

        /* GETTERS */

        // Getter for office ID
        public string GetOfficeID()
        {
            return officeID;
        }

        // Getter for office state
        public string GetCurrentState()
        {
            return currentState;
        }
    }
}
