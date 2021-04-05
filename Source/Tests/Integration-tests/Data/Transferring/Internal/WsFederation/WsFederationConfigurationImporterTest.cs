using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Internal.WsFederation;
using HansKindberg.IdentityServer.FeatureManagement;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.Stores;
using RelyingPartyModel = Rsk.WsFederation.Models.RelyingParty;

namespace IntegrationTests.Data.Transferring.Internal.WsFederation
{
	// ReSharper disable All
	[TestClass]
	public class WsFederationConfigurationImporterTest
	{
		#region Fields

		private static readonly IDictionary<Feature, bool> _features = new Dictionary<Feature, bool> {{Feature.WsFederation, true}};

		#endregion

		#region Properties

		protected internal virtual IDictionary<Feature, bool> Features => _features;

		#endregion

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
				.AddJsonFile($"Data/Transferring/Internal/WsFederation/Resources/WsFederationConfigurationImporter/{configurationFileName}.json", false, false);

			return await Task.FromResult(configurationBuilder.Build());
		}

		protected internal virtual async Task<Context> CreateContextAsync(DatabaseProvider databaseProvider)
		{
			return await Task.FromResult(new Context(databaseProvider: databaseProvider, features: this.Features));
		}

		protected internal virtual async Task<WsFederationConfigurationImporter> CreateWsFederationConfigurationImporterAsync(IServiceProvider serviceProvider)
		{
			return await this.CreateWsFederationConfigurationImporterAsync(serviceProvider.GetRequiredService<IWsFederationConfigurationDbContext>(), serviceProvider);
		}

		protected internal virtual async Task<WsFederationConfigurationImporter> CreateWsFederationConfigurationImporterAsync(IWsFederationConfigurationDbContext wsFederationConfigurationDbContext, IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new WsFederationConfigurationImporter(wsFederationConfigurationDbContext, serviceProvider.GetRequiredService<ILoggerFactory>()));
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

		[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
		protected internal virtual async Task ImportAsyncChangesTest(DatabaseProvider databaseProvider)
		{
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationPluginBuilder = context.ServiceProvider.GetRequiredService<IWsFederationPluginBuilder>();
				wsFederationPluginBuilder.Use(context.ApplicationBuilder);
			}

			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				Assert.IsFalse(await context.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>().RelyingParties.AnyAsync());
			}

			var importOptions = new ImportOptions();
			var relyingParties = new List<RelyingPartyModel>
			{
				new RelyingPartyModel
				{
					ClaimMapping = new Dictionary<string, string>
					{
						{"Key-1", "Value-1"},
						{"Key-2", "Value-2"},
						{"Key-3", "Value-3"}
					},
					Realm = "Relying-party-1",
					TokenType = "Token-type"
				},
				new RelyingPartyModel
				{
					ClaimMapping = new Dictionary<string, string>
					{
						{"Key-1", "Value-1"},
						{"Key-2", "Value-2"}
					},
					Realm = "Relying-party-2"
				},
				new RelyingPartyModel
				{
					Realm = "Relying-party-3"
				}
			};

			// Save relying-parties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationConfigurationImporter = await this.CreateWsFederationConfigurationImporterAsync(context.ServiceProvider);
				var relyingPartyImporter = wsFederationConfigurationImporter.Importers.OfType<RelyingPartyImporter>().First();
				var result = new DataImportResult();
				await relyingPartyImporter.ImportAsync(relyingParties, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(8, await wsFederationConfigurationImporter.CommitAsync());
			}

			// Test relying-party properties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationConfigurationDatabaseContext = context.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>();
				var relyingPartyStore = context.ServiceProvider.GetRequiredService<IRelyingPartyStore>();
				var relyingPartyEntities = wsFederationConfigurationDatabaseContext.RelyingParties.ToArray();
				Assert.AreEqual(3, relyingPartyEntities.Length);

