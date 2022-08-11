using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.InformationRepositoryService;
using Nop.Services.Stores;

namespace Nop.Services.InformationRepository
{
    /// <summary>
    /// Language service
    /// </summary>
    public partial class InformationRepositoryService : IInformationRepositoryService
    {
        #region Fields

        private readonly IRepository<SatRepositoryInfoDirectories> _infoRepRepository;
        private readonly IRepository<SatRepositoryInfoFiles> _infoRepFile;

        #endregion

        #region Ctor

        public InformationRepositoryService(IRepository<SatRepositoryInfoDirectories> infoRepRepository, IRepository<SatRepositoryInfoFiles> infoRepFile)
        {
            _infoRepRepository = infoRepRepository;
            _infoRepFile = infoRepFile;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a directory by ID
        /// </summary>
        /// <param name="folderId">Directory ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains directory information
        /// </returns>
        public virtual async Task<SatRepositoryInfoDirectories> GetDirectoryByIdAsync(int folderId = 0)
        {

            var query = from c in _infoRepRepository.Table
                        where c.Id == folderId
                        where c.Deleted == false
                        select c;

            return await query.FirstOrDefaultAsync();
            ;
        }

        /// <summary>
        /// Gets all DirectoriesA
        /// </summary>
        /// <param name="parentId">Parent directories ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the directories
        /// </returns>
        public virtual async Task<IList<SatRepositoryInfoDirectories>> GetAllDirectoriesAsync(int parentId=0)
        {
            var query = from c in _infoRepRepository.Table
                        orderby c.Id
                        where c.ParentId == parentId
                        where c.Deleted == false
                        select c;
            var allDirectories = await query.ToListAsync();

            return allDirectories;
        }

        /// <summary>
        /// Get directories by owner user identifier
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the directories
        /// </returns>
        public virtual async Task<IList<SatRepositoryInfoDirectories>> GetDirectoriesByOwnerUserIdAsync(int customerId, int parentId)
        {

            var query = from c in _infoRepRepository.Table
                        orderby c.Id
                        where c.OwnerUserId == customerId
                        where c.ParentId == parentId
                        where c.Deleted == false
                        select c;
            var directories = await query.ToListAsync();

            return directories;
        }

        /// <summary>
        /// Insert a satRepository Info Directory
        /// </summary>
        /// <param name="satRepositoryInfoDirectory">SatRepositoryInfoDirectories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertDirectoryAsync(SatRepositoryInfoDirectories satRepositoryInfoDirectory)
        {
            var query = from c in _infoRepRepository.Table
                        where c.OwnerUserId == satRepositoryInfoDirectory.OwnerUserId
                        where c.Name == satRepositoryInfoDirectory.Name
                        where c.Deleted == false
                        select c;
            var directory = await query.FirstOrDefaultAsync();
            if(directory == null)
            {
                await _infoRepRepository.InsertAsync(satRepositoryInfoDirectory);
            }
        }

        /// <summary>
        /// Rename Directory
        /// </summary>
        /// <param name="satRepositoryInfoDirectory">SatRepositoryInfoDirectories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        public virtual async Task<bool> RenameDirectoryAsync(SatRepositoryInfoDirectories satRepositoryInfoDirectory)
        {
            var query = from i in _infoRepRepository.Table
                        where i.Name == satRepositoryInfoDirectory.Name
                        where i.OwnerUserId == satRepositoryInfoDirectory.OwnerUserId
                        where i.Deleted == false
                        select i;

            var directory = await query.FirstOrDefaultAsync();
            if (directory == null)
            {
                await _infoRepRepository.UpdateAsync(satRepositoryInfoDirectory);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete directory by ID and files
        /// </summary>
        /// <param name="folderId">Directory ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public virtual async Task DeleteDirectoryByIdAsync(int folderId)
        {
            var folder = _infoRepRepository.Table.FirstOrDefault(i => i.Id == folderId);
            folder.Deleted = true;
            await _infoRepRepository.UpdateAsync(folder);

            var query = _infoRepFile.Table
                        .Where(i => i.DirectoryId == folderId);
            var files = await query.ToListAsync();
            foreach (var file in files)
            {
                file.Deleted = true;
                await _infoRepFile.UpdateAsync(file);
            }
        }

        /// <summary>
        /// Update a satRepository Info Directory
        /// </summary>
        /// <param name="satRepositoryInfoDirectory">SatRepositoryInfoDirectories</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateDirectoryAsync(SatRepositoryInfoDirectories satRepositoryInfoDirectory)
        {
            await _infoRepRepository.UpdateAsync(satRepositoryInfoDirectory);           

        }
        #endregion
    }
}