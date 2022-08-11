using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Data.Extensions;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.InformationRepositoryService;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.InformationRepository;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using static Nop.Web.Areas.Admin.Models.InformationRepository.InfoRepositoryModel;
using static Nop.Web.Areas.Admin.Models.InformationRepository.TreeFolderModel;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the language model factory implementation
    /// </summary>
    public partial class InformationRepositoryModelFactory : IInformationRepositoryModelFactory
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IFilesRepositoryService _filesRepositoryService;
        private readonly IInformationRepositoryService _informationRepositoryService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public InformationRepositoryModelFactory(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IFilesRepositoryService filesRepositoryService,
            IInformationRepositoryService informationRepositoryService,
            ILocalizationService localizationService)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _filesRepositoryService = filesRepositoryService;
            _informationRepositoryService = informationRepositoryService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare information repository model
        /// </summary>
        /// <param name="parentId">Parent directory ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the information repository model
        /// </returns>
        public virtual async Task<InfoRepositoryModel> PrepareInformationRepositoryModelAsync(Customer customer, int prntId =0)
        {
            DirectoriesListModel parentDirectory = new DirectoriesListModel();
            InfoRepositoryModel model = new InfoRepositoryModel();
            IList<FileInfoModel> filesToAuthListModels = new List<FileInfoModel>();
            model.ParentId = prntId;
            if (prntId > 0)
            {
                var infoParent = await _informationRepositoryService.GetDirectoryByIdAsync(prntId);
                parentDirectory.DirectoryName = infoParent.Name;
                parentDirectory.ParentId = infoParent.ParentId;
            }
            model.ParentName = prntId>0? parentDirectory.DirectoryName : String.Empty;

            //ARCHIVOS EN SEGUIMIENTO
            var followedFiles = await _filesRepositoryService.GetFileCostumerAsync(customer.Id);
            model.FollowedFileListModel = await followedFiles.SelectAwait(async file => {
                var fileInfoService = await _filesRepositoryService.GetLastFileVersion(file.Guid);
                if (fileInfoService.Deleted == false)
                {
                    FileInfoModel fileInfoModel = new FileInfoModel();
                    fileInfoModel.Id = fileInfoService.Id;
                    fileInfoModel.FileName = fileInfoService.Name;
                    fileInfoModel.FileExtention = fileInfoService.Extension;
                    fileInfoModel.FileCreated = (await _dateTimeHelper.ConvertToUserTimeAsync(fileInfoService.CreatedOnUtc, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt");
                    fileInfoModel.FileLastUpdate = fileInfoService.UpdatedOnUtc.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(fileInfoService.UpdatedOnUtc.Value, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt") :
                    (await _dateTimeHelper.ConvertToUserTimeAsync(fileInfoService.CreatedOnUtc, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt");
                    fileInfoModel.Size = fileInfoService.Size <= 0 ? "<0 MB" : fileInfoService.Size.ToString() + " MB";
                    fileInfoModel.OwnerName = await _customerService.GetCustomerFullNameAsync(customer);
                    fileInfoModel.Version = fileInfoService.Version;
                    fileInfoModel.Tags = fileInfoService.Tags;
                    fileInfoModel.PublishedTxt = fileInfoService.Published ? await _localizationService.GetResourceAsync("Admin.Informationrepository.PublishStatus") :
                await _localizationService.GetResourceAsync("Admin.Informationrepository.NotPublishedStatus");
                    switch (fileInfoService.Status)
                    {
                        case 0:
                            {//Por autorizar
                                fileInfoModel.StatusTxt = await _localizationService.GetResourceAsync("Admin.Informationrepository.NotAuthorizeStatus");
                            }
                            break;
                        case 1:
                            {//Autorizado
                                fileInfoModel.StatusTxt = await _localizationService.GetResourceAsync("Admin.Informationrepository.AuthorizeStatus");
                            }
                            break;
                        case 2:
                            {//Rechazado
                                fileInfoModel.StatusTxt = await _localizationService.GetResourceAsync("Admin.Informationrepository.RejectedStatus");
                            }
                            break;
                    }
                    return fileInfoModel;
                }
                else
                    return null;
            }).ToListAsync();

            //CARPETAS
            var directories = await _informationRepositoryService.GetAllDirectoriesAsync(prntId);
            
            model.DirectoryListModel = await directories.SelectAwait(async directory => {
                var customerAuthor = await _customerService.GetCustomerByIdAsync(directory.OwnerUserId);
                DirectoriesListModel directoryModel = new DirectoriesListModel();
                directoryModel.Id = directory.Id;
                directoryModel.DirectoryName = directory.Name;
                directoryModel.AuthorName = await _customerService.GetCustomerFullNameAsync(customerAuthor);
                directoryModel.DirectoryLastUpdate = directory.UpdatedOnUtc.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(directory.UpdatedOnUtc.Value, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt") :
                (await _dateTimeHelper.ConvertToUserTimeAsync(directory.CreatedOnUtc, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt");
                return directoryModel;
            }).ToListAsync();

            model.IsAuthor = await _customerService.IsAuthorAsync(customer);
            model.IsAuthorizer = await _customerService.IsAuthorizerAsync(customer);
            model.IsReader = await _customerService.IsReaderAsync(customer);

            //ARCHIVOS
            var files= await _filesRepositoryService.GetAllFilesAsync(prntId, model.IsReader);

            model.FileListModel = await files.SelectAwait(async x =>
            {
                var customerFileAuthor = await _customerService.GetCustomerByIdAsync(x.OwnerUserId);
                FileInfoModel fileModel = new FileInfoModel();
                fileModel.Id = x.Id;
                fileModel.FileName = x.Name;
                fileModel.FileExtention = x.Extension;
                fileModel.FileLastUpdate = x.UpdatedOnUtc.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(x.UpdatedOnUtc.Value, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt") :
                (await _dateTimeHelper.ConvertToUserTimeAsync(x.CreatedOnUtc, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt");
                fileModel.Version = x.Version;
                fileModel.FileFavEnabled = followedFiles.Any(followedFile => followedFile.Guid == x.GuidFile);
                fileModel.OwnerName = await _customerService.GetCustomerFullNameAsync(customerFileAuthor);
                fileModel.Published = x.Published;
                fileModel.PublishedTxt = x.Published ? await _localizationService.GetResourceAsync("Admin.Informationrepository.PublishStatus") :
                await _localizationService.GetResourceAsync("Admin.Informationrepository.NotPublishedStatus");
                fileModel.Status = x.Status;

                switch (x.Status)
                {
                    case 0:{//Por autorizar
                            if(x.Published == true)
                            {
                                filesToAuthListModels.Add(fileModel);
                            }
                            fileModel.StatusTxt = await _localizationService.GetResourceAsync("Admin.Informationrepository.NotAuthorizeStatus");
                        }
                        break;
                    case 1:
                        {//Autorizado
                            fileModel.StatusTxt = await _localizationService.GetResourceAsync("Admin.Informationrepository.AuthorizeStatus");
                        }
                        break;
                    case 2:
                        {//Rechazado
                            fileModel.StatusTxt = await _localizationService.GetResourceAsync("Admin.Informationrepository.RejectedStatus");
                        }
                        break;
                }
                if (model.IsReader && x.Published && x.Status==1)
                    return fileModel;
                else if (model.IsAuthor || model.IsAuthorizer)
                    return fileModel;
                else
                    return null;
            }).ToListAsync();

            model.FileToAuthListModel = filesToAuthListModels;

            model.InfoFolder.IdParent = prntId;
            model.UploadFileModel.ParentFolderId = prntId;
            model.GParentFolderId = parentDirectory.ParentId;
            

            return model;
        }

        public virtual async Task<FileInfoModel> PrepareFileInfoModelAsync(int fileId = 0) {
            var fileInfoService = await _filesRepositoryService.GetFileByID(fileId);
            var infoParent = fileInfoService.DirectoryId!=0?await _informationRepositoryService.GetDirectoryByIdAsync(fileInfoService.DirectoryId):null;
            var customer = await _customerService.GetCustomerByIdAsync(fileInfoService.OwnerUserId);
            FileInfoModel fileInfoModel = new FileInfoModel();
            fileInfoModel.FileName = fileInfoService.Name;
            fileInfoModel.FileCreated = (await _dateTimeHelper.ConvertToUserTimeAsync(fileInfoService.CreatedOnUtc, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt");
            fileInfoModel.FileLastUpdate = fileInfoService.UpdatedOnUtc.HasValue ? (await _dateTimeHelper.ConvertToUserTimeAsync(fileInfoService.UpdatedOnUtc.Value, DateTimeKind.Utc)).ToString("dd/MM/yyyy hh:mm:ss tt") : "";
            fileInfoModel.Size = fileInfoService.Size<=0? "<0 MB": fileInfoService.Size.ToString() +" MB";
            fileInfoModel.ParentFolderName = infoParent!=null?infoParent.Name: await _localizationService.GetResourceAsync("Admin.Informationrepository.Index");
            fileInfoModel.OwnerName = await _customerService.GetCustomerFullNameAsync(customer);
            fileInfoModel.Tags = fileInfoService.Tags;

            switch (fileInfoService.Extension)
            {
                case "application/vnd.ms-excel":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    fileInfoModel.FileExtention = "Hoja de cálculo de Excel";
                    break;
                case "application/x-zip-compressed":
                case "application/octet-stream":
                    fileInfoModel.FileExtention = "Archivo comprimido";
                    break;
                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    fileInfoModel.FileExtention = "Documento de Word";
                    break;
                case "application/pdf":
                    fileInfoModel.FileExtention = "Archivo PDF";
                    break;
                case "application/vnd.ms-powerpoint":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    fileInfoModel.FileExtention = "Archivo de Power Point";
                    break;
                case "image/jpeg":
                case "image/gif":
                case "image/png":
                    fileInfoModel.FileExtention = "Imágen";
                    break;
                case "audio/mpeg":
                case "video/mp4":
                    fileInfoModel.FileExtention = "Archivo multimedia";
                    break;
                default:
                    fileInfoModel.FileExtention = "Archivo";
                    break;
            }

            return fileInfoModel;
        }

        public virtual async Task<IList<FileVersionInfoModel>> GetVersionListAsync(Customer customer,int fileId = 0) {
            IList<FileVersionInfoModel> versionList = new List<FileVersionInfoModel>();
            bool isLector = await _customerService.IsReaderAsync(customer);
            var list = await _filesRepositoryService.GetFileVersionsByIDAsync(fileId, isLector);
            versionList = await list.SelectAwait(async fileV =>
            {
                FileVersionInfoModel fModel = new FileVersionInfoModel();
                fModel.Status = fileV.Published ? await _localizationService.GetResourceAsync("Admin.Informationrepository.PublishStatus") :
                await _localizationService.GetResourceAsync("Admin.Informationrepository.NotPublishedStatus");
                fModel.FileId = fileV.Id;
                fModel.Version = fileV.Version;
                switch (fileV.Status)
                {
                    case 0:
                        {//Por autorizar
                            fModel.Status += " - "+ await _localizationService.GetResourceAsync("Admin.Informationrepository.NotAuthorizeStatus");
                        }
                        break;
                    case 1:
                        {//Autorizado
                            fModel.Status += " - " + await _localizationService.GetResourceAsync("Admin.Informationrepository.AuthorizeStatus");
                        }
                        break;
                    case 2:
                        {//Rechazado
                            fModel.Status += " - " + await _localizationService.GetResourceAsync("Admin.Informationrepository.RejectedStatus");
                        }
                        break;
                }
                return fModel;
            }).ToListAsync();
            return versionList;
        }


        public virtual async Task<TreeFolderModel> GetTreeFolderAsync(int customerId) {
            TreeFolderModel tree = new TreeFolderModel();
            var directories = await _informationRepositoryService.GetAllDirectoriesAsync(0);
            ParentModel parent = new ParentModel();
            parent.Id = 0;
            parent.text = await _localizationService.GetResourceAsync("Admin.Informationrepository.Index");
            if (directories != null)
            {
                parent.nodes = await GetChild(directories, customerId);
            }
            tree.Parent.Add(parent);
            return tree;
        }

        public virtual async Task<List<NodeModel>> GetChild(IList<SatRepositoryInfoDirectories> nodeList, int customerId) {
            List<NodeModel> listN = new List<NodeModel>();
            foreach (var node in nodeList)
            {
                var nodeModel = new NodeModel();
                nodeModel.text = node.Name;
                nodeModel.Id = node.Id;
                var nodes = await _informationRepositoryService.GetAllDirectoriesAsync(node.Id);
                if (nodes != null)
                {
                    nodeModel.nodes = await GetChild(nodes, customerId);
                }
                listN.Add(nodeModel);
            }
            return listN;
        }
        #endregion
    }
}