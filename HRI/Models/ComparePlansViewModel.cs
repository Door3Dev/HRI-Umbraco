using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HRI.Models
{
    public class ComparePlansViewModel
    {
        [Required]
        [Remote("ValidateZipCode", "ComparePlansSurface")]
        public string ZipCode { get; set; }
        public string County { get; set; }

        public bool CoverSelf { get; set; }
        public bool CoverSpouse { get; set; }
        public bool CoverChildren { get; set; }
        
        [Range(1, 110)]
        public int? CustomerAge { get; set; }

        [Range(1, 110)]
        public int? SpouseAge { get; set; }

        public List<float?> ChildrenAges { get; set; }

        public decimal? Income { get; set; }
        public int? NumberInHousehold { get; set; }

        public List<Product> Products { get; set; }
    }
}