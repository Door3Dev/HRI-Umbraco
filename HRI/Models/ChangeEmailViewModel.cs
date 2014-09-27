using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRI.Models
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