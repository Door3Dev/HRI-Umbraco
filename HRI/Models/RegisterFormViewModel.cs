using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HRI.Models
{
    public class RegisterFormViewModel: IValidatableObject
    {
        public const string PhoneInputMask = "(999) 999-9999";

        private string _email;
        private string _confirmEmail;

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
        [Compare("Email", ErrorMessage = "Confirm Email and Email do not match.")]
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(PlanId) && (string.IsNullOrEmpty(MemberId) || MemberId.Length != 9))
            {
                yield return new ValidationResult("The Member ID should have 9 characters", new[]{"MemberId"});
            }
            if (Username == Password)
            {
                yield return new ValidationResult( "Password cannot be the same as Username", new[]{"Password"});
            }
            if (Regex.IsMatch(Username, "[ '\";!@#$%^&*]"))
            {
                yield return new ValidationResult("Username cannot contain space or next symbols ' \" ; ! @ # $ % ^ & * ", new[]{"Username"});
            }

            // Validate ZipCode Length, mask from front-end might have 5 characters, but contains "_" so check for that
            if (!String.IsNullOrWhiteSpace(Zipcode) && (Zipcode.Length != 5 || Zipcode.Contains("_")))
            {
                yield return new ValidationResult("Invalid Zip Code", new[]{"Zipcode"});
            }

            // Validate Phone Length
            if (!String.IsNullOrWhiteSpace(Phone))
            {
                var cleanPhone = Phone.Replace("(", "").Replace(")", "").Replace("-", "")
                    .Replace(" ", "").Replace("_", "");
                if (cleanPhone.Length != 10)
                {
                    yield return new ValidationResult("Invalid phone number", new[]{"Phone"});
                }
            }
        }
    }
}