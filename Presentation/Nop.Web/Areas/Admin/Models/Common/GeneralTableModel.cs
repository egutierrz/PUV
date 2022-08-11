using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents an address attribute model
    /// </summary>
    public partial record GeneralTableModel
    {
        #region Ctor

        public GeneralTableModel()
        {
            dataSrc = new List<string[]>();
            headersList = new List<HeaderModel>();
        }

        #endregion

        #region Properties

        public string Token { set; get; }
        public string JSessionId { set; get; }
        public List<string[]> dataSrc { get; set; }

        public IList<HeaderModel> headersList { get; set; }

        #endregion
    }

    public partial record HeaderModel
    {
        public string title { get; set; }
    }
}