				foreach(var relyingPartyEntity in relyingPartyEntities)
				{
					var relyingParty = await relyingPartyStore.FindRelyingPartyByRealm(relyingPartyEntity.Realm);

					Assert.AreEqual(relyingParty.ClaimMapping.Count, relyingPartyEntity.ClaimMapping.Count);
					Assert.AreEqual(relyingParty.DigestAlgorithm, relyingPartyEntity.DigestAlgorithm);
					Assert.AreEqual(relyingParty.Realm, relyingPartyEntity.Realm);
					Assert.AreEqual(relyingParty.SamlNameIdentifierFormat, relyingPartyEntity.SamlNameIdentifierFormat);
					Assert.AreEqual(relyingParty.SignatureAlgorithm, relyingPartyEntity.SignatureAlgorithm);
					Assert.AreEqual(relyingParty.TokenType, relyingPartyEntity.TokenType);
				}
			}

			// Save the same unchanged relying-parties again.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationConfigurationImporter = await this.CreateWsFederationConfigurationImporterAsync(context.ServiceProvider);
				var relyingPartyImporter = wsFederationConfigurationImporter.Importers.OfType<RelyingPartyImporter>().First();
				var result = new DataImportResult();
				await relyingPartyImporter.ImportAsync(relyingParties, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, await wsFederationConfigurationImporter.CommitAsync());
			}

			// Test relying-party properties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationConfigurationDatabaseContext = context.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>();
				var relyingPartyStore = context.ServiceProvider.GetRequiredService<IRelyingPartyStore>();
				var relyingPartyEntities = wsFederationConfigurationDatabaseContext.RelyingParties.ToArray();
				Assert.AreEqual(3, relyingPartyEntities.Length);

				foreach(var relyingPartyEntity in relyingPartyEntities)
				{
					var relyingParty = await relyingPartyStore.FindRelyingPartyByRealm(relyingPartyEntity.Realm);

					Assert.AreEqual(relyingParty.ClaimMapping.Count, relyingPartyEntity.ClaimMapping.Count);
					Assert.AreEqual(relyingParty.DigestAlgorithm, relyingPartyEntity.DigestAlgorithm);
					Assert.AreEqual(relyingParty.Realm, relyingPartyEntity.Realm);
					Assert.AreEqual(relyingParty.SamlNameIdentifierFormat, relyingPartyEntity.SamlNameIdentifierFormat);
					Assert.AreEqual(relyingParty.SignatureAlgorithm, relyingPartyEntity.SignatureAlgorithm);
					Assert.AreEqual(relyingParty.TokenType, relyingPartyEntity.TokenType);
				}
			}

			foreach(var relyingParty in relyingParties)
			{
				if(relyingParty.ClaimMapping != null)
				{
					var claimMapping = relyingParty.ClaimMapping.Reverse().ToArray();
					relyingParty.ClaimMapping.Clear();
					foreach(var item in claimMapping)
					{
						relyingParty.ClaimMapping.Add(item.Key, item.Value);
					}
				}
			}

			// Save changed relying-parties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationConfigurationImporter = await this.CreateWsFederationConfigurationImporterAsync(context.ServiceProvider);
				var relyingPartyImporter = wsFederationConfigurationImporter.Importers.OfType<RelyingPartyImporter>().First();
				var result = new DataImportResult();
				await relyingPartyImporter.ImportAsync(relyingParties, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, await wsFederationConfigurationImporter.CommitAsync());
			}

			var firstRelyingParty = relyingParties[0];
			firstRelyingParty.ClaimMapping.Clear();
			firstRelyingParty.ClaimMapping.Add("Key-21", "Value-21");
			firstRelyingParty.ClaimMapping.Add("Key-22", "Value-22");
			firstRelyingParty.ClaimMapping.Add("Key-23", "Value-23");

			// Save changed relying-parties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationConfigurationImporter = await this.CreateWsFederationConfigurationImporterAsync(context.ServiceProvider);
				var relyingPartyImporter = wsFederationConfigurationImporter.Importers.OfType<RelyingPartyImporter>().First();
				var result = new DataImportResult();
				await relyingPartyImporter.ImportAsync(relyingParties, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, ((DbContext)wsFederationConfigurationImporter.DatabaseContext).ChangeTracker.Entries().Count(entry => entry.State == EntityState.Added));
				Assert.AreEqual(0, ((DbContext)wsFederationConfigurationImporter.DatabaseContext).ChangeTracker.Entries().Count(entry => entry.State == EntityState.Deleted));
				Assert.AreEqual(3, ((DbContext)wsFederationConfigurationImporter.DatabaseContext).ChangeTracker.Entries().Count(entry => entry.State == EntityState.Modified));
				Assert.AreEqual(3, await wsFederationConfigurationImporter.CommitAsync());
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(DatabaseProvider databaseProvider)
		{
			var importOptions = new ImportOptions {DeleteAllOthers = true};

			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var wsFederationPluginBuilder = context.ServiceProvider.GetRequiredService<IWsFederationPluginBuilder>();
				wsFederationPluginBuilder.Use(context.ApplicationBuilder);

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
					expectedNumberOfErrors: 1
				);

				// Step 3
				await this.ImportAsyncScenarioTest(
					context,
					"Step-3",
					importOptions,
					expectedNumberOfErrors: 1
				);

				// Step 4
				await this.ImportAsyncScenarioTest(
					context,
					"Step-4",
					importOptions,
					expectedNumberOfErrors: 1
				);

				// Step 5
				await this.ImportAsyncScenarioTest(
					context,
					"Step-5",
					importOptions,
					expectedRelyingPartiesAfterImport: 1,
					expectedSavedChanges: 1
				);

				// Step 6
				await this.ImportAsyncScenarioTest(
					context,
					"Step-6",
					importOptions,
					expectedRelyingPartiesAfterImport: 2,
					expectedSavedChanges: 1
				);

				// Step 7
				await this.ImportAsyncScenarioTest(
					context,
					"Step-7",
					importOptions,
					expectedRelyingPartiesAfterImport: 2,
					expectedSavedChanges: 3
				);

				// Step 8
				await this.ImportAsyncScenarioTest(
					context,
					"Step-8",
					importOptions,
					expectedRelyingPartiesAfterImport: 1,
					expectedSavedChanges: 4
				);

				// Step 9
				await this.ImportAsyncScenarioTest(
					context,
					"Step-9",
					importOptions,
					expectedRelyingPartiesAfterImport: 1,
					expectedSavedChanges: 1
				);

				// Step 10
				await this.ImportAsyncScenarioTest(
					context,
					"Step-10",
					importOptions,
					expectedRelyingPartiesAfterImport: 0,
					expectedSavedChanges: 1
				);
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(Context context, string fileName, ImportOptions importOptions, int expectedNumberOfErrors = 0, int expectedRelyingPartiesAfterImport = 0, int expectedSavedChanges = 0)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			if(importOptions == null)
				throw new ArgumentNullException(nameof(importOptions));

			using(var serviceScope = context.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var wsFederationConfigurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>();
				var wsFederationConfigurationImporter = await this.CreateWsFederationConfigurationImporterAsync(serviceScope.ServiceProvider);
				var result = new DataImportResult();
				await wsFederationConfigurationImporter.ImportAsync(await this.CreateConfigurationAsync(fileName, context.FileProvider), importOptions, result);
				if(!result.Errors.Any() && !importOptions.VerifyOnly)
					result.SavedChanges = await wsFederationConfigurationDatabaseContext.SaveChangesAsync();
				Assert.AreEqual(expectedNumberOfErrors, result.Errors.Count);
				Assert.AreEqual(expectedSavedChanges, result.SavedChanges);
			}

			using(var serviceScope = context.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var wsFederationConfigurationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>();
				Assert.AreEqual(expectedRelyingPartiesAfterImport, await wsFederationConfigurationDatabaseContext.RelyingParties.CountAsync());
			}
		}

		[TestInitialize]
		public async Task InitializeAsync()
		{
			await this.CleanupAsync();
		}

		#endregion
	}
	// ReSharper restore All
}