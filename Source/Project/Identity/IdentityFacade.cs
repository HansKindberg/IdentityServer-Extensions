using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using RegionOrebroLan.Security.Claims;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace HansKindberg.IdentityServer.Identity
{
	public class IdentityFacade : IIdentityFacade
	{
		#region Constructors

		public IdentityFacade(SignInManager<UserEntity> signInManager, UserManager userManager)
		{
			this.SignInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
			this.UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		#endregion

		#region Properties

		public virtual IdentityContext DatabaseContext => this.UserManager.DatabaseContext;
		protected internal virtual SignInManager<UserEntity> SignInManager { get; }
		protected internal virtual UserManager UserManager { get; }
		public virtual IQueryable<UserEntity> Users => this.UserManager.Users;

		#endregion

		#region Methods

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

		public virtual async Task<UserEntity> GetUserAsync(string userName)
		{
			return await this.UserManager.FindByNameAsync(userName);
		}

		public virtual async Task<UserEntity> GetUserAsync(string provider, string subject)
		{
			return await this.UserManager.FindByLoginAsync(provider, subject);
		}

		protected internal virtual async Task<UserEntity> ModelToEntityAsync(UserModel user)
		{
			return await Task.FromResult(user != null ? new UserEntity {Email = user.Email, EmailConfirmed = !string.IsNullOrWhiteSpace(user.Email), UserName = user.UserName} : null);
		}

		public virtual async Task<UserEntity> ResolveUserAsync(IClaimBuilderCollection claims, string provider, string subject)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(provider == null)
				throw new ArgumentNullException(nameof(provider));

			if(subject == null)
				throw new ArgumentNullException(nameof(subject));

			var user = await this.GetUserAsync(provider, subject);

			if(user != null)
				return user;

			var autoSaveChanges = this.UserManager.Store.AutoSaveChanges;
			this.UserManager.Store.AutoSaveChanges = true;

			try
			{
				user = new UserEntity
				{
					UserName = Guid.NewGuid().ToString()
				};

				var identityResult = await this.UserManager.CreateAsync(user);

				if(!identityResult.Succeeded)
					throw new InvalidOperationException($"Could not create user: {await this.CreateErrorMessageAsync(identityResult)}");

				if(claims.Any())
				{
					identityResult = await this.UserManager.AddClaimsAsync(user, claims.Build());

					if(!identityResult.Succeeded)
						throw new InvalidOperationException($"Could not add claims for user: {await this.CreateErrorMessageAsync(identityResult)}");
				}

				identityResult = await this.UserManager.AddLoginAsync(user, new UserLoginInfo(provider, subject, provider));

				if(!identityResult.Succeeded)
					throw new InvalidOperationException($"Could not add login for user: {await this.CreateErrorMessageAsync(identityResult)}");

				return user;
			}
			finally
			{
				this.UserManager.Store.AutoSaveChanges = autoSaveChanges;
			}
		}

		public virtual async Task<IdentityResult> SaveUserAsync(UserModel user)
		{
			if(user == null)
				throw new ArgumentNullException(nameof(user));

			var result = await this.ValidateUserAsync(user);

			if(!result.Succeeded)
				return result;

			var userEntity = await this.UserManager.FindByNameAsync(user.UserName);

			if(userEntity == null)
				return await this.UserManager.CreateAsync(await this.ModelToEntityAsync(user), user.Password);

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

		public virtual async Task<SignInResult> SignInAsync(string password, bool persistent, string userName)
		{
			return await this.SignInManager.PasswordSignInAsync(userName, password, persistent, true);
		}

		public virtual async Task SignOutAsync()
		{
			await this.SignInManager.SignOutAsync();
		}

		public virtual async Task<IdentityResult> ValidateUserAsync(UserModel user)
		{
			if(user == null)
				throw new ArgumentNullException(nameof(user));

			if(string.IsNullOrWhiteSpace(user.UserName))
				return IdentityResult.Failed(new IdentityError {Description = "The user-name can not be null, empty or whitespaces only."});

			if(user.Password == null)
				return IdentityResult.Failed(new IdentityError {Description = $"The password for user \"{user.UserName}\" can not be null."});

			var userEntity = await this.ModelToEntityAsync(user);

			var passwordValidationErrors = new List<IdentityError>();

			foreach(var passwordValidator in this.UserManager.PasswordValidators)
			{
				var identityResult = await passwordValidator.ValidateAsync(this.UserManager, userEntity, user.Password);

				if(!identityResult.Succeeded)
					passwordValidationErrors.AddRange(identityResult.Errors);
			}

			// ReSharper disable ConvertIfStatementToReturnStatement

			if(passwordValidationErrors.Any())
				return IdentityResult.Failed(new IdentityError {Description = $"The password \"{user.Password}\" for user \"{user.UserName}\" is invalid. {string.Join(" ", passwordValidationErrors.Select(identityError => identityError.Description))}"});

			// ReSharper restore ConvertIfStatementToReturnStatement

			return IdentityResult.Success;
		}

		#endregion
	}
}