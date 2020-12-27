using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class UserImporter : PartialImporter<UserModel>
	{
		#region Constructors

		public UserImporter(IIdentityFacade facade, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
		}

		#endregion

		#region Properties

		protected internal virtual IdentityContext DatabaseContext => this.Facade.DatabaseContext;
		protected internal virtual IIdentityFacade Facade { get; }
		protected internal override string ModelIdentifierName => "UserName";
		protected internal override Func<UserModel, string> ModelIdentifierSelector => model => model.UserName;

		#endregion

		#region Methods

		protected internal virtual async Task AddErrorAsync(IdentityResult identityResult, IDataImportResult result)
		{
			if(identityResult == null)
				throw new ArgumentNullException(nameof(identityResult));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await this.AddErrorAsync(await this.CreateErrorMessageAsync(identityResult), result);
		}

		protected internal virtual async Task<string> CreateErrorMessageAsync(IdentityResult identityResult)
		{
			if(identityResult == null)
				throw new ArgumentNullException(nameof(identityResult));

			return await Task.FromResult(string.Join(" ", identityResult.Errors.Select(identityError => identityError.Description)));
		}

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
				var importIdentifiers = models.Select(import => import.UserName).ToHashSet(StringComparer.OrdinalIgnoreCase);

				foreach(var user in this.Facade.Users.Where(user => !importIdentifiers.Contains(user.UserName)))
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

		protected internal virtual async Task<UserEntity> ModelToEntityAsync(UserModel user)
		{
			return await Task.FromResult(user != null ? new UserEntity {Email = user.Email, UserName = user.UserName} : null);
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
}