using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Media;


namespace FoutloosTypen.Core.Models
{
    public class TtsRequest
    {
        public string Text { get; set; } = string.Empty;

        public float Rate { get; set; } = 1.0f;
        public float Volume { get; set; } = 1.0f;
        public float Pitch { get; set; } = 1.0f;

        public Locale? Locale { get; set; }
    }
}