using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;

namespace Nop.Services.Security
{
    /// <summary>
    /// Standard permission provider
    /// </summary>
    public partial class StandardPermissionProvider : IPermissionProvider
    {
        //admin area permissions
        public static readonly PermissionRecord AccessAdminPanel = new() { Name = "Access admin area", SystemName = "AccessAdminPanel", Category = "Standard" };
        public static readonly PermissionRecord ManageCustomers = new() { Name = "Admin area. Manage Users", SystemName = "ManageCustomers", Category = "Customers" };
        public static readonly PermissionRecord ManageMessageTemplates = new() { Name = "Admin area. Manage Message Templates", SystemName = "ManageMessageTemplates", Category = "Content Management" };
        public static readonly PermissionRecord ManageLanguages = new() { Name = "Admin area. Manage Languages", SystemName = "ManageLanguages", Category = "Configuration" };
        public static readonly PermissionRecord ManageSettings = new() { Name = "Admin area. Manage Settings", SystemName = "ManageSettings", Category = "Configuration" };
        public static readonly PermissionRecord ManageActivityLog = new() { Name = "Admin area. Manage Activity Log", SystemName = "ManageActivityLog", Category = "Configuration" };
        public static readonly PermissionRecord ManageAcl = new() { Name = "Admin area. Manage ACL", SystemName = "ManageACL", Category = "Configuration" };
        public static readonly PermissionRecord ManageEmailAccounts = new() { Name = "Admin area. Manage Email Accounts", SystemName = "ManageEmailAccounts", Category = "Configuration" };
        public static readonly PermissionRecord ManageStores = new() { Name = "Admin area. Manage Instances", SystemName = "ManageStores", Category = "Configuration" };
        public static readonly PermissionRecord ManagePlugins = new() { Name = "Admin area. Manage Plugins", SystemName = "ManagePlugins", Category = "Configuration" };
        public static readonly PermissionRecord ManageSystemLog = new() { Name = "Admin area. Manage System Log", SystemName = "ManageSystemLog", Category = "Configuration" };
        public static readonly PermissionRecord ManageMessageQueue = new() { Name = "Admin area. Manage Message Queue", SystemName = "ManageMessageQueue", Category = "Configuration" };
        public static readonly PermissionRecord ManageMaintenance = new() { Name = "Admin area. Manage Maintenance", SystemName = "ManageMaintenance", Category = "Configuration" };
        public static readonly PermissionRecord HtmlEditorManagePictures = new() { Name = "Admin area. HTML Editor. Manage pictures", SystemName = "HtmlEditor.ManagePictures", Category = "Configuration" };
        public static readonly PermissionRecord ManageScheduleTasks = new() { Name = "Admin area. Manage Schedule Tasks", SystemName = "ManageScheduleTasks", Category = "Configuration" };
        public static readonly PermissionRecord ManageAppSettings = new() { Name = "Admin area. Manage App Settings", SystemName = "ManageAppSettings", Category = "Configuration" };

        ////public store permissions
        public static readonly PermissionRecord PublicStoreAllowNavigation = new() { Name = "Public. Allow navigation", SystemName = "PublicStoreAllowNavigation", Category = "PublicStore" };
        public static readonly PermissionRecord AccessProfiling = new() { Name = "Public. Access MiniProfiler results", SystemName = "AccessProfiling", Category = "PublicStore" };

