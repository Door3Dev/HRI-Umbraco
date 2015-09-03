using System.ComponentModel.DataAnnotations;

namespace HRI.ViewModels
{
    public class ForgotUserNameViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}