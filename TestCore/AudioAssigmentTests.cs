using System.Threading;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Services;
using Moq;
using NUnit.Framework;

namespace FoutloosTypen.Core.Tests
{
    [TestFixture]
    public class AudioAssignmentServiceTests
    {
        private Mock<ITtsService> _ttsMock;
        private AudioAssignmentService _service;

        [SetUp]
        public void SetUp()
        {
            _ttsMock = new Mock<ITtsService>();
            _service = new AudioAssignmentService(_ttsMock.Object);
        }

        [Test]
        public async Task PlayInstructionAsync_WithText_CallsTtsService()
        {
            var assignmentText = "Luister goed naar de opdracht";
            var assignment = new AudioAssignment
            {
                InstructionText = assignmentText,
                SpeechRate = 0.9f,
                Volume = 1.0f
            };

            await _service.PlayInstructionAsync(assignment);

            _ttsMock.Verify(t =>
                t.SpeakAsync(
                    It.Is<TtsRequest>(r => r.Text == assignmentText && r.Volume == assignment.Volume),
                    It.IsAny<CancellationToken?>()),
                Times.Once);
        }

        [Test]
        public async Task PlayInstructionAsync_EmptyInstruction_DoesNotCallTts()
        {
            var assignment = new AudioAssignment
            {
                InstructionText = string.Empty,
                SpeechRate = 0.9f,
                Volume = 1.0f
            };

            await _service.PlayInstructionAsync(assignment);

            _ttsMock.Verify(t =>
                t.SpeakAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken?>()),
                Times.Never);
        }

        [Test]
        public async Task PlayInstructionAsync_MultipleAssignments_CallsTtsServiceForEach()
        {
            var assignments = new[]
            {
                new AudioAssignment { InstructionText = "Opdracht 1", SpeechRate = 0.9f, Volume = 1.0f },
                new AudioAssignment { InstructionText = "Opdracht 2", SpeechRate = 1.0f, Volume = 0.8f }
            };

            foreach (var assignment in assignments)
            {
                await _service.PlayInstructionAsync(assignment);
            }

            foreach (var assignment in assignments)
            {
                _ttsMock.Verify(t =>
                    t.SpeakAsync(
                        It.Is<TtsRequest>(r => r.Text == assignment.InstructionText && r.Volume == assignment.Volume),
                        It.IsAny<CancellationToken?>()),
                    Times.Once);
            }
        }
    }
}
