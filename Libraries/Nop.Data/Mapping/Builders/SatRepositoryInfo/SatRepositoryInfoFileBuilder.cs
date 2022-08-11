using System.Data;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Core.Domain.Customers;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.SatRepositoryInfo
{
    public partial class SatRepositoryInfoFileBuilder : NopEntityBuilder<SatRepositoryInfoFiles>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SatRepositoryInfoFiles.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(SatRepositoryInfoFiles.UpdatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(SatRepositoryInfoFiles.DeletedOnUtc)).AsDateTime().Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(SatRepositoryInfoFiles), nameof(SatRepositoryInfoFiles.OwnerUserId))).AsInt32().ForeignKey<Customer>(onDelete: Rule.None).Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(SatRepositoryInfoFiles), nameof(SatRepositoryInfoFiles.UpdatedUserId))).AsInt32().ForeignKey<Customer>(onDelete: Rule.None).Nullable()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(SatRepositoryInfoFiles), nameof(SatRepositoryInfoFiles.DeletedUserId))).AsInt32().ForeignKey<Customer>(onDelete: Rule.None).Nullable();
                 
        }

        #endregion
    }
}
