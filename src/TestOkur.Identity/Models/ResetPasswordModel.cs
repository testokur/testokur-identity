namespace TestOkur.Identity.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ResetPasswordModel
    {
        public ResetPasswordModel(bool success)
        {
            Success = success;
        }

        public ResetPasswordModel()
        {
        }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string NewPasswordRepeat { get; set; }

        public bool Success { get; set; }
    }
}
