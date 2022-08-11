using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Model.ServiceMonitoring;
using Sat.Cisco.SD_WAN.Device;
using Sat.Cisco.SD_WAN.Security;
using static Nop.Web.Areas.Admin.Model.ServiceMonitoring.ServiceMonitoringModel;
using Sat.Cisco.SD_WAN.Network;
using Newtonsoft.Json;
using Sat.Cisco.SD_WAN.ClusterManagement;
using Sat.Cisco.SD_WAN.Certificate;
using Sat.Cisco.SD_WAN.Statistics;
using Sat.Cisco.SD_WAN.Detail;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IServiceMonitoringModelFactory
    {
        Task<ServiceMonitoringModel> PrepareServiceServiceMonitoringAsync();
        Task<string> PrepareViewPercent(string jSessionId, string token);
        Task<string> PrepareViewDetail(string url, string jSessionId);
        Task<string> PrepareTransportHealthChartModel();


    }

    public class ServiceMonitoringModelFactory : IServiceMonitoringModelFactory
    {
        private readonly IApproute _approute;
        private readonly IAuthenticationService _authenticationService;
        private readonly IConnectionsService _connectionsService;
        private readonly IVEdgeInventory _vEdgeInventory;
        private readonly IBfd _bfd;
        private readonly IHealth _health;
        private readonly IIssues _issues;
        private readonly IStats _stats;
        private readonly IHardwareHealth _hardwareHealth;
        private readonly IControl _control;
        private readonly IDetail _detail;
        private readonly IInterface _interface;




        public ServiceMonitoringModelFactory(IApproute approute,
            IAuthenticationService authenticationService,
            IConnectionsService connectionsService,
            IVEdgeInventory vEdgeInventory,
            IBfd bfd,
            IHealth health,
            IIssues issues,
            IStats stats,
            IHardwareHealth hardwareHealth,
            IControl control,
            IDetail detail,
            IInterface iinterface)
        {
            _approute = approute;
            _authenticationService = authenticationService;
            _connectionsService = connectionsService;
            _vEdgeInventory = vEdgeInventory;
            _bfd = bfd;
            _health = health;
            _issues = issues;
            _stats = stats;
            _hardwareHealth = hardwareHealth;
            _control = control;
            _detail = detail;
            _interface = iinterface;
        }

        public async Task<ServiceMonitoringModel> PrepareServiceServiceMonitoringAsync()
        {
            var url = await _authenticationService.GetURLAsync();

            var model = new ServiceMonitoringModel();

            var jSessionId = await _authenticationService.LoginAsync();

            if (!string.IsNullOrEmpty(jSessionId))
            {
                var token = await _authenticationService.TokenAsync(jSessionId);
                if (!string.IsNullOrEmpty(token))
                {
                    model.JSessionId = jSessionId;
                    model.Token = token;
                }
                else
                    return null;
            }
            else
                return null;

            var widget = await _connectionsService.ConnectionsSummaryAsync(jSessionId);
            var widgetBfd = await _bfd.Sites_SummaryAsync(jSessionId);
            var widgetHealth = await _health.SummaryAsync(jSessionId);
            var wigetReboot = await _issues.RebootCountAsync(jSessionId);
            var wigetWarning = await _stats.SummaryAsync(jSessionId);
            var widgetWanEdgeHealth = await _hardwareHealth.SummaryAsync(jSessionId);

            dynamic d = JsonConvert.DeserializeObject<dynamic>(widget);

            foreach (var w in d.data)
            {
                var m = new WidgetModel();
                m.Name = w.name + " - " + w.count;
                m.Counting = w.count;
                m.Link = w.detailsURL;
                m.Image = "/images/serviceMonitoring/" + w.name + ".png";
                m.Detail = await _detail.Get_DetailAsync(jSessionId, m.Link);
                model.WidgetModelList.Add(m);
            }


            model.SiteHealth = await _bfd.Sites_SummaryAsync(jSessionId);
            model.Reboot = await _issues.RebootCountAsync(jSessionId);
            model.Certificate = await _stats.SummaryAsync(jSessionId);

            //Transport Interface Distribution
            model.TransportInterfaceDist = await _approute.Transport_Interface_DistAsync(jSessionId);

            //WAN Edge Inventory
            model.WANEdgeInventory = await _vEdgeInventory.SummaryAsync(jSessionId);
            //WAN Edge Health
            model.WANEdgeHealth = await _hardwareHealth.SummaryAsync(jSessionId);

            //VManage
            dynamic vManage = JsonConvert.DeserializeObject<dynamic>(widgetHealth);

            var v = new WidgetModel();
            foreach (var w in vManage.data)
            {
                v.Name = w.name + " - " + w.count;
                v.Counting = w.count;
                v.Link = w.detailsURL;
                v.Image = "/images/serviceMonitoring/" + w.name + ".png";
                v.Detail = await _detail.Get_DetailAsync(jSessionId, url: v.Link);
            }
            model.VManage = v;

            //Control status
            model.ControlStatus = await _control.CountAsync(jSessionId);
            return model;
        }

        public async Task<string> PrepareViewPercent(string jSessionId, string token)
        {
            var json = await _interface.CCapacity_DistributionAsync(jSessionId, token);
            return json;
        }

        public async Task<string> PrepareViewDetail(string url, string jSessionId)
        {
            var json = await _detail.Get_DetailAsync(jSessionId, url);
            return json;
        }
        public async Task<string> PrepareTransportHealthChartModel()
        {
            var jSessionId = await _authenticationService.LoginAsync();
            var model = await _approute.Transport_Interface_DistAsync(jSessionId);
            return model;
        }
    }
}