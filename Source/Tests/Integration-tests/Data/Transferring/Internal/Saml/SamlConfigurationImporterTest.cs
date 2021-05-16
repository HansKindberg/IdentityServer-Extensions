using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Internal.Saml;
using HansKindberg.IdentityServer.FeatureManagement;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;
using Rsk.Saml.Models;
using Rsk.Saml.Stores;
using ServiceProviderModel = Rsk.Saml.Models.ServiceProvider;

namespace IntegrationTests.Data.Transferring.Internal.Saml
{
	// ReSharper disable All
	[TestClass]
	public class SamlConfigurationImporterTest
	{
		#region Fields

		private static readonly IDictionary<Feature, bool> _features = new Dictionary<Feature, bool> {{Feature.Saml, true}};

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
				.AddJsonFile($"Data/Transferring/Internal/Saml/Resources/SamlConfigurationImporter/{configurationFileName}.json", false, false);

			return await Task.FromResult(configurationBuilder.Build());
		}

		protected internal virtual async Task<Context> CreateContextAsync(DatabaseProvider databaseProvider)
		{
			return await Task.FromResult(new Context(databaseProvider: databaseProvider, features: this.Features));
		}

		protected internal virtual async Task<SamlConfigurationImporter> CreateSamlConfigurationImporterAsync(IServiceProvider serviceProvider)
		{
			return await this.CreateSamlConfigurationImporterAsync(serviceProvider.GetRequiredService<ISamlConfigurationDbContext>(), serviceProvider);
		}

