namespace FoutloosTypen.Core
{
    public class XAuthSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = System.Array.Empty<string>();

        public string ConsumerKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
        public string OAuthToken { get; set; } = string.Empty;
        public string OAuthTokenSecret { get; set; } = string.Empty;

        public string ScopesJoined => string.Join(' ', Scopes ?? System.Array.Empty<string>());
    }
}
