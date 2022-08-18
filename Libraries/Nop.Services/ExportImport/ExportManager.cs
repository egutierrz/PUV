using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.HelpDesk;
using Nop.Core.Domain.Messages;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        private readonly ICountryService _countryService;

        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public ExportManager(
            ICountryService countryService,
            ICustomerAttributeFormatter customerAttributeFormatter,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService)
        {
            _countryService = countryService;
            _customerAttributeFormatter = customerAttributeFormatter;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<object> GetCustomCustomerAttributesAsync(Customer customer)
        {
            var selectedCustomerAttributes = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);

            return await _customerAttributeFormatter.FormatAttributesAsync(selectedCustomerAttributes, ";");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>A task that represents the asynchronous operation</returns>


        /// <summary>
        /// Export customer list to XML
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result in XML format
        /// </returns>
        public virtual async Task<string> ExportCustomersToXmlAsync(IList<Customer> customers)
        {
            var settings = new XmlWriterSettings
            {
                Async = true,
                ConformanceLevel = ConformanceLevel.Auto
            };

            await using var stringWriter = new StringWriter();
            await using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            await xmlWriter.WriteStartDocumentAsync();
            await xmlWriter.WriteStartElementAsync("Customers");
            await xmlWriter.WriteAttributeStringAsync("Version", NopVersion.CURRENT_VERSION);

            foreach (var customer in customers)
            {
                await xmlWriter.WriteStartElementAsync("Customer");
                await xmlWriter.WriteElementStringAsync("CustomerId", null, customer.Id.ToString());
                await xmlWriter.WriteElementStringAsync("CustomerGuid", null, customer.CustomerGuid.ToString());
                await xmlWriter.WriteElementStringAsync("Email", null, customer.Email);
                await xmlWriter.WriteElementStringAsync("Username", null, customer.Username);

                var customerPassword = await _customerService.GetCurrentPasswordAsync(customer.Id);
                await xmlWriter.WriteElementStringAsync("Password", null, customerPassword?.Password);
                await xmlWriter.WriteElementStringAsync("PasswordFormatId", null, (customerPassword?.PasswordFormatId ?? 0).ToString());
                await xmlWriter.WriteElementStringAsync("PasswordSalt", null, customerPassword?.PasswordSalt);

                await xmlWriter.WriteElementStringAsync("IsTaxExempt", null, customer.IsTaxExempt.ToString());
                await xmlWriter.WriteElementStringAsync("AffiliateId", null, customer.AffiliateId.ToString());
                await xmlWriter.WriteElementStringAsync("VendorId", null, customer.VendorId.ToString());
                await xmlWriter.WriteElementStringAsync("Active", null, customer.Active.ToString());

                await xmlWriter.WriteElementStringAsync("IsGuest", null, (await _customerService.IsBackgroundAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("IsRegistered", null, (await _customerService.IsRegisteredAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("IsAdministrator", null, (await _customerService.IsAdminAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("CreatedOnUtc", null, customer.CreatedOnUtc.ToString(CultureInfo.InvariantCulture));

                await xmlWriter.WriteElementStringAsync("FirstName", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute));
                await xmlWriter.WriteElementStringAsync("LastName", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute));
                await xmlWriter.WriteElementStringAsync("Gender", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.GenderAttribute));
                await xmlWriter.WriteElementStringAsync("Company", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CompanyAttribute));

                await xmlWriter.WriteElementStringAsync("CountryId", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("StreetAddress", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddressAttribute));
                await xmlWriter.WriteElementStringAsync("StreetAddress2", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddress2Attribute));
                await xmlWriter.WriteElementStringAsync("ZipPostalCode", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute));
                await xmlWriter.WriteElementStringAsync("City", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CityAttribute));
                await xmlWriter.WriteElementStringAsync("County", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CountyAttribute));
                await xmlWriter.WriteElementStringAsync("StateProvinceId", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("Phone", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute));
                await xmlWriter.WriteElementStringAsync("Fax", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FaxAttribute));
                await xmlWriter.WriteElementStringAsync("VatNumber", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.VatNumberAttribute));
                await xmlWriter.WriteElementStringAsync("VatNumberStatusId", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.VatNumberStatusIdAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("TimeZoneId", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.TimeZoneIdAttribute));

                await xmlWriter.WriteElementStringAsync("AvatarPictureId", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("ForumPostCount", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.ForumPostCountAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("Signature", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SignatureAttribute));

                var selectedCustomerAttributesString = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);

                if (!string.IsNullOrEmpty(selectedCustomerAttributesString))
                {
                    var selectedCustomerAttributes = new StringReader(selectedCustomerAttributesString);
                    var selectedCustomerAttributesXmlReader = XmlReader.Create(selectedCustomerAttributes);
                    await xmlWriter.WriteNodeAsync(selectedCustomerAttributesXmlReader, false);
                }

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
            await xmlWriter.WriteEndDocumentAsync();
            await xmlWriter.FlushAsync();

            return stringWriter.ToString();
        }


        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result in TXT (string) format
        /// </returns>
        public virtual async Task<string> ExportStatesToTxtAsync(IList<StateProvince> states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            const char separator = ',';
            var sb = new StringBuilder();
            foreach (var state in states)
            {
                sb.Append((await _countryService.GetCountryByIdAsync(state.CountryId)).TwoLetterIsoCode);
                sb.Append(separator);
                sb.Append(state.Name);
                sb.Append(separator);
                sb.Append(state.Abbreviation);
                sb.Append(separator);
                sb.Append(state.Published);
                sb.Append(separator);
                sb.Append(state.DisplayOrder);
                sb.Append(Environment.NewLine); //new line
            }

            return sb.ToString();
        }

        /// <summary>
        /// Export remedy register list to XLSX
        /// </summary>
        /// <param name="records">Remedy Registers</param>
        /// <returns>A task that represents the asynchronous operation
        /// The task result contains the result in XML format
        /// </returns>

        public virtual async Task<string> ExportRemedyRegistersToXmlAsync(IList<RemedyRegister> records)
        {
            var settings = new XmlWriterSettings
            {
                Async = true,
                ConformanceLevel = ConformanceLevel.Auto
            };

            await using var stringWriter = new StringWriter();
            await using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            await xmlWriter.WriteStartDocumentAsync();
            await xmlWriter.WriteStartElementAsync("RemedyRegisters");
            await xmlWriter.WriteAttributeStringAsync("Version", NopVersion.CURRENT_VERSION);

            foreach (var record in records)
            {
                await xmlWriter.WriteStartElementAsync("RemedyRegister");
                await xmlWriter.WriteElementStringAsync("Id", null, record.Id.ToString());
                await xmlWriter.WriteElementStringAsync("TicketNumber", null, record.TicketNumber);
                await xmlWriter.WriteElementStringAsync("SATNumber", null, record.SATNumber);
                await xmlWriter.WriteElementStringAsync("ReportType", null, record.ReportType.ToString());
                await xmlWriter.WriteElementStringAsync("EventType", null, record.FaultEventType.ToString());
                await xmlWriter.WriteElementStringAsync("EventCause", null, record.FaultEventCause);
                await xmlWriter.WriteElementStringAsync("Status", null, record.Status.ToString());
                await xmlWriter.WriteElementStringAsync("EventDate", null, record.EventDate);
                await xmlWriter.WriteElementStringAsync("CreatedOnUtc", null, record.CreatedOnUtc.ToString());
            }

            await xmlWriter.WriteEndElementAsync();
            await xmlWriter.WriteEndDocumentAsync();
            await xmlWriter.FlushAsync();

            return stringWriter.ToString();
        }

        #endregion
    }
}
