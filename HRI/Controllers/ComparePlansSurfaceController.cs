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
    public class ZipCountyRegion
    {
        public ZipCountyRegion(string z, string c, string r) { zipCode = z; county = c; region = r; }
        public string zipCode;
        public string county;
        public string region;
    }

    public class ComparePlansSurfaceController : SurfaceController
    {
        // TO-DO: Create a list of all zip codes / counties / region#
        private List<ZipCountyRegion> RegionData = new List<ZipCountyRegion>();

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
                    RegionData.Add(new ZipCountyRegion(zip, county, String.Empty));
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
            var applicableZipCodes = RegionData.GroupBy(o => o.zipCode)
                                                .Where(x => x.First().zipCode.StartsWith(zipCode))
                                                .Select(x => new ZipCountyRegion(x.First().zipCode, x.First().county, x.First().region))
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
            // TO-DO: Determine Plan Eligibility

            // TO-DO: Calculate cost per plan

            // TO-DO: Create a plans ViewModel

            // TO-DO: Route to Plans View with ViewModel
            return RedirectToCurrentUmbracoPage();
        }
    }
}