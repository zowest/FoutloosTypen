using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Services;
using FoutloosTypen.Core.Models;
using Microsoft.Maui.Media;

namespace FoutloosTypen.Core.Services
{
    public class TtsService : ITtsService
    {
        private CancellationTokenSource? _cts;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public async Task SpeakAsync(TtsRequest request, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return;

            await _lock.WaitAsync();
            try
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken ?? CancellationToken.None
                );

                var options = new SpeechOptions
                {
                    Volume = request.Volume,
                    Pitch = request.Pitch,
                    Locale = request.Locale
                };

                await TextToSpeech.Default.SpeakAsync(
                    request.Text,
                    options,
                    _cts.Token
                );
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _lock.Release();
            }
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }

        public Task<IEnumerable<Locale>> GetAvailableLocalesAsync()
        {
            return TextToSpeech.Default.GetLocalesAsync();
        }
    }
}
