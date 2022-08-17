using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Extensions;
using Nop.Web.Models.Common;
using Nop.Web.Models.Profile;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the profile model factory
    /// </summary>
    public partial class ProfileModelFactory : IProfileModelFactory
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ProfileModelFactory(CustomerSettings customerSettings,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _customerSettings = customerSettings;
            _countryService = countryService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the profile index model
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="page">Number of posts page; pass null to disable paging</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the profile index model
        /// </returns>
        public virtual async Task<ProfileIndexModel> PrepareProfileIndexModelAsync(Customer customer, int? page)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var pagingPosts = false;
            var postsPage = 0;

            if (page.HasValue)
            {
                postsPage = page.Value;
                pagingPosts = true;
            }

            var name = await _customerService.FormatUsernameAsync(customer);
            var title = string.Format(await _localizationService.GetResourceAsync("Profile.ProfileOf"), name);

            var model = new ProfileIndexModel
            {
                ProfileTitle = title,
                PostsPage = postsPage,
                PagingPosts = pagingPosts,
                CustomerProfileId = customer.Id,
            };
            return model;
        }

        /// <summary>
        /// Prepare the profile info model
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the profile info model
        /// </returns>
        public virtual async Task<ProfileInfoModel> PrepareProfileInfoModelAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            
            //location
            var locationEnabled = false;
            var location = string.Empty;
            if (_customerSettings.ShowCustomersLocation)
            {
                locationEnabled = true;

                var countryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute);
                var country = await _countryService.GetCountryByIdAsync(countryId);
                if (country != null)
                {
                    location = await _localizationService.GetLocalizedAsync(country, x => x.Name);
                }
                else
                {
                    locationEnabled = false;
                }
            }

            //registration date
            var joinDateEnabled = false;
            var joinDate = string.Empty;

            if (_customerSettings.ShowCustomersJoinDate)
            {
                joinDateEnabled = true;
                joinDate = (await _dateTimeHelper.ConvertToUserTimeAsync(customer.CreatedOnUtc, DateTimeKind.Utc)).ToString("f");
            }

            //birth date
            var dateOfBirthEnabled = false;
            var dateOfBirth = string.Empty;
            if (_customerSettings.DateOfBirthEnabled)
            {
                var dob = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
                if (dob.HasValue)
                {
                    dateOfBirthEnabled = true;
                    dateOfBirth = dob.Value.ToString("D");
                }
            }

            var model = new ProfileInfoModel
            {
                CustomerProfileId = customer.Id,
                LocationEnabled = locationEnabled,
                Location = location,
                JoinDateEnabled = joinDateEnabled,
                JoinDate = joinDate,
                DateOfBirthEnabled = dateOfBirthEnabled,
                DateOfBirth = dateOfBirth,
            };

            return model;
        }

        #endregion
    }
}