﻿using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Templates;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the template model factory
    /// </summary>
    public partial interface ITemplateModelFactory
    {
        /// <summary>
        /// Prepare templates model
        /// </summary>
        /// <param name="model">Templates model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the mplates model
        /// </returns>
        Task<TemplatesModel> PrepareTemplatesModelAsync(TemplatesModel model);
        
        /// <summary>
        /// Prepare topic template search model
        /// </summary>
        /// <param name="searchModel">Topic template search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic template search model
        /// </returns>
        Task<TopicTemplateSearchModel> PrepareTopicTemplateSearchModelAsync(TopicTemplateSearchModel searchModel);


        /// <summary>
        /// Prepare paged topic template list model
        /// </summary>
        /// <param name="searchModel">Topic template search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the opic template list model
        /// </returns>
        Task<TopicTemplateListModel> PrepareTopicTemplateListModelAsync(TopicTemplateSearchModel searchModel);

    }
}