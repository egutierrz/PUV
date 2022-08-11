using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Configuration;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Media.RoxyFileman;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework;
using Nop.Web.Framework.Configuration;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ComponentSettingController : BaseAdminController
    {
        #region Fields

        private readonly AppSettings _appSettings;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly INopDataProvider _dataProvider;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IRoxyFilemanService _roxyFilemanService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IComponentSettingModelFactory _settingModelFactory;
        private readonly IComponentSettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IUploadService _uploadService;
        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public ComponentSettingController(AppSettings appSettings,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            INopDataProvider dataProvider,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IRoxyFilemanService roxyFilemanService,
            IServiceScopeFactory serviceScopeFactory,
            IComponentSettingModelFactory settingModelFactory,
            IComponentSettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWorkContext workContext,
            IUploadService uploadService,
            IGdprService gdprService)
        {
            _appSettings = appSettings;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dataProvider = dataProvider;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _gdprService = gdprService;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _roxyFilemanService = roxyFilemanService;
            _serviceScopeFactory = serviceScopeFactory;
            _settingModelFactory = settingModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _workContext = workContext;
            _uploadService = uploadService;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateGdprConsentLocalesAsync(GdprConsent gdprConsent, GdprConsentModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(gdprConsent,
                    x => x.Message,
                    localized.Message,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(gdprConsent,
                    x => x.RequiredMessage,
                    localized.RequiredMessage,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> AllSettings(string settingName)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareSettingSearchModelAsync(new ComponentSettingSearchModel { SearchSettingName = WebUtility.HtmlEncode(settingName) });

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AllSettings(ComponentSettingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _settingModelFactory.PrepareSettingListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SettingUpdate(ComponentSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            //try to get a setting with the specified id
            var setting = await _settingService.GetSettingByIdAsync(model.Id)
                ?? throw new ArgumentException("No setting found with the specified id");

            if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                //setting name has been changed
                await _settingService.DeleteSettingAsync(setting);
            }

            await _settingService.SetSettingAsync(model.Name, model.Value, setting.StoreId);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"), setting);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> SettingAdd(ComponentSettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var storeId = model.StoreId;
            await _settingService.SetSettingAsync(model.Name, model.Value, storeId);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewSetting",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewSetting"), model.Name),
                await _settingService.GetSettingAsync(model.Name, storeId));

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> SettingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //try to get a setting with the specified id
            var setting = await _settingService.GetSettingByIdAsync(id)
                ?? throw new ArgumentException("No setting found with the specified id", nameof(id));

            await _settingService.DeleteSettingAsync(setting);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteSetting",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteSetting"), setting.Name), setting);

            return new NullJsonResult();
        }

        //action displaying notification (warning) to a store owner about a lot of traffic 
        //between the distributed cache server and the application when LoadAllLocaleRecordsOnStartup setting is set
        public async Task<IActionResult> DistributedCacheHighTrafficWarning(bool loadAllLocaleRecordsOnStartup)
        {
            //LoadAllLocaleRecordsOnStartup is set and distributed cache is used, so display warning
            if (_appSettings.Get<DistributedCacheConfig>().Enabled && _appSettings.Get<DistributedCacheConfig>().DistributedCacheType != DistributedCacheType.Memory && loadAllLocaleRecordsOnStartup)
            {
                return Json(new
                {
                    Result = await _localizationService
                        .GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.LoadAllLocaleRecordsOnStartup.Warning")
                });
            }

            return Json(new { Result = string.Empty });
        }

        //Action that displays a notification (warning) to the store owner about the absence of active authentication providers
        public async Task<IActionResult> ForceMultifactorAuthenticationWarning(bool forceMultifactorAuthentication)
        {
            //ForceMultifactorAuthentication is set and the store haven't active Authentication provider , so display warning
            if (forceMultifactorAuthentication && !await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
            {
                return Json(new
                {
                    Result = await _localizationService
                        .GetResourceAsync("Admin.Configuration.Settings.CustomerUser.ForceMultifactorAuthentication.Warning")
                });
            }

            return Json(new { Result = string.Empty });
        }

        //Action that displays a notification (warning) to the store owner about the need to restart the application after changing the setting
        public async Task<IActionResult> SeoFriendlyUrlsForLanguagesEnabledWarning(bool seoFriendlyUrlsForLanguagesEnabled)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeScope);

            if (seoFriendlyUrlsForLanguagesEnabled != localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                return Json(new
                {
                    Result = await _localizationService
                        .GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.SeoFriendlyUrlsForLanguagesEnabled.Warning")
                });
            }

            return Json(new { Result = string.Empty });
        }

        #endregion
    }
}