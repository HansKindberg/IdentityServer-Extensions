using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Interfaces;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Builder
{
	[TestClass]
	public class ApplicationBuilderExtensionTest
	{
		#region Methods

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

		[TestMethod]
		public async Task UseIdentityServer_Sqlite_Test()
		{
			await Task.CompletedTask;

			using(var context = new Context())
			{
				var applicationBuilder = context.ApplicationBuilder;
				applicationBuilder.UseIdentityServer();

				using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
				{
					var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

					Assert.IsFalse(configurationDbContext.ApiResources.Any());
					Assert.IsFalse(configurationDbContext.ApiScopes.Any());
					Assert.IsFalse(configurationDbContext.Clients.Any());
					Assert.IsFalse(configurationDbContext.ClientCorsOrigins.Any());
					Assert.IsFalse(configurationDbContext.IdentityResources.Any());
				}
			}
		}

		[TestMethod]
		public async Task UseIdentityServer_Sqlite_WithAlreadyMigratedDatabase_Test()
		{
			await Task.CompletedTask;

			using(var context = new Context())
			{
				var applicationBuilder = context.ApplicationBuilder;

				using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
				{
					var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

					await ((DbContext)configurationDbContext).Database.MigrateAsync();

					Assert.IsFalse(configurationDbContext.ApiResources.Any());
					Assert.IsFalse(configurationDbContext.ApiScopes.Any());
					Assert.IsFalse(configurationDbContext.Clients.Any());
					Assert.IsFalse(configurationDbContext.ClientCorsOrigins.Any());
					Assert.IsFalse(configurationDbContext.IdentityResources.Any());
				}

				applicationBuilder.UseIdentityServer();

				using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
				{
					var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>();

					Assert.IsFalse(configurationDbContext.ApiResources.Any());
					Assert.IsFalse(configurationDbContext.ApiScopes.Any());
					Assert.IsFalse(configurationDbContext.Clients.Any());
					Assert.IsFalse(configurationDbContext.ClientCorsOrigins.Any());
					Assert.IsFalse(configurationDbContext.IdentityResources.Any());
				}
			}
		}

		#endregion
	}
}