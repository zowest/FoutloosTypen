using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;

namespace BusinessLogic.Services
{
    public class AudioAssignmentService : IAudioAssignmentService
    {
        private readonly ITtsService _ttsService;

        public AudioAssignmentService(ITtsService ttsService)
        {
            _ttsService = ttsService;
        }

        public async Task PlayInstructionAsync(AudioAssignment assignment)
        {
            if (string.IsNullOrWhiteSpace(assignment.InstructionText))
                return;

            var request = new TtsRequest
            {
                Text = assignment.InstructionText,
                Rate = assignment.SpeechRate,
                Volume = assignment.Volume
            };

            await _ttsService.SpeakAsync(request);
        }

        public void StopInstruction()
        {
            _ttsService.Cancel();
        }
    }
}
