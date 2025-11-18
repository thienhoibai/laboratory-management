namespace IAM.Application.Auth
{
    public class GoogleAuthOptions
    {
        public const string SectionName = "Auth:Google";
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}
