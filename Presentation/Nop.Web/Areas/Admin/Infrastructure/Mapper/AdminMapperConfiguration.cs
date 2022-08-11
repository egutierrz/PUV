using AutoMapper;
using Nop.Core.Configuration;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure.Mapper;
using Nop.Data.Configuration;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Areas.Admin.Models.Cms;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Directory;
using Nop.Web.Areas.Admin.Models.ExternalAuthentication;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Areas.Admin.Models.Logging;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.MultiFactorAuthentication;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Areas.Admin.Models.Tasks;
using Nop.Web.Areas.Admin.Models.Templates;
using Nop.Web.Areas.Admin.Models.Topics;
using Nop.Web.Framework.Configuration;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class AdminMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public AdminMapperConfiguration()
        {
            //create specific maps
            CreateConfigMaps();
            CreateAuthenticationMaps();
            CreateMultiFactorAuthenticationMaps();
            CreateCmsMaps();
            CreateCommonMaps();
            CreateCustomersMaps();
            CreateDirectoryMaps();
            CreateGdprMaps();
            CreateLocalizationMaps();
            CreateLoggingMaps();
            CreateMediaMaps();
            CreateMessagesMaps();
            CreatePluginsMaps();
            CreateSecurityMaps();
            CreateSeoMaps();
            CreateStoresMaps();
            CreateTasksMaps();
            CreateTopicsMaps();

            //add some generic mapping rules
            ForAllMaps((mapConfiguration, map) =>
            {
                //exclude Form and CustomProperties from mapping BaseNopModel
                if (typeof(BaseNopModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    //map.ForMember(nameof(BaseNopModel.Form), options => options.Ignore());
                    map.ForMember(nameof(BaseNopModel.CustomProperties), options => options.Ignore());
                }

                //exclude ActiveStoreScopeConfiguration from mapping ISettingsModel
                if (typeof(ISettingsModel).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(ISettingsModel.ActiveStoreScopeConfiguration), options => options.Ignore());

                //exclude some properties from mapping configuration and models
                if (typeof(IConfig).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IConfig.Name), options => options.Ignore());

                //exclude Locales from mapping ILocalizedModel
                if (typeof(ILocalizedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(ILocalizedModel<ILocalizedModel>.Locales), options => options.Ignore());

                //exclude some properties from mapping store mapping supported entities and models
                if (typeof(IStoreMappingSupported).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IStoreMappingSupported.LimitedToStores), options => options.Ignore());
                if (typeof(IStoreMappingSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IStoreMappingSupportedModel.AvailableStores), options => options.Ignore());
                    map.ForMember(nameof(IStoreMappingSupportedModel.SelectedStoreIds), options => options.Ignore());
                }

                //exclude some properties from mapping ACL supported entities and models
                if (typeof(IAclSupported).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IAclSupported.SubjectToAcl), options => options.Ignore());
                if (typeof(IAclSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IAclSupportedModel.AvailableCustomerRoles), options => options.Ignore());
                    map.ForMember(nameof(IAclSupportedModel.SelectedCustomerRoleIds), options => options.Ignore());
                }

                //exclude some properties from mapping discount supported entities and models
                if (typeof(IDiscountSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IDiscountSupportedModel.AvailableDiscounts), options => options.Ignore());
                    map.ForMember(nameof(IDiscountSupportedModel.SelectedDiscountIds), options => options.Ignore());
                }

                if (typeof(IPluginModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    //exclude some properties from mapping plugin models
                    map.ForMember(nameof(IPluginModel.ConfigurationUrl), options => options.Ignore());
                    map.ForMember(nameof(IPluginModel.IsActive), options => options.Ignore());
                    map.ForMember(nameof(IPluginModel.LogoUrl), options => options.Ignore());

                    //define specific rules for mapping plugin models
                    if (typeof(IPlugin).IsAssignableFrom(mapConfiguration.SourceType))
                    {
                        map.ForMember(nameof(IPluginModel.DisplayOrder), options => options.MapFrom(plugin => ((IPlugin)plugin).PluginDescriptor.DisplayOrder));
                        map.ForMember(nameof(IPluginModel.FriendlyName), options => options.MapFrom(plugin => ((IPlugin)plugin).PluginDescriptor.FriendlyName));
                        map.ForMember(nameof(IPluginModel.SystemName), options => options.MapFrom(plugin => ((IPlugin)plugin).PluginDescriptor.SystemName));
                    }
                }
            });
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Create configuration maps 
        /// </summary>
        protected virtual void CreateConfigMaps()
        {
            CreateMap<CacheConfig, CacheConfigModel>();
            CreateMap<CacheConfigModel, CacheConfig>();

            CreateMap<HostingConfig, HostingConfigModel>();
            CreateMap<HostingConfigModel, HostingConfig>();

            CreateMap<DistributedCacheConfig, DistributedCacheConfigModel>()
                .ForMember(model => model.DistributedCacheTypeValues, options => options.Ignore());
            CreateMap<DistributedCacheConfigModel, DistributedCacheConfig>();

            CreateMap<AzureBlobConfig, AzureBlobConfigModel>();
            CreateMap<AzureBlobConfigModel, AzureBlobConfig>()
                .ForMember(entity => entity.Enabled, options => options.Ignore())
                .ForMember(entity => entity.DataProtectionKeysEncryptWithVault, options => options.Ignore());

            CreateMap<InstallationConfig, InstallationConfigModel>();
            CreateMap<InstallationConfigModel, InstallationConfig>();

            CreateMap<PluginConfig, PluginConfigModel>();
            CreateMap<PluginConfigModel, PluginConfig>();

            CreateMap<CommonConfig, CommonConfigModel>();
            CreateMap<CommonConfigModel, CommonConfig>();

            CreateMap<DataConfig, DataConfigModel>()
                .ForMember(model => model.DataProviderTypeValues, options => options.Ignore());
            CreateMap<DataConfigModel, DataConfig>();

            CreateMap<WebOptimizerConfig, WebOptimizerConfigModel>();
            CreateMap<WebOptimizerConfigModel, WebOptimizerConfig>()
                .ForMember(entity => entity.CdnUrl, options => options.Ignore())
                .ForMember(entity => entity.AllowEmptyBundle, options => options.Ignore())
                .ForMember(entity => entity.HttpsCompression, options => options.Ignore())
                .ForMember(entity => entity.EnableTagHelperBundling, options => options.Ignore())
                .ForMember(entity => entity.EnableCaching, options => options.Ignore())
                .ForMember(entity => entity.EnableMemoryCache, options => options.Ignore());
        }

        /// <summary>
        /// Create authentication maps 
        /// </summary>
        protected virtual void CreateAuthenticationMaps()
        {
            CreateMap<IExternalAuthenticationMethod, ExternalAuthenticationMethodModel>();
        }

        /// <summary>
        /// Create multi-factor authentication maps 
        /// </summary>
        protected virtual void CreateMultiFactorAuthenticationMaps()
        {
            CreateMap<IMultiFactorAuthenticationMethod, MultiFactorAuthenticationMethodModel>();
        }

        /// <summary>
        /// Create CMS maps 
        /// </summary>
        protected virtual void CreateCmsMaps()
        {
            CreateMap<IWidgetPlugin, WidgetModel>()
                .ForMember(model => model.WidgetViewComponentArguments, options => options.Ignore())
                .ForMember(model => model.WidgetViewComponentName, options => options.Ignore());
        }

        /// <summary>
        /// Create common maps 
        /// </summary>
        protected virtual void CreateCommonMaps()
        {
            CreateMap<Address, AddressModel>()
                .ForMember(model => model.AddressHtml, options => options.Ignore())
                .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.CountryName, options => options.Ignore())
                .ForMember(model => model.CustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.FormattedCustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.CityRequired, options => options.Ignore())
                .ForMember(model => model.CompanyRequired, options => options.Ignore())
                .ForMember(model => model.CountryRequired, options => options.Ignore())
                .ForMember(model => model.CountyRequired, options => options.Ignore())
                .ForMember(model => model.EmailRequired, options => options.Ignore())
                .ForMember(model => model.FaxRequired, options => options.Ignore())
                .ForMember(model => model.FirstNameRequired, options => options.Ignore())
                .ForMember(model => model.LastNameRequired, options => options.Ignore())
                .ForMember(model => model.PhoneRequired, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Required, options => options.Ignore())
                .ForMember(model => model.StreetAddressRequired, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeRequired, options => options.Ignore());
            CreateMap<AddressModel, Address>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomAttributes, options => options.Ignore());

            CreateMap<AddressAttribute, AddressAttributeModel>()
                .ForMember(model => model.AddressAttributeValueSearchModel, options => options.Ignore())
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore());
            CreateMap<AddressAttributeModel, AddressAttribute>()
                .ForMember(entity => entity.AttributeControlType, options => options.Ignore());

            CreateMap<AddressAttributeValue, AddressAttributeValueModel>();
            CreateMap<AddressAttributeValueModel, AddressAttributeValue>();

            CreateMap<AddressSettings, AddressSettingsModel>();
            CreateMap<AddressSettingsModel, AddressSettings>()
                .ForMember(settings => settings.PreselectCountryIfOnlyOne, options => options.Ignore());

            CreateMap<Setting, SettingModel>()
                .ForMember(setting => setting.AvailableStores, options => options.Ignore())
                .ForMember(setting => setting.Store, options => options.Ignore());
        }

        /// <summary>
        /// Create customers maps 
        /// </summary>
        protected virtual void CreateCustomersMaps()
        {
            CreateMap<CustomerAttribute, CustomerAttributeModel>()
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore())
                .ForMember(model => model.CustomerAttributeValueSearchModel, options => options.Ignore());
            CreateMap<CustomerAttributeModel, CustomerAttribute>()
                .ForMember(entity => entity.AttributeControlType, options => options.Ignore());

            CreateMap<CustomerAttributeValue, CustomerAttributeValueModel>();
            CreateMap<CustomerAttributeValueModel, CustomerAttributeValue>();

            CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(model => model.PurchasedWithProductName, options => options.Ignore())
                .ForMember(model => model.TaxDisplayTypeValues, options => options.Ignore());
            CreateMap<CustomerRoleModel, CustomerRole>();

            CreateMap<CustomerSettings, CustomerSettingsModel>();
            CreateMap<CustomerSettingsModel, CustomerSettings>()
                .ForMember(settings => settings.AvatarMaximumSizeBytes, options => options.Ignore())
                .ForMember(settings => settings.DeleteGuestTaskOlderThanMinutes, options => options.Ignore())
                .ForMember(settings => settings.DownloadableProductsValidateUser, options => options.Ignore())
                .ForMember(settings => settings.HashedPasswordFormat, options => options.Ignore())
                .ForMember(settings => settings.OnlineCustomerMinutes, options => options.Ignore())
                .ForMember(settings => settings.SuffixDeletedCustomers, options => options.Ignore())
                .ForMember(settings => settings.LastActivityMinutes, options => options.Ignore());

            CreateMap<MultiFactorAuthenticationSettings, MultiFactorAuthenticationSettingsModel>();
            CreateMap<MultiFactorAuthenticationSettingsModel, MultiFactorAuthenticationSettings>()
                .ForMember(settings => settings.ActiveAuthenticationMethodSystemNames, option => option.Ignore());

            CreateMap<RewardPointsSettings, RewardPointsSettingsModel>()
                .ForMember(model => model.ActivatePointsImmediately, options => options.Ignore())
                .ForMember(model => model.ActivationDelay_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayHowMuchWillBeEarned_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Enabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExchangeRate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaximumRewardPointsToUsePerOrder_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MinimumRewardPointsToUse_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MinOrderTotalToAwardPoints_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PointsForPurchases_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaximumRedeemedRate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PointsForRegistration_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore())
                .ForMember(model => model.PurchasesPointsValidity_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RegistrationPointsValidity_OverrideForStore, options => options.Ignore());
            CreateMap<RewardPointsSettingsModel, RewardPointsSettings>();

            
            CreateMap<ActivityLog, CustomerActivityLogModel>()
               .ForMember(model => model.CreatedOn, options => options.Ignore())
               .ForMember(model => model.ActivityLogTypeName, options => options.Ignore());

            CreateMap<Customer, CustomerModel>()
                .ForMember(model => model.Email, options => options.Ignore())
                .ForMember(model => model.FullName, options => options.Ignore())
                .ForMember(model => model.Company, options => options.Ignore())
                .ForMember(model => model.Phone, options => options.Ignore())
                .ForMember(model => model.ZipPostalCode, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.LastActivityDate, options => options.Ignore())
                .ForMember(model => model.CustomerRoleNames, options => options.Ignore())
                .ForMember(model => model.AvatarUrl, options => options.Ignore())
                .ForMember(model => model.UsernamesEnabled, options => options.Ignore())
                .ForMember(model => model.Password, options => options.Ignore())
                .ForMember(model => model.AvailableVendors, options => options.Ignore())
                .ForMember(model => model.GenderEnabled, options => options.Ignore())
                .ForMember(model => model.Gender, options => options.Ignore())
                .ForMember(model => model.FirstNameEnabled, options => options.Ignore())
                .ForMember(model => model.FirstName, options => options.Ignore())
                .ForMember(model => model.LastNameEnabled, options => options.Ignore())
                .ForMember(model => model.LastName, options => options.Ignore())
                .ForMember(model => model.DateOfBirthEnabled, options => options.Ignore())
                .ForMember(model => model.DateOfBirth, options => options.Ignore())
                .ForMember(model => model.CompanyEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddressEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Enabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress2, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeEnabled, options => options.Ignore())
                .ForMember(model => model.CityEnabled, options => options.Ignore())
                .ForMember(model => model.City, options => options.Ignore())
                .ForMember(model => model.CountyEnabled, options => options.Ignore())
                .ForMember(model => model.County, options => options.Ignore())
                .ForMember(model => model.CountryEnabled, options => options.Ignore())
                .ForMember(model => model.CountryId, options => options.Ignore())
                .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.StateProvinceEnabled, options => options.Ignore())
                .ForMember(model => model.StateProvinceId, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.PhoneEnabled, options => options.Ignore())
                .ForMember(model => model.FaxEnabled, options => options.Ignore())
                .ForMember(model => model.Fax, options => options.Ignore())
                .ForMember(model => model.CustomerAttributes, options => options.Ignore())
                .ForMember(model => model.RegisteredInStore, options => options.Ignore())
                .ForMember(model => model.DisplayRegisteredInStore, options => options.Ignore())
                .ForMember(model => model.AffiliateName, options => options.Ignore())
                .ForMember(model => model.TimeZoneId, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToSetTimeZone, options => options.Ignore())
                .ForMember(model => model.AvailableTimeZones, options => options.Ignore())
                .ForMember(model => model.VatNumber, options => options.Ignore())
                .ForMember(model => model.VatNumberStatusNote, options => options.Ignore())
                .ForMember(model => model.DisplayVatNumber, options => options.Ignore())
                .ForMember(model => model.LastVisitedPage, options => options.Ignore())
                .ForMember(model => model.AvailableNewsletterSubscriptionStores, options => options.Ignore())
                .ForMember(model => model.SelectedNewsletterSubscriptionStoreIds, options => options.Ignore())
                .ForMember(model => model.DisplayRewardPointsHistory, options => options.Ignore())
                .ForMember(model => model.SendEmail, options => options.Ignore())
                .ForMember(model => model.SendPm, options => options.Ignore())
                .ForMember(model => model.AllowSendingOfPrivateMessage, options => options.Ignore())
                .ForMember(model => model.AllowSendingOfWelcomeMessage, options => options.Ignore())
                .ForMember(model => model.AllowReSendingOfActivationMessage, options => options.Ignore())
                .ForMember(model => model.GdprEnabled, options => options.Ignore())
                .ForMember(model => model.MultiFactorAuthenticationProvider, options => options.Ignore())
                .ForMember(model => model.CustomerAssociatedExternalAuthRecordsSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerAddressSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerActivityLogSearchModel, options => options.Ignore());

            CreateMap<CustomerModel, Customer>()
                .ForMember(entity => entity.CustomerGuid, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LastActivityDateUtc, options => options.Ignore())
                .ForMember(entity => entity.EmailToRevalidate, options => options.Ignore())
                .ForMember(entity => entity.HasShoppingCartItems, options => options.Ignore())
                .ForMember(entity => entity.RequireReLogin, options => options.Ignore())
                .ForMember(entity => entity.FailedLoginAttempts, options => options.Ignore())
                .ForMember(entity => entity.CannotLoginUntilDateUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.IsSystemAccount, options => options.Ignore())
                .ForMember(entity => entity.SystemName, options => options.Ignore())
                .ForMember(entity => entity.LastLoginDateUtc, options => options.Ignore())
                .ForMember(entity => entity.BillingAddressId, options => options.Ignore())
                .ForMember(entity => entity.ShippingAddressId, options => options.Ignore())
                .ForMember(entity => entity.RegisteredInStoreId, options => options.Ignore());

            CreateMap<Customer, OnlineCustomerModel>()
                .ForMember(model => model.LastActivityDate, options => options.Ignore())
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.LastIpAddress, options => options.Ignore())
                .ForMember(model => model.Location, options => options.Ignore())
                .ForMember(model => model.LastVisitedPage, options => options.Ignore());
        }

        /// <summary>
        /// Create directory maps 
        /// </summary>
        protected virtual void CreateDirectoryMaps()
        {
            CreateMap<Country, CountryModel>()
                .ForMember(model => model.NumberOfStates, options => options.Ignore())
                .ForMember(model => model.StateProvinceSearchModel, options => options.Ignore());
            CreateMap<CountryModel, Country>();

            CreateMap<Currency, CurrencyModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.IsPrimaryExchangeRateCurrency, options => options.Ignore())
                .ForMember(model => model.IsPrimaryStoreCurrency, options => options.Ignore());
            CreateMap<CurrencyModel, Currency>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.RoundingType, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<MeasureDimension, MeasureDimensionModel>()
                .ForMember(model => model.IsPrimaryDimension, options => options.Ignore());
            CreateMap<MeasureDimensionModel, MeasureDimension>();

            CreateMap<MeasureWeight, MeasureWeightModel>()
                .ForMember(model => model.IsPrimaryWeight, options => options.Ignore());
            CreateMap<MeasureWeightModel, MeasureWeight>();

            CreateMap<StateProvince, StateProvinceModel>();
            CreateMap<StateProvinceModel, StateProvince>();
        }


        /// <summary>
        /// Create GDPR maps 
        /// </summary>
        protected virtual void CreateGdprMaps()
        {
            CreateMap<GdprSettings, GdprSettingsModel>()
                .ForMember(model => model.GdprConsentSearchModel, options => options.Ignore())
                .ForMember(model => model.GdprEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LogNewsletterConsent_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LogPrivacyPolicyConsent_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LogUserProfileChanges_OverrideForStore, options => options.Ignore());
            CreateMap<GdprSettingsModel, GdprSettings>();

            CreateMap<GdprConsent, GdprConsentModel>();
            CreateMap<GdprConsentModel, GdprConsent>();

            CreateMap<GdprLog, GdprLogModel>()
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.RequestType, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        /// <summary>
        /// Create localization maps 
        /// </summary>
        protected virtual void CreateLocalizationMaps()
        {
            CreateMap<Language, LanguageModel>()
                .ForMember(model => model.AvailableCurrencies, options => options.Ignore())
                .ForMember(model => model.LocaleResourceSearchModel, options => options.Ignore());
            CreateMap<LanguageModel, Language>();

            CreateMap<LocaleResourceModel, LocaleStringResource>()
                .ForMember(entity => entity.LanguageId, options => options.Ignore());
        }

        /// <summary>
        /// Create logging maps 
        /// </summary>
        protected virtual void CreateLoggingMaps()
        {
            CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(model => model.ActivityLogTypeName, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<ActivityLogModel, ActivityLog>()
                .ForMember(entity => entity.ActivityLogTypeId, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.EntityId, options => options.Ignore())
                .ForMember(entity => entity.EntityName, options => options.Ignore());

            CreateMap<ActivityLogType, ActivityLogTypeModel>();
            CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(entity => entity.SystemKeyword, options => options.Ignore());

            CreateMap<Log, LogModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.FullMessage, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<LogModel, Log>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LogLevelId, options => options.Ignore());
        }

        /// <summary>
        /// Create media maps 
        /// </summary>
        protected virtual void CreateMediaMaps()
        {
            CreateMap<MediaSettings, MediaSettingsModel>()
                .ForMember(model => model.AssociatedProductPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AvatarPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CartThumbPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CategoryThumbPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultImageQuality_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultPictureZoomEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ImportProductImagesUsingHash_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ManufacturerThumbPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaximumImageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MiniCartThumbPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MultipleThumbDirectories_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PicturesStoredIntoDatabase, options => options.Ignore())
                .ForMember(model => model.ProductDetailsPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductThumbPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.VendorThumbPictureSize_OverrideForStore, options => options.Ignore());
            CreateMap<MediaSettingsModel, MediaSettings>()
                .ForMember(settings => settings.AutoCompleteSearchThumbPictureSize, options => options.Ignore())
                .ForMember(settings => settings.AzureCacheControlHeader, options => options.Ignore())
                .ForMember(settings => settings.UseAbsoluteImagePath, options => options.Ignore())
                .ForMember(settings => settings.ImageSquarePictureSize, options => options.Ignore());
        }

        /// <summary>
        /// Create messages maps 
        /// </summary>
        protected virtual void CreateMessagesMaps()
        {
            CreateMap<Campaign, CampaignModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore())
                .ForMember(model => model.AvailableCustomerRoles, options => options.Ignore())
                .ForMember(model => model.AvailableEmailAccounts, options => options.Ignore())
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.DontSendBeforeDate, options => options.Ignore())
                .ForMember(model => model.EmailAccountId, options => options.Ignore())
                .ForMember(model => model.TestEmail, options => options.Ignore());
            CreateMap<CampaignModel, Campaign>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.DontSendBeforeDateUtc, options => options.Ignore());

            CreateMap<EmailAccount, EmailAccountModel>()
                .ForMember(model => model.IsDefaultEmailAccount, options => options.Ignore())
                .ForMember(model => model.Password, options => options.Ignore())
                .ForMember(model => model.SendTestEmailTo, options => options.Ignore());
            CreateMap<EmailAccountModel, EmailAccount>()
                .ForMember(entity => entity.Password, options => options.Ignore());

            CreateMap<MessageTemplate, MessageTemplateModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore())
                .ForMember(model => model.AvailableEmailAccounts, options => options.Ignore())
                .ForMember(model => model.HasAttachedDownload, options => options.Ignore())
                .ForMember(model => model.ListOfStores, options => options.Ignore())
                .ForMember(model => model.SendImmediately, options => options.Ignore());
            CreateMap<MessageTemplateModel, MessageTemplate>()
                .ForMember(entity => entity.DelayPeriod, options => options.Ignore());

            CreateMap<NewsLetterSubscription, NewsletterSubscriptionModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore());
            CreateMap<NewsletterSubscriptionModel, NewsLetterSubscription>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.NewsLetterSubscriptionGuid, options => options.Ignore())
                .ForMember(entity => entity.StoreId, options => options.Ignore());

            CreateMap<QueuedEmail, QueuedEmailModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.DontSendBeforeDate, options => options.Ignore())
                .ForMember(model => model.EmailAccountName, options => options.Ignore())
                .ForMember(model => model.PriorityName, options => options.Ignore())
                .ForMember(model => model.SendImmediately, options => options.Ignore())
                .ForMember(model => model.SentOn, options => options.Ignore());
            CreateMap<QueuedEmailModel, QueuedEmail>()
                .ForMember(entity => entity.AttachmentFileName, options => options.Ignore())
                .ForMember(entity => entity.AttachmentFilePath, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.DontSendBeforeDateUtc, options => options.Ignore())
                .ForMember(entity => entity.EmailAccountId, options => options.Ignore())
                .ForMember(entity => entity.Priority, options => options.Ignore())
                .ForMember(entity => entity.PriorityId, options => options.Ignore())
                .ForMember(entity => entity.SentOnUtc, options => options.Ignore());
        }

        /// <summary>
        /// Create plugins maps 
        /// </summary>
        protected virtual void CreatePluginsMaps()
        {
            CreateMap<PluginDescriptor, PluginModel>()
                .ForMember(model => model.CanChangeEnabled, options => options.Ignore())
                .ForMember(model => model.IsEnabled, options => options.Ignore());
        }

        /// <summary>
        /// Create security maps 
        /// </summary>
        protected virtual void CreateSecurityMaps()
        {
            CreateMap<CaptchaSettings, CaptchaSettingsModel>()
                .ForMember(model => model.Enabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ReCaptchaPrivateKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ReCaptchaPublicKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnApplyVendorPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnBlogCommentPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnContactUsPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnEmailProductToFriendPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnEmailWishlistToFriendPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnLoginPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnNewsCommentPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnProductReviewPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnRegistrationPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnForgotPasswordPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnForum_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CaptchaType_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ReCaptchaV3ScoreThreshold_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CaptchaTypeValues, options => options.Ignore());
            CreateMap<CaptchaSettingsModel, CaptchaSettings>()
                .ForMember(settings => settings.AutomaticallyChooseLanguage, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaDefaultLanguage, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaRequestTimeout, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaTheme, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaApiUrl, options => options.Ignore());
        }

        /// <summary>
        /// Create SEO maps 
        /// </summary>
        protected virtual void CreateSeoMaps()
        {
            CreateMap<UrlRecord, UrlRecordModel>()
                .ForMember(model => model.DetailsUrl, options => options.Ignore())
                .ForMember(model => model.Language, options => options.Ignore())
                .ForMember(model => model.Name, options => options.Ignore());
            CreateMap<UrlRecordModel, UrlRecord>()
                .ForMember(entity => entity.LanguageId, options => options.Ignore())
                .ForMember(entity => entity.Slug, options => options.Ignore());
        }

        
        /// <summary>
        /// Create stores maps 
        /// </summary>
        protected virtual void CreateStoresMaps()
        {
            CreateMap<Store, StoreModel>()
                .ForMember(model => model.AvailableLanguages, options => options.Ignore());
            CreateMap<StoreModel, Store>();
        }

        /// <summary>
        /// Create tasks maps 
        /// </summary>
        protected virtual void CreateTasksMaps()
        {
            CreateMap<ScheduleTask, ScheduleTaskModel>();
            CreateMap<ScheduleTaskModel, ScheduleTask>()
                .ForMember(entity => entity.Type, options => options.Ignore())
                .ForMember(entity => entity.LastStartUtc, options => options.Ignore())
                .ForMember(entity => entity.LastEndUtc, options => options.Ignore())
                .ForMember(entity => entity.LastSuccessUtc, options => options.Ignore())
                .ForMember(entity => entity.LastEnabledUtc, options => options.Ignore());
        }

        

        /// <summary>
        /// Create topics maps 
        /// </summary>
        protected virtual void CreateTopicsMaps()
        {
            CreateMap<Topic, TopicModel>()
                .ForMember(model => model.AvailableTopicTemplates, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore())
                .ForMember(model => model.TopicName, options => options.Ignore())
                .ForMember(model => model.Url, options => options.Ignore());
            CreateMap<TopicModel, Topic>();

            CreateMap<TopicTemplate, TopicTemplateModel>();
            CreateMap<TopicTemplateModel, TopicTemplate>();
        }

        
        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;

        #endregion
    }
}