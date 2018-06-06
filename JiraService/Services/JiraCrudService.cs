using Atlassian.Jira;
using Atlassian.Jira.Remote;
using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using IssueConnectorLib.Models.Database;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Models.Users;
using IssueConnectorLib.Services;
using JiraService.Connector;
using JiraService.Mapper;
using JiraService.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JiraService.Services
{
	public class JiraCrudService : IResourceCrudService
	{
		private IDatabaseService _dbService;
		private IResourceClient<Jira> _resourceClient;
		private ConfigSettings _configSettings;
		private IStandardInstanceMapper<IStandardInstance, Issue> _issueMapper;
		private IStandardCommentMapper<IStandardComment, Comment> _commentMapper;

		public JiraCrudService(
			IDatabaseService dbService, 
			IOptions<ConfigSettings> configSettingsOptions, 
			IResourceClient<Jira> client, 
			IStandardInstanceMapper<IStandardInstance, Issue> issueMapper,
			IStandardCommentMapper<IStandardComment, Comment> commentMapper)
		{
			_configSettings = configSettingsOptions.Value;
			_dbService = dbService;
			_resourceClient = client;
			_issueMapper = issueMapper;
			_commentMapper = commentMapper;
		}

		

		public Task<IStandardAttachment> CreateAttachmentAsync(IStandardAttachment instance)
		{
			throw new NotImplementedException();
		}

		public async Task<IStandardComment> CreateCommentAsync(IStandardComment comment)
		{
			var jiraIssue = await _resourceClient.RestClient.Issues.GetIssueAsync(comment.Identifiers[InstanceKeyNames.JIRA_ISSUE].Id);


			var jiraComment = await _commentMapper.ToNewSpecialComment(comment);

			//Hack to get the right user to post comment, because JIRA doesnt allow custom author out-of-the-box.
			//Using extension "Extender for JIRA"
			RemoteComment remoteComment = new RemoteComment()
			{
				author = jiraComment.Author,
				body = jiraComment.Body,
				roleLevel = jiraComment.RoleLevel,
				groupLevel = jiraComment.GroupLevel
			};

			var resource = String.Format("rest/api/2/issue/{0}/comment", jiraIssue.Key);

			var request = new RestRequest(resource, Method.POST);
			request.AddHeader("contextUser", comment.Author.UserMap[_configSettings.UserColumnName]);
			request.RequestFormat = DataFormat.Json;
			request.JsonSerializer = new RestSharpJsonSerializer(JsonSerializer.Create(_resourceClient.RestClient.RestClient.Settings.JsonSerializerSettings));
			request.AddJsonBody(remoteComment);

			var response = await _resourceClient.RestClient.RestClient.RestSharpClient.ExecuteTaskAsync(request);

			// End hack

			//var createdComment = await jiraIssue.AddCommentAsync(jiraComment);

			

			return comment;
		}

		public async Task<IStandardInstance> CreateInstanceAsync(IStandardInstance instance)
		{

			var newJiraIssue = await _issueMapper.ToNewSpecialObject(instance);

			try
			{
				var createdIssue = await newJiraIssue.SaveChangesAsync();
				try
				{
					if(instance.Status != null) await createdIssue.WorkflowTransitionAsync(instance.Status);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Error setting status. Value of status in instance: " + instance.Status);
					Debug.WriteLine(e.StackTrace);
				}

				instance.Identifiers[InstanceKeyNames.JIRA_ISSUE] = new Identifiers()
				{
					Id = createdIssue.Key.Value,
					ProjectId = createdIssue.Project,
					Url = _configSettings.HomeUri + "browse/" + createdIssue.Key.Value
				};
			}
			catch (Exception e)
			{
				Debug.WriteLine("Failed to save new issue...");
				Debug.WriteLine(e.StackTrace);
			}

			return instance;

		}

		public async Task<IStandardInstance> ReadInstanceAsync(string issueId, string projectId = null)
		{
			var jiraIssue = await _resourceClient.RestClient.Issues.GetIssueAsync(issueId);
	
			return await _issueMapper.ToStandardObject(jiraIssue);
		}

		public async Task<IStandardViewpoint> CreateViewpointAsync(IStandardViewpoint instance)
		{
			var jiraIssue = await _resourceClient.RestClient.Issues.GetIssueAsync(instance.Identifiers[InstanceKeyNames.JIRA_ISSUE].Id);

			using (WebClient client = new WebClient())
			{
				try
				{
					byte[] img = client.DownloadData(new Uri(instance.Url));


					UploadAttachmentInfo[] info = new UploadAttachmentInfo[1] {
					new UploadAttachmentInfo("viewpoint_" + instance.ViewpointId + ".png",
					img)
					};

					await jiraIssue.AddAttachmentAsync(info);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Failed to upload viewpoint attachment.");
				}
			}



			return instance;

		}

		public void DeleteAttachmentAsync(IStandardAttachment instance)
		{
			throw new NotImplementedException();
		}

		public void DeleteCommentAsync(IStandardComment comment)
		{
			throw new NotImplementedException();
		}

		public async void DeleteInstanceAsync(IStandardInstance instance)
		{
			await _resourceClient.RestClient.Issues.DeleteIssueAsync(instance.Identifiers[InstanceKeyNames.JIRA_ISSUE].Id);
		}

		public void DeleteViewpointAsync(IStandardViewpoint instance)
		{
			throw new NotImplementedException();
		}

		public Task<IStandardAttachment> ReadAttachmentAsync(string instanceId)
		{
			throw new NotImplementedException();
		}

		public Task<IStandardComment> ReadCommentAsync(string commentId)
		{
			throw new NotImplementedException();
		}



		public Task<IStandardViewpoint> ReadViewpointAsync(string instanceId)
		{
			throw new NotImplementedException();
		}


		public Task<IStandardAttachment> UpdateAttachmentAsync(IStandardAttachment instance)
		{
			throw new NotImplementedException();
		}

		public Task<IStandardComment> UpdateCommentAsync(IStandardComment comment)
		{
			throw new NotImplementedException();
		}

		public async Task<IStandardInstance> UpdateInstanceAsync(IStandardInstance instance)
		{
			var issueToBeUpdated = await _resourceClient.RestClient.Issues.GetIssueAsync(instance.Identifiers[InstanceKeyNames.JIRA_ISSUE].Id);
			var updatedIssue = await _issueMapper.ToUpdatedSpecialObject(instance, issueToBeUpdated);

			await updatedIssue.SaveChangesAsync();
			if (instance.Status != null && instance.Status.ToLower() != updatedIssue.Status?.Name?.ToLower())
			{
				await updatedIssue.WorkflowTransitionAsync(instance.Status);
			}

			return null;
		}
		public async Task UpdateInstanceIdMap(IStandardIdentifierMap idMap)
		{
			try
			{
				var issueKey = idMap.InstanceMap[InstanceKeyNames.JIRA_ISSUE].Id;
				var jiraIssue = await _resourceClient.RestClient.Issues.GetIssueAsync(issueKey);
				foreach (var id in idMap.InstanceMap)
				{
					if (!id.Key.Equals(InstanceKeyNames.JIRA_ISSUE))
					{
						jiraIssue[id.Key + "Id"] = id.Value.Id;
						jiraIssue[id.Key + "Url"] = id.Value.Url;
					}
				}
				await jiraIssue.SaveChangesAsync();
			}
			catch (Exception e)
			{
				Debug.WriteLine("Failed to extract jira issuekey", e.StackTrace);
			}
		}

		public Task<IStandardViewpoint> UpdateViewpointAsync(IStandardViewpoint instance)
		{
			throw new NotImplementedException();
		}

		private Issue CompareAndUpdateIssue(Issue issueToBeUpdated, Issue updatedIssue)
		{
			if (issueToBeUpdated == null || updatedIssue == null)
			{
				throw new ArgumentNullException();
			}
			Type type = issueToBeUpdated.GetType();
			foreach (PropertyInfo pInfo in type.GetProperties())
			{
				if (pInfo.CanRead)
				{
					//TODO
				}
			}
			return null;
		}

		public async Task<IStandardComment> ReadCommentAsync(string commentId, string instanceId, string projectId = null)
		{
			var jiraComment = (await _resourceClient.RestClient.Issues.GetCommentsAsync(instanceId)).FirstOrDefault(c => c.Id.Equals(commentId));

			var standardComment = await _commentMapper.ToStandardComment(jiraComment, instanceId);

			return standardComment;
		}

		private static readonly Random getrandom = new Random();

		public static int GetRandomNumber(int min, int max)
		{
			lock (getrandom) // synchronize
			{
				return getrandom.Next(min, max);
			}
		}

		public Task<IStandardViewpoint> ReadViewpointAsync(string instanceId, string projectId, string commentId, string viewpointId)
		{
			throw new NotImplementedException();
		}

	}
}
