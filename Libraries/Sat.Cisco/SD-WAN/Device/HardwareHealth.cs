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

namespace Sat.Cisco.SD_WAN.Network
{
    public class HardwareHealth : IHardwareHealth
    {
        private readonly INotificationService _notificationService;
        private readonly IComponentSettingService _componentSettingService;
        
        public HardwareHealth(INotificationService notificationService, 
            IComponentSettingService componentSettingService)
        {
            _notificationService = notificationService;
            _componentSettingService = componentSettingService;
        }

       
        public async Task<string?> SummaryAsync(string jsessionid)
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var client = new RestClient(ciscoSettings.SdWan_Url + "/dataservice/device/hardwarehealth/summary?isCached=true");

            try
            {
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddHeader("Cookie", "JSESSIONID=" + jsessionid);

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
