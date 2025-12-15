namespace FoutloosTypen.Services
{
    public class XAuthSettings
    {
        public string ClientId { get; init; } = string.Empty;
        public string RedirectUri { get; init; } = string.Empty;
        public string[] Scopes { get; init; } = System.Array.Empty<string>();
    }
}
