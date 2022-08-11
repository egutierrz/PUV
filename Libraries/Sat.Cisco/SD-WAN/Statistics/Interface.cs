using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Cisco;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using RestSharp;

namespace Sat.Cisco.SD_WAN.Statistics
{
    public interface IInterface
    {
        Task<string?> CCapacity_DistributionAsync(string jsessionid, string token);
    }
    public class Interface : IInterface
    {
        private readonly INotificationService _notificationService;
        private readonly IComponentSettingService _componentSettingService;
        
        public Interface(INotificationService notificationService, 
            IComponentSettingService componentSettingService)
        {
            _notificationService = notificationService;
            _componentSettingService = componentSettingService;
        }

       
        public async Task<string?> CCapacity_DistributionAsync(string jsessionid, string token)
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var client = new RestClient(ciscoSettings.SdWan_Url + "/dataservice/statistics/interface/ccapacity/distribution");

            try
            {
                var request = new RestRequest();
                request.Method = Method.Get;
                
                request.AddHeader("Cookie", "JSESSIONID=" + jsessionid);
                request.AddHeader("X-XSRF-TOKEN", token);

                RestResponse response = await client.ExecuteAsync(request);

                return response.Content;
            }
            catch (Exception e)
            {
                await _notificationService.ErrorNotificationAsync(e);
                return null;
            }
        }
    }
}
