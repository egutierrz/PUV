using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Security;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Security;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents common models factory implementation
    /// </summary>
    public partial class CommonModelFactory : ICommonModelFactory
    {
        #region Fields

        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly AppSettings _appSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly INopDataProvider _dataProvider;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly INopFileProvider _fileProvider;
        private readonly IServiceCollection _serviceCollection;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly NopHttpClient _nopHttpClient;
        private readonly ProxySettings _proxySettings;

        #endregion

        #region Ctor

        public CommonModelFactory(AdminAreaSettings adminAreaSettings,
            AppSettings appSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            INopDataProvider dataProvider,
            IDateTimeHelper dateTimeHelper,
            INopFileProvider fileProvider,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMaintenanceService maintenanceService,
            IServiceCollection serviceCollection,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            NopHttpClient nopHttpClient,
            ProxySettings proxySettings)
        {
            _adminAreaSettings = adminAreaSettings;
            _appSettings = appSettings;
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _dataProvider = dataProvider;
            _dateTimeHelper = dateTimeHelper;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _localizationService = localizationService;
            _maintenanceService = maintenanceService;
            _fileProvider = fileProvider;
            _serviceCollection = serviceCollection;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _nopHttpClient = nopHttpClient;
            _proxySettings = proxySettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare store URL warning model
        /// </summary>
        /// <param name="models">List of system warning models</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareStoreUrlWarningModelAsync(IList<SystemWarningModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //check whether current store URL matches the store configured URL
            var store = await _storeContext.GetCurrentStoreAsync();
            var currentStoreUrl = store.Url;
            if (!string.IsNullOrEmpty(currentStoreUrl) &&
                (currentStoreUrl.Equals(_webHelper.GetStoreLocation(false), StringComparison.InvariantCultureIgnoreCase) ||
                currentStoreUrl.Equals(_webHelper.GetStoreLocation(true), StringComparison.InvariantCultureIgnoreCase)))
            {
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.URL.Match")
                });
                return;
            }

            models.Add(new SystemWarningModel
            {
                Level = SystemWarningLevel.Fail,
                Text = string.Format(await _localizationService.GetResourceAsync("Admin.System.Warnings.URL.NoMatch"),
                    currentStoreUrl, _webHelper.GetStoreLocation(false))
            });
        }

        /// <summary>
        /// Prepare copyright removal key warning model
        /// </summary>
        /// <param name="models">List of system warning models</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareRemovalKeyWarningModelAsync(IList<SystemWarningModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (!_adminAreaSettings.CheckCopyrightRemovalKey)
                return;

            //try to get a warning
            var warning = string.Empty;
            try
            {
                warning = await _nopHttpClient.GetCopyrightWarningAsync();
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrEmpty(warning))
                return;

            models.Add(new SystemWarningModel
            {
                Level = SystemWarningLevel.CopyrightRemovalKey,
                Text = warning,
                DontEncode = true //this text could contain links, so don't encode it
            });
        }
        
        /// <summary>
        /// Prepare performance settings warning model
        /// </summary>
        /// <param name="models">List of system warning models</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PreparePerformanceSettingsWarningModelAsync(IList<SystemWarningModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //check whether "IgnoreStoreLimitations" setting disabled
            if (!_catalogSettings.IgnoreStoreLimitations && (await _storeService.GetAllStoresAsync()).Count == 1)
            {
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Recommendation,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.Performance.IgnoreStoreLimitations")
                });
            }

            //check whether "IgnoreAcl" setting disabled
            if (!_catalogSettings.IgnoreAcl)
            {
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Recommendation,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.Performance.IgnoreAcl")
                });
            }
        }

        /// <summary>
        /// Prepare file permissions warning model
        /// </summary>
        /// <param name="models">List of system warning models</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareFilePermissionsWarningModelAsync(IList<SystemWarningModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var dirPermissionsOk = true;
            var dirsToCheck = _fileProvider.GetDirectoriesWrite();
            foreach (var dir in dirsToCheck)
            {
                if (_fileProvider.CheckPermissions(dir, false, true, true, false))
                    continue;

                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = string.Format(await _localizationService.GetResourceAsync("Admin.System.Warnings.DirectoryPermission.Wrong"),
                        CurrentOSUser.FullName, dir)
                });
                dirPermissionsOk = false;
            }

            if (dirPermissionsOk)
            {
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.DirectoryPermission.OK")
                });
            }

            var filePermissionsOk = true;
            var filesToCheck = _fileProvider.GetFilesWrite();
            foreach (var file in filesToCheck)
            {
                if (_fileProvider.CheckPermissions(file, false, true, true, true))
                    continue;

                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Warning,
                    Text = string.Format(await _localizationService.GetResourceAsync("Admin.System.Warnings.FilePermission.Wrong"),
                        CurrentOSUser.FullName, file)
                });
                filePermissionsOk = false;
            }

            if (filePermissionsOk)
            {
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.FilePermission.OK")
                });
            }
        }

        /// <summary>
        /// Prepare backup file search model
        /// </summary>
        /// <param name="searchModel">Backup file search model</param>
        /// <returns>Backup file search model</returns>
        protected virtual BackupFileSearchModel PrepareBackupFileSearchModel(BackupFileSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Prepare system info model
        /// </summary>
        /// <param name="model">System info model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the system info model
        /// </returns>
        public virtual async Task<SystemInfoModel> PrepareSystemInfoModelAsync(SystemInfoModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.NopVersion = NopVersion.FULL_VERSION;
            model.ServerTimeZone = TimeZoneInfo.Local.StandardName;
            model.ServerLocalTime = DateTime.Now;
            model.UtcTime = DateTime.UtcNow;
            model.CurrentUserTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            model.HttpHost = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Host];

            //ensure no exception is thrown
            try
            {
                model.OperatingSystem = Environment.OSVersion.VersionString;
                model.AspNetInfo = RuntimeInformation.FrameworkDescription;
                model.IsFullTrust = AppDomain.CurrentDomain.IsFullyTrusted;
            }
            catch
            {
                // ignored
            }

            foreach (var header in _httpContextAccessor.HttpContext.Request.Headers)
            {
                model.Headers.Add(new SystemInfoModel.HeaderModel
                {
                    Name = header.Key,
                    Value = header.Value
                });
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var loadedAssemblyModel = new SystemInfoModel.LoadedAssembly
                {
                    FullName = assembly.FullName
                };

                //ensure no exception is thrown
                try
                {
                    loadedAssemblyModel.Location = assembly.IsDynamic ? null : assembly.Location;
                    loadedAssemblyModel.IsDebug = assembly.GetCustomAttributes(typeof(DebuggableAttribute), false)
                        .FirstOrDefault() is DebuggableAttribute attribute && attribute.IsJITOptimizerDisabled;

                    //https://stackoverflow.com/questions/2050396/getting-the-date-of-a-net-assembly
                    //we use a simple method because the more Jeff Atwood's solution doesn't work anymore 
                    //more info at https://blog.codinghorror.com/determining-build-date-the-hard-way/
                    loadedAssemblyModel.BuildDate = assembly.IsDynamic ? null : (DateTime?)TimeZoneInfo.ConvertTimeFromUtc(_fileProvider.GetLastWriteTimeUtc(assembly.Location), TimeZoneInfo.Local);

                }
                catch
                {
                    // ignored
                }

                model.LoadedAssemblies.Add(loadedAssemblyModel);
            }


            var currentStaticCacheManagerName = _staticCacheManager.GetType().Name;

            if (_appSettings.Get<DistributedCacheConfig>().Enabled)
                currentStaticCacheManagerName +=
                    $"({await _localizationService.GetLocalizedEnumAsync(_appSettings.Get<DistributedCacheConfig>().DistributedCacheType)})";

            model.CurrentStaticCacheManager = currentStaticCacheManagerName;

            model.AzureBlobStorageEnabled = _appSettings.Get<AzureBlobConfig>().Enabled;

            return model;
        }

        /// <summary>
        /// Prepare proxy connection warning model
        /// </summary>
        /// <param name="models">List of system warning models</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProxyConnectionWarningModelAsync(IList<SystemWarningModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //whether proxy is enabled
            if (!_proxySettings.Enabled)
                return;

            try
            {
                await _nopHttpClient.PingAsync();

                //connection is OK
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Pass,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.ProxyConnection.OK")
                });
            }
            catch
            {
                //connection failed
                models.Add(new SystemWarningModel
                {
                    Level = SystemWarningLevel.Fail,
                    Text = await _localizationService.GetResourceAsync("Admin.System.Warnings.ProxyConnection.Failed")
                });
            }
        }

        /// <summary>
        /// Prepare system warning models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of system warning models
        /// </returns>
        public virtual async Task<IList<SystemWarningModel>> PrepareSystemWarningModelsAsync()
        {
            var models = new List<SystemWarningModel>();

            //store URL
            await PrepareStoreUrlWarningModelAsync(models);

            //removal key
            await PrepareRemovalKeyWarningModelAsync(models);

            //performance settings
            await PreparePerformanceSettingsWarningModelAsync(models);

            //validate write permissions (the same procedure like during installation)
            await PrepareFilePermissionsWarningModelAsync(models);

            //proxy connection
            await PrepareProxyConnectionWarningModelAsync(models);

            //publish event
            var warningEvent = new SystemWarningCreatedEvent();
            await _eventPublisher.PublishAsync(warningEvent);
            //add another warnings (for example from plugins) 
            models.AddRange(warningEvent.SystemWarnings);

            return models;
        }

        /// <summary>
        /// Prepare maintenance model
        /// </summary>
        /// <param name="model">Maintenance model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the maintenance model
        /// </returns>
        public virtual Task<MaintenanceModel> PrepareMaintenanceModelAsync(MaintenanceModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.DeleteGuests.EndDate = DateTime.UtcNow.AddDays(-7);
            model.DeleteGuests.OnlyWithoutShoppingCart = true;
            model.DeleteAbandonedCarts.OlderThan = DateTime.UtcNow.AddDays(-182);

            model.DeleteAlreadySentQueuedEmails.EndDate = DateTime.UtcNow.AddDays(-7);

            model.BackupSupported = _dataProvider.BackupSupported;

            //prepare nested search model
            PrepareBackupFileSearchModel(model.BackupFileSearchModel);

            return Task.FromResult(model);
        }

        /// <summary>
        /// Prepare paged backup file list model
        /// </summary>
        /// <param name="searchModel">Backup file search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the backup file list model
        /// </returns>
        public virtual Task<BackupFileListModel> PrepareBackupFileListModelAsync(BackupFileSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get backup files
            var backupFiles = _maintenanceService.GetAllBackupFiles().ToPagedList(searchModel);

            //prepare list model
            var model = new BackupFileListModel().PrepareToGrid(searchModel, backupFiles, () =>
            {
                return backupFiles.Select(file => new BackupFileModel
                {
                    Name = _fileProvider.GetFileName(file),

                    //fill in additional values (not existing in the entity)
                    Length = $"{_fileProvider.FileLength(file) / 1024f / 1024f:F2} Mb",

                    Link = $"{_webHelper.GetStoreLocation()}db_backups/{_fileProvider.GetFileName(file)}"
                });
            });

            return Task.FromResult(model);
        }

        /// <summary>
        /// Prepare URL record search model
        /// </summary>
        /// <param name="searchModel">URL record search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the uRL record search model
        /// </returns>
        public virtual async Task<UrlRecordSearchModel> PrepareUrlRecordSearchModelAsync(UrlRecordSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available languages
            //we insert 0 as 'Standard' language.
            //let's insert -1 for 'All' language selection.
            await _baseAdminModelFactory.PrepareLanguagesAsync(searchModel.AvailableLanguages,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.System.SeNames.List.Language.Standard"));
            searchModel.AvailableLanguages.Insert(0,
                new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "-1" });
            searchModel.LanguageId = -1;

            //prepare "is active" filter (0 - all; 1 - active only; 2 - inactive only)
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.System.SeNames.List.IsActive.All")
            });
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.System.SeNames.List.IsActive.ActiveOnly")
            });
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.System.SeNames.List.IsActive.InactiveOnly")
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        
        /// <summary>
        /// Prepare language selector model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the language selector model
        /// </returns>
        public virtual async Task<LanguageSelectorModel> PrepareLanguageSelectorModelAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var model = new LanguageSelectorModel
            {
                CurrentLanguage = (await _workContext.GetWorkingLanguageAsync()).ToModel<LanguageModel>(),
                AvailableLanguages = (await _languageService
                    .GetAllLanguagesAsync(storeId: store.Id))
                    .Select(language => language.ToModel<LanguageModel>()).ToList()
            };

            return model;
        }
               
        #endregion
    }
}