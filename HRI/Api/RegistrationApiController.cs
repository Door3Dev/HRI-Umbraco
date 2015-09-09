using System.Text.RegularExpressions;
using System.Web.Http;
using HRI.Services;
using Umbraco.Web.WebApi;

namespace HRI.Api
{
    public class RegistrationApiController: UmbracoApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public bool EmailIsInUse(string email)
        {
            var regex = new Regex(@"^((?=.*\d)|(?=.*[^a-zA-Z]))(?=.*[a-z])(?=.*[A-Z])\S{8,}");
            if (regex.IsMatch(email))
            {
                var hriService = new HriApiService();
                return hriService.EmailIsInUse(email);
            }
            return false;
        }
    }
}