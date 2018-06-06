using Atlassian.Jira;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Models.SystemInfo;
using IssueConnectorLib.Models.Users;
using IssueConnectorLib.Services;
using JiraService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Mapper
{
	public class JiraStandardCommentMapper : IStandardCommentMapper<IStandardComment, Comment>
	{

		private IDatabaseService _dbService;
		private ConfigSettings _configSettings;
		private IResourceClient<Jira> _resourceClient;
		private string Origin = "JIRA_" + MachineInfo.GetMacAddress();

		public JiraStandardCommentMapper(IDatabaseService dbService, IResourceClient<Jira> client, IOptions<ConfigSettings> configSettingsOptions)
		{
			_dbService = dbService;
			_resourceClient = client;
			_configSettings = configSettingsOptions.Value;
		}


		public async Task<Comment> ToNewSpecialComment(IStandardComment instance)
		{
			return new Comment()
			{
				Author = instance.Author?.UserMap[_configSettings.UserColumnName],
				Body = GetCommentBody(instance)
			};
		}

		private string GetCommentBody(IStandardComment comment)
		{
			var result = "[" + comment.MessageOrigin.Split('_')[0] + "] | ";

			//add the viewpoint thumbnail to Jira comments
			if (comment.ViewpointIds != null)
			{
				foreach (var vpId in comment.ViewpointIds)
				{
					result += " !viewpoint_" + vpId + ".png|thumbnail! ";
				}
			}

			result += comment.Content;

			return result;

		}

		public async Task<IStandardComment> ToStandardComment(Comment comment, string issueId)
		{
			return new JiraComment()
			{
				Content = comment.Body,
				Identifiers = JiraStandardInstanceMapper.GetIdentifiers(await _resourceClient.RestClient.Issues.GetIssueAsync(issueId), _configSettings.HomeUri),
				Created = comment.CreatedDate,
				Author = comment.Author == null ? null : await _dbService.GetClientUserFrom(_configSettings.UserColumnName, comment.Author),
				MessageOrigin = Origin
			};
		}

		public Task<Comment> ToUpdatedSpecialComment(IStandardComment instance, Comment oldSpecial)
		{
			throw new NotImplementedException();
		}
	}
}
