using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Web.Controllers
{
    //do not inherit it from BasePublicController. otherwise a lot of extra action filters will be called
    //they can create guest account(s), etc
    public partial class BackwardCompatibility2XController : Controller
    {
        #region Fields

        #endregion

        #region Ctor

        public BackwardCompatibility2XController()
        {
        }

        #endregion

        #region Methods

    
        #endregion
    }
}