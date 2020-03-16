namespace TestOkur.Identity.Models
{
    using System;

    public class UpdateUserModel
    {
        public UpdateUserModel()
        {
        }

        public string UserId { get; set; }

        public string Email { get; set; }

        public int MaxAllowedDeviceCount { get; set; }

        public int MaxAllowedStudentCount { get; set; }

        public bool CanScan { get; set; }

        public int LicenseTypeId { get; set; }

        public DateTime? ExpiryDateUtc { get; set; }

        public DateTime? StartDateTimeUtc { get; set; }

        public DateTime? ActivationTimeUtc { get; set; }

        public bool? Active { get; set; }
    }
}
