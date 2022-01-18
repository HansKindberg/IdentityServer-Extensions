using System.Collections.Generic;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelectorLoader
	{
		#region Methods

		Task<IEnumerable<IClaimsSelector>> GetClaimsSelectorsAsync(string authenticationScheme);

		#endregion
	}
}