using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.HelpDesk;
using Nop.Services.ExportImport;
using Nop.Services.HelpDesk;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.ServiceDesk;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ServiceDeskController : BaseAdminController
    {

        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IServiceDeskModelFactory _serviceDeskModelFactory;
        private readonly IHelpDeskService _helpDeskService;
        private readonly IExportManager _exportManager;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public ServiceDeskController(IServiceDeskModelFactory serviceDeskModelFactory, 
            IPermissionService permissionService, IHelpDeskService helpDeskService, IExportManager exportManager, INotificationService notificationService)
        {
            _permissionService = permissionService;
            _serviceDeskModelFactory = serviceDeskModelFactory;
            _helpDeskService = helpDeskService;
            _exportManager = exportManager;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        // GET: ServiceDeskController
        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageServiceDesk))
                return AccessDeniedView();

            //prepare model
            var model = await _serviceDeskModelFactory.PrepareServiceDeskSearchModelAsync(new ServiceDeskSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ListRemedyRegister(ServiceDeskSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageServiceDesk))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _serviceDeskModelFactory.PrepareServiceDeskListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> ListRemedyCharts(ServiceDeskSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageServiceDesk))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var data = await _serviceDeskModelFactory.PrepareServiceDeskChartsModelAsync(searchModel);

            
            return Json(data);
        }

        [HttpPost, ActionName("ExportXML")]
        [FormValueRequired("exportxml-all")]
        public virtual async Task<IActionResult> ExportXmlAll(ServiceDeskSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageServiceDesk))
                return AccessDeniedView();

            var records = await _helpDeskService.GetAllRemedyRegisterAsync(
                createdOnFrom: model.CreatedOnFrom,
                createdOnTo: model.CreatedOnTo,
                ticketNumber: model.TicketNumber,
                satNumber: model.SatNumber,
                reportType: model.ReportTypeId,
                eventType: model.EventTypeId,
                causeEvent: model.CauseEvent,
                status: model.StatusId);

            try
            {
                var xml = await _exportManager.ExportRemedyRegistersToXmlAsync(records);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "Remedy Registers.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageServiceDesk))
                return AccessDeniedView();

            var records = new List<RemedyRegister>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                records.AddRange(await _helpDeskService.GetRecordsByIdsAsync(ids));
            }

            try
            {
                var xml = await _exportManager.ExportRemedyRegistersToXmlAsync(records);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "Remedy Registers.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        #endregion
    }
}
