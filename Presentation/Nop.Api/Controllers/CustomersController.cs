using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        protected readonly ILocalizationService _localizationService;
        protected readonly ILanguageService _languageService;
        protected readonly ICustomerRegistrationService _customerRegistrationService;
        protected readonly ICustomerService _customerService;
        protected readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;

        private IConfiguration Configuration { get; }
        private CustomerSettings CustomerSettings { get; }

        public CustomersController(
            ILocalizationService localizationService,
            ILanguageService languageService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IPermissionService permissionService,
            IConfiguration configuration,
            CustomerSettings customerSettings,
            IStoreService storeService)
        {
            _localizationService = localizationService;
            _languageService = languageService;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _permissionService = permissionService;
            Configuration = configuration;
            CustomerSettings = customerSettings;
            _storeService = storeService;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("/api/customers/login")]

        public async Task<IActionResult> CustomerLoginAsync(string username, string password, int instanceid, int? lid = null)
        {
            if (lid == null)
            {
                var getlang = await _languageService.GetAllLanguagesAsync();
                var lang = getlang.FirstOrDefault();
                lid = lang != null ? lang.Id :null;
            }
            var store = await _storeService.GetStoreByIdAsync(instanceid);
            if(store == null)
            {
                return BadRequest(new
                {
                    Success = "-1",
                    Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials", Convert.ToInt32(lid), false)
                });
            }
              
            if (CustomerSettings.UsernamesEnabled && username != null)
            {
                username = username.Trim();
            }
            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(username, password);
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = CustomerSettings.UsernamesEnabled
                            ? await _customerService.GetCustomerByUsernameAsync(username)
                            : await _customerService.GetCustomerByEmailAsync(username);                        

                        var roles = await _customerService.GetCustomerRolesAsync(customer);
                        var name = await _customerService.GetCustomerFullNameAsync(customer);

                        return Ok(new
                        {
                            Success = "0",
                            Message = await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login", Convert.ToInt32(lid), false),
                            CustomerId = customer.Id,
                            UserName = customer.Username,
                            Role = roles,
                            Name = name,
                            Token = GenerateJwtToken(customer, instanceid)
                    });
                    }
                case CustomerLoginResults.CustomerNotExist:
                    return Unauthorized(new
                    {
                        Success = "-1",
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist", Convert.ToInt32(lid), false)
                    });
                case CustomerLoginResults.Deleted:
                    return Unauthorized(new
                    {
                        Success = "-1",
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted", Convert.ToInt32(lid), false)
                    });

                case CustomerLoginResults.NotActive:
                    return Unauthorized(new
                    {
                        Success = "-1",
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive", Convert.ToInt32(lid), false)
                    });
                case CustomerLoginResults.NotRegistered:
                    return NotFound(new
                    {
                        Success = "-1",
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered", Convert.ToInt32(lid), false)
                    });
                case CustomerLoginResults.LockedOut:
                    return Unauthorized(new
                    {
                        Success = "-1",
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut", Convert.ToInt32(lid), false)
                    });
                case CustomerLoginResults.WrongPassword:
                default:
                    return Unauthorized(new
                    {
                        Success = "-1",
                        Message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials", Convert.ToInt32(lid), false)
                    });
            }
        }

        [NonAction]
        private string GenerateJwtToken(Customer user, int storeId)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings").GetSection("Secret").Value);
            // generate token that is valid for 7 days
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("id", user.Id.ToString()),
                    new Claim("storeid", storeId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
    
}
