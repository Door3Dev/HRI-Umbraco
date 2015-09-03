using System.ComponentModel.DataAnnotations;

namespace HRI.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string UserName { get; set; }
    }
}