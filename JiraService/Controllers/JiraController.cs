using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Jira;
using IssueConnectorWebService.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JiraService.Controllers
{
	[Produces("application/json")]
	[Route("api/Jira")]
	public class JiraController : Controller
	{

		private static readonly string IssueCreated = "issue_created";

		private static readonly string IssueUpdated = "issue_updated";

		private static readonly string IssueDeleted = "issue_deleted";
		private static readonly string ApiUser = "apiuser";

		public readonly IResourcePublisher _publisher;

		private readonly ILogger _logger;

		public JiraController(IResourcePublisher publisher,
			ILogger<JiraController> logger)
		{
			_publisher = publisher;
			_logger = logger;
		}


		[Route("issues")]
		[HttpPost]
		public void Post([FromBody] JiraEvent jiraEvent, [FromQuery(Name = "user_id")] string userId)
		{
			try
			{
				//string test = jiraEvent.Content.ReadAsStringAsync().Result;
				_logger.LogDebug("JiraWebhookEvent key: " + jiraEvent.Issue.Key);

				if (!userId.ToLower().Equals(ApiUser))
				{
					string origin = GetOrigin(jiraEvent);

					if (isCreated(jiraEvent))
					{
						_publisher.OnInstanceCreatedAsync(jiraEvent.Issue.Key, userId);

					}
					else if (isUpdated(jiraEvent))
					{
						_publisher.OnInstanceUpdatedAsync(jiraEvent.Issue.Key, userId);
					}
					else if (isDeleted(jiraEvent))
					{
						_publisher.OnInstanceDeletedAsync(jiraEvent.Issue.Key, userId);
					} 
				}

			} catch(ProjectNotPresentException e)
			{
				_logger.LogDebug(e.Message);
				_logger.LogError(e.StackTrace);
			}
			catch(Exception e)
			{
				_logger.LogError("Unknown error occured", e.StackTrace);
			}


		}

		[Route("newcomment/")]
		[HttpPost("{commentId}")]
		public async void PostComment([FromQuery(Name = "commentId")] string commentId, [FromQuery(Name = "issueId")] string issueId, [FromQuery(Name = "user_key")] string userKey)
		{
			try
			{
				_logger.LogDebug("Jira Comment posted to issue " + issueId + " by " + userKey);

				if (!userKey.ToLower().Equals(ApiUser))
				{
					await _publisher.OnCommentCreatedAsync(issueId, commentId, userKey);
				}
			}
			catch (Exception e)
			{

				Debug.WriteLine("Error adding comment...");
				Debug.WriteLine(e.StackTrace);
			}
		}

		private string GetOrigin(JiraEvent jiraEvent)
		{
			return jiraEvent.Issue.Self.Split("rest")[0];
		}

		// PUT: api/Jira/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE: api/ApiWithActions/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}

		private bool isCreated(JiraEvent jiraEvent)
		{
			return jiraEvent.WebHookEvent.Contains(IssueCreated);
		}

		private bool isUpdated(JiraEvent jiraEvent)
		{
			return (jiraEvent.WebHookEvent.Contains(IssueUpdated) && !jiraEvent.issue_event_type_name.Contains("comment"));
		}

		private bool isDeleted(JiraEvent jiraEvent)
		{
			return jiraEvent.issue_event_type_name.Contains(IssueDeleted);
		}
	}
}
