using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.HelpDesk;
using Nop.Data;
using System.Threading.Tasks;

namespace Nop.Services.HelpDesk
{
    /// <summary>
    /// Help Desk service
    /// </summary>
    public partial class HelpDeskService : IHelpDeskService
    {
        #region Fields

        private readonly IRepository<RemedyRegister> _remedyRegisterRepository;

        #endregion

        #region Ctor

        public HelpDeskService(IRepository<RemedyRegister> helpDeskRepository)
        {
            _remedyRegisterRepository = helpDeskRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a remedy register
        /// </summary>
        /// <param name="remedyRegister">RemedyRegister</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertRemedyRegisterAsync(RemedyRegister remedyRegister)
        {
            remedyRegister.CreatedOnUtc = DateTime.UtcNow;
            await _remedyRegisterRepository.InsertAsync(remedyRegister);
        }

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
        public virtual async Task<IPagedList<RemedyRegister>> GetAllRemedyRegisterAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
            string ticketNumber = null, string satNumber = null, int? reportType = null, int? eventType = null, string causeEvent = null,
            int? status = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await _remedyRegisterRepository.GetAllPagedAsync(query =>
            {

                //filter by creation date
                if (!string.IsNullOrEmpty(ticketNumber))
                    query = query.Where(item => item.TicketNumber.Contains(ticketNumber));
                if (!string.IsNullOrEmpty(satNumber))
                    query = query.Where(item => item.SATNumber.Contains(satNumber));
                if (reportType.HasValue)
                    query = query.Where(item => item.ReportType.Equals(reportType));
                if (eventType.HasValue)
                    query = query.Where(item => item.FaultEventType.Equals(eventType));
                if (!string.IsNullOrEmpty(causeEvent))
                    query = query.Where(item => item.FaultEventCause.Contains(causeEvent));
                if (status.HasValue)
                    query = query.Where(item => item.Status.Equals(status));
                if (createdOnFrom.HasValue)
                    query = query.Where(item => createdOnFrom.Value <= item.CreatedOnUtc);
                if (createdOnTo.HasValue)
                    query = query.Where(item => createdOnTo.Value >= item.CreatedOnUtc);

                query = query.OrderByDescending(item => item.CreatedOnUtc).ThenBy(item => item.Id);

                return query;
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Get Remedy registers by identifiers
        /// </summary>
        /// <param name="recordIds">Remedy register identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the remedy registers
        /// </returns>
        public virtual async Task<IList<RemedyRegister>> GetRecordsByIdsAsync(int[] recordIds)
        {
            return await _remedyRegisterRepository.GetByIdsAsync(recordIds, includeDeleted: false);
        }

        #endregion
    }
}