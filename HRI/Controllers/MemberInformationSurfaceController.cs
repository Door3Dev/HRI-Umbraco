using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

using log4net;
using HRI.Models;
using HRI.Services;

namespace HRI.Controllers
{
    public class MemberInformationSurfaceController : SurfaceController
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(MemberInformationSurfaceController));

        private MemberInformationService svr = new MemberInformationService();

        
        public ActionResult GetMemberProfile()
        {
            //TO DO: Get member id from database
            var member = Services.MemberService.GetByUsername(User.Identity.Name);
            string memberId = member.GetValue("yNumber").ToString();

            try
            {
                SubscriberInformation subInfo = svr.GetMemberInformation(memberId);
                if (subInfo != null)
                {
                    MembershipInformationViewModel model =  svr.GetMemberInformationViewModel(subInfo);
                    return PartialView("MembershipInformationPartial", model);
                }
                else
                {
                    return PartialView("MembershipInformationPartial", new MembershipInformationViewModel());
                }
            }
            catch
            {
                return PartialView("MembershipInformationPartial", new MembershipInformationViewModel());
            }            
            
        }
    }
}