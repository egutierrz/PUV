using System;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Security;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeTo440
{
    [NopMigration("2020-06-10 00:00:00", "4.40.0", UpdateMigrationType.Data, MigrationProcessType.Update)]
    public class DataMigration : Migration
    {
        private readonly INopDataProvider _dataProvider;

        public DataMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            // new permission
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "AccessProfiling", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var profilingPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Public store. Access MiniProfiler results",
                        SystemName = "AccessProfiling",
                        Category = "PublicStore"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.SuperAdminRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = profilingPermission.Id
                    }
                );
            }

            var activityLogTypeTable = _dataProvider.GetTable<ActivityLogType>();

            //Plataforma Base            
            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.PB_SystemAccess.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.PB_SystemAccess.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.PB_SystemAccess.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.PB_PasswordChange.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.PB_PasswordChange.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.PB_PasswordChange.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.PB_PasswordRecovery.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.PB_PasswordRecovery.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.PB_PasswordRecovery.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.PB_ProfileUpdate.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.PB_ProfileUpdate.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.PB_ProfileUpdate.GetEnumDescription()
                    }
                );

            //Repositorio de Información

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_ModuleEntry.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_ModuleEntry.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_ModuleEntry.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_DirectoryCreation.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_DirectoryCreation.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_DirectoryCreation.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileDownload.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileDownload.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileDownload.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FilePublication.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FilePublication.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FilePublication.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileAuthorization.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileAuthorization.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileAuthorization.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileRejection.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileRejection.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileRejection.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_NewFileVersion.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_NewFileVersion.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_NewFileVersion.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_DirectoryDeletion.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_DirectoryDeletion.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_DirectoryDeletion.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileDeletion.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileDeletion.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileDeletion.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FolderModification.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FolderModification.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FolderModification.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileTagCreation.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileTagCreation.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileTagCreation.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileTracking.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileTracking.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileTracking.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.RI_FileTagModification.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.RI_FileTagModification.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.RI_FileTagModification.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.HD_TryRemedy.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.HD_TryRemedy.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.HD_TryRemedy.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.HD_FailedPermissionRemedy.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.HD_FailedPermissionRemedy.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.HD_FailedPermissionRemedy.GetEnumDescription()
                    }
                );

            if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, ActivityLogTypeEnum.HD_RemedyRegister.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
                _dataProvider.InsertEntity(
                    new ActivityLogType
                    {
                        SystemKeyword = ActivityLogTypeEnum.HD_RemedyRegister.ToString(),
                        Enabled = true,
                        Name = ActivityLogTypeEnum.HD_RemedyRegister.GetEnumDescription()
                    }
                );
            //<MFA #475>
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageMultifactorAuthenticationMethods", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multiFactorAuthenticationPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Multi-factor Authentication Methods",
                        SystemName = "ManageMultifactorAuthenticationMethods",
                        Category = "Configuration"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.SuperAdminRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );
            }
            //</MFA #475>

            //issue-3852
            var tableName = nameof(RewardPointsHistory);
            var rph = Schema.Table(tableName);
            var columnName = "UsedWithOrder_Id";

            if (rph.Column(columnName).Exists())
            {
                var constraintName = "RewardPointsHistory_UsedWithOrder";

                if (rph.Constraint(constraintName).Exists())
                    Delete.UniqueConstraint(constraintName).FromTable(tableName);

                Delete.Column(columnName).FromTable(tableName);
            }
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
