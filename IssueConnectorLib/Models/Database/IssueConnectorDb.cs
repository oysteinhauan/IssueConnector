using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.Database
{
	public class IssueConnectorDb : IDisposable
	{
		public MySqlConnection Connection;

		public IssueConnectorDb(string connectionString)
		{
			Connection = new MySqlConnection(connectionString);
		}

		public void Dispose()
		{
			Connection.Close();
		}
	}
}
