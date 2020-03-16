namespace TestOkur.Identity.Models
{
    public class LoggedOutViewModel
    {
        public LoggedOutViewModel()
        {
        }

        public LoggedOutViewModel(string postLogoutRedirectUri)
        {
            PostLogoutRedirectUri = postLogoutRedirectUri;
        }

        public string PostLogoutRedirectUri { get; set; }

        public string ClientName { get; set; }

        public string SignOutIframeUrl { get; set; }

        public bool AutomaticRedirectAfterSignOut { get; set; } = true;

        public string LogoutId { get; set; }

        public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

        public string ExternalAuthenticationScheme { get; set; }
    }
}
