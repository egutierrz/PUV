using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Framework.Factories;
using Nop.Web.Infrastructure.Installation;
using Sat.Cisco.SD_WAN.Device;
using Sat.Cisco.SD_WAN.Security;
using Sat.Cisco.SD_WAN.Network;
using Sat.Cisco.SD_WAN.Device;
using Sat.Cisco.SD_WAN.ClusterManagement;
using Sat.Cisco.SD_WAN.Certificate;
using Sat.Cisco.SD_WAN.Statistics;
using Sat.Cisco.SD_WAN.Detail;

namespace Nop.Web.Infrastructure
{
    /// <summary>
    /// Represents the registering services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //installation localization service
            services.AddScoped<IInstallationLocalizationService, InstallationLocalizationService>();

            //common factories
            services.AddScoped<IAclSupportedModelFactory, AclSupportedModelFactory>();
            services.AddScoped<ILocalizedModelFactory, LocalizedModelFactory>();
            services.AddScoped<IStoreMappingSupportedModelFactory, StoreMappingSupportedModelFactory>();

            //admin factories
            services.AddScoped<IBaseAdminModelFactory, BaseAdminModelFactory>();
            services.AddScoped<IActivityLogModelFactory, ActivityLogModelFactory>();
            services.AddScoped<IAddressModelFactory, AddressModelFactory>();
            services.AddScoped<IAddressAttributeModelFactory, AddressAttributeModelFactory>();
            services.AddScoped<ICommonModelFactory, CommonModelFactory>();
            services.AddScoped<ICountryModelFactory, CountryModelFactory>();
            services.AddScoped<ICurrencyModelFactory, CurrencyModelFactory>();
            services.AddScoped<ICustomerAttributeModelFactory, CustomerAttributeModelFactory>();
            services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
            services.AddScoped<ICustomerRoleModelFactory, CustomerRoleModelFactory>();
            services.AddScoped<IEmailAccountModelFactory, EmailAccountModelFactory>();
            services.AddScoped<IExternalAuthenticationMethodModelFactory, ExternalAuthenticationMethodModelFactory>();
            services.AddScoped<IHomeModelFactory, HomeModelFactory>();
            services.AddScoped<ILanguageModelFactory, LanguageModelFactory>();
            services.AddScoped<ILogModelFactory, LogModelFactory>();
            services.AddScoped<IMessageTemplateModelFactory, MessageTemplateModelFactory>();
            services.AddScoped<IMultiFactorAuthenticationMethodModelFactory, MultiFactorAuthenticationMethodModelFactory>();
            services.AddScoped<IPluginModelFactory, PluginModelFactory>();
            services.AddScoped<IQueuedEmailModelFactory, QueuedEmailModelFactory>();
            services.AddScoped<IScheduleTaskModelFactory, ScheduleTaskModelFactory>();
            services.AddScoped<ISecurityModelFactory, SecurityModelFactory>();
            services.AddScoped<ISettingModelFactory, SettingModelFactory>();
            services.AddScoped<IComponentSettingModelFactory, ComponentSettingModelFactory>();
            services.AddScoped<IStoreModelFactory, StoreModelFactory>();
            services.AddScoped<ITemplateModelFactory, TemplateModelFactory>();
            services.AddScoped<ITopicModelFactory, TopicModelFactory>();
            services.AddScoped<IWidgetModelFactory, WidgetModelFactory>();
            services.AddScoped<IServiceDeskModelFactory, ServiceDeskModelFactory>();

            services.AddScoped<IInformationRepositoryModelFactory, InformationRepositoryModelFactory>();

            services.AddScoped<IInformationRepositoryModelFactory, InformationRepositoryModelFactory>();

            //factories
            services.AddScoped<Factories.IAddressModelFactory, Factories.AddressModelFactory>();
            services.AddScoped<Factories.ICommonModelFactory, Factories.CommonModelFactory>();
            services.AddScoped<Factories.ICountryModelFactory, Factories.CountryModelFactory>();
            services.AddScoped<Factories.ICustomerModelFactory, Factories.CustomerModelFactory>();
            services.AddScoped<Factories.IExternalAuthenticationModelFactory, Factories.ExternalAuthenticationModelFactory>();
            services.AddScoped<Factories.IProfileModelFactory, Factories.ProfileModelFactory>();
            services.AddScoped<Factories.ITopicModelFactory, Factories.TopicModelFactory>();
            services.AddScoped<Factories.IWidgetModelFactory, Factories.WidgetModelFactory>();

            //helpers classes
            services.AddScoped<ITinyMceHelper, TinyMceHelper>();

            //CISCO
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IConnectionsService, ConnectionsService>();
            services.AddScoped<IServiceMonitoringModelFactory, ServiceMonitoringModelFactory>();
            services.AddScoped<IVEdgeInventory, VEdgeInventory>();
            services.AddScoped<IBfd, Bfd>();
            services.AddScoped<IHealth, Health>();
            services.AddScoped<IIssues, Issues>();
            services.AddScoped<IStats, Stats>();
            services.AddScoped<IHardwareHealth, HardwareHealth>();
            services.AddScoped<IControl, Control>();
            services.AddScoped<IApproute, Approute>();
            services.AddScoped<IInterface, Interface>();
            services.AddScoped<IDetail, Detail>();

            services.AddScoped<IDevice, Device>();
            services.AddScoped<IControlPanelModelFactory, ControlPanelModelFactory>();
            services.AddScoped<IAlarms, Alarms>();
            services.AddScoped<IEvents, Events>();

        }

        // <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 2002;
    }
}
