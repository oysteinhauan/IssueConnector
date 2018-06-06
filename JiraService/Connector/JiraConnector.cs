using Atlassian.Jira;
using MassTransit.Log4NetIntegration.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Connector
{
	public class JiraConnector
	{

		public async static Task<Jira> Connect(string username, string password, string ServiceUri)
		{
			Log4NetLogger.Use();
			try
			{
				Debug.WriteLine("Connecting jira...");
				var jiraClient = Jira.CreateRestClient(ServiceUri, username, password);
				Debug.WriteLine("Succesfully connected to JIRA");

				return jiraClient;




			}
			catch (Exception e)
			{
				Debug.WriteLine("A JIRA error occured:");
				Debug.WriteLine(e);
				return null;
			}
		}
	}
}
