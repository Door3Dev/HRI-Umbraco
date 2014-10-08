using System.ComponentModel.DataAnnotations;

namespace HRI.Models
{
    public class SendVerificationLinkModel
    {
        [Required]
        public string UserName { get; set; }
        public string RedirectUrl { get; set; }
    }
}