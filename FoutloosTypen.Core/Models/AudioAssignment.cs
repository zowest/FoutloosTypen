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
            base.AssignmentType = AssignmentTypeEnum.Audio;
        }

        public AudioAssignment(int id, double timelimit, int lessonId) : base(id, timelimit, lessonId)
        {
            base.AssignmentType = AssignmentTypeEnum.Audio;
        }

        // Hide the base property so consumers of AudioAssignment always see Audio.
        public new AssignmentTypeEnum AssignmentType
        {
            get => AssignmentTypeEnum.Audio;
            set => base.AssignmentType = AssignmentTypeEnum.Audio;
        }

        public string InstructionText { get; set; } = string.Empty;
        public float SpeechRate { get; set; } = 0.9f;
        public float Volume { get; set; } = 1.0f;
    }

}