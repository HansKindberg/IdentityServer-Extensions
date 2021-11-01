using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Identity.Data;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Security.Claims;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserLoginModel = HansKindberg.IdentityServer.Identity.Models.UserLogin;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace IntegrationTests.Identity
{
	[TestClass]
	public class IdentityFacadeTest
	{
		#region Methods

		[TestMethod]
		public async Task Case_Sqlite_Test()
		{
			await this.CaseTest(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task Case_SqlServer_Test()
		{
			await this.CaseTest(DatabaseProvider.SqlServer);
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		protected internal virtual async Task CaseTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";
				await identityFacade.UserManager.CreateAsync(new UserEntity { Id = id, UserName = "Test" });
				await identityFacade.DatabaseContext.SaveChangesAsync();
				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id.ToLowerInvariant()));
				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id.ToUpperInvariant()));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FirstOrDefaultAsync(user => user.Id == id));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FirstOrDefaultAsync(user => user.Id == id.ToLowerInvariant()));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FirstOrDefaultAsync(user => user.Id == id.ToUpperInvariant()));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FirstOrDefaultAsync(user => EF.Functions.Like(user.Id, id)));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FirstOrDefaultAsync(user => EF.Functions.Like(user.Id, id.ToLowerInvariant())));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FirstOrDefaultAsync(user => EF.Functions.Like(user.Id, id.ToUpperInvariant())));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FindAsync(id));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FindAsync(id.ToLowerInvariant()));
				Assert.IsNotNull(await identityFacade.DatabaseContext.Users.FindAsync(id.ToUpperInvariant()));
			}
		}

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
			return await Task.FromResult(new IdentityFacade(serviceProvider.GetRequiredService<ILoggerFactory>(), serviceProvider.GetRequiredService<SignInManager<UserEntity>>(), serviceProvider.GetRequiredService<UserManager>()));
		}

		protected internal virtual async Task GetUserLoginsAsync_IfTheParameterIsAnIdThatNoUserExistsFor_ShouldReturnAnEmptyCollection(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				Assert.IsFalse((await identityFacade.GetUserLoginsAsync(Guid.NewGuid().ToString())).Any());
			}
		}

		[TestMethod]
		public async Task GetUserLoginsAsync_Sqlite_IfTheParameterIsAnIdThatNoUserExistsFor_ShouldReturnAnEmptyCollection()
		{
			await this.GetUserLoginsAsync_IfTheParameterIsAnIdThatNoUserExistsFor_ShouldReturnAnEmptyCollection(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task GetUserLoginsAsync_SqlServer_IfTheParameterIsAnIdThatNoUserExistsFor_ShouldReturnAnEmptyCollection()
		{
			await this.GetUserLoginsAsync_IfTheParameterIsAnIdThatNoUserExistsFor_ShouldReturnAnEmptyCollection(DatabaseProvider.SqlServer);
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

				claims.Add(new ClaimBuilder { Type = "2", Value = "1" });
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

		protected internal virtual async Task ResolveUserAsync_IfTheUserAlreadyExistsAndTheDatabaseClaimsHaveBeenExplicitlyChanged_ShouldCorrectClaims(DatabaseProvider databaseProvider)
		{
			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = "First-type",
					Value = "First-value"
				},
				new ClaimBuilder
				{
					Type = "Second-type",
					Value = "Second-value"
				},
				new ClaimBuilder
				{
					Type = "Third-type",
					Value = "Third-value"
				}
			};
			const string provider = "Provider";
			const string userIdentifier = "User-identifier";

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				var user = await identityFacade.ResolveUserAsync(claims, provider, userIdentifier);
				Assert.IsNotNull(user);
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
				Assert.IsNotNull(identityContext);
				Assert.AreEqual(3, identityContext.UserClaims.Count());
				Assert.AreEqual("First-type", (await identityContext.UserClaims.FindAsync(1)).ClaimType);
				Assert.AreEqual("First-value", (await identityContext.UserClaims.FindAsync(1)).ClaimValue);
				Assert.AreEqual("Second-type", (await identityContext.UserClaims.FindAsync(2)).ClaimType);
				Assert.AreEqual("Second-value", (await identityContext.UserClaims.FindAsync(2)).ClaimValue);
				Assert.AreEqual("Third-type", (await identityContext.UserClaims.FindAsync(3)).ClaimType);
				Assert.AreEqual("Third-value", (await identityContext.UserClaims.FindAsync(3)).ClaimValue);
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
				var thirdClaim = await identityContext.UserClaims.FindAsync(3);
				thirdClaim.ClaimType = "Second-type";
				thirdClaim.ClaimValue = "Second-value";
				Assert.AreEqual(1, await identityContext.SaveChangesAsync());
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
				Assert.IsNotNull(identityContext);
				Assert.AreEqual(3, identityContext.UserClaims.Count());
				Assert.AreEqual("First-type", (await identityContext.UserClaims.FindAsync(1)).ClaimType);
				Assert.AreEqual("First-value", (await identityContext.UserClaims.FindAsync(1)).ClaimValue);
				Assert.AreEqual("Second-type", (await identityContext.UserClaims.FindAsync(2)).ClaimType);
				Assert.AreEqual("Second-value", (await identityContext.UserClaims.FindAsync(2)).ClaimValue);
				Assert.AreEqual("Second-type", (await identityContext.UserClaims.FindAsync(3)).ClaimType);
				Assert.AreEqual("Second-value", (await identityContext.UserClaims.FindAsync(3)).ClaimValue);
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				var user = await identityFacade.ResolveUserAsync(claims, provider, userIdentifier);
				Assert.IsNotNull(user);
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
				Assert.IsNotNull(identityContext);
				Assert.AreEqual(3, identityContext.UserClaims.Count());
				Assert.AreEqual("First-type", (await identityContext.UserClaims.FindAsync(1)).ClaimType);
				Assert.AreEqual("First-value", (await identityContext.UserClaims.FindAsync(1)).ClaimValue);
				Assert.AreEqual("Second-type", (await identityContext.UserClaims.FindAsync(2)).ClaimType);
				Assert.AreEqual("Second-value", (await identityContext.UserClaims.FindAsync(2)).ClaimValue);
				Assert.AreEqual("Third-type", (await identityContext.UserClaims.FindAsync(3)).ClaimType);
				Assert.AreEqual("Third-value", (await identityContext.UserClaims.FindAsync(3)).ClaimValue);
			}
		}

		[TestMethod]
		public async Task ResolveUserAsync_Sqlite_IfTheUserAlreadyExists_ShouldUpdateClaims()
		{
			await this.ResolveUserAsync_IfTheUserAlreadyExists_ShouldUpdateClaims(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ResolveUserAsync_Sqlite_IfTheUserAlreadyExistsAndTheDatabaseClaimsHaveBeenExplicitlyChanged_ShouldCorrectClaims()
		{
			await this.ResolveUserAsync_IfTheUserAlreadyExistsAndTheDatabaseClaimsHaveBeenExplicitlyChanged_ShouldCorrectClaims(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task ResolveUserAsync_SqlServer_IfTheUserAlreadyExists_ShouldUpdateClaims()
		{
			await this.ResolveUserAsync_IfTheUserAlreadyExists_ShouldUpdateClaims(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task ResolveUserAsync_SqlServer_IfTheUserAlreadyExistsAndTheDatabaseClaimsHaveBeenExplicitlyChanged_ShouldCorrectClaims()
		{
			await this.ResolveUserAsync_IfTheUserAlreadyExistsAndTheDatabaseClaimsHaveBeenExplicitlyChanged_ShouldCorrectClaims(DatabaseProvider.SqlServer);
		}

		protected internal virtual async Task SaveUserAsync_IfDatabaseContextSaveChangesIsCalled_ShouldPersistTheUserToTheDatabase(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				var identityResult = await identityFacade.SaveUserAsync(new UserModel { Password = "P@ssword12", UserName = "Test" });
				Assert.IsTrue(identityResult.Succeeded);
				Assert.AreEqual(0, await identityFacade.DatabaseContext.Users.CountAsync());
				Assert.IsNull(await identityFacade.GetUserAsync("Test"));
				Assert.AreEqual(1, await identityFacade.DatabaseContext.SaveChangesAsync());
				Assert.AreEqual(1, await identityFacade.DatabaseContext.Users.CountAsync());
				var user = await identityFacade.GetUserAsync("Test");
				Assert.IsNotNull(user);
				Assert.AreEqual("Test", user.UserName);
			}
		}

		protected internal virtual async Task SaveUserAsync_IfDatabaseContextSaveChangesIsNotCalled_ShouldNotPersistTheUserToTheDatabase(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				var identityResult = await identityFacade.SaveUserAsync(new UserModel { Password = "P@ssword12", UserName = "Test" });
				Assert.IsTrue(identityResult.Succeeded);
				Assert.AreEqual(0, await identityFacade.DatabaseContext.Users.CountAsync());
				Assert.IsNull(await identityFacade.GetUserAsync("Test"));
			}
		}

		[TestMethod]
		public async Task SaveUserAsync_Sqlite_IfDatabaseContextSaveChangesIsCalled_ShouldPersistTheUserToTheDatabase()
		{
			await this.SaveUserAsync_IfDatabaseContextSaveChangesIsCalled_ShouldPersistTheUserToTheDatabase(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserAsync_Sqlite_IfDatabaseContextSaveChangesIsNotCalled_ShouldNotPersistTheUserToTheDatabase()
		{
			await this.SaveUserAsync_IfDatabaseContextSaveChangesIsNotCalled_ShouldNotPersistTheUserToTheDatabase(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserAsync_SqlServer_IfDatabaseContextSaveChangesIsCalled_ShouldPersistTheUserToTheDatabase()
		{
			await this.SaveUserAsync_IfDatabaseContextSaveChangesIsCalled_ShouldPersistTheUserToTheDatabase(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserAsync_SqlServer_IfDatabaseContextSaveChangesIsNotCalled_ShouldNotPersistTheUserToTheDatabase()
		{
			await this.SaveUserAsync_IfDatabaseContextSaveChangesIsNotCalled_ShouldNotPersistTheUserToTheDatabase(DatabaseProvider.SqlServer);
		}

		protected internal virtual async Task SaveUserLoginAsync_IfAUserLoginExistAndTheUserIdIsNotNullAndAUserForTheUserIdDoesNotExist_ShouldCreateTheUserAndMoveTheUserLoginAndRemoveThePreviousLoginUser(DatabaseProvider databaseProvider)
		{
			const string newId = "afba39de-89b3-47fb-88a5-3dfb5e0fd176";
			const string oldId = "d63c5a23-042e-4c74-8ca1-269407c1a799";

			// Prepare
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { Id = oldId, UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user);
				var claims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-1"),
					new Claim("Claim-type-2", "Claim-value-2"),
					new Claim("Claim-type-3", "Claim-value-3")
				};
				await identityFacade.UserManager.AddClaimsAsync(user, claims);
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-1", "User-identifier-1", "Provider-1"));
				Assert.AreEqual(3, await identityFacade.DatabaseContext.UserClaims.CountAsync());
				Assert.AreEqual(1, await identityFacade.DatabaseContext.UserLogins.CountAsync());
				Assert.AreEqual(1, await identityFacade.DatabaseContext.Users.CountAsync());
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				Assert.IsFalse(identityFacade.UserManager.Store.AutoSaveChanges);

				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = newId, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(1, identityFacade.DatabaseContext.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Added));
				Assert.AreEqual(1, identityFacade.DatabaseContext.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Deleted));
				Assert.AreEqual(1, identityFacade.DatabaseContext.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Modified));
				Assert.AreEqual(3, await identityFacade.DatabaseContext.SaveChangesAsync());
				Assert.AreEqual(0, await identityFacade.DatabaseContext.UserClaims.CountAsync());
				Assert.AreEqual(1, await identityFacade.DatabaseContext.UserLogins.CountAsync());
				Assert.AreEqual(newId, (await identityFacade.DatabaseContext.UserLogins.FirstAsync()).UserId);
				Assert.AreEqual(1, await identityFacade.DatabaseContext.Users.CountAsync());
			}
		}

		protected internal virtual async Task SaveUserLoginAsync_IfAUserLoginExistAndTheUserIdIsNull_ShouldDoNothing(DatabaseProvider databaseProvider)
		{
			// Prepare
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user);
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-1", "User-identifier-1", "Provider-1"));
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);
				Assert.IsFalse(identityFacade.UserManager.Store.AutoSaveChanges);

				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(0, await identityFacade.DatabaseContext.SaveChangesAsync());
			}
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_IfNothingIsChanged_ShouldDoNothing()
		{
			await this.SaveUserLoginsAsync_IfNothingIsChanged_ShouldDoNothing(DatabaseProvider.SqlServer);
		}

		protected internal virtual async Task SaveUserLoginAsync_IfTheUserAndUserLoginDoesNotExist_ShouldCreateTheUserAndUserLogin(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				Assert.IsFalse(await identityFacade.DatabaseContext.UserLogins.AnyAsync());
				Assert.IsFalse(await identityFacade.DatabaseContext.Users.AnyAsync());

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";
				const string provider = "Provider-1";
				const string userIdentifier = "User-identifier-1";
				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = id, Provider = provider, UserIdentifier = userIdentifier });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(2, await identityFacade.DatabaseContext.SaveChangesAsync());

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync(provider, userIdentifier));
			}
		}

		protected internal virtual async Task SaveUserLoginAsync_IfTheUserAndUserLoginDoesNotExistAndIfTheUserIdIsNull_ShouldCreateTheUserAndUserLogin(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(2, await identityFacade.DatabaseContext.SaveChangesAsync());

				Assert.AreEqual(1, await identityFacade.Users.CountAsync());
				var user = await identityFacade.Users.FirstAsync();
				var userByLogin = await identityFacade.GetUserAsync("Provider-1", "User-identifier-1");
				Assert.IsNotNull(userByLogin);
				Assert.AreEqual(user.Id, userByLogin.Id);
			}
		}

		protected internal virtual async Task SaveUserLoginAsync_IfTheUserIdIsChanged_ShouldChangeTheUserIdAndRemoveClaims(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";

				// Prepare
				var autoSaveChanges = identityFacade.UserManager.Store.AutoSaveChanges;
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { Id = id, UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user);
				var claims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-1"),
					new Claim("Claim-type-2", "Claim-value-2"),
					new Claim("Claim-type-3", "Claim-value-3")
				};
				await identityFacade.UserManager.AddClaimsAsync(user, claims);
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-1", "User-identifier-1", "Provider-1"));
				identityFacade.UserManager.Store.AutoSaveChanges = autoSaveChanges;

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.AreEqual(3, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));

				const string newId = "89648c35-08c1-4113-bf09-bdf5b06cece5";
				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = newId, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(6, await identityFacade.DatabaseContext.SaveChangesAsync());

				Assert.IsNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(newId));
				Assert.AreEqual(0, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));
			}
		}

		protected internal virtual async Task SaveUserLoginAsync_IfTheUserIdIsChangedAndTheOldUserHasMoreLogins_ShouldNotRemoveTheOldUserButRemoveTheClaimsForTheOldUser(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";

				// Prepare
				var autoSaveChanges = identityFacade.UserManager.Store.AutoSaveChanges;
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { Id = id, UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user);
				var claims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-1"),
					new Claim("Claim-type-2", "Claim-value-2"),
					new Claim("Claim-type-3", "Claim-value-3")
				};
				await identityFacade.UserManager.AddClaimsAsync(user, claims);
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-1", "User-identifier-1", "Provider-1"));
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-2", "User-identifier-2", "Provider-2"));
				identityFacade.UserManager.Store.AutoSaveChanges = autoSaveChanges;

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.AreEqual(3, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-2", "User-identifier-2"));

				const string newId = "89648c35-08c1-4113-bf09-bdf5b06cece5";
				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = newId, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(6, await identityFacade.DatabaseContext.SaveChangesAsync());

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(newId));
				Assert.AreEqual(0, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));
				Assert.AreEqual(newId, (await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1")).Id);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-2", "User-identifier-2"));
				Assert.AreEqual(id, (await identityFacade.UserManager.FindByLoginAsync("Provider-2", "User-identifier-2")).Id);
			}
		}

		protected internal virtual async Task SaveUserLoginAsync_IfTheUserIdIsChangedAndTheOldUserHasNoMoreLogins_ShouldRemoveTheOldUser(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";

				// Prepare
				var autoSaveChanges = identityFacade.UserManager.Store.AutoSaveChanges;
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { Id = id, UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user);
				var claims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-1"),
					new Claim("Claim-type-2", "Claim-value-2"),
					new Claim("Claim-type-3", "Claim-value-3")
				};
				await identityFacade.UserManager.AddClaimsAsync(user, claims);
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-1", "User-identifier-1", "Provider-1"));
				identityFacade.UserManager.Store.AutoSaveChanges = autoSaveChanges;

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.AreEqual(3, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));

				const string newId = "89648c35-08c1-4113-bf09-bdf5b06cece5";
				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = newId, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(6, await identityFacade.DatabaseContext.SaveChangesAsync());

				Assert.IsNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(newId));
				Assert.AreEqual(0, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		protected internal virtual async Task SaveUserLoginAsync_IfTheUserIdIsForAPasswordUser_ShouldThrowAnInvalidOperationException(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";

				// Prepare
				var autoSaveChanges = identityFacade.UserManager.Store.AutoSaveChanges;
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { Id = id, UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user, "P@ssword12");
				identityFacade.UserManager.Store.AutoSaveChanges = autoSaveChanges;

				user = await identityFacade.UserManager.FindByIdAsync(id);
				Assert.IsNotNull(user);
				Assert.IsNotNull(user.PasswordHash);

				try
				{
					await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = id, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
					Assert.Fail("Should have thrown an exception");
				}
				catch(Exception exception)
				{
					if(!(exception is InvalidOperationException))
						Assert.Fail("Should have thrown an invalid-operation-exception.");
				}
			}
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfAUserLoginExistAndTheUserIdIsNotNullAndAUserForTheUserIdDoesNotExist_ShouldCreateTheUserAndMoveTheUserLoginAndRemoveThePreviousLoginUser()
		{
			await this.SaveUserLoginAsync_IfAUserLoginExistAndTheUserIdIsNotNullAndAUserForTheUserIdDoesNotExist_ShouldCreateTheUserAndMoveTheUserLoginAndRemoveThePreviousLoginUser(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfAUserLoginExistAndTheUserIdIsNull_ShouldDoNothing()
		{
			await this.SaveUserLoginAsync_IfAUserLoginExistAndTheUserIdIsNull_ShouldDoNothing(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfNothingIsChanged_ShouldDoNothing()
		{
			await this.SaveUserLoginsAsync_IfNothingIsChanged_ShouldDoNothing(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserAndUserLoginDoesNotExist_ShouldCreateTheUserAndUserLogin()
		{
			await this.SaveUserLoginAsync_IfTheUserAndUserLoginDoesNotExist_ShouldCreateTheUserAndUserLogin(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserAndUserLoginDoesNotExistAndIfTheUserIdIsNull_ShouldCreateTheUserAndUserLogin()
		{
			await this.SaveUserLoginAsync_IfTheUserAndUserLoginDoesNotExistAndIfTheUserIdIsNull_ShouldCreateTheUserAndUserLogin(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserIdIsChanged_ShouldChangeTheUserIdAndRemoveClaims()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsChanged_ShouldChangeTheUserIdAndRemoveClaims(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserIdIsChangedAndTheOldUserHasMoreLogins_ShouldNotRemoveTheOldUserButRemoveTheClaimsForTheOldUser()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsChangedAndTheOldUserHasMoreLogins_ShouldNotRemoveTheOldUserButRemoveTheClaimsForTheOldUser(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserIdIsChangedAndTheOldUserHasNoMoreLogins_ShouldRemoveTheOldUser()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsChangedAndTheOldUserHasNoMoreLogins_ShouldRemoveTheOldUser(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserIdIsForAPasswordUser_ShouldThrowAnInvalidOperationException()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsForAPasswordUser_ShouldThrowAnInvalidOperationException(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_Sqlite_IfTheUserLoginIdDoesNotExistAsUser_ShouldCreateTheUser()
		{
			await this.SaveUserLoginsAsync_IfTheUserLoginIdDoesNotExistAsUser_ShouldCreateTheUser(DatabaseProvider.Sqlite);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfAUserLoginExistAndTheUserIdIsNotNullAndAUserForTheUserIdDoesNotExist_ShouldCreateTheUserAndMoveTheUserLoginAndRemoveThePreviousLoginUser()
		{
			await this.SaveUserLoginAsync_IfAUserLoginExistAndTheUserIdIsNotNullAndAUserForTheUserIdDoesNotExist_ShouldCreateTheUserAndMoveTheUserLoginAndRemoveThePreviousLoginUser(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfAUserLoginExistAndTheUserIdIsNull_ShouldDoNothing()
		{
			await this.SaveUserLoginAsync_IfAUserLoginExistAndTheUserIdIsNull_ShouldDoNothing(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserAndUserLoginDoesNotExist_ShouldCreateTheUserAndUserLogin()
		{
			await this.SaveUserLoginAsync_IfTheUserAndUserLoginDoesNotExist_ShouldCreateTheUserAndUserLogin(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserAndUserLoginDoesNotExistAndIfTheUserIdIsNull_ShouldCreateTheUserAndUserLogin()
		{
			await this.SaveUserLoginAsync_IfTheUserAndUserLoginDoesNotExistAndIfTheUserIdIsNull_ShouldCreateTheUserAndUserLogin(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserIdIsChanged_ShouldChangeTheUserIdAndRemoveClaims()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsChanged_ShouldChangeTheUserIdAndRemoveClaims(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserIdIsChangedAndTheOldUserHasMoreLogins_ShouldNotRemoveTheOldUserButRemoveTheClaimsForTheOldUser()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsChangedAndTheOldUserHasMoreLogins_ShouldNotRemoveTheOldUserButRemoveTheClaimsForTheOldUser(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserIdIsChangedAndTheOldUserHasNoMoreLogins_ShouldRemoveTheOldUser()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsChangedAndTheOldUserHasNoMoreLogins_ShouldRemoveTheOldUser(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserIdIsForAPasswordUser_ShouldThrowAnInvalidOperationException()
		{
			await this.SaveUserLoginAsync_IfTheUserIdIsForAPasswordUser_ShouldThrowAnInvalidOperationException(DatabaseProvider.SqlServer);
		}

		[TestMethod]
		public async Task SaveUserLoginAsync_SqlServer_IfTheUserLoginIdDoesNotExistAsUser_ShouldCreateTheUser()
		{
			await this.SaveUserLoginsAsync_IfTheUserLoginIdDoesNotExistAsUser_ShouldCreateTheUser(DatabaseProvider.SqlServer);
		}

		protected internal virtual async Task SaveUserLoginsAsync_IfNothingIsChanged_ShouldDoNothing(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";

				// Prepare
				var autoSaveChanges = identityFacade.UserManager.Store.AutoSaveChanges;
				identityFacade.UserManager.Store.AutoSaveChanges = true;
				var user = new UserEntity { Id = id, UserName = "Test" };
				await identityFacade.UserManager.CreateAsync(user);
				var claims = new List<Claim>
				{
					new Claim("Claim-type-1", "Claim-value-1"),
					new Claim("Claim-type-2", "Claim-value-2"),
					new Claim("Claim-type-3", "Claim-value-3")
				};
				await identityFacade.UserManager.AddClaimsAsync(user, claims);
				await identityFacade.UserManager.AddLoginAsync(user, new UserLoginInfo("Provider-1", "User-identifier-1", "Provider-1"));
				identityFacade.UserManager.Store.AutoSaveChanges = autoSaveChanges;

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.AreEqual(3, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));

				await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = id, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.AreEqual(5, identityFacade.DatabaseContext.ChangeTracker.Entries().Count(entry => entry.State == EntityState.Unchanged));

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
				Assert.AreEqual(3, (await identityFacade.UserManager.GetClaimsAsync(user)).Count);
				Assert.IsNotNull(await identityFacade.UserManager.FindByLoginAsync("Provider-1", "User-identifier-1"));
			}
		}

		protected internal virtual async Task SaveUserLoginsAsync_IfTheUserLoginIdDoesNotExistAsUser_ShouldCreateTheUser(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);
				var identityFacade = await this.CreateIdentityFacadeAsync(serviceProvider);

				const string id = "32348cf4-1e8e-4a97-a185-b52cdd70b996";
				Assert.IsNull(await identityFacade.UserManager.FindByIdAsync(id));

				var result = await identityFacade.SaveUserLoginAsync(new UserLoginModel { Id = id, Provider = "Provider-1", UserIdentifier = "User-identifier-1" });
				Assert.IsTrue(result.Succeeded);
				Assert.AreEqual(2, await identityFacade.DatabaseContext.SaveChangesAsync());

				Assert.IsNotNull(await identityFacade.UserManager.FindByIdAsync(id));
			}
		}

		#endregion
	}
}