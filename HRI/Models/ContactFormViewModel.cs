using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRI.Models
{
    public class ContactFormViewModel
    {
        public ContactFormViewModel()
        {
            MessageTypes = new[]
            {
                "Billing Questions",
                "ID Cards",
                "Benefits Questions",
                "Provider Network",
                "Techincal Issues",
                "Claims",
                "Cancellation",
                "Update Information",
                "Pharmacy"
            };
        }

        [Required]
        public string MessageType { get; set; }

        public string[] MessageTypes { get; set; }

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
    }
}