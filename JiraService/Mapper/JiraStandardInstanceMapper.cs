using Atlassian.Jira;
using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Models.Users;
using IssueConnectorLib.Services;
using JiraService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JiraService.Mapper
{
	public class JiraStandardInstanceMapper : IStandardInstanceMapper<IStandardInstance, Issue>
	{

		private IDatabaseService _dbService;
		private IResourceClient<Jira> _resourceClient;
		private ConfigSettings _configSettings;

		public JiraStandardInstanceMapper(IDatabaseService dbService, IResourceClient<Jira> client, IOptions<ConfigSettings> configSettingsOptions)
		{
			_dbService = dbService;
			_resourceClient = client;
			_configSettings = configSettingsOptions.Value;
		}
		public async Task<Issue> ToNewSpecialObject(IStandardInstance instance)
		{
			var projectKey = await GetProjectId(instance);
			var newJiraIssue = new Issue(_resourceClient.RestClient, projectKey)
			{
				Type = instance.Type,
				Reporter = instance.Author?.UserMap[_configSettings.UserColumnName],
				Summary = "[" + instance.MessageOrigin.Split('_')[0] + "] | " + instance.Summary,
				Priority = instance.Priority
			};

			//Labels
			if (instance.Labels != null)
			{
				newJiraIssue.Labels.AddRange(instance.Labels);
			}

			//Assignee
			newJiraIssue.Assignee = instance.Assignee?.UserMap[_configSettings.UserColumnName];

			//Identifiers
			foreach (var identifier in instance.Identifiers)
			{
				if (identifier.Value.Id != null)
				{
					try
					{
						newJiraIssue[identifier.Key + "Id"] = identifier.Value.Id;
						newJiraIssue[identifier.Key + "Url"] = identifier.Value.Url;
					}
					catch (Exception e)
					{
						Debug.WriteLine("Error adding identifier to field:");
						Debug.WriteLine(e.StackTrace);
					}
				}
			}

			return newJiraIssue;
		}

		public async Task<IStandardInstance> ToStandardObject(Issue jiraIssue)
		{

			var author = (jiraIssue.Reporter == null) ? null : await GetUserMap(jiraIssue.Reporter);
			var assignee = (jiraIssue.Assignee == null) ? null : await GetUserMap(jiraIssue.Assignee);

			var mappedInstance = new JiraStandardIssue()
			{

				Author = author,
				Created = jiraIssue.Created.Value,
				Summary = jiraIssue.Summary,
				Type = jiraIssue.Type.Name,
				Assignee = assignee,
				Status = jiraIssue.Status.Name,
				Priority = jiraIssue.Priority.Name,
				Labels = jiraIssue.Labels.ToArray(),
				Identifiers = GetIdentifiers(jiraIssue, _configSettings.HomeUri)
			};


			return mappedInstance;
		}

		public static Dictionary<string, Identifiers> GetIdentifiers(Issue jiraIssue, string baseUrl)
		{

			var result = new Dictionary<string, Identifiers>
			{
				{
					InstanceKeyNames.JIRA_ISSUE,
					new Identifiers()
					{
						Id = jiraIssue.Key.Value,
						ProjectId = jiraIssue.Project,
						Url = baseUrl + "browse/" + jiraIssue.Key.Value
					}
				}
			};
			FieldInfo[] fields = typeof(InstanceKeyNames).GetFields(BindingFlags.NonPublic |
					   BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				var fieldValue = field.GetValue(null).ToString();
				if (!fieldValue.Equals(InstanceKeyNames.JIRA_ISSUE))
				{

					try
					{
						if (jiraIssue[fieldValue + "Id"]?.Value != null)
						{
							result.Add(field.GetValue(null).ToString(), new Identifiers()
							{
								Id = jiraIssue[fieldValue + "Id"].ToString(),
							});
						}
					}
					catch (InvalidOperationException e)
					{
						Debug.WriteLine("Couldn't find custom field for " + fieldValue);
					} catch(Exception e)
					{
						Debug.WriteLine("Error adding id for " + fieldValue);
						Debug.WriteLine(e.StackTrace);
					}
				}
			}

			return result;
		}

		public async Task<Issue> ToUpdatedSpecialObject(IStandardInstance instance, Issue oldIssue)
		{
			string summary = GetSummary(instance);

			oldIssue.Type = instance.Type;
			oldIssue.Summary = summary;

			oldIssue.Reporter = instance.Author?.UserMap[_configSettings.UserColumnName];

			oldIssue.Priority = instance.Priority ?? oldIssue.Priority;

			oldIssue.Assignee = instance.Assignee?.UserMap[_configSettings.UserColumnName];

			return oldIssue;
		}

		private async Task<string> GetProjectId(IStandardInstance instance)
		{
			var projectmappings = await _dbService.GetAllProjectMappings();
			var externalProjectId = "";
			foreach (var id in instance.Identifiers)
			{
				if (id.Value.ProjectId != null)
				{
					externalProjectId = id.Value.ProjectId;
					break;
				}
			}

			foreach (var mapping in projectmappings.Mappings)
			{
				if (mapping.Contains(externalProjectId))
				{
					return mapping.JiraProjectKey;
				}
			}
			return null;
		}

		private async Task<ClientUser> GetUserMap(string user)
		{
			return await _dbService.GetClientUserFrom(_configSettings.UserColumnName, user);
		}

		private static string GetSummary(IStandardInstance instance)
		{
			var summary = instance.Summary;

			if (summary.StartsWith("[JIRA]"))
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
	}
}
