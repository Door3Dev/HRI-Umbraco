using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class Product
    {
        public string Name { get; set; }
        public List<Plan> Plans { get; set; }
        public string Description { get; set; }
    }
}