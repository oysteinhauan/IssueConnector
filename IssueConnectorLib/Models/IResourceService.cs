using IssueConnectorLib.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models
{
    public interface IResourceCrudService
    {


		Task<IStandardInstance> CreateInstanceAsync(IStandardInstance instance);
		Task<IStandardInstance> UpdateInstanceAsync(IStandardInstance instance);
		void DeleteInstanceAsync(IStandardInstance instance);
		Task<IStandardInstance> ReadInstanceAsync(string instanceId, string projectId = null);
		

		Task UpdateInstanceIdMap(IStandardIdentifierMap idMap);

		Task<IStandardComment> CreateCommentAsync(IStandardComment comment);
		Task<IStandardComment> UpdateCommentAsync(IStandardComment comment);
		void DeleteCommentAsync(IStandardComment comment);
		Task<IStandardComment> ReadCommentAsync(string commentId, string instanceId, string projectId = null);

		Task<IStandardAttachment> CreateAttachmentAsync(IStandardAttachment instance);
		Task<IStandardAttachment> UpdateAttachmentAsync(IStandardAttachment instance);
		void DeleteAttachmentAsync(IStandardAttachment instance);
		Task<IStandardAttachment> ReadAttachmentAsync(string instanceId);

		Task<IStandardViewpoint> CreateViewpointAsync(IStandardViewpoint instance);
		Task<IStandardViewpoint> UpdateViewpointAsync(IStandardViewpoint instance);
		void DeleteViewpointAsync(IStandardViewpoint instance);
		Task<IStandardViewpoint> ReadViewpointAsync(string instanceId, string projectId, string commentId, string viewpointId);

	}
}
