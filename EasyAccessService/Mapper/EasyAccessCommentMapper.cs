using EasyAccessService.Database;
using EasyAccessService.Models;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Mappers;
using IssueConnectorLib.Models.SystemInfo;
using IssueConnectorLib.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Mapper
{
    public class EasyAccessCommentMapper : IEasyAccessCommentMapper
    {

		private IEasyAccessDatabaseService _dbService;
		private EasyAccessConfig _configSettings;
		private string Origin = "EasyAccess_" + MachineInfo.GetMacAddress();

		public EasyAccessCommentMapper(
			IEasyAccessDatabaseService dbService, 
			IOptions<EasyAccessConfig> configSettingsOptions
			)
		{
			_configSettings = configSettingsOptions.Value;
			_dbService = dbService;
		}

		public async Task<IStandardComment> ToStandardComment(Comment comment, List<Comment> allComments, List<ViewPoint> viewpoints)
		{
			return new StandardComment()
			{
				Author = comment.AuthorId == null ? null : await _dbService.GetClientUserFrom(_configSettings.UserColumnName, comment.AuthorId),
				Content = comment.Content,
				Created = comment.Date,
				Status = comment.Status,
				Priority = comment.Priority,
				Identifiers = EasyAccessInstanceMapper.GetIdentifiers(comment.TopicGuid, comment.ProjectId, allComments),
				ViewpointIds = viewpoints?.Select(vp => vp.Id).ToList()
			};
		}

		public async Task<Comment> ToNewSpecialComment(IStandardComment comment)
		{

			var author = comment.Author == null ? null : await _dbService.GetEasyAccessUserFrom(comment.Author.UserMap[_configSettings.UserColumnName]);
			var projectId = await ProjectIdProvider.GetProjectId(comment.Identifiers, await _dbService.GetAllProjectMappings());
			return new Comment()
			{
				Content = "[" + comment.MessageOrigin.Split('_')[0] + "] | " + comment.Content,
				AuthorId = author?.Id,
				AuthorEmail = author?.Email,
				AuthorName = author?.Name,
				ProjectId = projectId,
				TopicGuid = comment.Identifiers[InstanceKeyNames.EASY_ACCESS_TOPIC].Id,
				Date = comment.Created
			};
		}

		public Task<Comment> ToUpdatedSpecialComment(IStandardComment instance, Comment oldSpecial)
		{
			throw new NotImplementedException();
		}
	}
}
