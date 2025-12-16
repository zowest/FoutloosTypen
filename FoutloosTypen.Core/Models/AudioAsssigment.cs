using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Enums;

namespace FoutloosTypen.Core.Models
{
    public class AudioAssignment
    {
        public int Id { get; set; }
        public AssignmentType AssignmentType => AssignmentType.Audio;

        public string InstructionText { get; set; } = string.Empty;

        public float SpeechRate { get; set; } = 0.9f;
        public float Volume { get; set; } = 1.0f;
    }
}