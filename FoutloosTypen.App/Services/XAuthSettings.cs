namespace FoutloosTypen.Services
{
    public class XAuthSettings
    {
        public string ClientId { get; init; } = "RW5hLTJ2eFExaHVjRnBDUFhGcmU6MTpjaQ";
        public string RedirectUri { get; init; } = "http://127.0.0.1:51789/callback";
        public string[] Scopes { get; init; } = new[] { "tweet.write", "users.read", "media.write", "offline.access" };
        public string ConsumerKey { get; init; } = "T1KTAEaHFp8ZGQb9NNZgrOfmM";
        public string ConsumerSecret { get; init; } = "CKIWs11WKG7jzuinIu5GrJJCnQIcXxaALNZXjzURcFEMXPLAnZ";
        public string AccessToken { get; init; } = "1995817350831243264-FfrkWQRt0zpUsmK21Xpz2cGCKQAgXS";
        public string AccessTokenSecret { get; init; } = "jgVf96uYnhLyQAksJBDswqg9aAlZ1KeVFGoK4IWjzkg8z";
    }
}