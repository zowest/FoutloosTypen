using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace FoutloosTypen.Services
{
    public interface IXMediaUploadService
    {
        Task<string> UploadMediaV11Async(string filePath, string contentType, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret);
        Task CreateTweetWithOAuth1Async(string text, string? mediaId, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret);
    }

    public class MediaUploadService : IXMediaUploadService
    {
        private const string MediaUploadV11 = "https://upload.twitter.com/1.1/media/upload.json";
        private const string TweetEndpoint = "https://api.x.com/2/tweets";

        public async Task<string> UploadMediaV11Async(string filePath, string contentType, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret)
        {
            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret) ||
                string.IsNullOrWhiteSpace(oauthToken) || string.IsNullOrWhiteSpace(oauthTokenSecret))
            {
                throw new ArgumentException("OAuth1 credentials (consumer key/secret and oauth token/secret) must be provided for v1.1 media upload.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var totalBytes = fileBytes.Length;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            // INIT
            var initParams = new List<KeyValuePair<string, string>>
            {
                new("command", "INIT"),
                new("total_bytes", totalBytes.ToString()),
                new("media_type", contentType)
            };
            var initRequest = new HttpRequestMessage(HttpMethod.Post, MediaUploadV11)
            {
                Content = new FormUrlEncodedContent(initParams)
            };

            var header = "OAuth " + BuildOAuth1Header(MediaUploadV11, HttpMethod.Post, initParams, consumerKey, consumerSecret, oauthToken, oauthTokenSecret);
#if DEBUG
            Debug.WriteLine("[MediaUploadService] INIT header: " + header);
            Debug.WriteLine("[MediaUploadService] INIT params: " + string.Join(",", initParams.Select(kv => kv.Key + "=" + kv.Value)));
#endif
            initRequest.Headers.TryAddWithoutValidation("Authorization", header);
            var initResp = await client.SendAsync(initRequest);
            var initJson = await initResp.Content.ReadAsStringAsync();
            if (!initResp.IsSuccessStatusCode)
                throw new HttpRequestException($"v1.1 INIT failed: {(int)initResp.StatusCode} {initResp.ReasonPhrase} - {initJson}");

            using var initDoc = JsonDocument.Parse(initJson);
            var mediaId = initDoc.RootElement.GetProperty("media_id_string").GetString();

            // APPEND (one chunk, base64)
            var base64 = Convert.ToBase64String(fileBytes);
            var appendParams = new List<KeyValuePair<string, string>>
            {
                new("command", "APPEND"),
                new("media_id", mediaId!),
                new("segment_index", "0"),
                new("media_data", base64)
            };
            var appendRequest = new HttpRequestMessage(HttpMethod.Post, MediaUploadV11)
            {
                Content = new FormUrlEncodedContent(appendParams)
            };
            appendRequest.Headers.TryAddWithoutValidation("Authorization", "OAuth " + BuildOAuth1Header(MediaUploadV11, HttpMethod.Post, appendParams, consumerKey, consumerSecret, oauthToken, oauthTokenSecret));
            var appendResp = await client.SendAsync(appendRequest);
            if (!appendResp.IsSuccessStatusCode)
                throw new HttpRequestException($"v1.1 APPEND failed: {(int)appendResp.StatusCode} {appendResp.ReasonPhrase}");

            // FINALIZE
            var finalizeParams = new List<KeyValuePair<string, string>>
            {
                new("command", "FINALIZE"),
                new("media_id", mediaId!)
            };
            var finalizeRequest = new HttpRequestMessage(HttpMethod.Post, MediaUploadV11)
            {
                Content = new FormUrlEncodedContent(finalizeParams)
            };
            finalizeRequest.Headers.TryAddWithoutValidation("Authorization", "OAuth " + BuildOAuth1Header(MediaUploadV11, HttpMethod.Post, finalizeParams, consumerKey, consumerSecret, oauthToken, oauthTokenSecret));
            var finalizeResp = await client.SendAsync(finalizeRequest);
            var finalizeJson = await finalizeResp.Content.ReadAsStringAsync();
            if (!finalizeResp.IsSuccessStatusCode)
                throw new HttpRequestException($"v1.1 FINALIZE failed: {(int)finalizeResp.StatusCode} {finalizeResp.ReasonPhrase} - {finalizeJson}");

            return mediaId!;
        }

        private static string BuildOAuth1Header(string url, HttpMethod method, IEnumerable<KeyValuePair<string, string>> requestParams, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret)
        {
            var oauthParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", Guid.NewGuid().ToString("N") },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { "oauth_token", oauthToken },
                { "oauth_version", "1.0" }
            };

            foreach (var kv in requestParams)
                oauthParams[kv.Key] = kv.Value;

            var uri = new Uri(url);
            var normalizedUrl = uri.GetLeftPart(UriPartial.Path);

            var paramString = string.Join('&', oauthParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var signatureBaseString = $"{method.Method.ToUpperInvariant()}&{Uri.EscapeDataString(normalizedUrl)}&{Uri.EscapeDataString(paramString)}";
            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(oauthTokenSecret)}";

            using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
            var signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));

            var headerParams = new Dictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", oauthParams["oauth_nonce"] },
                { "oauth_signature", signature },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", oauthParams["oauth_timestamp"] },
                { "oauth_token", oauthToken },
                { "oauth_version", "1.0" }
            };

            return string.Join(", ", headerParams.Select(kv => $"{kv.Key}=\"{Uri.EscapeDataString(kv.Value)}\""));
        }

        public async Task CreateTweetWithOAuth1Async(string text, string? mediaId, string consumerKey, string consumerSecret, string oauthToken, string oauthTokenSecret)
        {
            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret) ||
                string.IsNullOrWhiteSpace(oauthToken) || string.IsNullOrWhiteSpace(oauthTokenSecret))
            {
                throw new ArgumentException("OAuth1 credentials (consumer key/secret and oauth token/secret) must be provided to post with user context.");
            }

            using var http = new HttpClient();

            object payload = mediaId is not null
                ? new { text = text, media = new { media_ids = new[] { mediaId } } }
                : new { text = text };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var header = "OAuth " + BuildOAuth1Header(TweetEndpoint, HttpMethod.Post,
                         Enumerable.Empty<KeyValuePair<string, string>>(), consumerKey,
                         consumerSecret, oauthToken, oauthTokenSecret);
            var request = new HttpRequestMessage(HttpMethod.Post, TweetEndpoint)
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation("Authorization", header);
            request.Headers.TryAddWithoutValidation("User-Agent", "v2CreateTweetCSharp");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            var resp = await http.SendAsync(request);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Tweet create failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
        }
    }
}
