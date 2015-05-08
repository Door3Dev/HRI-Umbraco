using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace HRI.Services
{
    public class HriApiService
    {
        private readonly IContentService _contentService;

        public HriApiService()
        {
            _contentService = ApplicationContext.Current.Services.ContentService;
        }

        public bool UpdateUserEmail(IMember member, string newEmail)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var values = new Dictionary<string, string>
                {
                    { "MemberId", member.GetValue<string>("yNumber") }, 
                    { "EmailAddress", newEmail }
                };

                var jsonContent = new JavaScriptSerializer().Serialize(values);
              
                // Get ahold of the root/home node
                var root = _contentService.GetRootContent().First();
                // Get the API uri
                var apiUri = root.GetValue<string>("apiUri");
                // Apend the command to invoke the register function
                var url = apiUri + "/Registration?p1=" + member.Email;

                var response = client.UploadString(url, jsonContent);
            }
            return true;
        }
    }
}