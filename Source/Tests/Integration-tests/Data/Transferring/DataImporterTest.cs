using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Identity.Data;
using IdentityServer4.EntityFramework.Interfaces;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Data.Transferring
{
	[TestClass]
	public class DataImporterTest
	{
		#region Methods

		protected internal virtual async Task AssertConfigurationData(IConfigurationDbContext configurationDatabaseContext)
		{
			if(configurationDatabaseContext == null)
				throw new ArgumentNullException(nameof(configurationDatabaseContext));

			Assert.AreEqual(2, await configurationDatabaseContext.ApiResources.CountAsync());
			Assert.AreEqual(6, await configurationDatabaseContext.ApiScopes.CountAsync());
			Assert.AreEqual(2, await configurationDatabaseContext.Clients.CountAsync());
			Assert.AreEqual(3, await configurationDatabaseContext.ClientCorsOrigins.CountAsync());
			Assert.AreEqual(2, await configurationDatabaseContext.IdentityResources.CountAsync());
		}

		protected internal virtual async Task AssertConfigurationDatabaseIsEmpty(IConfigurationDbContext configurationDatabaseContext)
		{
			if(configurationDatabaseContext == null)
				throw new ArgumentNullException(nameof(configurationDatabaseContext));

			Assert.IsFalse(await configurationDatabaseContext.ApiResources.AnyAsync());
			Assert.IsFalse(await configurationDatabaseContext.ApiScopes.AnyAsync());
			Assert.IsFalse(await configurationDatabaseContext.Clients.AnyAsync());
			Assert.IsFalse(await configurationDatabaseContext.ClientCorsOrigins.AnyAsync());
			Assert.IsFalse(await configurationDatabaseContext.IdentityResources.AnyAsync());
		}

		[TestCleanup]
		public void Cleanup()
		{
			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		protected internal virtual async Task<IConfiguration> CreateConfigurationAsync(string configurationFileName, IFileProvider fileProvider)
		{
			var configurationBuilder = new ConfigurationBuilder()
				.SetFileProvider(fileProvider)
				.AddJsonFile($"Data/Transferring/Resources/DataImporter/{configurationFileName}.json", false, false);

			return await Task.FromResult(configurationBuilder.Build());
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_Duplicate_Test()
		{
			await this.ImportAsyncDuplicateTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_InvalidClient_Test()
		{
			await this.ImportAsyncInvalidClientTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_Scenario_Test()
		{
			await this.ImportAsyncScenarioTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_WithConfiguredData_Test()
		{
			await this.ImportAsyncWithConfiguredDataTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_WithoutConfiguredData_Test()
		{
			await this.ImportAsyncWithoutConfiguredDataTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_Duplicate_Test()
		{
			await this.ImportAsyncDuplicateTest(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_InvalidClient_Test()
		{
			await this.ImportAsyncInvalidClientTest(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_Scenario_Test()
		{
			await this.ImportAsyncScenarioTest(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_WithConfiguredData_Test()
		{
			await this.ImportAsyncWithConfiguredDataTest(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_WithoutConfiguredData_Test()
		{
			await this.ImportAsyncWithoutConfiguredDataTest(DatabaseProvider.SqlServer);
		}

		protected internal virtual async Task ImportAsyncDuplicateTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var configuration = await this.CreateConfigurationAsync("Duplicates", context.FileProvider);

					var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

					var result = await dataImporter.ImportAsync(configuration, new ImportOptions {VerifyOnly = true});

					Assert.AreEqual(5, result.Errors.Count);
					Assert.AreEqual("ApiResource.Name \"api-resource\" has 1 duplicate.", result.Errors[0]);
					Assert.AreEqual("ApiScope.Name \"api-scope\" has 2 duplicates.", result.Errors[1]);
					Assert.AreEqual("Client.ClientId \"client\" has 3 duplicates.", result.Errors[2]);
					Assert.AreEqual("IdentityResource.Name \"identity-resource\" has 4 duplicates.", result.Errors[3]);
					Assert.AreEqual("User.UserName \"user\" has 5 duplicates.", result.Errors[4]);
				}
			}
		}

		protected internal virtual async Task ImportAsyncInvalidClientTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var configuration = await this.CreateConfigurationAsync("Invalid-Client", context.FileProvider);

					var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

					var result = await dataImporter.ImportAsync(configuration, new ImportOptions {VerifyOnly = true});

					Assert.AreEqual(2, result.Errors.Count);
					Assert.AreEqual("Client \"client-1\": no allowed grant type specified", result.Errors[0]);
					Assert.AreEqual("Client \"client-2\": Client secret is required for grant-type, but no client secret is configured.", result.Errors[1]);
				}
			}
		}

		[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
		protected internal virtual async Task ImportAsyncScenarioTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				// Step 1
				foreach(var options in new[] {new ImportOptions(), new ImportOptions {VerifyOnly = true}})
				{
					using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
					{
						var configuration = await this.CreateConfigurationAsync("Step-1", context.FileProvider);

						var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

						var result = await dataImporter.ImportAsync(configuration, options);

						Assert.AreEqual(2, result.Errors.Count);
						Assert.AreEqual("Client \"interactive_client\": AllowedCorsOrigins contains invalid origin: http://localhost/", result.Errors[0]);
						Assert.AreEqual("User.UserName \"User-1\" has 1 duplicate.", result.Errors[1]);
						Assert.AreEqual(0, result.SavedChanges);
					}
				}

				// Step 2
				foreach(var options in new[] {new ImportOptions {VerifyOnly = true}, new ImportOptions()})
				{
					using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
					{
						var configuration = await this.CreateConfigurationAsync("Step-2", context.FileProvider);

						var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

						var result = await dataImporter.ImportAsync(configuration, options);

						Assert.AreEqual(1, result.Errors.Count);
						Assert.AreEqual("User.UserName \"User-1\" has 1 duplicate.", result.Errors[0]);
						Assert.AreEqual(0, result.SavedChanges);
					}
				}

				// Step 3
				foreach(var options in new[] {new ImportOptions {VerifyOnly = true}, new ImportOptions()})
				{
					using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
					{
						var configuration = await this.CreateConfigurationAsync("Step-3", context.FileProvider);

						var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

						var result = await dataImporter.ImportAsync(configuration, options);

						Assert.IsFalse(result.Errors.Any());
						Assert.AreEqual(!options.VerifyOnly ? 45 : 0, result.SavedChanges);
					}
				}

				using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var configurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

					Assert.AreEqual(2, await configurationDatabaseContext.ApiResources.CountAsync());
					Assert.AreEqual(6, await configurationDatabaseContext.ApiScopes.CountAsync());
					Assert.AreEqual(3, await configurationDatabaseContext.ClientCorsOrigins.CountAsync());
					Assert.AreEqual(2, await configurationDatabaseContext.Clients.CountAsync());
					Assert.AreEqual(2, await configurationDatabaseContext.IdentityResources.CountAsync());
				}

				using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var identityDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IdentityContext>();

					Assert.AreEqual(0, await identityDatabaseContext.RoleClaims.CountAsync());
					Assert.AreEqual(0, await identityDatabaseContext.Roles.CountAsync());
					Assert.AreEqual(0, await identityDatabaseContext.UserClaims.CountAsync());
					Assert.AreEqual(0, await identityDatabaseContext.UserLogins.CountAsync());
					Assert.AreEqual(0, await identityDatabaseContext.UserRoles.CountAsync());
					Assert.AreEqual(0, await identityDatabaseContext.UserTokens.CountAsync());
					Assert.AreEqual(3, await identityDatabaseContext.Users.CountAsync());
				}
			}
		}

		protected internal virtual async Task ImportAsyncWithConfiguredDataTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var configurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

					await this.AssertConfigurationDatabaseIsEmpty(configurationDatabaseContext);

					var configuration = await this.CreateConfigurationAsync("Configuration-Data", context.FileProvider);

					var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

					await dataImporter.ImportAsync(configuration, new ImportOptions());

					await this.AssertConfigurationData(configurationDatabaseContext);
				}
			}
		}

		protected internal virtual async Task ImportAsyncWithoutConfiguredDataTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var configuration = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
					var configurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

					await this.AssertConfigurationDatabaseIsEmpty(configurationDatabaseContext);

					var dataImporter = (DataImporter)serviceScope.ServiceProvider.GetRequiredService<IDataImporter>();

					await dataImporter.ImportAsync(configuration, new ImportOptions());

					await this.AssertConfigurationDatabaseIsEmpty(configurationDatabaseContext);
				}
			}
		}

		[TestInitialize]
		public void Initialize()
		{
			this.Cleanup();
		}

		#endregion
	}
}