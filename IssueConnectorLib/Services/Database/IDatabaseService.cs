
using IssueConnectorLib.Models.ProjectMappings;
using IssueConnectorLib.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Services
{
    public interface IDatabaseService
    {


		Task<MappingInstance> GetProjectMappingFrom(string columnName, string value);

		Task<List<ClientUser>> GetAllRegisteredUsers();

		Task<ClientUser> GetClientUserFrom(string columnName, string value);

		Task<ProjectMappings> GetAllProjectMappings();
		Task<int> SaveNewProjectMap(string jiraProjectKey, string trimbleConnectPID, string easyAccessPID);

		Task<int> AddUserMapping(string jirauser, string connectUser, string easyAccessUser);

    }
}
