using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.InformationRepository
{
    /// <summary>
    /// Represents a customer role search model
    /// </summary>
    public partial record FileVersionInfoModel : BaseNopModel
    {


        #region Properties

        public int Version { get; set; }
        public int FileId { get; set; }
        public string Status { get; set; }

        #endregion

    }
}