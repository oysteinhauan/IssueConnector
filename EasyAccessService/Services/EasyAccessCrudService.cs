using EasyAccessService.Connector;
using EasyAccessService.Database;
using EasyAccessService.Mapper;
using EasyAccessService.Models;
using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Exceptions;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Models.ProjectMappings;
using IssueConnectorLib.Models.Users;
using IssueConnectorLib.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyAccessService.Services
{
	public class EasyAccessCrudService : IResourceCrudService
	{

		private IResourceClient<HttpClient> _resourceClient;
		private EasyAccessConfig _settings;
		private IEasyAccessDatabaseService _dbService;
		private IEasyAccessCommentMapper _commentMapper;
		private IEasyAccessStandardInstanceMapper _instanceMapper;


		private const string userColumnName = "easy_access_user_id";

		public EasyAccessCrudService(IOptions<EasyAccessConfig> options,
			IEasyAccessDatabaseService dbService,
			IEasyAccessStandardInstanceMapper instanceMapper,
			IEasyAccessCommentMapper commentMapper,
			IResourceClient<HttpClient> easyAccessResourceClient)
		{
			_dbService = dbService;
			_settings = options.Value;
			_instanceMapper = instanceMapper;
			_commentMapper = commentMapper;
			_resourceClient = easyAccessResourceClient;
		}

		private async Task IssueDeletor()
		{
			string requestPath = "projects/636561988418328110/topics";

			var response = await _resourceClient.RestClient.GetAsync(requestPath);

			var topics = await response.Content.ReadAsAsync<List<Topic>>();

			foreach (var topic in topics)
			{
				string deletePath = "projects/636561988418328110/topics/" + topic.Id;

				var deleteResponse = _resourceClient.RestClient.DeleteAsync(deletePath);
			}
		}


		public async Task<IStandardInstance> CreateInstanceAsync(IStandardInstance instance)
		{

			try
			{

				var newTopic = await _instanceMapper.ToNewSpecialObject(instance);

				string requestUri = "projects/" + newTopic.ProjectId + "/topics";

				string json = newTopic.ToJson();

				Debug.WriteLine("creating topic: projectId=" + newTopic.ProjectId + ", title=" + newTopic.Title);

				var response = await _resourceClient.RestClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8,
												"application/json"));

				var createdTopic = await response.Content.ReadAsAsync<Topic>();

				var topicUrl = "Project/" + createdTopic.ProjectId + "/Topics#/list/topic/" + createdTopic.Id;

				instance.Identifiers.Add(InstanceKeyNames.EASY_ACCESS_TOPIC, new Identifiers()
				{
					Id = createdTopic.Id,
					ProjectId = createdTopic.ProjectId,
					Url = _settings.HomeUri + topicUrl
				});

				await UpdateInstanceIdMap(instance.Identifiers);

				return instance;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Failed to create instance..", e.StackTrace);
			}
			return null;
		}


		public void DeleteInstanceAsync(IStandardInstance instance)
		{
			throw new NotImplementedException();
		}

		public async Task<IStandardInstance> ReadInstanceAsync(string instanceId, string projectId)
		{
			Debug.WriteLine("Getting topic with id " + instanceId + " in project " + projectId);

			string requestUri = "projects/" + projectId + "/topics/" + instanceId;

			var response = await _resourceClient.RestClient.GetAsync(requestUri);

			if (!response.IsSuccessStatusCode) throw new ResourceException("Error getting instance at " + requestUri + ". StatusCode=" + response.StatusCode);

			var topic = await response.Content.ReadAsAsync<Topic>();

			//Need comments to find connected issues and identifiers
			var comments = await GetAllCommentsForTopic(instanceId, projectId);

			return await _instanceMapper.ToStandardObject(topic, comments);
		}

		private async Task<ClientUser> GetUserMap(string user)
		{
			return await _dbService.GetClientUserFrom(userColumnName, user);
		}

		public async Task<List<Comment>> GetAllCommentsForTopic(string topicId, string projectId)
		{
			Debug.WriteLine("Getting all comments for topic: " + topicId);
			string requestUri = "https://topics.quadridcm.com/api/projects/" + projectId + "/topics/" + topicId + "/comments";

			var response = await _resourceClient.RestClient.GetAsync(requestUri);

			var comments = await response.Content.ReadAsAsync<List<Comment>>();

			return comments;
		}


		public async Task<IStandardInstance> UpdateInstanceAsync(IStandardInstance instance)
		{
			try
			{
				string projectId = await ProjectIdProvider.GetProjectId(instance.Identifiers, await _dbService.GetAllProjectMappings());

				string requestUri = "projects/" + projectId + "/topics/" + instance.Identifiers[InstanceKeyNames.EASY_ACCESS_TOPIC].Id;

				//fetch the old topic first
				var oldTopicResponse = await _resourceClient.RestClient.GetAsync(requestUri);

				if (!oldTopicResponse.IsSuccessStatusCode) throw new ResourceException("Error getting instance at " + requestUri +". StatusCode=" + oldTopicResponse.StatusCode);

				var oldTopic = await oldTopicResponse.Content.ReadAsAsync<Topic>();

				//map the instance and merge it into the old topic
				var topicToBeUpdated = await _instanceMapper.ToUpdatedSpecialObject(instance, oldTopic);

				string json = topicToBeUpdated.ToJson();

				Debug.WriteLine("Topic to be updated: id=" + topicToBeUpdated.Id + ", projectId=" + topicToBeUpdated.ProjectId + ", title=" + topicToBeUpdated.Title);

				//update the topic
				var updatedTopicResponse = await _resourceClient.RestClient.PutAsync(requestUri, new StringContent(json, Encoding.UTF8,
										"application/json"));

				if (!updatedTopicResponse.IsSuccessStatusCode) throw new ResourceException("Error updating instance with id=" + topicToBeUpdated.Id + ". StatusCode=" + updatedTopicResponse.StatusCode);

				var updatedTopic = await updatedTopicResponse.Content.ReadAsAsync<Topic>();

				//EasyAccess status and priority is updated via comments.
				await UpdateStatusAndPriority(instance, updatedTopic);

				Debug.WriteLine("Topic was updated.");
			}
			catch (Exception e)
			{
				Debug.WriteLine("Error updating topic...");
				Debug.WriteLine(e.Message, e.StackTrace);
			}

			return instance;
		}

		private async Task UpdateStatusAndPriority(IStandardInstance instance, Topic updatedTopic)
		{
			var modifiedUser = (instance.ModifiedUser == null) ? null : await _dbService.GetEasyAccessUserFrom(instance.ModifiedUser.UserMap[_settings.UserColumnName]);

			if(!(updatedTopic.Status?.ToLower() == instance.Status?.ToLower()))
			{
				await UpdateStatusOfTopic(updatedTopic, instance.Status, modifiedUser);
			}

			if (!(updatedTopic.Priority?.ToLower() == instance.Priority?.ToLower()))
			{
				await UpdatePriorityOfTopic(updatedTopic, instance.Priority, modifiedUser);
			}
		}

		public async Task UpdateInstanceIdMap(Dictionary<string, Identifiers> idMap)
		{
			var topicId = idMap[InstanceKeyNames.EASY_ACCESS_TOPIC].Id;
			var projectId = idMap[InstanceKeyNames.EASY_ACCESS_TOPIC].ProjectId;

			foreach (var id in idMap)
			{
				if (!id.Key.Equals(InstanceKeyNames.EASY_ACCESS_TOPIC))
				{
					var idComment = new Comment()
					{
						TopicGuid = topicId,
						ProjectId = projectId,
						AuthorName = "IssueConnector",
						Content = id.Key + "Id_" + id.Value.Id,
						Date = DateTime.Now
					};

					var urlComment = new Comment()
					{
						TopicGuid = topicId,
						ProjectId = projectId,
						AuthorName = "IssueConnector",
						Content = id.Key + "Url: " + id.Value.Url,
						Date = DateTime.Now
					};
					await PostComment(idComment);
					await PostComment(urlComment);

				}
			}
		}

		public async Task UpdateInstanceIdMap(IStandardIdentifierMap idMap)
		{
			await UpdateInstanceIdMap(idMap.InstanceMap);
		}

		private async Task PostComment(Comment comment)
		{
			string requestUri = "projects/" + comment.ProjectId + "/topics/" + comment.TopicGuid + "/comments";

			string json = comment.ToJson();

			var response = await _resourceClient.RestClient.PostAsync(requestUri, new StringContent(json, Encoding.UTF8,
										"application/json"));

			var createdComment = await response.Content.ReadAsAsync<Comment>();

		}

		public async Task UpdateStatusOfTopic(Topic topic, string newStatus, EasyAccessUser madeUpdate)
		{
			string requestUri = "projects/" + topic.ProjectId + "/topics/" + topic.Id + "/comments";

			var comment = new Comment()
			{
				TopicGuid = topic.Id,
				ProjectId = topic.ProjectId,
				Date = DateTime.Now,
				AuthorName = madeUpdate.Name,
				AuthorEmail = madeUpdate.Email,
				AuthorId = madeUpdate.Id,
				Status = newStatus

			};

			await PostComment(comment);
		}

		public async Task UpdatePriorityOfTopic(Topic topic, string newPriority, EasyAccessUser madeUpdate)
		{
			string requestUri = "projects/" + topic.ProjectId + "/topics/" + topic.Id + "/comments";

			var comment = new Comment()
			{
				TopicGuid = topic.Id,
				ProjectId = topic.ProjectId,
				Date = DateTime.Now,
				AuthorName = madeUpdate.Name,
				AuthorEmail = madeUpdate.Email,
				AuthorId = madeUpdate.Id,
				Priority = newPriority

			};

			await PostComment(comment);
		}

		public async Task<IStandardComment> CreateCommentAsync(IStandardComment comment)
		{
			var eaComment = await _commentMapper.ToNewSpecialComment(comment);
			await PostComment(eaComment);
			return comment;
		}

		public Task<IStandardComment> UpdateCommentAsync(IStandardComment comment)
		{
			throw new NotImplementedException();
		}

		public void DeleteCommentAsync(IStandardComment comment)
		{
			throw new NotImplementedException();
		}

		public async Task<IStandardComment> ReadCommentAsync(string commentId, string instanceId, string projectId)
		{
			string requestPath = "projects/" + projectId + "/topics/" + instanceId + "/comments/" + commentId;

			var response = await _resourceClient.RestClient.GetAsync(requestPath);

			var comment = await response.Content.ReadAsAsync<Comment>();

			string requestPathViewPoints = "projects/" + projectId + "/topics/" + instanceId + "/comments/" + commentId + "/viewpoints";

			//Hack to wait for the viewpoints to be registered...
			Thread.Sleep(2000);

			var vpResponse = await _resourceClient.RestClient.GetAsync(requestPathViewPoints);

			var viewpoints = await vpResponse.Content.ReadAsAsync<List<ViewPoint>>();

			var standardComment = await _commentMapper.ToStandardComment(comment, await GetAllCommentsForTopic(instanceId, projectId), viewpoints);

			return standardComment;

		}

		public Task<IStandardAttachment> CreateAttachmentAsync(IStandardAttachment instance)
		{
			throw new NotImplementedException();
		}

		public Task<IStandardAttachment> UpdateAttachmentAsync(IStandardAttachment instance)
		{
			throw new NotImplementedException();
		}

		public void DeleteAttachmentAsync(IStandardAttachment instance)
		{
			throw new NotImplementedException();
		}

		public Task<IStandardAttachment> ReadAttachmentAsync(string instanceId)
		{
			throw new NotImplementedException();
		}

		public Task<IStandardViewpoint> CreateViewpointAsync(IStandardViewpoint instance)
		{
			throw new NotImplementedException();

		}

		public Task<IStandardViewpoint> UpdateViewpointAsync(IStandardViewpoint instance)
		{
			throw new NotImplementedException();
		}

		public void DeleteViewpointAsync(IStandardViewpoint instance)
		{
			throw new NotImplementedException();
		}


		public async Task<IStandardViewpoint> ReadViewpointAsync(string instanceId, string projectId, string commentId, string viewpointId)
		{
			string requestPath = "projects/" + projectId + "/topics/" + instanceId + "/comments/" + commentId + "/viewpoints/" + viewpointId;
			string commentsRequestPath = "projects/" + projectId + "/topics/" + instanceId + "/comments";

			var response = await _resourceClient.RestClient.GetAsync(requestPath);
			var commentsResponse = await _resourceClient.RestClient.GetAsync(commentsRequestPath);

			var viewPoint = await response.Content.ReadAsAsync<ViewPoint>();
			var comments = await commentsResponse.Content.ReadAsAsync<List<Comment>>();

			var standardViewpoint = EasyAccessViewpointMapper.ToStandardViewpoint(viewPoint, comments);

			return standardViewpoint;

		}
	}
}
