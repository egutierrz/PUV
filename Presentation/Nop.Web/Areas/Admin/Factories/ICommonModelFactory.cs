using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Common;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents common models factory
    /// </summary>
    public partial interface ICommonModelFactory
    {
        /// <summary>
        /// Prepare system info model
        /// </summary>
        /// <param name="model">System info model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the system info model
        /// </returns>
        Task<SystemInfoModel> PrepareSystemInfoModelAsync(SystemInfoModel model);

        /// <summary>
        /// Prepare system warning models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of system warning models
        /// </returns>
        Task<IList<SystemWarningModel>> PrepareSystemWarningModelsAsync();

        /// <summary>
        /// Prepare maintenance model
        /// </summary>
        /// <param name="model">Maintenance model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the maintenance model
        /// </returns>
        Task<MaintenanceModel> PrepareMaintenanceModelAsync(MaintenanceModel model);
        
        /// <summary>
        /// Prepare paged backup file list model
        /// </summary>
        /// <param name="searchModel">Backup file search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the backup file list model
        /// </returns>
        Task<BackupFileListModel> PrepareBackupFileListModelAsync(BackupFileSearchModel searchModel);

    
        /// <summary>
        /// Prepare language selector model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the language selector model
        /// </returns>
        Task<LanguageSelectorModel> PrepareLanguageSelectorModelAsync();

    }
}