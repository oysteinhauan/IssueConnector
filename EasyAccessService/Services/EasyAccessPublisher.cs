using EasyAccessService.Models;
using IssueConnectorLib;
using IssueConnectorLib.Exceptions;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using IssueConnectorLib.Models.SystemInfo;
using MassTransit.Log4NetIntegration.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IssueConnectorLib.Services.ServiceBus;

namespace EasyAccess.Services
{
    public class EasyAccessPublisher : IResourcePublisher 

	{

		private IResourceCrudService _easyAccessService;
		private EasyAccessConfig _config;

		public EasyAccessPublisher(IResourceCrudService easyAccessService, IOptions<EasyAccessConfig> configOptions)
		{
			_easyAccessService = easyAccessService;
			_config = configOptions.Value;
		}

		public async Task OnInstanceCreatedAsync(string topicId, string user, string projectId)
		{
			try
			{
				Log4NetLogger.Use();
				var createdInstance = await _easyAccessService.ReadInstanceAsync(topicId, projectId);
				
				//If the instance contains more than one identifier, it was not created originally at EasyAccess
				if (createdInstance.Identifiers.Count > 1)
				{
					Debug.WriteLine("Instance was created at another origin than " + GetMessageOrigin());
				}
				else
				{
					createdInstance.MessageOrigin = GetMessageOrigin();
					createdInstance.Action = "Created";

					await PublishInstance(createdInstance);
				}
			}
			catch (ResourceException e)
			{
				Debug.WriteLine(e.Message, e.StackTrace);
			}catch(Exception e)
			{
				Debug.WriteLine("Error publishing instance..", e.StackTrace);
			}

		}

		public async Task OnInstanceUpdatedAsync(string instanceId, string user, string projectId)
		{
			var updatedInstance = await _easyAccessService.ReadInstanceAsync(instanceId, projectId);

			updatedInstance.MessageOrigin = GetMessageOrigin();
			updatedInstance.Action = "Updated";

			await PublishInstance(updatedInstance);

		}

		public async Task OnInstanceDeletedAsync(string instanceId, string user, string projectId = null)
		{
			//TODO add deletion functionality - but this MUST be fail-proof to avoid "disaster"
		}

		public async Task OnCommentCreatedAsync(string instanceId, string commentId, string user, string projectId = null)
		{

			var createdComment = await _easyAccessService.ReadCommentAsync(commentId, instanceId, projectId);
			createdComment.MessageOrigin = GetMessageOrigin();

			await PublishInstance(createdComment);

		}

		public Task OnCommentUpdatedAsync(string instanceId, string commentId, string user)
		{
			throw new NotImplementedException();
		}

		public Task OnCommentDeletedAsync(string instanceId, string commentId, string user)
		{
			throw new NotImplementedException();
		}
		
		//publish the message to the queue
		private async Task PublishInstance<T>(T instance) where T : class
		{
			//TODO change this to ServiceBusProvider.GetAzureBus(string hostUri, string accessKeyname, string accessKey) to use with azure service bus
			var bus = ServiceBusProvider.GetRabbitMqPublisherBus(_config.RabbitMqUri);
			var busHandler = await bus.StartAsync();

			await bus.Publish(instance);

			await busHandler.StopAsync();
		}

		public async Task PublishKeyMap(Dictionary<string, Identifiers> identifiers)
		{
			var bus = ServiceBusProvider.GetRabbitMqPublisherBus(_config.RabbitMqUri);
			var busHandler = await bus.StartAsync();

			await bus.Publish<IStandardIdentifierMap>(new StandardIdentifierMap() {
				InstanceMap = identifiers,
				MessageOrigin = GetMessageOrigin()
			});

			await busHandler.StopAsync();
		}

		public async Task OnViewpointCreatedAsync(string instanceId, string viewpointId, string projectId, string commentId)
		{
			var viewpoint = await _easyAccessService.ReadViewpointAsync(instanceId, projectId, commentId, viewpointId);
			await PublishInstance(viewpoint);
		}

		public Task OnViewpointDeletedAsync(string instanceId, string viewpointId, string projectId, string commentId)
		{
			throw new NotImplementedException();
		}

		private string GetMessageOrigin()
		{
			return _config.SystemName + "_" + MachineInfo.GetMacAddress();
		}
	}

}
