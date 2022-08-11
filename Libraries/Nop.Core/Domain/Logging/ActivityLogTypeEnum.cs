using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Logging
{
    public enum ActivityLogTypeEnum
    {
        [Description("PB - Acceso al sistema")]
        PB_SystemAccess,
        [Description("PB - Creación de usuario")]
        PB_UserCreation,
        [Description("PB - Cambio de password")]
        PB_PasswordChange,
        [Description("PB - Recuperación de password")]
        PB_PasswordRecovery,
        [Description("PB - Modificación de datos de usuario")]
        PB_ProfileUpdate,

        [Description("RI - Ingreso al módulo de Repositorio de Información")]
        RI_ModuleEntry,
        [Description("RI - Creación de directorio")]
        RI_DirectoryCreation,
        [Description("RI - Creación de archivo")]
        RI_FileCreation,
        [Description("RI - Descarga de archivo")]
        RI_FileDownload,
        [Description("RI - Publicación de archivo")]
        RI_FilePublication,
        [Description("RI - Autorización de archivo")]
        RI_FileAuthorization,
        [Description("RI - Rechazo de archivo")]
        RI_FileRejection,
        [Description("RI - Nueva versión de archivo")]
        RI_NewFileVersion,
        [Description("RI - Eliminación de directorio")]
        RI_DirectoryDeletion,
        [Description("RI - Eliminación de archivo")]
        RI_FileDeletion,
        [Description("RI - Modificación de carpeta")]
        RI_FolderModification,
        [Description("RI - Creación de tag de archivo")]
        RI_FileTagCreation,
        [Description("RI - Seguimiento de archivo")]
        RI_FileTracking,
        [Description("RI - Modificación de tag de archivo")]
        RI_FileTagModification,
        [Description("HD. Intento llamado a Remedy")]
        HD_TryRemedy,
        [Description("HD. Sin permiso a Remedy")]
        HD_FailedPermissionRemedy,
        [Description("HD. Registró en Remedy")]
        HD_RemedyRegister
    }

    public static class EnumExtensionMethods
    {
        public static string GetEnumDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }
    }
}
