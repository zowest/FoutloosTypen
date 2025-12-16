using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IAudioAssignmentService
    {
        Task PlayInstructionAsync(AudioAssignment assignment);
        void StopInstruction();

        Task PlayPracticeMaterialsAsync(int assignmentId, CancellationToken? cancellationToken = null);
        List<PracticeMaterial> GetPracticeMaterialsForAudio(int assignmentId);
    }
}