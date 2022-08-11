using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Model.ServiceMonitoring
{
    public partial record ServiceMonitoringModel : BaseNopModel
    {
        public ServiceMonitoringModel() {
            WidgetModelList = new List<WidgetModel>();
        }

        public string Token { set; get; }
        public string JSessionId { set; get; }

        public IList<WidgetModel> WidgetModelList { set; get; }
        public WidgetModel VManage { set; get; }
        public string Reboot { set; get; }
        public string Certificate { set; get; }

        public string ControlStatus { set; get; }
        public string SiteHealth { set; get; }
        public string TransportInterfaceDist { set; get; }
        public string WANEdgeInventory { set; get; }
        public string WANEdgeHealth { set; get; }

        
    
        public partial record WidgetModel : BaseNopEntityModel
        {
            public string Name { get; set; }
            public string Image { get; set; }
            public int Counting { get; set; }
            public string Link { get; set; }
            public string Detail { get; set; }
        }
    }
}
