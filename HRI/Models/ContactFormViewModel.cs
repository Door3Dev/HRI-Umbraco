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
        public string PhoneNumber { get; set; }

        [Required]
        public string Message { get; set; }

        public string Username { get; set; }

        public string MemberId { get; set; }

        public string YNumber { get; set; }

        public static IList<Tuple<string, string>> GetCategoriesAndEmails(string categoriesAndEmails)
        {
            return (categoriesAndEmails ?? string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(catEm =>
                {
                    var strings = catEm.Split(new[] { ';' }).Select(s => s.Trim()).ToList();

                    if (strings.Any(string.IsNullOrEmpty) || strings.Count != 2)
                    {
                        return null;
                    }

                    return Tuple.Create(strings[0], strings[1]);
                })
                .Where(i => i != null)
                .ToList();
        }
    }
}