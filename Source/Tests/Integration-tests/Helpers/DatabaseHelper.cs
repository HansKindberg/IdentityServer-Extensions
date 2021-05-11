using System;
using System.IO;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Helpers
{
	public static class DatabaseHelper
	{
		#region Fields

		private static readonly string _databaseFilePath = Path.Combine(Global.ProjectDirectoryPath, "Test-data", "Identity-Server");
		private static readonly string _sqliteDatabaseFilePath = $"{_databaseFilePath}.db";
		private static readonly string _sqlServerDatabaseFilePath = $"{_databaseFilePath}.mdf";
		public static readonly string SqliteConnectionString = $"Data Source={_sqliteDatabaseFilePath}";
		public static readonly string SqlServerConnectionString = $"AttachDbFileName={_sqlServerDatabaseFilePath};Initial Catalog={_sqlServerDatabaseFilePath};Integrated Security=True;Server=(LocalDB)\\MSSQLLocalDB";

		#endregion

		#region Methods

		private static void DeleteIdentityServerDatabase(IServiceCollection services)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			using(var serviceProvider = services.BuildServiceProvider())
			{
				using(var serviceScope = serviceProvider.CreateScope())
				{
					serviceScope.ServiceProvider.GetRequiredService<EmptyDatabaseContext>().Database.EnsureDeleted();
				}
			}
		}

		public static void DeleteIdentityServerDatabase()
		{
			var services = new ServiceCollection();
			services.AddDbContext<EmptyDatabaseContext>(optionsBuilder => optionsBuilder.UseSqlite(SqliteConnectionString));
			DeleteIdentityServerDatabase(services);

			services = new ServiceCollection();
			services.AddDbContext<EmptyDatabaseContext>(optionsBuilder => optionsBuilder.UseSqlServer(SqlServerConnectionString));
			DeleteIdentityServerDatabase(services);
		}

		public static async Task MigrateDatabaseAsync(IServiceProvider serviceProvider)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				await ((DbContext)serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>()).Database.MigrateAsync();
				await serviceScope.ServiceProvider.GetRequiredService<IdentityContext>().Database.MigrateAsync();
			}
		}

		#endregion
	}
}