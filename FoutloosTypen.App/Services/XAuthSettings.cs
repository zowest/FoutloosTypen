namespace FoutloosTypen.Services 
{ 
    public class XAuthSettings 
    { 
        // OAuth2 / PKCE
        public string ClientId { get; init; } = string.Empty; 
        public string RedirectUri { get; init; } = string.Empty; 
        public string[] Scopes { get; init; } = System.Array.Empty<string>(); 

        // OAuth1 (required for v1.1 media upload)
        public string ConsumerKey { get; init; } = string.Empty; 
        public string ConsumerSecret { get; init; } = string.Empty; 

        // Legacy property kept for compatibility
        public string AccessTokenSecret { get; init; } = string.Empty;

        // OAuth1 user tokens (to be provided via env vars or SecureStorage)
        public string OAuthToken { get; init; } = string.Empty;
        public string OAuthTokenSecret { get; init; } = string.Empty;
    } 
}