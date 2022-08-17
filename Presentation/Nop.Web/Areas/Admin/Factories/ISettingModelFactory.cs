using System.Threading.Tasks;
using Nop.Core.Domain.Gdpr;
using Nop.Web.Areas.Admin.Models.Settings;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the setting model factory
    /// </summary>
    public partial interface ISettingModelFactory
    {
        /// <summary>
        /// Prepare app settings model
        /// </summary>
        /// <param name="model">AppSettings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the app settings model
        /// </returns>
        Task<AppSettingsModel> PrepareAppSettingsModel(AppSettingsModel model = null);
        

        /// <summary>
        /// Prepare customer user settings model
        /// </summary>
        /// <param name="model">Customer user settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer user settings model
        /// </returns>
        Task<CustomerUserSettingsModel> PrepareCustomerUserSettingsModelAsync(CustomerUserSettingsModel model = null);

        /// <summary>
        /// Prepare GDPR settings model
        /// </summary>
        /// <param name="model">Gdpr settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR settings model
        /// </returns>
        Task<GdprSettingsModel> PrepareGdprSettingsModelAsync(GdprSettingsModel model = null);

        /// <summary>
        /// Prepare paged GDPR consent list model
        /// </summary>
        /// <param name="searchModel">GDPR search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent list model
        /// </returns>
        Task<GdprConsentListModel> PrepareGdprConsentListModelAsync(GdprConsentSearchModel searchModel);

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
        Task<GdprConsentModel> PrepareGdprConsentModelAsync(GdprConsentModel model, GdprConsent gdprConsent, bool excludeProperties = false);

        /// <summary>
        /// Prepare general and common settings model
        /// </summary>
        /// <param name="model">General common settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the general and common settings model
        /// </returns>
        Task<GeneralCommonSettingsModel> PrepareGeneralCommonSettingsModelAsync(GeneralCommonSettingsModel model = null);

        
        /// <summary>
        /// Prepare setting search model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting search model
        /// </returns>
        Task<SettingSearchModel> PrepareSettingSearchModelAsync(SettingSearchModel searchModel);

        /// <summary>
        /// Prepare paged setting list model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting list model
        /// </returns>
        Task<SettingListModel> PrepareSettingListModelAsync(SettingSearchModel searchModel);

        /// <summary>
        /// Prepare setting mode model
        /// </summary>
        /// <param name="modeName">Mode name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting mode model
        /// </returns>
        Task<SettingModeModel> PrepareSettingModeModelAsync(string modeName);

        /// <summary>
        /// Prepare store scope configuration model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store scope configuration model
        /// </returns>
        Task<StoreScopeConfigurationModel> PrepareStoreScopeConfigurationModelAsync();
    }
}