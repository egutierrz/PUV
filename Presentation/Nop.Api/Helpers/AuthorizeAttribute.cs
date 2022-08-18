using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Nop.Api.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Customer customer = new Customer();
            var user = context.HttpContext.Items["User"];
            var access = context.HttpContext.Items["ApiAccess"];

            if (user != null && access != null)
            {
                bool accessApi = (bool)access;
                if(accessApi)
                {
                    customer = (Customer)user;
                }
                else
                {
                    // not logged in
                    context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
            if (customer.Id == 0 || !customer.Active)
            {
                // not logged in
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
