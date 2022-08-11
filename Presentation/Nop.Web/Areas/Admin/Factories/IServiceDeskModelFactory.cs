using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.ServiceDesk;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the service desk model factory
    /// </summary>
    public partial interface IServiceDeskModelFactory
    {

        /// <summary>
        /// Prepare service desk search model
        /// </summary>
        /// <param name="searchModel">Service desk search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the service desk search model
        /// </returns>
        Task<ServiceDeskSearchModel> PrepareServiceDeskSearchModelAsync(ServiceDeskSearchModel searchModel);

        /// <summary>
        /// Prepare paged service desk list model
        /// </summary>
        /// <param name="searchModel">Service desk search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the service desk list model
        /// </returns>
        Task<ServiceDeskListModel> PrepareServiceDeskListModelAsync(ServiceDeskSearchModel searchModel);

        Task<Dictionary<int, int[]>> PrepareServiceDeskChartsModelAsync(ServiceDeskSearchModel searchModel);

    }
}
