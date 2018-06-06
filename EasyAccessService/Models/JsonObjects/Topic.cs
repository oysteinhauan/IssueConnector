using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Models
{
	using System;
	using System.Net;
	using System.Collections.Generic;

	using Newtonsoft.Json;
	using IssueConnectorLib;

	public partial class Topic 
	{
		[JsonProperty("qGuid")]
		public string QGuid { get; set; }

		[JsonProperty("projectId")]
		public string ProjectId { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("type")]
		public string TopicType { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("priority")]
		public string Priority { get; set; }

		[JsonProperty("authorName")]
		public string AuthorName { get; set; }

		[JsonProperty("authorEmail")]
		public string AuthorEmail { get; set; }

		[JsonProperty("authorId")]
		public string AuthorId { get; set; }

		[JsonProperty("referenceLink")]
		public object ReferenceLink { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("index")]
		public long Index { get; set; }

		[JsonProperty("labels")]
		public string [] Labels { get; set; }

		[JsonProperty("creationDate")]
		public DateTime? CreationDate { get; set; }

		[JsonProperty("modifiedDate")]
		public DateTime? ModifiedDate { get; set; }

		[JsonProperty("assignedToName")]
		public string AssignedToName { get; set; }

		[JsonProperty("assignedToEmail")]
		public string AssignedToEmail { get; set; }

		[JsonProperty("assignedToId")]
		public string AssignedToId { get; set; }

		[JsonProperty("relatedTopics")]
		public object RelatedTopics { get; set; }

		//[JsonProperty("files")]
		//public object[] Files { get; set; } = new object[0];

		//[JsonProperty("documentReferences")]
		//public object[] DocumentReferences { get; set; } = new object[0];

		[JsonProperty("importGuid")]
		public object ImportGuid { get; set; }
	}

	public partial class Topic
	{
		public static Topic FromJson(string json) => JsonConvert.DeserializeObject<Topic>(json, Converter.Settings);
	}

	public static class SerializeTopic
	{
		public static string ToJson(this Topic self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}

	public class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
		};
	}
}

