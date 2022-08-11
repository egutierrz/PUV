using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Services.InformationRepositoryService;
//using Nop.Services.Installation;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.InformationRepository;
using static Nop.Web.Areas.Admin.Models.InformationRepository.InfoRepositoryModel;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class InformationRepositoryController : BaseAdminController
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IFilesRepositoryService _filesRepositoryService;
        private readonly IInformationRepositoryModelFactory _informationRepositoryFactory;
        private readonly IInformationRepositoryService _informationRepositoryService;
        //private readonly InstallationService _installationService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;


        #region Ctor

        public InformationRepositoryController(ICustomerActivityService customerActivityService,
            IFilesRepositoryService filesRepositoryService,
            IInformationRepositoryModelFactory informationRepositoryFactory,
            IInformationRepositoryService informationRepositoryService,
            //InstallationService installationService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IWorkContext workContext) {
            _customerActivityService = customerActivityService;
            _filesRepositoryService = filesRepositoryService;
            _informationRepositoryFactory = informationRepositoryFactory;
            _informationRepositoryService = informationRepositoryService;
            //_installationService = installationService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _workContext = workContext;
        }

        #endregion

        // GET: ServiceDeskController
        public virtual async Task<IActionResult> Index()
        {
            //await _installationService.InstallLanguageSpanishAsync(); //SE AGREGO SOLO PARA INSTALAR LAS ETIQUETAS QUE FALTABAN

            var customer = await _workContext.GetCurrentCustomerAsync();
            var model = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, 0);

            await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_ModuleEntry.ToString(), ActivityLogTypeEnum.RI_ModuleEntry.GetEnumDescription(),customer);

            return View(model);
        }

        
        public virtual async Task<IActionResult> GetFilesFromFolderId(int idFolder)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var model = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, idFolder);
            return View("Index", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> AddFile(IFormFile file, int folderId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var fileStream = file.OpenReadStream();
            byte[] bytes = new byte[file.Length];
            fileStream.Read(bytes, 0, (int)file.Length);

            SatRepositoryInfoBinaryFiles binaryFile = new SatRepositoryInfoBinaryFiles();
            binaryFile.BinaryFile = bytes;
            binaryFile.CreatedOnUtc = DateTime.UtcNow;

            SatRepositoryInfoFiles infoFile = new SatRepositoryInfoFiles();
            infoFile.Name = file.FileName;
            infoFile.Extension = file.ContentType;
            infoFile.Size = (decimal)((file.Length / 1024f) / 1024f);
            infoFile.DirectoryId = folderId;
            infoFile.OwnerUserId = customer.Id;
            infoFile.CreatedOnUtc = DateTime.UtcNow;

            try
            {
                await _filesRepositoryService.UploadFileVersionedAsync(infoFile, binaryFile, customer);
            }
            catch(Exception exc)
            {
                return Json(new { message = exc, id = 0 });
            }

            return Json(new {message=1, id= infoFile.Id});
        }


        public virtual async Task<IActionResult> RemoveFile(int idFile)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var fileInfo = await _filesRepositoryService.GetFileByID(idFile);
                fileInfo.Deleted = true;
                fileInfo.DeletedOnUtc = DateTime.UtcNow;
                fileInfo.DeletedUserId = customer.Id;
                await _filesRepositoryService.UpdateFileAsync(fileInfo);
                var fileFollowed = await _filesRepositoryService.GetSatRepositoryFilesCustomerByFileIdAsync(fileInfo.GuidFile);
                if (fileFollowed!=null)
                {
                    SatRepositoryFilesCustomers fileCustomerModel = new SatRepositoryFilesCustomers();
                    fileCustomerModel.Guid = fileInfo.GuidFile;
                    fileCustomerModel.CustomerId = customer.Id;
                    await _filesRepositoryService.DeleteFileCostumerAsync(fileCustomerModel);
                }
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileDeletion.ToString(), ActivityLogTypeEnum.RI_FileDeletion.GetEnumDescription(), fileInfo);
                await _filesRepositoryService.SendNotificationToCustomerByActionInFile(fileInfo.GuidFile, customer, ActivityLogTypeEnum.RI_FileDeletion);
            }
            catch (Exception exc)
            {
                return Json(new { message = exc, id = 0 });
            }
            return Json(new { success = 1 });
        }

        public virtual async Task<IActionResult> DownloadFile(int idFile) {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var fileInfo = await _filesRepositoryService.GetFileByID(idFile);
                var fileBytes = await _filesRepositoryService.DownloadFileAsync(idFile);
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileDownload.ToString(), ActivityLogTypeEnum.RI_FileDownload.GetEnumDescription(), fileInfo);
                return Json(new { name = fileInfo.Name, blob = fileBytes, type=fileInfo.Extension });
            }
            catch (Exception exc) {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> FollowFile(int idFile)
        {
            try
            {
                var file = await _filesRepositoryService.GetFileByID(idFile);
                var customer = await _workContext.GetCurrentCustomerAsync();
                SatRepositoryFilesCustomers fileCustomerModel = new SatRepositoryFilesCustomers();
                fileCustomerModel.Guid = file.GuidFile;
                fileCustomerModel.CustomerId = customer.Id;
                await _filesRepositoryService.CreateFileCostumerAsync(fileCustomerModel);
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileTracking.ToString(), ActivityLogTypeEnum.RI_FileTracking.GetEnumDescription(), fileCustomerModel);
                return Json(new { success=1 });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> UnfollowFile(int idFile)
        {
            try
            {
                var file = await _filesRepositoryService.GetFileByID(idFile);
                var customer = await _workContext.GetCurrentCustomerAsync();
                var fileCustomerInfo= await _filesRepositoryService.GetSatRepositoryFilesCustomerByFileIdAsync(file.GuidFile);
                SatRepositoryFilesCustomers fileCustomerModel = new SatRepositoryFilesCustomers();
                fileCustomerModel.Id = fileCustomerInfo.FirstOrDefault().Id;
                fileCustomerModel.Guid = fileCustomerInfo.FirstOrDefault().Guid;
                fileCustomerModel.CustomerId = fileCustomerInfo.FirstOrDefault().CustomerId;
                await _filesRepositoryService.DeleteFileCostumerAsync(fileCustomerModel);
                return Json(new { success = 1 });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> PublishFile(int idFile)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                SatRepositoryInfoFiles fileInfoModel = await _filesRepositoryService.GetFileByID(idFile);
                fileInfoModel.Published = true;
                fileInfoModel.UpdatedOnUtc = DateTime.UtcNow;
                fileInfoModel.UpdatedUserId = customer.Id;
                await _filesRepositoryService.UpdateFileAsync(fileInfoModel);
                await _filesRepositoryService.SendNotificationToCustomerByActionInFile(fileInfoModel.GuidFile, customer, ActivityLogTypeEnum.RI_FilePublication);
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FilePublication.ToString(), ActivityLogTypeEnum.RI_FilePublication.GetEnumDescription(), fileInfoModel);
                return Json(new { success = 1 });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> AuthReject(int idFile, int authReject)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                SatRepositoryInfoFiles fileInfoModel = await _filesRepositoryService.GetFileByID(idFile);
                fileInfoModel.UpdatedOnUtc = DateTime.UtcNow;
                fileInfoModel.UpdatedUserId = customer.Id;
                if (authReject == 1)
                {
                    fileInfoModel.Status = 1;
                    await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileAuthorization.ToString(), ActivityLogTypeEnum.RI_FileAuthorization.GetEnumDescription(), fileInfoModel);
                    await _filesRepositoryService.UpdateFileAsync(fileInfoModel);
                    await _filesRepositoryService.SendNotificationToCustomerByActionInFile(fileInfoModel.GuidFile, customer, ActivityLogTypeEnum.RI_FileAuthorization);
                }
                else
                {
                    fileInfoModel.Status = 2;
                    await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileRejection.ToString(), ActivityLogTypeEnum.RI_FileRejection.GetEnumDescription(), fileInfoModel);
                    await _filesRepositoryService.UpdateFileAsync(fileInfoModel);
                    await _filesRepositoryService.SendNotificationToCustomerByActionInFile(fileInfoModel.GuidFile, customer, ActivityLogTypeEnum.RI_FileRejection);
                }
                //PENDIENTE AGREGAR COMENTARIOS DE RECHAZO
                
                return Json(new { success = 1 });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> AddUpdateTagList(int idFile, string tagList)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                SatRepositoryInfoFiles fileInfoModel = await _filesRepositoryService.GetFileByID(idFile);
                fileInfoModel.Tags = tagList;
                fileInfoModel.UpdatedOnUtc = DateTime.UtcNow;
                fileInfoModel.UpdatedUserId = customer.Id;
                await _filesRepositoryService.UpdateFileAsync(fileInfoModel);
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FileTagModification.ToString(), ActivityLogTypeEnum.RI_FileTagModification.GetEnumDescription(), fileInfoModel);
                return Json(new { success = 1 });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> GetFileInfoById(int idFile)
        {
            try
            {
                var fileInfoModel = await _informationRepositoryFactory.PrepareFileInfoModelAsync(idFile);
                return Json(new { fileInfo = fileInfoModel });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        public virtual async Task<IActionResult> GetFileVersionById(int idFile)
        {
            try
            {
                Customer customer = await _workContext.GetCurrentCustomerAsync();
                var fileInfoModel = await _informationRepositoryFactory.GetVersionListAsync(customer, idFile);
                return Json(new { fileInfo = fileInfoModel });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return Json(new { message = 0 });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> AddFolder(InfoRepositoryFolderModel model)
        {
            InfoRepositoryModel modelV = new InfoRepositoryModel();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(model.Name))
                        throw new NopException(string.Format(await _localizationService.GetResourceAsync("Admin.Common.Field.Required"), await _localizationService.GetResourceAsync("Admin.Informationrepository.CreateFolder.Fields.Name")));//
                    SatRepositoryInfoDirectories directory = new SatRepositoryInfoDirectories();
                
                    directory.CreatedOnUtc = DateTime.UtcNow;
                    directory.Name = model.Name;
                    directory.ParentId = model.IdParent;
                    directory.OwnerUserId = customer.Id;
                
                    await _informationRepositoryService.InsertDirectoryAsync(directory);
                    modelV = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, model.IdParent);

                    await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_DirectoryCreation.ToString(), ActivityLogTypeEnum.RI_DirectoryCreation.GetEnumDescription(), directory);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.Alert.Save.Ok"));

                    return View("Index", modelV);
                }
                catch (Exception exc)
                {
                    await _notificationService.ErrorNotificationAsync(exc);
                    modelV = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, 0);
                    return View("Index", modelV);
                }
            }

            modelV = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, 0);
            return View("Index", modelV);
        }

        public virtual async Task<IActionResult> SearchTermAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Content("");

            //products
            var productNumber = 10;

            var products = await _filesRepositoryService.GetAllFilesByWordAsync(term);

            //var models = _productModelFactory.PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              url = p.DirectoryId
                          })
                .ToList();
            return Json(result);
        }

        public virtual async Task<IActionResult> RemoveFolder(int idFolder)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var folderInfo = await _informationRepositoryService.GetDirectoryByIdAsync(idFolder);
                folderInfo.Deleted = true;
                folderInfo.UpdatedOnUtc = DateTime.UtcNow;
                //folderInfo. = customer.Id;
                await _informationRepositoryService.UpdateDirectoryAsync(folderInfo);
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FolderModification.ToString(), ActivityLogTypeEnum.RI_FileDeletion.GetEnumDescription(), folderInfo);
            }
            catch (Exception exc)
            {
                return Json(new { message = exc, id = 0 });
            }
            return Json(new { success = 1 });
        }

        public virtual async Task<IActionResult> GetFolderTree()
        {
            var folderInfo = new List<SatRepositoryInfoDirectories>();
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                return Json(new { success = 1, tree = await _informationRepositoryFactory.GetTreeFolderAsync(customer.Id) });
                //;
            }
            catch (Exception exc)
            {
                return Json(new { message = exc, tree = new List<string>() });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> UpdateFolder(InfoRepositoryFolderModel model)
        {
            InfoRepositoryModel modelV = new InfoRepositoryModel();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(model.NameUpdate))
                        throw new NopException(string.Format(await _localizationService.GetResourceAsync("Admin.Common.Field.Required"), await _localizationService.GetResourceAsync("Admin.Informationrepository.CreateFolder.Fields.Name")));//
                    SatRepositoryInfoDirectories directory = await _informationRepositoryService.GetDirectoryByIdAsync(model.Id);

                    directory.UpdatedOnUtc = DateTime.UtcNow;
                    directory.Name = model.NameUpdate;

                    var result = await _informationRepositoryService.RenameDirectoryAsync(directory);
                    if (!result)
                    {
                        throw new NopException(await _localizationService.GetResourceAsync("Admin.Informationrepository.UpdatedFolder.Fields.Name.Exist"));
                    }
                    modelV = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, model.IdParent);

                    await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.RI_FolderModification.ToString(), ActivityLogTypeEnum.RI_DirectoryCreation.GetEnumDescription(), directory);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.Alert.Save.Ok"));

                    return View("Index", modelV);
                }
                catch (Exception exc)
                {
                    await _notificationService.ErrorNotificationAsync(exc);
                    modelV = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, 0);
                    return View("Index", modelV);
                }
            }

            modelV = await _informationRepositoryFactory.PrepareInformationRepositoryModelAsync(customer, 0);
            return View("Index", modelV);
        }
    }
}
