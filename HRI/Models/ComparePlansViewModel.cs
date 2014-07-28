using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class ComparePlansViewModel
    {
        [Required]
        public int ZipCode { get; set; }

        [Required]
        public string County { get; set; }

        public bool CoverSelf { get; set; }

        public int CustomerAge { get; set; }

        public bool CoverSpouse { get; set; }

        public int SpouseAge { get; set; }

        public List<int> ChildrenAges { get; set; }

        public decimal Income { get; set; }

        public int NumberInHousehold { get; set; }
    }
}