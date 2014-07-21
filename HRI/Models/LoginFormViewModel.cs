using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class LoginFormViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public bool HasValidUsernameAndPassword
        {
            get
            {
                return Password == "password";
            }
        }
    }
}