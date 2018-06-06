using EasyAccessService.Models;
using IssueConnectorLib.Models.Database;
using IssueConnectorLib.Services;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Database
{
	public class EasyAccessDatabaseService : DatabaseService, IEasyAccessDatabaseService
	{


		public EasyAccessDatabaseService(IOptions<DatabaseConnectionString> connectionStringOptions) : base(connectionStringOptions)
		{
		}

		public async Task<List<string>> GetAllEasyAccessProjectIds()
		{
			using (var conn = new MySqlConnection(_connectionString.DefaultConnection))
			{
				var result = new List<string>();

				conn.Open();

				string sqlStatement = "select easy_access_project_id from project_map";

				MySqlCommand cmd = new MySqlCommand()
				{
					Connection = conn,
					CommandText = sqlStatement
				};
				cmd.Prepare();

				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						result.Add(reader["easy_access_project_id"].ToString());
					}
					reader.Close();
				}

				return result;
			}
		}

		public async Task<EasyAccessUser> GetEasyAccessUserFrom(string easyAccessUserId)
		{
			using (var conn = new MySqlConnection(_connectionString.DefaultConnection))
			{
				conn.Open();

				string sqlStatement = "select * from easy_access_user where easy_access_user_id = @value";

				MySqlCommand cmd = new MySqlCommand()
				{
					Connection = conn,
					CommandText = sqlStatement
				};
				cmd.Prepare();

				cmd.Parameters.AddWithValue("@value", easyAccessUserId);

				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						return new EasyAccessUser()
						{
							Id = reader["easy_access_user_id"].ToString(),
							Email = reader["email"].ToString(),
							Name = reader["display_name"].ToString()
						};
					}
					reader.Close();
					return null;
				}
			}


		}

		public async Task<int> InsertEasyAccessUsers(List<EasyAccessUser> users)
		{
			int rowsAffected = 0;
			using (var connection = new MySqlConnection(_connectionString.DefaultConnection))
			{
				await connection.OpenAsync();


				//using IGNORE to skip errors when inserting duplicates


				foreach (var user in users)
				{
					MySqlCommand cmd = new MySqlCommand
					{
						Connection = connection
					};
					cmd.CommandText = "INSERT IGNORE INTO easy_access_user values (@id, @email, @name)";
					cmd.Prepare();

					cmd.Parameters.AddWithValue("@id", user.Id);
					cmd.Parameters.AddWithValue("@email", user.Email);
					cmd.Parameters.AddWithValue("@name", user.Name);

					rowsAffected += await cmd.ExecuteNonQueryAsync();
				}

				return rowsAffected;
			}
		}
	}
}
