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
    public interface IEvents
    {
        Task<string?> EventsAsync(string jsessionid, string token, int hours);
        Task<string?> AggregationAsync(string jsessionid, string token, int hours);
    }
}
