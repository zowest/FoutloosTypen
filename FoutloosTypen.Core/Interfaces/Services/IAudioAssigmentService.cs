using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IAudioAssignmentService
    {
        Task PlayInstructionAsync(AudioAssignment assignment);
        void StopInstruction();
    }
}