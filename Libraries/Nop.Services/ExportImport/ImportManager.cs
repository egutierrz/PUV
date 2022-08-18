using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INopFileProvider _fileProvider;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public ImportManager(CatalogSettings catalogSettings,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            INopFileProvider fileProvider,
            IStateProvinceService stateProvinceService)
        {
            _catalogSettings = catalogSettings;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _httpClientFactory = httpClientFactory;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _logger = logger;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        private static ExportedAttributeType GetTypeOfExportedAttribute(IXLWorksheet worksheet, PropertyManager<ExportProductAttribute> productAttributeManager, PropertyManager<ExportSpecificationAttribute> specificationAttributeManager, int iRow)
        {
            productAttributeManager.ReadFromXlsx(worksheet, iRow, ExportProductAttribute.ProducAttributeCellOffset);

            if (productAttributeManager.IsCaption)
            {
                return ExportedAttributeType.ProductAttribute;
            }

            specificationAttributeManager.ReadFromXlsx(worksheet, iRow, ExportProductAttribute.ProducAttributeCellOffset);

            if (specificationAttributeManager.IsCaption)
            {
                return ExportedAttributeType.SpecificationAttribute;
            }

            return ExportedAttributeType.NotSpecified;
        }

        

        protected virtual int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            for (var i = 0; i < properties.Length; i++)
                if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
        }
       
        private async Task<string> DownloadFileAsync(string urlString, IList<string> downloadedFiles)
        {
            if (string.IsNullOrEmpty(urlString))
                return string.Empty;

            if (!Uri.IsWellFormedUriString(urlString, UriKind.Absolute))
                return urlString;

            if (!_catalogSettings.ExportImportAllowDownloadImages)
                return string.Empty;

            //ensure that temp directory is created
            var tempDirectory = _fileProvider.MapPath(ExportImportDefaults.UploadsTempPath);
            _fileProvider.CreateDirectory(tempDirectory);

            var fileName = _fileProvider.GetFileName(urlString);
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var filePath = _fileProvider.Combine(tempDirectory, fileName);
            try
            {
                var client = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                var fileData = await client.GetByteArrayAsync(urlString);
                await using (var fs = new FileStream(filePath, FileMode.OpenOrCreate)) 
                    fs.Write(fileData, 0, fileData.Length);

                downloadedFiles?.Add(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Download image failed", ex);
            }

            return string.Empty;
        }

        
        #endregion

        #region Methods

        /// <summary>
        /// Get property list by excel cells
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="worksheet">Excel worksheet</param>
        /// <returns>Property list</returns>
        public static IList<PropertyByName<T>> GetPropertiesByExcelCells<T>(IXLWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Row(1).Cell(poz);

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }

        
        /// <summary>
        /// Import states from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="writeLog">Indicates whether to add logging</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of imported states
        /// </returns>
        public virtual async Task<int> ImportStatesFromTxtAsync(Stream stream, bool writeLog = true)
        {
            var count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var tmp = line.Split(',');

                    if (tmp.Length != 5)
                        throw new NopException("Wrong file format");

                    //parse
                    var countryTwoLetterIsoCode = tmp[0].Trim();
                    var name = tmp[1].Trim();
                    var abbreviation = tmp[2].Trim();
                    var published = bool.Parse(tmp[3].Trim());
                    var displayOrder = int.Parse(tmp[4].Trim());

                    var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(countryTwoLetterIsoCode);
                    if (country == null)
                    {
                        //country cannot be loaded. skip
                        continue;
                    }

                    //import
                    var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(country.Id, showHidden: true);
                    var state = states.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                    if (state != null)
                    {
                        state.Abbreviation = abbreviation;
                        state.Published = published;
                        state.DisplayOrder = displayOrder;
                        await _stateProvinceService.UpdateStateProvinceAsync(state);
                    }
                    else
                    {
                        state = new StateProvince
                        {
                            CountryId = country.Id,
                            Name = name,
                            Abbreviation = abbreviation,
                            Published = published,
                            DisplayOrder = displayOrder
                        };
                        await _stateProvinceService.InsertStateProvinceAsync(state);
                    }

                    count++;
                }
            }

            //activity log
            if (writeLog)
            {
                await _customerActivityService.InsertActivityAsync("ImportStates",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.ImportStates"), count));
            }

            return count;
        }

        

        #endregion
    }
}