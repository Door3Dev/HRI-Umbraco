using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace HRI.Helpers
{
    public class RequireRouteValuesAttribute : ActionMethodSelectorAttribute
    {
        public RequireRouteValuesAttribute(string[] valueNames)
        {
            ValueNames = valueNames;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            var result = ValueNames.All(value => controllerContext.HttpContext.Request[value] != null);
            return result;
        }

        public string[] ValueNames { get; private set; }
    }
}