using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.HelpDesk;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Services.HelpDesk
{
    /// <summary>
    /// Help Desk service interface
    /// </summary>
    public partial interface IHelpDeskService
    {
        /// <summary>
        /// Inserts a remedy register
        /// </summary>
        /// <param name="remedyRegister">RemedyRegister</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertRemedyRegisterAsync(RemedyRegister remedyRegister);

        /// <summary>
        /// Gets all remedy register items
        /// </summary>
        /// <param name="createdOnFrom">Record item creation from; pass null to load all records</param>
        /// <param name="createdOnTo">Record item creation to; pass null to load all records</param>
        /// <param name="ticketNumber">Ticket Number</param>
        /// <param name="satNumber">Sat Number</param>
        /// <param name="reportType">Report Type</param>
        /// <param name="eventType">Event Type</param>
        /// <param name="causeEvent">Cause Event</param>
        /// <param name="status">Status</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the remedy register items
        /// </returns>
        Task<IPagedList<RemedyRegister>> GetAllRemedyRegisterAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
            string ticketNumber = null, string satNumber = null, int? reportType = null, int? eventType = null, string causeEvent = null,
            int? status = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get Remedy registers by identifiers
        /// </summary>
        /// <param name="recordIds">Remedy register identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the remedy registers
        /// </returns>
        Task<IList<RemedyRegister>> GetRecordsByIdsAsync(int[] recordIds);

    }
}
