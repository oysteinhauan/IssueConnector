using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using JiraService.Models;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Filters
{
	public class JiraMessageFilter
	{
		public static bool OfInterest(IStandardInstance contextMessage, string systemIdentifier)
		{
			if (contextMessage.MessageOrigin.Equals(systemIdentifier)) return false;
			
			return true;
		}

		public static bool OfInterest(IStandardIdentifierMap contextMessage, string systemIdentifier)
		{
			if (contextMessage.MessageOrigin.Equals(systemIdentifier)) return false;

			if (!contextMessage.InstanceMap.ContainsKey(InstanceKeyNames.JIRA_ISSUE)) return false;

			return true;
		}

		internal static bool OfInterest(IStandardAttachment message, string machineOrigin)
		{
			throw new NotImplementedException();
		}

		internal static bool OfInterest(IStandardComment message, string machineOrigin)
		{
			if (message.MessageOrigin.Equals(machineOrigin)) return false;
			if (!message.Identifiers.ContainsKey(InstanceKeyNames.JIRA_ISSUE)) return false;

			return true;
		}

		internal static bool OfInterest(IStandardViewpoint message, string machineOrigin)
		{
			if (message.Identifiers.ContainsKey(InstanceKeyNames.JIRA_ISSUE)) return true;

			return false;
		}
	}
}
