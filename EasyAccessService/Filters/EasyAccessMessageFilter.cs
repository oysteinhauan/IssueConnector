using IssueConnectorLib;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using IssueConnectorLib.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyAccessService.Models;

namespace EasyAccessService.Filters
{
    public class EasyAccessMessageFilter
    {

		public static bool OfInterest(IStandardInstance contextMessage, string systemIdentifier)
		{
			if (contextMessage.MessageOrigin.Equals(systemIdentifier)) return false;

			return true;
		}

		public static bool OfInterest(IStandardIdentifierMap contextMessage, string systemIdentifier)
		{
			if (contextMessage.MessageOrigin.Equals(systemIdentifier)) return false;

			if (!contextMessage.InstanceMap.ContainsKey(InstanceKeyNames.EASY_ACCESS_TOPIC)) return false;

			return true;
		}

		internal static bool OfInterest(IStandardAttachment message, string machineOrigin)
		{
			return false;
		}

		internal static bool OfInterest(IStandardComment message, string machineOrigin)
		{
			if (message.MessageOrigin.Equals(machineOrigin)) return false;

			return true;
		}

		internal static bool OfInterest(IStandardViewpoint message, string machineOrigin)
		{
			return false;
		}

	    public static bool IsOriginalTopic(Topic topic)
	    {
		    return !(topic.Title.StartsWith('[') && topic.Title.Contains("] | "));
	    }
	    
	    public static bool IsOriginalComment(Comment comment)
	    {
		    return !(comment.Content.StartsWith('[') && comment.Content.Contains("] | "));
	    }
	}
}
