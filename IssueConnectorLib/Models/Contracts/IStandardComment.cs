using IssueConnectorLib.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib.Models
{
    public interface IStandardComment
    {
		Dictionary<string, Identifiers> Identifiers { get; set; }
		ClientUser Author { get; set; }
		DateTime? Created { get; set; }
		string Content { get; set; }
		string Status { get; set; }
		string Priority { get; set; }
		string MessageOrigin { get; set; }
		List<string> ViewpointIds { get; set; }
    }
}
