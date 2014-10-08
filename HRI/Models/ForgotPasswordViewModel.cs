using System.ComponentModel.DataAnnotations;

namespace HRI.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string UserName { get; set; }
    }
}