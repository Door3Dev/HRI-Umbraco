using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HRI.Models
{
    public class RegisterFormViewModel
    {
        private string _email;
        private string _confirmEmail;

        [Remote("ValidateMemberId", "RegisterSurface")]
        public string MemberId { get; set; }

        public string Ssn { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessage = "Please enter a valid e-mail address")]
        public string Email {
            set
            {
                if (value != null)
                    _email = value.Trim().ToLower();
            }
            get { return _email;  }
        }

        [Required]
        [System.ComponentModel.DataAnnotations.Compare("Email", ErrorMessage = "Confirm Email and Email do not match.")]
        public string ConfirmEmail
        {
            set
            {
                if (value != null)
                    _confirmEmail = value.Trim().ToLower();
            }
            get { return _confirmEmail; }
        }

        [Required]
        public string Zipcode { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [RegularExpression(".{8,}", ErrorMessage = "Password do not match the pattern")]
        public string Password { get; set; }

        public string PlanId { get; set; }

        public string Phone { get; set; }
    }
}