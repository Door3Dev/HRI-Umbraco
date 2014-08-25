using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmNewPassword { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Guid { get; set; }
    }
}