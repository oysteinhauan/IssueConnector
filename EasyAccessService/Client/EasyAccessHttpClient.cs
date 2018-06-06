using EasyAccessService.Connector;
using EasyAccessService.Models;
using IssueConnectorLib.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace EasyAccessService.Client
{
	public class EasyAccessHttpClient : IResourceClient<HttpClient>
	{
		private EasyAccessConfig _configSettings;
		private Timer resourceReconnectorConnectorTimer;

		public EasyAccessHttpClient(IOptions<EasyAccessConfig> configSettingsOptions){

			_configSettings = configSettingsOptions.Value;
			StartResourceReconnectorTimer();


		}

		public HttpClient RestClient { get; set; }

		public async Task Connect()
		{
			Debug.WriteLine("Connecting EasyAccessHttpClient...");
			RestClient = await TrimbleEasyAccessConnector.Connect(_configSettings.Username, _configSettings.Password, _configSettings.ServiceUri);
		}

		private void StartResourceReconnectorTimer()
		{
			//Reconnect resource/refresh token every 24hrs

			resourceReconnectorConnectorTimer = new Timer();
			resourceReconnectorConnectorTimer.Elapsed += new ElapsedEventHandler(ReconnectResource);
			resourceReconnectorConnectorTimer.Interval = 86400000; //24h
			resourceReconnectorConnectorTimer.Enabled = true;
		}

		private async void ReconnectResource(object source, ElapsedEventArgs e)
		{
			await Reconnect();
		}

		public async Task Reconnect()
		{
			Debug.WriteLine("Reconnecting EasyAccessHttpClient...");
			await Connect();
		}
	}
}
