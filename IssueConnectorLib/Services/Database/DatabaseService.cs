using IssueConnectorLib.Models.Exceptions;
using IssueConnectorLib.Models.Database;
using IssueConnectorLib.Models.ProjectMappings;
using IssueConnectorLib.Models.Users;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IssueConnectorLib.Services
{
	public class DatabaseService : IDatabaseService
	{

		protected readonly DatabaseConnectionString _connectionString;

		public DatabaseService(IOptions<DatabaseConnectionString> connectionsStringOptions)
		{
			_connectionString = connectionsStringOptions.Value;
		}


		

		public async Task<MappingInstance> GetProjectMappingFrom(string columnName, string value)
		{
			using (var connection = new MySqlConnection(_connectionString.DefaultConnection))
			{
				await connection.OpenAsync();

				MySqlCommand cmd = new MySqlCommand
				{
					Connection = connection,

					CommandText = "SELECT * FROM project_map WHERE " + columnName + " = @value"
				};
				cmd.Prepare();

				cmd.Parameters.AddWithValue("@value", value);

				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (reader.Read())
					{
						return new MappingInstance()
						{
							JiraProjectKey = reader["jira_project_key"].ToString(),
							TrimbleConnectProjectId = reader["trimble_connect_project_id"].ToString(),
							EasyAccessProjectId = reader["easy_access_project_id"].ToString()
						};
					}
					reader.Close();
					throw new ProjectNotPresentException("No such project was found.", new Exception());

				}

			}
		}

		public async Task<List<ClientUser>> GetAllRegisteredUsers()
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
							////JiraUsername = reader["jira_user_name"].ToString(),
							//TrimbleConnectUsername = reader["trimble_connect_user_id"].ToString(),
							//EasyAccessUsername = reader["easy_access_user_id"].ToString()

						});
					}
					reader.Close();
					return users;
				}

			}
		}

		public async Task<ProjectMappings> GetAllProjectMappings()
		{
			using (var connection = new MySqlConnection(_connectionString.DefaultConnection))
			{
				List<MappingInstance> maps = new List<MappingInstance>();

				await connection.OpenAsync();
				string sqlStatement = "select * from project_map";

				MySqlCommand cmd = new MySqlCommand(sqlStatement, connection);

				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (reader.Read())
					{
						maps.Add(new MappingInstance()
						{
							JiraProjectKey = reader["jira_project_key"].ToString(),
							TrimbleConnectProjectId = reader["trimble_connect_project_id"].ToString(),
							EasyAccessProjectId = reader["easy_access_project_id"].ToString()
						});
					}
					reader.Close();
					return new ProjectMappings()
					{
						Mappings = maps
					};
				}
			}

		}

		public async Task<int> SaveNewProjectMap(string jiraProjectKey, string trimbleConnectPID, string easyAccessPID)
		{

			using (var connection = new MySqlConnection(_connectionString.DefaultConnection))
			{

				await connection.OpenAsync();

				MySqlCommand cmd = new MySqlCommand
				{
					Connection = connection,

					CommandText = "INSERT INTO project_map VALUES (@jiraProjectKey, @trimbleConnectPID, @easyAccessPID);"
				};
				cmd.Prepare();

				cmd.Parameters.AddWithValue("@jiraProjectKey", jiraProjectKey);
				cmd.Parameters.AddWithValue("@trimbleConnectPID", trimbleConnectPID);
				cmd.Parameters.AddWithValue("@easyAccessPID", easyAccessPID);

				return await cmd.ExecuteNonQueryAsync();
			}
		}

		public async Task<int> AddUserMapping(string jirauser, string connectUser, string easyAccessUser)
		{
			using (var connection = new MySqlConnection(_connectionString.DefaultConnection))
			{
				await connection.OpenAsync();

				MySqlCommand cmd = new MySqlCommand
				{
					Connection = connection,

					CommandText = "INSERT INTO user_map VALUES (@jirauser, @tcuser, @eauser)"
				};
				cmd.Prepare();

				cmd.Parameters.AddWithValue("@jirauser", jirauser);
				cmd.Parameters.AddWithValue("@tcuser", connectUser);
				cmd.Parameters.AddWithValue("@eauser", easyAccessUser);

				return await cmd.ExecuteNonQueryAsync();
			}
		}

		


		public async Task<ClientUser> GetClientUserFrom(string columnName, string value)
		{
			using(var conn = new MySqlConnection(_connectionString.DefaultConnection))
			{
				await conn.OpenAsync();

				MySqlCommand cmd = new MySqlCommand
				{
					Connection = conn
				};

				cmd.CommandText = "select * from user_map where "+ columnName + " = @value";
				cmd.Prepare();

				cmd.Parameters.AddWithValue("@value", value);

				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (reader.Read())
					{
						Dictionary<string, string> userMap = new Dictionary<string, string>();
						for (int index = 0; index < reader.FieldCount; index++)
						{
							try
							{
								userMap[reader.GetName(index)] = reader.GetString(index);
							}
							catch (Exception e)
							{
								Debug.WriteLine("Error adding user from column: " + reader.GetName(index));
							}
						}
						return new ClientUser()
						{
							UserMap = userMap
						};
					}
					reader.Close();
					throw new UserNotFoundException("No user with name "+ value + " in " + columnName);
				}
			}
		}

	}
}
