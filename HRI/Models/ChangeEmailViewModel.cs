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
        public string Email2 { get; set; }
    }
}