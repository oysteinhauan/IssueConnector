using EasyAccessService.Models;
using IssueConnectorLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Database
{
    public interface IEasyAccessDatabaseService : IDatabaseService
    {
		Task<int> InsertEasyAccessUsers(List<EasyAccessUser> users);

		Task<EasyAccessUser> GetEasyAccessUserFrom(string jiraUser);

		Task<List<string>> GetAllEasyAccessProjectIds();

	}
}