        //Admin custom permissions
        public static readonly PermissionRecord ManageServiceMonitoring = new() { Name = "Admin area. Monitoreo de los servicios", SystemName = "ManageServiceMonitoring", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageServiceDesk = new() { Name = "Admin area. Mesa de servicio", SystemName = "ManageServiceDesk", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageSecurityDisplay = new() { Name = "Admin area. Visualización de la seguridad", SystemName = "ManageSecurityDisplay", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageOnlineReports = new() { Name = "Admin area. Reportes en línea", SystemName = "ManageOnlineReports", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageInformationRepository = new() { Name = "Admin area. Repositorio de información", SystemName = "ManageInformationRepository", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageInformationRepositoryPersonal = new() { Name = "Admin area. Repositorio de información personal", SystemName = "ManageInformationRepositoryPersonal", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageServiceLevel = new() { Name = "Admin area. Niveles de servicio", SystemName = "ManageServiceLevel", Category = "Monitor SAT" };
        public static readonly PermissionRecord ManageControlPanel = new() { Name = "Admin area. Control Panel", SystemName = "ManageControlPanel", Category = "Monitor SAT" };
        public static readonly PermissionRecord AccessApiClient = new() { Name = "API. Client Access", SystemName = "AccessApiClient", Category = "API" };
        public static readonly PermissionRecord AccessApiClientRemedy = new() { Name = "API. Remedy Client", SystemName = "AccessApiClientRemedy", Category = "API" };

        public static readonly PermissionRecord RegisterDirectory = new() { Name = "Repositorio de información. Registro de directorio.", SystemName = "RegisterDirectory", Category = "Monitor SAT" };
        public static readonly PermissionRecord RegisterFile = new() { Name = "Repositorio de información. Registro de archivo.", SystemName = "RegisterFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord DownloadFile = new() { Name = "Repositorio de información. Descarga de archivo.", SystemName = "DownloadFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord PublishFile = new() { Name = "Repositorio de información. Publicación de archivo.", SystemName = "PublishFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord AuthFile = new() { Name = "Repositorio de información. Autorización de archivo.", SystemName = "AuthFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord RejectFile = new() { Name = "Repositorio de información. Rechazo de archivo.", SystemName = "RejectFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord NewVersionFile = new() { Name = "Repositorio de información. Nueva versión de archivo.", SystemName = "NewVersionFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord DeleteDirectory = new() { Name = "Repositorio de información. Eliminación de directorio.", SystemName = "DeleteDirectory", Category = "Monitor SAT" };
        public static readonly PermissionRecord DeleteFile = new() { Name = "Repositorio de información. Eliminación de archivo.", SystemName = "DeleteFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord UpdateDirectory = new() { Name = "Repositorio de información. Modificación de carpeta.", SystemName = "UpdateDirectory", Category = "Monitor SAT" };
        public static readonly PermissionRecord RegisterTagFile = new() { Name = "Repositorio de información. Registro de tag de archivo.", SystemName = "RegisterTagFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord UpdateTagFile = new() { Name = "Repositorio de información. Modificación de tag de archivo.", SystemName = "UpdateTagFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord TraceFile = new() { Name = "Repositorio de información. Seguimiento de archivo.", SystemName = "TraceFile", Category = "Monitor SAT" };
        public static readonly PermissionRecord DeleteTraceFile = new() { Name = "Repositorio de información. Eliminación de seguimiento de archivo.", SystemName = "DeleteTraceFile", Category = "Monitor SAT" };
        
        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel,
                ManageCustomers,
                ManageMessageTemplates,
                ManageLanguages,
                ManageSettings,
                ManageActivityLog,
                ManageAcl,
                ManageEmailAccounts,
                ManageStores,
                ManagePlugins,
                ManageSystemLog,
                ManageMessageQueue,
                ManageMaintenance,
                ManageScheduleTasks,
                PublicStoreAllowNavigation,
                ManageControlPanel,
                ManageServiceMonitoring,
                ManageServiceDesk,
                ManageSecurityDisplay,
                ManageOnlineReports,
                ManageInformationRepository,
                ManageInformationRepositoryPersonal,
                ManageServiceLevel,
                HtmlEditorManagePictures,
                RegisterDirectory,
                RegisterFile,
                DownloadFile,
                PublishFile,
                AuthFile,
                RejectFile,
                NewVersionFile,
                DeleteDirectory,
                DeleteFile,
                UpdateDirectory,
                RegisterTagFile,
                UpdateTagFile,
                TraceFile,
                DeleteTraceFile,
                AccessApiClient,
                AccessApiClientRemedy
            };
        }

        /// <summary>
        /// Get default permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.SuperAdminRoleName,
                    new[]
                    {
                        AccessAdminPanel,
                        ManageCustomers,
                        ManageMessageTemplates,
                        ManageLanguages,
                        ManageSettings,
                        ManageActivityLog,
                        ManageAcl,
                        ManageEmailAccounts,
                        ManageStores,
                        ManagePlugins,
                        ManageSystemLog,
                        ManageMessageQueue,
                        ManageMaintenance,
                        ManageScheduleTasks,
                        ManageAppSettings,
                        ManageServiceMonitoring,
                        ManageServiceDesk,
                        ManageSecurityDisplay,
                        ManageOnlineReports,
                        ManageInformationRepository,
                        ManageInformationRepositoryPersonal,
                        ManageServiceLevel,
                        AccessApiClient,
                        AccessApiClientRemedy,
                        ManageControlPanel
                    }
                ),       
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        AccessAdminPanel,
                        ManageControlPanel,
                        ManageServiceDesk,
                        ManageServiceMonitoring,
                        ManageOnlineReports,
                        ManageServiceLevel,
                        ManageActivityLog,
                        ManageCustomers
                    }
                ),
                (
                    NopCustomerDefaults.UserRoleName,
                    new[]
                    {
                        AccessAdminPanel,
                        ManageServiceMonitoring,
                        ManageOnlineReports,
                        ManageInformationRepository,
                        ManageInformationRepositoryPersonal
                    }
                ),
                (
                    NopCustomerDefaults.MonitoreoRoleName,
                    new[]
                    {
                        AccessAdminPanel,
                        ManageServiceMonitoring,
                        ManageOnlineReports
                    }
                ),
                (
                    NopCustomerDefaults.BackgroundRoleName,
                    new[]
                    {
                        PublicStoreAllowNavigation
                    }
                ),
                (
                    NopCustomerDefaults.ReaderRoleName,
                    new[]
                    {
                        PublicStoreAllowNavigation,
                        ManageInformationRepository,
                        ManageInformationRepositoryPersonal
                    }
                ),
                (
                    NopCustomerDefaults.AuthorRoleName,
                    new[]
                    {
                        RegisterFile,
                        DeleteFile,
                        PublishFile,
                        NewVersionFile,
                        RegisterDirectory,
                        UpdateDirectory,
                        DeleteDirectory,
                        RegisterTagFile,
                        UpdateTagFile,
                        DownloadFile,
                        TraceFile,
                        DeleteTraceFile,
                        ManageInformationRepository,
                        ManageInformationRepositoryPersonal
                    }
                ),
                (
                    NopCustomerDefaults.ApproverRoleName,
                    new[]
                    {
                        RejectFile,
                        AuthFile,
                        ManageInformationRepository            ,
                        ManageInformationRepositoryPersonal
                    }
                ),(
                    NopCustomerDefaults.ApiRoleName,
                    new[]
                    {
                        AccessApiClient
                    }
                  )
            };
        }
    }
}