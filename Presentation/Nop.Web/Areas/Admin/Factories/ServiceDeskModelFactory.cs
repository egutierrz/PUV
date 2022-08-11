using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.HelpDesk;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.ServiceDesk;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the service desk model factory implementation
    /// </summary>
    public partial class ServiceDeskModelFactory : IServiceDeskModelFactory
    {

        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IHelpDeskService _helpDeskService;

        #endregion

        #region Ctor

        public ServiceDeskModelFactory(IDateTimeHelper dateTimeHelper, 
            IHelpDeskService helpDeskService)
        {
            _dateTimeHelper = dateTimeHelper;
            _helpDeskService = helpDeskService;
        }

        #endregion

        /// <summary>
        /// Prepare service desk search model
        /// </summary>
        /// <param name="searchModel">Service desk search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the service desk search model
        /// </returns>
        public virtual async Task<ServiceDeskSearchModel> PrepareServiceDeskSearchModelAsync(ServiceDeskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableReportType.Add(new SelectListItem() { Text = "Seleccione", Value = null , Selected = true });
            searchModel.AvailableReportType.Add(new SelectListItem() { Text = "Incidente", Value = "1" });
            searchModel.AvailableReportType.Add(new SelectListItem() { Text = "Requerimiento", Value = "2" });

            searchModel.AvailableEventType.Add(new SelectListItem() { Text = "Seleccione", Value = null, Selected = true });
            searchModel.AvailableEventType.Add(new SelectListItem() { Text = "User Service Restoration", Value = "1" });
            searchModel.AvailableEventType.Add(new SelectListItem() { Text = "Infraestructure Restoration", Value = "2" });

            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Seleccione", Value = null, Selected = true });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Assigned", Value = "1" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Pending", Value = "2" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Resolved", Value = "3" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Closed", Value = "4" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Cancelled", Value = "5" });

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged service desk list model
        /// </summary>
        /// <param name="searchModel">Service desk search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the service desk list model
        /// </returns>
        public virtual async Task<ServiceDeskListModel> PrepareServiceDeskListModelAsync(ServiceDeskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableReportType.Add(new SelectListItem() { Text = "Seleccione", Value = null, Selected = true });
            searchModel.AvailableReportType.Add(new SelectListItem() { Text = "Incidente", Value = "1" });
            searchModel.AvailableReportType.Add(new SelectListItem() { Text = "Requerimiento", Value = "2" });

            searchModel.AvailableEventType.Add(new SelectListItem() { Text = "Seleccione", Value = null, Selected = true });
            searchModel.AvailableEventType.Add(new SelectListItem() { Text = "User Service Restoration", Value = "1" });
            searchModel.AvailableEventType.Add(new SelectListItem() { Text = "Infraestructure Restoration", Value = "2" });

            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Seleccione", Value = null, Selected = true });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Assigned", Value = "1" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Pending", Value = "2" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Resolved", Value = "3" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Closed", Value = "4" });
            searchModel.AvailableStatus.Add(new SelectListItem() { Text = "Cancelled", Value = "5" });

            //get parameters to filter records
            var startDateValue = searchModel.CreatedOnFrom == null ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = searchModel.CreatedOnTo == null ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            //get log
            var remedyRegisters = await _helpDeskService.GetAllRemedyRegisterAsync(createdOnFrom: startDateValue, createdOnTo: endDateValue,
                ticketNumber: searchModel.TicketNumber, satNumber: searchModel.SatNumber, reportType: searchModel.ReportTypeId, eventType: searchModel.EventTypeId,
                causeEvent: searchModel.CauseEvent, status: searchModel.StatusId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            if (remedyRegisters is null)
                return new ServiceDeskListModel();

            var model = await new ServiceDeskListModel().PrepareToGridAsync(searchModel, remedyRegisters, () =>
            {
                return remedyRegisters.SelectAwait(async item =>
                {
                    //fill in model values from the entity
                    var itemModel = new ServiceDeskModel();
                    itemModel.Id = item.Id;
                    itemModel.TicketNumber = item.TicketNumber;
                    itemModel.SatNumber = item.SATNumber;
                    itemModel.ReportType = searchModel.AvailableReportType.Where(r => r.Value == item.ReportType.ToString()).SingleOrDefault().Text;
                    itemModel.EventType = searchModel.AvailableEventType.Where(r => r.Value == item.FaultEventType.ToString()).SingleOrDefault().Text;
                    itemModel.CauseEvent = item.FaultEventCause;
                    itemModel.Status = searchModel.AvailableStatus.Where(r => r.Value == item.Status.ToString()).SingleOrDefault().Text;                    
                    itemModel.EventDate = item.EventDate;
                    itemModel.CreatedOn = (DateTime)item.CreatedOnUtc;

                    return itemModel;
                });
            });
            return model;
        }

        public async Task<Dictionary<int,int[]>> PrepareServiceDeskChartsModelAsync(ServiceDeskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            Dictionary<int, int[]> result = new Dictionary<int, int[]>();

            //get parameters to filter records
            var startDateValue = searchModel.CreatedOnFrom == null ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = searchModel.CreatedOnTo == null ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            //get log
            var remedyRegisters = await _helpDeskService.GetAllRemedyRegisterAsync(createdOnFrom: startDateValue, createdOnTo: endDateValue,
                ticketNumber: searchModel.TicketNumber, satNumber: searchModel.SatNumber, reportType: searchModel.ReportTypeId, eventType: searchModel.EventTypeId,
                causeEvent: searchModel.CauseEvent, status: searchModel.StatusId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            if (remedyRegisters is null)
                return result;

            int inAssigned = remedyRegisters.Where(r => r.ReportType == 1 && r.Status == 1).Count();
            int inPending = remedyRegisters.Where(r => r.ReportType == 1 && r.Status == 2).Count();
            int inResolved = remedyRegisters.Where(r => r.ReportType == 1 && r.Status == 3).Count();
            int inClosed = remedyRegisters.Where(r => r.ReportType == 1 && r.Status == 4).Count();
            int inCancelled = remedyRegisters.Where(r => r.ReportType == 1 && r.Status == 5).Count();
            int[] incidentes = { inAssigned, inPending, inResolved, inClosed, inCancelled };

            result.Add(1, incidentes);

            int reAssigned = remedyRegisters.Where(r => r.ReportType == 2 && r.Status == 1).Count();
            int rePending = remedyRegisters.Where(r => r.ReportType == 2 && r.Status == 2).Count();
            int reResolved = remedyRegisters.Where(r => r.ReportType == 2 && r.Status == 3).Count();
            int reClosed = remedyRegisters.Where(r => r.ReportType == 2 && r.Status == 4).Count();
            int reCancelled = remedyRegisters.Where(r => r.ReportType == 2 && r.Status == 5).Count();
            int[] requerimientos = { reAssigned, rePending, reResolved, reClosed, reCancelled };

            result.Add(2, requerimientos);

            return result;
        }
    }
}
