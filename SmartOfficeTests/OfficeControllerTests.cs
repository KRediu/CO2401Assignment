using TDDAssignment;
using System;
using NUnit.Framework;
using NSubstitute;
using TDDAssignment.Interfaces;
using NUnit.Framework.Legacy;

[TestFixture]
class OfficeControllerTests
{
    [TestFixture]
    public class Tests
    {
        // Template test
        [Test]
        public void TemplateTest()
        {
            Assert.Pass("Example Test Passed!");
        }

        // Constructor correctly sets officeID - L1R1
        [Test]
        public void L1R1_Constructor_SetsOfficeID()
        {
            // Arrange
            string officeID = "hello";

            // Act - initializes object with preset ID and retrieves it into another variable
            var controller = new OfficeController(officeID);
            string actualID = controller.GetOfficeID();

            // Assert - compares preset ID with initialized ID
            Assert.That(actualID, Is.EqualTo(officeID.ToLower()));
        }

        // officeID is converted to lowercase in the constructor - L1R2
        [Test]
        public void L1R2_Constructor_SetsInitialIDToLower()
        {
            // Arrange
            string officeID = "FULLUPPERCASE";

            // Act - initializes object with preset uppercase ID and retrieves it into another variable
            var controller = new OfficeController(officeID);
            string actualID = controller.GetOfficeID();

            // Assert - compares lowercased preset ID to initialized one
            Assert.That(actualID, Is.EqualTo(officeID.ToLower()));
        }

        // SetOfficeID() sets officeID (converts to lowercase) - L1R3
        [Test]
        public void L1R3_SetOfficeID_SetsNewIDToLowerCase()
        {
            // Arrange
            string initialOfficeID = "lowercase";
            string newOfficeID = "UPPERCASE";
            var controller = new OfficeController(initialOfficeID);

            // Act - set new uppercase ID into setter function
            controller.SetOfficeID(newOfficeID);
            string actualID = controller.GetOfficeID();

            // Assert - compares lowercased new ID to set one
            Assert.That(actualID, Is.EqualTo(newOfficeID.ToLower()));
        }

        // Constructor sets currentState to "out of hours" - L1R4
        [Test]
        public void L1R4_Constructor_SetsInitialStateToOutOfHours()
        {
            // Arrange
            string officeID = "hello";
            string expectedState = "out of hours";

            // Act - initialize object and retrieve current state into variable
            var controller = new OfficeController(officeID);
            string actualState = controller.GetCurrentState();

            // Assert - compares variable and set state
            Assert.That(actualState, Is.EqualTo(expectedState));
        }

