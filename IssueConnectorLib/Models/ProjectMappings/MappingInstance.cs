using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.ProjectMappings
{
    public class MappingInstance
    {
		public string JiraProjectKey { get; set; }
		public string JiraUrl { get; set; }

		public string TrimbleConnectProjectId { get; set; }
		public string TrimbleConnectUrl { get; set; }

		public string EasyAccessProjectId { get; set; }
		public string EasyAccessUrl { get; set; }

		public bool Contains(string id)
		{
			return (JiraProjectKey.Equals(id) || TrimbleConnectProjectId.Equals(id) || EasyAccessProjectId.Equals(id));
		}



	}
}
