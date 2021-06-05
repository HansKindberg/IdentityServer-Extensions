using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity.Data;
using HansKindberg.IdentityServer.Web.Localization;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
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
		public async Task AddForwardedHeaders_Test()
		{
			await Task.CompletedTask;

			var jsonFileRelativePath = $"DependencyInjection/Extensions/Resources/ForwardedHeaders/Empty.json";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: jsonFileRelativePath))
			{
				var forwardedHeaders = context.ServiceProvider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

				Assert.IsFalse(forwardedHeaders.AllowedHosts.Any());
				Assert.AreEqual(ForwardedHeaders.None, forwardedHeaders.ForwardedHeaders);
				Assert.IsFalse(forwardedHeaders.KnownNetworks.Any());
				Assert.IsFalse(forwardedHeaders.KnownProxies.Any());
			}

			jsonFileRelativePath = $"DependencyInjection/Extensions/Resources/ForwardedHeaders/Default.json";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: jsonFileRelativePath))
			{
				var forwardedHeaders = context.ServiceProvider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

				Assert.AreEqual(3, forwardedHeaders.AllowedHosts.Count);
				Assert.AreEqual("A", forwardedHeaders.AllowedHosts[0]);
				Assert.AreEqual("B", forwardedHeaders.AllowedHosts[1]);
				Assert.AreEqual("C", forwardedHeaders.AllowedHosts[2]);

				Assert.AreEqual(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, forwardedHeaders.ForwardedHeaders);

				Assert.AreEqual(3, forwardedHeaders.KnownNetworks.Count);
				Assert.AreEqual("::ffff:10.0.0.0", forwardedHeaders.KnownNetworks[0].Prefix.ToString());
				Assert.AreEqual(104, forwardedHeaders.KnownNetworks[0].PrefixLength);
				Assert.AreEqual("::ffff:192.168.0.0", forwardedHeaders.KnownNetworks[1].Prefix.ToString());
				Assert.AreEqual(112, forwardedHeaders.KnownNetworks[1].PrefixLength);
				Assert.AreEqual("::ffff:172.16.0.0", forwardedHeaders.KnownNetworks[2].Prefix.ToString());
				Assert.AreEqual(108, forwardedHeaders.KnownNetworks[2].PrefixLength);

				Assert.AreEqual(3, forwardedHeaders.KnownProxies.Count);
				Assert.AreEqual("::ffff:10.0.0.0", forwardedHeaders.KnownProxies[0].ToString());
				Assert.AreEqual("::ffff:192.168.0.0", forwardedHeaders.KnownProxies[1].ToString());
				Assert.AreEqual("::ffff:172.16.0.0", forwardedHeaders.KnownProxies[2].ToString());
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

				using(var serviceScope = serviceProvider.CreateScope())
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
				using(var serviceScope = context.ServiceProvider.CreateScope())
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
		public async Task AddIdentityServer_KeyManagement_Test()
		{
			await Task.CompletedTask;

			using(var context = new Context(databaseProvider: DatabaseProvider.Sqlite))
			{
				context.ApplicationBuilder.UseIdentityServer();

				using(var serviceScope = context.ServiceProvider.CreateScope())
				{
					var extendedIdentityServerOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<ExtendedIdentityServerOptions>>();
					var signingAlgorithms = extendedIdentityServerOptions.Value.KeyManagement.SigningAlgorithms.ToArray();
					Assert.AreEqual(1, signingAlgorithms.Length);
					var signingAlgorithm = signingAlgorithms.ElementAt(0);
					Assert.AreEqual("RS256", signingAlgorithm.Name);
					Assert.IsTrue(signingAlgorithm.UseX509Certificate);

					var identityServerOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<IdentityServerOptions>>();
					signingAlgorithms = identityServerOptions.Value.KeyManagement.SigningAlgorithms.ToArray();
					Assert.AreEqual(1, signingAlgorithms.Length);
					signingAlgorithm = signingAlgorithms.ElementAt(0);
					Assert.AreEqual("RS256", signingAlgorithm.Name);
					Assert.IsTrue(signingAlgorithm.UseX509Certificate);
				}
			}

			var signingAlgorithmsPath = $"{ConfigurationKeys.IdentityServerPath}:{nameof(IdentityServerOptions.KeyManagement)}:{nameof(IdentityServerOptions.KeyManagement.SigningAlgorithms)}";
			var additionalConfiguration = new Dictionary<string, string>
			{
				{$"{signingAlgorithmsPath}:1:Name", "Test-1"},
				{$"{signingAlgorithmsPath}:2:Name", "Test-2"},
				{$"{signingAlgorithmsPath}:2:UseX509Certificate", "true"},
			};

			using(var context = new Context(additionalConfiguration: additionalConfiguration, databaseProvider: DatabaseProvider.Sqlite))
			{
				context.ApplicationBuilder.UseIdentityServer();

				using(var serviceScope = context.ServiceProvider.CreateScope())
				{
					var extendedIdentityServerOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<ExtendedIdentityServerOptions>>();
					var signingAlgorithms = extendedIdentityServerOptions.Value.KeyManagement.SigningAlgorithms.ToArray();
					Assert.AreEqual(3, signingAlgorithms.Length);
					var signingAlgorithm = signingAlgorithms.ElementAt(0);
					Assert.AreEqual("RS256", signingAlgorithm.Name);
					Assert.IsTrue(signingAlgorithm.UseX509Certificate);
					signingAlgorithm = signingAlgorithms.ElementAt(1);
					Assert.AreEqual("Test-1", signingAlgorithm.Name);
					Assert.IsFalse(signingAlgorithm.UseX509Certificate);
					signingAlgorithm = signingAlgorithms.ElementAt(2);
					Assert.AreEqual("Test-2", signingAlgorithm.Name);
					Assert.IsTrue(signingAlgorithm.UseX509Certificate);

					var identityServerOptions = serviceScope.ServiceProvider.GetRequiredService<IOptions<IdentityServerOptions>>();
					signingAlgorithms = identityServerOptions.Value.KeyManagement.SigningAlgorithms.ToArray();
					Assert.AreEqual(3, signingAlgorithms.Length);
					signingAlgorithm = signingAlgorithms.ElementAt(0);
					Assert.AreEqual("RS256", signingAlgorithm.Name);
					Assert.IsTrue(signingAlgorithm.UseX509Certificate);
					signingAlgorithm = signingAlgorithms.ElementAt(1);
					Assert.AreEqual("Test-1", signingAlgorithm.Name);
					Assert.IsFalse(signingAlgorithm.UseX509Certificate);
					signingAlgorithm = signingAlgorithms.ElementAt(2);
					Assert.AreEqual("Test-2", signingAlgorithm.Name);
					Assert.IsTrue(signingAlgorithm.UseX509Certificate);
				}
			}
		}

		[TestMethod]
		public async Task AddIdentityServer_OperationalStore_Test()
		{
			await this.AddIdentityServerTest(context =>
			{
				using(var serviceScope = context.ServiceProvider.CreateScope())
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

				using(var serviceScope = context.ServiceProvider.CreateScope())
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