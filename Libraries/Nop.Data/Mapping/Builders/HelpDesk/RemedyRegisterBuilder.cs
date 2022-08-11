using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.HelpDesk;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.HelpDesk
{
    /// <summary>
    /// Represents a remedy register builder
    /// </summary>
    public partial class RemedyRegisterBuilder : NopEntityBuilder<RemedyRegister>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RemedyRegister.TicketNumber)).AsString(200).NotNullable()
                .WithColumn(nameof(RemedyRegister.SATNumber)).AsString(200).Nullable()
                .WithColumn(nameof(RemedyRegister.ReportType)).AsString(200).Nullable()
                .WithColumn(nameof(RemedyRegister.FaultEventType)).AsString(200).Nullable()
                .WithColumn(nameof(RemedyRegister.FaultEventCause)).AsString(200).Nullable()
                .WithColumn(nameof(RemedyRegister.Status)).AsString(200).Nullable()
                .WithColumn(nameof(RemedyRegister.EventDate)).AsString(200).Nullable()
                .WithColumn(nameof(RemedyRegister.CreatedOnUtc)).AsDateTime().Nullable().WithDefault(FluentMigrator.SystemMethods.CurrentDateTime);
        }

        #endregion
    }
}