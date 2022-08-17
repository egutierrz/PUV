using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Common;

namespace Nop.Web.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Ger JQuery Datepicker date format from the .net current culture
        /// </summary>
        /// <param name="html">HtmlHelper object.</param>
        /// <returns>Format string that supported in JQuery Datepicker.</returns>
        public static string GetJQueryDateFormat(this IHtmlHelper html)
        {
            /*
                *  Date used in this comment : 5th - Nov - 2009 (Thursday)
                *
                *  .NET    JQueryUI        Output      Comment
                *  --------------------------------------------------------------
                *  d       d               5           day of month(No leading zero)
                *  dd      dd              05          day of month(two digit)
                *  ddd     D               Thu         day short name
                *  dddd    DD              Thursday    day long name
                *  M       m               11          month of year(No leading zero)
                *  MM      mm              11          month of year(two digit)
                *  MMM     M               Nov         month name short
                *  MMMM    MM              November    month name long.
                *  yy      y               09          Year(two digit)
                *  yyyy    yy              2009        Year(four digit)             *
                */

            var currentFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

            // Convert the date
            currentFormat = currentFormat.Replace("dddd", "DD");
            currentFormat = currentFormat.Replace("ddd", "D");

            // Convert month
            if (currentFormat.Contains("MMMM"))
            {
                currentFormat = currentFormat.Replace("MMMM", "MM");
            }
            else if (currentFormat.Contains("MMM"))
            {
                currentFormat = currentFormat.Replace("MMM", "M");
            }
            else if (currentFormat.Contains("MM"))
            {
                currentFormat = currentFormat.Replace("MM", "mm");
            }
            else
            {
                currentFormat = currentFormat.Replace("M", "m");
            }

            // Convert year
            currentFormat = currentFormat.Contains("yyyy") ?
                currentFormat.Replace("yyyy", "yy") : currentFormat.Replace("yy", "y");

            return currentFormat;
        }

        /// <summary>
        /// Prepare a common pager
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="model">Pager model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pager
        /// </returns>
        /// <remarks>We have two pagers: The first one can have custom routes. The second one just adds query string parameter</remarks>
        public static async Task<IHtmlContent> PagerAsync<TModel>(this IHtmlHelper<TModel> html, PagerModel model)
        {
            if (model.TotalRecords == 0)
                return new HtmlString(string.Empty);

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            var links = new StringBuilder();
            if (model.ShowTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(await model.GetCurrentPageTextAsync(), model.PageIndex + 1, model.TotalPages, model.TotalRecords));
                links.Append("</li>");
            }

            if (model.ShowPagerItems && (model.TotalPages > 1))
            {
                if (model.ShowFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.pageNumber = 1;

                        links.Append("<li class=\"first-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetFirstButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.FirstPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetFirstButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.FirstPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        links.Append("</li>");
                    }
                }

                if (model.ShowPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        model.RouteValues.pageNumber = model.PageIndex;

                        links.Append("<li class=\"previous-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetPreviousButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.PreviousPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetPreviousButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.PreviousPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        links.Append("</li>");
                    }
                }

                if (model.ShowIndividualPages)
                {
                    //individual pages
                    var firstIndividualPageIndex = model.GetFirstIndividualPageIndex();
                    var lastIndividualPageIndex = model.GetLastIndividualPageIndex();
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                            links.AppendFormat("<li class=\"current-page\"><span>{0}</span></li>", i + 1);
                        else
                        {
                            model.RouteValues.pageNumber = i + 1;

                            links.Append("<li class=\"individual-page\">");
                            if (model.UseRouteLinks)
                            {
                                var link = html.RouteLink((i + 1).ToString(),
                                    model.RouteActionName,
                                    model.RouteValues,
                                    new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), i + 1) });
                                links.Append(await link.RenderHtmlContentAsync());
                            }
                            else
                            {
                                var link = html.ActionLink((i + 1).ToString(),
                                    model.RouteActionName,
                                    model.RouteValues,
                                    new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), i + 1) });
                                links.Append(await link.RenderHtmlContentAsync());
                            }
                            links.Append("</li>");
                        }
                    }
                }

                if (model.ShowNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        model.RouteValues.pageNumber = (model.PageIndex + 2);

                        links.Append("<li class=\"next-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetNextButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.NextPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetNextButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.NextPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        links.Append("</li>");
                    }
                }

                if (model.ShowLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.pageNumber = model.TotalPages;

                        links.Append("<li class=\"last-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetLastButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.LastPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetLastButtonTextAsync(),
                                model.RouteActionName,
                                model.RouteValues,
                                new { title = await localizationService.GetResourceAsync("Pager.LastPageTitle") });
                            links.Append(await link.RenderHtmlContentAsync());
                        }
                        links.Append("</li>");
                    }
                }
            }

            var result = links.ToString();
            if (!string.IsNullOrEmpty(result))
                result = "<ul>" + result + "</ul>";

            return new HtmlString(result);
        }

        /// <summary>
        /// Prepare a common pager
        /// </summary>
        /// <param name="helper">HTML helper</param>
        /// <param name="model">Pager model</param>
        /// <returns>Pager</returns>
        /// <remarks>We have two pagers: The first one can have custom routes. The second one just adds query string parameter</remarks>
        public static Pager Pager(this IHtmlHelper helper, IPageableModel model)
        {
            return new Pager(model, helper.ViewContext);
        }

        /// <summary>
        /// Get a value of the text flow uses for the current UI culture
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="ignoreRtl">A value indicating whether to we should ignore RTL language property for admin area. False by default</param>
        /// <returns>"rtl" if text flows from right to left; otherwise, "ltr".</returns>
        public static string GetUIDirection(this IHtmlHelper html, bool ignoreRtl = false)
        {
            if (ignoreRtl)
                return "ltr";

            return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
        }

        
    }
}