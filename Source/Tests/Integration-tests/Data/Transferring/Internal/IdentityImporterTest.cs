using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Identity.Data;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace IntegrationTests.Data.Transferring.Internal
{
	[TestClass]
	public class IdentityImporterTest
	{
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
				.AddJsonFile($"Data/Transferring/Internal/Resources/IdentityImporter/{configurationFileName}.json", false, false);

			return await Task.FromResult(configurationBuilder.Build());
		}

		protected internal virtual async Task<IdentityImporter> CreateIdentityImporterAsync(IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new IdentityImporter(serviceProvider.GetRequiredService<IIdentityFacade>(), serviceProvider.GetRequiredService<ILoggerFactory>()));
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

		[SuppressMessage("Style", "IDE0057:Use range operator")]
		public async Task ImportAsyncChangesTest(DatabaseProvider databaseProvider)
		{
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				context.ApplicationBuilder.UseIdentity();
			}

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				Assert.IsFalse(await context.ServiceProvider.GetRequiredService<IIdentityFacade>().Users.AnyAsync());
			}

			var importOptions = new ImportOptions();
			var users = new List<UserModel>
			{
				new UserModel
				{
					Email = "alice@example.com",
					Id = "f6762ef5-d224-437a-9a67-459a9331266c",
					Password = "P@ssword12",
					UserName = "alice"
				},
				new UserModel
				{
					Email = "bob@example.com",
					Id = "efbe866d-d6ba-49cd-875a-36c52aa2339d",
					Password = "P@ssword12",
					UserName = "bob"
				}
			};

			// Save users.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityImporter = await this.CreateIdentityImporterAsync(context.ServiceProvider);

				var userImporter = identityImporter.Importers.OfType<UserImporter>().First();

				var result = new DataImportResult();

				await userImporter.ImportAsync(users, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(2, await identityImporter.CommitAsync());
			}

			// Test user properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityFacade = (IdentityFacade)context.ServiceProvider.GetRequiredService<IIdentityFacade>();

				var userEntities = identityFacade.Users.ToArray();
				Assert.AreEqual(2, userEntities.Length);

				foreach(var user in users)
				{
					var userEntity = await identityFacade.UserManager.FindByIdAsync(user.Id);

					Assert.AreEqual(user.Email, userEntity.Email);
					Assert.AreEqual(user.Id, userEntity.Id);
					Assert.AreEqual(user.UserName, userEntity.UserName);
					Assert.IsTrue(await identityFacade.UserManager.CheckPasswordAsync(userEntity, user.Password));
				}
			}

			// Save the same unchanged users again.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityImporter = await this.CreateIdentityImporterAsync(context.ServiceProvider);

				var userImporter = identityImporter.Importers.OfType<UserImporter>().First();

				var result = new DataImportResult();

				await userImporter.ImportAsync(users, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				// Even if the properties are not changed, the password must be re-changed.
				Assert.AreEqual(2, await identityImporter.CommitAsync());
			}

			// Test user properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityFacade = (IdentityFacade)context.ServiceProvider.GetRequiredService<IIdentityFacade>();

				var userEntities = identityFacade.Users.ToArray();
				Assert.AreEqual(2, userEntities.Length);

				foreach(var user in users)
				{
					var userEntity = await identityFacade.UserManager.FindByIdAsync(user.Id);

					Assert.AreEqual(user.Email, userEntity.Email);
					Assert.AreEqual(user.Id, userEntity.Id);
					Assert.AreEqual(user.UserName, userEntity.UserName);
					Assert.IsTrue(await identityFacade.UserManager.CheckPasswordAsync(userEntity, user.Password));
				}
			}

			foreach(var user in users)
			{
				user.Email = $"changed-{user.Email}";
				user.Password = $"changed-{user.Password}";
			}

			// Save changed users.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityImporter = await this.CreateIdentityImporterAsync(context.ServiceProvider);

				var userImporter = identityImporter.Importers.OfType<UserImporter>().First();

				var result = new DataImportResult();

				await userImporter.ImportAsync(users, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(2, await identityImporter.CommitAsync());
			}

			// Test user properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityFacade = (IdentityFacade)context.ServiceProvider.GetRequiredService<IIdentityFacade>();

				var userEntities = identityFacade.Users.ToArray();
				Assert.AreEqual(2, userEntities.Length);

				foreach(var user in users)
				{
					var userEntity = await identityFacade.UserManager.FindByIdAsync(user.Id);

					Assert.AreEqual(user.Email, userEntity.Email);
					Assert.AreEqual(user.Id, userEntity.Id);
					Assert.AreEqual(user.UserName, userEntity.UserName);
					Assert.IsTrue(await identityFacade.UserManager.CheckPasswordAsync(userEntity, user.Password));
				}
			}

			foreach(var user in users)
			{
				user.Email = user.Email.ToUpperInvariant();
				user.Password = "c" + user.Password.ToUpperInvariant().Substring(1);
				user.UserName = user.UserName.ToUpperInvariant();
			}

			// Save changed users.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityImporter = await this.CreateIdentityImporterAsync(context.ServiceProvider);

				var userImporter = identityImporter.Importers.OfType<UserImporter>().First();

				var result = new DataImportResult();

				await userImporter.ImportAsync(users, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
				Assert.AreEqual(2, await identityImporter.CommitAsync());
			}

			// Test user properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityFacade = (IdentityFacade)context.ServiceProvider.GetRequiredService<IIdentityFacade>();

				var userEntities = identityFacade.Users.ToArray();
				Assert.AreEqual(2, userEntities.Length);

				foreach(var user in users)
				{
					var userEntity = await identityFacade.UserManager.FindByIdAsync(user.Id);

					Assert.AreEqual(user.Email, userEntity.Email);
					Assert.AreEqual(user.Id, userEntity.Id);
					Assert.AreEqual(user.UserName, userEntity.UserName);
					Assert.IsTrue(await identityFacade.UserManager.CheckPasswordAsync(userEntity, user.Password));
				}
			}

			var unSavedUsers = new List<UserModel>();

			// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach(var user in users)
			{
				unSavedUsers.Add(new UserModel
				{
					Email = $"unsaved-{user.Email}",
					Id = user.Id,
					Password = $"unsaved-{user.Password}",
					UserName = $"unsaved-{user.UserName}"
				});
			}
			// ReSharper restore ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

			// Do not commit for changed users.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityImporter = await this.CreateIdentityImporterAsync(context.ServiceProvider);

				var userImporter = identityImporter.Importers.OfType<UserImporter>().First();

				var result = new DataImportResult();

				await userImporter.ImportAsync(unSavedUsers, importOptions, result);

				Assert.IsFalse(result.Errors.Any());
			}

			// Test user properties.
			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var identityFacade = (IdentityFacade)context.ServiceProvider.GetRequiredService<IIdentityFacade>();

				var userEntities = identityFacade.Users.ToArray();
				Assert.AreEqual(2, userEntities.Length);

				foreach(var user in users)
				{
					var userEntity = await identityFacade.UserManager.FindByIdAsync(user.Id);

					Assert.AreEqual(user.Email, userEntity.Email);
					Assert.AreEqual(user.Id, userEntity.Id);
					Assert.AreEqual(user.UserName, userEntity.UserName);
					Assert.IsTrue(await identityFacade.UserManager.CheckPasswordAsync(userEntity, user.Password));
				}
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(DatabaseProvider databaseProvider)
		{
			var importOptions = new ImportOptions {DeleteAllOthers = true};

			using(var context = new Context(databaseProvider: databaseProvider))
			{
				var serviceProvider = context.ServiceProvider;
				await DatabaseHelper.MigrateDatabaseAsync(serviceProvider);

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
					expectedSavedChanges: 1,
					expectedUsersAfterImport: 1
				);
			}
		}

		protected internal virtual async Task ImportAsyncScenarioTest(Context context, string fileName, ImportOptions importOptions, int expectedNumberOfErrors = 0, int expectedSavedChanges = 0, int expectedUsersAfterImport = 0)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			if(importOptions == null)
				throw new ArgumentNullException(nameof(importOptions));

			var serviceProvider = context.ServiceProvider;

			using(var serviceScope = serviceProvider.CreateScope())
			{
				var identityContext = serviceScope.ServiceProvider.GetRequiredService<IdentityContext>();
				var identityImporter = await this.CreateIdentityImporterAsync(serviceScope.ServiceProvider);
				var result = new DataImportResult();
				await identityImporter.ImportAsync(await this.CreateConfigurationAsync(fileName, context.FileProvider), importOptions, result);
				if(!result.Errors.Any() && !importOptions.VerifyOnly)
					result.SavedChanges = await identityContext.SaveChangesAsync();
				Assert.AreEqual(expectedNumberOfErrors, result.Errors.Count);
				Assert.AreEqual(expectedSavedChanges, result.SavedChanges);
			}

			using(var serviceScope = serviceProvider.CreateScope())
			{
				var databaseContext = serviceScope.ServiceProvider.GetRequiredService<IdentityContext>();

				Assert.IsFalse(await databaseContext.RoleClaims.AnyAsync());
				Assert.IsFalse(await databaseContext.Roles.AnyAsync());
				Assert.IsFalse(await databaseContext.UserClaims.AnyAsync());
				Assert.IsFalse(await databaseContext.UserLogins.AnyAsync());
				Assert.IsFalse(await databaseContext.UserRoles.AnyAsync());
				Assert.IsFalse(await databaseContext.UserTokens.AnyAsync());
				Assert.AreEqual(expectedUsersAfterImport, await databaseContext.Users.CountAsync());
			}
		}

		[TestInitialize]
		public async Task InitializeAsync()
		{
			await this.CleanupAsync();
		}

		#endregion
	}
}