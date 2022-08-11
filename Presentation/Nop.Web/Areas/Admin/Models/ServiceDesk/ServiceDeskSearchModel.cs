using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.ServiceDesk
{
    /// <summary>
    /// Represents a service desk search model
    /// </summary>
    public partial record ServiceDeskSearchModel : BaseSearchModel
    {
        public ServiceDeskSearchModel()
        {
            AvailableEventType = new List<SelectListItem>();
            AvailableReportType = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.TicketNumber")]
        public string TicketNumber { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.SatNumber")]
        public string SatNumber { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.ReportType")]
        public int? ReportTypeId { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.EventType")]
        public int? EventTypeId { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.CauseEvent")]
        public string CauseEvent { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.Status")]
        public int? StatusId { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        public List<SelectListItem> AvailableEventType { set; get; }
        public List<SelectListItem> AvailableReportType { set; get; }
        public List<SelectListItem> AvailableStatus { set; get; }


        #endregion
    }
}