using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Enums;

namespace FoutloosTypen.Core.Models
{
    public class AudioAssignment : Assignment
    {
        public AudioAssignment() : base()
        {
            AssignmentType = AssignmentType.Audio;
        }

        public AudioAssignment(int id, double timelimit, int lessonId) : base(id, timelimit, lessonId)
        {
            AssignmentType = AssignmentType.Audio;
        }

        // audio-specific data
        public string InstructionText { get; set; } = string.Empty;
        public float SpeechRate { get; set; } = 0.9f;
        public float Volume { get; set; } = 1.0f;
    }

}