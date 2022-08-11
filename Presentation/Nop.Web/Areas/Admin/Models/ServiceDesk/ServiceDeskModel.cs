using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Web.Areas.Admin.Models.ServiceDesk
{
    /// <summary>
    /// Represents an service desk model
    /// </summary>
    public partial record ServiceDeskModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.TicketNumber")]
        public string TicketNumber { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.SatNumber")]
        public string SatNumber { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.ReportType")]
        public string ReportType { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.EventType")]
        public string EventType { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.CauseEvent")]
        public string CauseEvent { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.EventDate")]
        public string EventDate { get; set; }

        [NopResourceDisplayName("Admin.ServiceDesk.Remedy.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        #endregion
    }
}