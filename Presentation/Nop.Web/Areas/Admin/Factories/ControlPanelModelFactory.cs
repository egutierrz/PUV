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
using Nop.Web.Areas.Admin.Models.Common;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IControlPanelModelFactory
    {
        Task<string> PrepareAlarmsTblAsync();
        Task<string> PrepareEventsTblAsync();
    }

    public class ControlPanelModelFactory : IControlPanelModelFactory
    {
        private readonly IAlarms _alarmsService;
        private readonly IEvents _eventsService;
        private readonly IApproute _approute;
        private readonly IAuthenticationService _authenticationService;
        private readonly IConnectionsService _connectionsService;
        
        private readonly IBfd _bfd;
        private readonly IHealth _health;
        private readonly IIssues _issues;
        private readonly IStats _stats;
        private readonly IHardwareHealth _hardwareHealth;
        private readonly IControl _control;
        private readonly IDetail _detail;
        private readonly IInterface _interface;




        public ControlPanelModelFactory(IAlarms alarmsService,
            IEvents eventsService,
            IApproute approute,
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
            _alarmsService = alarmsService;
            _eventsService = eventsService;
            _approute = approute;
            _authenticationService = authenticationService;
            _connectionsService = connectionsService;
            _bfd = bfd;
            _health = health;
            _issues = issues;
            _stats = stats;
            _hardwareHealth = hardwareHealth;
            _control = control;
            _detail = detail;
            _interface = iinterface;
        }

        public async Task<string> PrepareAlarmsTblAsync()
        {
            var jSessionId = await _authenticationService.LoginAsync();
            var token = await _authenticationService.TokenAsync(jSessionId);

            var json = await _alarmsService.AlarmsAsync(jSessionId, token, 24);
            return json;
        }

        public async Task<string> PrepareEventsTblAsync()
        {
            var jSessionId = await _authenticationService.LoginAsync();
            var token = await _authenticationService.TokenAsync(jSessionId);
            var json = await _eventsService.EventsAsync(jSessionId, token, 3);
            return json;
        }

    }
}