        // SetCurrentState() handles valid and invalid states - L1R5
        [Test]
        [TestCase("open", true)] // Valid state
        [TestCase("closed", true)] // Valid state
        [TestCase("out of hours", true)] // Valid state
        [TestCase("fire drill", true)] // Valid state
        [TestCase("fire alarm", true)] // Valid state
        [TestCase("  open  ", true)]  // Valid state - Leading/trailing spaces
        [TestCase("OpEn", true)]      // Valid state - Mixed case
        [TestCase("FIRE ALARM", true)] // Valid state - Uppercase
        [TestCase("invalid", false)] // Invalid state
        [TestCase("not a state", false)] // Invalid state
        [TestCase("", false)] // Invalid state - empty string
        [TestCase(null, false)] // Invalid state - null
        [TestCase("    ", false)] // Invalid state - only spaces
        public void L1R5_SetCurrentState_ValidAndInvalidStates(string state, bool expectedResult)
        {
            // Arrange
            var controller = new OfficeController("hello");

            // Act - sets test case and returns success/failure as variable
            bool result = controller.SetCurrentState(state);

            // Assert - checks if set was successful
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        // State transitions follow the rules - L1R6    
        [Test]
        [TestCase("open", "out of hours", true)] // Valid transition
        [TestCase("closed", "out of hours", true)] // Valid transition
        [TestCase("open", "fire drill", true)] // Valid transition
        [TestCase("open", "fire alarm", true)] // Valid transition
        [TestCase("fire drill", "open", true)] // Valid transition
        [TestCase("fire alarm", "out of hours", true)] // Valid transition
        [TestCase("closed", "fire alarm", true)] // Valid transition
        [TestCase("closed", "open", false)] // Invalid transition - Cannot go from closed to open directly
        [TestCase("fire alarm", "fire drill", false)] // Invalid transition - Cannot switch from fire alarm to fire drill
        [TestCase("closed", "random state", false)] // Invalid transition - Invalid state
        [TestCase("fire alarm", " ", false)] // Invalid transition - Empty state string
        [TestCase("open", "", false)] // Invalid transition - Null/empty input
        [TestCase("out of hours", null, false)] // Invalid transition - Null input
        public void L1R6_SetCurrentState_FollowsStateTransitionRules(string initialState, string newState, bool expectedResult)
        {
            // Arrange
            var controller = new OfficeController("hello");
            controller.SetCurrentState(initialState);

            // Act - return transition success/failure in variable
            bool result = controller.SetCurrentState(newState);

            // Assert - check if result matches case
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        // SetCurrentState() returns true if the state is unchanged - L1R7
        [Test]
        public void L1R7_SetCurrentState_ReturnsTrueIfStateUnchanged()
        {
            // Arrange
            var controller = new OfficeController("hello");
            string currentState = controller.GetCurrentState();

            // Act - send identical state to setter
            bool result = controller.SetCurrentState(currentState);

            // Assert - check is set was successful and state is the same as before
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo(currentState));
        }

        // Second constructor initializes with valid state - L2R1
        [Test]
        public void L2R1_Constructor_ValidInitialState()
        {
            // Arrange
            string startState = "open";

            // Act - initialize object with variable valid state
            var controller = new OfficeController("hello", startState);

            // Assert - check if current state matches variable
            Assert.That(controller.GetCurrentState(), Is.EqualTo(startState.ToLower()));
        }

        // Second constructor throws exception with invalid state - L2R1
        [Test]
        public void L2R1_Constructor_InvalidInitialState_ThrowsException()
        {
            // Arrange
            string startState = "invalidState";

            // Act - retrieve exception message
            var ex = Assert.Throws<ArgumentException>(() => new OfficeController("hello", startState));

            // Assert - check that message matches expected one
            Assert.That(ex.Message, Is.EqualTo("Argument Exception: OfficeController can only be initialised to the following states 'open', 'closed', 'out of hours'"));
        }

        // Third constructor with dependency injection - L2R2
        [Test]
        public void L2R2_Constructor_InitializesWithDependencies()
        {
            // Arrange - create mock objects for dependencies using NSubstitute
            var mockLightManager = Substitute.For<ILightManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockWebService = Substitute.For<IWebService>();
            var mockEmailService = Substitute.For<IEmailService>();

            // Act
            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, mockEmailService);

            // Assert - ensure object has been initialized, and check Received() methods for each mock object to ensure they exist
            Assert.That(controller.GetOfficeID(), Is.EqualTo("hello".ToLower()));

            mockLightManager.Received(0);
            mockFireAlarmManager.Received(0);
            mockDoorManager.Received(0);
            mockWebService.Received(0);
            mockEmailService.Received(0);
        }

        // GetStatusReport() aggregates status reports - L2R3
        [Test]
        public void L2R3_GetStatusReport_ReturnsFullStatus()
        {
            // Arrange - initialize related mock objects, and have them return simulated check messages
            var mockLightManager = Substitute.For<ILightManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockWebService = Substitute.For<IWebService>(); // initialized for necessity - test in L3

            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockDoorManager.GetStatus().Returns("Doors,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockFireAlarmManager.GetStatus().Returns("FireAlarm,OK,FAULT,OK,FAULT,OK,OK,OK,OK,OK,OK,");

            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string statusReport = controller.GetStatusReport();

            // Assert - ensure status report matches combined check messages, and that all status calls for checks or returns were made
            Assert.That(statusReport, Is.EqualTo("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,Doors,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,FireAlarm,OK,FAULT,OK,FAULT,OK,OK,OK,OK,OK,OK,"));

            mockLightManager.Received(3).GetStatus();
            mockDoorManager.Received(3).GetStatus();
            mockFireAlarmManager.Received(3).GetStatus();
        }

        // GetStatusReport() can handle null or empty strings for an object - L2R3
        [Test]
        public void L2R3_GetStatusReport_HandlesNullOrEmptyStatus()
        {
            // Arrange - initializes mock objects and has one return null string, one empty string, and one correct to be displayed
            var mockLightManager = Substitute.For<ILightManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();

            mockLightManager.GetStatus().Returns(string.Empty);
            mockDoorManager.GetStatus().Returns((string)null);
            mockFireAlarmManager.GetStatus().Returns("FireAlarm,OK,OK,OK,");

            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, null, null);

            // Act
            string statusReport = controller.GetStatusReport();

            // Assert - ensure status report matches valid strings and appropriate status calls for checks or returns were made
            Assert.That(statusReport, Is.EqualTo("FireAlarm,OK,OK,OK,"));

            mockLightManager.Received(3).GetStatus();
            mockDoorManager.Received(2).GetStatus(); // set to 2 calls for the null string one, as it will not check the second condition.
            mockFireAlarmManager.Received(3).GetStatus();
        }

