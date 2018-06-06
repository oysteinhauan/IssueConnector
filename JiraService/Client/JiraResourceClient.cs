using Atlassian.Jira;
using IssueConnectorLib.Models;
using JiraService.Connector;
using JiraService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace JiraService.Client
{
	public class JiraResourceClient : IResourceClient<Jira>
	{
		private ConfigSettings _configSettings;
		private Timer resourceReconnectorConnectorTimer;

		public JiraResourceClient(IOptions<ConfigSettings> configSettingsOptions)
		{
			_configSettings = configSettingsOptions.Value;
			StartResourceReconnectorTimer();


		}

		public async Task Connect()
		{
			RestClient = await JiraConnector.Connect(_configSettings.Username, _configSettings.Password, _configSettings.ServiceUri);
		
		}

		public async Task Reconnect()
		{
			Debug.WriteLine("Reconnecting Jira...");
			await Connect();
		}

		private void StartResourceReconnectorTimer()
		{
			resourceReconnectorConnectorTimer = new Timer();
			resourceReconnectorConnectorTimer.Elapsed += new ElapsedEventHandler(ReconnectResource);
			resourceReconnectorConnectorTimer.Interval = 86400000; //24h
			resourceReconnectorConnectorTimer.Enabled = true;
		}

		private async void ReconnectResource(object source, ElapsedEventArgs e)
		{
			await Reconnect();
		}

		public Jira RestClient { get; set; }
	}
}
