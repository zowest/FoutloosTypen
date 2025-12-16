using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ISocialShareService
    {
        Task<bool> ShareToXWithConfirmationAsync(string lessonName, string progressText);
        Task<bool> RegrantPostingConsentAsync();
    }
}
