using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.HelpDesk;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data.Extensions;


namespace Nop.Data.Migrations.Installation
{
    [NopMigration("2020/01/31 11:24:16:2551771", "Nop.Data base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// <remarks>
        /// We use an explicit table creation order instead of an automatic one
        /// due to problems creating relationships between tables
        /// </remarks>
        /// </summary>
        public override void Up()
        {
            Create.TableFor<AddressAttribute>();
            Create.TableFor<AddressAttributeValue>();
            Create.TableFor<Country>();
            Create.TableFor<Currency>();
            Create.TableFor<MeasureDimension>();
            Create.TableFor<MeasureWeight>();
            Create.TableFor<StateProvince>();
            Create.TableFor<Address>();
            Create.TableFor<CustomerAttribute>();
            Create.TableFor<CustomerAttributeValue>();
            Create.TableFor<Customer>();
            Create.TableFor<CustomerPassword>();
            Create.TableFor<CustomerAddressMapping>();
            Create.TableFor<CustomerRole>();
            Create.TableFor<CustomerCustomerRoleMapping>();
            Create.TableFor<ExternalAuthenticationRecord>();
            Create.TableFor<Store>();
            Create.TableFor<StoreMapping>();
            Create.TableFor<Language>();
            Create.TableFor<LocaleStringResource>();
            Create.TableFor<LocalizedProperty>();
            Create.TableFor<Setting>();
            Create.TableFor<GdprConsent>();
            Create.TableFor<GdprLog>();
            Create.TableFor<ActivityLogType>();
            Create.TableFor<ActivityLog>();
            Create.TableFor<Log>();
            Create.TableFor<Campaign>();
            Create.TableFor<EmailAccount>();
            Create.TableFor<MessageTemplate>();
            Create.TableFor<NewsLetterSubscription>();
            Create.TableFor<QueuedEmail>();
            Create.TableFor<AclRecord>();
            Create.TableFor<PermissionRecord>();
            Create.TableFor<PermissionRecordCustomerRoleMapping>();
            Create.TableFor<ScheduleTask>();
            Create.TableFor<SatRepositoryInfoDirectories>();
            Create.TableFor<SatRepositoryInfoFiles>();
            Create.TableFor<SatRepositoryInfoBinaryFiles>();
            Create.TableFor<RemedyRegister>();
            Create.TableFor<SatRepositoryFilesCustomers>();
            Create.TableFor<ComponentSetting>();
        }
    }
}
