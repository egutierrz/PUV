using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.SatRepositoryInfo;

namespace Nop.Services.InformationRepositoryService
{
    public partial interface IFilesRepositoryService
    {
        /// <summary>
        /// Download a File
        /// </summary>
        /// <param name="fileId">file identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task returns the file in binary
        /// </returns>
        public Task<byte[]> DownloadFileAsync(int fileId = 0);

        /// <summary>
        /// Gets all Files
        /// </summary>
        /// <param name="directoryId">directory identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the files of a directory
        /// </returns>
        /// <summary>
        public Task<IList<SatRepositoryInfoFiles>> GetAllFilesAsync(int directoryId = 0, bool isLector = false);

        /// <summary>
        /// Gets all Files (search by word in name and tags)
        /// </summary>
        /// <param name="searchword">search word</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the files of a directory
        /// </returns>
        public Task<IList<SatRepositoryInfoFiles>> GetAllFilesByWordAsync(string searchword);

        /// <summary>
        /// Get file by ID
        /// </summary>
        /// <param name="fileId">file identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the file information
        /// </returns>
        public Task<SatRepositoryInfoFiles> GetFileByID(int fileId);

        /// <summary>
        /// Upload a file with version control
        /// </summary>
        /// <param name="infoFile">SatRepositoryInfoFiles</param>
        /// <param name="binaryFile">SatRepositoryInfoBinaryFiles</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task UploadFileVersionedAsync(SatRepositoryInfoFiles infoFile, SatRepositoryInfoBinaryFiles binaryFile, Customer customer);

        /// <summary>
        /// Insert a file (SatRepositoryInfoFiles and  SatRepositoryInfoBinaryFiles)
        /// </summary>
        /// <param name="infoFile">SatRepositoryInfoFiles</param>
        /// <param name="binaryFile">SatRepositoryInfoBinaryFiles</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task UploadFileAsync(SatRepositoryInfoFiles infoFile, SatRepositoryInfoBinaryFiles binaryFile);

        /// <summary>
        /// Updates the file information
        /// </summary>
        /// <param name="satRepositoryInfoFiles">File information</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task UpdateFileAsync(SatRepositoryInfoFiles satRepositoryInfoFiles);

        /// <summary>
        /// Get file versions by ID
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the file versions
        /// </returns>
        public Task<IList<SatRepositoryInfoFiles>> GetFileVersionsByIDAsync(int fileId, bool isLector = false);

        /// <summary>
        /// Get last file version by actual file version id
        /// </summary>
        /// <param name="fileId">Is file id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the last file version
        /// </returns>
        public Task<SatRepositoryInfoFiles> GetLastFileVersion(string guid);

        /// <summary>
        /// Get follows files by CostumerID
        /// </summary>
        /// <param name="customerId">costumer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the costumer follows files information
        /// </returns>
        public Task<IList<SatRepositoryFilesCustomers>> GetFileCostumerAsync(int costumerID);

        /// <summary>
        /// Get follows files by FileID
        /// </summary>
        /// <param name="fileID">file identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the costumer follows files information
        /// </returns>
        public Task<IList<SatRepositoryFilesCustomers>> GetSatRepositoryFilesCustomerByFileIdAsync(string guid);

        /// <summary>
        /// Get a customers List of SatRepositoryFilesCustomers
        /// </summary>
        /// <param name="records"></param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a list of customers in a list of SatRepositoryFilesCustomers
        /// </returns>
        public Task<IList<Customer>> GetCustomerEmailInFileCustomerFollows(IList<SatRepositoryFilesCustomers> records);

        /// <summary>
        /// Send email message to a list of email directions list associated to an file
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="modifier">Is a modifier</param>
        /// <param name="activity">Is an activity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task SendNotificationToCustomerByActionInFile(string guid, Customer modifier, ActivityLogTypeEnum activity);

        /// <summary>
        /// Insert a satRepositoryFileCostumer 
        /// </summary>
        /// <param name="satRepositoryFileCostumer">satRepositoryFileCostumer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task CreateFileCostumerAsync(SatRepositoryFilesCustomers satRepositoryFileCostumer);

        public Task DeleteFileCostumerAsync(SatRepositoryFilesCustomers satRepositoryFileCostumer);

        public Task DeleteFileAsync(SatRepositoryInfoFiles satRepositoryInfoFiles);
    }
}
