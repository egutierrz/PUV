using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.HelpDesk;
using Nop.Services.HelpDesk;
using Nop.Api.Dto.HelpDesk;
using Nop.Services.Security;
using Nop.Core;
using Nop.Api.Services;
using Nop.Core.Domain.Customers;
using Nop.Services.Logging;
using Nop.Core.Domain.Logging;
using Nop.Services.Customers;

namespace Nop.Api.Controllers
{
    public class HelpDeskController : Controller
    {
        #region Fields

        protected readonly IHelpDeskService _helpDeskService;
        protected readonly IPermissionService _permissionService;
        private readonly IApiWorkContextService _apiWorkContextService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor
        public HelpDeskController(IHelpDeskService helpDeskService, 
            IPermissionService permissionService,
            IApiWorkContextService apiWorkContextService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService)
        {
            _helpDeskService = helpDeskService;
            _permissionService = permissionService;
            _apiWorkContextService = apiWorkContextService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        [Helpers.Authorize]
        [HttpPost]
        [Route("/api/helpDesk/remedyRegister")]
        public async Task<IActionResult> RemedyRegister(RemedyRegisterDto remedyRegisterDto)
        {
            Customer customer = _apiWorkContextService.GetCurrentCustomer();
            await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.HD_TryRemedy.ToString(),
                "HD. Intento llamado a Remedy", customer);
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessApiClientRemedy, customer))
            {
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.HD_FailedPermissionRemedy.ToString(),
                "HD. Sin permiso a Remedy", customer);
                return Forbid();
            }

            
            await _helpDeskService.InsertRemedyRegisterAsync(remedyRegisterDto.GetRemedyRegisterDomain(remedyRegisterDto));
            await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.HD_RemedyRegister.ToString(),
                "HD. Registró en Remedy", customer);
            return Accepted();
        }

        [HttpGet]
        [Route("/api/helpDesk/remedyRegister")]
        public async Task<IActionResult> RemedyRegisterGet(
            string ticketNumber, 
            string satNumber, 
            string reportType,
            string faultEventType,
            string faultEventCause,
            string status,
            String eventDate,
            string userName
            )
        {
            Customer customer = await _customerService.GetCustomerByUsernameAsync(userName);
            if(customer == null)
                return Forbid();

            RemedyRegisterDto remedyRegisterDto = new RemedyRegisterDto()
            {
                TicketNumber = ticketNumber,
                SATNumber = satNumber,
                ReportType = reportType,
                FaultEventCause = faultEventCause,
                FaultEventType  = faultEventType,
                Status  = status,
                EventDate = eventDate,
            };

            await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.HD_TryRemedy.ToString(),
                "HD. Intento llamado a Remedy", customer);
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessApiClientRemedy, customer))
            {
                await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.HD_FailedPermissionRemedy.ToString(),
                "HD. Sin permiso a Remedy", customer);
                return Forbid();
            }


            await _helpDeskService.InsertRemedyRegisterAsync(remedyRegisterDto.GetRemedyRegisterDomain(remedyRegisterDto));
            await _customerActivityService.InsertActivityAsync(customer, ActivityLogTypeEnum.HD_RemedyRegister.ToString(),
                "HD. Registró en Remedy", customer);
            return Accepted();
        }

        #endregion
    }
}
