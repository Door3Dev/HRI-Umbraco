using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class RegisterFormViewModel
    {
        /// <summary>
        /// Describes if the user is enrolled but not paid, shopping, or a broker
        /// </summary>
        [Required]
        public int UserType { get; set; }

        public string MemberId { get; set; }

        public string SSN { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string ConfirmEmail { get; set; }

        [Required]
        public string Zipcode { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Phone { get; set; }

        public int RegistrationSuccessPage { get; set; }
    }
}