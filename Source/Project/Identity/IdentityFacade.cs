using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserLoginEntity = Microsoft.AspNetCore.Identity.IdentityUserLogin<string>;
using UserLoginModel = HansKindberg.IdentityServer.Identity.Models.UserLogin;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace HansKindberg.IdentityServer.Identity
{
	// ReSharper disable All
	/// <inheritdoc />
	public class IdentityFacade : IIdentityFacade
	{
		#region Constructors

		public IdentityFacade(ILoggerFactory loggerFactory, SignInManager<UserEntity> signInManager, UserManager userManager)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.SignInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
			this.UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		#endregion

		#region Properties

		public virtual IdentityContext DatabaseContext => this.UserManager.DatabaseContext;
		protected internal virtual ILogger Logger { get; }
		protected internal virtual SignInManager<UserEntity> SignInManager { get; }
		protected internal virtual UserManager UserManager { get; }
		public virtual IQueryable<UserEntity> Users => this.UserManager.Users;

		#endregion

		#region Methods

		protected internal virtual async Task<bool> ClaimsAreEqualAsync(IClaimBuilderCollection firstClaims, IList<Claim> secondClaims)
		{
			await Task.CompletedTask;

			if(firstClaims == null)
				return secondClaims == null;

			if(secondClaims == null)
				return false;

			if(firstClaims.Count != secondClaims.Count)
				return false;

			foreach(var firstClaim in firstClaims)
			{
				if(!secondClaims.Any(secondClaim => string.Equals(firstClaim.Type, secondClaim.Type, StringComparison.OrdinalIgnoreCase) && string.Equals(firstClaim.Value, secondClaim.Value, StringComparison.Ordinal)))
					return false;
			}

			return true;
		}

		protected internal virtual async Task<string> CreateErrorMessageAsync(IdentityResult identityResult)
		{
			if(identityResult == null)
				throw new ArgumentNullException(nameof(identityResult));

			return await Task.FromResult(string.Join(" ", identityResult.Errors.Select(identityError => identityError.Description)));
		}

		public virtual async Task<IdentityResult> DeleteUserAsync(UserEntity user)
		{
			if(user == null)
				throw new ArgumentNullException(nameof(user));

			return await this.UserManager.DeleteAsync(user);
		}

		protected internal virtual async Task<UserEntity> EnsureLoginUserAsync(UserLoginModel userLogin)
		{
			if(userLogin == null)
				throw new ArgumentNullException(nameof(userLogin));

			UserEntity loginUser = null;

			if(userLogin.Id != null)
				loginUser = await this.DatabaseContext.Users.FindAsync(userLogin.Id);

			if(loginUser == null)
			{
				loginUser = new UserEntity {Id = userLogin.Id ?? Guid.NewGuid().ToString(), UserName = Guid.NewGuid().ToString()};
				loginUser.NormalizedUserName = loginUser.UserName.ToUpperInvariant();
				await this.DatabaseContext.Users.AddAsync(loginUser);
			}
			else
			{
				if(loginUser.PasswordHash != null)
					throw new InvalidOperationException($"The user with id {loginUser.Id.ToStringRepresentation()} already exists with a password and can not be associated with a login.");

				loginUser.SecurityStamp = Guid.NewGuid().ToString();
			}

			return loginUser;
		}

		public virtual async Task<UserEntity> GetUserAsync(string userName)
		{
			return await this.UserManager.FindByNameAsync(userName);
		}

		public virtual async Task<UserEntity> GetUserAsync(string provider, string userIdentifier)
		{
			return await this.UserManager.FindByLoginAsync(provider, userIdentifier);
		}

		public virtual async Task<IEnumerable<UserLoginInfo>> GetUserLoginsAsync(string id)
		{
			return await this.UserManager.GetLoginsAsync(new UserEntity {Id = id});
		}

		public virtual async Task<UserEntity> ResolveUserAsync(IClaimBuilderCollection claims, string provider, string userIdentifier)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(provider == null)
				throw new ArgumentNullException(nameof(provider));

			if(userIdentifier == null)
				throw new ArgumentNullException(nameof(userIdentifier));

			var user = await this.GetUserAsync(provider, userIdentifier);
			var userClaims = (user != null ? await this.UserManager.GetClaimsAsync(user) : null) ?? new List<Claim>();

			if(user != null && await this.ClaimsAreEqualAsync(claims, userClaims))
				return user;

			var autoSaveChanges = this.UserManager.Store.AutoSaveChanges;
			this.UserManager.Store.AutoSaveChanges = true;
			var userExists = user != null;

			try
			{
				if(!userExists)
				{
					user = new UserEntity
					{
						UserName = Guid.NewGuid().ToString()
					};

					var identityResult = await this.UserManager.CreateAsync(user);

					if(!identityResult.Succeeded)
						throw new InvalidOperationException($"Could not create user: {await this.CreateErrorMessageAsync(identityResult)}");

					identityResult = await this.UserManager.AddLoginAsync(user, new UserLoginInfo(provider, userIdentifier, provider));

					if(!identityResult.Succeeded)
						throw new InvalidOperationException($"Could not add login for user: {await this.CreateErrorMessageAsync(identityResult)}");
				}

				await this.SaveClaimsAsync(claims, user);

				return user;
			}
			finally
			{
				this.UserManager.Store.AutoSaveChanges = autoSaveChanges;
			}
		}

		protected internal virtual async Task SaveClaimsAsync(IClaimBuilderCollection claims, UserEntity user)
		{
			var logPrefix = $"{this.GetType().FullName}.{nameof(this.SaveClaimsAsync)}:";
			this.Logger.LogDebugIfEnabled($"{logPrefix} user-id = {user?.Id.ToStringRepresentation()}, starting...");

			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(user == null)
				throw new ArgumentNullException(nameof(user));

			try
			{
				var comparer = StringComparer.Ordinal;
				var sortedClaims = new ClaimBuilderCollection();

				foreach(var claim in claims.OrderBy(claim => claim.Type, comparer).ThenBy(claim => claim.Value, comparer))
				{
					sortedClaims.Add(new ClaimBuilder {Type = claim.Type, Value = claim.Value});
				}

				var i = 0;
				var userClaimsToRemove = new List<IdentityUserClaim<string>>();

				foreach(var userClaim in this.DatabaseContext.UserClaims.Where(claim => claim.UserId == user.Id).OrderBy(claim => claim.Id))
				{
					if(sortedClaims.Count < i + 1)
					{
						userClaimsToRemove.Add(userClaim);
					}
					else
					{
						var claim = sortedClaims[i];
						const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

						if(!string.Equals(claim.Type, userClaim.ClaimType, comparison) || !string.Equals(claim.Value, userClaim.ClaimValue, comparison))
						{
							this.Logger.LogDebugIfEnabled($"{logPrefix} changing claim with id {userClaim.Id.ToStringRepresentation()} from type {userClaim.ClaimType.ToStringRepresentation()} to type {claim.Type.ToStringRepresentation()} and from value {userClaim.ClaimValue.ToStringRepresentation()} to value {claim.Value.ToStringRepresentation()}.");

							userClaim.ClaimType = claim.Type;
							userClaim.ClaimValue = claim.Value;
						}
					}

					i++;
				}

				if(userClaimsToRemove.Any())
				{
					this.Logger.LogDebugIfEnabled($"{logPrefix} removing {userClaimsToRemove.Count} claims with id's: {string.Join(", ", userClaimsToRemove.Select(userClaim => userClaim.Id))}");
					this.DatabaseContext.UserClaims.RemoveRange(userClaimsToRemove);
				}
				else if(sortedClaims.Count > i)
				{
					foreach(var claim in sortedClaims.Skip(i))
					{
						var claimToAdd = new IdentityUserClaim<string>
						{
							ClaimType = claim.Type,
							ClaimValue = claim.Value,
							UserId = user.Id
						};

						this.Logger.LogDebugIfEnabled($"{logPrefix} adding claim with type {claim.Type.ToStringRepresentation()} and value {claim.Value.ToStringRepresentation()}.");
						await this.DatabaseContext.UserClaims.AddAsync(claimToAdd);
					}
				}

				var savedChanges = await this.DatabaseContext.SaveChangesAsync();
				this.Logger.LogDebugIfEnabled($"{logPrefix} saved changes = {savedChanges}");
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not save claims for user.", exception);
			}
		}

		public virtual async Task<IdentityResult> SaveUserAsync(UserModel user)
		{
			if(user == null)
				throw new ArgumentNullException(nameof(user));

			var result = await this.ValidateUserAsync(user);

			if(!result.Succeeded)
				return result;

			try
			{
				var userEntity = user.Id != null ? await this.UserManager.FindByIdAsync(user.Id) : await this.UserManager.FindByNameAsync(user.UserName);

				if(userEntity == null)
					return await this.UserManager.CreateAsync(await this.UserModelToUserEntityAsync(user), user.Password);

				// This is to avoid a DbUpdateConcurrencyException to be thrown.
				// We are doing 3 transactions:
				// - Update
				// - Remove password
				// - Add password
				// We need to reset the concurrency-stamp after each transaction to avoid a DbUpdateConcurrencyException.
				var concurrencyStamp = userEntity.ConcurrencyStamp;

				userEntity.Email = user.Email;
				userEntity.EmailConfirmed = !string.IsNullOrWhiteSpace(user.Email);
				userEntity.UserName = user.UserName;

				result = await this.UserManager.UpdateAsync(userEntity);

				if(!result.Succeeded)
					return result;

				userEntity.ConcurrencyStamp = concurrencyStamp;

				result = await this.UserManager.RemovePasswordAsync(userEntity);

				if(!result.Succeeded)
					return result;

				userEntity.ConcurrencyStamp = concurrencyStamp;

				return await this.UserManager.AddPasswordAsync(userEntity, user.Password);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not save user with id {user.Id.ToStringRepresentation()} and user-name {user.UserName.ToStringRepresentation()}.", exception);
			}
		}

		public virtual async Task<IdentityResult> SaveUserLoginAsync(UserLoginModel userLogin, bool allowMultipleLoginsForUser = false)
		{
			if(userLogin == null)
				throw new ArgumentNullException(nameof(userLogin));

			if(userLogin.Provider == null)
				throw new ArgumentException("The user-login-provider can not be null.", nameof(userLogin));

			if(userLogin.UserIdentifier == null)
				throw new ArgumentException("The user-login-user-identifier can not be null.", nameof(userLogin));

			try
			{
				var existingUserLogin = await this.DatabaseContext.UserLogins.FirstOrDefaultAsync(item => item.LoginProvider == userLogin.Provider && item.ProviderKey == userLogin.UserIdentifier);

				if(existingUserLogin != null)
				{
					if(userLogin.Id != null && !string.Equals(userLogin.Id, existingUserLogin.UserId, StringComparison.OrdinalIgnoreCase))
					{
						var oldLoginUserId = existingUserLogin.UserId;
						var numberOfUserLoginsForOldLoginUserId = await this.DatabaseContext.UserLogins.CountAsync(item => item.UserId == oldLoginUserId);
						var newLoginUser = await this.EnsureLoginUserAsync(userLogin);
						existingUserLogin.UserId = newLoginUser.Id;

						if(numberOfUserLoginsForOldLoginUserId < 2)
						{
							this.DatabaseContext.Users.Remove(await this.DatabaseContext.Users.FindAsync(oldLoginUserId));
						}
						else
						{
							this.DatabaseContext.UserClaims.RemoveRange(this.DatabaseContext.UserClaims.Where(item => item.UserId == oldLoginUserId));
							(await this.DatabaseContext.Users.FindAsync(oldLoginUserId)).SecurityStamp = Guid.NewGuid().ToString();
						}
					}
				}
				else
				{
					var loginUser = await this.EnsureLoginUserAsync(userLogin);

					this.DatabaseContext.UserClaims.RemoveRange(this.DatabaseContext.UserClaims.Where(item => item.UserId == loginUser.Id));
					this.DatabaseContext.UserLogins.RemoveRange(this.DatabaseContext.UserLogins.Where(item => item.UserId == loginUser.Id));

					await this.DatabaseContext.UserLogins.AddAsync(new UserLoginEntity {LoginProvider = userLogin.Provider, ProviderDisplayName = userLogin.Provider, ProviderKey = userLogin.UserIdentifier, UserId = loginUser.Id});
				}

				if(!allowMultipleLoginsForUser && userLogin.Id != null)
				{
					var userLoginsToDelete = this.DatabaseContext.UserLogins.Where(item => item.UserId == userLogin.Id && item.LoginProvider != userLogin.Provider && item.ProviderKey != userLogin.UserIdentifier);

					if(userLoginsToDelete.Any())
					{
						this.DatabaseContext.UserLogins.RemoveRange(userLoginsToDelete);
						this.DatabaseContext.UserClaims.RemoveRange(this.DatabaseContext.UserClaims.Where(item => item.UserId == userLogin.Id));
						(await this.DatabaseContext.Users.FindAsync(userLogin.Id)).SecurityStamp = Guid.NewGuid().ToString();
					}
				}

				return IdentityResult.Success;
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not save user-login with id {userLogin.Id.ToStringRepresentation()}, provider {userLogin.Provider.ToStringRepresentation()} and user-identifier {userLogin.UserIdentifier.ToStringRepresentation()}.", exception);
			}
		}

		public virtual async Task<SignInResult> SignInAsync(string password, bool persistent, string userName)
		{
			return await this.SignInManager.PasswordSignInAsync(userName, password, persistent, true);
		}

		public virtual async Task SignOutAsync()
		{
			await this.SignInManager.SignOutAsync();
		}

		protected internal virtual async Task<UserLoginEntity> UserLoginModelToUserLoginEntityAsync(UserLoginModel userLogin)
		{
			UserLoginEntity userLoginEntity = null;

			if(userLogin != null)
				userLoginEntity = new UserLoginEntity {LoginProvider = userLogin.Provider, ProviderDisplayName = userLogin.Provider, ProviderKey = userLogin.UserIdentifier, UserId = userLogin.Id};

			return await Task.FromResult(userLoginEntity);
		}

		protected internal virtual async Task<UserEntity> UserModelToUserEntityAsync(UserModel user)
		{
			UserEntity userEntity = null;

			if(user != null)
			{
				userEntity = new UserEntity {Email = user.Email, EmailConfirmed = !string.IsNullOrWhiteSpace(user.Email), UserName = user.UserName};

				if(user.Id != null)
					userEntity.Id = user.Id;
			}

			return await Task.FromResult(userEntity);
		}

		public virtual async Task<IdentityResult> ValidateUserAsync(UserModel user)
		{
			if(user == null)
				throw new ArgumentNullException(nameof(user));

			if(string.IsNullOrWhiteSpace(user.UserName))
				return IdentityResult.Failed(new IdentityError {Description = "The user-name can not be null, empty or whitespaces only."});

			if(user.Password == null)
				return IdentityResult.Failed(new IdentityError {Description = $"The password for user \"{user.UserName}\" can not be null."});

			var userEntity = await this.UserModelToUserEntityAsync(user);

			var passwordValidationErrors = new List<IdentityError>();

			foreach(var passwordValidator in this.UserManager.PasswordValidators)
			{
				var identityResult = await passwordValidator.ValidateAsync(this.UserManager, userEntity, user.Password);

				if(!identityResult.Succeeded)
					passwordValidationErrors.AddRange(identityResult.Errors);
			}

			if(passwordValidationErrors.Any())
				return IdentityResult.Failed(new IdentityError {Description = $"The password \"{user.Password}\" for user \"{user.UserName}\" is invalid. {string.Join(" ", passwordValidationErrors.Select(identityError => identityError.Description))}"});

			return IdentityResult.Success;
		}

		#endregion
	}
	// ReSharper restore All
}