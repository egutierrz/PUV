﻿using System;
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
    public interface IControl
    {
        Task<string?> CountAsync(string jsessionid);
    }
}
