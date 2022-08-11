using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.SatRepositoryInfo
{
    public partial class SatRepositoryInfoBinaryFiles : BaseEntity
    {

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// Gets or sets the directory name
        /// </summary>
        public byte[] BinaryFile { get; set; }


    }

}
