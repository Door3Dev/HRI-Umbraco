using System.ComponentModel.DataAnnotations;

namespace HRI.ViewModels
{
    public class ChangeEmailViewModel
    {
        [Required]       
        public string Password { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Compare("Email", ErrorMessage = "Confirm New Email and New Email do not match.")]
        public string Email2 { get; set; }
    }
}