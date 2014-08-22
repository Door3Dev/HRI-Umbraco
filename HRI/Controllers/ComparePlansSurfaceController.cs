using HRI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Serialization;
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
        private double BaseRate = 214.578957;
        private Dictionary<int, double> RegionsFactor = new Dictionary<int, double>
        {
            {1, 1.086664},
            {2, 1.017235},
            {3, 1.220650},
            {4, 1.431276},
            {5, 1.00},
            {6, 1.056058},
            {7, 1.027582},
            {8, 1.431276}
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
        #endregion

        #region Products Data
        private Dictionary<string, Product> Products = new Dictionary<string,Product>() {
            { 
                "EssentialCare",
                new Product {
                    Name = "EssentialCare Plan",
                    Description = "Benefits, deductibles, co-pays, and all other plan features adhere to the NYS requirements for the “standard plan,” allowing you to make a true comparison across insurers.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0010004",
                            MetalTier = "Platinum",
                            RateFactor = 1.679497,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010003",
                            MetalTier = "Gold",
                            RateFactor = 1.428384,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010002",
                            MetalTier = "Silver",
                            RateFactor = 1.261463,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010001",
                            MetalTier = "Bronze",
                            RateFactor = 1.000000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010005",
                            MetalTier = "Catastrophic",
                            RateFactor = 0.692666,
                            Description = PlansDescriptioData.EssentialCareCatastrophicPlanDescription
                        }
                    }
                }
            },
            { 
                "PrimarySelect", 
                new Product {
                    Name = "PrimarySelect Plan",
                    Description = "Breaks down barriers to accessing the healthcare you deserve - visits to your selected primary care physician are free and you get access to the full MagnaCare Extra network of specialty care providers without a referral.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0030004",
                            MetalTier = "Platinum",
                            RateFactor = 1.610092,
                            Description = PlansDescriptioData.PrimarySelectPlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030003",
                            MetalTier = "Gold",
                            RateFactor = 1.427252,
                            Description = PlansDescriptioData.PrimarySelectGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030002",
                            MetalTier = "Silver",
                            RateFactor = 1.260554,
                            Description = PlansDescriptioData.PrimarySelectSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030001",
                            MetalTier = "Bronze",
                            RateFactor = 0.866851,
                            Description = PlansDescriptioData.PrimarySelectBronzePlanDescription
                        },
                    }
                }
            },

            { 
                "PrimarySelectEPO", 
                new Product {
                    Name = "PrimarySelect EPO Plan",
                    Description = "Choose your primary care physician from our list of patient-centered medical homes to help you manage your health options and still enjoy direct access to the full MagnaCare Extra network, too.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0040002",
                            MetalTier = "Silver",
                            RateFactor = 1.189375,
                            Description = PlansDescriptioData.PrimarySelectSilverEPOPlanDescription
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
                            HiosId = "71644NY0020004",
                            MetalTier = "Platinum",
                            RateFactor = 0.691953,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020003",
                            MetalTier = "Gold",
                            RateFactor = 0.588494,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020002",
                            MetalTier = "Silver",
                            RateFactor = 0.519723,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020001",
                            MetalTier = "Bronze",
                            RateFactor = 0.412000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
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
                            HiosId = "71644NY0090004",
                            MetalTier = "Platinum",
                            RateFactor = 1.679497,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090003",
                            MetalTier = "Gold",
                            RateFactor = 1.428384,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090002",
                            MetalTier = "Silver",
                            RateFactor = 1.261463,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090001",
                            MetalTier = "Bronze",
                            RateFactor = 1.000000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
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
                            HiosId = "71644NY0130004",
                            MetalTier = "Platinum",
                            RateFactor = 1.610092,
                            Description = PlansDescriptioData.PrimarySelectPlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130003",
                            MetalTier = "Gold",
                            RateFactor = 1.427252,
                            Description = PlansDescriptioData.PrimarySelectGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130002",
                            MetalTier = "Silver",
                            RateFactor = 1.260554,
                            Description = PlansDescriptioData.PrimarySelectSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130001",
                            MetalTier = "Bronze",
                            RateFactor = 0.866851,
                            Description = PlansDescriptioData.PrimarySelectBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "PrimarySelectEPO29", 
                new Product {
                    Name = "PrimarySelect EPO Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0150002",
                            MetalTier = "Silver",
                            RateFactor = 1.189375,
                            Description = PlansDescriptioData.PrimarySelectSilverEPOPlanDescription
                        }
                    }
                }
            }
        };
        #endregion

        public ComparePlansSurfaceController()
        {
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
                    var county = textInfo.ToTitleCase(splits[19].ToLower());
                    ZipCodes.Add(new ZipCode(zip, county));
                }
            }
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
        public ActionResult ShowPlans([Bind(Prefix = "viewModel")]ComparePlansViewModel model)
        {
            // If the model is NOT valid
            if (ModelState.IsValid == false)
            {
                // Return the user to the page
                return CurrentUmbracoPage();
            }
            if (model.CoverSelf && !model.CustomerAge.HasValue
                || model.CoverSpouse && !model.SpouseAge.HasValue
                || model.ChildrenAges != null && model.ChildrenAges.Count(x => x > 30) > 0)
            {
                // If the user does exist then it was a wrong password
                //don't add a field level error, just model level
                ModelState.AddModelError("viewModel", "The information you have entered is incomplete. Please enter details of the person you want to cover and submit the form again.");
                return CurrentUmbracoPage();
            }

            var county = ZipCodes.First(z => z.zipCode == model.ZipCode).county;
            var regionNumber = Regions[county];
            var regionFactor = RegionsFactor[regionNumber];

            // Family factor calculation
            var familyFactor = IndividualFactor;
            if (model.CoverSelf && model.CoverSpouse)
                familyFactor = CoupleFactor;
            if (model.ChildrenAges != null) {
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
            var productList = new List<Product>();
            if (model.ChildrenAges == null)
            {
                productList.Add(Products["EssentialCare"]);
                productList.Add(Products["PrimarySelect"]);
                productList.Add(Products["PrimarySelectEPO"]);
            }
            else if (model.CoverSelf
                && model.ChildrenAges.Count >= 1
                && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0)
            {
                productList.Add(Products["EssentialCare29"]);
                productList.Add(Products["PrimarySelect29"]);
                productList.Add(Products["PrimarySelectEPO29"]);
            }
            else if (model.ChildrenAges.Count >= 1)
            {
                productList.Add(Products["EssentialCareChildOnly"]);
            }

            // Calculate price for each plan
            foreach (var product in productList)
            {
                foreach (var plan in product.Plans)
                {
                    // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Couple and Two Dependents Factor =
                    plan.Price = Math.Round(BaseRate * ConversionFactor * plan.RateFactor * regionFactor * familyFactor, 0);
                }
            }

            model.Products = productList;
            model.County = county;

            TempData["model"] = model;
            TempData["ShowPlans"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public ActionResult SelectPlan(string zipCode, string planId, string planPrice)
        {
            TempData["NewUser"] = true;
            TempData["ZipCode"] = zipCode;
            TempData["PlanId"] = planId;
            TempData["PlanPrice"] = planPrice;
            TempData["IsEnrolled"] = true;
            // Redirect to the registration page
            return RedirectToUmbracoPage(1343);

        }
    }
}