using HRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class ComparePlansSurfaceController : SurfaceController
    {
        private class ZipCountyRegion
        {
            public ZipCountyRegion(int z, string c, int r){zipCode = z; county = c; region = r;}
            public int zipCode;
            public string county;
            public int region;
        }

        // TO-DO: Create a list of all zip codes / counties / region#
        private List<ZipCountyRegion> RegionData = new List<ZipCountyRegion>()
        {
            new ZipCountyRegion(10000, "Manhattan", 0),
            new ZipCountyRegion(10001, "Brooklyn", 1),
            new ZipCountyRegion(10001, "Harlem", 2),
            new ZipCountyRegion(10003, "Williamsburg", 3),
            new ZipCountyRegion(10004, "Tarrytown", 4),
            new ZipCountyRegion(10004, "Park Slope", 5),
            new ZipCountyRegion(10004, "Queens", 6)
        };

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
        public string GetRegionsList(int zipCode)
        {
            var applicableCounties = RegionData.Where(o => o.zipCode == zipCode).ToList();           
            var oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string sJSON = oSerializer.Serialize(applicableCounties);
            return sJSON;
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