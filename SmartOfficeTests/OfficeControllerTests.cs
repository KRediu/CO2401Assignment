using TDDAssignment;
using System;
using NUnit.Framework;
using NSubstitute;
using TDDAssignment.Interfaces;

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
            var mockLightManager = NSubstitute.Substitute.For<ILightManager>();
            var mockFireAlarmManager = NSubstitute.Substitute.For<IFireAlarmManager>();
            var mockDoorManager = NSubstitute.Substitute.For<IDoorManager>();
            var mockWebService = NSubstitute.Substitute.For<IWebService>();
            var mockEmailService = NSubstitute.Substitute.For<IEmailService>();
            string officeID = "office123";

            // Act
            var controller = new OfficeController(officeID, mockLightManager, mockFireAlarmManager, mockDoorManager, mockWebService, mockEmailService);

            // Assert
            Assert.That(controller.GetOfficeID(), Is.EqualTo(officeID.ToLower()));
        }

        // L2R3: GetStatusReport() aggregates status reports
        [Test]
        public void L2R3_GetStatusReport_ReturnsCombinedStatus()
        {
            // Arrange
            var mockLightManager = NSubstitute.Substitute.For<ILightManager>();
            var mockFireAlarmManager = NSubstitute.Substitute.For<IFireAlarmManager>();
            var mockDoorManager = NSubstitute.Substitute.For<IDoorManager>();

            mockLightManager.GetStatus().Returns("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockDoorManager.GetStatus().Returns("Doors,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,");
            mockFireAlarmManager.GetStatus().Returns("FireAlarm,OK,FAULT,OK,FAULT,OK,OK,OK,OK,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, null, null);

            // Act
            string statusReport = controller.GetStatusReport();

            // Assert
            Assert.That(statusReport, Is.EqualTo("Lights,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,Doors,OK,OK,FAULT,OK,OK,OK,OK,FAULT,OK,OK,FireAlarm,OK,FAULT,OK,FAULT,OK,OK,OK,OK,OK,OK,"));
        }

        [Test]
        public void L2R3_GetStatusReport_HandlesNullOrEmptyStatus()
        {
            // Arrange
            var mockLightManager = NSubstitute.Substitute.For<ILightManager>();
            var mockFireAlarmManager = NSubstitute.Substitute.For<IFireAlarmManager>();
            var mockDoorManager = NSubstitute.Substitute.For<IDoorManager>();

            mockLightManager.GetStatus().Returns(string.Empty);
            mockDoorManager.GetStatus().Returns((string)null);
            mockFireAlarmManager.GetStatus().Returns("FireAlarm,OK,OK,OK,");

            var controller = new OfficeController("office123", mockLightManager, mockFireAlarmManager, mockDoorManager, null, null);

            // Act
            string statusReport = controller.GetStatusReport();

            // Assert
            Assert.That(statusReport, Is.EqualTo("FireAlarm,OK,OK,OK,"));
        }

        // L2R4: SetCurrentState("open") should call OpenAllDoors
        [Test]
        public void L2R4_SetCurrentState_Open_CallsOpenAllDoors()
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
        }

        // L2R5: SetCurrentState("open") fails if OpenAllDoors() fails
        [Test]
        public void L2R5_SetCurrentState_Open_FailsIfDoorsDontOpen()
        {
            // Arrange
            var mockDoorManager = Substitute.For<IDoorManager>();
            mockDoorManager.OpenAllDoors().Returns(false); // Simulate failure to open doors

            var controller = new OfficeController("office123", null, null, mockDoorManager, null, null);
            controller.SetCurrentState("closed");

            // Act
            bool result = controller.SetCurrentState("open");

            // Assert
            Assert.That(result, Is.False);
            Assert.That(controller.GetCurrentState(), Is.EqualTo("closed"));
        }

    }

}
