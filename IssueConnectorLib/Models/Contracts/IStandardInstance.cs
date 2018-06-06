using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib
{
    public interface IStandardInstance
    {
		Dictionary<string, Identifiers> Identifiers { get; set; }
		string MessageOrigin { get; set; }
		string Action { get; set; }
		DateTime Created { get; set; }
		ClientUser Author { get; set; }
		ClientUser Assignee { get; set; }
		ClientUser ModifiedUser { get; set; }
		string Summary { get; set; }
		string Type { get; set; }
		string Priority { get; set; }
		string Status { get; set; }
		string [] Labels { get; set; }

		Dictionary<string, string> CustomFields { get; set; }


	}
}
