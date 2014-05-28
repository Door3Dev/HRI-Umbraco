using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.UI.Controllers
{
  public class BrokersController : SurfaceController
  {
    public ActionResult Index()
    {
      return PartialView("Index");
    }
  }
}