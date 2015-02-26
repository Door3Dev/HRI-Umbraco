using System.Text.RegularExpressions;
using System.Web.Security;
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
        private ProductsData Data = new ProductsData();


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
            TempData["enrollment"] = Request["enrollment"] != null;

            // If the model is NOT valid
            if (ModelState.IsValid == false)
            {
                // Return the user to the page
                return CurrentUmbracoPage();
            }
            if (model.CoverSelf && !model.CustomerAge.HasValue
                || model.CoverSpouse && !model.SpouseAge.HasValue
                || model.CoverChildren && model.ChildrenAges != null
                    && model.ChildrenAges.Any(x => !x.HasValue || x.Value <= 0)
                || !model.CoverSelf && !model.CoverSpouse && !model.CoverChildren)
            {
                // If the user does exist then it was a wrong password
                //don't add a field level error, just model level
                ModelState.AddModelError("viewModel", "The information you have entered is incomplete. Please enter details of the person you want to cover and submit the form again.");
                return CurrentUmbracoPage();
            }

            // Children cannot be age 30 or above.
            if (model.CoverChildren && model.ChildrenAges.Any(x => x >= 30))
            {
                ModelState.AddModelError("viewModel", "Children cannot be age 30 or above.");
                return CurrentUmbracoPage();
            }

            var zipCodeErrorMsg = ValidateZipCodeCore(model.ZipCode);
            if (zipCodeErrorMsg != null)
            {
                ModelState.AddModelError("ZipCode", zipCodeErrorMsg);
                return CurrentUmbracoPage();
            }

            var county = ZipCodes.First(z => z.zipCode == model.ZipCode).county;
            var regionNumber = Data.Regions[county];
            var regionFactor = Data.RegionsFactor[regionNumber];

            // Family factor calculation
            var familyFactor = Data.IndividualFactor;

            if (model.CoverSelf && model.CoverSpouse)
                familyFactor = Data.CoupleFactor;
            if (model.CoverChildren && model.ChildrenAges != null)
            {
                if (model.CoverSelf && !model.CoverSpouse)
                {
                    if (model.ChildrenAges.Count == 1)
                        familyFactor = Data.PrimarySubscriberAnd1DependentFactor;
                    else if (model.ChildrenAges.Count == 2)
                        familyFactor = Data.PrimarySubscriberAnd2DependentFactor;
                    else if (model.ChildrenAges.Count == 3)
                        familyFactor = Data.PrimarySubscriberAnd3DependentFactor;
                    else if (model.ChildrenAges.Count >= 4)
                        familyFactor = Data.PrimarySubscriberAnd4DependentFactor;
                }
                if (model.CoverSelf && model.CoverSpouse)
                {
                    if (model.ChildrenAges.Count == 1)
                        familyFactor = Data.CoupleAnd1DependentFactor;
                    else if (model.ChildrenAges.Count == 2)
                        familyFactor = Data.CoupleAnd2DependentFactor;
                    else if (model.ChildrenAges.Count == 3)
                        familyFactor = Data.CoupleAnd3DependentFactor;
                    else if (model.ChildrenAges.Count >= 4)
                        familyFactor = Data.CoupleAnd4DependentFactor;
                }


            }

            // Build products list
            var productListAll = new List<Product>();
            if (!model.CoverChildren || model.ChildrenAges == null)
            {
                productListAll.Add(Data.Products["EssentialCare"]);
                productListAll.Add(Data.Products["PrimarySelect"]);
                productListAll.Add(Data.Products["PrimarySelectPCMH"]);
                productListAll.Add(Data.Products["TotalIndependence"]);
            }
            else if (model.CoverSelf && model.CoverChildren && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productListAll.Add(Data.Products["EssentialCare"]);
                productListAll.Add(Data.Products["PrimarySelect"]);
                productListAll.Add(Data.Products["PrimarySelectPCMH"]);
                productListAll.Add(Data.Products["TotalIndependence"]);
            }
            else if (model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productListAll.Add(Data.Products["EssentialCareChildOnly"]);
                productListAll.Add(Data.Products["TotalIndependenceChildOnly"]);
            }
            else if (model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0
                || !model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0)
            {
                productListAll.Add(Data.Products["EssentialCare29"]);
                productListAll.Add(Data.Products["PrimarySelect29"]);
                productListAll.Add(Data.Products["PrimarySelectPCMH29"]);
                productListAll.Add(Data.Products["TotalIndependence29"]);
            }


            // Build PCMH products list
            var productList = new List<Product>();
            if (!model.CoverChildren || model.ChildrenAges == null)
            {
                productList.Add(Data.Products["EssentialCare"]);
                productList.Add(Data.Products["PrimarySelect"]);
                productList.Add(Data.Products["TotalIndependence"]);
            }
            else if (model.CoverSelf && model.CoverChildren && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productList.Add(Data.Products["EssentialCare"]);
                productList.Add(Data.Products["PrimarySelect"]);
                productList.Add(Data.Products["TotalIndependence"]);
            }
            else if (model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 1 && age <= 25) > 0)
            {
                productList.Add(Data.Products["EssentialCareChildOnly"]);
                productList.Add(Data.Products["TotalIndependenceChildOnly"]);
            }
            else if (model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0
                 || !model.CoverSelf && model.ChildrenAges.Count >= 1 && model.ChildrenAges.Count(age => age >= 26 && age <= 29) > 0)
            {
                productList.Add(Data.Products["EssentialCare29"]);
                productList.Add(Data.Products["PrimarySelect29"]);
                productList.Add(Data.Products["TotalIndependence29"]);
            }


            // Calculate price for each plan in productListAll
            foreach (var product in productListAll)
            {
                foreach (var plan in product.Plans)
                {

                    if (product.Name == "EssentialCare Child Only Plan" || product.Name == "TotalIndependence Child Only Plan")
                        // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Child Only Factor =
                        plan.Price = Math.Round(Data.BaseRate * Data.ConversionFactor * plan.RateFactor * regionFactor * Data.ChildOnly, 0);

                    else

                        // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Couple and Two Dependents Factor =
                        plan.Price = Math.Round(Data.BaseRate * Data.ConversionFactor * plan.RateFactor * regionFactor * familyFactor, 0);
                }
            }

            // Calculate price for each plan in productList
            foreach (var product in productList)
            {
                foreach (var plan in product.Plans)
                {

                    if (product.Name == "EssentialCare Child Only Plan" || product.Name == "TotalIndependence Child Only Plan")
                        // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Child Only Factor =
                        plan.Price = Math.Round(Data.BaseRate * Data.ConversionFactor * plan.RateFactor * regionFactor * Data.ChildOnly, 0);

                    else


                    // Base Rate x Conversion Factor x Platinum Select Factor x Region 2 Factor x Couple and Two Dependents Factor =
                    plan.Price = Math.Round(Data.BaseRate * Data.ConversionFactor * plan.RateFactor * regionFactor * familyFactor, 0);
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
        public ActionResult SelectPlan(string zipCode, string planId, string planPrice, bool enrollment)
        {
            if (enrollment)
        {
                var username = Membership.GetUser().UserName;
                var member = ApplicationContext.Services.MemberService.GetByUsername(username);
                member.SetValue("zipCode", zipCode);
                member.SetValue("healthplanid", planId);
                ApplicationContext.Services.MemberService.Save(member);
                // Redirect to the Enrollment Plan Confirmation
                return RedirectToUmbracoPage(31568);
            }
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
            var regex = new Regex(@"^\d{5}$");
            if (!regex.Match(zipCode).Success)
                return "Zip Code is invalid.";

            var zipCodeItem = zips.FirstOrDefault(z => z.zipCode == zipCode);
            return zipCodeItem != null ? null : "Zip Code not in service area.";
        }
        
        // internal static function for validating zip codes from other controllers
        internal static bool IsValidZipCodeInternal(string zipCode)
        {
            var zips = GetZipCodes();
            return ValidateZipCodeCore(zipCode, zips) == null;
        }

    }
}