        // GetStatusReport() throws exception if controller is missing necessary dependencies - L2R3
        [Test]
        public void L2R3_GetStatusReport_ThrowsExceptionIfMissingDependencies()
        {
            // Arrange - initialize third constructor but no dependencies
            var controller = new OfficeController("hello", null, null, null, null, null);

            // Act - retrieve exception message
            var ex = Assert.Throws<InvalidOperationException>(() => controller.GetStatusReport());

            // Assert - check that it matches expected message
            Assert.That(ex.Message, Is.EqualTo("OfficeController is missing dependencies"));
        }

        // SetCurrentState("open") fails if OpenAllDoors() fails - L2R4
        [Test]
        public void L2R4_SetCurrentState_Open_FailsIfDoorsDontOpen()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.OpenAllDoors().Returns(false); // Simulate failure to open doors

            var controller = new OfficeController("hello", null, null, mockDoorManager, null, null);

            // Act - attempt to set open state
            bool result = controller.SetCurrentState("open");

            // Assert - ensure set fails, state is the initial one, and that a call to relevant function was made
            Assert.That(result, Is.False);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("out of hours"));

            mockDoorManager.Received(1).OpenAllDoors();
        }

        // SetCurrentState("open") should call OpenAllDoors - L2R5
        [Test]
        public void L2R5_SetCurrentState_Open_CallsOpenAllDoors()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.OpenAllDoors().Returns(true); // Simulate success to open doors

            var controller = new OfficeController("hello", null, null, mockDoorManager, null, null);

            // Act - attempt to set open state
            bool result = controller.SetCurrentState("open");

            // Assert - ensure transition was successful, state is open, and that a call to the relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("open"));

            mockDoorManager.Received(1).OpenAllDoors();
        }

        // SetCurrentState("closed") fails if LockAllDoors() fails - L3R1
        [Test]
        public void L3R1_SetCurrentState_Closed_FailsIfDoorsDontLock()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.LockAllDoors().Returns(false); // Simulate failure to lock doors

            var controller = new OfficeController("hello", null, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("closed");

            // Assert - ensure set fails, state is the initial one, and that a call to relevant function was made
            Assert.That(result, Is.False);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("out of hours"));

            mockDoorManager.Received(1).LockAllDoors();
        }

        // SetCurrentState("closed") should call LockAllDoors() - L3R1
        [Test]
        public void L3R1_SetCurrentState_Closed_CallsLockAllDoors()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.LockAllDoors().Returns(true); // Simulate success to open doors

            var controller = new OfficeController("hello", null, null, mockDoorManager, null, null);

            // Act - attempt to set state to closed
            bool result = controller.SetCurrentState("closed");

            // Assert - ensure transition is successful, state is closed, and that a call to relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("closed"));

            mockDoorManager.Received(1).LockAllDoors();
        }

        // SetCurrentState("closed") should call SetAllLights(false) - L3R1
        [Test]
        public void L3R1_SetCurrentState_Closed_CallsSetAllLightsOff()
        {
            // Arrange - arrange object and return necessities
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockLightManager = Substitute.For<ILightManager>();
            mockDoorManager.LockAllDoors().Returns(true);

            var controller = new OfficeController("hello", mockLightManager, null, mockDoorManager, null, null);

            // Act - set state to closed
            bool result = controller.SetCurrentState("closed");

            // Assert - ensure transition is successful, state is closed, and that a call to relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("closed"));

            mockLightManager.Received(1).SetAllLights(false);
        }

        // SetCurrentState("fire alarm") should call OpenAllDoors(), but no failure regardless - L3R2
        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsOpenAllDoors()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();

            var controller = new OfficeController("hello", null, null, mockDoorManager, null, null);

            // Act - transition to fire alarm
            bool result = controller.SetCurrentState("fire alarm");

            // Assert - ensure transition is successful, state is fire alarm, and that a call to relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));

            mockDoorManager.Received(1).OpenAllDoors();
        }

        // SetCurrentState("fire alarm") should call SetAllLights(true) - L3R2
        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsSetAllLightsOn()
        {
            // Arrange
            var mockLightManager = Substitute.For<ILightManager>();

            var controller = new OfficeController("hello", mockLightManager, null, null, null, null);

            // Act - transition to fire alarm
            bool result = controller.SetCurrentState("fire alarm");

            // Assert - ensure transition is successful, state is fire alarm, and that a call to relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));

            mockLightManager.Received(1).SetAllLights(true);
        }

        // SetCurrentState("fire alarm") should call SetAlarm(true) - L3R2
        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsSetFireAlarmTrue()
        {
            // Arrange
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            var controller = new OfficeController("hello", null, mockFireAlarmManager, null, null, null);

            // Act - transition to fire alarm
            bool result = controller.SetCurrentState("fire alarm");

            // Assert - ensure transition is successful, state is fire alarm, and that a call to relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));

            mockFireAlarmManager.Received(1).SetAlarm(true);
        }

        // SetCurrentState("fire alarm") should call LogFireAlarm("fire alarm") - L3R2
        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsLogFireAlarm()
        {
            // Arrange
            var mockWebService = Substitute.For<IWebService>();

            var controller = new OfficeController("hello", null, null, null, mockWebService, null);

            // Act - transition to fire alarm
            bool result = controller.SetCurrentState("fire alarm");

            // Assert - ensure transition is successful, state is fire alarm, and that a call to relevant function was made
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));

            mockWebService.Received(1).LogFireAlarm("fire alarm");
        }
        
        // GetStatusReport() calls LogEngineerRequired() with light fault - L3R3
        [Test]
        public void L3R3_GetStatusReport_LogsLightFault()
        {
            // Arrange - initialize necessary mock objects and simulate light fault message
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert - ensure light fault is logged correctly
            mockWebService.Received(1).LogEngineerRequired("Lights,");
        }

        // GetStatusReport() calls LogEngineerRequired() with door fault - L3R3
        [Test]
        public void L3R3_GetStatusReport_LogsDoorFault()
        {
            // Arrange - initialize necessary mock objects and simulate door fault message
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockDoorManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert - ensure door fault is logged correctly
            mockWebService.Received(1).LogEngineerRequired("Doors,");
        }

        // GetStatusReport() calls LogEngineerRequired() with fire alarm fault - L3R3
        [Test]
        public void L3R3_GetStatusReport_LogsFireAlarmFault()
        {
            // Arrange - initialize necessary mock objects and simulate fire alarm fault message
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockFireAlarmManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert - ensure fire alarm fault is logged correctly
            mockWebService.Received(1).LogEngineerRequired("FireAlarm,");
        }

        // GetStatusReport() calls LogEngineerRequired() with multiple faults - L3R3
        [Test]
        public void L3R3_GetStatusReport_LogsMultipleFaults()
        {
            // Arrange - initialize necessary mock objects and simulate fault messages from all managers
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockDoorManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockFireAlarmManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("hello", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert - ensure all faults are logged correctly
            mockWebService.Received(1).LogEngineerRequired("Lights,FireAlarm,Doors,");
        }

        // SetCurrentState("fire alarm") sends mail with SendMail() upon no web service exception - L3R4
        [Test]
        public void L3R4_SetCurrentState_FireAlarm_SendsMailUponException_NoWebService()
        {
            // Arrange - initialize only email mock object, no web service
            var mockEmailService = Substitute.For<IEmailService>();

            var controller = new OfficeController("hello", null, null, null, null, mockEmailService);

            // Act - transition into fire alarm
            bool result = controller.SetCurrentState("fire alarm");

            // Assert - ensure transition was made, and that SendMail() was called with appropriate exception message
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));

            mockEmailService.Received(1).SendMail("citycouncil@preston.gov.uk", "failed to log alarm", "Object reference not set to an instance of an object.");
        }

        // SetCurrentState("fire alarm") sends mail with SendMail() upon any exception - L3R4
        [Test]
        public void L3R4_SetCurrentState_FireAlarm_SendsMailUponException_Any()
        {
            // Arrange - initialize necessities, and throw exception upon LogFireAlarm() call
            var mockEmailService = Substitute.For<IEmailService>();
            var mockWebService = Substitute.For<IWebService>();

            mockWebService.When(x => x.LogFireAlarm(Arg.Any<string>())).Do(x => throw new Exception("exception test"));

            var controller = new OfficeController("hello", null, null, null, mockWebService, mockEmailService);

            // Act - transition into fire alarm
            bool result = controller.SetCurrentState("fire alarm");

            // Assert - ensure transition was made, and that SendMail() was called with appropriate exception message
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));

            mockEmailService.Received(1).SendMail("citycouncil@preston.gov.uk", "failed to log alarm", "exception test");
        }
    }
}
