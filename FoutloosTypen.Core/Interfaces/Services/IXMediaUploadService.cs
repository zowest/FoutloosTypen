using System.Threading.Tasks;
using FoutloosTypen.Core.Models;

namespace FoutloosTypen.Core.Interfaces.Services
{
    public interface IXMediaUploadService
    {
        Task<string> UploadMediaV11Async(string filePath, string contentType, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret);
        Task CreateTweetWithOAuth1Async(string text, string? mediaId, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret);
    }
}
