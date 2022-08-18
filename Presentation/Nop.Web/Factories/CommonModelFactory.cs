using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the common models factory
    /// </summary>
    public partial class CommonModelFactory : ICommonModelFactory
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DisplayDefaultFooterItemSettings _displayDefaultFooterItemSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly INopHtmlHelper _nopHtmlHelper;
        private readonly IPermissionService _permissionService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly SitemapSettings _sitemapSettings;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        private readonly StoreInformationSettings _storeInformationSettings;

        #endregion

        #region Ctor

        public CommonModelFactory(
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            DisplayDefaultFooterItemSettings displayDefaultFooterItemSettings,
            IActionContextAccessor actionContextAccessor,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            INopHtmlHelper nopHtmlHelper,
            IPermissionService permissionService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IWorkContext workContext,
            LocalizationSettings localizationSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _customerSettings = customerSettings;
            _displayDefaultFooterItemSettings = displayDefaultFooterItemSettings;
            _actionContextAccessor = actionContextAccessor;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _nopHtmlHelper = nopHtmlHelper;
            _permissionService = permissionService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        /// <summary>
        /// Prepare the logo model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the logo model
        /// </returns>
        public virtual async Task<LogoModel> PrepareLogoModelAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var model = new LogoModel
            {
                StoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name)
            };

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.StoreLogoPath
                , store, await _themeContext.GetWorkingThemeNameAsync(), _webHelper.IsCurrentConnectionSecured());
            model.LogoPath = String.Empty;


            return model;
        }

        /// <summary>
        /// Prepare the language selector model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the language selector model
        /// </returns>
        public virtual async Task<LanguageSelectorModel> PrepareLanguageSelectorModelAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var availableLanguages = (await _languageService
                    .GetAllLanguagesAsync(storeId: store.Id))
                    .Select(x => new LanguageModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        FlagImageFileName = x.FlagImageFileName,
                    }).ToList();

            var model = new LanguageSelectorModel
            {
                CurrentLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };

            return model;
        }

        /// <summary>
        /// Prepare the header links model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the header links model
        /// </returns>
        public virtual async Task<HeaderLinksModel> PrepareHeaderLinksModelAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();


            var model = new HeaderLinksModel
            {
                RegistrationType = _customerSettings.UserRegistrationType,
                IsAuthenticated = await _customerService.IsRegisteredAsync(customer),
                CustomerName = await _customerService.IsRegisteredAsync(customer) ? await _customerService.FormatUsernameAsync(customer) : string.Empty,
                //ShoppingCartEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart),
                //WishlistEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist),
                
            };
            
            return model;
        }

        /// <summary>
        /// Prepare the admin header links model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the admin header links model
        /// </returns>
        public virtual async Task<AdminHeaderLinksModel> PrepareAdminHeaderLinksModelAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var model = new AdminHeaderLinksModel
            {
                ImpersonatedCustomerName = await _customerService.IsRegisteredAsync(customer) ? await _customerService.FormatUsernameAsync(customer) : string.Empty,
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel),
                EditPageUrl = _nopHtmlHelper.GetEditPageUrl()
            };

            return model;
        }

        /// <summary>
        /// Prepare the social model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the social model
        /// </returns>
        public virtual async Task<SocialModel> PrepareSocialModelAsync()
        {
            var model = new SocialModel
            {
                FacebookLink = _storeInformationSettings.FacebookLink,
                TwitterLink = _storeInformationSettings.TwitterLink,
                YoutubeLink = _storeInformationSettings.YoutubeLink,
                WorkingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
            };

            return model;
        }

        /// <summary>
        /// Prepare the footer model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the footer model
        /// </returns>
        public virtual async Task<FooterModel> PrepareFooterModelAsync()
        {
            //footer topics
            var store = await _storeContext.GetCurrentStoreAsync();
            
            //model
            var model = new FooterModel
            {
                StoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name),
                //WishlistEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist),
                //ShoppingCartEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart),
                SitemapEnabled = _sitemapSettings.SitemapEnabled,
                SearchEnabled = _catalogSettings.ProductSearchEnabled,
                WorkingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                DisplayTaxShippingInfoFooter = _catalogSettings.DisplayTaxShippingInfoFooter,
                HidePoweredByNopCommerce = _storeInformationSettings.HidePoweredByNopCommerce,
                IsHomePage = _webHelper.GetStoreLocation().Equals(_webHelper.GetThisPageUrl(false), StringComparison.InvariantCultureIgnoreCase),
                AllowCustomersToCheckGiftCardBalance = _customerSettings.AllowCustomersToCheckGiftCardBalance && _captchaSettings.Enabled,
                DisplaySitemapFooterItem = _displayDefaultFooterItemSettings.DisplaySitemapFooterItem,
                DisplayContactUsFooterItem = _displayDefaultFooterItemSettings.DisplayContactUsFooterItem,
                DisplayProductSearchFooterItem = _displayDefaultFooterItemSettings.DisplayProductSearchFooterItem,
                DisplayNewsFooterItem = _displayDefaultFooterItemSettings.DisplayNewsFooterItem,
                DisplayBlogFooterItem = _displayDefaultFooterItemSettings.DisplayBlogFooterItem,
                DisplayForumsFooterItem = _displayDefaultFooterItemSettings.DisplayForumsFooterItem,
                DisplayRecentlyViewedProductsFooterItem = _displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem,
                DisplayCompareProductsFooterItem = _displayDefaultFooterItemSettings.DisplayCompareProductsFooterItem,
                DisplayNewProductsFooterItem = _displayDefaultFooterItemSettings.DisplayNewProductsFooterItem,
                DisplayCustomerInfoFooterItem = _displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem,
                DisplayCustomerOrdersFooterItem = _displayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem,
                DisplayCustomerAddressesFooterItem = _displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem,
                DisplayShoppingCartFooterItem = _displayDefaultFooterItemSettings.DisplayShoppingCartFooterItem,
                DisplayWishlistFooterItem = _displayDefaultFooterItemSettings.DisplayWishlistFooterItem,
                DisplayApplyVendorAccountFooterItem = _displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem
            };

            return model;
        }

        /// <summary>
        /// Prepare the contact us model
        /// </summary>
        /// <param name="model">Contact us model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the contact us model
        /// </returns>
        public virtual async Task<ContactUsModel> PrepareContactUsModelAsync(ContactUsModel model, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!excludeProperties)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                model.Email = customer.Email;
                model.FullName = await _customerService.GetCustomerFullNameAsync(customer);
            }

            model.SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;

            return model;
        }

        /// <summary>
        /// Prepare the sitemap model
        /// </summary>
        /// <param name="pageModel">Sitemap page model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sitemap model
        /// </returns>
        public virtual async Task<SitemapModel> PrepareSitemapModelAsync(SitemapPageModel pageModel)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.SitemapPageModelKey,
                language, customerRoleIds, store);

            var cachedModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //get URL helper
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

                var model = new SitemapModel();

                //prepare common items
                var commonGroupTitle = await _localizationService.GetResourceAsync("Sitemap.General");

                //home page
                model.Items.Add(new SitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Homepage"),
                    Url = urlHelper.RouteUrl("Homepage")
                });

                //search
                model.Items.Add(new SitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Search"),
                    Url = urlHelper.RouteUrl("ProductSearch")
                });

                //contact us
                model.Items.Add(new SitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("ContactUs"),
                    Url = urlHelper.RouteUrl("ContactUs")
                });

                //customer info
                model.Items.Add(new SitemapModel.SitemapItemModel
                {
                    GroupTitle = commonGroupTitle,
                    Name = await _localizationService.GetResourceAsync("Account.MyAccount"),
                    Url = urlHelper.RouteUrl("CustomerInfo")
                });
                
                return model;
            });

            //prepare model with pagination
            pageModel.PageSize = Math.Max(pageModel.PageSize, _sitemapSettings.SitemapPageSize);
            pageModel.PageNumber = Math.Max(pageModel.PageNumber, 1);

            var pagedItems = new PagedList<SitemapModel.SitemapItemModel>(cachedModel.Items, pageModel.PageNumber - 1, pageModel.PageSize);
            var sitemapModel = new SitemapModel { Items = pagedItems };
            sitemapModel.PageModel.LoadPagedList(pagedItems);

            return sitemapModel;
        }


        /// <summary>
        /// Prepare the favicon model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the favicon model
        /// </returns>
        public virtual Task<FaviconAndAppIconsModel> PrepareFaviconAndAppIconsModelAsync()
        {
            var model = new FaviconAndAppIconsModel
            {
                HeadCode = _commonSettings.FaviconAndAppIconsHeadCode
            };

            return Task.FromResult(model);
        }

        /// <summary>
        /// Get robots.txt file
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the robots.txt file as string
        /// </returns>
        public virtual async Task<string> PrepareRobotsTextFileAsync()
        {
            var sb = new StringBuilder();

            //if robots.custom.txt exists, let's use it instead of hard-coded data below
            var robotsFilePath = _fileProvider.Combine(_fileProvider.MapPath("~/"), "robots.custom.txt");
            if (_fileProvider.FileExists(robotsFilePath))
            {
                //the robots.txt file exists
                var robotsFileContent = await _fileProvider.ReadAllTextAsync(robotsFilePath, Encoding.UTF8);
                sb.Append(robotsFileContent);
            }
            else
            {
                //doesn't exist. Let's generate it (default behavior)

                var disallowPaths = new List<string>
                {
                    "/admin",
                    "/bin/",
                    "/files/",
                    "/files/exportimport/",
                    "/country/getstatesbycountryid",
                    "/install",
                    "/setproductreviewhelpfulness",
                    "/*?*returnUrl="
                };
                var localizableDisallowPaths = new List<string>
                {
                    "/addproducttocart/catalog/",
                    "/addproducttocart/details/",
                    "/backinstocksubscriptions/manage",
                    "/boards/forumsubscriptions",
                    "/boards/forumwatch",
                    "/boards/postedit",
                    "/boards/postdelete",
                    "/boards/postcreate",
                    "/boards/topicedit",
                    "/boards/topicdelete",
                    "/boards/topiccreate",
                    "/boards/topicmove",
                    "/boards/topicwatch",
                    "/cart$",
                    "/changecurrency",
                    "/changelanguage",
                    "/changetaxtype",
                    "/checkout",
                    "/checkout/billingaddress",
                    "/checkout/completed",
                    "/checkout/confirm",
                    "/checkout/shippingaddress",
                    "/checkout/shippingmethod",
                    "/checkout/paymentinfo",
                    "/checkout/paymentmethod",
                    "/clearcomparelist",
                    "/compareproducts",
                    "/compareproducts/add/*",
                    "/customer/avatar",
                    "/customer/activation",
                    "/customer/addresses",
                    "/customer/changepassword",
                    "/customer/checkusernameavailability",
                    "/customer/downloadableproducts",
                    "/customer/info",
                    "/customer/productreviews",
                    "/deletepm",
                    "/emailwishlist",
                    "/eucookielawaccept",
                    "/inboxupdate",
                    "/newsletter/subscriptionactivation",
                    "/onepagecheckout",
                    "/order/history",
                    "/orderdetails",
                    "/passwordrecovery/confirm",
                    "/poll/vote",
                    "/privatemessages",
                    "/recentlyviewedproducts",
                    "/returnrequest",
                    "/returnrequest/history",
                    "/rewardpoints/history",
                    "/search?",
                    "/sendpm",
                    "/sentupdate",
                    "/shoppingcart/*",
                    "/storeclosed",
                    "/subscribenewsletter",
                    "/topic/authenticate",
                    "/viewpm",
                    "/uploadfilecheckoutattribute",
                    "/uploadfileproductattribute",
                    "/uploadfilereturnrequest",
                    "/wishlist",
                };

                const string newLine = "\r\n"; //Environment.NewLine
                sb.Append("User-agent: *");
                sb.Append(newLine);

                //sitemaps
                if (_sitemapXmlSettings.SitemapXmlEnabled)
                {
                    sb.AppendFormat("Sitemap: {0}sitemap.xml", _webHelper.GetStoreLocation());
                    sb.Append(newLine);
                }

                //host
                sb.AppendFormat("Host: {0}", _webHelper.GetStoreLocation());
                sb.Append(newLine);

                //usual paths
                foreach (var path in disallowPaths)
                {
                    sb.AppendFormat("Disallow: {0}", path);
                    sb.Append(newLine);
                }
                //localizable paths (without SEO code)
                foreach (var path in localizableDisallowPaths)
                {
                    sb.AppendFormat("Disallow: {0}", path);
                    sb.Append(newLine);
                }

                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    //URLs are localizable. Append SEO code
                    foreach (var language in await _languageService.GetAllLanguagesAsync(storeId: store.Id))
                    {
                        foreach (var path in localizableDisallowPaths)
                        {
                            sb.AppendFormat("Disallow: /{0}{1}", language.UniqueSeoCode, path);
                            sb.Append(newLine);
                        }
                    }
                }

                //load and add robots.txt additions to the end of file.
                var robotsAdditionsFile = _fileProvider.Combine(_fileProvider.MapPath("~/"), "robots.additions.txt");
                if (_fileProvider.FileExists(robotsAdditionsFile))
                {
                    var robotsFileContent = await _fileProvider.ReadAllTextAsync(robotsAdditionsFile, Encoding.UTF8);
                    sb.Append(robotsFileContent);
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}