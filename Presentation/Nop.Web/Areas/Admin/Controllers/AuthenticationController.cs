using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.ExternalAuthentication;
using Nop.Web.Areas.Admin.Models.MultiFactorAuthentication;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class AuthenticationController : BaseAdminController
    {
        #region Fields

        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;

        #endregion

        #region Ctor

        public AuthenticationController(ExternalAuthenticationSettings externalAuthenticationSettings,
            IEventPublisher eventPublisher,
            IPermissionService permissionService,
            ISettingService settingService,
            MultiFactorAuthenticationSettings multiFactorAuthenticationSettings)
        {
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _eventPublisher = eventPublisher;
            _permissionService = permissionService;
            _settingService = settingService;
            _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
        }

        #endregion


    }
}