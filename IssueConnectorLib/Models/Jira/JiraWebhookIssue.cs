using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.Jira
{
	public class JiraWebhookIssue
	{
		[JsonProperty]
		public string Key { get; set; }

		[JsonProperty]
		public long Id { get; set; }

		[JsonProperty]
		public string Self { get; set; }

    }
}
