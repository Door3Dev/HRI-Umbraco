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
            MessageTypes = new SelectList(new[]
            {
                new{ID = "0", Name = "Please Choose A Topic"},
                new{ID = "1", Name = "Billing Questions"},
                new{ID = "2", Name = "ID Cards"},
                new{ID = "3", Name = "Benefits Questions"},
                new{ID = "4", Name = "Provider Network"},
                new{ID = "5", Name = "Techincal Issues"},
                new{ID = "6", Name = "Claims Option On Our Website"},
                new{ID = "7", Name = "Cancellation"},
                new{ID = "8", Name = "Update Information"},
                new{ID = "9", Name = "Pharmacy"},
            }, "ID", "Name", 0);
        }

        public string MessageType { get; set; }

        public SelectList MessageTypes { get; set; }

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