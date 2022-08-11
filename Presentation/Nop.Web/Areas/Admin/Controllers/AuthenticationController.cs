using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
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
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExternalAuthenticationMethodModelFactory _externalAuthenticationMethodModelFactory;
        private readonly IMultiFactorAuthenticationMethodModelFactory _multiFactorAuthenticationMethodModelFactory;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;

        #endregion

        #region Ctor

        public AuthenticationController(ExternalAuthenticationSettings externalAuthenticationSettings,
            IAuthenticationPluginManager authenticationPluginManager,
            IEventPublisher eventPublisher,
            IExternalAuthenticationMethodModelFactory externalAuthenticationMethodModelFactory,
            IMultiFactorAuthenticationMethodModelFactory multiFactorAuthenticationMethodModelFactory,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            IPermissionService permissionService,
            ISettingService settingService,
            MultiFactorAuthenticationSettings multiFactorAuthenticationSettings)
        {
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _authenticationPluginManager = authenticationPluginManager;
            _eventPublisher = eventPublisher;
            _externalAuthenticationMethodModelFactory = externalAuthenticationMethodModelFactory;
            _multiFactorAuthenticationMethodModelFactory = multiFactorAuthenticationMethodModelFactory;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _permissionService = permissionService;
            _settingService = settingService;
            _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
        }

        #endregion


    }
}