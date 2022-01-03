using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.FeatureManagement
{
	[TestClass]
	public class ConfigurationFeatureManagerTest
	{
		#region Methods

		protected internal virtual async Task<IConfiguration> CreateConfigurationAsync(IDictionary<string, string> sections = null)
		{
			var configurationBuilder = new ConfigurationBuilder();

			if(sections != null)
				configurationBuilder.AddInMemoryCollection(sections);

			return await Task.FromResult(configurationBuilder.Build());
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		protected internal virtual async Task<IDictionary<string, string>> CreateConfigurationSectionsAsync(bool enabled, params string[] features)
		{
			var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach(var feature in features ?? Array.Empty<string>())
			{
				sections.Add($"{nameof(Microsoft.FeatureManagement)}:{feature}", enabled.ToString().ToLowerInvariant());
			}

			return await Task.FromResult(sections);
		}

		[TestMethod]
		public async Task GetFeatureNamesAsync_Test()
		{
			var configurationFeatureManager = new ConfigurationFeatureManager(await this.CreateConfigurationAsync());

			var featureNames = new List<string>();

			await foreach(var featureName in configurationFeatureManager.GetFeatureNamesAsync())
			{
				featureNames.Add(featureName);
			}

			Assert.AreEqual(17, featureNames.Count);
			Assert.AreEqual(nameof(Feature.CertificateForwarding), featureNames[0]);
			Assert.AreEqual(nameof(Feature.DataDirectory), featureNames[1]);
			Assert.AreEqual(nameof(Feature.DataSeeding), featureNames[2]);
			Assert.AreEqual(nameof(Feature.DataTransfer), featureNames[3]);
			Assert.AreEqual(nameof(Feature.Debug), featureNames[4]);
			Assert.AreEqual(nameof(Feature.Development), featureNames[5]);
			Assert.AreEqual(nameof(Feature.Diagnostics), featureNames[6]);
			Assert.AreEqual(nameof(Feature.DynamicAuthenticationProviders), featureNames[7]);
			Assert.AreEqual(nameof(Feature.FormsAuthentication), featureNames[8]);
			Assert.AreEqual(nameof(Feature.ForwardedHeaders), featureNames[9]);
			Assert.AreEqual(nameof(Feature.Home), featureNames[10]);
			Assert.AreEqual(nameof(Feature.HostInformation), featureNames[11]);
			Assert.AreEqual(nameof(Feature.Hsts), featureNames[12]);
			Assert.AreEqual(nameof(Feature.HttpsRedirection), featureNames[13]);
			Assert.AreEqual(nameof(Feature.Saml), featureNames[14]);
			Assert.AreEqual(nameof(Feature.SecurityHeaders), featureNames[15]);
			Assert.AreEqual(nameof(Feature.WsFederation), featureNames[16]);
		}

		[TestMethod]
		public async Task IsEnabledAsync_Test()
		{
			var alwaysDisabledFeatures = new[] { null, string.Empty, "   ", "NonExistentFeature" };

			var configurationFeatureManager = new ConfigurationFeatureManager(await this.CreateConfigurationAsync());
			foreach(var feature in alwaysDisabledFeatures.Concat(new[] { nameof(Feature.CertificateForwarding) }))
			{
				await this.IsEnabledAsyncTest(configurationFeatureManager, false, feature);
			}

			configurationFeatureManager = new ConfigurationFeatureManager(await this.CreateConfigurationAsync(await this.CreateConfigurationSectionsAsync(false, nameof(Feature.CertificateForwarding), "NonExistentFeature")));
			foreach(var feature in alwaysDisabledFeatures.Concat(new[] { nameof(Feature.CertificateForwarding) }))
			{
				await this.IsEnabledAsyncTest(configurationFeatureManager, false, feature);
			}

			configurationFeatureManager = new ConfigurationFeatureManager(await this.CreateConfigurationAsync(await this.CreateConfigurationSectionsAsync(true, nameof(Feature.CertificateForwarding), "NonExistentFeature")));
			foreach(var feature in alwaysDisabledFeatures)
			{
				await this.IsEnabledAsyncTest(configurationFeatureManager, false, feature);
			}

			await this.IsEnabledAsyncTest(configurationFeatureManager, true, nameof(Feature.CertificateForwarding));
		}

		protected internal virtual async Task IsEnabledAsyncTest(ConfigurationFeatureManager configurationFeatureManager, bool enabled, string feature)
		{
			if(configurationFeatureManager == null)
				throw new ArgumentNullException(nameof(configurationFeatureManager));

			Assert.AreEqual(enabled, await configurationFeatureManager.IsEnabledAsync(feature));
			Assert.AreEqual(enabled, await configurationFeatureManager.IsEnabledAsync(feature, "Context"));
		}

		#endregion
	}
}