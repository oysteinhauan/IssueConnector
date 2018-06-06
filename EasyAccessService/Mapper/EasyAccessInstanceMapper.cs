using EasyAccessService.Database;
using EasyAccessService.Models;
using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Users;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyAccessService.Mapper
{
	public class EasyAccessInstanceMapper : IEasyAccessStandardInstanceMapper
	{

		private EasyAccessConfig _configSettings;
		private IEasyAccessDatabaseService _dbService;
		private const string EasyAccessBaseUrl = "https://ea.np.trimble.com/EasyAccess/";

		public EasyAccessInstanceMapper(IOptions<EasyAccessConfig> configSettingsOptions,
			IEasyAccessDatabaseService dbService)
		{
			_configSettings = configSettingsOptions.Value;
			_dbService = dbService;
		}

		public static Dictionary<string, Identifiers> GetIdentifiers(string topicId, string projectId, List<Comment> comments)
		{
			var topicUrl = "Project/" + projectId +"/Topics#/list/topic/" + topicId;

			var result = new Dictionary<string, Identifiers>();

			result.Add(InstanceKeyNames.EASY_ACCESS_TOPIC, new Identifiers()
			{
				Id = topicId,
				ProjectId = projectId,
				Url = EasyAccessBaseUrl + topicUrl
			});
			FieldInfo[] fields = typeof(InstanceKeyNames).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			foreach (var field in fields)
			{
				foreach (var comment in comments)
				{
					if (comment.Content != null && comment.Content.Contains(field.GetValue(null).ToString() + "Id"))
					{
						result.Add(field.GetValue(null).ToString(), new Identifiers() { Id = comment.Content.Split('_')[1] });
						break;
					}
				}
			}
			return result;
		}



		private static string GetTitle(IStandardInstance instance)
		{
			var summary = instance.Summary;

			if (summary.StartsWith("[EasyAccess]"))
			{
				return summary.Substring(summary.IndexOf('|') + 1);

			}
			else if (summary.StartsWith('[') && summary.Contains("] | "))
			{
				return summary;
			}
			else
			{
				return "[" + instance.MessageOrigin.Split('_')[0] + "] | " + summary;
			}
		}

		public async Task<IStandardInstance> ToStandardObject(Topic topic, List<Comment> comments)
		{
			var author = topic.AuthorId == null ? null : await _dbService.GetClientUserFrom(_configSettings.UserColumnName, topic.AuthorId); 

			var assignee = topic.AssignedToId == null ? null : await _dbService.GetClientUserFrom(_configSettings.UserColumnName, topic.AssignedToId);

			var instance = new StandardTopic()
			{
				Identifiers = GetIdentifiers(topic.Id, topic.ProjectId, comments),
				Author = author,
				Assignee = assignee,
				Created = topic.CreationDate.Value,
				Summary = topic.Title,
				Type = topic.TopicType,
				Status = topic.Status,
				Priority = topic.Priority,
				Labels = topic.Labels
			};

			return instance;
		}

		public async Task<Topic> ToNewSpecialObject(IStandardInstance instance)
		{

			string projectId = await ProjectIdProvider.GetProjectId(instance.Identifiers, await _dbService.GetAllProjectMappings());

			var author = instance.Author == null ? null : await _dbService.GetEasyAccessUserFrom(instance.Author.UserMap[_configSettings.UserColumnName]);

			var assignee = instance.Assignee == null ? null : await _dbService.GetEasyAccessUserFrom(instance.Assignee.UserMap[_configSettings.UserColumnName]);

			var newTopic = new Topic()
			{
				Status = instance.Status,
				Priority = instance.Priority,

				AuthorId = author?.Id,
				AuthorName = author?.Name,
				AuthorEmail = author?.Email,

				AssignedToId = assignee?.Id,
				AssignedToEmail = assignee?.Email,
				AssignedToName = assignee?.Name,

				CreationDate = instance.Created,
				Title = "[" + instance.MessageOrigin.Split('_')[0] + "] | " + instance.Summary,
				TopicType = instance.Type,
				ProjectId = projectId,
				Labels = instance.Labels
			};

			return newTopic;
		}

		public async Task<Topic> ToUpdatedSpecialObject(IStandardInstance instance, Topic oldTopic)
		{

			var assignee = instance.Assignee == null ? null : await _dbService.GetEasyAccessUserFrom(instance.Assignee.UserMap[_configSettings.UserColumnName]);
			var author = instance.Author == null ? null : await _dbService.GetEasyAccessUserFrom(instance.Author.UserMap[_configSettings.UserColumnName]);

			var result = oldTopic;

			result.TopicType = instance.Type;

			result.Title = GetTitle(instance);

			result.AssignedToId = assignee?.Id;
			result.AssignedToName = assignee?.Name;
			result.AssignedToEmail = assignee?.Email;

			result.AuthorId = author?.Id;
			result.AuthorEmail = author?.Email;
			result.AuthorName = author?.Name;

			result.Labels = instance.Labels;

			return result;
		}
	}
}
