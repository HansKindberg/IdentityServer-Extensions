using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using RegionOrebroLan.Security.Claims;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
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
		Task<UserEntity> GetUserAsync(string provider, string subject);
		Task<UserEntity> ResolveUserAsync(IClaimBuilderCollection claims, string provider, string subject);
		Task<IdentityResult> SaveUserAsync(UserModel user);
		Task<SignInResult> SignInAsync(string password, bool persistent, string userName);
		Task SignOutAsync();
		Task<IdentityResult> ValidateUserAsync(UserModel user);

		#endregion
	}
}