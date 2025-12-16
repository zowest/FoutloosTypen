namespace FoutloosTypen.Core
{
    public class XAuthSettings
    {
        public string? ClientId { get; set; }
        public string? RedirectUri { get; set; }
        public string[]? Scopes { get; set; }

        public string? ConsumerKey { get; set; }
        public string? ConsumerSecret { get; set; }
        public string? OAuthToken { get; set; }
        public string? OAuthTokenSecret { get; set; }

        public string ScopesJoined => Scopes == null ? string.Empty : string.Join(' ', Scopes);
    }
}
