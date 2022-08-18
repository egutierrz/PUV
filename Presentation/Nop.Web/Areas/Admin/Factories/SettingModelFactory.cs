using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Data.Configuration;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Configuration;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the setting model factory implementation
    /// </summary>
    public partial class SettingModelFactory : ISettingModelFactory
    {
        #region Fields

        private readonly AppSettings _appSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerAttributeModelFactory _customerAttributeModelFactory;
        private readonly INopDataProvider _dataProvider;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public SettingModelFactory(AppSettings appSettings,
            CurrencySettings currencySettings,
            IAddressModelFactory addressModelFactory,
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerAttributeModelFactory customerAttributeModelFactory,
            INopDataProvider dataProvider,
            IDateTimeHelper dateTimeHelper,
            IGdprService gdprService,
            ILocalizedModelFactory localizedModelFactory,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWorkContext workContext)
        {
            _appSettings = appSettings;
            _currencySettings = currencySettings;
            _addressModelFactory = addressModelFactory;
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerAttributeModelFactory = customerAttributeModelFactory;
            _dataProvider = dataProvider;
            _dateTimeHelper = dateTimeHelper;
            _gdprService = gdprService;
            _localizedModelFactory = localizedModelFactory;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

       
        /// <summary>
        /// Prepare GDPR consent search model
        /// </summary>
        /// <param name="searchModel">GDPR consent search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent search model
        /// </returns>
        protected virtual Task<GdprConsentSearchModel> PrepareGdprConsentSearchModelAsync(GdprConsentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare address settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the address settings model
        /// </returns>
        protected virtual async Task<AddressSettingsModel> PrepareAddressSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressSettings = await _settingService.LoadSettingAsync<AddressSettings>(storeId);

            //fill in model values from the entity
            var model = addressSettings.ToSettingsModel<AddressSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare customer settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer settings model
        /// </returns>
        protected virtual async Task<CustomerSettingsModel> PrepareCustomerSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var customerSettings = await _settingService.LoadSettingAsync<CustomerSettings>(storeId);

            //fill in model values from the entity
            var model = customerSettings.ToSettingsModel<CustomerSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare multi-factor authentication settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multiFactorAuthenticationSettingsModel
        /// </returns>
        protected virtual async Task<MultiFactorAuthenticationSettingsModel> PrepareMultiFactorAuthenticationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var multiFactorAuthenticationSettings = await _settingService.LoadSettingAsync<MultiFactorAuthenticationSettings>(storeId);

            //fill in model values from the entity
            var model = multiFactorAuthenticationSettings.ToSettingsModel<MultiFactorAuthenticationSettingsModel>();

            return model;

        }

        /// <summary>
        /// Prepare date time settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the date time settings model
        /// </returns>
        protected virtual async Task<DateTimeSettingsModel> PrepareDateTimeSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dateTimeSettings = await _settingService.LoadSettingAsync<DateTimeSettings>(storeId);

            //fill in model values from the entity
            var model = new DateTimeSettingsModel
            {
                AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone
            };

            //fill in additional values (not existing in the entity)
            model.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;

            //prepare available time zones
            await _baseAdminModelFactory.PrepareTimeZonesAsync(model.AvailableTimeZones, false);

            return model;
        }

        /// <summary>
        /// Prepare external authentication settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the external authentication settings model
        /// </returns>
        protected virtual async Task<ExternalAuthenticationSettingsModel> PrepareExternalAuthenticationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var externalAuthenticationSettings = await _settingService.LoadSettingAsync<ExternalAuthenticationSettings>(storeId);

            //fill in model values from the entity
            var model = new ExternalAuthenticationSettingsModel
            {
                AllowCustomersToRemoveAssociations = externalAuthenticationSettings.AllowCustomersToRemoveAssociations
            };

            return model;
        }

        /// <summary>
        /// Prepare store information settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store information settings model
        /// </returns>
        protected virtual async Task<StoreInformationSettingsModel> PrepareStoreInformationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeId);
            var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new StoreInformationSettingsModel
            {
                StoreClosed = storeInformationSettings.StoreClosed,
                DefaultStoreTheme = storeInformationSettings.DefaultStoreTheme,
                AllowCustomerToSelectTheme = storeInformationSettings.AllowCustomerToSelectTheme,
                LogoPictureId = storeInformationSettings.LogoPictureId,
                DisplayEuCookieLawWarning = storeInformationSettings.DisplayEuCookieLawWarning,
                FacebookLink = storeInformationSettings.FacebookLink,
                TwitterLink = storeInformationSettings.TwitterLink,
                YoutubeLink = storeInformationSettings.YoutubeLink,
                SubjectFieldOnContactUsForm = commonSettings.SubjectFieldOnContactUsForm,
                UseSystemEmailForContactUsForm = commonSettings.UseSystemEmailForContactUsForm,
                PopupForTermsOfServiceLinks = commonSettings.PopupForTermsOfServiceLinks
            };

            
            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.StoreClosed_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.StoreClosed, storeId);
            model.DefaultStoreTheme_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.DefaultStoreTheme, storeId);
            model.AllowCustomerToSelectTheme_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.AllowCustomerToSelectTheme, storeId);
            model.LogoPictureId_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.LogoPictureId, storeId);
            model.DisplayEuCookieLawWarning_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.DisplayEuCookieLawWarning, storeId);
            model.FacebookLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.FacebookLink, storeId);
            model.TwitterLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.TwitterLink, storeId);
            model.YoutubeLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.YoutubeLink, storeId);
            model.SubjectFieldOnContactUsForm_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.SubjectFieldOnContactUsForm, storeId);
            model.UseSystemEmailForContactUsForm_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.UseSystemEmailForContactUsForm, storeId);
            model.PopupForTermsOfServiceLinks_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.PopupForTermsOfServiceLinks, storeId);

            return model;
        }

        /// <summary>
        /// Prepare Sitemap settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sitemap settings model
        /// </returns>
        protected virtual async Task<SitemapSettingsModel> PrepareSitemapSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sitemapSettings = await _settingService.LoadSettingAsync<SitemapSettings>(storeId);

            //fill in model values from the entity
            var model = new SitemapSettingsModel
            {
                SitemapEnabled = sitemapSettings.SitemapEnabled,
                SitemapPageSize = sitemapSettings.SitemapPageSize,
                SitemapIncludeCategories = sitemapSettings.SitemapIncludeCategories,
                SitemapIncludeManufacturers = sitemapSettings.SitemapIncludeManufacturers,
                SitemapIncludeProducts = sitemapSettings.SitemapIncludeProducts,
                SitemapIncludeProductTags = sitemapSettings.SitemapIncludeProductTags,
                SitemapIncludeBlogPosts = sitemapSettings.SitemapIncludeBlogPosts,
                SitemapIncludeNews = sitemapSettings.SitemapIncludeNews,
                SitemapIncludeTopics = sitemapSettings.SitemapIncludeTopics
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.SitemapEnabled_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapEnabled, storeId);
            model.SitemapPageSize_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapPageSize, storeId);
            model.SitemapIncludeCategories_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeCategories, storeId);
            model.SitemapIncludeManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeManufacturers, storeId);
            model.SitemapIncludeProducts_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeProducts, storeId);
            model.SitemapIncludeProductTags_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeProductTags, storeId);
            model.SitemapIncludeBlogPosts_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeBlogPosts, storeId);
            model.SitemapIncludeNews_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeNews, storeId);
            model.SitemapIncludeTopics_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeTopics, storeId);

            return model;
        }

        /// <summary>
        /// Prepare minification settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the minification settings model
        /// </returns>
        protected virtual async Task<MinificationSettingsModel> PrepareMinificationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var minificationSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new MinificationSettingsModel
            {
                EnableHtmlMinification = minificationSettings.EnableHtmlMinification,
                UseResponseCompression = minificationSettings.UseResponseCompression
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.EnableHtmlMinification_OverrideForStore = await _settingService.SettingExistsAsync(minificationSettings, x => x.EnableHtmlMinification, storeId);
            model.UseResponseCompression_OverrideForStore = await _settingService.SettingExistsAsync(minificationSettings, x => x.UseResponseCompression, storeId);

            return model;
        }

        
        /// <summary>
        /// Prepare security settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the security settings model
        /// </returns>
        protected virtual async Task<SecuritySettingsModel> PrepareSecuritySettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var securitySettings = await _settingService.LoadSettingAsync<SecuritySettings>(storeId);

            //fill in model values from the entity
            var model = new SecuritySettingsModel
            {
                EncryptionKey = securitySettings.EncryptionKey,
                HoneypotEnabled = securitySettings.HoneypotEnabled
            };

            //fill in additional values (not existing in the entity)
            if (securitySettings.AdminAreaAllowedIpAddresses != null)
                model.AdminAreaAllowedIpAddresses = string.Join(",", securitySettings.AdminAreaAllowedIpAddresses);

            return model;
        }

        /// <summary>
        /// Prepare captcha settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the captcha settings model
        /// </returns>
        protected virtual async Task<CaptchaSettingsModel> PrepareCaptchaSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var captchaSettings = await _settingService.LoadSettingAsync<CaptchaSettings>(storeId);

            //fill in model values from the entity
            var model = captchaSettings.ToSettingsModel<CaptchaSettingsModel>();

            model.CaptchaTypeValues = await captchaSettings.CaptchaType.ToSelectListAsync();

            if (storeId <= 0)
                return model;

            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.Enabled, storeId);
            model.ShowOnLoginPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnLoginPage, storeId);
            model.ShowOnRegistrationPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnRegistrationPage, storeId);
            model.ShowOnContactUsPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnContactUsPage, storeId);
            model.ShowOnEmailWishlistToFriendPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnEmailWishlistToFriendPage, storeId);
            model.ShowOnEmailProductToFriendPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnEmailProductToFriendPage, storeId);
            model.ShowOnBlogCommentPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnBlogCommentPage, storeId);
            model.ShowOnNewsCommentPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnNewsCommentPage, storeId);
            model.ShowOnProductReviewPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnProductReviewPage, storeId);
            model.ShowOnApplyVendorPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnApplyVendorPage, storeId);
            model.ShowOnForgotPasswordPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnForgotPasswordPage, storeId);
            model.ShowOnForum_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnForum, storeId);
            model.ReCaptchaPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ReCaptchaPublicKey, storeId);
            model.ReCaptchaPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ReCaptchaPrivateKey, storeId);
            model.CaptchaType_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.CaptchaType, storeId);
            model.ReCaptchaV3ScoreThreshold_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ReCaptchaV3ScoreThreshold, storeId);

            return model;
        }

        
        /// <summary>
        /// Prepare localization settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the localization settings model
        /// </returns>
        protected virtual async Task<LocalizationSettingsModel> PrepareLocalizationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeId);

            //fill in model values from the entity
            var model = new LocalizationSettingsModel
            {
                UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection,
                SeoFriendlyUrlsForLanguagesEnabled = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled,
                AutomaticallyDetectLanguage = localizationSettings.AutomaticallyDetectLanguage,
                LoadAllLocaleRecordsOnStartup = localizationSettings.LoadAllLocaleRecordsOnStartup,
                LoadAllLocalizedPropertiesOnStartup = localizationSettings.LoadAllLocalizedPropertiesOnStartup,
                LoadAllUrlRecordsOnStartup = localizationSettings.LoadAllUrlRecordsOnStartup
            };

            return model;
        }

        /// <summary>
        /// Prepare admin area settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the admin area settings model
        /// </returns>
        protected virtual async Task<AdminAreaSettingsModel> PrepareAdminAreaSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var adminAreaSettings = await _settingService.LoadSettingAsync<AdminAreaSettings>(storeId);

            //fill in model values from the entity
            var model = new AdminAreaSettingsModel
            {
                UseRichEditorInMessageTemplates = adminAreaSettings.UseRichEditorInMessageTemplates
            };

            //fill in overridden values
            if (storeId > 0)
            {
                model.UseRichEditorInMessageTemplates_OverrideForStore = await _settingService.SettingExistsAsync(adminAreaSettings, x => x.UseRichEditorInMessageTemplates, storeId);
            }

            return model;
        }

        /// <summary>
        /// Prepare display default menu item settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the display default menu item settings model
        /// </returns>
        protected virtual async Task<DisplayDefaultMenuItemSettingsModel> PrepareDisplayDefaultMenuItemSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var displayDefaultMenuItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultMenuItemSettings>(storeId);

            //fill in model values from the entity
            var model = new DisplayDefaultMenuItemSettingsModel
            {
                DisplayHomepageMenuItem = displayDefaultMenuItemSettings.DisplayHomepageMenuItem,
                DisplayNewProductsMenuItem = displayDefaultMenuItemSettings.DisplayNewProductsMenuItem,
                DisplayProductSearchMenuItem = displayDefaultMenuItemSettings.DisplayProductSearchMenuItem,
                DisplayCustomerInfoMenuItem = displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem,
                DisplayBlogMenuItem = displayDefaultMenuItemSettings.DisplayBlogMenuItem,
                DisplayForumsMenuItem = displayDefaultMenuItemSettings.DisplayForumsMenuItem,
                DisplayContactUsMenuItem = displayDefaultMenuItemSettings.DisplayContactUsMenuItem
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.DisplayHomepageMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayHomepageMenuItem, storeId);
            model.DisplayNewProductsMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayNewProductsMenuItem, storeId);
            model.DisplayProductSearchMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayProductSearchMenuItem, storeId);
            model.DisplayCustomerInfoMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayCustomerInfoMenuItem, storeId);
            model.DisplayBlogMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayBlogMenuItem, storeId);
            model.DisplayForumsMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayForumsMenuItem, storeId);
            model.DisplayContactUsMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayContactUsMenuItem, storeId);

            return model;
        }

        /// <summary>
        /// Prepare display default footer item settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the display default footer item settings model
        /// </returns>
        protected virtual async Task<DisplayDefaultFooterItemSettingsModel> PrepareDisplayDefaultFooterItemSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var displayDefaultFooterItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultFooterItemSettings>(storeId);

            //fill in model values from the entity
            var model = new DisplayDefaultFooterItemSettingsModel
            {
                DisplaySitemapFooterItem = displayDefaultFooterItemSettings.DisplaySitemapFooterItem,
                DisplayContactUsFooterItem = displayDefaultFooterItemSettings.DisplayContactUsFooterItem,
                DisplayProductSearchFooterItem = displayDefaultFooterItemSettings.DisplayProductSearchFooterItem,
                DisplayNewsFooterItem = displayDefaultFooterItemSettings.DisplayNewsFooterItem,
                DisplayBlogFooterItem = displayDefaultFooterItemSettings.DisplayBlogFooterItem,
                DisplayForumsFooterItem = displayDefaultFooterItemSettings.DisplayForumsFooterItem,
                DisplayRecentlyViewedProductsFooterItem = displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem,
                DisplayCompareProductsFooterItem = displayDefaultFooterItemSettings.DisplayCompareProductsFooterItem,
                DisplayNewProductsFooterItem = displayDefaultFooterItemSettings.DisplayNewProductsFooterItem,
                DisplayCustomerInfoFooterItem = displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem,
                DisplayCustomerOrdersFooterItem = displayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem,
                DisplayCustomerAddressesFooterItem = displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem,
                DisplayShoppingCartFooterItem = displayDefaultFooterItemSettings.DisplayShoppingCartFooterItem,
                DisplayWishlistFooterItem = displayDefaultFooterItemSettings.DisplayWishlistFooterItem,
                DisplayApplyVendorAccountFooterItem = displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.DisplaySitemapFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplaySitemapFooterItem, storeId);
            model.DisplayContactUsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayContactUsFooterItem, storeId);
            model.DisplayProductSearchFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayProductSearchFooterItem, storeId);
            model.DisplayNewsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayNewsFooterItem, storeId);
            model.DisplayBlogFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayBlogFooterItem, storeId);
            model.DisplayForumsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayForumsFooterItem, storeId);
            model.DisplayRecentlyViewedProductsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayRecentlyViewedProductsFooterItem, storeId);
            model.DisplayCompareProductsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCompareProductsFooterItem, storeId);
            model.DisplayNewProductsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayNewProductsFooterItem, storeId);
            model.DisplayCustomerInfoFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerInfoFooterItem, storeId);
            model.DisplayCustomerOrdersFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerOrdersFooterItem, storeId);
            model.DisplayCustomerAddressesFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerAddressesFooterItem, storeId);
            model.DisplayShoppingCartFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayShoppingCartFooterItem, storeId);
            model.DisplayWishlistFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayWishlistFooterItem, storeId);
            model.DisplayApplyVendorAccountFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayApplyVendorAccountFooterItem, storeId);

            return model;
        }

        /// <summary>
        /// Prepare setting model to add
        /// </summary>
        /// <param name="model">Setting model to add</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareAddSettingModelAsync(SettingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare app settings model
        /// </summary>
        /// <param name="model">AppSettings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the app settings model
        /// </returns>
        public virtual async Task<AppSettingsModel> PrepareAppSettingsModel(AppSettingsModel model = null)
        {
            model ??= new AppSettingsModel
            {
                CacheConfigModel = _appSettings.Get<CacheConfig>().ToConfigModel<CacheConfigModel>(),
                HostingConfigModel = _appSettings.Get<HostingConfig>().ToConfigModel<HostingConfigModel>(),
                DistributedCacheConfigModel = _appSettings.Get<DistributedCacheConfig>().ToConfigModel<DistributedCacheConfigModel>(),
                AzureBlobConfigModel = _appSettings.Get<AzureBlobConfig>().ToConfigModel<AzureBlobConfigModel>(),
                InstallationConfigModel = _appSettings.Get<InstallationConfig>().ToConfigModel<InstallationConfigModel>(),
                PluginConfigModel = _appSettings.Get<PluginConfig>().ToConfigModel<PluginConfigModel>(),
                CommonConfigModel = _appSettings.Get<CommonConfig>().ToConfigModel<CommonConfigModel>(),
                DataConfigModel = _appSettings.Get<DataConfig>().ToConfigModel<DataConfigModel>(),
                WebOptimizerConfigModel = _appSettings.Get<WebOptimizerConfig>().ToConfigModel<WebOptimizerConfigModel>(),
            };

            model.DistributedCacheConfigModel.DistributedCacheTypeValues = await _appSettings.Get<DistributedCacheConfig>().DistributedCacheType.ToSelectListAsync();

            model.DataConfigModel.DataProviderTypeValues = await _appSettings.Get<DataConfig>().DataProvider.ToSelectListAsync();

            //Since we decided to use the naming of the DB connections section as in the .net core - "ConnectionStrings",
            //we are forced to adjust our internal model naming to this convention in this check.
            model.EnvironmentVariables.AddRange(from property in model.GetType().GetProperties()
                                                where property.Name != nameof(AppSettingsModel.EnvironmentVariables)
                                                from pp in property.PropertyType.GetProperties()
                                                where Environment.GetEnvironmentVariables().Contains($"{property.Name.Replace("Model", "").Replace("DataConfig", "ConnectionStrings")}__{pp.Name}")
                                                select $"{property.Name}_{pp.Name}");
            return model;
        }
       
        
        
        /// <summary>
        /// Prepare customer user settings model
        /// </summary>
        /// <param name="model">Customer user settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer user settings model
        /// </returns>
        public virtual async Task<CustomerUserSettingsModel> PrepareCustomerUserSettingsModelAsync(CustomerUserSettingsModel model = null)
        {
            model ??= new CustomerUserSettingsModel
            {
                ActiveStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync()
            };

            //prepare customer settings model
            model.CustomerSettings = await PrepareCustomerSettingsModelAsync();

            //prepare multi-factor authentication settings model
            model.MultiFactorAuthenticationSettings = await PrepareMultiFactorAuthenticationSettingsModelAsync();

            //prepare address settings model
            model.AddressSettings = await PrepareAddressSettingsModelAsync();

            //prepare date time settings model
            model.DateTimeSettings = await PrepareDateTimeSettingsModelAsync();

            //prepare external authentication settings model
            model.ExternalAuthenticationSettings = await PrepareExternalAuthenticationSettingsModelAsync();

            //prepare nested search models
            await _customerAttributeModelFactory.PrepareCustomerAttributeSearchModelAsync(model.CustomerAttributeSearchModel);
            await _addressAttributeModelFactory.PrepareAddressAttributeSearchModelAsync(model.AddressAttributeSearchModel);

            return model;
        }

        /// <summary>
        /// Prepare GDPR settings model
        /// </summary>
        /// <param name="model">Gdpr settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR settings model
        /// </returns>
        public virtual async Task<GdprSettingsModel> PrepareGdprSettingsModelAsync(GdprSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var gdprSettings = await _settingService.LoadSettingAsync<GdprSettings>(storeId);

            //fill in model values from the entity
            model ??= gdprSettings.ToSettingsModel<GdprSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            //prepare nested search model
            await PrepareGdprConsentSearchModelAsync(model.GdprConsentSearchModel);

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.GdprEnabled_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.GdprEnabled, storeId);
            model.LogPrivacyPolicyConsent_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.LogPrivacyPolicyConsent, storeId);
            model.LogNewsletterConsent_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.LogNewsletterConsent, storeId);
            model.LogUserProfileChanges_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.LogUserProfileChanges, storeId);

            return model;
        }

        /// <summary>
        /// Prepare paged GDPR consent list model
        /// </summary>
        /// <param name="searchModel">GDPR search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent list model
        /// </returns>
        public virtual async Task<GdprConsentListModel> PrepareGdprConsentListModelAsync(GdprConsentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get sort options
            var consentList = (await _gdprService.GetAllConsentsAsync()).ToPagedList(searchModel);

            //prepare list model
            var model = await new GdprConsentListModel().PrepareToGridAsync(searchModel, consentList, () =>
            {
                return consentList.SelectAwait(async consent =>
                {
                    var gdprConsentModel = consent.ToModel<GdprConsentModel>();

                    var gdprConsent = await _gdprService.GetConsentByIdAsync(gdprConsentModel.Id);
                    gdprConsentModel.Message = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.Message);
                    gdprConsentModel.RequiredMessage = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.RequiredMessage);

                    return gdprConsentModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare GDPR consent model
        /// </summary>
        /// <param name="model">GDPR consent model</param>
        /// <param name="gdprConsent">GDPR consent</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent model
        /// </returns>
        public virtual async Task<GdprConsentModel> PrepareGdprConsentModelAsync(GdprConsentModel model, GdprConsent gdprConsent, bool excludeProperties = false)
        {
            Func<GdprConsentLocalizedModel, int, Task> localizedModelConfiguration = null;

            //fill in model values from the entity
            if (gdprConsent != null)
            {
                model ??= gdprConsent.ToModel<GdprConsentModel>();

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Message = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.Message, languageId, false, false);
                    locale.RequiredMessage = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.RequiredMessage, languageId, false, false);
                };
            }

            //set default values for the new model
            if (gdprConsent == null)
                model.DisplayOrder = 1;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare general and common settings model
        /// </summary>
        /// <param name="model">General common settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the general and common settings model
        /// </returns>
        public virtual async Task<GeneralCommonSettingsModel> PrepareGeneralCommonSettingsModelAsync(GeneralCommonSettingsModel model = null)
        {
            model ??= new GeneralCommonSettingsModel
            {
                ActiveStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync()
            };

            //prepare store information settings model
            model.StoreInformationSettings = await PrepareStoreInformationSettingsModelAsync();

            //prepare Sitemap settings model
            model.SitemapSettings = await PrepareSitemapSettingsModelAsync();

            //prepare Minification settings model
            model.MinificationSettings = await PrepareMinificationSettingsModelAsync();

            //prepare security settings model
            model.SecuritySettings = await PrepareSecuritySettingsModelAsync();

            //prepare captcha settings model
            model.CaptchaSettings = await PrepareCaptchaSettingsModelAsync();

            //prepare PDF settings model
            model.LocalizationSettings = await PrepareLocalizationSettingsModelAsync();

            //prepare admin area settings model
            model.AdminAreaSettings = await PrepareAdminAreaSettingsModelAsync();

            //prepare display default menu item settings model
            model.DisplayDefaultMenuItemSettings = await PrepareDisplayDefaultMenuItemSettingsModelAsync();

            //prepare display default footer item settings model
            model.DisplayDefaultFooterItemSettings = await PrepareDisplayDefaultFooterItemSettingsModelAsync();

            return model;
        }

        /// <summary>
        /// Prepare setting search model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting search model
        /// </returns>
        public virtual async Task<SettingSearchModel> PrepareSettingSearchModelAsync(SettingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare model to add
            await PrepareAddSettingModelAsync(searchModel.AddSetting);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged setting list model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting list model
        /// </returns>
        public virtual async Task<SettingListModel> PrepareSettingListModelAsync(SettingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get settings
            var settings = (await _settingService.GetAllSettingsAsync()).AsQueryable();

            //filter settings
            if (!string.IsNullOrEmpty(searchModel.SearchSettingName))
                settings = settings.Where(setting => setting.Name.ToLowerInvariant().Contains(searchModel.SearchSettingName.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(searchModel.SearchSettingValue))
                settings = settings.Where(setting => setting.Value.ToLowerInvariant().Contains(searchModel.SearchSettingValue.ToLowerInvariant()));

            var pagedSettings = settings.ToList().ToPagedList(searchModel);

            //prepare list model
            var model = await new SettingListModel().PrepareToGridAsync(searchModel, pagedSettings, () =>
            {
                return pagedSettings.SelectAwait(async setting =>
                {
                    //fill in model values from the entity
                    var settingModel = setting.ToModel<SettingModel>();

                    //fill in additional values (not existing in the entity)
                    settingModel.Store = setting.StoreId > 0
                        ? (await _storeService.GetStoreByIdAsync(setting.StoreId))?.Name ?? "Deleted"
                        : await _localizationService.GetResourceAsync("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");

                    return settingModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare setting mode model
        /// </summary>
        /// <param name="modeName">Mode name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting mode model
        /// </returns>
        public virtual async Task<SettingModeModel> PrepareSettingModeModelAsync(string modeName)
        {
            var model = new SettingModeModel
            {
                ModeName = modeName,
                Enabled = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), modeName)
            };

            return model;
        }

        /// <summary>
        /// Prepare store scope configuration model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store scope configuration model
        /// </returns>
        public virtual async Task<StoreScopeConfigurationModel> PrepareStoreScopeConfigurationModelAsync()
        {
            var model = new StoreScopeConfigurationModel
            {
                Stores = (await _storeService.GetAllStoresAsync()).Select(store => store.ToModel<StoreModel>()).ToList(),
                StoreId = await _storeContext.GetActiveStoreScopeConfigurationAsync()
            };

            return model;
        }

        #endregion
    }
}