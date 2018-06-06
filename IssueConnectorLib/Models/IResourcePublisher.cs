using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models
{
    public interface IResourcePublisher
    {

		Task OnInstanceCreatedAsync(string instanceId, string user, string projectId = null);
		Task OnInstanceUpdatedAsync(string instanceId, string user = null, string projectId = null);
		Task OnInstanceDeletedAsync(string instanceId, string user, string projectId = null);

		Task OnCommentCreatedAsync(string instanceId, string commentId, string user,  string projectId = null);
		Task OnCommentUpdatedAsync(string instanceId, string commentId, string user);
		Task OnCommentDeletedAsync(string instanceId, string commentId, string user);
		Task PublishKeyMap(Dictionary<string, Identifiers> identifiers);

		Task OnViewpointCreatedAsync(string instanceId, string viewpointId, string projectId, string commentId);
		Task OnViewpointDeletedAsync(string instanceId, string viewpointId, string projectId, string commentId);
	}
}
