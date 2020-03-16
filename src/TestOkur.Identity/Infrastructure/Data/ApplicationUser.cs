namespace TestOkur.Identity.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Identity;
    using TestOkur.Identity.Models;

    public class ApplicationUser : IdentityUser
    {
        private const int FullLicenseTypeId = 5;

        public ApplicationUser(string id, string userName)
            : this(id, userName, int.MaxValue, int.MaxValue, true, FullLicenseTypeId)
        {
        }

        public ApplicationUser(
            string id,
            string userName,
            int maxAllowedDeviceCount,
            int maxAllowedStudentCount,
            bool canScan,
            int licenseTypeId)
         : base(userName)
        {
            MaxAllowedStudentCount = maxAllowedStudentCount;
            CanScan = canScan;
            LicenseTypeId = licenseTypeId;
            MaxAllowedDeviceCount = maxAllowedDeviceCount;
            Id = id;
            Email = userName;
            EmailConfirmed = true;
            LockoutEnabled = false;
            CreatedDateTimeUtc = DateTime.UtcNow;
        }

        protected ApplicationUser()
        {
        }

        public List<LoginDevice> LoginDevices { get; set; }

        public int MaxAllowedDeviceCount { get; set; }

        public int MaxAllowedStudentCount { get; set; }

        public bool CanScan { get; set; }

        public int LicenseTypeId { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedDateTimeUtc { get; private set; }

        public DateTime? StartDateTimeUtc { get; set; }

        public DateTime? ExpiryDateUtc { get; set; }

        public DateTime? ActivationTimeUtc { get; set; }

        public void Extend()
        {
            ExpiryDateUtc = DateTime.UtcNow > ExpiryDateUtc ?
                DateTime.UtcNow.AddYears(1) :
                ExpiryDateUtc.Value.AddYears(1);
        }

        public void Activate()
        {
            Active = true;
            ActivationTimeUtc = DateTime.UtcNow;
        }

        public void UpdateFromModel(UpdateUserModel model)
        {
            UserName = model.Email;
            Email = model.Email;
            MaxAllowedDeviceCount = model.MaxAllowedDeviceCount;
            MaxAllowedStudentCount = model.MaxAllowedStudentCount;
            CanScan = model.CanScan;
            LicenseTypeId = model.LicenseTypeId;
            ExpiryDateUtc = model.ExpiryDateUtc;

            if (model.Active != null)
            {
                Active = model.Active.Value;
            }

            if (model.ActivationTimeUtc != null)
            {
                ActivationTimeUtc = model.ActivationTimeUtc;
            }

            if (model.StartDateTimeUtc != null)
            {
                StartDateTimeUtc = model.StartDateTimeUtc;
            }
        }
    }
}
