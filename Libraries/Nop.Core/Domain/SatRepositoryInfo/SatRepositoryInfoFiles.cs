using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.SatRepositoryInfo
{
    public partial class SatRepositoryInfoFiles : BaseEntity
    {
        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string GuidFile { get; set; }

        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file extension
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the file size
        /// </summary>
        public decimal Size { get; set; }

        /// <summary>
        /// Gets or sets the checksum
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Gets or sets the tags
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the directory identifier
        /// </summary>
        public int DirectoryId { get; set; }

        /// <summary>
        /// Gets or sets the owner user identifier
        /// </summary>
        public int OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the Updated User identifier
        /// </summary>
        public int? UpdatedUserId { get; set; }

        /// <summary>
        /// Gets or sets the Deleted User identifier
        /// </summary>
        public int? DeletedUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
        public DateTime? UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity delete
        /// </summary>
        public DateTime? DeletedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the file version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the binary file identifier
        /// </summary>
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has been published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the status identifier
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has been deleted
        /// </summary>
        public bool Deleted { get; set; }

    }

}
