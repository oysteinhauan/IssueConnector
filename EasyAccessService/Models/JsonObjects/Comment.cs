namespace EasyAccessService.Models
{
	using System;
	using System.Net;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public partial class Comment
	{
		[JsonProperty("topicGuid")]
		public string TopicGuid { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("projectId")]
		public string ProjectId { get; set; }

		[JsonProperty("verbalStatus")]
		public string VerbalStatus { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("date")]
		public DateTime? Date { get; set; }

		[JsonProperty("authorName")]
		public string AuthorName { get; set; }

		[JsonProperty("authorEmail")]
		public string AuthorEmail { get; set; }

		[JsonProperty("authorId")]
		public string AuthorId { get; set; }

		[JsonProperty("content")]
		public string Content { get; set; }

		[JsonProperty("modifiedDate")]
		public DateTime? ModifiedDate { get; set; }

		[JsonProperty("priority")]
		public string Priority { get; set; }

		[JsonProperty("importGuid")]
		public string ImportGuid { get; set; }
	}

	public partial class Comment
	{
		public static Comment FromJson(string json) => JsonConvert.DeserializeObject<Comment>(json, Converter.Settings);
	}

	public static class SerializeComment
	{
		public static string ToJson(this Comment self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}