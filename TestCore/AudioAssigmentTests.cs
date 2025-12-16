using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using FoutloosTypen.Core.Interfaces;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using Moq;
using NUnit.Framework;

namespace FoutloosTypen.Core.Tests
{
    [TestFixture]
    public class AudioAssignmentServiceTests
    {
        private Mock<ITtsService> _ttsMock!;
        private AudioAssignmentService _service!;

        [SetUp]
        public void SetUp()
        {
            _ttsMock = new Mock<ITtsService>();

            _service = new AudioAssignmentService(_ttsMock.Object);
        }

        [Test]
        public async Task PlayInstructionAsync_WithText_CallsTtsService()
        {
            // Arrange
            var assignmentText = "Luister goed naar de opdracht";

            // Act
            await _service.PlayInstructionAsync(assignmentText);

            // Assert
            _ttsMock.Verify(t =>
                t.SpeakAsync(
                    It.Is<TtsRequest>(r => r.Text == assignmentText),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
