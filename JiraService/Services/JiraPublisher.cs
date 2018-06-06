using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using IssueConnectorLib.Models.SystemInfo;
using IssueConnectorLib.Services;
using JiraService.Models;
using MassTransit.Log4NetIntegration.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using IssueConnectorLib.Services.ServiceBus;

namespace JiraService.Services
{
    public class JiraPublisher : IResourcePublisher
    {

		private IResourceCrudService _jiraService;
	    private ConfigSettings _config;
		private IDatabaseService _dbService;

		public JiraPublisher(IResourceCrudService jiraService, IDatabaseService databaseService, IOptions<ConfigSettings> configSettingOptions)
		{
			_dbService = databaseService;
			_jiraService = jiraService;
			_config = configSettingOptions.Value;
		}

		public async Task OnInstanceCreatedAsync(string issueId, string user, string projectId = null)
		{
			Log4NetLogger.Use();
			var createdInstance = await _jiraService.ReadInstanceAsync(issueId);
				createdInstance.MessageOrigin = GetMessageOrigin();
				createdInstance.Action = "Created";

			await PublishInstance(createdInstance);

		}

		public async Task OnInstanceUpdatedAsync(string instanceId, string user, string projectId = null)
		{
			Log4NetLogger.Use();
			var updatedInstance = await _jiraService.ReadInstanceAsync(instanceId);
			updatedInstance.MessageOrigin = GetMessageOrigin();
			updatedInstance.Action = "Updated";
			updatedInstance.ModifiedUser = await _dbService.GetClientUserFrom(_config.UserColumnName, user);

			Debug.WriteLine("Publishing updated instance from " + GetMessageOrigin() + " with id: " + instanceId);

			await PublishInstance(updatedInstance);
		}

		public async Task OnInstanceDeletedAsync(string instanceId, string user, string projectid = null)
		{

		}

		public async Task OnCommentCreatedAsync(string instanceId, string commentId, string user, string projectId = null)
		{
			var comment = await _jiraService.ReadCommentAsync(commentId, instanceId);

			if (!(comment.Content.StartsWith('[') && comment.Content.Contains("] | ")))
			{
				await PublishInstance<IStandardComment>(comment); 
			}
		}

		public Task OnCommentUpdatedAsync(string instanceId, string commentId, string origin)
		{
			throw new NotImplementedException();
		}

		public Task OnCommentDeletedAsync(string instanceId, string commentId, string origin)
		{
			throw new NotImplementedException();
		}

		public async Task PublishKeyMap(Dictionary<string, Identifiers> identifiers)
		{

			await PublishInstance(new StandardIdentifierMap()
			{
				InstanceMap = identifiers,
				MessageOrigin = GetMessageOrigin()
			});
		}

		private async Task PublishInstance<T>(T instance) where T : class
		{
			var bus = ServiceBusProvider.GetRabbitMqPublisherBus(_config.RabbitMqUri);
			var busHandler = await bus.StartAsync();

			await bus.Publish<T>(instance);

			await busHandler.StopAsync();
		}

		public Task OnViewpointCreatedAsync(string instanceId, string viewpointId, string projectId, string commentId)
		{
			throw new NotImplementedException();
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
