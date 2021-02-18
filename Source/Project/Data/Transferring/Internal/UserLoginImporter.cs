using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserLoginEntity = Microsoft.AspNetCore.Identity.IdentityUserLogin<string>;
using UserLoginModel = HansKindberg.IdentityServer.Identity.Models.UserLogin;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	// ReSharper disable All
	public class UserLoginImporter : IdentityPartialImporter<UserLoginModel>
	{
		#region Constructors

		public UserLoginImporter(IIdentityFacade facade, ILoggerFactory loggerFactory) : base(facade, loggerFactory) { }

		#endregion

		#region Properties

		protected internal override string ModelIdentifierName => nameof(UserLoginModel.Id);
		protected internal override Func<UserLoginModel, string> ModelIdentifierSelector => model => model.Id;

		#endregion

		#region Methods

		[SuppressMessage("Style", "IDE0082:'typeof' can be converted  to 'nameof'")]
		protected internal override async Task FilterOutDuplicateModelsAsync(IList<UserLoginModel> models, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await base.FilterOutDuplicateModelsAsync(models, result);

			var errorFormat = $"{typeof(UserLoginModel).Name}.{nameof(UserLoginModel.Provider)}+{nameof(UserLoginModel.UserIdentifier)} {{0}} has {{1}} duplicate{{2}}.";

			var copies = models.ToArray();
			models.Clear();

			foreach(var group in copies.GroupBy(model => (model.Provider.ToUpperInvariant(), model.UserIdentifier.ToUpperInvariant())))
			{
				if(group.Count() > 1)
					await this.AddErrorAsync(string.Format(null, errorFormat, group.Key, group.Count() - 1, group.Count() > 2 ? "s" : null), result);
				else
					models.Add(group.First());
			}
		}

		[SuppressMessage("Style", "IDE0082:'typeof' can be converted  to 'nameof'")]
		protected internal override async Task FilterOutInvalidModelsAsync(IList<UserLoginModel> models, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await base.FilterOutInvalidModelsAsync(models, result);
			await base.FilterOutInvalidModelsAsync(models, $"{typeof(UserLoginModel).Name}.{nameof(UserLoginModel.Provider)}", model => model.Provider, result);
			await base.FilterOutInvalidModelsAsync(models, $"{typeof(UserLoginModel).Name}.{nameof(UserLoginModel.UserIdentifier)}", model => model.UserIdentifier, result);

			var copies = models.ToArray();
			models.Clear();

			foreach(var userLogin in copies)
			{
				var user = await this.Facade.Users.FirstOrDefaultAsync(item => item.Id == userLogin.Id);

				if(user?.PasswordHash != null)
					await this.AddErrorAsync($"{typeof(UserLoginModel).Name}.{nameof(UserLoginModel.Id)} \"{userLogin.Id}\" exists as a user with a password.", result);
				else
					models.Add(userLogin);
			}
		}

		protected internal override async Task ImportAsync(IList<UserLoginModel> models, ImportOptions options, IDataImportResult result)
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
				var saveResult = await this.Facade.SaveUserLoginAsync(import);

				if(!saveResult.Succeeded)
					await this.AddErrorAsync(saveResult, result);
			}

			if(options.DeleteAllOthers)
			{
				var userLoginsToDelete = new List<UserLoginEntity>();

				foreach(var userLogin in this.DatabaseContext.UserLogins)
				{
					var keep = models.Any(import => import.Id.Equals(userLogin.UserId, StringComparison.OrdinalIgnoreCase) && import.Provider.Equals(userLogin.LoginProvider, StringComparison.OrdinalIgnoreCase) && import.UserIdentifier.Equals(userLogin.ProviderKey, StringComparison.OrdinalIgnoreCase));

					if(!keep)
						userLoginsToDelete.Add(userLogin);
				}

				var involvedUserIds = userLoginsToDelete.Select(userLogin => userLogin.UserId).ToHashSet(StringComparer.OrdinalIgnoreCase);

				this.DatabaseContext.UserLogins.RemoveRange(userLoginsToDelete);

				var userLoginUserIds = this.DatabaseContext.UserLogins.Select(userLogin => userLogin.UserId).ToHashSet(StringComparer.OrdinalIgnoreCase);

				var userToDeleteIds = involvedUserIds.Where(userId => !userLoginUserIds.Contains(userId)).ToHashSet(StringComparer.OrdinalIgnoreCase);
				var userToKeepIds = involvedUserIds.Where(userId => userLoginUserIds.Contains(userId)).ToHashSet(StringComparer.OrdinalIgnoreCase);

				this.DatabaseContext.UserClaims.RemoveRange(this.DatabaseContext.UserClaims.Where(userClaim => userToKeepIds.Contains(userClaim.UserId)));
				this.DatabaseContext.Users.RemoveRange(this.DatabaseContext.Users.Where(user => userToDeleteIds.Contains(user.Id)));

				foreach(var user in this.DatabaseContext.Users.Where(user => userToKeepIds.Contains(user.Id)))
				{
					user.SecurityStamp = Guid.NewGuid().ToString();
				}
			}

			await this.PopulateResultAsync(result);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.UserLogins.CountAsync());

			item.Relations.Add(typeof(UserEntity), await this.CreateResultItemAsync(await this.DatabaseContext.Users.CountAsync()));

			result.Items.Add(typeof(UserLoginEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedUserLoginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker,
				result.Items,
				new[] {typeof(UserLoginEntity)}.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is UserLoginEntity userLogin)
						deletedUserLoginIds.Add(userLogin.UserId);

					return false;
				}
			);

			var parent = result.Items[typeof(UserLoginEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker,
				parent.Relations,
				new[] {typeof(UserEntity)}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<UserEntity>(await this.DatabaseContext.Users.Where(user => deletedUserLoginIds.Contains(user.Id)).CountAsync(), parent.Relations);
		}

		#endregion
	}
	// ReSharper restore All
}