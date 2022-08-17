using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Nop.Core;
using Nop.Core.ComponentModel;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;

namespace Nop.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents application part manager extensions
    /// </summary>
    public static partial class ApplicationPartManagerExtensions
    {
        #region Fields

        private static readonly INopFileProvider _fileProvider;
        private static readonly List<string> _baseAppLibraries;
        private static readonly ReaderWriterLockSlim _locker = new();

        #endregion

        #region Ctor

        static ApplicationPartManagerExtensions()
        {
            //we use the default file provider, since the DI isn't initialized yet
            _fileProvider = CommonHelper.DefaultFileProvider;

            _baseAppLibraries = new List<string>();

            //get all libraries from /bin/{version}/ directory
            _baseAppLibraries.AddRange(_fileProvider.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Select(fileName => _fileProvider.GetFileName(fileName)));

            //get all libraries from base site directory
            if (!AppDomain.CurrentDomain.BaseDirectory.Equals(Environment.CurrentDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                _baseAppLibraries.AddRange(_fileProvider.GetFiles(Environment.CurrentDirectory, "*.dll")
                    .Select(fileName => _fileProvider.GetFileName(fileName)));
            }
        }

        #endregion

        #region Properties

       

        #endregion

        #region Utilities

        /// <summary>
        /// Copy the plugin assembly file to the shadow copy directory
        /// </summary>
        /// <param name="fileProvider">Nop file provider</param>
        /// <param name="assemblyFile">Path to the plugin assembly file</param>
        /// <param name="shadowCopyDirectory">Path to the shadow copy directory</param>
        /// <returns>Path to the shadow copied file</returns>
        private static string ShadowCopyFile(INopFileProvider fileProvider, string assemblyFile, string shadowCopyDirectory)
        {
            //get path to the new shadow copied file
            var shadowCopiedFile = fileProvider.Combine(shadowCopyDirectory, fileProvider.GetFileName(assemblyFile));

            //check if a shadow copied file already exists and if it does
            if (fileProvider.FileExists(shadowCopiedFile))
            {
                //it exists, then check if it's updated (compare creation time of files)
                var areFilesIdentical = fileProvider.GetCreationTime(shadowCopiedFile).ToUniversalTime().Ticks >=
                    fileProvider.GetCreationTime(assemblyFile).ToUniversalTime().Ticks;
                if (areFilesIdentical)
                {
                    //no need to copy again
                    return shadowCopiedFile;
                }

                //file already exists but passed file is more updated, so delete an existing file and copy again
                //More info: https://www.nopcommerce.com/boards/topic/11511/access-error-nopplugindiscountrulesbillingcountrydll/page/4#60838
                fileProvider.DeleteFile(shadowCopiedFile);
            }

            //try to shadow copy
            try
            {
                fileProvider.FileCopy(assemblyFile, shadowCopiedFile, true);
            }
            catch (IOException)
            {
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    var oldFile = $"{shadowCopiedFile}{Guid.NewGuid():N}.old";
                    fileProvider.FileMove(shadowCopiedFile, oldFile);
                }
                catch (IOException exception)
                {
                    throw new IOException($"{shadowCopiedFile} rename failed, cannot initialize plugin", exception);
                }

                //or retry the shadow copy
                fileProvider.FileCopy(assemblyFile, shadowCopiedFile, true);
            }

            return shadowCopiedFile;
        }

        /// <summary>
        /// Load and register the assembly
        /// </summary>
        /// <param name="applicationPartManager">Application part manager</param>
        /// <param name="assemblyFile">Path to the assembly file</param>
        /// <param name="useUnsafeLoadAssembly">Indicating whether to load an assembly into the load-from context, bypassing some security checks</param>
        /// <returns>Assembly</returns>
        private static Assembly AddApplicationParts(ApplicationPartManager applicationPartManager, string assemblyFile, bool useUnsafeLoadAssembly)
        {
            //try to load a assembly
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(assemblyFile);
            }
            catch (FileLoadException)
            {
                if (useUnsafeLoadAssembly)
                {
                    //if an application has been copied from the web, it is flagged by Windows as being a web application,
                    //even if it resides on the local computer.You can change that designation by changing the file properties,
                    //or you can use the<loadFromRemoteSources> element to grant the assembly full trust.As an alternative,
                    //you can use the UnsafeLoadFrom method to load a local assembly that the operating system has flagged as
                    //having been loaded from the web.
                    //see http://go.microsoft.com/fwlink/?LinkId=155569 for more information.
                    assembly = Assembly.UnsafeLoadFrom(assemblyFile);
                }
                else
                    throw;
            }

            //register the plugin definition
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));

            return assembly;
        }

        

        #endregion

        #region Methods

       

        #endregion
    }
}
