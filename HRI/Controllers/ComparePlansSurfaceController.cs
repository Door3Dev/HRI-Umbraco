using HRI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class ComparePlansSurfaceController : SurfaceController
    {
        private class ZipCode
        {
            public ZipCode(string z, string c) { zipCode = z; county = c; }
            public string zipCode;
            public string county;
        }

        private List<ZipCode> ZipCodes = new List<ZipCode>();

        #region Regions Counties 
        private Dictionary<string, int> Regions = new Dictionary<string, int>() { 
            {"Warren", 1},
            {"Washington", 1},
            {"Rensselaer", 1},
            {"Columbia", 1},
            {"Greene", 1},
            {"Schoharie", 1},
            {"Montgomery", 1},
            {"Fulton", 1},
            {"Saratoga", 1},
            {"Schenectady", 1},
            {"Albany", 1},
            {"Orleans", 2},
            {"Genesee", 2},
            {"Wyoming", 2},
            {"Allegany", 2},
            {"Cattaraugus", 2},
            {"Chautauqua", 2},
            {"Erie", 2},
            {"Niagara", 2},
            {"Delaware", 3},
            {"Ulster", 3},
            {"Dutchess", 3},
            {"Putnam", 3},
            {"Orange", 3},
            {"Sullivan", 3},
            {"Westchester", 4},
            {"Rockland", 4},
            {"New York", 4},
            {"Kings", 4},
            {"Queens", 4},
            {"Richmond", 4},
            {"Bronx", 4},
            {"Monroe", 5},
            {"Wayne", 5},
            {"Seneca", 5},
            {"Yates", 5},
            {"Livington", 5},
            {"Ontario", 5},
            {"Onondaga", 6},
            {"Cortland", 6},
            {"Broome", 6},
            {"Tioga", 6},
            {"Chemug", 6},
            {"Steuben", 6},
            {"Schuyler", 6},
            {"Tompkins", 6},
            {"Cayuga", 6},
            {"Clinton", 7},
            {"Franklin", 7},
            {"Essex", 7},
            {"Hamilton", 7},
            {"Herkimer", 7},
            {"Otsego", 7},
            {"Chenango", 7},
            {"Madison", 7},
            {"Oneida", 7},
            {"Lewis", 7},
            {"Oswego", 7},
            {"Jefferson", 7},
            {"St. Lawrence", 7},
            {"Nassau", 8},
            {"Suffolk", 8}
        };
        #endregion

        #region Base Rates
        private double BaseRate = 322.73;
        private Dictionary<int, double> RegionsFactor = new Dictionary<int, double>
        {
            {1, 0.8408},
            {2, 0.7483},
            {3, 0.9445},
            {4, 1.0865},
            {5, 0.7737},
            {6, 0.7232},
            {7, 0.7950},
            {8, 1.0865}
        };
        private double ConversionFactor = 1.00;   
        private double IndividualFactor = 1.00;
        private double CoupleFactor = 2.00;
        private double PrimarySubscriberAnd1DependentFactor = 1.70;
        private double PrimarySubscriberAnd2DependentFactor = 1.70;
        private double PrimarySubscriberAnd3DependentFactor = 1.70;
        private double CoupleAnd1DependentFactor = 2.85;
        private double CoupleAnd2DependentFactor = 2.85;
        private double CoupleAnd3DependentFactor = 2.85;
        private double ChildOnly = 0.412;
        #endregion

        #region Products Data
        private Dictionary<string, Product> Products = new Dictionary<string,Product>() {
            { 
                "EssentialCare",
                new Product {
                    Name = "EssentialCare Plan",
                    Description = "EssentialCare is our standard plan offering, aligning with state and federal requirements for deductibles, copays, and other coverage benefits, allowing consumers to compare EssentialCare “apples to apples” with plans from other insurers.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0010004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6795,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4284,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2224,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.000000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010005-00",
                            MetalTier = "Catastrophic",
                            RateFactor = 0.5253,
                            Description = PlansDescriptioData.EssentialCareCatastrophicPlanDescription
                        }
                    }
                }
            },
             { 
                "TotalIndependence",
                new Product {
                    Name = "TotalIndependence Plan",
                    Description = "This low premium, high deductible plan is a basic option for individuals.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0730001-00",
                            MetalTier = "Gold",
                            RateFactor = 1.2787,
                            Description = PlansDescriptioData.TotalIndependenceGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0700001-00",
                            MetalTier = "Silver",
                            RateFactor = 1.0838,
                            Description = PlansDescriptioData.TotalIndependenceSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0670001-00",
                            MetalTier = "Bronze",
                            RateFactor = 0.9514,
                            Description = PlansDescriptioData.TotalIndependenceBronzePlanDescription
                        }
                    }
                }
            },
            { 
                "PrimarySelect", 
                new Product {
                    Name = "PrimarySelect Plan",
                    Description = "PrimarySelect is Health Republic’s signature program, emphasizing the role of a primary care physician in our members’ health. After selecting a primary care physician, visits are always free of charge. This promotes preventive care and wellness screenings, which leads to better health outcomes for patients and lower costs for everyone.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0030004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6101,
                            Description = PlansDescriptioData.PrimarySelectPlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4273,
                            Description = PlansDescriptioData.PrimarySelectGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2216,
                            Description = PlansDescriptioData.PrimarySelectSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.1218,
                            Description = PlansDescriptioData.PrimarySelectBronzePlanDescription
                        },
                    }
                }
            },

            { 
                "PrimarySelectPCMH", 
                new Product {
                    Name = "PrimarySelect PCMH Plan",
                    Description = "Similar in structure to PrimarySelect, PrimarySelect PCMH (Patient Centered Medical Homes) focuses on comprehensive patient care with a specialized network of patient centered medical homes certified by the National Committee for Quality Assurance (NCQA). Only available at the Silver level, PrimarySelect PCMH is a cost-friendly option for those looking to get the most out of their health plan.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0040002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.1526,
                            Description = PlansDescriptioData.PrimarySelectSilverPCMHPlanDescription
                        }
                    }
                }
            },

            { 
                "EssentialCareChildOnly", 
                new Product {
                    Name = "EssentialCare Child Only Plan",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0020004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6795,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4284,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2224,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.0000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        }
                    }
                }
            },

              { 
                "TotalIndependenceChildOnly", 
                new Product {
                    Name = "TotalIndependence Child Only Plan",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0740001-00",
                            MetalTier = "Gold",
                            RateFactor = 1.2787,
                            Description = PlansDescriptioData.TotalIndependenceGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0710001-00",
                            MetalTier = "Silver",
                            RateFactor = 1.0838,
                            Description = PlansDescriptioData.TotalIndependenceSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0680001-00",
                            MetalTier = "Bronze",
                            RateFactor = 0.9514,
                            Description = PlansDescriptioData.TotalIndependenceBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "EssentialCare29", 
                new Product {
                    Name = "EssentialCare Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0090004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6795,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4284,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2224,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.000000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        }
                    }
                }
            },

             { 
                "TotalIndependence29", 
                new Product {
                    Name = "TotalIndependence Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0850001-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6314,
                            Description = PlansDescriptioData.TotalIndependencePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0750001-00",
                            MetalTier = "Gold",
                            RateFactor = 1.2787,
                            Description = PlansDescriptioData.TotalIndependenceGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0720001-00",
                            MetalTier = "Silver",
                            RateFactor = 1.0838,
                            Description = PlansDescriptioData.TotalIndependenceSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0690001-00",
                            MetalTier = "Bronze",
                            RateFactor = 0.9514,
                            Description = PlansDescriptioData.TotalIndependenceBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "PrimarySelect29", 
                new Product {
                    Name = "PrimarySelect Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0130004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6101,
                            Description = PlansDescriptioData.PrimarySelectPlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4273,
                            Description = PlansDescriptioData.PrimarySelectGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2216,
                            Description = PlansDescriptioData.PrimarySelectSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.1218,
                            Description = PlansDescriptioData.PrimarySelectBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "PrimarySelectPCMH29", 
                new Product {
                    Name = "PrimarySelect PCMH Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0150002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.1526,
                            Description = PlansDescriptioData.PrimarySelectSilverPCMHPlanDescription
                        }
                    }
                }
            }
        };
        #endregion

        public ComparePlansSurfaceController()
        {
            ZipCodes = GetZipCodes();
        }

        private static List<ZipCode> GetZipCodes()
        {
            var zips = new List<ZipCode>();
            var zipCodePath = HostingEnvironment.MapPath("~/App_Data/zip-codes.csv");
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            using (var rd = new StreamReader(zipCodePath))
            {
                rd.ReadLine();
                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(',');
                    var endsWithAny = splits[0].Contains('*');
                    var zip = endsWithAny ? splits[0].Substring(0, splits[0].Length - 1) : splits[0];
                    var county = textInfo.ToTitleCase(splits[7].ToLower().Trim());
                    zips.Add(new ZipCode(zip, county));
                }
            }
            return zips;
        }

        /// <summary>
        /// This function will return all the ZipCodeRegion elements that match the given zip code.
        /// 
        /// To create a dropdown of a particular field in javascript, do an AJAX call to this function and use javascript to build the list:
        /// 
        /// http://stackoverflow.com/questions/2637694/how-to-populate-a-dropdownlist-with-json-data-in-jquery
        ///  
        /// </summary>
        /// <param name="zipCode">The zipcode to get counties for.</param>
        /// <returns></returns>
        [HttpGet]
        public string GetZipCodeList(string zipCode)
        {
            var applicableZipCodes = ZipCodes.GroupBy(o => o.zipCode)
                                                .Where(x => x.First().zipCode.StartsWith(zipCode))
                                                .Select(x => new ZipCode(x.First().zipCode, x.First().county))
                                                .Take(100)
                                                .ToList();
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return oSerializer.Serialize(applicableZipCodes);
        }

        /// <summary>
        /// This function will take a ComparePlansViewModel and determine what plans a person is eligible for - and at what price estimate.
        /// The user is then routed to a page where they can view and select one of the options.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ShowPlans(ComparePlansViewModel model)
        {
            TempData["model"] = model;

            // If the model is NOT valid
            if (ModelState.IsValid == false)
            {
                // Return the user to the page
                return CurrentUmbracoPage();
            }
            if (model.CoverSelf && !model.CustomerAge.HasValue
                || model.CoverSpouse && !model.SpouseAge.HasValue
                || model.CoverChildren && model.ChildrenAges != null 
                    && (model.ChildrenAges.Any(x => !x.HasValue) 
                        || model.ChildrenAges.Any(x => x > 30))
                || !model.CoverSelf && !model.CoverSpouse && !model.CoverChildren)
            {
                // If the user does exist then it was a wrong password
                //don't add a field level error, just model level
                ModelState.AddModelError("viewModel", "The information you have entered is incomplete. Please enter details of the person you want to cover and submit the form again.");
                return CurrentUmbracoPage();
            }

            var zipCodeErrorMsg = ValidateZipCodeCore(model.ZipCode);
            if (zipCodeErrorMsg != null)
            {
                ModelState.AddModelError("ZipCode", zipCodeErrorMsg);
                return CurrentUmbracoPage();
            }

            var county = ZipCodes.First(z => z.zipCode == model.ZipCode).county;
            var regionNumber = Regions[county];
            var regionFactor = RegionsFactor[regionNumber];

            // Family factor calculation
            var familyFactor = IndividualFactor;

            if (model.CoverSelf && model.CoverSpouse)
                familyFactor = CoupleFactor;
            if (model.CoverChildren && model.ChildrenAges != null)
            {
                if (model.CoverSelf && !model.CoverSpouse)
                {
                    if (model.ChildrenAges.Count == 1)
                        familyFactor = PrimarySubscriberAnd1DependentFactor;
                    else if (model.ChildrenAges.Count == 2)
                        familyFactor = PrimarySubscriberAnd2DependentFactor;
                    else if (model.ChildrenAges.Count == 3)
                        familyFactor = PrimarySubscriberAnd3DependentFactor;
                }
                if (model.CoverSelf && model.CoverSpouse)
                {
                    if (model.ChildrenAges.Count == 1)
                        familyFactor = CoupleAnd1DependentFactor;
                    else if (model.ChildrenAges.Count == 2)
                        familyFactor = CoupleAnd2DependentFactor;
                    else if (model.ChildrenAges.Count == 3)
                        familyFactor = CoupleAnd3DependentFactor;
                }
              
                       
            }

            // Build products list
            var productListAll = new List<Product>();
            if (!model.CoverChildren || model.ChildrenAges == null)
            {
                productListAll.Add(Products["EssentialCare"]);
                productListAll.Add(Products["PrimarySelect"]);
                productListAll.Add(Products["PrimarySelectPCMH"]);
                productListAll.Add(Products["TotalIndependence"]);
            }
            else if (model.CoverSelf && model.CoverChildren && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productListAll.Add(Products["EssentialCare"]);
                productListAll.Add(Products["PrimarySelect"]);
                productListAll.Add(Products["PrimarySelectPCMH"]);
                productListAll.Add(Products["TotalIndependence"]);
            }
            else if (model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productListAll.Add(Products["EssentialCareChildOnly"]);
                productListAll.Add(Products["TotalIndependenceChildOnly"]);
            }
            else if (model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0 
                || !model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0)
                
            {
                productListAll.Add(Products["EssentialCare29"]);
                productListAll.Add(Products["PrimarySelect29"]);
                productListAll.Add(Products["PrimarySelectPCMH29"]);
                productListAll.Add(Products["TotalIndependence29"]);
            }


            // Build PCMH products list
            var productList = new List<Product>();
            if (!model.CoverChildren || model.ChildrenAges == null)
            {
                productList.Add(Products["EssentialCare"]);
                productList.Add(Products["PrimarySelect"]);
                productList.Add(Products["TotalIndependence"]);
            }
            else if (model.CoverSelf && model.CoverChildren && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productList.Add(Products["EssentialCare"]);
                productList.Add(Products["PrimarySelect"]);
                productList.Add(Products["TotalIndependence"]);
            }
           else if (model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0
                || !model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0)
            {
                productList.Add(Products["EssentialCare29"]);
                productList.Add(Products["PrimarySelect29"]);
                productList.Add(Products["TotalIndependence29"]);
            }


            // Calculate price for each plan in productListAll
            foreach (var product in productListAll)
            {
                foreach (var plan in product.Plans)
                {
                   
                    if (product.Name == "EssentialCare Child Only Plan" || product.Name == "TotalIndependence Child Only Plan")
                    // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Child Only Factor =
                        plan.Price = Math.Round(BaseRate * ConversionFactor * plan.RateFactor * regionFactor * ChildOnly, 0);

                    else 

                    // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Couple and Two Dependents Factor =
                    plan.Price = Math.Round(BaseRate * ConversionFactor * plan.RateFactor * regionFactor * familyFactor, 0);
                }
            }

            // Calculate price for each plan in productList
            foreach (var product in productList)
            {
                foreach (var plan in product.Plans)
                {
                    
                        // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Couple and Two Dependents Factor =
                        plan.Price = Math.Round(BaseRate * ConversionFactor * plan.RateFactor * regionFactor * familyFactor, 0);
                }
            }

         // show rates based on county            
            if (county == "Bronx")
            {
                model.Products = productListAll;
                model.County = county;
            }
           else if (county == "Essex")
            {
                model.Products = productListAll;
                model.County = county;
            }
             else if (county == "Hamilton")
            {
                model.Products = productListAll;
                model.County = county;
            }
             else if (county == "Kings")
            {
                model.Products = productListAll;
                model.County = county;
            }
            else if (county == "Nassau")
            {
                model.Products = productListAll;
                model.County = county;
            }
            else if (county == "New York")
            {
                model.Products = productListAll;
                model.County = county;
            }
             else if (county == "Queens")
            {
                model.Products = productListAll;
                model.County = county;
            }
             else if (county == "Richmond")
            {
                model.Products = productListAll;
                model.County = county;
            }
              else if (county == "Rockland")
            {
                model.Products = productListAll;
                model.County = county;
            }
             else if (county == "Suffolk")
            {
                model.Products = productListAll;
                model.County = county;
            }
            else if (county == "Westchester")
            {
                model.Products = productListAll;
                model.County = county;
            }

            else 
            {
                model.Products = productList;
                model.County = county;
            }


            TempData["ShowPlans"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public ActionResult SelectPlan(string zipCode, string planId, string planPrice)
        {
            TempData["ZipCode"] = zipCode;
            TempData["PlanId"] = planId;
            // Redirect to the registration page
            return RedirectToUmbracoPage(1343);

        }

        public JsonResult ValidateZipCode(string ZipCode)
        {
            var errorMsg = ValidateZipCodeCore(ZipCode);

            return errorMsg == null
                ? Json(true, JsonRequestBehavior.AllowGet) : Json(errorMsg, JsonRequestBehavior.AllowGet);
        }

        private string ValidateZipCodeCore(string zipCode)
        {
            return ValidateZipCodeCore(zipCode, ZipCodes);
        }

        private static string ValidateZipCodeCore(string zipCode, List<ZipCode> zips)
        {
            var zipCodeItem = zips.FirstOrDefault(z => z.zipCode == zipCode);
            return zipCodeItem != null ? null : "Zip Code is incorrect.";
        }
        
        // internal static function for validating zip codes from other controllers
        internal static bool IsValidZipCodeInternal(string zipCode)
        {
            var zips = GetZipCodes();
            return ValidateZipCodeCore(zipCode, zips) == null;
        }

    }
}