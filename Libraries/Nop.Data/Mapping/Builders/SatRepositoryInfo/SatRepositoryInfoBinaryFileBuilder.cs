using System.Data;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Core.Domain.Customers;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.SatRepositoryInfo
{
    public partial class SatRepositoryInfoBinaryFileBuilder : NopEntityBuilder<SatRepositoryInfoBinaryFiles>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SatRepositoryInfoBinaryFiles.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(SatRepositoryInfoBinaryFiles.BinaryFile)).AsBinary(int.MaxValue).NotNullable();
            
        }

        #endregion
    }
}
