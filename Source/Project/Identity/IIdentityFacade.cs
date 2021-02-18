using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using RegionOrebroLan.Security.Claims;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserLoginModel = HansKindberg.IdentityServer.Identity.Models.UserLogin;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace HansKindberg.IdentityServer.Identity
{
	public interface IIdentityFacade
	{
		#region Properties

		IdentityContext DatabaseContext { get; }
		IQueryable<UserEntity> Users { get; }

		#endregion

		#region Methods

		Task<IdentityResult> DeleteUserAsync(UserEntity user);
		Task<UserEntity> GetUserAsync(string userName);
		Task<UserEntity> GetUserAsync(string provider, string userIdentifier);
		Task<IEnumerable<UserLoginInfo>> GetUserLoginsAsync(string id);
		Task<UserEntity> ResolveUserAsync(IClaimBuilderCollection claims, string provider, string userIdentifier);
		Task<IdentityResult> SaveUserAsync(UserModel user);

		/// <summary>
		/// Saves a user-login.
		/// </summary>
		/// <param name="userLogin">The model for the user-login.</param>
		/// <param name="allowMultipleLoginsForUser">If multiple user-logins are allowed for the same user-id or not.</param>
		Task<IdentityResult> SaveUserLoginAsync(UserLoginModel userLogin, bool allowMultipleLoginsForUser = false);

		Task<SignInResult> SignInAsync(string password, bool persistent, string userName);
		Task SignOutAsync();
		Task<IdentityResult> ValidateUserAsync(UserModel user);

		#endregion
	}
}