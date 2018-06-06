using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorConfig.Models.UserInfo
{
    public class ClientUser
    {
		public string JiraUsername { get; set; }
		public string TrimbleConnectUsername{ get; set; }
		public string EasyAccessUsername { get; set; }
    }
}
