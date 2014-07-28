using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string UserName { get; set; }
    }
}