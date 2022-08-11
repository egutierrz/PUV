using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.InformationRepository
{
    /// <summary>
    /// Represents a language model
    /// </summary>
    public partial record TreeFolderModel : BaseNopEntityModel
    {
        #region Ctor

        public TreeFolderModel()
        {
            Parent = new List<ParentModel>();
        }

        #endregion

        #region Properties

        public IList<ParentModel> Parent { get; set; }

        #endregion
        public partial record ParentModel : BaseNopEntityModel
        {
            public ParentModel()
            {
                nodes = new List<NodeModel>();
                state = new StateModel();
                selectable = true;
                text = "";
            }
            public string text { get; set; }
            public StateModel state { get; set; }
            public bool selectable { get; set; }
            public IList<NodeModel> nodes { get; set; }
        }

        public partial record StateModel
        {
            public StateModel()
            {
                expanded = true;
                disabled = false;
                selected = false;
            }
            public bool expanded { get; set; }
            public bool disabled { get; set; }
            public bool selected { get; set; }
        }

        public partial record NodeModel : BaseNopEntityModel
        {
            public NodeModel()
            {
                nodes = new List<NodeModel>();
            }
            public string text { get; set; }
            public IList<NodeModel> nodes { get; set; }
        }
    }
}