using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IGdprService _gdprService;

        #endregion

        #region Ctor

        public CustomerModelFactory(AddressSettings addressSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            GdprSettings gdprSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressModelFactory addressModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            IGeoLookupService geoLookupService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            RewardPointsSettings rewardPointsSettings,
            IGdprService gdprService)
        {
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _gdprSettings = gdprSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressModelFactory = addressModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _geoLookupService = geoLookupService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _rewardPointsSettings = rewardPointsSettings;
            _gdprService = gdprService;
        }

        #endregion

        #region Utilities

       
        /// <summary>
        /// Prepare customer attribute models
        /// </summary>
        /// <param name="models">List of customer attribute models</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareCustomerAttributeModelsAsync(IList<CustomerModel.CustomerAttributeModel> models, Customer customer)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //get available customer attributes
            var customerAttributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
            foreach (var attribute in customerAttributes)
            {
                var attributeModel = new CustomerModel.CustomerAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new CustomerModel.CustomerAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                if (customer != null)
                {
                    var selectedCustomerAttributes = await _genericAttributeService
                        .GetAttributeAsync<string>(customer, NopCustomerDefaults.CustomCustomerAttributes);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                            {
                                if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = await _customerAttributeParser.ParseCustomerAttributeValuesAsync(selectedCustomerAttributes);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //do nothing
                                //values are already pre-set
                            }
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!string.IsNullOrEmpty(selectedCustomerAttributes))
                                {
                                    var enteredText = _customerAttributeParser.ParseValues(selectedCustomerAttributes, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        case AttributeControlType.FileUpload:
                        default:
                            //not supported attribute control types
                            break;
                    }
                }

                models.Add(attributeModel);
            }
        }

        /// <summary>
        /// Prepare HTML string address
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareModelAddressHtmlAsync(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var addressHtmlSb = new StringBuilder("<div>");

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.County));

            if (_addressSettings.StateProvinceEnabled && !string.IsNullOrEmpty(model.StateProvinceName))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));

            var customAttributesFormatted = await _addressAttributeFormatter.FormatAttributesAsync(address?.CustomAttributes);
            if (!string.IsNullOrEmpty(customAttributesFormatted))
            {
                //already encoded
                addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
            }

            addressHtmlSb.Append("</div>");

            model.AddressHtml = addressHtmlSb.ToString();
        }

        
        /// <summary>
        /// Prepare customer address search model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer address search model</returns>
        protected virtual CustomerAddressSearchModel PrepareCustomerAddressSearchModel(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        
        

        /// <summary>
        /// Prepare customer activity log search model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Customer activity log search model</returns>
        protected virtual CustomerActivityLogSearchModel PrepareCustomerActivityLogSearchModel(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare customer back in stock subscriptions search model
        /// </summary>
        /// <param name="searchModel">Customer back in stock subscriptions search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer back in stock subscriptions search model
        /// </returns>
        protected virtual async Task<CustomerAssociatedExternalAuthRecordsSearchModel> PrepareCustomerAssociatedExternalAuthRecordsSearchModelAsync(
            CustomerAssociatedExternalAuthRecordsSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            searchModel.CustomerId = customer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            
            return searchModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare customer search model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer search model
        /// </returns>
        public virtual async Task<CustomerSearchModel> PrepareCustomerSearchModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            searchModel.AvatarEnabled = _customerSettings.AllowCustomersToUploadAvatars;
            searchModel.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            searchModel.LastNameEnabled = _customerSettings.LastNameEnabled;
            searchModel.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            searchModel.CompanyEnabled = _customerSettings.CompanyEnabled;
            searchModel.PhoneEnabled = _customerSettings.PhoneEnabled;
            searchModel.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;

            //search registered customers by default
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AccessRoleName);
            if (registeredRole != null)
                searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

            //prepare available customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged customer list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer list model
        /// </returns>
        public virtual async Task<CustomerListModel> PrepareCustomerListModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            _ = int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
            _ = int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

            //get customers
            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
                email: searchModel.SearchEmail,
                username: searchModel.SearchUsername,
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                dayOfBirth: dayOfBirth,
                monthOfBirth: monthOfBirth,
                company: searchModel.SearchCompany,
                phone: searchModel.SearchPhone,
                zipPostalCode: searchModel.SearchZipPostalCode,
                ipAddress: searchModel.SearchIpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new CustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
            {
                return customers.SelectAwait(async customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<CustomerModel>();

                    //convert dates to the user time
                    customerModel.Email = (await _customerService.IsRegisteredAsync(customer))
                        ? customer.Email
                        : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                    customerModel.FullName = await _customerService.GetCustomerFullNameAsync(customer);
                    customerModel.Company = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CompanyAttribute);
                    customerModel.Phone = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    customerModel.ZipPostalCode = await _genericAttributeService
                        .GetAttributeAsync<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);

                    customerModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(customer.CreatedOnUtc, DateTimeKind.Utc);
                    customerModel.LastActivityDate = await _dateTimeHelper.ConvertToUserTimeAsync(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerRoleNames = string.Join(", ",
                        (await _customerService.GetCustomerRolesAsync(customer)).Select(role => role.Name));
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        var avatarPictureId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                        customerModel.AvatarUrl = String.Empty;
                    }

                    return customerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer model
        /// </summary>
        /// <param name="model">Customer model</param>
        /// <param name="customer">Customer</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer model
        /// </returns>
        public virtual async Task<CustomerModel> PrepareCustomerModelAsync(CustomerModel model, Customer customer, bool excludeProperties = false)
        {
            if (customer != null)
            {
                //fill in model values from the entity
                model ??= new CustomerModel();

                model.Id = customer.Id;
                model.AllowSendingOfWelcomeMessage = await _customerService.IsRegisteredAsync(customer) &&
                    _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval;
                model.AllowReSendingOfActivationMessage = await _customerService.IsRegisteredAsync(customer) && !customer.Active &&
                    _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                model.GdprEnabled = _gdprSettings.GdprEnabled;

                model.MultiFactorAuthenticationProvider = await _genericAttributeService
                    .GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute);

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.Email = customer.Email;
                    model.Username = customer.Username;
                    model.VendorId = customer.VendorId;
                    model.AdminComment = customer.AdminComment;
                    model.IsTaxExempt = customer.IsTaxExempt;
                    model.Active = customer.Active;
                    model.FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                    model.LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);
                    model.Gender = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.GenderAttribute);
                    model.DateOfBirth = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
                    model.Company = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CompanyAttribute);
                    model.StreetAddress = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddressAttribute);
                    model.StreetAddress2 = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddress2Attribute);
                    model.ZipPostalCode = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);
                    model.City = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CityAttribute);
                    model.County = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CountyAttribute);
                    model.CountryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute);
                    model.StateProvinceId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute);
                    model.Phone = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    model.Fax = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FaxAttribute);
                    model.TimeZoneId = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.TimeZoneIdAttribute);
                    model.VatNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.VatNumberAttribute);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = await _dateTimeHelper.ConvertToUserTimeAsync(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = customer.LastIpAddress;
                    model.LastVisitedPage = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute);
                    model.SelectedCustomerRoleIds = (await _customerService.GetCustomerRoleIdsAsync(customer)).ToList();
                    model.RegisteredInStore = (await _storeService.GetAllStoresAsync())
                        .FirstOrDefault(store => store.Id == customer.RegisteredInStoreId)?.Name ?? string.Empty;
                    model.DisplayRegisteredInStore = model.Id > 0 && !string.IsNullOrEmpty(model.RegisteredInStore) &&
                        (await _storeService.GetAllStoresAsync()).Select(x => x.Id).Count() > 1;

                    
                }

                PrepareCustomerAddressSearchModel(model.CustomerAddressSearchModel, customer);
                PrepareCustomerActivityLogSearchModel(model.CustomerActivityLogSearchModel, customer);

                await PrepareCustomerAssociatedExternalAuthRecordsSearchModelAsync(model.CustomerAssociatedExternalAuthRecordsSearchModel, customer);
            }
            else
            {
                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    //precheck Registered Role as a default role while creating a new customer through admin
                    var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AccessRoleName);
                    if (registeredRole != null)
                        model.SelectedCustomerRoleIds.Add(registeredRole.Id);
                }
            }

            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            model.LastNameEnabled = _customerSettings.LastNameEnabled;
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CountyEnabled = _customerSettings.CountyEnabled;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.FaxEnabled = _customerSettings.FaxEnabled;

            //set default values for the new model
            if (customer == null)
            {
                model.Active = true;
                model.DisplayVatNumber = false;
            }

            //prepare model customer attributes
            await PrepareCustomerAttributeModelsAsync(model.CustomerAttributes, customer);

            //prepare model stores for newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = (await _storeService.GetAllStoresAsync()).Select(store => new SelectListItem
            {
                Value = store.Id.ToString(),
                Text = store.Name,
                Selected = model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id)
            }).ToList();

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);

            //prepare available time zones
            await _baseAdminModelFactory.PrepareTimeZonesAsync(model.AvailableTimeZones, false);

            //prepare available countries and states
            if (_customerSettings.CountryEnabled)
            {
                await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);
                if (_customerSettings.StateProvinceEnabled)
                    await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId == 0 ? null : (int?)model.CountryId);
            }

            return model;
        }

        
        /// <summary>
        /// Prepare paged customer address list model
        /// </summary>
        /// <param name="searchModel">Customer address search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer address list model
        /// </returns>
        public virtual async Task<CustomerAddressListModel> PrepareCustomerAddressListModelAsync(CustomerAddressSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer addresses
            var addresses = (await _customerService.GetAddressesByCustomerIdAsync(customer.Id))
                .OrderByDescending(address => address.CreatedOnUtc).ThenByDescending(address => address.Id).ToList()
                .ToPagedList(searchModel);

            //prepare list model
            var model = await new CustomerAddressListModel().PrepareToGridAsync(searchModel, addresses, () =>
            {
                return addresses.SelectAwait(async address =>
                {
                    //fill in model values from the entity        
                    var addressModel = address.ToModel<AddressModel>();

                    addressModel.CountryName = (await _countryService.GetCountryByAddressAsync(address))?.Name;
                    addressModel.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name;

                    //fill in additional values (not existing in the entity)
                    await PrepareModelAddressHtmlAsync(addressModel, address);

                    return addressModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer address model
        /// </summary>
        /// <param name="model">Customer address model</param>
        /// <param name="customer">Customer</param>
        /// <param name="address">Address</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer address model
        /// </returns>
        public virtual async Task<CustomerAddressModel> PrepareCustomerAddressModelAsync(CustomerAddressModel model,
            Customer customer, Address address, bool excludeProperties = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (address != null)
            {
                //fill in model values from the entity
                model ??= new CustomerAddressModel();

                //whether to fill in some of properties
                if (!excludeProperties)
                    model.Address = address.ToModel(model.Address);
            }

            model.CustomerId = customer.Id;

            //prepare address model
            await _addressModelFactory.PrepareAddressModelAsync(model.Address, address);
            model.Address.FirstNameRequired = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.CountyRequired = _addressSettings.CountyRequired;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxRequired = _addressSettings.FaxRequired;

            return model;
        }

        
        /// <summary>
        /// Prepare paged customer activity log list model
        /// </summary>
        /// <param name="searchModel">Customer activity log search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer activity log list model
        /// </returns>
        public virtual async Task<CustomerActivityLogListModel> PrepareCustomerActivityLogListModelAsync(CustomerActivityLogSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer activity log
            var activityLog = await _customerActivityService.GetAllActivitiesAsync(customerId: customer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new CustomerActivityLogListModel().PrepareToGridAsync(searchModel, activityLog, () =>
            {
                return activityLog.SelectAwait(async logItem =>
                {
                    //fill in model values from the entity
                    var customerActivityLogModel = logItem.ToModel<CustomerActivityLogModel>();

                    //fill in additional values (not existing in the entity)
                    customerActivityLogModel.ActivityLogTypeName = (await _customerActivityService.GetActivityTypeByIdAsync(logItem.ActivityLogTypeId))?.Name;

                    //convert dates to the user time
                    customerActivityLogModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(logItem.CreatedOnUtc, DateTimeKind.Utc);

                    return customerActivityLogModel;
                });
            });

            return model;
        }

        

        /// <summary>
        /// Prepare online customer search model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the online customer search model
        /// </returns>
        public virtual Task<OnlineCustomerSearchModel> PrepareOnlineCustomerSearchModelAsync(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged online customer list model
        /// </summary>
        /// <param name="searchModel">Online customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the online customer list model
        /// </returns>
        public virtual async Task<OnlineCustomerListModel> PrepareOnlineCustomerListModelAsync(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            var lastActivityFrom = DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes);

            //get online customers
            var customers = await _customerService.GetOnlineCustomersAsync(customerRoleIds: null,
                 lastActivityFromUtc: lastActivityFrom,
                 pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new OnlineCustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
            {
                return customers.SelectAwait(async customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<OnlineCustomerModel>();

                    //convert dates to the user time
                    customerModel.LastActivityDate = await _dateTimeHelper.ConvertToUserTimeAsync(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerInfo = (await _customerService.IsRegisteredAsync(customer))
                        ? customer.Email
                        : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                    customerModel.LastIpAddress = _customerSettings.StoreIpAddresses
                        ? customer.LastIpAddress
                        : await _localizationService.GetResourceAsync("Admin.Customers.OnlineCustomers.Fields.IPAddress.Disabled");
                    customerModel.Location = _geoLookupService.LookupCountryName(customer.LastIpAddress);
                    customerModel.LastVisitedPage = _customerSettings.StoreLastVisitedPage
                        ? await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute)
                        : await _localizationService.GetResourceAsync("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage.Disabled");

                    return customerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare GDPR request (log) search model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR request search model
        /// </returns>
        public virtual async Task<GdprLogSearchModel> PrepareGdprLogSearchModelAsync(GdprLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare request types
            await _baseAdminModelFactory.PrepareGdprRequestTypesAsync(searchModel.AvailableRequestTypes);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged GDPR request list model
        /// </summary>
        /// <param name="searchModel">GDPR request search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR request list model
        /// </returns>
        public virtual async Task<GdprLogListModel> PrepareGdprLogListModelAsync(GdprLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerId = 0;
            var customerInfo = "";
            if (!string.IsNullOrEmpty(searchModel.SearchEmail))
            {
                var customer = await _customerService.GetCustomerByEmailAsync(searchModel.SearchEmail);
                if (customer != null)
                    customerId = customer.Id;
                else
                {
                    customerInfo = searchModel.SearchEmail;
                }
            }
            //get requests
            var gdprLog = await _gdprService.GetAllLogAsync(
                customerId: customerId,
                customerInfo: customerInfo,
                requestType: searchModel.SearchRequestTypeId > 0 ? (GdprRequestType?)searchModel.SearchRequestTypeId : null,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new GdprLogListModel().PrepareToGridAsync(searchModel, gdprLog, () =>
            {
                return gdprLog.SelectAwait(async log =>
                {
                    //fill in model values from the entity
                    var customer = await _customerService.GetCustomerByIdAsync(log.CustomerId);

                    var requestModel = log.ToModel<GdprLogModel>();

                    //fill in additional values (not existing in the entity)
                    requestModel.CustomerInfo = customer != null && !customer.Deleted && !string.IsNullOrEmpty(customer.Email)
                        ? customer.Email
                        : log.CustomerInfo;
                    requestModel.RequestType = await _localizationService.GetLocalizedEnumAsync(log.RequestType);

                    requestModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(log.CreatedOnUtc, DateTimeKind.Utc);

                    return requestModel;
                });
            });

            return model;
        }

        #endregion
    }
}