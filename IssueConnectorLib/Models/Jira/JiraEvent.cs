using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IssueConnectorLib.Models.Jira
{
	[JsonObject(MemberSerialization.OptIn)]
	public class JiraEvent
	{
		[JsonProperty]
		public string Timestamp { get; set; }

		[JsonProperty]
		public string WebHookEvent { get; set; }

		[JsonProperty]
		public string issue_event_type_name { get; set; }


		[JsonProperty]
		public JiraWebhookIssue Issue { get; set; }


	}
}
