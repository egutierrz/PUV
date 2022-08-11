using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class ControlPanelController : BaseAdminController
    {
        private readonly IControlPanelModelFactory _serviceControlPanelModelFactory;
        private readonly IPermissionService _permissionService;

        public ControlPanelController(IControlPanelModelFactory serviceControlPanelModelFactory,
            IPermissionService permissionService)
        {
            _serviceControlPanelModelFactory = serviceControlPanelModelFactory;
            _permissionService = permissionService;
        }


        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageControlPanel))
                return AccessDeniedView();

            //prepare model

            return View();
        }

        public virtual async Task<IActionResult> StatusList()
        {
            return View();
        }

        public virtual async Task<IActionResult> EventsList()
        {
            return View();
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetStatusList()
        {
            var lst = new List<string[]>();
            string[] a = new string[] { "Tiger Nixon", "<i class='fas fa-check-circle success-icon'></i>", "Edinburgh", "5421", "2011/04/25", "$320,800" };
            lst.Add(a);
            a = new string[] { "Garrett Winters", "<i class='fas fa-check-circle success-icon'></i>", "Tokyo", "8422", "2011/07/25", "$170,750" };
            lst.Add(a);
            a = new string[] { "Ashton Cox", "<i class='fas fa-check-circle success-icon'></i>", "San Francisco", "1562", "2009/01/12", "$86,000" };
            lst.Add(a);
            return Json(new
            {
                dataLst = lst
            });
        }

        public virtual async Task<IActionResult> AlarmsList()
        {
            return View();
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetAlarmsList()
        {
            var lst = await _serviceControlPanelModelFactory.PrepareAlarmsTblAsync();
            return Json(new{ dataLst = JsonConvert.DeserializeObject<dynamic>(lst) });
        }

        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetEventsList()
        {
            var lst = await _serviceControlPanelModelFactory.PrepareEventsTblAsync();
            return Json(new { dataLst = JsonConvert.DeserializeObject<dynamic>(lst) });
        }

    }
}
