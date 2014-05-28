using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.UI.Controllers
{
  public class AboutUsController : SurfaceController
  {
    public ActionResult Index()
    {
      return PartialView("WeAreDifferent");
    }

    public ActionResult WeAreDifferent()
    {
      return PartialView("WeAreDifferent");
    }

    public ActionResult OurBoard()
    {
      return PartialView("OurBoard");
    }

    public ActionResult OurStaff()
    {
      return PartialView("OurStaff");
    }
    
    public ActionResult Careers()
    {
      return PartialView("Careers");
    }
  }
}