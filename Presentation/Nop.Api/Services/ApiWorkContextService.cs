using Nop.Core.Domain.Customers;

namespace Nop.Api.Services
{
    public interface IApiWorkContextService
    {
        Customer GetCurrentCustomer();
    }
    public class ApiWorkContextService : IApiWorkContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiWorkContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public virtual Customer GetCurrentCustomer()
        {
            Customer customer = new Customer();
            if(_httpContextAccessor.HttpContext != null)
            {
                var user = _httpContextAccessor.HttpContext.Items["User"];
                customer = user != null ? (Customer)user : new Customer();
            }
            return customer;
        }
    }
}
