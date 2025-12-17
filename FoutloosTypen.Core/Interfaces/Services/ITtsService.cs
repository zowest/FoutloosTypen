using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Media;


namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ITtsService
    {
        Task SpeakAsync(TtsRequest request, CancellationToken? cancellationToken = null);
        void Cancel();
        Task<IEnumerable<Locale>> GetAvailableLocalesAsync();
    }
}