namespace TestOkur.Identity.Configuration
{
    using System.Collections.Generic;

    public class AppConfiguration
    {
        public string MasterPassword { get; set; }

        public string CertificatePassword { get; set; }

        public IEnumerable<AdminUserInfo> AdminUsers { get; set; }
    }
}
