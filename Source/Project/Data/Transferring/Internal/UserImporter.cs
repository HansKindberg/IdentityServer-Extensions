using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	// ReSharper disable All
	public class UserImporter : IdentityPartialImporter<UserModel>
	{
		#region Constructors

		public UserImporter(IIdentityFacade facade, ILoggerFactory loggerFactory) : base(facade, loggerFactory) { }

		#endregion

		#region Properties

		protected internal override string ModelIdentifierName => nameof(UserModel.Id);
		protected internal override Func<UserModel, string> ModelIdentifierSelector => model => model.Id;

		#endregion

		#region Methods

		[SuppressMessage("Style", "IDE0082:'typeof' can be converted  to 'nameof'")]
		protected internal override async Task FilterOutDuplicateModelsAsync(IList<UserModel> models, IDataImportResult result)
		{
			await base.FilterOutDuplicateModelsAsync(models, result);
			await this.FilterOutDuplicateModelsAsync(models, $"{typeof(UserModel).Name}.{nameof(UserModel.UserName)}", model => model.UserName, result);
		}

		[SuppressMessage("Style", "IDE0082:'typeof' can be converted  to 'nameof'")]
		protected internal override async Task FilterOutInvalidModelsAsync(IList<UserModel> models, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await base.FilterOutInvalidModelsAsync(models, result);

			var copies = models.ToArray();
			models.Clear();

			foreach(var user in copies)
			{
				var userLogins = await this.Facade.GetUserLoginsAsync(user.Id);

				if(userLogins.Any())
				{
					await this.AddErrorAsync($"{typeof(UserModel).Name}.{nameof(UserModel.Id)} \"{user.Id}\" has external logins.", result);

					continue;
				}

				var validationResult = await this.Facade.ValidateUserAsync(user);

				if(validationResult.Succeeded)
					models.Add(user);
				else
					await this.AddErrorAsync(validationResult, result);
			}
		}

		protected internal override async Task ImportAsync(IList<UserModel> models, ImportOptions options, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await this.InitializeResultAsync(result);

			await this.FilterOutInvalidModelsAsync(models, result);
			await this.FilterOutDuplicateModelsAsync(models, result);

			foreach(var import in models)
			{
				var saveResult = await this.Facade.SaveUserAsync(import);

				if(!saveResult.Succeeded)
					await this.AddErrorAsync(saveResult, result);
			}

			if(options.DeleteAllOthers)
			{
				var importIdentifiers = models.Select(import => import.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

				foreach(var user in this.Facade.Users.Where(user => !importIdentifiers.Contains(user.Id) && user.PasswordHash != null))
				{
					var deleteResult = await this.Facade.DeleteUserAsync(user);

					if(!deleteResult.Succeeded)
						await this.AddErrorAsync(deleteResult, result);
				}
			}

			await this.PopulateResultAsync(result);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.Users.CountAsync());

			item.Relations.Add(typeof(IdentityUserClaim<string>), await this.CreateResultItemAsync(await this.DatabaseContext.UserClaims.CountAsync()));
			item.Relations.Add(typeof(IdentityUserLogin<string>), await this.CreateResultItemAsync(await this.DatabaseContext.UserLogins.CountAsync()));
			item.Relations.Add(typeof(IdentityUserRole<string>), await this.CreateResultItemAsync(await this.DatabaseContext.UserRoles.CountAsync()));
			item.Relations.Add(typeof(IdentityUserToken<string>), await this.CreateResultItemAsync(await this.DatabaseContext.UserTokens.CountAsync()));

			result.Items.Add(typeof(UserEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker,
				result.Items,
				new[] {typeof(UserEntity)}.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is UserEntity user)
						deletedUserIds.Add(user.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(UserEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker,
				parent.Relations,
				new[] {typeof(IdentityUserClaim<string>), typeof(IdentityUserLogin<string>), typeof(IdentityUserRole<string>), typeof(IdentityUserToken<string>)}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<IdentityUserClaim<string>>(await this.DatabaseContext.UserClaims.Where(userClaim => deletedUserIds.Contains(userClaim.UserId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<IdentityUserLogin<string>>(await this.DatabaseContext.UserLogins.Where(userLogin => deletedUserIds.Contains(userLogin.UserId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<IdentityUserRole<string>>(await this.DatabaseContext.UserRoles.Where(userRole => deletedUserIds.Contains(userRole.UserId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<IdentityUserToken<string>>(await this.DatabaseContext.UserTokens.Where(userToken => deletedUserIds.Contains(userToken.UserId)).CountAsync(), parent.Relations);
		}

		#endregion
	}
	// ReSharper restore All
}