using Atlassian.Jira;
using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Users;
using JiraService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JiraService.Mapper
{
    public class StandardInstanceMapper
    {
		private const string userColumnName = "jira_user_name";

		public static IStandardInstance ToStandardInstance(Issue jiraIssue, ClientUser author, ClientUser assignee, string jiraBaseUrl)
		{
			var mappedInstance = new JiraStandardIssue() {

				Author = author,
				Created = jiraIssue.Created.Value,
				Summary = jiraIssue.Summary,
				Identifiers = new Dictionary<string, Identifiers>(),
				Type = jiraIssue.Type.Name,
				Assignee = assignee,
				Status = jiraIssue.Status.Name,
				Priority = jiraIssue.Priority.Name,
				Labels = jiraIssue.Labels.ToArray()
			};

			SetIdentifiers(jiraIssue, mappedInstance, jiraBaseUrl);
			return mappedInstance;
		}

		private static void SetIdentifiers(Issue jiraIssue, JiraStandardIssue issueToBeMapped, string jiraBaseUrl)
		{
			issueToBeMapped.Identifiers[InstanceKeyNames.JIRA_ISSUE] = new Identifiers()
			{
				Id = jiraIssue.Key.Value,
				ProjectId = jiraIssue.Project,
				Url = jiraBaseUrl + "browse/" + jiraIssue.Key.Value
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
						if (jiraIssue[fieldValue + "Id"].Value != null)
						{
							issueToBeMapped.Identifiers.Add(field.GetValue(null).ToString(), new Identifiers()
							{
								Id = jiraIssue[fieldValue + "Id"].ToString(),
							});
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine("No field present for " + fieldValue);
					}
				}
			}
		}

		

		public static Issue ToNewJiraIssue(IStandardInstance instance, Jira jiraClient, string projectKey)
		{
			var jiraIssue = new Issue(jiraClient, projectKey) {
				Type = instance.Type,
				Reporter = instance.Author.UserMap[userColumnName],
				Summary = "[" + instance.MessageOrigin.Split('_')[0] + "] | " + instance.Summary,
				Priority = instance.Priority
			};

			try
			{
				jiraIssue.Assignee = instance.Assignee.UserMap[userColumnName];
			} catch(Exception e)
			{
				Debug.WriteLine("No assigned user");
			}

			foreach(var identifier in instance.Identifiers)
			{
				if(identifier.Value.Id != null)
				{
					try
					{
						jiraIssue[identifier.Key + "Id"] = identifier.Value.Id;
						jiraIssue[identifier.Key + "Url"] = identifier.Value.Url;
					}
					catch (Exception e)
					{
						Debug.WriteLine("Error adding identifier to field:");
						Debug.WriteLine(e.StackTrace);
					}
				}
			}

			return jiraIssue;
		}

		public static Issue ToUpdatedJiraIssue(IStandardInstance instance, Jira jiraClient, Issue oldIssue)
		{
			string summary = GetSummary(instance);

			oldIssue.Type = instance.Type;
			oldIssue.Summary = summary;
			
			oldIssue.Reporter = instance.Author.UserMap[userColumnName];
			oldIssue.Priority = instance.Priority;

			try
			{
				oldIssue.Assignee = instance.Assignee.UserMap[userColumnName];
			}
			catch (Exception e)
			{
				Debug.WriteLine("No assigned user");
			}

			return oldIssue;
		}
		

		private static string GetSummary(IStandardInstance instance)
		{
			var summary = instance.Summary;

			if (summary.StartsWith("[JIRA]"))
			{
				return summary.Substring(summary.IndexOf('|') + 1);

			}
			else if(summary.StartsWith('[') && summary.Contains("] | "))
			{
				return summary;
			} else
			{
				return "[" + instance.MessageOrigin.Split('_')[0] + "] | " + summary;
			}
		}
	}
}
