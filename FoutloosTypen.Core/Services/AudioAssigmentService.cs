using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Repositories;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Services
{
    // Inherit AssignmentService so AudioAssignmentService exposes the same assignment APIs
    public class AudioAssignmentService : AssignmentService, IAudioAssignmentService
    {
        private readonly IPracticeMaterialService _practiceMaterialService;
        private readonly ITtsService _ttsService;

        public AudioAssignmentService(
            IAssignmentRepository assignmentRepository,
            IPracticeMaterialService practiceMaterialService,
            ITtsService ttsService)
            : base(assignmentRepository)
        {
            _practiceMaterialService = practiceMaterialService;
            _ttsService = ttsService;
        }

        // Keep existing instruction playback
        public async Task PlayInstructionAsync(AudioAssignment assignment)
        {
            if (string.IsNullOrWhiteSpace(assignment.InstructionText))
                return;

            var request = new TtsRequest
            {
                Text = assignment.InstructionText,
                Rate = assignment.S     peechRate,
                Volume = assignment.Volume
            };

            await _ttsService.SpeakAsync(request);
        }

        public void StopInstruction()
        {
            _ttsService.Cancel();
        }

        // New: play all practice material sentences for an assignment as audio
        public async Task PlayPracticeMaterialsAsync(int assignmentId, CancellationToken? cancellationToken = null)
        {
            var materials = _practiceMaterialService
                .GetAll()
                .Where(pm => pm.AssignmentId == assignmentId)
                .ToList();

            foreach (var pm in materials)
            {
                if (string.IsNullOrWhiteSpace(pm.Sentence))
                    continue;

                var req = new TtsRequest
                {
                    Text = pm.Sentence,
                    Rate = 0.95f,
                    Volume = 1.0f
                };

                // await each sentence; adjust behaviour if you want parallelism or gaps
                await _ttsService.SpeakAsync(req, cancellationToken);
            }
        }

        // Return copies of practice materials with the text hidden so UI can show a placeholder
        public List<PracticeMaterial> GetPracticeMaterialsForAudio(int assignmentId)
        {
            var materials = _practiceMaterialService
                .GetAll()
                .Where(pm => pm.AssignmentId == assignmentId)
                .Select(pm => new PracticeMaterial(pm.Id, "[Audio]", pm.AssignmentId))
                .ToList();

            return materials;
        }
    }
}
