using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;

namespace Nop.Web.Controllers
{
    //do not inherit it from BasePublicController. otherwise a lot of extra action filters will be called
    //they can create guest account(s), etc
    public partial class BackwardCompatibility1XController : Controller
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public BackwardCompatibility1XController(
            ICustomerService customerService,
            IWebHelper webHelper)
        {
            _customerService = customerService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> GeneralRedirect()
        {
            // use Request.RawUrl, for instance to parse out what was invoked
            // this regex will extract anything between a "/" and a ".aspx"
            var regex = new Regex(@"(?<=/).+(?=\.aspx)", RegexOptions.Compiled);
            var rawUrl = _webHelper.GetRawUrl(HttpContext.Request);
            var aspxfileName = regex.Match(rawUrl).Value.ToLowerInvariant();

           
            //no permanent redirect in this case
            return RedirectToRoute("Homepage");
        }
       
        public virtual async Task<IActionResult> RedirectUserProfile(string id)
        {
            //we can't use dash in MVC
            var userId = Convert.ToInt32(id);
            var user = await _customerService.GetCustomerByIdAsync(userId);
            if (user == null)
                return RedirectToRoutePermanent("Homepage");

            return RedirectToRoutePermanent("CustomerProfile", new { id = user.Id });
        }

        #endregion
    }
}