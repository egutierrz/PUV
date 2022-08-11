using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Web.Areas.Admin.Models.InformationRepository;
using Nop.Web.Areas.Admin.Models.Localization;
using static Nop.Web.Areas.Admin.Models.InformationRepository.InfoRepositoryModel;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the language model factory
    /// </summary>
    public partial interface IInformationRepositoryModelFactory
    {
        /// <summary>
        /// Prepare information repository model
        /// </summary>
        /// <param name="parentId">Parent directory ID</param>
        /// <param name="customerId">Client ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the information repository model
        /// </returns>
        Task<InfoRepositoryModel> PrepareInformationRepositoryModelAsync(Customer customer, int parentId = 0);

        /// <summary>
        /// Prepare file information model
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains file information model
        /// </returns>
        Task<FileInfoModel> PrepareFileInfoModelAsync(int fileId = 0);

        /// <summary>
        /// Prepare file list version
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains file list version
        /// </returns>
        Task<IList<FileVersionInfoModel>> GetVersionListAsync(Customer customer, int fileId = 0);

        Task<TreeFolderModel> GetTreeFolderAsync(int customerId);
    }
}