using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.SatRepositoryInfo
{
    public partial class SatRepositoryInfoDirectories : BaseEntity
    {

        /// <summary>
        /// Gets or sets the directory name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
        public DateTime? UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the parent directory identifier
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the directory has been deleted
        /// </summary>
        public bool Deleted { get; set; }
    }


}
