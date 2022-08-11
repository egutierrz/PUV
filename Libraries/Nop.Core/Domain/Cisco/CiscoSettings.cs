using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.Cisco
{
    public class CiscoSettings : ISettings
    {
        public string SdWan_User { get; set; }
        public string SdWan_Password { get; set; }
        public string SdWan_Url { get; set; }

    }
}
