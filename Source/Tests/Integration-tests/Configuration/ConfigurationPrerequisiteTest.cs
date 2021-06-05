using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsk.Saml.Configuration;

namespace IntegrationTests.Configuration
{
	[TestClass]
	public class ConfigurationPrerequisiteTest
	{
		#region Methods

		[TestMethod]
		public async Task ConfiguringCollections_DoesNotSetTheCollectionButAddsTheItems()
		{
			var samlIdpOptions = new SamlIdpOptions();
			Assert.AreEqual(8, samlIdpOptions.DefaultClaimMapping.Count);

			var samlPath = $"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.Saml)}";
			var defaultClaimMappingPath = $"{samlPath}:{nameof(SamlIdpOptions.DefaultClaimMapping)}";
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{$"{defaultClaimMappingPath}:First-key", "First-value"},
				{$"{defaultClaimMappingPath}:Second-key", "Second-value"},
				{$"{defaultClaimMappingPath}:Third-key", "Third-value"}
			});
			var configuration = configurationBuilder.Build();

			var services = new ServiceCollection();
			services.Configure<SamlIdpOptions>(configuration.GetSection(samlPath));
			await using(var serviceProvider = services.BuildServiceProvider())
			{
				var configuredSamlIdpOptions = serviceProvider.GetRequiredService<IOptions<SamlIdpOptions>>().Value;
				Assert.AreEqual(11, configuredSamlIdpOptions.DefaultClaimMapping.Count);
			}
		}

		#endregion
	}
}