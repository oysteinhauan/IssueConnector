using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib.Models
{
    public interface IStandardViewpoint
    {
		Dictionary<string, Identifiers> Identifiers { get; set; }
		string ViewpointId { get; set; }
		string EncodedThumbnail { get; set; }
		System.DateTime Date { get; set; }
		string Url { get; set; }
		byte[] ImageByteArray { get; set; }
	}
}
