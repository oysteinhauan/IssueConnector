using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Models
{
	public class JiraComment : IStandardComment
	{
		public Dictionary<string, Identifiers> Identifiers { get; set; }
		public DateTime? Created { get; set; }
		public string Content { get; set; }
		public string Status { get; set; }
		public string Priority { get; set; }
		public string MessageOrigin { get; set; }
		public ClientUser Author { get; set; }
		public List<string> ViewpointIds { get; set; }
	}
}
