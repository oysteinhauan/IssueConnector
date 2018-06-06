using EasyAccessService.Filters;
using EasyAccessService.Models;
using IssueConnectorLib.Services.ServiceBus;
using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using IssueConnectorLib.Models.SystemInfo;
using MassTransit;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccess.Services
{
	public class EasyAccessConsumer : IResourceConsumer
	{
		private IBusControl _bus;
		private IResourceCrudService _resourceService;
		private IResourcePublisher _resourcePublisher;

		private string MachineOrigin = "EasyAccess_" + MachineInfo.GetMacAddress();

		public EasyAccessConsumer(IResourceCrudService resourceService, IResourcePublisher publisher) {
			_resourcePublisher = publisher;
			_resourceService = resourceService;
		}

		public async Task StartConsumerAsync()
		{
			Debug.WriteLine("Starting EasyAccessConsumer...");
			_bus = ServiceBusProvider.GetRabbitMqConsumerBus(this, "rabbitmq://localhost", "EasyAccess", MachineInfo.GetMacAddress());
			await _bus.StartAsync();
		}

		public async Task StopConsumerAsync()
		{
			await _bus.StopAsync();
		}

		public async Task Consume(ConsumeContext<IStandardInstance> context)
		{
			var instance = context.Message;
			Debug.WriteLine("Consumed IStandardInstance at EasyAccess: " + context.Message.ToString());
			if (EasyAccessMessageFilter.OfInterest(context.Message, MachineOrigin)){
				switch (context.Message.Action.ToLower())
				{
					case "created":
						Debug.WriteLine("Creating instance...");
						var handledInstance = await HandleCreation(instance);
						
						break;

					case "updated":
						Debug.WriteLine("Updating instance...");
						await HandleUpdate(instance);

						break;
				}
			}
		}

		public Task Consume(ConsumeContext<IStandardViewpoint> context)
		{
			Debug.WriteLine(context.Message.ToString());
			if (EasyAccessMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				//Do stuff
			}
			return Task.FromResult(0);
		}

		public Task Consume(ConsumeContext<IStandardComment> context)
		{
			Debug.WriteLine(context.Message.Content);
			if (EasyAccessMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				_resourceService.CreateCommentAsync(context.Message);
			}
			return Task.FromResult(0);
		}

		public Task Consume(ConsumeContext<IStandardAttachment> context)
		{
			Debug.WriteLine(context.Message.ToString());
			if (EasyAccessMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				//Do stuff
			}
			return Task.FromResult(0);
		}

		public async Task<IStandardInstance> HandleCreation(IStandardInstance instance)
		{
			IStandardInstance handledInstance = new StandardTopic();
			if (!instance.Identifiers.ContainsKey(InstanceKeyNames.EASY_ACCESS_TOPIC))
			{
				Debug.WriteLine("Creating new Topic from instance from: " + instance.MessageOrigin);
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
			if (instance.Identifiers.ContainsKey(InstanceKeyNames.EASY_ACCESS_TOPIC))
			{
				var updatedInstance = await _resourceService.UpdateInstanceAsync(instance);
				return updatedInstance;
			}
			return null;
		}

		public async Task Consume(ConsumeContext<IStandardIdentifierMap> context)
		{

			if (EasyAccessMessageFilter.OfInterest(context.Message, MachineOrigin))
			{
				await _resourceService.UpdateInstanceIdMap(context.Message);
			}
		}
	}
}
