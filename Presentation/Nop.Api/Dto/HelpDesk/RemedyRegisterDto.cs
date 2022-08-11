using Nop.Core.Domain.HelpDesk;
namespace Nop.Api.Dto.HelpDesk
{
    public class RemedyRegisterDto
    {

        /// <summary>
        /// Gets or sets the ticket number
        /// </summary>
        public string? TicketNumber { get; set; }

        /// <summary>
        /// Gets or sets the SAT number
        /// </summary>
        public string? SATNumber { get; set; }

        /// <summary>
        /// Gets or sets the report type
        /// </summary>
        public string? ReportType { get; set; }

        /// <summary>
        /// Gets or sets the type of fault or event
        /// </summary>
        public string? FaultEventType { get; set; }

        /// <summary>
        /// Gets or sets the cause of failure or event
        /// </summary>
        public string? FaultEventCause { get; set; }

        /// <summary>
        /// Gets or sets the status of record
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the date and time of event
        /// </summary>
        public String? EventDate { get; set; }

        public RemedyRegisterDto()
        { }

        public RemedyRegisterDto(string ticketNumber, string sATNumber, string reportType, string faultEventType, string faultEventCause, string status, String eventDate)
        {
            TicketNumber = ticketNumber;
            SATNumber = sATNumber;
            ReportType = reportType;
            FaultEventType = faultEventType;
            FaultEventCause = faultEventCause;
            Status = status;
            EventDate = eventDate;
        }

        public RemedyRegister GetRemedyRegisterDomain(RemedyRegisterDto remedyRegisterDto)
        {
            int reportType = remedyRegisterDto.ReportType.Trim().ToUpper() == "" ? 1 :  2;
            int eventType = 0;
            int status = 0;

            return new RemedyRegister() 
            {
                TicketNumber = remedyRegisterDto.TicketNumber,
                SATNumber = remedyRegisterDto.SATNumber,
                ReportType = reportType,
                FaultEventType = eventType,
                FaultEventCause = remedyRegisterDto.FaultEventCause,
                Status = status,
                EventDate = remedyRegisterDto.EventDate
            };
        }
    }
}
