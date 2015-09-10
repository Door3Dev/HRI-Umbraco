using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using HRI.Services;
using Umbraco.Web.WebApi;

namespace HRI.Api
{
    public class RegistrationApiController: UmbracoApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public bool EmailIsInUse([EmailAddress]string email)
        {
           var hriService = new HriApiService();
            return hriService.EmailIsInUse(email);
        }

    }
}