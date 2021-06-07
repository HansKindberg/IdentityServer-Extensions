using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Extensions;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Data.Transferring.Internal
{
	[TestClass]
	[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
	public class IdentityProviderImporterTest
	{
		#region Methods

		[TestCleanup]
		public async Task CleanupAsync()
		{
			await Task.CompletedTask;

			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		protected internal virtual async Task<IConfiguration> CreateConfigurationAsync(string configurationFileName, IFileProvider fileProvider)
		{
			var configurationBuilder = new ConfigurationBuilder()
				.SetFileProvider(fileProvider)
				.AddJsonFile($"Data/Transferring/Internal/Resources/IdentityProviderImporter/{configurationFileName}.json", false, false);

			return await Task.FromResult(configurationBuilder.Build());
		}

		protected internal virtual async Task<ConfigurationImporter> CreateConfigurationImporterAsync(IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new ConfigurationImporter(serviceProvider.GetRequiredService<IClientConfigurationValidator>(), serviceProvider.GetRequiredService<IConfigurationDbContext>(), serviceProvider.GetRequiredService<ILoggerFactory>()));
		}

		protected internal virtual void FakeHttpContextIfNecessary(Context context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

			httpContextAccessor.HttpContext ??= new DefaultHttpContext {RequestServices = context.ServiceProvider};
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_Changes_Test()
		{
			await this.ImportAsyncChangesTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_Sqlite_Scenario_Test()
		{
			await this.ImportAsyncScenarioTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_Changes_Test()
		{
			await this.ImportAsyncChangesTest(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task ImportAsync_SqlServer_Scenario_Test()
		{
			await this.ImportAsyncScenarioTest(DatabaseProvider.SqlServer);
		}

		public async Task ImportAsyncChangesTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				context.ApplicationBuilder.UseIdentityServer();
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				Assert.IsFalse(await context.ServiceProvider.GetRequiredService<IConfigurationDbContext>().IdentityProviders.AnyAsync());
			}

			var importOptions = new ImportOptions();
			var identityProviders = new List<IdentityProvider>
			{
				new OidcProvider
				{
					Properties =
					{
						{"First-property", "First-property-value"},
						{"Second-property", "Second-property-value"}
					},
					Scheme = "Scheme-1"
				},
				new OidcProvider
				{
					Scheme = "Scheme-2"
				}
			};

			// Save identity-providers.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var configurationImporter = await this.CreateConfigurationImporterAsync(context.ServiceProvider);

				var identityProviderImporter = configurationImporter.Importers.OfType<IdentityProviderImporter>().First();

				var result = new DataImportResult();

				await identityProviderImporter.ImportAsync(identityProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(2, await configurationImporter.CommitAsync());
			}

			// Test identity-provider properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				this.FakeHttpContextIfNecessary(context);

				var identityProviderStore = context.ServiceProvider.GetRequiredService<IIdentityProviderStore>();

				var identityProviderNames = (await identityProviderStore.GetAllSchemeNamesAsync()).ToArray();
				Assert.AreEqual(2, identityProviderNames.Length);

				IdentityProvider identityProvider;

				foreach(var identityProviderName in identityProviderNames)
				{
					identityProvider = await identityProviderStore.GetBySchemeAsync(identityProviderName.Scheme);

					Assert.AreEqual(identityProviderName.DisplayName, identityProvider.DisplayName);
					Assert.AreEqual(identityProviderName.Enabled, identityProvider.Enabled);
					Assert.AreEqual(identityProviderName.Scheme, identityProvider.Scheme);
				}

				identityProvider = await identityProviderStore.GetBySchemeAsync("Scheme-1");
				Assert.AreEqual(2, identityProvider.Properties.Count);

				identityProvider = await identityProviderStore.GetBySchemeAsync("Scheme-2");
				Assert.AreEqual(0, identityProvider.Properties.Count);
			}

			// Save the same unchanged identity-providers again.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var configurationImporter = await this.CreateConfigurationImporterAsync(context.ServiceProvider);

				var identityProviderImporter = configurationImporter.Importers.OfType<IdentityProviderImporter>().First();

				var result = new DataImportResult();

				await identityProviderImporter.ImportAsync(identityProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, await configurationImporter.CommitAsync());
			}

			foreach(var identityProvider in identityProviders)
			{
				identityProvider.Properties.Add("New-property", "New-property-value");
			}

			// Save changed identity-providers.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var configurationImporter = await this.CreateConfigurationImporterAsync(context.ServiceProvider);

				var identityProviderImporter = configurationImporter.Importers.OfType<IdentityProviderImporter>().First();

				var result = new DataImportResult();

				await identityProviderImporter.ImportAsync(identityProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(2, await configurationImporter.CommitAsync());
			}

			// Test identity-provider properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				this.FakeHttpContextIfNecessary(context);

				var identityProviderStore = context.ServiceProvider.GetRequiredService<IIdentityProviderStore>();

				var identityProviderNames = (await identityProviderStore.GetAllSchemeNamesAsync()).ToArray();
				Assert.AreEqual(2, identityProviderNames.Length);

				IdentityProvider identityProvider;

				foreach(var identityProviderName in identityProviderNames)
				{
					identityProvider = await identityProviderStore.GetBySchemeAsync(identityProviderName.Scheme);

					Assert.AreEqual(identityProviderName.DisplayName, identityProvider.DisplayName);
					Assert.AreEqual(identityProviderName.Enabled, identityProvider.Enabled);
					Assert.AreEqual(identityProviderName.Scheme, identityProvider.Scheme);
				}

				identityProvider = await identityProviderStore.GetBySchemeAsync("Scheme-1");
				Assert.AreEqual(3, identityProvider.Properties.Count);

				identityProvider = await identityProviderStore.GetBySchemeAsync("Scheme-2");
				Assert.AreEqual(1, identityProvider.Properties.Count);
			}
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
					context,
					"Step-1",
					importOptions,
					expectedNumberOfErrors: 1
				);

				// Step 2
				await this.ImportAsyncScenarioTest(
					context,
					"Step-2",
					importOptions,
					expectedIdentityProvidersAfterImport: 1,
					expectedSavedChanges: 1
				);

				// Step 3
				await this.ImportAsyncScenarioTest(
					context,
					"Step-3",
					importOptions,
					expectedIdentityProvidersAfterImport: 2,
					expectedSavedChanges: 2
				);

				// Step 4
				await this.ImportAsyncScenarioTest(
					context,
					"Step-4",
					importOptions,
					expectedSavedChanges: 2,
					expectedIdentityProvidersAfterImport: 1
				);
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(Context context, string fileName, ImportOptions importOptions, int expectedIdentityProvidersAfterImport = 0, int expectedNumberOfErrors = 0, int expectedSavedChanges = 0)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			if(importOptions == null)
				throw new ArgumentNullException(nameof(importOptions));

			var serviceProvider = context.ServiceProvider;

			using(var serviceScope = serviceProvider.CreateScope())
			{
				var configurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();
				var configurationImporter = await this.CreateConfigurationImporterAsync(serviceScope.ServiceProvider);
				var result = new DataImportResult();
				await configurationImporter.ImportAsync(await this.CreateConfigurationAsync(fileName, context.FileProvider), importOptions, result);
				if(!result.Errors.Any() && !importOptions.VerifyOnly)
					result.SavedChanges = await configurationDatabaseContext.SaveChangesAsync();
				Assert.AreEqual(expectedNumberOfErrors, result.Errors.Count);
				Assert.AreEqual(expectedSavedChanges, result.SavedChanges);
			}

			using(var serviceScope = serviceProvider.CreateScope())
			{
				var databaseContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

				Assert.AreEqual(expectedIdentityProvidersAfterImport, await databaseContext.IdentityProviders.CountAsync());
			}
		}

		[TestInitialize]
		public async Task InitializeAsync()
		{
			await this.CleanupAsync();
		}

		#endregion
	}
}