using System.Data;
using Nop.Core.Domain.SatRepositoryInfo;
using Nop.Core.Domain.Customers;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.SatRepositoryInfo
{
    public partial class SatRepositoryFileCustomerBuilder : NopEntityBuilder<SatRepositoryFilesCustomers>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {

        }
        #endregion
    }
}