using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.InformationRepository
{
    /// <summary>
    /// Represents a language model
    /// </summary>
    public partial record InfoRepositoryFolderModel : BaseNopEntityModel
    {
        #region Ctor

        public InfoRepositoryFolderModel()
        {
            IdParent = 0;
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Informationrepository.CreateFolder.Fields.Name")]
        public int IdParent { get; set; }

        [NopResourceDisplayName("Admin.Informationrepository.CreateFolder.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Informationrepository.CreateFolder.Fields.Name")]
        public string NameUpdate { get; set; }

        #endregion
    }
}