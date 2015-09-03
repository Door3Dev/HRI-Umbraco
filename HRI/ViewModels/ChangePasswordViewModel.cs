using System.ComponentModel.DataAnnotations;

namespace HRI.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm New Password and New Password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}