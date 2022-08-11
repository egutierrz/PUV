using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.ServiceDesk
{
    /// <summary>
    /// Represents an service desk list model
    /// </summary>
    public partial record ServiceDeskListModel : BasePagedListModel<ServiceDeskModel>
    {
    }
}