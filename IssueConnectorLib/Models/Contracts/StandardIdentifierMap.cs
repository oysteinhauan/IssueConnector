using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib.Models.Contracts
{
	public class StandardIdentifierMap : IStandardIdentifierMap
	{
		public string MessageOrigin { get; set; }
		public Dictionary<string, Identifiers> InstanceMap { get; set; }
	}
}
