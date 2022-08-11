using System.Data;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Core.Domain.Customers;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.SatRepositoryInfo
{
    public partial class SatRepositoryInfoDirectoryBuilder : NopEntityBuilder<SatRepositoryInfoDirectories>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SatRepositoryInfoDirectories.Name)).AsString(400).Nullable()
                .WithColumn(nameof(SatRepositoryInfoDirectories.CreatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(SatRepositoryInfoDirectories.UpdatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(SatRepositoryInfoDirectories), nameof(SatRepositoryInfoDirectories.OwnerUserId))).AsInt32().ForeignKey<Customer>(onDelete: Rule.None).Nullable();
        }

        #endregion
    }
}
