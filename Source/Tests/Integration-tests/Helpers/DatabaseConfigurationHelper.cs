using System.Collections.Generic;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Configuration;

namespace IntegrationTests.Helpers
{
	public static class DatabaseConfigurationHelper
	{
		#region Methods

		private static IList<KeyValuePair<string, string>> CreateExplicitDatabaseTestConfiguration(string connectionString, DatabaseProvider provider)
		{
			var configuration = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>($"ConnectionStrings:{ConfigurationKeys.DefaultConnectionStringName}", connectionString),
				new KeyValuePair<string, string>($"{ConfigurationKeys.DataPath}:{nameof(DataOptions.Provider)}", provider.ToString())
			};

			return configuration;
		}

		public static IList<KeyValuePair<string, string>> CreateExplicitSqliteTestConfiguration()
		{
			return CreateExplicitDatabaseTestConfiguration(DatabaseHelper.SqliteConnectionString, DatabaseProvider.Sqlite);
		}

		public static IList<KeyValuePair<string, string>> CreateExplicitSqlServerTestConfiguration()
		{
			return CreateExplicitDatabaseTestConfiguration(DatabaseHelper.SqlServerConnectionString, DatabaseProvider.SqlServer);
		}

		#endregion
	}
}