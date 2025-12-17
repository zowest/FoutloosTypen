using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FoutloosTypen.Core.Interfaces.Repositories;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class MediaUploadRepository : IMediaUploadRepository
    {
        private const string MediaUploadUrl = "https://upload.twitter.com/1.1/media/upload.json";
        private const string TweetUrl = "https://api.twitter.com/2/tweets";

        public async Task<string> UploadMediaAsync(string filePath, string contentType, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret)
        {
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "media", Path.GetFileName(filePath));

            var authHeader = GenerateOAuthHeader("POST", MediaUploadUrl, consumerKey, consumerSecret, oauthToken, oauthTokenSecret);
            client.DefaultRequestHeaders.Add("Authorization", authHeader);

            var response = await client.PostAsync(MediaUploadUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Media upload failed: {response.StatusCode} - {responseContent}");
            }

            var mediaId = ExtractMediaId(responseContent);
            return mediaId;
        }

        public async Task PostTweetAsync(string text, string? mediaId, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret)
        {
            using var client = new HttpClient();

            var payload = string.IsNullOrEmpty(mediaId)
                ? $"{{\"text\":\"{EscapeJson(text)}\"}}"
                : $"{{\"text\":\"{EscapeJson(text)}\",\"media\":{{\"media_ids\":[\"{mediaId}\"]}}}}";

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var authHeader = GenerateOAuthHeader("POST", TweetUrl, consumerKey, consumerSecret, oauthToken, oauthTokenSecret);
            client.DefaultRequestHeaders.Add("Authorization", authHeader);

            var response = await client.PostAsync(TweetUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Tweet post failed: {response.StatusCode} - {responseContent}");
            }
        }

        private string GenerateOAuthHeader(string method, string url, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret)
        {
            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var parameters = new[]
            {
                new KeyValuePair<string, string>("oauth_consumer_key", consumerKey),
                new KeyValuePair<string, string>("oauth_nonce", nonce),
                new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"),
                new KeyValuePair<string, string>("oauth_timestamp", timestamp),
                new KeyValuePair<string, string>("oauth_token", oauthToken),
                new KeyValuePair<string, string>("oauth_version", "1.0")
            };

            var sortedParams = parameters.OrderBy(p => p.Key).ThenBy(p => p.Value);
            var paramString = string.Join("&", sortedParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            var signatureBase = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";
            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(oauthTokenSecret)}";

            using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
            var signatureBytes = hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBase));
            var signature = Convert.ToBase64String(signatureBytes);

            return $"OAuth oauth_consumer_key=\"{Uri.EscapeDataString(consumerKey)}\", " +
                   $"oauth_nonce=\"{Uri.EscapeDataString(nonce)}\", " +
                   $"oauth_signature=\"{Uri.EscapeDataString(signature)}\", " +
                   $"oauth_signature_method=\"HMAC-SHA1\", " +
                   $"oauth_timestamp=\"{timestamp}\", " +
                   $"oauth_token=\"{Uri.EscapeDataString(oauthToken)}\", " +
                   $"oauth_version=\"1.0\"";
        }

        private string ExtractMediaId(string responseContent)
        {
            var startIndex = responseContent.IndexOf("\"media_id_string\":\"") + 19;
            var endIndex = responseContent.IndexOf("\"", startIndex);
            return responseContent.Substring(startIndex, endIndex - startIndex);
        }

        private string EscapeJson(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
