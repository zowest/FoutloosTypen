using System.Threading.Tasks;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface IMediaUploadRepository
    {
        Task<string> UploadMediaAsync(string filePath, string contentType, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret);
        Task PostTweetAsync(string text, string? mediaId, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret);
    }
}
