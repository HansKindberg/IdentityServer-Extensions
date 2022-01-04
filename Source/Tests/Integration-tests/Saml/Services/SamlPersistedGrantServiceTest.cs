using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.FeatureManagement;
using IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsk.Saml.Models;
using Rsk.Saml.Services;
using Rsk.Saml.Validation;
using SamlPersistedGrantService = HansKindberg.IdentityServer.Saml.Services.SamlPersistedGrantService;

namespace IntegrationTests.Saml.Services
{
	[TestClass]
	public class SamlPersistedGrantServiceTest
	{
		#region Methods

		[TestCleanup]
		public void Cleanup()
		{
			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		protected internal virtual async Task ForceAuthenticationTest(DatabaseProvider databaseProvider, bool forceAuthentication)
		{
			await Task.CompletedTask;

			using(var context = new Context(databaseProvider: databaseProvider, features: new Dictionary<Feature, bool> { { Feature.Saml, true } }))
			{
				context.ApplicationBuilder.UseIdentityServer();

				var samlPersistedGrantService = (SamlPersistedGrantService)context.ServiceProvider.GetRequiredService<ISamlPersistedGrantService>();

				var key = await samlPersistedGrantService.StoreRequest(new ValidatedSamlMessage
				{
					Message = new Saml2Request
					{
						ForceAuthentication = forceAuthentication
					},
					Raw = new NameValueCollection(),
					ServiceProvider = new Rsk.Saml.Models.ServiceProvider
					{
						EntityId = "saml-service-provider-1"
					}
				});

				var validatedSamlMessage = await samlPersistedGrantService.GetRequest(key);
				var saml2Request = validatedSamlMessage.Message as Saml2Request;

				Assert.IsNotNull(saml2Request);
				Assert.AreEqual(forceAuthentication, saml2Request.ForceAuthentication);
			}
		}

		[TestInitialize]
		public void Initialize()
		{
			this.Cleanup();
		}

		[TestMethod]
		public async Task Sqlite_ForceAuthenticationIsFalse_Test()
		{
			await this.ForceAuthenticationTest(DatabaseProvider.Sqlite, false);
		}

		[TestMethod]
		public async Task Sqlite_ForceAuthenticationIsTrue_Test()
		{
			await this.ForceAuthenticationTest(DatabaseProvider.Sqlite, true);
		}

		[TestMethod]
		public async Task SqlServer_ForceAuthenticationIsFalse_Test()
		{
			await this.ForceAuthenticationTest(DatabaseProvider.SqlServer, false);
		}

		[TestMethod]
		public async Task SqlServer_ForceAuthenticationIsTrue_Test()
		{
			await this.ForceAuthenticationTest(DatabaseProvider.SqlServer, true);
		}

		#endregion
	}
}