using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using SkiaSharp;

namespace Nop.Services.Media.RoxyFileman
{
    /// <summary>
    /// Database RoxyFileman service
    /// </summary>
    public class DatabaseRoxyFilemanService : FileRoxyFilemanService
    {
        #region Fields


        #endregion

        #region Ctor

        public DatabaseRoxyFilemanService(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IWebHelper webHelper,
            IWorkContext workContext) : base(webHostEnvironment, httpContextAccessor, fileProvider, webHelper, workContext)
        {
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Initial service configuration
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();
        }

        #endregion

    }
}