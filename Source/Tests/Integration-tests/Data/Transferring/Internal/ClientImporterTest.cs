using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Extensions;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClientModel = IdentityServer4.Models.Client;
using SecretModel = IdentityServer4.Models.Secret;

namespace IntegrationTests.Data.Transferring.Internal
{
	[TestClass]
	public class ClientImporterTest
	{
		#region Methods

		[TestCleanup]
		public void Cleanup()
		{
			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		protected internal virtual async Task<ClientImporter> CreateClientImporterAsync(IConfigurationDbContext configurationDbContext, IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new ClientImporter(serviceProvider.GetRequiredService<IClientConfigurationValidator>(), configurationDbContext, serviceProvider.GetRequiredService<ILoggerFactory>()));
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_Scenario_Test()
		{
			await this.ImportAsyncScenarioTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_Scenario_Test()
		{
			await this.ImportAsyncScenarioTest(DatabaseProvider.SqlServer);
		}

		protected internal virtual async Task ImportAsyncScenarioTest(DatabaseProvider databaseProvider)
		{
			var importOptions = new ImportOptions {DeleteAllOthers = true};

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				// Step 1
				await this.ImportAsyncScenarioTest(
					importOptions,
					new List<ClientModel>
					{
						new ClientModel
						{
							AllowedGrantTypes = new[] {"a", "b", "c"}.ToList(),
							ClientId = "client-1",
							ClientSecrets = new[] {new SecretModel("a"), new SecretModel("b"), new SecretModel("c")}.ToList(),
							Description = "client-1-description"
						}
					},
					7,
					serviceProvider
				);

				// Step 2
				await this.ImportAsyncScenarioTest(
					importOptions,
					new List<ClientModel>
					{
						new ClientModel
						{
							AllowedGrantTypes = new[] {"a", "b", "c"}.ToList(),
							ClientId = "client-1",
							ClientSecrets = new[] {new SecretModel("a"), new SecretModel("b"), new SecretModel("c")}.ToList(),
							Description = "client-1-description"
						}
					},
					0,
					serviceProvider
				);

				// Step 3
				await this.ImportAsyncScenarioTest(
					importOptions,
					new List<ClientModel>
					{
						new ClientModel
						{
							AllowedGrantTypes = new[] {"a"}.ToList(),
							ClientId = "client-1",
							ClientSecrets = new[] {new SecretModel("a")}.ToList(),
							Description = "client-1-description"
						}
					},
					4,
					serviceProvider
				);

				// Step 4
				await this.ImportAsyncScenarioTest(
					importOptions,
					new List<ClientModel>(),
					1,
					serviceProvider
				);
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(ImportOptions importOptions, IList<ClientModel> models, int savedChangesExpected, IServiceProvider serviceProvider)
		{
			if(importOptions == null)
				throw new ArgumentNullException(nameof(importOptions));

			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var clientStore = serviceScope.ServiceProvider.GetRequiredService<IClientStore>();
				var configurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

				var clientImporter = await this.CreateClientImporterAsync(configurationDatabaseContext, serviceScope.ServiceProvider);
				var result = new DataImportResult();
				await clientImporter.ImportAsync(models, importOptions, result);
				var savedChanges = await configurationDatabaseContext.SaveChangesAsync();
				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(savedChangesExpected, savedChanges);

				foreach(var model in models)
				{
					var client = await clientStore.FindClientByIdAsync(model.ClientId);
					Assert.AreEqual(model.Description, client.Description);
					Assert.AreEqual(model.AllowedGrantTypes.Count, client.AllowedGrantTypes.Count);
					for(var i = 0; i < model.AllowedGrantTypes.Count; i++)
					{
						Assert.AreEqual(model.AllowedGrantTypes.ElementAt(i), client.AllowedGrantTypes.ElementAt(i));
					}

					Assert.AreEqual(model.ClientSecrets.Count, client.ClientSecrets.Count);
					for(var i = 0; i < model.ClientSecrets.Count; i++)
					{
						Assert.AreEqual(model.ClientSecrets.ElementAt(i).Description, client.ClientSecrets.ElementAt(i).Description);
						Assert.AreEqual(model.ClientSecrets.ElementAt(i).Expiration, client.ClientSecrets.ElementAt(i).Expiration);
						Assert.AreEqual(model.ClientSecrets.ElementAt(i).Type, client.ClientSecrets.ElementAt(i).Type);
						Assert.AreEqual(model.ClientSecrets.ElementAt(i).Value, client.ClientSecrets.ElementAt(i).Value);
					}
				}
			}

			using(var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var configurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

				Assert.AreEqual(configurationDatabaseContext.ClientClaims().Count(), models.SelectMany(client => client.Claims).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientGrantTypes().Count(), models.SelectMany(client => client.AllowedGrantTypes).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientIdentityProviderRestrictions().Count(), models.SelectMany(client => client.IdentityProviderRestrictions).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientPostLogoutRedirectUris().Count(), models.SelectMany(client => client.PostLogoutRedirectUris).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientProperties().Count(), models.SelectMany(client => client.Properties).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientRedirectUris().Count(), models.SelectMany(client => client.RedirectUris).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientSecrets().Count(), models.SelectMany(client => client.ClientSecrets).Count());
				Assert.AreEqual(configurationDatabaseContext.ClientScopes().Count(), models.SelectMany(client => client.AllowedScopes).Count());
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