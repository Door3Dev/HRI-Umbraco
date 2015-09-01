using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Web.Models;

namespace HRI.Models
{
    public class LoginFormViewModel: LoginModel
    {
        public bool RememberMe { get; set; }
    }
}