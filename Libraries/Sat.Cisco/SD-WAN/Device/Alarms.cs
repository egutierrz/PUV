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

namespace Sat.Cisco.SD_WAN.Device
{
    public interface IAlarms
    {
        Task<string?> AlarmsAsync(string jsessionid, string token, int hours);

        Task<string?> AggregationAsync(string jsessionid, string token, int hours);
    }
    public class Alarms : IAlarms
    {
        private readonly INotificationService _notificationService;
        private readonly IComponentSettingService _componentSettingService;

        public Alarms(INotificationService notificationService,
            IComponentSettingService componentSettingService)
        {
            _notificationService = notificationService;
            _componentSettingService = componentSettingService;
        }


        public async Task<string?> AlarmsAsync(string jsessionid, string token, int hours)
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var client = new RestClient(ciscoSettings.SdWan_Url + "/dataservice/alarms");

            try
            { 
                var query = "{\"query\": {\"condition\": \"AND\",\"rules\": [{\"value\": [\""+hours+"\"],\"field\": \"entry_time\",\"type\": \"date\",\"operator\": \"last_n_hours\"} ] }, \"size\": 10000}";
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("Cookie", "JSESSIONID=" + jsessionid);
                request.AddHeader("X-XSRF-TOKEN", token);
                request.AddHeader("Content-Type", "application/json");
                request.AddBody(query);

                RestResponse response = await client.ExecuteAsync(request);

                return response.Content;
            }
            catch (Exception e)
            {
                await _notificationService.ErrorNotificationAsync(e);
                return null;
            }
        }

        public async Task<string?> AggregationAsync(string jsessionid, string token, int hours)
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var client = new RestClient(ciscoSettings.SdWan_Url + "/dataservice/alarms/aggregation");

            try
            {
                var query = "{\"query\": {\"condition\": \"AND\",\"rules\": [{\"value\": [\"" + hours + "\"],\"field\": \"entry_time\",\"type\": \"date\",\"operator\": \"last_n_hours\"}]},\"aggregation\": {\"field\": [{\"property\": \"severity\",\"order\": \"asc\",\"sequence\": 1}],\"histogram\": {\"property\": \"entry_time\",\"interval\": 1,\"type\": \"hour\",\"order\": \"asc\"}}}";
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("Cookie", "JSESSIONID=" + jsessionid);
                request.AddHeader("X-XSRF-TOKEN", token);
                request.AddHeader("Content-Type", "application/json");
                request.AddBody(query);

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
