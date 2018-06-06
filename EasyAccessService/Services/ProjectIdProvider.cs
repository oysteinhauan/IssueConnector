using IssueConnectorLib.Models;
using IssueConnectorLib.Models.ProjectMappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Models
{
    public class ProjectIdProvider
    {
		public static async Task<string> GetProjectId(Dictionary<string, Identifiers> identifiers, ProjectMappings projectMappings)
		{
			var externalProjectId = "";

			//Find a project Id in the map
			foreach (var id in identifiers)
			{
				if (id.Value.ProjectId != null)
				{
					externalProjectId = id.Value.ProjectId;
					break;
				}
			}

			//Find the right mapping
			foreach (var mapping in projectMappings.Mappings)
			{
				if (mapping.Contains(externalProjectId))
				{
					return mapping.EasyAccessProjectId;
				}
			}
			return null;
		}
	}
}
