using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Security;
using Nop.Services.Customers;
using Nop.Services.Security;

namespace Nop.Api.Controllers
{
    public class PermissionsController : Controller
    {
        protected readonly IPermissionService _permissionService;
        protected readonly ICustomerService _customerService;
        public PermissionsController(IPermissionService permissionService,
            ICustomerService customerService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
        }

        [Helpers.Authorize]
        [HttpGet]
        [Route("/api/permissions/list")]
        public async Task<IActionResult> ListPermissions()
        {

            var customerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            List<PermissionRecordCustomerRoleMapping> listPermission = new List<PermissionRecordCustomerRoleMapping>();

            foreach (var role in customerRoles)
            {
                IList<PermissionRecordCustomerRoleMapping> permission = await _permissionService.GetMappingByCustomerRoleIdAsync(role.Id);
                listPermission.AddRange(permission);
            }

            return Ok(new { 
                Permission = listPermission,
                Roles = customerRoles
            });
        }
    }
}
