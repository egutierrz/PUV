using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.SatRepositoryInfo
{
    public partial class SatRepositoryFilesCustomers : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the File identifier
        /// </summary>
        public string Guid { get; set; }

    }
}
