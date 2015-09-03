using System.ComponentModel.DataAnnotations;

namespace HRI.ViewModels
{
    public class ChangeUserNameViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

    }
}