using Umbraco.Web.Models;

namespace HRI.ViewModels
{
    public class LoginFormViewModel: LoginModel
    {
        public bool RememberMe { get; set; }
    }
}