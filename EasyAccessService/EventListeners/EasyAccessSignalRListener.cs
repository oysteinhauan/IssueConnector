using EasyAccessService.Database;
using EasyAccessService.Models;
using IssueConnectorLib.Models;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using EasyAccessService.Filters;

namespace EasyAccessService.EventListeners
{
    public class EasyAccessSignalRListener : IEventListener
    {
		private IHubProxy HubProxy { get; set; }

		private HubConnection Connection { get; set; }
		private IResourcePublisher _publisher;

		private EasyAccessConfig _config;
		private IEasyAccessDatabaseService _dbService;
		
	    //TODO design a better way to identify services

		public EasyAccessSignalRListener(IResourcePublisher publisher, IEasyAccessDatabaseService dbService, IOptions<EasyAccessConfig> configOptions)
		{
			_publisher = publisher;
			_dbService = dbService;
			_config = configOptions.Value;
		}

		public async Task ConnectAsync()
		{
			Debug.WriteLine("Connecting to SignalR");
			Connection = new HubConnection(_config.SignalServiceUri, await GetConnectionString());
			Connection.Closed += Connection_Closed;

			HubProxy = Connection.CreateHubProxy("topicsHub");

			//Handle incoming event from server: use Invoke to write to console from SignalR's thread
			HubProxy.On<Topic>("NewTopic", (topic) =>
			{
				//Ignore if topic was created by IssueConnector
				if (!EasyAccessMessageFilter.IsOriginalTopic(topic)) return;
				
				//Else publish the topic created at EasyAccess
				Debug.WriteLine("Original Easy Access topic created with id: " + topic.Id);
				_publisher.OnInstanceCreatedAsync(topic.Id, topic.AuthorId, topic.ProjectId);
			}
			);

			HubProxy.On<Topic>("UpdatedTopic", (topic) => {
					
				//TODO, need a "loop-stopping-mechanism" to implement this properly.
				Debug.WriteLine("Updated topic: " + topic.Title);
			}
		   );

			HubProxy.On<Topic, string>("DeletedTopic", (topic, username) =>
				{
					//TODO
				}
			);

			HubProxy.On<Comment>("NewComment", (comment) =>
			{
				Debug.WriteLine("New comment posted. Content: " + comment.Content +
					". Status: " + comment.Status + ". Priority: " + comment.Priority);

				try
				{
					
					//Ignore if comment was created by issue connector 
					if (comment.AuthorName.Equals("IssueConnector")) return;
					
					//If comment is created at another system than easy access, ignore
					if (!EasyAccessMessageFilter.IsOriginalComment(comment)) return;
					
					//If status or priority is updated, send a message with the updated topic
					if (comment.Status != null || comment.Priority != null)
					{
						_publisher.OnInstanceUpdatedAsync(comment.TopicGuid, null, comment.ProjectId);
					}
					
					//Ignore comment if content is empty
					if (comment.Content.Length > 0)
					{
						_publisher.OnCommentCreatedAsync(comment.TopicGuid, comment.Id, comment.AuthorId, comment.ProjectId); 
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("Error publishing comment.");
					Debug.WriteLine(e.StackTrace);
				}
			}

			);

			HubProxy.On<Comment>("DeletedComment", (comment) =>
				{
					//TODO, this is probably not critical to implement at the moment
				}
			);

			HubProxy.On<ViewPoint>("NewViewPoint", (viewpoint) => {
				Debug.WriteLine("New viewpoint created: " + viewpoint.Id);
				_publisher.OnViewpointCreatedAsync(viewpoint.TopicGuid, viewpoint.Id, viewpoint.ProjectId, viewpoint.CommentGuid);
			}
			);

			HubProxy.On<ViewPoint>("DeletedViewPoint", (viewpoint) => { }
			);

			try
			{
				await Connection.Start();
			}
			catch (HttpRequestException)
			{
				Debug.WriteLine("Unable to connect to server: Start server before connecting clients.");
				//No connection
				return;
			}

		}

		private async Task<string> GetConnectionString()
		{
			//get all connected easy access projects from the IssueConnectorDb
			var projectIds = await _dbService.GetAllEasyAccessProjectIds();

			var result = "";

			//generate the string iteratively
			foreach(var id in projectIds)
			{
				result += ("&id=" + id); 
			}

			//remove the first '&' from string
			return result.Substring(1);
		}

		public void Stop()
		{
			try
			{
				Connection.Stop();
			} catch (Exception e)
			{
				Debug.WriteLine("Failed to stop signalR...", e.StackTrace);
			}
		}

		private async void Connection_Closed()
		{
			Debug.WriteLine("Connection to EasyAccess SignalR closed.");
			await ConnectAsync();
		}
	}
}
