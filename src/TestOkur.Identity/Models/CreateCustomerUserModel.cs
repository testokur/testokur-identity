namespace TestOkur.Identity.Models
{
    using TestOkur.Identity.Infrastructure.Data;

    public class CreateCustomerUserModel
    {
        public CreateCustomerUserModel()
        {
        }

        public string Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int MaxAllowedDeviceCount { get; set; }

        public int MaxAllowedStudentCount { get; set; }

        public bool CanScan { get; set; }

        public int LicenseTypeId { get; set; }

        public ApplicationUser ToApplicationUser()
        {
            return new ApplicationUser(
                Id,
                Email,
                MaxAllowedDeviceCount,
                MaxAllowedStudentCount,
                CanScan,
                LicenseTypeId);
        }
    }
}
