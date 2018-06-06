using EasyAccessService.Models;
using IssueConnectorLib.Enums;
using IssueConnectorLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyAccessService.Mapper
{
    public class EasyAccessViewpointMapper
    {
		public static StandardViewpoint ToStandardViewpoint(ViewPoint viewPoint, List<Comment> comments)
		{
			var standardVp = new StandardViewpoint()
			{
				Date = viewPoint.Date,
				//.Split(',') because the thumbnail string from easy access starts with data type. Base64 contains no ','.
				EncodedThumbnail = viewPoint.ViewPointImage.Thumbnail.Split(',')[1].Trim(),
				Identifiers = new Dictionary<string, Identifiers>(),
				Url = viewPoint.ViewPointImage.Url,
				ViewpointId = viewPoint.Id
				
			};

			SetIdentifiers(viewPoint, standardVp, comments);

			return standardVp;
		}

		private static void SetIdentifiers(ViewPoint vp, StandardViewpoint standardViewpoint, List<Comment> comments)
		{
			standardViewpoint.Identifiers.Add(InstanceKeyNames.EASY_ACCESS_TOPIC, new Identifiers()
			{
				Id = vp.TopicGuid,
				ProjectId = vp.ProjectId,
			});
			FieldInfo[] fields = typeof(InstanceKeyNames).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			foreach (var field in fields)
			{
				foreach (var comment in comments)
				{
					if (comment.Content != null && comment.Content.Contains(field.GetValue(null).ToString() + "Id"))
					{
						//The mapped id's are stored in the comments of easy access. Example JiraIssueId_CE-56
						standardViewpoint.Identifiers[field.GetValue(null).ToString()] = new Identifiers() { Id = comment.Content.Split('_')[1] };
						break;
					}
				}
			}
		}
	}
}
