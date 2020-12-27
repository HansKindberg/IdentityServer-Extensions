using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.DataProtection.Data;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity.Data;
using HansKindberg.IdentityServer.Web.Localization;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan;

namespace IntegrationTests.DependencyInjection.Extensions
{
	[TestClass]
	public class ServiceCollectionExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task AddDataDirectory_Test()
		{
			await Task.CompletedTask;

			var expectedDataDirectoryPath = Path.Combine(Global.ProjectDirectoryPath, "Test-data");

			using(var context = new Context(databaseProvider: DatabaseProvider.Sqlite, features: new Dictionary<Feature, bool> {{Feature.DataDirectory, false}}))
			{
				var serviceProvider = context.ServiceProvider;

				var applicationDomain = serviceProvider.GetRequiredService<IApplicationDomain>();
				var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

				Assert.IsNull(applicationDomain.GetData(ConfigurationKeys.DataDirectoryPath) as string);
				Assert.IsFalse(featureManager.IsEnabled(Feature.DataDirectory));
			}

			using(var context = new Context())
			{
				var serviceProvider = context.ServiceProvider;

				var applicationDomain = serviceProvider.GetRequiredService<IApplicationDomain>();
				var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

				Assert.AreEqual(expectedDataDirectoryPath, applicationDomain.GetData(ConfigurationKeys.DataDirectoryPath) as string);
				Assert.IsTrue(featureManager.IsEnabled(Feature.DataDirectory));
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task AddDataProtection_IfXmlEncryptorIsConfiguredWithTheDefaultXmlRepository_ShouldThrowAnException()
		{
			var exceptions = new List<InvalidOperationException>();

			var configurationFileName = "Default-Certificate";
			try
			{
				await this.AddDataProtectionTest(configurationFileName, typeof(CertificateXmlEncryptor), null);
			}
			catch(InvalidOperationException invalidOperationException)
			{
				exceptions.Add(invalidOperationException);
			}

			configurationFileName = "Default-Dpapi";
			try
			{
				await this.AddDataProtectionTest(configurationFileName, typeof(DpapiXmlEncryptor), null);
			}
			catch(InvalidOperationException invalidOperationException)
			{
				exceptions.Add(invalidOperationException);
			}

			configurationFileName = "Default-DpapiNg";
			try
			{
				await this.AddDataProtectionTest(configurationFileName, typeof(DpapiNGXmlEncryptor), null);
			}
			catch(InvalidOperationException invalidOperationException)
			{
				exceptions.Add(invalidOperationException);
			}

			if(exceptions.Count == 3)
				throw exceptions.First();
		}

		[TestMethod]
		public async Task AddDataProtection_Test()
		{
			var configurationFileName = "Default";
			var pathToDirectoryToDelete = Path.Combine(Global.ProjectDirectoryPath, "Test-data", "Data-protection");
			await this.AddDataProtectionTest(configurationFileName, null, null);

			var expectedXmlRepositoryType = typeof(FileSystemXmlRepository);
			configurationFileName = "FileSystem";
			await this.AddDataProtectionTest(configurationFileName, null, expectedXmlRepositoryType);
			Directory.Delete(pathToDirectoryToDelete, true);

			configurationFileName = "FileSystem-Certificate";
			await this.AddDataProtectionTest(configurationFileName, typeof(CertificateXmlEncryptor), expectedXmlRepositoryType);
			Directory.Delete(pathToDirectoryToDelete, true);

			configurationFileName = "FileSystem-Dpapi";
			await this.AddDataProtectionTest(configurationFileName, typeof(DpapiXmlEncryptor), expectedXmlRepositoryType);
			Directory.Delete(pathToDirectoryToDelete, true);

			configurationFileName = "FileSystem-DpapiNg";
			await this.AddDataProtectionTest(configurationFileName, typeof(DpapiNGXmlEncryptor), expectedXmlRepositoryType);
			Directory.Delete(pathToDirectoryToDelete, true);

			expectedXmlRepositoryType = typeof(EntityFrameworkCoreXmlRepository<SqliteDataProtection>);
			configurationFileName = "Sqlite";
			var pathToDatabaseFileToDelete = Path.Combine(Global.ProjectDirectoryPath, "Test-data", "Data-Protection.db");
			await this.AddDataProtectionTest(configurationFileName, null, expectedXmlRepositoryType);
			File.Delete(pathToDatabaseFileToDelete);

			configurationFileName = "Sqlite-Certificate";
			await this.AddDataProtectionTest(configurationFileName, typeof(CertificateXmlEncryptor), expectedXmlRepositoryType);
			File.Delete(pathToDatabaseFileToDelete);

			configurationFileName = "Sqlite-Dpapi";
			await this.AddDataProtectionTest(configurationFileName, typeof(DpapiXmlEncryptor), expectedXmlRepositoryType);
			File.Delete(pathToDatabaseFileToDelete);

			configurationFileName = "Sqlite-DpapiNg";
			await this.AddDataProtectionTest(configurationFileName, typeof(DpapiNGXmlEncryptor), expectedXmlRepositoryType);
			File.Delete(pathToDatabaseFileToDelete);
		}

		protected internal virtual async Task AddDataProtectionTest(string configurationFileName, Type expectedXmlEncryptorType, Type expectedXmlRepositoryType)
		{
			await Task.CompletedTask;

			var jsonFileRelativePath = $"DependencyInjection/Extensions/Resources/DataProtection/{configurationFileName}.json";
			var failMessage = $"Failed for {jsonFileRelativePath}.";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: jsonFileRelativePath))
			{
				context.ApplicationBuilder.UseDataProtection();

				var serviceProvider = context.ServiceProvider;

				// This will trigger exceptions if the configuration is invalid.
				var keyManager = serviceProvider.GetRequiredService<IKeyManager>();
				Assert.IsNotNull(keyManager);
				Assert.IsNotNull(keyManager.CreateNewKey(new DateTimeOffset(), new DateTimeOffset()));

				var keyManagementOptions = serviceProvider.GetRequiredService<IOptions<KeyManagementOptions>>().Value;
				Assert.AreEqual(expectedXmlEncryptorType, keyManagementOptions.XmlEncryptor?.GetType(), failMessage);
				Assert.AreEqual(expectedXmlRepositoryType, keyManagementOptions.XmlRepository?.GetType(), failMessage);
			}
		}

