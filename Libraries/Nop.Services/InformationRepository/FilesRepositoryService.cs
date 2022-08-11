using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Services.InformationRepositoryService
{
    public partial class FilesRepositoryService : IFilesRepositoryService
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IRepository<SatRepositoryInfoFiles> _infoRepRepository;
        private readonly IRepository<SatRepositoryInfoBinaryFiles> _infoRepBinaryFile;
        private readonly IRepository<SatRepositoryFilesCustomers> _infoRepFileCostumer;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;

        public FilesRepositoryService(ICustomerActivityService customerActivityService,
            IRepository<SatRepositoryInfoFiles> infoRepRepository, 
            IRepository<SatRepositoryInfoBinaryFiles> infoRepBinaryFile,
            IRepository<SatRepositoryFilesCustomers> infoRepFileCostumer,
            ICustomerService customer,
            IWorkflowMessageService workflowMessageService)
        {
            _customerActivityService = customerActivityService;
            _infoRepRepository = infoRepRepository;
            _infoRepBinaryFile = infoRepBinaryFile;
            _infoRepFileCostumer = infoRepFileCostumer;
            _customerService = customer;
            _workflowMessageService = workflowMessageService;
        }

        /// <summary>
        /// Download a File
        /// </summary>
        /// <param name="fileId">file identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task returns the file in binary
        /// </returns>
        public async Task<byte[]> DownloadFileAsync(int fileId = 0)
        {
            var query = from i in _infoRepRepository.Table
                        join b in _infoRepBinaryFile.Table on i.BinaryFileId equals b.Id
                        where i.Id == fileId
                        select b.BinaryFile;
            var binaryFile = await query.FirstOrDefaultAsync();
            return binaryFile;
            
        }

        /// <summary>
        /// Gets all Files
        /// </summary>
        /// <param name="directoryId">directory identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the files of a directory
        /// </returns>
        public async Task<IList<SatRepositoryInfoFiles>> GetAllFilesAsync(int directoryId = 0, bool isLector = false)
        {
            var queryGroup = from i in _infoRepRepository.Table
                             where i.DirectoryId == directoryId
                             where i.Deleted == false
                             orderby i.CreatedOnUtc descending
                             group i by i.Name into NameGroup
                             select NameGroup.Key + (from file in NameGroup select file.Version).Max();
            if (isLector)
            {
                queryGroup = from i in _infoRepRepository.Table
                             where i.DirectoryId == directoryId
                             where i.Deleted == false
                             where i.Published == true
                             where i.Status == 1
                             orderby i.CreatedOnUtc descending
                             group i by i.Name into NameGroup
                             select NameGroup.Key + (from file in NameGroup select file.Version).Max();
            }



            var allGroups = await queryGroup.ToListAsync();

            var query = from i in _infoRepRepository.Table
                         where i.DirectoryId == directoryId
                         where i.Deleted == false
                         where allGroups.Contains(i.Name + i.Version)
                         select i;
                       

            var allFiles = await query.ToListAsync();
            return allFiles;
        }
        /// <summary>
        /// Gets all Files (search by word in name and tags)
        /// </summary>
        /// <param name="searchword">search word</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the files of a directory
        /// </returns>
        public async Task<IList<SatRepositoryInfoFiles>> GetAllFilesByWordAsync(string searchword)
        {
            var queryGroup = from i in _infoRepRepository.Table
                             where i.Deleted == false
                             orderby i.CreatedOnUtc descending
                             group i by i.Name into NameGroup
                             select NameGroup.Key + (from file in NameGroup select file.Version).Max();


            var allGroups = await queryGroup.ToListAsync();

            var query = from i in _infoRepRepository.Table
                        where i.Deleted == false
                        where allGroups.Contains(i.Name + i.Version)
                        where i.Name.Contains(searchword) || i.Tags.Contains(searchword)
                        select i;

            var allFiles = await query.ToListAsync();

            return allFiles;
        }

        /// <summary>
        /// Get file by ID
        /// </summary>
        /// <param name="fileId">file identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the file information
        /// </returns>
        public async Task<SatRepositoryInfoFiles> GetFileByID(int fileId)
        {
            var query = from i in _infoRepRepository.Table
                        where i.Id == fileId
                        select i;

            var file = await query.FirstOrDefaultAsync();

            return file;
        }

        /// <summary>
        /// Upload a file with version control
        /// </summary>
        /// <param name="infoFile">SatRepositoryInfoFiles</param>
        /// <param name="binaryFile">SatRepositoryInfoBinaryFiles</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UploadFileVersionedAsync(SatRepositoryInfoFiles infoFile, SatRepositoryInfoBinaryFiles binaryFile, Customer customer)
        {
            var query = from i in _infoRepRepository.Table
                        where i.Name == infoFile.Name
                        where i.DirectoryId == infoFile.DirectoryId
                        orderby i.Version descending
                        select i;

            var file = await query.FirstOrDefaultAsync();
            if (file != null)
            {
                infoFile.Version = file.Version + 1;
                infoFile.GuidFile = file.GuidFile;
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileCreation.ToString(), ActivityLogTypeEnum.RI_FileCreation.GetEnumDescription(), infoFile);
                await UploadFileAsync(infoFile, binaryFile);
                await SendNotificationToCustomerByActionInFile(file.GuidFile, customer, ActivityLogTypeEnum.RI_NewFileVersion);
            }
            else
            {
                infoFile.Version = 1;
                infoFile.GuidFile = Guid.NewGuid().ToString();
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileCreation.ToString(), ActivityLogTypeEnum.RI_FileCreation.GetEnumDescription(), infoFile);
                await UploadFileAsync(infoFile, binaryFile);
            }
        }

        /// <summary>
        /// Insert a file (SatRepositoryInfoFiles and  SatRepositoryInfoBinaryFiles)
        /// </summary>
        /// <param name="infoFile">SatRepositoryInfoFiles</param>
        /// <param name="binaryFile">SatRepositoryInfoBinaryFiles</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UploadFileAsync(SatRepositoryInfoFiles infoFile, SatRepositoryInfoBinaryFiles binaryFile)
        {
            await _infoRepBinaryFile.InsertAsync(binaryFile);
            infoFile.BinaryFileId = binaryFile.Id;
            await _infoRepRepository.InsertAsync(infoFile);
            
        }

        /// <summary>
        /// Updates the file information
        /// </summary>
        /// <param name="satRepositoryInfoFiles">File information</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateFileAsync(SatRepositoryInfoFiles satRepositoryInfoFiles)
        {
            await _infoRepRepository.UpdateAsync(satRepositoryInfoFiles);
        }

        /// <summary>
        /// Get file versions by ID
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the file versions
        /// </returns>
        public async Task<IList<SatRepositoryInfoFiles>> GetFileVersionsByIDAsync(int fileId, bool isLector = false)
        {
            //get name 
            var query = from i in _infoRepRepository.Table
                        where i.Id == fileId
                        select i;
            var file = await query.FirstOrDefaultAsync();

            //get file versions with same name
            query = from e in _infoRepRepository.Table
                    where e.Name == file.Name
                    where e.DirectoryId == file.DirectoryId
                    orderby e.Version descending
                    select e;
            if (isLector)
            {
                query = from e in query
                        where e.Published == true
                        where e.Status == 1
                        orderby e.Version descending
                        select e;
            }


            return await query.ToListAsync();
            
        }

        /// <summary>
        /// Get last file version by actual file version id
        /// </summary>
        /// <param name="fileId">Is file id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the last file version
        /// </returns>
        public async Task<SatRepositoryInfoFiles> GetLastFileVersion(string guid)
        {
            var query = from i in _infoRepRepository.Table
                        where i.GuidFile == guid
                        orderby i.Version descending
                        select i;
            //var actualFileVersion = await query.FirstOrDefaultAsync();

            //query = from e in _infoRepRepository.Table
            //        where e.Name == actualFileVersion.Name
            //        where e.DirectoryId == actualFileVersion.DirectoryId
            //        orderby e.Version descending
            //        select e;

            var lastFileVersion = await query.FirstOrDefaultAsync();

            return lastFileVersion;
        }

        /// <summary>
        /// Get follows files by CostumerID
        /// </summary>
        /// <param name="customerId">costumer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the costumer follows files information
        /// </returns>
        public async Task<IList<SatRepositoryFilesCustomers>> GetFileCostumerAsync(int costumerID)
        {
            var query = from i in _infoRepFileCostumer.Table
                        where i.CustomerId == costumerID
                        select i;

            var files = await query.ToListAsync();

            return files;
        }

        /// <summary>
        /// Get follows files by FileID
        /// </summary>
        /// <param name="fileID">file identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the costumer follows files information
        /// </returns>
        public async Task<IList<SatRepositoryFilesCustomers>> GetSatRepositoryFilesCustomerByFileIdAsync(string guid)
        {
            var query = from i in _infoRepFileCostumer.Table
                        where i.Guid == guid
                        select i;

            var files = await query.ToListAsync();

            return files;
        }

        /// <summary>
        /// Get a customers List of SatRepositoryFilesCustomers
        /// </summary>
        /// <param name="records"></param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a list of customers in a list of SatRepositoryFilesCustomers
        /// </returns>
        public async Task<IList<Customer>> GetCustomerEmailInFileCustomerFollows(IList<SatRepositoryFilesCustomers> records)
        {
            IList<Customer> customers = new List<Customer>();
            foreach (var record in records)
            {
                customers.Add(await _customerService.GetCustomerByIdAsync(record.CustomerId));
            }
            return customers;
        }

        /// <summary>
        /// Send email message to a list of email directions list associated to an file
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="modifier">Is a modifier</param>
        /// <param name="activity">Is an activity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task SendNotificationToCustomerByActionInFile(string guid, Customer modifier, ActivityLogTypeEnum activity)
        {
            var file = await GetLastFileVersion(guid);
            var associations = await GetSatRepositoryFilesCustomerByFileIdAsync(guid);
            var customers = await GetCustomerEmailInFileCustomerFollows(associations);
            String messageTemplate = null;
            switch (activity)
            {
                case ActivityLogTypeEnum.RI_NewFileVersion:
                    messageTemplate = MessageTemplateSystemNames.RI_UPDATE_VERSION;
                    //file = await GetLastFileVersion(guid);
                    break;
                case ActivityLogTypeEnum.RI_FileDeletion:
                    messageTemplate = MessageTemplateSystemNames.RI_DELETE_FILE;
                    break;
                case ActivityLogTypeEnum.RI_FileAuthorization:
                    messageTemplate = MessageTemplateSystemNames.RI_AUTHORIZED_VERSION;
                    break;
                case ActivityLogTypeEnum.RI_FilePublication:
                    messageTemplate = MessageTemplateSystemNames.RI_PUBLISHED_VERSION;
                    break;
                case ActivityLogTypeEnum.RI_FileRejection:
                    messageTemplate = MessageTemplateSystemNames.RI_REJECTED_VERSION;
                    break;
            }
            foreach (var customer in customers)
            {
                await _workflowMessageService.SendCustomerNotificationByChangesInFollowedFile(customer, modifier, file, messageTemplate, 2);
            }
        }

        /// <summary>
        /// Insert a satRepositoryFileCostumer 
        /// </summary>
        /// <param name="satRepositoryFileCostumer">satRepositoryFileCostumer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task CreateFileCostumerAsync(SatRepositoryFilesCustomers satRepositoryFileCostumer)
        {
            await _infoRepFileCostumer.InsertAsync(satRepositoryFileCostumer);
        }

        public async Task DeleteFileCostumerAsync(SatRepositoryFilesCustomers satRepositoryFileCostumer)
        {
            await _infoRepFileCostumer.DeleteAsync(satRepositoryFileCostumer);
        }

        public async Task DeleteFileAsync(SatRepositoryInfoFiles satRepositoryInfoFiles)
        {
            var query = from i in _infoRepBinaryFile.Table
                        where i.Id == satRepositoryInfoFiles.Id
                        select i;

            await _infoRepBinaryFile.DeleteAsync(query.FirstOrDefault());

            await _infoRepRepository.DeleteAsync(satRepositoryInfoFiles);
        }

    }
}
