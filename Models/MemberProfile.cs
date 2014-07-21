using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Profile;
using System.Web.Security;

namespace HRI.Models
{    
    public class MemberProfile : ProfileBase
    {
 
        public static MemberProfile GetUserProfile(string username)
        {
            return Create(username) as MemberProfile;
        }
 
 
        [SettingsAllowAnonymous(false)]
        public string AuthGuid
        {
            get
            {
                var o = base.GetPropertyValue("auth_guid");
                if (o == DBNull.Value)
                {
                    return string.Empty;
                }
                return (string)o;
            }
            set
            {
                base.SetPropertyValue("auth_guid", value);
            }
        }
 
        [SettingsAllowAnonymous(false)]
        public string FirstName
        {
            get
            {
                var o = base.GetPropertyValue("first_name");
                if (o == DBNull.Value)
                {
                    return string.Empty;
                }
                return (string)o;
            }
            set
            {
                base.SetPropertyValue("first_name", value);
            }
        }
 
        [SettingsAllowAnonymous(false)]
        public string LastName
        {
            get
            {
                var o = base.GetPropertyValue("last_name");
                if (o == DBNull.Value)
                {
                    return string.Empty;
                }
                return (string)o;
            }
            set
            {
                base.SetPropertyValue("last_name", value);
            }
        }
         
    }
}