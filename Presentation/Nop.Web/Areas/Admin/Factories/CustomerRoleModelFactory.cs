﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer role model factory implementation
    /// </summary>
    public partial class CustomerRoleModelFactory : ICustomerRoleModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomerRoleModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IWorkContext workContext)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare customer role search model
        /// </summary>
        /// <param name="searchModel">Customer role search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer role search model
        /// </returns>
        public virtual Task<CustomerRoleSearchModel> PrepareCustomerRoleSearchModelAsync(CustomerRoleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged customer role list model
        /// </summary>
        /// <param name="searchModel">Customer role search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer role list model
        /// </returns>
        public virtual async Task<CustomerRoleListModel> PrepareCustomerRoleListModelAsync(CustomerRoleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get customer roles
            var customerRoles = (await _customerService.GetAllCustomerRolesAsync(true)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new CustomerRoleListModel().PrepareToGridAsync(searchModel, customerRoles, () =>
            {
                return customerRoles.SelectAwait(async role =>
                {
                    //fill in model values from the entity
                    var customerRoleModel = role.ToModel<CustomerRoleModel>();

                    //fill in additional values (not existing in the entity)
                    //customerRoleModel.PurchasedWithProductName = (await _productService.GetProductByIdAsync(role.PurchasedWithProductId))?.Name;

                    return customerRoleModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer role model
        /// </summary>
        /// <param name="model">Customer role model</param>
        /// <param name="customerRole">Customer role</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer role model
        /// </returns>
#pragma warning disable CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        public virtual async Task<CustomerRoleModel> PrepareCustomerRoleModelAsync(CustomerRoleModel model, CustomerRole customerRole, bool excludeProperties = false)
#pragma warning restore CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        {
            if (customerRole != null)
            {
                //fill in model values from the entity
                model ??= customerRole.ToModel<CustomerRoleModel>();
                //model.PurchasedWithProductName = (await _productService.GetProductByIdAsync(customerRole.PurchasedWithProductId))?.Name;
            }

            //set default values for the new model
            if (customerRole == null)
                model.Active = true;

            //prepare available tax display types
            //await _baseAdminModelFactory.PrepareTaxDisplayTypesAsync(model.TaxDisplayTypeValues, false);

            return model;
        }
        #endregion
    }
}