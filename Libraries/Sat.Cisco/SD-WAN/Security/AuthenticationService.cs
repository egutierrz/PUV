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

namespace Sat.Cisco.SD_WAN.Security
{
    public interface IAuthenticationService
    {
        Task<string> LoginAsync();
        Task<string?> TokenAsync(string jsessionid);
        Task<string?> GetURLAsync();
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly INotificationService _notificationService;
        private readonly IComponentSettingService _componentSettingService;
        
        public AuthenticationService(INotificationService notificationService, 
            IComponentSettingService componentSettingService)
        {
            _notificationService = notificationService;
            _componentSettingService = componentSettingService;
        }

        public async Task<string> LoginAsync()
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var client = new RestClient(ciscoSettings.SdWan_Url + "/j_security_check");
            
            try
            {
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("j_username", ciscoSettings.SdWan_User);
                request.AddParameter("j_password", ciscoSettings.SdWan_Pass);
                RestResponse response = await client.ExecuteAsync(request);
                Cookie? jsessionId = response.Cookies != null ? response.Cookies.Where(s => s.Name == "JSESSIONID").SingleOrDefault()  : null;
                return jsessionId != null ? jsessionId.Value : "";
            }
            catch (Exception e)
            {
                await _notificationService.ErrorNotificationAsync(e);
                return "";
            }
        }

        public async Task<string?> TokenAsync(string jsessionid)
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var client = new RestClient(ciscoSettings.SdWan_Url + "/dataservice/client/token");

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
        public async Task<string?> GetURLAsync()
        {
            CiscoSettings ciscoSettings = await _componentSettingService.LoadSettingAsync<CiscoSettings>();
            var url = ciscoSettings.SdWan_Url;
            return url;
        }
    }
}
