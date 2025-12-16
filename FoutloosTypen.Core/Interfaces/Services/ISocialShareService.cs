using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface ISocialShareService
    {
        Task<bool> ShareToXWithConfirmationAsync(string lessonName, string progressText);
        Task<bool> RegrantPostingConsentAsync();
    }
}
