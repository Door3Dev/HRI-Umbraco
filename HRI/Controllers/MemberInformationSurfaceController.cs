using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

using log4net;
using HRI.Models;
using HRI.Services;
using HRI.ViewModels;

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
                    logger.Error("No information for the member was available in the WEB ODS for member id: " + memberId);
                    return PartialView("MembershipInformationPartial", new MembershipInformationViewModel());
                }
            }
            catch(Exception ex)
            {
                logger.Error("No Member profile information for member id: " + memberId + ", because of error: " + ex.Message);
                return PartialView("MembershipInformationPartial", new MembershipInformationViewModel());
            }            
            
        }
    }
}