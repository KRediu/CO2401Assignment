using TDDAssignment;
using System;
using NUnit.Framework;

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

    }

}
