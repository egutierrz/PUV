using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.SatRepositoryInfo;

namespace Nop.Services.InformationRepositoryService
{
    /// <summary>
    /// Information repository services interface
    /// </summary>
    /// 
    public partial interface IInformationRepositoryService
    {

        /// <summary>
        /// Gets a directory by ID
        /// </summary>
        /// <param name="folderId">Directory ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains directory information
        /// </returns>
        Task<SatRepositoryInfoDirectories> GetDirectoryByIdAsync(int folderId);

        /// <summary>
        /// Gets all DirectoriesA
        /// </summary>
        /// <param name="parentId">Parent directories ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the directories
        /// </returns>
        Task<IList<SatRepositoryInfoDirectories>> GetAllDirectoriesAsync(int parentId);


        /// <summary>
        /// Get directories by owner user identifier
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the directories
        /// </returns>
        Task<IList<SatRepositoryInfoDirectories>> GetDirectoriesByOwnerUserIdAsync(int customerId, int parentId);

        /// <summary>
        /// Insert a satRepository Info Directory
        /// </summary>
        /// <param name="satRepositoryInfoDirectory">SatRepositoryInfoDirectories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertDirectoryAsync(SatRepositoryInfoDirectories satRepositoryInfoDirectory);

        /// <summary>
        /// Rename Directory
        /// </summary>
        /// <param name="satRepositoryInfoDirectory">SatRepositoryInfoDirectories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        Task<bool> RenameDirectoryAsync(SatRepositoryInfoDirectories satRepositoryInfoDirectory);

        /// <summary>
        /// Delete directory by ID and files
        /// </summary>
        /// <param name="folderId">Directory ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task DeleteDirectoryByIdAsync(int folderId);

        /// <summary>
        /// Update a satRepository Info Directory
        /// </summary>
        /// <param name="satRepositoryInfoDirectory">SatRepositoryInfoDirectories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateDirectoryAsync(SatRepositoryInfoDirectories satRepositoryInfoDirectory);

    }
}