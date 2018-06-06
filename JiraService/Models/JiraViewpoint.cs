using IssueConnectorLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Models
{
	public class JiraViewpoint : IStandardViewpoint
	{
		public Dictionary<string, Identifiers> Identifiers { get; set; }
		public string EncodedThumbnail { get; set; }
		public DateTime Date { get; set; }
		public string Url { get; set; }
		public string ViewpointId { get; set; }
		public byte[] ImageByteArray { get; set; }
	}
}
