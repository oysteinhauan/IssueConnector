using IssueConnectorConfig.Models.UserInfo;
using IssueConnectorLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorConfig.Services.Database
{
    public interface IConfigDatabaseService : IDatabaseService 
    {
		Task<List<ClientUser>> GetAllUserMappingsForConfig();
		

    }
}
