using System.ComponentModel.DataAnnotations;

namespace HRI.Models
{
    public class RegisterFormViewModel
    {
        public string MemberId { get; set; }

        public string Ssn { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        [Required]
        [CompareAttribute("Email", ErrorMessage = "Confirm Email and Email do not match.")]
        public string ConfirmEmail { get; set; }

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