		protected internal virtual async Task<SamlConfigurationImporter> CreateSamlConfigurationImporterAsync(ISamlConfigurationDbContext samlConfigurationDbContext, IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new SamlConfigurationImporter(samlConfigurationDbContext, serviceProvider.GetRequiredService<ILoggerFactory>()));
		}

		protected internal virtual async Task<X509Certificate2> GetCertificateAsync(int index, bool privateKey = false)
		{
			var certificateFileName = $"Signing-Certificate-{index}.{(privateKey ? "pfx" : "cer")}";
			var certificateFilePath = Path.Combine(Global.ProjectDirectoryPath, @"Data\Transferring\Internal\Saml\Resources\SamlConfigurationImporter", certificateFileName);
			var certificate = new X509Certificate2(certificateFilePath, "password");

			return await Task.FromResult(certificate);
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
				var samlPluginBuilder = context.ServiceProvider.GetRequiredService<ISamlPluginBuilder>();
				samlPluginBuilder.Use(context.ApplicationBuilder);
			}

			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				Assert.IsFalse(await context.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>().ServiceProviders.AnyAsync());
			}

			var importOptions = new ImportOptions();
			var serviceProviders = new List<ServiceProviderModel>
			{
				new ServiceProviderModel
				{
					AssertionConsumerServices =
					{
						new Service
						{
							Binding = "Binding-1",
							Index = 1,
							IsDefault = true,
							Location = "Location-1"
						},
						new Service
						{
							Binding = "Binding-2",
							Index = 2,
							IsDefault = true,
							Location = "Location-2"
						},
						new Service
						{
							Binding = "Binding-3",
							Index = 3,
							IsDefault = true,
							Location = "Location-3"
						}
					},
					ClaimsMapping = new Dictionary<string, string>
					{
						{"Key-1", "Value-1"},
						{"Key-2", "Value-2"},
						{"Key-3", "Value-3"}
					},
					EntityId = "Service-provider-1",
					EncryptionCertificate = await this.GetCertificateAsync(1, true),
					SigningCertificates =
					{
						await this.GetCertificateAsync(1, true),
						await this.GetCertificateAsync(2),
						await this.GetCertificateAsync(3)
					},
					SingleLogoutServices =
					{
						new Service
						{
							Binding = "Binding-1",
							Index = 1,
							IsDefault = true,
							Location = "Location-1"
						},
						new Service
						{
							Binding = "Binding-2",
							Index = 2,
							IsDefault = true,
							Location = "Location-2"
						},
						new Service
						{
							Binding = "Binding-3",
							Index = 3,
							IsDefault = true,
							Location = "Location-3"
						}
					}
				},
				new ServiceProviderModel
				{
					EntityId = "Service-provider-2",
					EncryptionCertificate = await this.GetCertificateAsync(2),
					SigningCertificates =
					{
						await this.GetCertificateAsync(1, true),
						await this.GetCertificateAsync(2)
					}
				},
				new ServiceProviderModel
				{
					EntityId = "Service-provider-3",
					EncryptionCertificate = await this.GetCertificateAsync(3)
				}
			};

			// Test certificates.
			Assert.IsTrue(serviceProviders[0].EncryptionCertificate.HasPrivateKey);
			Assert.IsFalse(serviceProviders[1].EncryptionCertificate.HasPrivateKey);
			Assert.IsFalse(serviceProviders[2].EncryptionCertificate.HasPrivateKey);

			// Save service-providers.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationImporter = await this.CreateSamlConfigurationImporterAsync(context.ServiceProvider);

				var serviceProviderImporter = samlConfigurationImporter.Importers.OfType<ServiceProviderImporter>().First();

				var result = new DataImportResult();

				await serviceProviderImporter.ImportAsync(serviceProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(17, await samlConfigurationImporter.CommitAsync());
			}

			// Test service-provider properties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationDatabaseContext = context.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();
				var serviceProviderStore = context.ServiceProvider.GetRequiredService<IServiceProviderStore>();

				var serviceProviderEntities = samlConfigurationDatabaseContext.ServiceProviders.ToArray();
				Assert.AreEqual(3, serviceProviderEntities.Length);

				foreach(var serviceProviderEntity in serviceProviderEntities)
				{
					var serviceProviderModel = await serviceProviderStore.FindServiceProviderByEntityId(serviceProviderEntity.EntityId);

					Assert.AreEqual(serviceProviderModel.AllowIdpInitiatedSso, serviceProviderEntity.AllowIdpInitiatedSso);
					Assert.AreEqual(serviceProviderModel.AssertionConsumerServices.Count, serviceProviderEntity.AssertionConsumerServices.Count);
					Assert.AreEqual(serviceProviderModel.ClaimsMapping.Count, serviceProviderEntity.ClaimsMapping.Count);
					Assert.AreEqual(serviceProviderModel.EncryptAssertions, serviceProviderEntity.EncryptAssertions);
					Assert.AreEqual(Convert.ToBase64String(serviceProviderModel.EncryptionCertificate.GetRawCertData()), Convert.ToBase64String(serviceProviderEntity.EncryptionCertificate));
					Assert.IsFalse(serviceProviderModel.EncryptionCertificate.HasPrivateKey);
					Assert.AreEqual(serviceProviderModel.EntityId, serviceProviderEntity.EntityId);
					Assert.AreEqual(serviceProviderModel.RequireAuthenticationRequestsSigned, serviceProviderEntity.RequireAuthenticationRequestsSigned);
					Assert.AreEqual(serviceProviderModel.SignAssertions, serviceProviderEntity.SignAssertions);
					Assert.AreEqual(serviceProviderModel.SigningCertificates.Count, serviceProviderEntity.SigningCertificates.Count);
					Assert.IsFalse(serviceProviderModel.SigningCertificates.Any(signingCertificate => signingCertificate.HasPrivateKey));
					Assert.AreEqual(serviceProviderModel.SingleLogoutServices.Count, serviceProviderEntity.SingleLogoutServices.Count);
				}
			}

			// Save the same unchanged service-providers again.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationImporter = await this.CreateSamlConfigurationImporterAsync(context.ServiceProvider);

				var serviceProviderImporter = samlConfigurationImporter.Importers.OfType<ServiceProviderImporter>().First();

				var result = new DataImportResult();

				await serviceProviderImporter.ImportAsync(serviceProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, await samlConfigurationImporter.CommitAsync());
			}

			// Test service-provider properties.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationDatabaseContext = context.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();
				var serviceProviderStore = context.ServiceProvider.GetRequiredService<IServiceProviderStore>();

				var serviceProviderEntities = samlConfigurationDatabaseContext.ServiceProviders.ToArray();
				Assert.AreEqual(3, serviceProviderEntities.Length);

				foreach(var serviceProviderEntity in serviceProviderEntities)
				{
					var serviceProviderModel = await serviceProviderStore.FindServiceProviderByEntityId(serviceProviderEntity.EntityId);

					Assert.AreEqual(serviceProviderModel.AllowIdpInitiatedSso, serviceProviderEntity.AllowIdpInitiatedSso);
					Assert.AreEqual(serviceProviderModel.AssertionConsumerServices.Count, serviceProviderEntity.AssertionConsumerServices.Count);
					Assert.AreEqual(serviceProviderModel.ClaimsMapping.Count, serviceProviderEntity.ClaimsMapping.Count);
					Assert.AreEqual(serviceProviderModel.EncryptAssertions, serviceProviderEntity.EncryptAssertions);
					Assert.AreEqual(Convert.ToBase64String(serviceProviderModel.EncryptionCertificate.GetRawCertData()), Convert.ToBase64String(serviceProviderEntity.EncryptionCertificate));
					Assert.IsFalse(serviceProviderModel.EncryptionCertificate.HasPrivateKey);
					Assert.AreEqual(serviceProviderModel.EntityId, serviceProviderEntity.EntityId);
					Assert.AreEqual(serviceProviderModel.RequireAuthenticationRequestsSigned, serviceProviderEntity.RequireAuthenticationRequestsSigned);
					Assert.AreEqual(serviceProviderModel.SignAssertions, serviceProviderEntity.SignAssertions);
					Assert.AreEqual(serviceProviderModel.SigningCertificates.Count, serviceProviderEntity.SigningCertificates.Count);
					Assert.IsFalse(serviceProviderModel.SigningCertificates.Any(signingCertificate => signingCertificate.HasPrivateKey));
					Assert.AreEqual(serviceProviderModel.SingleLogoutServices.Count, serviceProviderEntity.SingleLogoutServices.Count);
				}
			}

			foreach(var serviceProvider in serviceProviders)
			{
				serviceProvider.AssertionConsumerServices.Reverse();
				serviceProvider.SigningCertificates.Reverse();
				serviceProvider.SingleLogoutServices.Reverse();

				if(serviceProvider.ClaimsMapping != null)
				{
					var claimsMapping = serviceProvider.ClaimsMapping.Reverse().ToArray();
					serviceProvider.ClaimsMapping.Clear();
					foreach(var item in claimsMapping)
					{
						serviceProvider.ClaimsMapping.Add(item);
					}
				}
			}

			// Save changed service-providers.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationImporter = await this.CreateSamlConfigurationImporterAsync(context.ServiceProvider);

				var serviceProviderImporter = samlConfigurationImporter.Importers.OfType<ServiceProviderImporter>().First();

				var result = new DataImportResult();

				await serviceProviderImporter.ImportAsync(serviceProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, await samlConfigurationImporter.CommitAsync());
			}

			serviceProviders[0].AssertionConsumerServices.First(item => item.Index == 2).Index = 22;
			serviceProviders[0].AssertionConsumerServices.First(item => item.Index == 3).Index = 23;

			// Save changed service-providers.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationImporter = await this.CreateSamlConfigurationImporterAsync(context.ServiceProvider);

				var serviceProviderImporter = samlConfigurationImporter.Importers.OfType<ServiceProviderImporter>().First();

				var result = new DataImportResult();

				await serviceProviderImporter.ImportAsync(serviceProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(2, await samlConfigurationImporter.CommitAsync());
			}

			//// Test service-provider properties.
			//using(var context = await this.CreateContextAsync(databaseProvider))
			//{
			//	var samlConfigurationDatabaseContext = context.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();
			//	var serviceProviderStore = context.ServiceProvider.GetRequiredService<IServiceProviderStore>();
			//	var serviceProviderEntities = samlConfigurationDatabaseContext.ServiceProviders.ToArray();

			//	foreach(var serviceProviderEntity in serviceProviderEntities)
			//	{
			//		var serviceProviderModel = await serviceProviderStore.FindServiceProviderByEntityId(serviceProviderEntity.EntityId);

			//		// Do asserts.
			//	}
			//}

			var firstServiceProvider = serviceProviders[0];
			firstServiceProvider.AssertionConsumerServices.Clear();
			firstServiceProvider.AssertionConsumerServices.AddRange(new[]
			{
				new Service
				{
					Binding = "Binding-21",
					Index = 21,
					IsDefault = true,
					Location = "Location-21"
				},
				new Service
				{
					Binding = "Binding-22",
					Index = 22,
					IsDefault = true,
					Location = "Location-22"
				},
				new Service
				{
					Binding = "Binding-23",
					Index = 23,
					IsDefault = true,
					Location = "Location-23"
				}
			});
			firstServiceProvider.ClaimsMapping.Clear();
			firstServiceProvider.ClaimsMapping = new Dictionary<string, string>
			{
				{"Key-21", "Value-21"},
				{"Key-22", "Value-22"},
				{"Key-23", "Value-23"}
			};
			firstServiceProvider.SigningCertificates.Clear();
			firstServiceProvider.SigningCertificates.AddRange(new[]
			{
				await this.GetCertificateAsync(3),
				await this.GetCertificateAsync(2)
			});
			firstServiceProvider.SingleLogoutServices.Clear();
			firstServiceProvider.SingleLogoutServices.AddRange(new[]
			{
				new Service
				{
					Binding = "Binding-21",
					Index = 21,
					IsDefault = true,
					Location = "Location-21"
				},
				new Service
				{
					Binding = "Binding-22",
					Index = 22,
					IsDefault = true,
					Location = "Location-22"
				},
				new Service
				{
					Binding = "Binding-23",
					Index = 23,
					IsDefault = true,
					Location = "Location-23"
				}
			});

			// Save changed service-providers.
			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlConfigurationImporter = await this.CreateSamlConfigurationImporterAsync(context.ServiceProvider);
				var serviceProviderImporter = samlConfigurationImporter.Importers.OfType<ServiceProviderImporter>().First();
				var result = new DataImportResult();
				await serviceProviderImporter.ImportAsync(serviceProviders, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(0, ((DbContext)samlConfigurationImporter.DatabaseContext).ChangeTracker.Entries().Count(entry => entry.State == EntityState.Added));
				Assert.AreEqual(1, ((DbContext)samlConfigurationImporter.DatabaseContext).ChangeTracker.Entries().Count(entry => entry.State == EntityState.Deleted));
				Assert.AreEqual(9, ((DbContext)samlConfigurationImporter.DatabaseContext).ChangeTracker.Entries().Count(entry => entry.State == EntityState.Modified));
				Assert.AreEqual(10, await samlConfigurationImporter.CommitAsync());
			}

			//// Test service-provider properties.
			//using(var context = await this.CreateContextAsync(databaseProvider))
			//{
			//	var samlConfigurationDatabaseContext = context.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();
			//	var serviceProviderStore = context.ServiceProvider.GetRequiredService<IServiceProviderStore>();
			//	var serviceProviderEntities = samlConfigurationDatabaseContext.ServiceProviders.ToArray();

			//	foreach(var serviceProviderEntity in serviceProviderEntities)
			//	{
			//		var serviceProviderModel = await serviceProviderStore.FindServiceProviderByEntityId(serviceProviderEntity.EntityId);

			//		// Do asserts.
			//	}
			//}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(DatabaseProvider databaseProvider)
		{
			var importOptions = new ImportOptions {DeleteAllOthers = true};

			using(var context = await this.CreateContextAsync(databaseProvider))
			{
				var samlPluginBuilder = context.ServiceProvider.GetRequiredService<ISamlPluginBuilder>();
				samlPluginBuilder.Use(context.ApplicationBuilder);

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
					expectedSavedChanges: 1,
					expectedServiceProvidersAfterImport: 1
				);

				// Step 6
				await this.ImportAsyncScenarioTest(
					context,
					"Step-6",
					importOptions,
					expectedSavedChanges: 1,
					expectedServiceProvidersAfterImport: 2
				);

				// Step 7
				await this.ImportAsyncScenarioTest(
					context,
					"Step-7",
					importOptions,
					expectedSavedChanges: 12,
					expectedServiceProvidersAfterImport: 2
				);

				// Step 8
				await this.ImportAsyncScenarioTest(
					context,
					"Step-8",
					importOptions,
					expectedSavedChanges: 9,
					expectedServiceProvidersAfterImport: 1
				);

				// Step 9
				await this.ImportAsyncScenarioTest(
					context,
					"Step-9",
					importOptions,
					expectedSavedChanges: 4,
					expectedServiceProvidersAfterImport: 1
				);

				// Step 10
				await this.ImportAsyncScenarioTest(
					context,
					"Step-10",
					importOptions,
					expectedSavedChanges: 1,
					expectedServiceProvidersAfterImport: 0
				);
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(Context context, string fileName, ImportOptions importOptions, int expectedNumberOfErrors = 0, int expectedSavedChanges = 0, int expectedServiceProvidersAfterImport = 0)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			if(importOptions == null)
				throw new ArgumentNullException(nameof(importOptions));

			using(var serviceScope = context.ServiceProvider.CreateScope())
			{
				var samlConfigurationContext = serviceScope.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();
				var samlConfigurationImporter = await this.CreateSamlConfigurationImporterAsync(serviceScope.ServiceProvider);
				var result = new DataImportResult();
				await samlConfigurationImporter.ImportAsync(await this.CreateConfigurationAsync(fileName, context.FileProvider), importOptions, result);
				if(!result.Errors.Any() && !importOptions.VerifyOnly)
					result.SavedChanges = await samlConfigurationContext.SaveChangesAsync();
				Assert.AreEqual(expectedNumberOfErrors, result.Errors.Count);
				Assert.AreEqual(expectedSavedChanges, result.SavedChanges);
			}

			using(var serviceScope = context.ServiceProvider.CreateScope())
			{
				var samlConfigurationContext = serviceScope.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();
				Assert.AreEqual(expectedServiceProvidersAfterImport, await samlConfigurationContext.ServiceProviders.CountAsync());
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