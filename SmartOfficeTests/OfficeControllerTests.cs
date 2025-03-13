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
        // L1R1: Constructor sets officeID
        [Test]
        public void L1R1_Constructor_SetsOfficeID()
        {
            // Arrange
            string officeID = "hello";

            // Act
            var controller = new OfficeController(officeID);
            string actualID = controller.GetOfficeID();

            // Assert
            Assert.That(actualID, Is.EqualTo(officeID.ToLower()));
        }

        // L1R2: officeID is converted to lowercase in the constructor
        [Test]
        public void L1R2_Constructor_SetsInitialIDToLower()
        {
            // Arrange
            string officeID = "FULLUPPERCASE";

            // Act
            var controller = new OfficeController(officeID);
            string actualID = controller.GetOfficeID();

            // Assert
            Assert.That(actualID, Is.EqualTo(officeID.ToLower()));
        }

        // L1R3: SetOfficeID() sets officeID (converts to lowercase)
        [Test]
        public void L1R3_SetOfficeID_SetsNewIDToLowerCase()
        {
            // Arrange
            string initialOfficeID = "lowercase";
            string newOfficeID = "UPPERCASE";
            var controller = new OfficeController(initialOfficeID);

            // Act
            controller.SetOfficeID(newOfficeID);
            string actualID = controller.GetOfficeID();

            // Assert
            Assert.That(actualID, Is.EqualTo(newOfficeID.ToLower()));
        }

        // L1R4: Constructor sets currentState to "out of hours"
        [Test]
        public void L1R4_Constructor_SetsInitialStateToOutOfHours()
        {
            // Arrange
            string officeID = "hello";
            string expectedState = "out of hours";

            // Act
            var controller = new OfficeController(officeID);
            string actualState = controller.GetCurrentState();

            // Assert
            Assert.That(actualState, Is.EqualTo(expectedState));
        }

        // L1R5: SetCurrentState() handles valid and invalid states
        [Test]
        [TestCase("open", true)]
        [TestCase("closed", true)]
        [TestCase("out of hours", true)]
        [TestCase("fire drill", true)]
        [TestCase("fire alarm", true)]
        [TestCase("  open  ", true)]  // Leading/trailing spaces
        [TestCase("OpEn", true)]      // Mixed case
        [TestCase("FIRE ALARM", true)] // Uppercase

        [TestCase("invalid", false)]
        [TestCase("not a state", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("    ", false)]      // Only spaces
        public void L1R5_SetCurrentState_ValidAndInvalidStates(string state, bool expectedResult)
        {
            // Arrange
            var controller = new OfficeController("hello");

            // Act
            bool result = controller.SetCurrentState(state);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        // L1R6: State transitions follow the rules
        [Test]
        [TestCase("closed", "open", false)]          // Cannot go from closed to open directly
        [TestCase("fire drill", "closed", false)]    // Cannot go from fire drill to closed
        [TestCase("fire alarm", "fire drill", false)]// Cannot switch from fire alarm to fire drill
        [TestCase("closed", "random state", false)]  // Invalid state
        [TestCase("fire alarm", " ", false)]         // Empty state string
        [TestCase("open", "", false)]                // Null/empty input
        [TestCase("out of hours", null, false)]      // Null input
        [TestCase("closed", "fire drill", false)]    // Cannot go from closed to fire drill
        public void L1R6_SetCurrentState_FollowsStateTransitionRules(string initialState, string newState, bool expectedResult)
        {
            // Arrange
            var controller = new OfficeController("hello");
            controller.SetCurrentState(initialState);
            string statea = controller.GetCurrentState();

            // Act
            bool result = controller.SetCurrentState(newState);
            string state = controller.GetCurrentState();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        // L1R7: SetCurrentState() returns true if the state is unchanged
        [Test]
        public void L1R7_SetCurrentState_ReturnsTrueIfStateUnchanged()
        {
            // Arrange
            var controller = new OfficeController("hello");
            string currentState = controller.GetCurrentState();

            // Act
            bool result = controller.SetCurrentState(currentState);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo(currentState));
        }

        // L2R1: Constructor with initial state validation
        [Test]
        public void L2R1_Constructor_ValidInitialState()
        {
            // Arrange
            string officeID = "office123";
            string startState = "open";

            // Act
            var controller = new OfficeController(officeID, startState);

            // Assert
            Assert.That(controller.GetCurrentState(), Is.EqualTo(startState.ToLower()));
        }

        [Test]
        public void L2R1_Constructor_InvalidInitialState_ThrowsException()
        {
            // Arrange
            string officeID = "office123";
            string startState = "invalidState";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new OfficeController(officeID, startState));
            Assert.That(ex.Message, Is.EqualTo("Argument Exception: OfficeController can only be initialised to the following states 'open', 'closed', 'out of hours'"));
        }

        // L2R2: Constructor with dependency injection
        [Test]
        public void L2R2_Constructor_InitializesWithDependencies()
        {
            // Arrange
            var mockLightManager = Substitute.For<ILightManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockWebService = Substitute.For<IWebService>();
            var mockEmailService = Substitute.For<IEmailService>();
            string officeID = "office123";

            // Act
            var controller = new OfficeController(officeID, mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, mockEmailService);

            // Assert
            Assert.That(controller.GetOfficeID(), Is.EqualTo(officeID.ToLower()));
        }

        // L2R3: GetStatusReport() aggregates status reports
        [Test]
        public void L2R3_GetStatusReport_ReturnsFullStatus()
        {
            // Arrange
            var mockLightManager = Substitute.For<ILightManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockWebService = Substitute.For<IWebService>();

            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockDoorManager.GetStatus().Returns("Doors,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockFireAlarmManager.GetStatus().Returns("FireAlarm,OK,FAULT,OK,FAULT,OK,OK,OK,OK,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string statusReport = controller.GetStatusReport();

            // Assert
            Assert.That(statusReport, Is.EqualTo("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,Doors,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,FireAlarm,OK,FAULT,OK,FAULT,OK,OK,OK,OK,OK,OK,"));

            mockLightManager.Received(3).GetStatus();
            mockDoorManager.Received(3).GetStatus();
            mockFireAlarmManager.Received(3).GetStatus();
        }

        [Test]
        public void L2R3_GetStatusReport_HandlesNullOrEmptyStatus()
        {
            // Arrange
            var mockLightManager = Substitute.For<ILightManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();

            mockLightManager.GetStatus().Returns(string.Empty);
            mockDoorManager.GetStatus().Returns((string)null);
            mockFireAlarmManager.GetStatus().Returns("FireAlarm,OK,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, null, null);

            // Act
            string statusReport = controller.GetStatusReport();

            // Assert
            Assert.That(statusReport, Is.EqualTo("FireAlarm,OK,OK,OK,"));

            mockLightManager.Received(3).GetStatus();
            mockDoorManager.Received(2).GetStatus(); // set to 2 calls for the null string one, as it will not check the second condition.
            mockFireAlarmManager.Received(3).GetStatus();
        }

        public void L2R3_GetStatusReport_ThrowsExceptionIfMissingDependencies()
        {
            // Arrange
            var controller = new OfficeController("office123", null, null, null, null, null);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => controller.GetStatusReport());

            // Assert
            Assert.That(ex.Message, Is.EqualTo("OfficeController is missing dependencies"));
        }

        // L2R4: SetCurrentState("open") fails if OpenAllDoors() fails
        [Test]
        public void L2R4_SetCurrentState_Open_FailsIfDoorsDontOpen()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.OpenAllDoors().Returns(false); // Simulate failure to open doors

            var controller = new OfficeController("office123", null, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("open");

            // Assert
            Assert.That(result, Is.False);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("out of hours"));

            mockDoorManager.Received(1).OpenAllDoors();
        }

        // L2R5: SetCurrentState("open") should call OpenAllDoors
        [Test]
        public void L2R5_SetCurrentState_Open_CallsOpenAllDoors()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.OpenAllDoors().Returns(true);

            var controller = new OfficeController("office123", null, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("open");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("open"));

            mockDoorManager.Received(1).OpenAllDoors();
        }
        // L2R4: SetCurrentState("open") fails if OpenAllDoors() fails
        [Test]
        public void L3R1_SetCurrentState_Closed_FailsIfDoorsDontLock()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockLightManager = Substitute.For<ILightManager>();
            mockDoorManager.LockAllDoors().Returns(false); // Simulate failure to open doors

            var controller = new OfficeController("office123", mockLightManager, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("closed");

            // Assert
            Assert.That(result, Is.False);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("out of hours"));

            mockDoorManager.Received(1).LockAllDoors();
        }

        // L2R5: SetCurrentState("open") should call OpenAllDoors
        [Test]
        public void L3R1_SetCurrentState_Closed_CallsLockAllDoors()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.LockAllDoors().Returns(true);

            var controller = new OfficeController("office123", null, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("closed");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("closed"));

            mockDoorManager.Received(1).LockAllDoors();
        }

        [Test]
        public void L3R1_SetCurrentState_Closed_CallsSetAllLightsOff()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockLightManager = Substitute.For<ILightManager>();
            mockDoorManager.LockAllDoors().Returns(true);

            var controller = new OfficeController("office123", mockLightManager, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("closed");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("closed"));

            mockLightManager.Received(1).SetAllLights(false);
        }

        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsOpenAllDoors()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();

            var controller = new OfficeController("office123", null, null, mockDoorManager, null, null);

            // Act
            bool result = controller.SetCurrentState("fire alarm");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));
            mockDoorManager.Received(1).OpenAllDoors();
        }

        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsSetAllLightsOn()
        {
            // Arrange
            var mockLightManager = Substitute.For<ILightManager>();

            var controller = new OfficeController("office123", mockLightManager, null, null, null, null);

            // Act
            bool result = controller.SetCurrentState("fire alarm");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));
            mockLightManager.Received(1).SetAllLights(true);
        }

        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsSetFireAlarmTrue()
        {
            // Arrange
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            var controller = new OfficeController("office123", null, mockFireAlarmManager, null, null, null);

            // Act
            bool result = controller.SetCurrentState("fire alarm");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));
            mockFireAlarmManager.Received(1).SetAlarm(true);
        }

        [Test]
        public void L3R2_SetCurrentState_FireAlarm_CallsLogFireAlarm()
        {
            // Arrange
            var mockWebService = Substitute.For<IWebService>();

            var controller = new OfficeController("office123", null, null, null, mockWebService, null);

            // Act
            bool result = controller.SetCurrentState("fire alarm");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));
            mockWebService.Received(1).LogFireAlarm("fire alarm");
        }

        [Test]
        public void L3R3_GetStatusReport_LogsLightFault()
        {
            // Arrange
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert
            mockWebService.Received(1).LogEngineerRequired("Lights,");
        }

        [Test]
        public void L3R3_GetStatusReport_LogsDoorFault()
        {
            // Arrange
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockDoorManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert
            mockWebService.Received(1).LogEngineerRequired("Doors,");
        }

        [Test]
        public void L3R3_GetStatusReport_LogsFireAlarmFault()
        {
            // Arrange
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockFireAlarmManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert
            mockWebService.Received(1).LogEngineerRequired("FireAlarm,");
        }

        [Test]
        public void L3R3_GetStatusReport_LogsMultipleFaults()
        {
            // Arrange
            var mockWebService = Substitute.For<IWebService>();
            var mockLightManager = Substitute.For<ILightManager>();
            var mockDoorManager = Substitute.For<IDoorManager>();
            var mockFireAlarmManager = Substitute.For<IFireAlarmManager>();

            mockDoorManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockFireAlarmManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, null);

            // Act
            string result = controller.GetStatusReport();

            // Assert
            mockWebService.Received(1).LogEngineerRequired("Lights,FireAlarm,Doors,");
        }

        [Test]
        public void L3R4_SetCurrentState_FireAlarm_SendsMailUponException_NoWebService()
        {
            // Arrange
            var mockEmailService = Substitute.For<IEmailService>();

            var controller = new OfficeController("office123", null, null, null, null, mockEmailService);

            // Act
            bool result = controller.SetCurrentState("fire alarm");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));
            mockEmailService.Received(1).SendMail("citycouncil@preston.gov.uk", "failed to log alarm", "Object reference not set to an instance of an object.");
        }

        [Test]
        public void L3R4_SetCurrentState_FireAlarm_SendsMailUponException_Other()
        {
            // Arrange
            var mockEmailService = Substitute.For<IEmailService>();
            var mockWebService = Substitute.For<IWebService>();

            mockWebService.When(x => x.LogFireAlarm(Arg.Any<string>())).Do(x => throw new Exception("exception test"));

            var controller = new OfficeController("office123", null, null, null, mockWebService, mockEmailService);

            // Act
            bool result = controller.SetCurrentState("fire alarm");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("fire alarm"));
            mockEmailService.Received(1).SendMail("citycouncil@preston.gov.uk", "failed to log alarm", "exception test");
        }
    }
}
