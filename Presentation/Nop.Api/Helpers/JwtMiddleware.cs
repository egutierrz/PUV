using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Security;

namespace Nop.Api.Helpers
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        public IConfiguration Configuration { get; }
        public JwtMiddleware(RequestDelegate next, IConfiguration configuration,
            ICustomerService customerService, IPermissionService permissionService, IWorkContext workContext)
        {
            _next = next;
            Configuration = configuration;
            _customerService = customerService;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        public async Task Invoke(HttpContext context)
        {
         var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachUserToContextAsync(context, token);

            await _next(context);
        }

        private async Task AttachUserToContextAsync(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings").GetSection("Secret").Value);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                Customer customer = await _customerService.GetCustomerByIdAsync(userId);
                await _workContext.SetCurrentCustomerAsync(customer);
                bool apiaccesss = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessApiClient);
                // attach user to context on successful jwt validation
                context.Items["User"] = customer;
                context.Items["ApiAccess"] = apiaccesss;
                
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
