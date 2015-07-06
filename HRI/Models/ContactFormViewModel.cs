using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HRI.Models
{
    public class ContactFormViewModel
    {
        [Required]
        public string MessageType { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\(\d{3}\) \d{3}\-\d{4}$", ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Message { get; set; }

        public string Username { get; set; }

        public string MemberId { get; set; }

        public string YNumber { get; set; }

        public static IDictionary<string, IEnumerable<string>> GetCategoriesAndEmails(string categoriesAndEmails)
        {
            return (categoriesAndEmails ?? string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(';').Select(x => x.Trim())) // Billing Questions; ContactUs@healthrepublicny.org; door3-a@mailinator.com;door3-a@reconmail.com
                .Where(x => x.Count() > 1)
                .ToDictionary(x => x.First(), x => x.Skip(1));  // note, will throw an error if there are duplicated categorie in the input
        }
    }
}