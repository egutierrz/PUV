using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Cisco;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Sat.Cisco.SD_WAN.Security;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class ServiceMonitoringController : BaseAdminController
    {
        #region Fields


        private readonly IPermissionService _permissionService;
        private readonly IServiceMonitoringModelFactory _serviceMonitoringModelFactory;

        #endregion

        #region Methods
        public ServiceMonitoringController(IAuthenticationService authenticationService,
            IPermissionService permissionService,
            IServiceMonitoringModelFactory serviceMonitoringModelFactory)
        {
            _permissionService = permissionService;
            _serviceMonitoringModelFactory = serviceMonitoringModelFactory;
        }

        // GET: IndexAsync
        public async Task<IActionResult> IndexAsync()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageServiceMonitoring))
                return AccessDeniedView();

            var model = await _serviceMonitoringModelFactory.PrepareServiceServiceMonitoringAsync();
            if (model == null)
                return AccessDeniedView();

            HttpContext.Response.Cookies.Append("JSESSIONID", model.JSessionId);

            return View(model);
        }

        public virtual async Task<IActionResult> ViewPercentUtilization(string jSessionId, string token)
        {
            var w = await _serviceMonitoringModelFactory.PrepareViewPercent(jSessionId, token);
            var j = JsonConvert.DeserializeObject<dynamic>(w);
            return Json(new { message = 1, data = j });
        }

        public virtual async Task<IActionResult> ViewDetail(string url, string jSessionId)
        {
            var w = await _serviceMonitoringModelFactory.PrepareViewDetail(url, jSessionId);
            var j = JsonConvert.DeserializeObject<dynamic>(w);
            return Json(new { message = 1, data = j });
        }

        public async Task<IActionResult> GetTransportHealthChart()
        {
            var model = await _serviceMonitoringModelFactory.PrepareTransportHealthChartModel();
            return Json(new { success = 1 });
        }
        #endregion
    }
}
