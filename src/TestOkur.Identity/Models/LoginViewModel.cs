namespace TestOkur.Identity.Models
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
        }

        public LoginViewModel(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool RememberLogin { get; set; }

        public string ReturnUrl { get; set; }
    }
}
