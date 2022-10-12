using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using RegionOrebroLan.Security.Claims;

namespace TestHelpers.Security.Claims
{
	public static class ClaimsPrincipalFactory
	{
		#region Methods

		public static ClaimsPrincipal Create(IClaimBuilderCollection claims = null, string authenticationType = "Test", string nameType = JwtClaimTypes.Name, string roleType = JwtClaimTypes.Role)
		{
			return Create((claims ?? new ClaimBuilderCollection()).Build(), authenticationType, nameType, roleType);
		}

		public static ClaimsPrincipal Create(IEnumerable<Claim> claims, string authenticationType = "Test", string nameType = JwtClaimTypes.Name, string roleType = JwtClaimTypes.Role)
		{
			return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType, nameType, roleType));
		}

		public static async Task<ClaimsPrincipal> CreateAsync(IClaimBuilderCollection claims = null, string authenticationType = "Test", string nameType = JwtClaimTypes.Name, string roleType = JwtClaimTypes.Role)
		{
			return await Task.FromResult(Create(claims, authenticationType, nameType, roleType)).ConfigureAwait(false);
		}

		public static async Task<ClaimsPrincipal> CreateAsync(IEnumerable<Claim> claims, string authenticationType = "Test", string nameType = JwtClaimTypes.Name, string roleType = JwtClaimTypes.Role)
		{
			return await Task.FromResult(Create(claims, authenticationType, nameType, roleType)).ConfigureAwait(false);
		}

		#endregion
	}
}