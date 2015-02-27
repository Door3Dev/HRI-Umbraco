using System;
using System.ComponentModel.DataAnnotations;

namespace HRI.Models
{
    public class RegisterFormViewModel
    {
        public const string PhoneInputMask = "(999) 999-9999";

        private string _email;
        private string _confirmEmail;

        [Required]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "The Member ID should have 9 characters.")]
        public string MemberId { get; set; }

        public string Ssn { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Please enter a valid e-mail address")]
        public string Email 
        {
            set
            {
                _email = value == null ? null : value.Trim().ToLower();
            }

            get
            {
                return _email;
            }
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
        public string Password { get; set; }

        public string PlanId { get; set; }

        public string Phone { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}