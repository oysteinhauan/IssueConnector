// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using IssueConnectorWebService.Models.EasyAccess;
//
//    var data = TopicProjectSettings.FromJson(jsonString);

namespace EasyAccessService.Models
{
	using System;
	using System.Net;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public partial class TopicProjectSettings
	{
		[JsonProperty("qGuid")]
		public string QGuid { get; set; }

		[JsonProperty("projectId")]
		public string ProjectId { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("easyAccessProjectId")]
		public string EasyAccessProjectId { get; set; }

		[JsonProperty("types")]
		public string[] Types { get; set; }

		[JsonProperty("statuses")]
		public string[] Statuses { get; set; }

		[JsonProperty("labels")]
		public string[] Labels { get; set; }

		[JsonProperty("snippetLabels")]
		public object SnippetLabels { get; set; }

		[JsonProperty("priorities")]
		public string[] Priorities { get; set; }

		[JsonProperty("users")]
		public List<EasyAccessUser> Users { get; set; }

		[JsonProperty("hiddenStatus")]
		public object HiddenStatus { get; set; }
	}

	public partial class User
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }
	}

	public partial class TopicProjectSettings
	{
		public static TopicProjectSettings FromJson(string json) => JsonConvert.DeserializeObject<TopicProjectSettings>(json, EasyAccessService.Models.Converter.Settings);
	}

}