		[TestMethod]
		public async Task AddIdentity_Test()
		{
			await Task.CompletedTask;

			using(var context = new Context())
			{
				context.ApplicationBuilder.UseIdentity();

				var serviceProvider = context.ServiceProvider;

				using(var serviceScope = serviceProvider.GetService<IServiceScopeFactory>().CreateScope())
				{
					var identityContext = serviceScope.ServiceProvider.GetRequiredService<IdentityContext>();
					Assert.IsFalse(identityContext.Users.Any());
				}
			}
		}

		[TestMethod]
		public async Task AddIdentityServer_ConfigurationStore_Test()
		{
			await this.AddIdentityServerTest(context =>
			{
				using(var serviceScope = context.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var configurationContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();
					// ReSharper disable PossibleNullReferenceException
					var configurationStoreOptions = (ConfigurationStoreOptions)configurationContext.GetType().BaseType.GetField("storeOptions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(configurationContext);
					Assert.AreEqual("Test-schema", configurationStoreOptions.DefaultSchema);
					// ReSharper restore PossibleNullReferenceException
				}
			}, "ConfigurationStore");
		}

		[TestMethod]
		public async Task AddIdentityServer_OperationalStore_Test()
		{
			await this.AddIdentityServerTest(context =>
			{
				using(var serviceScope = context.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var operationalContext = serviceScope.ServiceProvider.GetRequiredService<IPersistedGrantDbContext>();
					// ReSharper disable PossibleNullReferenceException
					var operationalStoreOptions = (OperationalStoreOptions)operationalContext.GetType().BaseType.GetField("storeOptions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(operationalContext);
					Assert.AreEqual("Test-schema", operationalStoreOptions.DefaultSchema);
					Assert.IsTrue(operationalStoreOptions.EnableTokenCleanup);
					Assert.AreEqual(1234, operationalStoreOptions.TokenCleanupBatchSize);
					Assert.AreEqual(5678, operationalStoreOptions.TokenCleanupInterval);
					// ReSharper restore PossibleNullReferenceException
				}
			}, "OperationalStore");
		}

		[TestMethod]
		public async Task AddIdentityServer_Test()
		{
			await Task.CompletedTask;

			using(var context = new Context(databaseProvider: DatabaseProvider.Sqlite))
			{
				context.ApplicationBuilder.UseIdentityServer();

				using(var serviceScope = context.ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
				{
					var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();
					Assert.IsFalse(configurationDbContext.ApiScopes.Any());

					var persistedGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<IPersistedGrantDbContext>();
					Assert.IsFalse(persistedGrantDbContext.PersistedGrants.Any());
				}
			}
		}

		protected internal virtual async Task AddIdentityServerTest(Action<Context> assertAction, string configurationFileName)
		{
			assertAction ??= context => { };

			await Task.CompletedTask;

			var jsonFileRelativePath = $"DependencyInjection/Extensions/Resources/IdentityServer/{configurationFileName}.json";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: jsonFileRelativePath))
			{
				assertAction(context);
			}
		}

		[TestMethod]
		public async Task AddRequestLocalization_Test()
		{
			await Task.CompletedTask;

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: "DependencyInjection/Extensions/Resources/RequestLocalization/Default.json"))
			{
				var requestLocalizationOptions = context.ServiceProvider.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

				Assert.AreEqual(CultureInfo.GetCultureInfo("sv-SE"), requestLocalizationOptions.DefaultRequestCulture.Culture);
				Assert.AreEqual(CultureInfo.GetCultureInfo("sv"), requestLocalizationOptions.DefaultRequestCulture.UICulture);
				Assert.IsTrue(requestLocalizationOptions.FallBackToParentCultures);
				Assert.IsTrue(requestLocalizationOptions.FallBackToParentUICultures);
				Assert.AreEqual(3, requestLocalizationOptions.RequestCultureProviders.Count);
				Assert.IsTrue(requestLocalizationOptions.RequestCultureProviders[0] is OpenIdConnectRequestCultureProvider);
				Assert.IsTrue(requestLocalizationOptions.RequestCultureProviders[1] is CookieRequestCultureProvider);
				Assert.IsTrue(requestLocalizationOptions.RequestCultureProviders[2] is AcceptLanguageHeaderRequestCultureProvider);
				Assert.AreEqual(2, requestLocalizationOptions.SupportedCultures.Count);
				Assert.AreEqual(CultureInfo.GetCultureInfo("en-001"), requestLocalizationOptions.SupportedCultures[0]);
				Assert.AreEqual(CultureInfo.GetCultureInfo("sv-SE"), requestLocalizationOptions.SupportedCultures[1]);
				Assert.AreEqual(2, requestLocalizationOptions.SupportedUICultures.Count);
				Assert.AreEqual(CultureInfo.GetCultureInfo("en"), requestLocalizationOptions.SupportedUICultures[0]);
				Assert.AreEqual(CultureInfo.GetCultureInfo("sv"), requestLocalizationOptions.SupportedUICultures[1]);
			}

			using(var context = new Context())
			{
				var requestLocalizationOptions = context.ServiceProvider.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

				Assert.AreEqual(CultureInfo.CurrentCulture, requestLocalizationOptions.DefaultRequestCulture.Culture);
				Assert.AreEqual(CultureInfo.CurrentUICulture, requestLocalizationOptions.DefaultRequestCulture.UICulture);
				Assert.IsTrue(requestLocalizationOptions.FallBackToParentCultures);
				Assert.IsTrue(requestLocalizationOptions.FallBackToParentUICultures);
				Assert.IsFalse(requestLocalizationOptions.RequestCultureProviders.Any());
				Assert.IsFalse(requestLocalizationOptions.SupportedCultures.Any());
				Assert.IsFalse(requestLocalizationOptions.SupportedUICultures.Any());
			}
		}

		[TestCleanup]
		public void Cleanup()
		{
			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		[TestInitialize]
		public void Initialize()
		{
			this.Cleanup();
		}

		#endregion
	}
}