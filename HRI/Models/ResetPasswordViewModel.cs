using System.ComponentModel.DataAnnotations;

namespace HRI.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm New Password and New Password do not match.")]
        public string ConfirmNewPassword { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Guid { get; set; }
    }
}