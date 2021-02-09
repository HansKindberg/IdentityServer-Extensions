using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Identity;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Security.Claims;
using UserEntity = HansKindberg.IdentityServer.Identity.User;

namespace IntegrationTests.Identity
{
	[TestClass]
	public class IdentityFacadeTest
	{
		#region Methods

		[TestMethod]
		public async Task ClaimsAreEqualAsync_Test()
		{
			using(var context = new Context(databaseProvider: DatabaseProvider.Sqlite))
			{
				var serviceProvider = context.ServiceProvider;
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				Assert.IsTrue(await identityFacade.ClaimsAreEqualAsync(null, null));
				Assert.IsFalse(await identityFacade.ClaimsAreEqualAsync(new ClaimBuilderCollection(), null));
				Assert.IsFalse(await identityFacade.ClaimsAreEqualAsync(null, new List<Claim>()));

				var firstClaims = new ClaimBuilderCollection
				{
					new ClaimBuilder
					{
						Type = "Claim-type-1",
						Value = "Claim-value-1"
					}
				};
				var secondClaims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-1")
				};
				Assert.IsTrue(await identityFacade.ClaimsAreEqualAsync(firstClaims, secondClaims));

				secondClaims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-2")
				};
				Assert.IsFalse(await identityFacade.ClaimsAreEqualAsync(firstClaims, secondClaims));
			}
		}

		[TestCleanup]
		public async Task CleanupAsync()
		{
			await Task.CompletedTask;

			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		protected internal virtual async Task<IdentityFacade> CreateIdentityFacadeAsync(IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new IdentityFacade(serviceProvider.GetRequiredService<SignInManager<UserEntity>>(), serviceProvider.GetRequiredService<UserManager>()));
		}

		[TestInitialize]
		public async Task InitializeAsync()
		{
			await this.CleanupAsync();
		}

		protected internal virtual async Task ResolveUserAsync_IfTheUserAlreadyExists_ShouldUpdateClaims(DatabaseProvider databaseProvider)
		{
			const string claimType = "Claim-type";
			const string provider = "Provider";
			const string subject = "Subject";

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				var claimValue = "Initial value";
				var claims = new ClaimBuilderCollection
				{
					new ClaimBuilder
					{
						Type = claimType,
						Value = claimValue
					}
				};

				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				var user = await identityFacade.ResolveUserAsync(claims, provider, subject);
				Assert.IsNotNull(user);
				var userClaims = await identityFacade.UserManager.GetClaimsAsync(user);
				Assert.IsNotNull(userClaims);
				Assert.AreEqual(1, userClaims.Count);
				Assert.AreEqual(claimType, userClaims[0].Type);
				Assert.AreEqual(claimValue, userClaims[0].Value);

				claimValue = "New value";
				claims[0].Value = claimValue;

				user = await identityFacade.ResolveUserAsync(claims, provider, subject);
				Assert.IsNotNull(user);
				userClaims = await identityFacade.UserManager.GetClaimsAsync(user);
				Assert.IsNotNull(userClaims);
				Assert.AreEqual(1, userClaims.Count);
				Assert.AreEqual(claimType, userClaims[0].Type);
				Assert.AreEqual(claimValue, userClaims[0].Value);

				claims = new ClaimBuilderCollection
				{
					new ClaimBuilder
					{
						Type = "1",
						Value = "1"
					},
					new ClaimBuilder
					{
						Type = "1",
						Value = "2"
					},
					new ClaimBuilder
					{
						Type = "1",
						Value = "3"
					},
					new ClaimBuilder
					{
						Type = "1",
						Value = "4"
					}
				};
				user = await identityFacade.ResolveUserAsync(claims, provider, subject);
				userClaims = await identityFacade.UserManager.GetClaimsAsync(user);
				Assert.IsNotNull(userClaims);
				Assert.AreEqual(4, userClaims.Count);

				claims.Add(new ClaimBuilder {Type = "2", Value = "1"});
				user = await identityFacade.ResolveUserAsync(claims, provider, subject);
				userClaims = await identityFacade.UserManager.GetClaimsAsync(user);
				Assert.IsNotNull(userClaims);
				Assert.AreEqual(5, userClaims.Count);

				claims.RemoveAt(4);
				claims.RemoveAt(3);
				for(var i = 0; i < claims.Count; i++)
				{
					var claim = claims[i];
					claim.Type = $"{i + 1}";
					claim.Value += " (changed)";
				}

				user = await identityFacade.ResolveUserAsync(claims, provider, subject);
				userClaims = await identityFacade.UserManager.GetClaimsAsync(user);
				Assert.IsNotNull(userClaims);
				Assert.AreEqual(3, userClaims.Count);
				Assert.AreEqual(userClaims[0].Type, "1");
				Assert.AreEqual(userClaims[0].Value, "1 (changed)");
				Assert.AreEqual(userClaims[1].Type, "2");
				Assert.AreEqual(userClaims[1].Value, "2 (changed)");
				Assert.AreEqual(userClaims[2].Type, "3");
				Assert.AreEqual(userClaims[2].Value, "3 (changed)");
			}
		}

		[TestMethod]
		public async Task ResolveUserAsync_Sqlite_IfTheUserAlreadyExists_ShouldUpdateClaims()
		{
			await this.ResolveUserAsync_IfTheUserAlreadyExists_ShouldUpdateClaims(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ResolveUserAsync_SqlServer_IfTheUserAlreadyExists_ShouldUpdateClaims()
		{
			await this.ResolveUserAsync_IfTheUserAlreadyExists_ShouldUpdateClaims(DatabaseProvider.SqlServer);
		}

		#endregion
	}
}