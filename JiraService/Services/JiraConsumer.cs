using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using IssueConnectorLib.Models.SystemInfo;
using JiraService.Filters;
using JiraService.Models;
using IssueConnectorLib.Services.ServiceBus;
using MassTransit;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Services
{
	public class JiraConsumer : IResourceConsumer
	{
		private IBusControl _bus;
		private IResourceCrudService _resourceService;
		private IResourcePublisher _resourcePublisher;
		private ConfigSettings _configSettings;

		private string MachineOrigin = "JIRA_" + MachineInfo.GetMacAddress();

		public JiraConsumer(IResourceCrudService resourceService, IResourcePublisher publisher, IOptions<ConfigSettings> configSettings) {
			_resourcePublisher = publisher;
			_resourceService = resourceService;
			_configSettings = configSettings.Value;
		}

		public async Task StartConsumerAsync()
		{
			Debug.WriteLine("Starting Jira Consumer...");
			_bus = ServiceBusProvider.GetRabbitMqConsumerBus(this, _configSettings.RabbitMqUri, _configSettings.SystemName, MachineInfo.GetMacAddress());
			await _bus.StartAsync();
		}

		public async Task StopConsumerAsync()
		{
			await _bus.StopAsync();
		}

		public async Task Consume(ConsumeContext<IStandardInstance> context)
		{
			var instance = context.Message;
			
			Debug.WriteLine(context.Message.Summary);
			if (JiraMessageFilter.OfInterest(context.Message, MachineOrigin)){
				try
				{
					switch (context.Message.Action.ToLower())
					{
						case "created":
							await HandleCreation(instance);

							break;

						case "updated":
							await HandleUpdate(instance);
							break;

						case "deleted":
							await HandleDeletion(instance);
							break;
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("Error handling instance: ");
					Debug.WriteLine(e.StackTrace);
				}
			}
		}

		public async Task Consume(ConsumeContext<IStandardViewpoint> context)
		{
			Debug.WriteLine(context.Message.ToString());
			if (JiraMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				await _resourceService.CreateViewpointAsync(context.Message);
			}
		}

		public async Task Consume(ConsumeContext<IStandardComment> context)
		{
			Debug.WriteLine(context.Message.Content);
			if (JiraMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				await _resourceService.CreateCommentAsync(context.Message);
			}
		}

		public async Task Consume(ConsumeContext<IStandardAttachment> context)
		{
			Debug.WriteLine(context.Message.ToString());
			if (JiraMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				//Do stuff
			}
		}

		public async Task Consume(ConsumeContext<IStandardIdentifierMap> context)
		{
			var instanceMap = context.Message;
			if (JiraMessageFilter.OfInterest(instanceMap, MachineOrigin))
			{
				await _resourceService.UpdateInstanceIdMap(instanceMap); 
			}
		}


		public async Task<IStandardInstance> HandleCreation(IStandardInstance instance)
		{
			IStandardInstance handledInstance = new JiraStandardIssue();
			if (!instance.Identifiers.ContainsKey(InstanceKeyNames.JIRA_ISSUE))
			{
				handledInstance = await _resourceService.CreateInstanceAsync(instance);
				await _resourcePublisher.PublishKeyMap(instance.Identifiers);
			} 
			return handledInstance;
		}

		public async Task HandleDeletion(IStandardInstance instance)
		{
			throw new NotImplementedException();
		}

		public async Task<IStandardInstance> HandleUpdate(IStandardInstance instance)
		{
			var handledInstance = await _resourceService.UpdateInstanceAsync(instance);
			return handledInstance;
		}

		public async Task<IStandardComment> HandleCommentCreation(IStandardComment comment)
		{
			if(comment.Content != null)
			{
				await _resourceService.CreateCommentAsync(comment);
			} 

			return null;
		}

	}
}
