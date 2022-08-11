using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cisco;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Topics;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Core.Security;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Services.Installation
{
    /// <summary>
    /// Installation service
    /// </summary>
    public partial class InstallationService : IInstallationService
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;
        private readonly INopFileProvider _fileProvider;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<TopicTemplate> _topicTemplateRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public InstallationService(INopDataProvider dataProvider,
            INopFileProvider fileProvider,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<Address> addressRepository,
            IRepository<Country> countryRepository,
            IRepository<Currency> currencyRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<Language> languageRepository,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Store> storeRepository,
            IRepository<TopicTemplate> topicTemplateRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IWebHelper webHelper,
            ILocalizationService localizationService)
        {
            _dataProvider = dataProvider;
            _fileProvider = fileProvider;
            _activityLogTypeRepository = activityLogTypeRepository;
            _addressRepository = addressRepository;
            _countryRepository = countryRepository;
            _currencyRepository = currencyRepository;
            _customerRepository = customerRepository;
            _customerRoleRepository = customerRoleRepository;
            _emailAccountRepository = emailAccountRepository;
            _languageRepository = languageRepository;
            _measureDimensionRepository = measureDimensionRepository;
            _measureWeightRepository = measureWeightRepository;
            _stateProvinceRepository = stateProvinceRepository;
            _storeRepository = storeRepository;
            _topicTemplateRepository = topicTemplateRepository;
            _urlRecordRepository = urlRecordRepository;
            _webHelper = webHelper;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<T> InsertInstallationDataAsync<T>(T entity) where T : BaseEntity
        {
            return await _dataProvider.InsertEntityAsync(entity);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InsertInstallationDataAsync<T>(params T[] entities) where T : BaseEntity
        {
            await _dataProvider.BulkInsertEntitiesAsync(entities);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InsertInstallationDataAsync<T>(IList<T> entities) where T : BaseEntity
        {
            if (!entities.Any())
                return;

            await InsertInstallationDataAsync(entities.ToArray());
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateInstallationDataAsync<T>(T entity) where T : BaseEntity
        {
            await _dataProvider.UpdateEntityAsync(entity);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateInstallationDataAsync<T>(IList<T> entities) where T : BaseEntity
        {
            if (!entities.Any())
                return;

            foreach (var entity in entities)
                await _dataProvider.UpdateEntityAsync(entity);
        }

        
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<string> ValidateSeNameAsync<T>(T entity, string seName) where T : BaseEntity
        {
            //duplicate of ValidateSeName method of \Nop.Services\Seo\UrlRecordService.cs (we cannot inject it here)
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            //validation
            var okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-";
            seName = seName.Trim().ToLowerInvariant();

            var sb = new StringBuilder();
            foreach (var c in seName.ToCharArray())
            {
                var c2 = c.ToString();
                if (okChars.Contains(c2))
                    sb.Append(c2);
            }

            seName = sb.ToString();
            seName = seName.Replace(" ", "-");
            while (seName.Contains("--"))
                seName = seName.Replace("--", "-");
            while (seName.Contains("__"))
                seName = seName.Replace("__", "_");

            //max length
            seName = CommonHelper.EnsureMaximumLength(seName, NopSeoDefaults.SearchEngineNameLength);

            //ensure this sename is not reserved yet
            var i = 2;
            var tempSeName = seName;
            while (true)
            {
                //check whether such slug already exists (and that is not the current entity)

                var query = from ur in _urlRecordRepository.Table
                            where tempSeName != null && ur.Slug == tempSeName
                            select ur;
                var urlRecord = await query.FirstOrDefaultAsync();

                var entityName = entity.GetType().Name;
                var reserved = urlRecord != null && !(urlRecord.EntityId == entity.Id && urlRecord.EntityName.Equals(entityName, StringComparison.InvariantCultureIgnoreCase));
                if (!reserved)
                    break;

                tempSeName = $"{seName}-{i}";
                i++;
            }

            seName = tempSeName;

            return seName;
        }

        protected virtual string GetSamplesPath()
        {
            return _fileProvider.GetAbsolutePath(NopInstallationDefaults.SampleImagesPath);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallEmailAccountsAsync()
        {
            var emailAccounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    Email = "test@mail.com",
                    DisplayName = "Store name",
                    Host = "smtp.mail.com",
                    Port = 25,
                    Username = "123",
                    Password = "123",
                    EnableSsl = false,
                    UseDefaultCredentials = false
                }
            };

            await InsertInstallationDataAsync(emailAccounts);
        }
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallStoresAsync()
        {
            var storeUrl = _webHelper.GetStoreLocation();
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "SAT PUV",
                    Url = storeUrl,
                    SslEnabled = _webHelper.IsCurrentConnectionSecured(),
                    Hosts = "instance.com",
                    DisplayOrder = 1,
                    //should we set some default company info?
                    CompanyName = "SAT",
                    CompanyAddress = "your company country, state, zip, street, etc",
                    CompanyPhoneNumber = "(123) 456-78901",
                    CompanyVat = null
                }
            };

            await InsertInstallationDataAsync(stores);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallLanguagesAsync((string languagePackDownloadLink, int languagePackProgress) languagePackInfo, CultureInfo cultureInfo, RegionInfo regionInfo)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            var defaultCulture = new CultureInfo(NopCommonDefaults.DefaultLanguageCulture);
            var defaultLanguage = new Language
            {
                Name = defaultCulture.TwoLetterISOLanguageName.ToUpperInvariant(),
                LanguageCulture = defaultCulture.Name,
                UniqueSeoCode = defaultCulture.TwoLetterISOLanguageName,
                FlagImageFileName = $"{defaultCulture.Name.ToLowerInvariant()[^2..]}.png",
                Rtl = defaultCulture.TextInfo.IsRightToLeft,
                Published = true,
                DisplayOrder = 1
            };
            await InsertInstallationDataAsync(defaultLanguage);

            //Install locale resources for default culture
            var directoryPath = _fileProvider.MapPath(NopInstallationDefaults.LocalizationResourcesPath);
            var pattern = $"*.{NopInstallationDefaults.LocalizationResourcesFileExtension}";
            foreach (var filePath in _fileProvider.EnumerateFiles(directoryPath, pattern))
            {
                using var streamReader = new StreamReader(filePath);
                await localizationService.ImportResourcesFromXmlAsync(defaultLanguage, streamReader);
            }

            if (cultureInfo == null || regionInfo == null || cultureInfo.Name == NopCommonDefaults.DefaultLanguageCulture)
                return;

            var language = new Language
            {
                Name = cultureInfo.TwoLetterISOLanguageName.ToUpperInvariant(),
                LanguageCulture = cultureInfo.Name,
                UniqueSeoCode = cultureInfo.TwoLetterISOLanguageName,
                FlagImageFileName = $"{regionInfo.TwoLetterISORegionName.ToLowerInvariant()}.png",
                Rtl = cultureInfo.TextInfo.IsRightToLeft,
                Published = true,
                DisplayOrder = 2
            };
            await InsertInstallationDataAsync(language);

            if (string.IsNullOrEmpty(languagePackInfo.languagePackDownloadLink))
                return;

            //download and import language pack
            try
            {
                var httpClientFactory = EngineContext.Current.Resolve<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                await using var stream = await httpClient.GetStreamAsync(languagePackInfo.languagePackDownloadLink);
                using var streamReader = new StreamReader(stream);
                await localizationService.ImportResourcesFromXmlAsync(language, streamReader);

                //set this language as default
                language.DisplayOrder = 0;
                await UpdateInstallationDataAsync(language);

                //save progress for showing in admin panel (only for first start)
                await InsertInstallationDataAsync(new GenericAttribute
                {
                    EntityId = language.Id,
                    Key = NopCommonDefaults.LanguagePackProgressAttribute,
                    KeyGroup = nameof(Language),
                    Value = languagePackInfo.languagePackProgress.ToString(),
                    StoreId = 0,
                    CreatedOrUpdatedDateUTC = DateTime.UtcNow
                });
            }
            catch { }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallCurrenciesAsync(CultureInfo cultureInfo, RegionInfo regionInfo)
        {
            //set some currencies with a rate against the USD
            var defaultCurrencies = new List<string>() { "USD", "AUD", "GBP", "CAD", "CNY", "EUR", "HKD", "JPY", "RUB", "SEK", "INR" };
            var currencies = new List<Currency>
            {
                new Currency
                {
                    Name = "US Dollar",
                    CurrencyCode = "USD",
                    Rate = 1,
                    DisplayLocale = "en-US",
                    CustomFormatting = string.Empty,
                    Published = true,
                    DisplayOrder = 1,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Australian Dollar",
                    CurrencyCode = "AUD",
                    Rate = 1.34M,
                    DisplayLocale = "en-AU",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 2,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "British Pound",
                    CurrencyCode = "GBP",
                    Rate = 0.75M,
                    DisplayLocale = "en-GB",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 3,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Canadian Dollar",
                    CurrencyCode = "CAD",
                    Rate = 1.32M,
                    DisplayLocale = "en-CA",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 4,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Chinese Yuan Renminbi",
                    CurrencyCode = "CNY",
                    Rate = 6.43M,
                    DisplayLocale = "zh-CN",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 5,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Euro",
                    CurrencyCode = "EUR",
                    Rate = 0.86M,
                    DisplayLocale = string.Empty,
                    CustomFormatting = $"{"\u20ac"}0.00", //euro symbol
                    Published = false,
                    DisplayOrder = 6,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Hong Kong Dollar",
                    CurrencyCode = "HKD",
                    Rate = 7.84M,
                    DisplayLocale = "zh-HK",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 7,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Japanese Yen",
                    CurrencyCode = "JPY",
                    Rate = 110.45M,
                    DisplayLocale = "ja-JP",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 8,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Russian Rouble",
                    CurrencyCode = "RUB",
                    Rate = 63.25M,
                    DisplayLocale = "ru-RU",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 9,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                },
                new Currency
                {
                    Name = "Swedish Krona",
                    CurrencyCode = "SEK",
                    Rate = 8.80M,
                    DisplayLocale = "sv-SE",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 10,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding1
                },
                new Currency
                {
                    Name = "Indian Rupee",
                    CurrencyCode = "INR",
                    Rate = 68.03M,
                    DisplayLocale = "en-IN",
                    CustomFormatting = string.Empty,
                    Published = false,
                    DisplayOrder = 12,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    RoundingType = RoundingType.Rounding001
                }
            };

            //set additional currency
            if (cultureInfo != null && regionInfo != null)
            {
                if (!defaultCurrencies.Contains(regionInfo.ISOCurrencySymbol))
                {
                    currencies.Add(new Currency
                    {
                        Name = regionInfo.CurrencyEnglishName,
                        CurrencyCode = regionInfo.ISOCurrencySymbol,
                        Rate = 1,
                        DisplayLocale = cultureInfo.Name,
                        CustomFormatting = string.Empty,
                        Published = true,
                        DisplayOrder = 0,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow,
                        RoundingType = RoundingType.Rounding001
                    });
                }

                foreach (var currency in currencies.Where(currency => currency.CurrencyCode == regionInfo.ISOCurrencySymbol))
                {
                    currency.Published = true;
                    currency.DisplayOrder = 0;
                }
            }


            await InsertInstallationDataAsync(currencies);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallCountriesAndStatesAsync()
        {
            var countries = ISO3166.GetCollection().Select(country => new Country
            {
                Name = country.Name,
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = country.Alpha2,
                ThreeLetterIsoCode = country.Alpha3,
                NumericIsoCode = country.NumericCode,
                SubjectToVat = country.SubjectToVat,
                DisplayOrder = country.NumericCode == 840 ? 1 : 100,
                Published = true
            }).ToList();

            await InsertInstallationDataAsync(countries.ToArray());

            //Import states for all countries
            var directoryPath = _fileProvider.MapPath(NopInstallationDefaults.LocalizationResourcesPath);
            var pattern = "*.txt";

            //we use different scope to prevent creating wrong settings in DI, because the settings data not exists yet
            var serviceScopeFactory = EngineContext.Current.Resolve<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var importManager = EngineContext.Current.Resolve<IImportManager>(scope);
            foreach (var filePath in _fileProvider.EnumerateFiles(directoryPath, pattern))
            {
                await using var stream = new FileStream(filePath, FileMode.Open);
                await importManager.ImportStatesFromTxtAsync(stream, false);
            }
        }


        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallSampleCustomersAsync()
        {
            var crRegistered = await _customerRoleRepository.Table
                .FirstOrDefaultAsync(customerRole => customerRole.SystemName == NopCustomerDefaults.AccessRoleName);

            if (crRegistered == null)
                throw new ArgumentNullException(nameof(crRegistered));

            //default store 
            var defaultStore = await _storeRepository.Table.FirstOrDefaultAsync();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //second user
            var secondUserEmail = "steve_gates@nopCommerce.com";
            var secondUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = secondUserEmail,
                Username = secondUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            var defaultSecondUserAddress = await InsertInstallationDataAsync(
                new Address
                {
                    FirstName = "Steve",
                    LastName = "Gates",
                    PhoneNumber = "87654321",
                    Email = secondUserEmail,
                    FaxNumber = string.Empty,
                    Company = "Steve Company",
                    Address1 = "750 Bel Air Rd.",
                    Address2 = string.Empty,
                    City = "Los Angeles",
                    StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "California")?.Id,
                    CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA")?.Id,
                    ZipPostalCode = "90077",
                    CreatedOnUtc = DateTime.UtcNow
                });

            secondUser.BillingAddressId = defaultSecondUserAddress.Id;
            secondUser.ShippingAddressId = defaultSecondUserAddress.Id;

            await InsertInstallationDataAsync(secondUser);

            await InsertInstallationDataAsync(new CustomerAddressMapping { CustomerId = secondUser.Id, AddressId = defaultSecondUserAddress.Id });
            await InsertInstallationDataAsync(new CustomerCustomerRoleMapping { CustomerId = secondUser.Id, CustomerRoleId = crRegistered.Id });

            //set default customer name
            await InsertInstallationDataAsync(new GenericAttribute
            {
                EntityId = secondUser.Id,
                Key = NopCustomerDefaults.FirstNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultSecondUserAddress.FirstName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            },
            new GenericAttribute
            {
                EntityId = secondUser.Id,
                Key = NopCustomerDefaults.LastNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultSecondUserAddress.LastName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            });

            //set customer password
            await InsertInstallationDataAsync(new CustomerPassword
            {
                CustomerId = secondUser.Id,
                Password = "123456",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            });

            //third user
            var thirdUserEmail = "arthur_holmes@nopCommerce.com";
            var thirdUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = thirdUserEmail,
                Username = thirdUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            var defaultThirdUserAddress = await InsertInstallationDataAsync(
                new Address
                {
                    FirstName = "Arthur",
                    LastName = "Holmes",
                    PhoneNumber = "111222333",
                    Email = thirdUserEmail,
                    FaxNumber = string.Empty,
                    Company = "Holmes Company",
                    Address1 = "221B Baker Street",
                    Address2 = string.Empty,
                    City = "London",
                    CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "GBR")?.Id,
                    ZipPostalCode = "NW1 6XE",
                    CreatedOnUtc = DateTime.UtcNow
                });

            thirdUser.BillingAddressId = defaultThirdUserAddress.Id;
            thirdUser.ShippingAddressId = defaultThirdUserAddress.Id;

            await InsertInstallationDataAsync(thirdUser);

            await InsertInstallationDataAsync(new CustomerAddressMapping { CustomerId = thirdUser.Id, AddressId = defaultThirdUserAddress.Id });
            await InsertInstallationDataAsync(new CustomerCustomerRoleMapping { CustomerId = thirdUser.Id, CustomerRoleId = crRegistered.Id });

            //set default customer name
            await InsertInstallationDataAsync(new GenericAttribute
            {
                EntityId = thirdUser.Id,
                Key = NopCustomerDefaults.FirstNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultThirdUserAddress.FirstName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            },
            new GenericAttribute
            {
                EntityId = thirdUser.Id,
                Key = NopCustomerDefaults.LastNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultThirdUserAddress.LastName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            });

            //set customer password
            await InsertInstallationDataAsync(new CustomerPassword
            {
                CustomerId = thirdUser.Id,
                Password = "123456",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            });

            //fourth user
            var fourthUserEmail = "james_pan@nopCommerce.com";
            var fourthUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = fourthUserEmail,
                Username = fourthUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            var defaultFourthUserAddress = await InsertInstallationDataAsync(
                new Address
                {
                    FirstName = "James",
                    LastName = "Pan",
                    PhoneNumber = "369258147",
                    Email = fourthUserEmail,
                    FaxNumber = string.Empty,
                    Company = "Pan Company",
                    Address1 = "St Katharine’s West 16",
                    Address2 = string.Empty,
                    City = "St Andrews",
                    CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "GBR")?.Id,
                    ZipPostalCode = "KY16 9AX",
                    CreatedOnUtc = DateTime.UtcNow
                });

            fourthUser.BillingAddressId = defaultFourthUserAddress.Id;
            fourthUser.ShippingAddressId = defaultFourthUserAddress.Id;

            await InsertInstallationDataAsync(fourthUser);

            await InsertInstallationDataAsync(new CustomerAddressMapping { CustomerId = fourthUser.Id, AddressId = defaultFourthUserAddress.Id });
            await InsertInstallationDataAsync(new CustomerCustomerRoleMapping { CustomerId = fourthUser.Id, CustomerRoleId = crRegistered.Id });

            //set default customer name
            await InsertInstallationDataAsync(new GenericAttribute
            {
                EntityId = fourthUser.Id,
                Key = NopCustomerDefaults.FirstNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultFourthUserAddress.FirstName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            },
            new GenericAttribute
            {
                EntityId = fourthUser.Id,
                Key = NopCustomerDefaults.LastNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultFourthUserAddress.LastName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            });

            //set customer password
            await InsertInstallationDataAsync(new CustomerPassword
            {
                CustomerId = fourthUser.Id,
                Password = "123456",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            });

            //fifth user
            var fifthUserEmail = "brenda_lindgren@nopCommerce.com";
            var fifthUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = fifthUserEmail,
                Username = fifthUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            var defaultFifthUserAddress = await InsertInstallationDataAsync(
                new Address
                {
                    FirstName = "Brenda",
                    LastName = "Lindgren",
                    PhoneNumber = "14785236",
                    Email = fifthUserEmail,
                    FaxNumber = string.Empty,
                    Company = "Brenda Company",
                    Address1 = "1249 Tongass Avenue, Suite B",
                    Address2 = string.Empty,
                    City = "Ketchikan",
                    StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "Alaska")?.Id,
                    CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "USA")?.Id,
                    ZipPostalCode = "99901",
                    CreatedOnUtc = DateTime.UtcNow
                });

            fifthUser.BillingAddressId = defaultFifthUserAddress.Id;
            fifthUser.ShippingAddressId = defaultFifthUserAddress.Id;

            await InsertInstallationDataAsync(fifthUser);

            await InsertInstallationDataAsync(new CustomerAddressMapping { CustomerId = fifthUser.Id, AddressId = defaultFifthUserAddress.Id });
            await InsertInstallationDataAsync(new CustomerCustomerRoleMapping { CustomerId = fifthUser.Id, CustomerRoleId = crRegistered.Id });

            //set default customer name
            await InsertInstallationDataAsync(new GenericAttribute
            {
                EntityId = fifthUser.Id,
                Key = NopCustomerDefaults.FirstNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultFifthUserAddress.FirstName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            },
            new GenericAttribute
            {
                EntityId = fifthUser.Id,
                Key = NopCustomerDefaults.LastNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultFifthUserAddress.LastName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            });

            //set customer password
            await InsertInstallationDataAsync(new CustomerPassword
            {
                CustomerId = fifthUser.Id,
                Password = "123456",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            });

            //sixth user
            var sixthUserEmail = "victoria_victoria@nopCommerce.com";
            var sixthUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = sixthUserEmail,
                Username = sixthUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };
            var defaultSixthUserAddress = await InsertInstallationDataAsync(
                new Address
                {
                    FirstName = "Victoria",
                    LastName = "Terces",
                    PhoneNumber = "45612378",
                    Email = sixthUserEmail,
                    FaxNumber = string.Empty,
                    Company = "Terces Company",
                    Address1 = "201 1st Avenue South",
                    Address2 = string.Empty,
                    City = "Saskatoon",
                    StateProvinceId = (await _stateProvinceRepository.Table.FirstOrDefaultAsync(sp => sp.Name == "Saskatchewan"))?.Id,
                    CountryId = (await _countryRepository.Table.FirstOrDefaultAsync(c => c.ThreeLetterIsoCode == "CAN"))?.Id,
                    ZipPostalCode = "S7K 1J9",
                    CreatedOnUtc = DateTime.UtcNow
                });

            sixthUser.BillingAddressId = defaultSixthUserAddress.Id;
            sixthUser.ShippingAddressId = defaultSixthUserAddress.Id;

            await InsertInstallationDataAsync(sixthUser);

            await InsertInstallationDataAsync(new CustomerAddressMapping { CustomerId = sixthUser.Id, AddressId = defaultSixthUserAddress.Id });
            await InsertInstallationDataAsync(new CustomerCustomerRoleMapping { CustomerId = sixthUser.Id, CustomerRoleId = crRegistered.Id });

            //set default customer name
            await InsertInstallationDataAsync(new GenericAttribute
            {
                EntityId = sixthUser.Id,
                Key = NopCustomerDefaults.FirstNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultSixthUserAddress.FirstName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            },
            new GenericAttribute
            {
                EntityId = sixthUser.Id,
                Key = NopCustomerDefaults.LastNameAttribute,
                KeyGroup = nameof(Customer),
                Value = defaultSixthUserAddress.LastName,
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            });

            //set customer password
            await InsertInstallationDataAsync(new CustomerPassword
            {
                CustomerId = sixthUser.Id,
                Password = "123456",
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallCustomersAndUsersAsync(string defaultUserEmail, string defaultUserPassword)
        {
            var crSuperAdministrators = new CustomerRole
            {
                Name = "Super Administrador",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.SuperAdminRoleName
            };
            var crAdministrators = new CustomerRole
            {
                Name = "Administrador",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.AdministratorsRoleName
            };
            var crAccess = new CustomerRole
            {
                Name = "Acceso",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.AccessRoleName
            };
            var crUsuario = new CustomerRole
            {
                Name = "Usuario",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.UserRoleName
            };
            var crVendors = new CustomerRole
            {
                Name = "Monitoreo SAT",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.MonitoreoRoleName
            };

            var crReader = new CustomerRole
            {
                Name = "Lector",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.ReaderRoleName
            };

            var crAuthor = new CustomerRole
            {
                Name = "Autor",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.AuthorRoleName
            };

            var crApprover = new CustomerRole
            {
                Name = "Aprobador",
                Active = true,
                IsSystemRole = true,
                SystemName = NopCustomerDefaults.ApproverRoleName
            };

            var customerRoles = new List<CustomerRole>
            {
                crSuperAdministrators,
                crAccess,
                crAdministrators,
                crUsuario,
                crVendors,
                crReader,
                crAuthor,
                crApprover
            };

            await InsertInstallationDataAsync(customerRoles);

            //default store 
            var defaultStore = await _storeRepository.Table.FirstOrDefaultAsync();

            if (defaultStore == null)
                throw new Exception("No default store could be loaded");

            var storeId = defaultStore.Id;

            //admin user
            var adminUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            var defaultAdminUserAddress = await InsertInstallationDataAsync(
                new Address
                {
                    FirstName = "Juan",
                    LastName = "Rodriguez",
                    PhoneNumber = "12345678",
                    Email = defaultUserEmail,
                    FaxNumber = string.Empty,
                    Company = "SAT",
                    Address1 = "Bahía de Santa Bárbara núm. 23",
                    Address2 = string.Empty,
                    City = "Ciudad de México",
                    StateProvinceId = _stateProvinceRepository.Table.FirstOrDefault(sp => sp.Name == "Distrito Federal")?.Id,
                    CountryId = _countryRepository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == "MEX")?.Id,
                    ZipPostalCode = "06030",
                    CreatedOnUtc = DateTime.UtcNow
                });

            adminUser.BillingAddressId = defaultAdminUserAddress.Id;
            adminUser.ShippingAddressId = defaultAdminUserAddress.Id;

            await InsertInstallationDataAsync(adminUser);

            await InsertInstallationDataAsync(new CustomerAddressMapping { CustomerId = adminUser.Id, AddressId = defaultAdminUserAddress.Id });

            await InsertInstallationDataAsync(
                new CustomerCustomerRoleMapping { CustomerId = adminUser.Id, CustomerRoleId = crSuperAdministrators.Id },
                new CustomerCustomerRoleMapping { CustomerId = adminUser.Id, CustomerRoleId = crAccess.Id });

            //set default customer name
            await InsertInstallationDataAsync(new GenericAttribute
            {
                EntityId = adminUser.Id,
                Key = NopCustomerDefaults.FirstNameAttribute,
                KeyGroup = nameof(Customer),
                Value = "John",
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            },
            new GenericAttribute
            {
                EntityId = adminUser.Id,
                Key = NopCustomerDefaults.LastNameAttribute,
                KeyGroup = nameof(Customer),
                Value = "Smith",
                StoreId = 0,
                CreatedOrUpdatedDateUTC = DateTime.UtcNow
            });

            //set hashed admin password
            var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            await customerRegistrationService.ChangePasswordAsync(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword, null, NopCustomerServicesDefaults.DefaultHashedPasswordFormat));

            //search engine (crawler) built-in user
            var searchEngineUser = new Customer
            {
                Email = "builtin@search_engine_record.com",
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "Built-in system guest record used for requests from search engines.",
                Active = true,
                IsSystemAccount = true,
                SystemName = NopCustomerDefaults.SearchEngineCustomerName,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            await InsertInstallationDataAsync(searchEngineUser);

            //built-in user for background tasks
            var backgroundTaskUser = new Customer
            {
                Email = "builtin@background-task-record.com",
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "Built-in system record used for background tasks.",
                Active = true,
                IsSystemAccount = true,
                SystemName = NopCustomerDefaults.BackgroundTaskCustomerName,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            await InsertInstallationDataAsync(backgroundTaskUser);

        }

        
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallMessageTemplatesAsync()
        {
            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");

            var messageTemplates = new List<MessageTemplate>
            {
                
                
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerPasswordRecoveryMessage,
                    Subject = "Recuperación de Password",
                    Body = $"<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}To change your password <a href=\"%Customer.PasswordRecoveryURL%\">click here</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Store.Name%{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.CustomerWelcomeMessage,
                    Subject = "Bienvenido %Store.Name%",
                    Body = $"We welcome you to <a href=\"%Store.URL%\"> %Store.Name%</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}You can now take part in the various services we have to offer you. Some of these services include:{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.{Environment.NewLine}<br />{Environment.NewLine}Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.{Environment.NewLine}<br />{Environment.NewLine}Order History - View your history of purchases that you have made with us.{Environment.NewLine}<br />{Environment.NewLine}Products Reviews - Share your opinions on products with our other customers.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}For help with any of our online services, please email the store-owner: <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Note: This email address was provided on our registration page. If you own the email and did not register on our site, please send an email to <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RI_UPDATE_VERSION,
                    Subject = "Repositorio de Información - %Ri.File.Name% Nueva Versión",
                    Body = $"<p>{Environment.NewLine}<b>Repositorio de Información</a>{Environment.NewLine}<b/>{Environment.NewLine}<br />{Environment.NewLine}El archivo %Ri.File.Name% tiene una nueva versión.{Environment.NewLine}<br />{Environment.NewLine}Propietario: %Ri.Modifier.Name%{Environment.NewLine}<br />{Environment.NewLine}Nombre de Archivo: %Ri.File.Name%{Environment.NewLine}<br />{Environment.NewLine}Versión: %Ri.File.Version%{Environment.NewLine}<br />{Environment.NewLine}Actualización: %Ri.File.CreatedDate%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RI_PUBLISHED_VERSION,
                    Subject = "Repositorio de Información - Publicación del archivo %Ri.File.Name% Versión %Ri.File.Version%",
                    Body = $"<p>{Environment.NewLine}<b>Repositorio de Información</b>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Se ha publicado la versión %Ri.File.Version% del archivo %Ri.File.Name%.{Environment.NewLine}<br />{Environment.NewLine}Autor: %Ri.Modifier.Name%{Environment.NewLine}<br />{Environment.NewLine}Nombre de Archivo: %Ri.File.Name%{Environment.NewLine}<br />{Environment.NewLine}Versión: %Ri.File.Version%{Environment.NewLine}<br />{Environment.NewLine}Actualización: %Ri.File.UpdateDate%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RI_AUTHORIZED_VERSION,
                    Subject = "Repositorio de Información - Autorización del archivo %Ri.File.Name% Versión %Ri.File.Version%",
                    Body = $"<p>{Environment.NewLine}<b>Repositorio de Información</b>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Se ha autorizado la versión %Ri.File.Version% del archivo %Ri.File.Name%.{Environment.NewLine}<br />{Environment.NewLine}Aprobador: %Ri.Modifier.Name%{Environment.NewLine}<br />{Environment.NewLine}Nombre de Archivo: %Ri.File.Name%{Environment.NewLine}<br />{Environment.NewLine}Versión: %Ri.File.Version%{Environment.NewLine}<br />{Environment.NewLine}Actualización: %Ri.File.UpdateDate%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RI_REJECTED_VERSION,
                    Subject = "Repositorio de Información - Rechazo del archivo %Ri.File.Name% Versión %Ri.File.Version%",
                    Body = $"<p>{Environment.NewLine}<b>Repositorio de Información</b>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Se ha rechazado la versión %Ri.File.Version% del archivo %Ri.File.Name%.{Environment.NewLine}<br />{Environment.NewLine}Aprobador: %Ri.Modifier.Name%{Environment.NewLine}<br />{Environment.NewLine}Nombre de Archivo: %Ri.File.Name%{Environment.NewLine}<br />{Environment.NewLine}Versión: %Ri.File.Version%{Environment.NewLine}<br />{Environment.NewLine}Actualización: %Ri.File.UpdateDate%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                },
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.RI_DELETE_FILE,
                    Subject = "Repositorio de Información - Eliminación del archivo %Ri.File.Name% Versión %Ri.File.Version%",
                    Body = $"<p>{Environment.NewLine}<b>Repositorio de Información</b>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Se ha eliminado la versión %Ri.File.Version% del archivo %Ri.File.Name%.{Environment.NewLine}<br />{Environment.NewLine}Eliminó: %Ri.Modifier.Name%{Environment.NewLine}<br />{Environment.NewLine}Nombre de Archivo: %Ri.File.Name%{Environment.NewLine}<br />{Environment.NewLine}Actualización: %Ri.File.DeletedDate%{Environment.NewLine}<br />{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                }
            };

            await InsertInstallationDataAsync(messageTemplates);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallTopicsAsync()
        {
            var defaultTopicTemplate =
                _topicTemplateRepository.Table.FirstOrDefault(tt => tt.Name == "Default template");
            if (defaultTopicTemplate == null)
                throw new Exception("Topic template cannot be loaded");

            var topics = new List<Topic>
            {
               
                new Topic
                {
                    SystemName = "LoginRegistrationInfo",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = "About login / registration",
                    Body =
                        "<p>Put your login / registration information here. You can edit this in the admin site.</p>",
                    TopicTemplateId = defaultTopicTemplate.Id
                },
                new Topic
                {
                    SystemName = "PageNotFound",
                    IncludeInSitemap = false,
                    IsPasswordProtected = false,
                    DisplayOrder = 1,
                    Published = true,
                    Title = string.Empty,
                    Body =
                        "<p><strong>The page you requested was not found, and we have a fine guess why.</strong></p><ul><li>If you typed the URL directly, please make sure the spelling is correct.</li><li>The page no longer exists. In this case, we profusely apologize for the inconvenience and for any damage this may cause.</li></ul>",
                    TopicTemplateId = defaultTopicTemplate.Id
                }
            };

            await InsertInstallationDataAsync(topics);

            //search engine names
            foreach (var topic in topics)
            {
                await InsertInstallationDataAsync(new UrlRecord
                {
                    EntityId = topic.Id,
                    EntityName = nameof(Topic),
                    LanguageId = 0,
                    IsActive = true,
                    Slug = await ValidateSeNameAsync(topic, !string.IsNullOrEmpty(topic.Title) ? topic.Title : topic.SystemName)
                });
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallSettingsAsync(RegionInfo regionInfo)
        {
            var isMetric = regionInfo?.IsMetric ?? false;
            var country = regionInfo?.TwoLetterISORegionName ?? string.Empty;
            var isGermany = country == "DE";
            var isEurope = ISO3166.FromCountryCode(country)?.SubjectToVat ?? false;

            var settingService = EngineContext.Current.Resolve<ISettingService>();
            await settingService.SaveSettingAsync(new PdfSettings
            {
                LogoPictureId = 0,
                LetterPageSizeEnabled = false,
                RenderOrderNotes = true,
                FontFileName = "FreeSerif.ttf",
                InvoiceFooterTextColumn1 = null,
                InvoiceFooterTextColumn2 = null
            });


            await settingService.SaveSettingAsync(new SitemapXmlSettings
            {
                SitemapXmlEnabled = true,
                SitemapXmlIncludeBlogPosts = true,
                SitemapXmlIncludeCategories = true,
                SitemapXmlIncludeManufacturers = true,
                SitemapXmlIncludeNews = true,
                SitemapXmlIncludeProducts = true,
                SitemapXmlIncludeProductTags = true,
                SitemapXmlIncludeCustomUrls = true,
                SitemapXmlIncludeTopics = true
            });

            await settingService.SaveSettingAsync(new CommonSettings
            {
                UseSystemEmailForContactUsForm = true,

                DisplayJavaScriptDisabledWarning = false,
                Log404Errors = true,
                BreadcrumbDelimiter = "/",
                BbcodeEditorOpenLinksInNewWindow = false,
                PopupForTermsOfServiceLinks = true,
                JqueryMigrateScriptLoggingActive = false,
                UseResponseCompression = true,
                FaviconAndAppIconsHeadCode = "<link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"/icons/icons_0/apple-touch-icon.png\"><link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"/icons/icons_0/favicon-32x32.png\"><link rel=\"icon\" type=\"image/png\" sizes=\"192x192\" href=\"/icons/icons_0/android-chrome-192x192.png\"><link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"/icons/icons_0/favicon-16x16.png\"><link rel=\"manifest\" href=\"/icons/icons_0/site.webmanifest\"><link rel=\"mask-icon\" href=\"/icons/icons_0/safari-pinned-tab.svg\" color=\"#5bbad5\"><link rel=\"shortcut icon\" href=\"/icons/icons_0/favicon.ico\"><meta name=\"msapplication-TileColor\" content=\"#2d89ef\"><meta name=\"msapplication-TileImage\" content=\"/icons/icons_0/mstile-144x144.png\"><meta name=\"msapplication-config\" content=\"/icons/icons_0/browserconfig.xml\"><meta name=\"theme-color\" content=\"#ffffff\">",
                EnableHtmlMinification = true,
                RestartTimeout = NopCommonDefaults.RestartTimeout
            });

            await settingService.SaveSettingAsync(new SeoSettings
            {
                PageTitleSeparator = ". ",
                PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                HomepageTitle = "Portal Único de Visualización",
                HomepageDescription = "SAT PUV",
                DefaultTitle = "PUV",
                DefaultMetaKeywords = string.Empty,
                DefaultMetaDescription = string.Empty,
                GenerateProductMetaDescription = true,
                ConvertNonWesternChars = false,
                AllowUnicodeCharsInUrls = true,
                CanonicalUrlsEnabled = false,
                QueryStringInCanonicalUrlsEnabled = false,
                WwwRequirement = WwwRequirement.NoMatter,
                TwitterMetaTags = true,
                OpenGraphMetaTags = true,
                MicrodataEnabled = true,
                ReservedUrlRecordSlugs = NopSeoDefaults.ReservedUrlRecordSlugs,
                CustomHeadTags = string.Empty
            });

            await settingService.SaveSettingAsync(new AdminAreaSettings
            {
                DefaultGridPageSize = 15,
                PopupGridPageSize = 7,
                GridPageSizes = "7, 15, 20, 50, 100",
                RichEditorAdditionalSettings = null,
                RichEditorAllowJavaScript = false,
                RichEditorAllowStyleTag = false,
                UseRichEditorForCustomerEmails = false,
                UseRichEditorInMessageTemplates = false,
                CheckCopyrightRemovalKey = true,
                UseIsoDateFormatInJsonResult = true,
                ShowDocumentationReferenceLinks = true
            });

            await settingService.SaveSettingAsync(new GdprSettings
            {
                GdprEnabled = false,
                LogPrivacyPolicyConsent = true,
                LogNewsletterConsent = true,
                LogUserProfileChanges = true
            });

            //await settingService.SaveSettingAsync(new CatalogSettings
            //{
            //    AllowViewUnpublishedProductPage = true,
            //    DisplayDiscontinuedMessageForUnpublishedProducts = true,
            //    PublishBackProductWhenCancellingOrders = false,
            //    ShowSkuOnProductDetailsPage = true,
            //    ShowSkuOnCatalogPages = false,
            //    ShowManufacturerPartNumber = false,
            //    ShowGtin = false,
            //    ShowFreeShippingNotification = true,
            //    AllowProductSorting = true,
            //    AllowProductViewModeChanging = true,
            //    DefaultViewMode = "grid",
            //    ShowProductsFromSubcategories = false,
            //    ShowCategoryProductNumber = false,
            //    ShowCategoryProductNumberIncludingSubcategories = false,
            //    CategoryBreadcrumbEnabled = true,
            //    ShowShareButton = true,
            //    PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style \"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script src=\"http://s7.addthis.com/js/250/addthis_widget.js#pubid=nopsolutions\"></script><!-- AddThis Button END -->",
            //    ProductReviewsMustBeApproved = false,
            //    OneReviewPerProductFromCustomer = false,
            //    DefaultProductRatingValue = 5,
            //    AllowAnonymousUsersToReviewProduct = false,
            //    ProductReviewPossibleOnlyAfterPurchasing = false,
            //    NotifyStoreOwnerAboutNewProductReviews = false,
            //    NotifyCustomerAboutProductReviewReply = false,
            //    EmailAFriendEnabled = true,
            //    AllowAnonymousUsersToEmailAFriend = false,
            //    RecentlyViewedProductsNumber = 3,
            //    RecentlyViewedProductsEnabled = true,
            //    NewProductsNumber = 6,
            //    NewProductsEnabled = true,
            //    CompareProductsEnabled = true,
            //    CompareProductsNumber = 4,
            //    ProductSearchAutoCompleteEnabled = true,
            //    ProductSearchEnabled = true,
            //    ProductSearchAutoCompleteNumberOfProducts = 10,
            //    ShowLinkToAllResultInSearchAutoComplete = false,
            //    ProductSearchTermMinimumLength = 3,
            //    ShowProductImagesInSearchAutoComplete = false,
            //    ShowBestsellersOnHomepage = false,
            //    NumberOfBestsellersOnHomepage = 4,
            //    SearchPageProductsPerPage = 6,
            //    SearchPageAllowCustomersToSelectPageSize = true,
            //    SearchPagePageSizeOptions = "6, 3, 9, 18",
            //    SearchPagePriceRangeFiltering = true,
            //    SearchPageManuallyPriceRange = true,
            //    ProductsAlsoPurchasedEnabled = true,
            //    ProductsAlsoPurchasedNumber = 4,
            //    AjaxProcessAttributeChange = true,
            //    NumberOfProductTags = 15,
            //    ProductsByTagPageSize = 6,
            //    IncludeShortDescriptionInCompareProducts = false,
            //    IncludeFullDescriptionInCompareProducts = false,
            //    IncludeFeaturedProductsInNormalLists = false,
            //    UseLinksInRequiredProductWarnings = true,
            //    DisplayTierPricesWithDiscounts = true,
            //    IgnoreDiscounts = false,
            //    IgnoreFeaturedProducts = false,
            //    IgnoreAcl = true,
            //    IgnoreStoreLimitations = true,
            //    CacheProductPrices = false,
            //    ProductsByTagAllowCustomersToSelectPageSize = true,
            //    ProductsByTagPageSizeOptions = "6, 3, 9, 18",
            //    ProductsByTagPriceRangeFiltering = true,
            //    ProductsByTagManuallyPriceRange = true,
            //    MaximumBackInStockSubscriptions = 200,
            //    ManufacturersBlockItemsToDisplay = 2,
            //    DisplayTaxShippingInfoFooter = isGermany,
            //    DisplayTaxShippingInfoProductDetailsPage = isGermany,
            //    DisplayTaxShippingInfoProductBoxes = isGermany,
            //    DisplayTaxShippingInfoShoppingCart = isGermany,
            //    DisplayTaxShippingInfoWishlist = isGermany,
            //    DisplayTaxShippingInfoOrderDetailsPage = isGermany,
            //    DefaultCategoryPageSizeOptions = "6, 3, 9",
            //    DefaultCategoryPageSize = 6,
            //    DefaultManufacturerPageSizeOptions = "6, 3, 9",
            //    DefaultManufacturerPageSize = 6,
            //    ShowProductReviewsTabOnAccountPage = true,
            //    ProductReviewsPageSizeOnAccountPage = 10,
            //    ProductReviewsSortByCreatedDateAscending = false,
            //    ExportImportProductAttributes = true,
            //    ExportImportProductSpecificationAttributes = true,
            //    ExportImportUseDropdownlistsForAssociatedEntities = true,
            //    ExportImportProductsCountInOneFile = 500,
            //    ExportImportSplitProductsFile = false,
            //    ExportImportRelatedEntitiesByName = true,
            //    CountDisplayedYearsDatePicker = 1,
            //    UseAjaxLoadMenu = false,
            //    UseAjaxCatalogProductsLoading = true,
            //    EnableManufacturerFiltering = true,
            //    EnablePriceRangeFiltering = true,
            //    EnableSpecificationAttributeFiltering = true,
            //});

            await settingService.SaveSettingAsync(new LocalizationSettings
            {
                DefaultAdminLanguageId = _languageRepository.Table.Single(l => l.LanguageCulture == NopCommonDefaults.DefaultLanguageCulture).Id,
                UseImagesForLanguageSelection = false,
                SeoFriendlyUrlsForLanguagesEnabled = false,
                AutomaticallyDetectLanguage = false,
                LoadAllLocaleRecordsOnStartup = true,
                LoadAllLocalizedPropertiesOnStartup = true,
                LoadAllUrlRecordsOnStartup = false,
                IgnoreRtlPropertyForAdminArea = false
            });

            await settingService.SaveSettingAsync(new CustomerSettings
            {
                UsernamesEnabled = true,
                CheckUsernameAvailabilityEnabled = true,
                AllowUsersToChangeUsernames = false,
                DefaultPasswordFormat = PasswordFormat.Hashed,
                HashedPasswordFormat = NopCustomerServicesDefaults.DefaultHashedPasswordFormat,
                PasswordMinLength = 8,
                PasswordRequireDigit = true,
                PasswordRequireLowercase = true,
                PasswordRequireNonAlphanumeric = true,
                PasswordRequireUppercase = true,
                UnduplicatedPasswordsNumber = 4,
                PasswordRecoveryLinkDaysValid = 7,
                PasswordLifetime = 90,
                FailedPasswordAllowedAttempts = 0,
                FailedPasswordLockoutMinutes = 30,
                UserRegistrationType = UserRegistrationType.Disabled,
                AllowCustomersToUploadAvatars = false,
                AvatarMaximumSizeBytes = 20000,
                DefaultAvatarEnabled = true,
                ShowCustomersLocation = false,
                ShowCustomersJoinDate = false,
                AllowViewingProfiles = false,
                NotifyNewCustomerRegistration = false,
                HideDownloadableProductsTab = false,
                HideBackInStockSubscriptionsTab = false,
                DownloadableProductsValidateUser = false,
                CustomerNameFormat = CustomerNameFormat.ShowFirstName,
                FirstNameEnabled = true,
                FirstNameRequired = true,
                LastNameEnabled = true,
                LastNameRequired = true,
                GenderEnabled = false,
                DateOfBirthEnabled = false,
                DateOfBirthRequired = false,
                DateOfBirthMinimumAge = null,
                CompanyEnabled = false,
                StreetAddressEnabled = false,
                StreetAddress2Enabled = false,
                ZipPostalCodeEnabled = false,
                CityEnabled = false,
                CountyEnabled = false,
                CountyRequired = false,
                CountryEnabled = false,
                CountryRequired = false,
                StateProvinceEnabled = false,
                StateProvinceRequired = false,
                PhoneEnabled = false,
                FaxEnabled = false,
                AcceptPrivacyPolicyEnabled = false,
                NewsletterEnabled = true,
                NewsletterTickedByDefault = false,
                HideNewsletterBlock = false,
                NewsletterBlockAllowToUnsubscribe = false,
                OnlineCustomerMinutes = 20,
                StoreLastVisitedPage = false,
                StoreIpAddresses = true,
                LastActivityMinutes = 15,
                SuffixDeletedCustomers = false,
                EnteringEmailTwice = false,
                RequireRegistrationForDownloadableProducts = false,
                AllowCustomersToCheckGiftCardBalance = false,
                DeleteGuestTaskOlderThanMinutes = 1440,
                PhoneNumberValidationEnabled = false,
                PhoneNumberValidationUseRegex = false,
                PhoneNumberValidationRule = "^[0-9]{1,14}?$"
            });

            await settingService.SaveSettingAsync(new MultiFactorAuthenticationSettings
            {
                ForceMultifactorAuthentication = false
            });

            //await settingService.SaveSettingAsync(new AddressSettings
            //{
            //    CompanyEnabled = false,
            //    StreetAddressEnabled = true,
            //    StreetAddressRequired = true,
            //    StreetAddress2Enabled = true,
            //    ZipPostalCodeEnabled = true,
            //    ZipPostalCodeRequired = true,
            //    CityEnabled = true,
            //    CityRequired = true,
            //    CountyEnabled = false,
            //    CountyRequired = false,
            //    CountryEnabled = true,
            //    StateProvinceEnabled = true,
            //    PhoneEnabled = true,
            //    PhoneRequired = true,
            //    FaxEnabled = true
            //});

            //await settingService.SaveSettingAsync(new MediaSettings
            //{
            //    AvatarPictureSize = 120,
            //    ProductThumbPictureSize = 415,
            //    ProductDetailsPictureSize = 550,
            //    ProductThumbPictureSizeOnProductDetailsPage = 100,
            //    AssociatedProductPictureSize = 220,
            //    CategoryThumbPictureSize = 450,
            //    ManufacturerThumbPictureSize = 420,
            //    VendorThumbPictureSize = 450,
            //    CartThumbPictureSize = 80,
            //    MiniCartThumbPictureSize = 70,
            //    AutoCompleteSearchThumbPictureSize = 20,
            //    ImageSquarePictureSize = 32,
            //    MaximumImageSize = 1980,
            //    DefaultPictureZoomEnabled = false,
            //    DefaultImageQuality = 80,
            //    MultipleThumbDirectories = false,
            //    ImportProductImagesUsingHash = true,
            //    AzureCacheControlHeader = string.Empty,
            //    UseAbsoluteImagePath = true
            //});

            await settingService.SaveSettingAsync(new StoreInformationSettings
            {
                StoreClosed = false,
                DefaultStoreTheme = "DefaultClean",
                AllowCustomerToSelectTheme = false,
                DisplayEuCookieLawWarning = isEurope,
                FacebookLink = "",
                TwitterLink = "",
                YoutubeLink = "",
                HidePoweredByNopCommerce = true
            });

            await settingService.SaveSettingAsync(new ExternalAuthenticationSettings
            {
                RequireEmailValidation = false,
                LogErrors = false,
                AllowCustomersToRemoveAssociations = true
            });

            //await settingService.SaveSettingAsync(new RewardPointsSettings
            //{
            //    Enabled = true,
            //    ExchangeRate = 1,
            //    PointsForRegistration = 0,
            //    RegistrationPointsValidity = 30,
            //    PointsForPurchases_Amount = 10,
            //    PointsForPurchases_Points = 1,
            //    MinOrderTotalToAwardPoints = 0,
            //    MaximumRewardPointsToUsePerOrder = 0,
            //    MaximumRedeemedRate = 0,
            //    PurchasesPointsValidity = 45,
            //    ActivationDelay = 0,
            //    ActivationDelayPeriodId = 0,
            //    DisplayHowMuchWillBeEarned = true,
            //    PointsAccumulatedForAllStores = true,
            //    PageSize = 10
            //});

            await settingService.SaveSettingAsync(new MessageTemplatesSettings
            {
                CaseInvariantReplacement = false,
                Color1 = "#b9babe",
                Color2 = "#ebecee",
                Color3 = "#dde2e6"
            });

            
            await settingService.SaveSettingAsync(new SecuritySettings
            {
                EncryptionKey = CommonHelper.GenerateRandomDigitCode(16),
                AdminAreaAllowedIpAddresses = null,
                HoneypotEnabled = false,
                HoneypotInputName = "hpinput",
                AllowNonAsciiCharactersInHeaders = true
            });

            

            await settingService.SaveSettingAsync(new DateTimeSettings
            {
                DefaultStoreTimeZoneId = string.Empty,
                AllowCustomersToSetTimeZone = false
            });

            var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
            if (eaGeneral == null)
                throw new Exception("Default email account cannot be loaded");
            await settingService.SaveSettingAsync(new EmailAccountSettings
            {
                DefaultEmailAccountId = eaGeneral.Id
            });

            await settingService.SaveSettingAsync(new WidgetSettings
            {
                ActiveWidgetSystemNames = new List<string> { "Widgets.NivoSlider" }
            });

            //await settingService.SaveSettingAsync(new DisplayDefaultMenuItemSettings
            //{
            //    DisplayHomepageMenuItem = true,
            //    DisplayNewProductsMenuItem = true,
            //    DisplayProductSearchMenuItem = true,
            //    DisplayCustomerInfoMenuItem = true,
            //    DisplayBlogMenuItem = true,
            //    DisplayForumsMenuItem = true,
            //    DisplayContactUsMenuItem = true
            //});

            //await settingService.SaveSettingAsync(new DisplayDefaultFooterItemSettings
            //{
            //    DisplaySitemapFooterItem = true,
            //    DisplayContactUsFooterItem = true,
            //    DisplayProductSearchFooterItem = true,
            //    DisplayNewsFooterItem = true,
            //    DisplayBlogFooterItem = true,
            //    DisplayForumsFooterItem = true,
            //    DisplayRecentlyViewedProductsFooterItem = true,
            //    DisplayCompareProductsFooterItem = true,
            //    DisplayNewProductsFooterItem = true,
            //    DisplayCustomerInfoFooterItem = true,
            //    DisplayCustomerOrdersFooterItem = true,
            //    DisplayCustomerAddressesFooterItem = true,
            //    DisplayShoppingCartFooterItem = true,
            //    DisplayWishlistFooterItem = true,
            //    DisplayApplyVendorAccountFooterItem = true
            //});

            await settingService.SaveSettingAsync(new CaptchaSettings
            {
                ReCaptchaApiUrl = "https://www.google.com/recaptcha/",
                ReCaptchaDefaultLanguage = string.Empty,
                ReCaptchaPrivateKey = string.Empty,
                ReCaptchaPublicKey = string.Empty,
                ReCaptchaRequestTimeout = 20,
                ReCaptchaTheme = string.Empty,
                AutomaticallyChooseLanguage = true,
                Enabled = false,
                CaptchaType = CaptchaType.CheckBoxReCaptchaV2,
                ReCaptchaV3ScoreThreshold = 0.5M,
                ShowOnApplyVendorPage = false,
                ShowOnBlogCommentPage = false,
                ShowOnContactUsPage = false,
                ShowOnEmailProductToFriendPage = false,
                ShowOnEmailWishlistToFriendPage = false,
                ShowOnForgotPasswordPage = false,
                ShowOnLoginPage = false,
                ShowOnNewsCommentPage = false,
                ShowOnProductReviewPage = false,
                ShowOnRegistrationPage = false,
            });

            await settingService.SaveSettingAsync(new MessagesSettings
            {
                UsePopupNotifications = false
            });

            await settingService.SaveSettingAsync(new ProxySettings
            {
                Enabled = false,
                Address = string.Empty,
                Port = string.Empty,
                Username = string.Empty,
                Password = string.Empty,
                BypassOnLocal = true,
                PreAuthenticate = true
            });

            await settingService.SaveSettingAsync(new CookieSettings
            {
                CompareProductsCookieExpires = 24 * 10,
                RecentlyViewedProductsCookieExpires = 24 * 10,
                CustomerCookieExpires = 24 * 365
            });
        }

        protected virtual async Task InstallComponentSettingsAsync()
        {
            var settingService = EngineContext.Current.Resolve<IComponentSettingService>();
            await settingService.SaveSettingAsync(new CiscoSettings
            {
                SdWan_Password = "RG!_Yw919_83",
                SdWan_User = "devnetuser",
                SdWan_Url = "https://sandbox-sdwan-2.cisco.com"
            });
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallTopicTemplatesAsync()
        {
            var topicTemplates = new List<TopicTemplate>
            {
                new TopicTemplate
                {
                    Name = "Default template",
                    ViewPath = "TopicDetails",
                    DisplayOrder = 1
                }
            };

            await InsertInstallationDataAsync(topicTemplates);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task InstallScheduleTasksAsync()
        {
            var lastEnabledUtc = DateTime.UtcNow;
            var tasks = new List<ScheduleTask>
            {
                new ScheduleTask
                {
                    Name = "Send emails",
                    Seconds = 60,
                    Type = "Nop.Services.Messages.QueuedMessagesSendTask, Nop.Services",
                    Enabled = true,
                    LastEnabledUtc = lastEnabledUtc,
                    StopOnError = false
                },
                new ScheduleTask
                {
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = "Nop.Services.Common.KeepAliveTask, Nop.Services",
                    Enabled = true,
                    LastEnabledUtc = lastEnabledUtc,
                    StopOnError = false
                },
                new ScheduleTask
                {
                    Name = "Delete guests",
                    Seconds = 600,
                    Type = "Nop.Services.Customers.DeleteGuestsTask, Nop.Services",
                    Enabled = true,
                    LastEnabledUtc = lastEnabledUtc,
                    StopOnError = false
                },
                new ScheduleTask
                {
                    Name = "Clear cache",
                    Seconds = 600,
                    Type = "Nop.Services.Caching.ClearCacheTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false
                },
                new ScheduleTask
                {
                    Name = "Clear log",
                    //60 minutes
                    Seconds = 3600,
                    Type = "Nop.Services.Logging.ClearLogTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false
                }
            };

            await InsertInstallationDataAsync(tasks);
        }

        #endregion
        #region Install Custom

        public virtual async Task InstallLanguageSpanishAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["account.login.wrongcredentials.customernotexist"] = "No se encontró ningún registro de usuario.",
                ["Account.Fields.Username.Rule"] = "Solo se aceptan letras y números",
                ["Account.Fields.Username.Min"] = "Debe ser mayor o igual que 8 caracteres.",
                ["Account.Login.Title"] = "Portal Único de Visualización",
                ["Account.Login.LoginButton"] = "Ingresar",
                ["Admin.Common.Close"] = "Cerrar",
                ["Admin.Common.Field.Required"] = "El campo {0} es requerido",
                ["Admin.Customers.Users"] = "Usuarios",
                ["Admin.Customers.Customers"] = "Usuarios",
                ["Admin.Customers.Customers.Info"] = "Información del usuario",
                ["Admin.Customers.Customers.List.CustomerRoles"] = "Perfiles de usuario",
                ["Admin.Customers.Customers.Fields.CustomerRoles"] = "Perfiles de usuario",
                ["Admin.Customers.Customers.AddNew"] = "Agregar un nuevo usuario",
                ["Admin.Customers.Customers.EditCustomerDetails"] = "Editar los detalles del usuario",
                ["Admin.Customers.Customers.BackToList"] = "volver a la lista de usuarios",
                ["Admin.Customers.UsersRoles"] = "Perfiles de usuario",
                ["Admin.Customers.CustomerRoles"] = "Perfiles de usuario",
                ["Admin.Customers.CustomerRoles.AddNew"] = "Agregar un nuevo perfil de usuario",
                ["Admin.Customers.CustomerRoles.BackToList"] = "volver a la lista de perfiles de usuario",
                ["Admin.Customers.CustomerRoles.EditCustomerRoleDetails"] = "Editar los detalles del perfiles de usuario",
                ["Admin.Customers.OnlineCustomers"] = "Usuarios en línea",
                ["Admin.Customers.OnlineCustomers.Fields.CustomerInfo"] = "Información del usuario",
                ["Admin.DT.Info"] = "_START_-_END_ de _TOTAL_ elementos",
                ["SAT.PUV.Dashboard"] = "Inicio",
                ["SAT.PUV.Servicedesk"] = "Mesa de servicio",
                ["SAT.PUV.Servicemonitoring"] = "Monitoreo de los servicios",
                ["SAT.PUV.ServiceMonitoring.Dashboard"] = "Dashboard",
                ["SAT.PUV.ServiceMonitoring.Geography"] = "Geografía",
                ["SAT.PUV.Securitydisplay"] = "Visualización de la seguridad",
                ["SAT.PUV.Onlinereports"] = "Reportes en línea",
                ["SAT.PUV.Informationrepository"] = "Repositorio de información",
                ["SAT.PUV.Informationrepository.Public"] = "Repositorio público",
                ["SAT.PUV.Informationrepository.Personal"] = "Repositorio personal",
                ["Admin.Informationrepository.NoDirectoriesExist"] = "No hay carpetas",
                ["Admin.Informationrepository.NoFilesUploaded"] = "No hay archivos cargados",
                ["Admin.Informationrepository.DragDropFiles"] = "Arrastre sus archivos o seleccionelos aquí",
                ["Admin.Informationrepository.Index"] = "Mis documentos",
                ["Admin.Informationrepository.WorkingFiles"] = "Archivos en seguimiento",
                ["Admin.Informationrepository.Folders"] = "Carpetas",
                ["Admin.Informationrepository.Files"] = "Archivos",
                ["Admin.Informationrepository.FilesToAuth"] = "Archivos por autorizar",
                ["Admin.Informationrepository.Author"] = "Autor",
                ["Admin.Informationrepository.PublishStatus"] = "Publicado",
                ["Admin.Informationrepository.NotPublishedStatus"] = "No publicado",
                ["Admin.Informationrepository.AuthorizeStatus"] = "Autorizado",
                ["Admin.Informationrepository.NotAuthorizeStatus"] = "No autorizado",
                ["Admin.Informationrepository.RejectedStatus"] = "Rechazado",
                ["Admin.Informationrepository.CreateFolder"] = "Crear carpeta",
                ["Admin.Informationrepository.CreateFolder.Fields.Name"] = "Nombre de la carpeta",
                ["Admin.Informationrepository.UpdatedFolder.Fields.Name.Exist"] = "El nombre de la carpeta ya existe.",
                ["Admin.Informationrepository.UploadFile"] = "Cargar archivo",
                ["Admin.Informationrepository.UploadFile.OwnerName"] = "Autor",
                ["Admin.Informationrepository.UploadFile.ParentFolderName"] = "Ubicación del archivo",
                ["Admin.Informationrepository.UploadFile.FileName"] = "Nombre del archivo",
                ["Admin.Informationrepository.UploadFile.FileType"] = "Tipo",
                ["Admin.Informationrepository.UploadFile.Size"] = "Tamaño",
                ["Admin.Informationrepository.UploadFile.Version"] = "Versión",
                ["Admin.Informationrepository.UploadFile.CreatedOn"] = "Creado",
                ["Admin.Informationrepository.UploadFile.UpdatedOn"] = "Modificado",
                ["Admin.Informationrepository.UploadFile.Tags"] = "Tags",
                ["Admin.Informationrepository.UploadFile.Publish"] = "Publicar",
                ["Admin.Informationrepository.UploadFile.Auth"] = "Autorizar",
                ["Admin.Informationrepository.UploadFile.AuthQuestion"] = "¿Está seguro que desea autorizar el archivo?",
                ["Admin.Informationrepository.UploadFile.Reject"] = "Rechazar",
                ["Admin.Informationrepository.UploadFile.RejectQuestion"] = "¿Está seguro que desea rechazar el archivo?",
                ["Admin.Informationrepository.UploadFile.FileStatus"] = "Estatus de archivo",
                ["common.fileuploader.details"] = "Detalles",
                ["common.fileuploader.copy"] = "Copiar",
                ["common.fileuploader.move"] = "Mover",
                ["common.fileuploader.rename"] = "Renombrar",
                ["common.fileuploader.version"] = "Versión",
                ["Admin.ServiceDesk.Remedy.Fields.TicketNumber"] = "Número de Ticket",
                ["Admin.ServiceDesk.Remedy.Fields.SatNumber"] = "Número de SAT",
                ["Admin.ServiceDesk.Remedy.Fields.ReportType"] = "Tipo de Reporte",
                ["Admin.ServiceDesk.Remedy.Fields.EventType"] = "Tipo de Evento",
                ["Admin.ServiceDesk.Remedy.Fields.CauseEvent"] = "Causa del Evento",
                ["Admin.ServiceDesk.Remedy.Fields.Status"] = "Estatus",
                ["Admin.ServiceDesk.Remedy.Fields.CreatedOn"] = "Fecha de Creación",
                ["Admin.ServiceDesk.Remedy.Fields.CreatedOnFrom"] = "Creado a partir de",
                ["Admin.ServiceDesk.Remedy.Fields.CreatedOnTo"] = "Creado hasta",
                ["Admin.ServiceDesk.Remedy.Fields.EventDate"] = "Fecha del Evento",
                ["SAT.PUV.Users"] = "Usuarios",
                ["SAT.PUV.Customers.Users"] = "Administración",
                ["SAT.PUV.Customers.UsersRoles"] = "Perfiles",
                ["SAT.PUV.Customers.OnlineUsers"] = "Usuarios Conectados",
                ["SAT.PUV.Configuration.Instances"] = "Instancias",
                ["SAT.PUV.Configuration.ACL"] = "Lista de control de acceso",
                ["SAT.PUV.Customers.ActivityLog"] = "Registro de actividad",
                ["SAT.PUV.Customers.ActivityLogType"] = "Tipos de actividad",
                ["Admin.Configuration.ACL.DescriptionUser"] = "Lista de control de acceso es una lista de permisos asociados a los roles del usuario. Esta lista especifica los derechos de acceso de los usuarios a los objetos.",
                ["Admin.Configuration.Settings.ComponentSettings"] = "Configuración de componentes",
                ["Admin.Configuration.Instances"] = "Instancias",
                ["Admin.Configuration.Instance.Fields.Name"] = "Nombre de la instancia",
                ["Admin.Configuration.Instance.Fields.Url"] = "URL",
                ["SAT.PUV.Servicelevel"] = "Niveles de servicio",
                ["SAT.PUV.Ipaddressmanagement"] = "Gestión de direccionamiento IP a través de DHCP",
                ["Admin.ServiceDesk.Remedy.NoRecords"] = "No hay registros seleccionados",
                ["Admin.Contentmanagement.MessageTemplates.Description.RepositorioInformacion.Authorizedversion"] = "Versión Autorizada",
                ["Admin.Customers.OnLineusers"] = "Usuarios Conectados",
                ["Admin.PageTitle"] = "Administrador PUV",
                ["SAT.PUV.ControlPanel"] = "Tablero de Control",
                ["SAT.PUV.ControlPanel.GeneralDashboard"] = "Tablero General",
                ["SAT.PUV.ControlPanel.DevicesStatus"] = "Estado de Dispositivos",
                ["SAT.PUV.ControlPanel.DevicesAlarms"] = "Alarmas de Dispositivos",
                ["SAT.PUV.ControlPanel.DevicesEvents"] = "Eventos de Dispositivos",
                ["activitylog.deletecustomerrole"] = "Eliminado un perfil de usuario (' {0} ')",
                ["activitylog.editcustomerrole"] = "Editar un perfil de usuario (' {0} ')",
                ["admin.customers.customerroles.added"] = "El nuevo perfil de usuario se ha agregado correctamente.",
                ["admin.customers.customerroles.deleted"] = "El perfil de usuario se ha eliminado correctamente.",
                ["admin.customers.customerroles.fields.name.hint"] = "El nombre del perfil de usuario.",
                ["admin.customers.customerroles.fields.systemname.hint"] = "El nombre del sistema del perfil de usuario.",
                ["admin.customers.customerroles.updated"] = "El perfil de usuario se ha actualizado correctamente.",
                ["admin.customers.customers.list.customerroles.hint"] = "Filtrar por perfil de usuario.",
                ["account.login.newcustomer"] = "Nuevo usuario.",
                ["activitylog.deletecustomerattribute"] = "Eliminado un atributo de usuario (ID = {0})",
                ["activitylog.deletecustomerattributevalue"] = "Eliminado un valor de atributo de usuario (ID = {0})",
                ["activitylog.deletecustomerrole"] = "Eliminado un perfil de usuario (' {0} ')",
                ["activitylog.editcustomer"] = "Editar un usuario (ID = {0})",
                ["activitylog.editcustomerattribute"] = "Editar un atributo de usuario(ID = { 0 })",
                ["activitylog.editcustomerattributevalue"] = "Editar un valor de atributo de usuario (ID = {0})",
                ["activitylog.editcustomerrole"] = "Editar un rol de usuario (' {0} ')",
                ["admin.configuration.settings.customeruser"] = "Configuración del usuario",
                ["admin.customers.customerroles.fields.issystemrole"] = "Es el perfil del sistema",
                ["admin.contentmanagement.messagetemplates.description.customer.emailrevalidationmessage"] = "Esta plantilla de mensaje se utiliza cuando un usuario cambia una dirección de correo electrónico en su cuenta. El usuario recibe un mensaje para confirmar una dirección de correo electrónico utilizada al cambiar la dirección de correo electrónico.",
                ["admin.contentmanagement.messagetemplates.fields.limitedtostores"] = "Limitado a las instancias",
                ["account.login.unsuccessful"] = "El usuario o contraseña son incorrectos, favor de verificar.",
                ["account.administration"] = "Regresar al Menú",
                ["Admin.Dashboard"] = "Inicio",
                ["admin.customers.customers.updated"] = "El usuariose ha actualizado correctamente.",
                ["admin.configuration.settings.allsettings"] = "Parametros del sistema",
                ["admin.customers.customers.customerrolesmanagingerror"] = "No tiene suficientes derechos para administrar los perfiles de los usuarios.",
                ["account.passwordrecovery.tooltip"] = "Por favor ingrese su dirección de correo electrónico. Recibirá un enlace para restablecer su contraseña.",
                ["Account.Login.SAC3"] = "Servicios Administrados de Comunicaciones 3",
            }, 2);
        }
        #endregion
        #region Methods

        /// <summary>
        /// Install required data
        /// </summary>
        /// <param name="defaultUserEmail">Default user email</param>
        /// <param name="defaultUserPassword">Default user password</param>
        /// <param name="languagePackInfo">Language pack info</param>
        /// <param name="regionInfo">RegionInfo</param>
        /// <param name="cultureInfo">CultureInfo</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InstallRequiredDataAsync(string defaultUserEmail, string defaultUserPassword,
            (string languagePackDownloadLink, int languagePackProgress) languagePackInfo, RegionInfo regionInfo, CultureInfo cultureInfo)
        {
            await InstallStoresAsync();
            await InstallLanguagesAsync(languagePackInfo, cultureInfo, regionInfo);
            //await InstallCurrenciesAsync(cultureInfo, regionInfo);
            await InstallCountriesAndStatesAsync();
            await InstallEmailAccountsAsync();
            await InstallMessageTemplatesAsync();
            //await InstallTopicTemplatesAsync();
            await InstallSettingsAsync(regionInfo);
            await InstallCustomersAndUsersAsync(defaultUserEmail, defaultUserPassword);
            //await InstallTopicsAsync();
            await InstallScheduleTasksAsync();
            await InstallLanguageSpanishAsync();
            await InstallComponentSettingsAsync();
        }

        /// <summary>
        /// Install sample data
        /// </summary>
        /// <param name="defaultUserEmail">Default user email</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InstallSampleDataAsync(string defaultUserEmail)
        {
            await InstallSampleCustomersAsync();

            var settingService = EngineContext.Current.Resolve<ISettingService>();

            await settingService.SaveSettingAsync(new DisplayDefaultMenuItemSettings
            {
                DisplayHomepageMenuItem = false,
                DisplayNewProductsMenuItem = false,
                DisplayProductSearchMenuItem = false,
                DisplayCustomerInfoMenuItem = false,
                DisplayBlogMenuItem = false,
                DisplayForumsMenuItem = false,
                DisplayContactUsMenuItem = false
            });
        }

        #endregion
    }
}