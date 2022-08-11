using System;

namespace Nop.Core.Domain.HelpDesk
{
    /// <summary>
    /// Represents an register of remedy
    /// </summary>
    public partial class RemedyRegister : BaseEntity
    {
        /// <summary>
        /// Gets or sets the ticket number
        /// </summary>
        public string TicketNumber { get; set; }

        /// <summary>
        /// Gets or sets the SAT number
        /// </summary>
        public string SATNumber { get; set; }

        /// <summary>
        /// Gets or sets the report type
        /// </summary>
        public int ReportType { get; set; }

        /// <summary>
        /// Gets or sets the type of fault or event
        /// </summary>
        public int FaultEventType { get; set; }

        /// <summary>
        /// Gets or sets the cause of failure or event
        /// </summary>
        public string FaultEventCause { get; set; }

        /// <summary>
        /// Gets or sets the status of record
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the date event
        /// </summary>
        public string EventDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime? CreatedOnUtc { get; set; }
    }
}