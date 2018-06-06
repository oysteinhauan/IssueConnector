using IssueConnectorConfig.Models.UserInfo;
using IssueConnectorLib.Models.Database;
using IssueConnectorLib.Services;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorConfig.Services.Database
{
	public class ConfigDatabaseService : DatabaseService, IConfigDatabaseService
	{
		public ConfigDatabaseService(IOptions<DatabaseConnectionString> connectionsStringOptions) : base(connectionsStringOptions)
		{
		}

		public async Task<List<ClientUser>> GetAllUserMappingsForConfig()
		{

			using (var connection = new MySqlConnection(_connectionString.DefaultConnection))
			{
				await connection.OpenAsync();

				List<ClientUser> users = new List<ClientUser>();

				string query = "select * from user_map";

				MySqlCommand cmd = new MySqlCommand(query, connection);

				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (reader.Read())
					{
						users.Add(new ClientUser()
						{
							JiraUsername = reader["jira_user_name"].ToString(),
							TrimbleConnectUsername = reader["trimble_connect_user_id"].ToString(),
							EasyAccessUsername = reader["easy_access_user_id"].ToString()

						});
					}
					reader.Close();
					return users;
				}

			}

		}
	}
}
