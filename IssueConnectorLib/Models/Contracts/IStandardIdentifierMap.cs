using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib.Models.Contracts
{
    public interface IStandardIdentifierMap
    {
		Dictionary<string, Identifiers> InstanceMap { get; set; }
		string MessageOrigin { get; set; }
    }
}
