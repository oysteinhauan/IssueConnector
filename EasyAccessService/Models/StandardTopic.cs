using IssueConnectorLib;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Models
{
	public class StandardTopic : IStandardInstance
	{
		public Dictionary<string, Identifiers> Identifiers { get; set; }
		public string MessageOrigin { get; set; }
		public string Action { get; set; }
		public DateTime Created { get; set; }
		public ClientUser Author { get; set; }
		public ClientUser Assignee { get; set; }
		public string Summary { get; set; }
		public string Type { get; set; }
		public string Priority { get; set; }
		public string Status { get; set; }
		public ClientUser ModifiedUser { get; set; }
		public string [] Labels { get; set; }
		public Dictionary<string, string> CustomFields { get; set; }
		
	}
}
