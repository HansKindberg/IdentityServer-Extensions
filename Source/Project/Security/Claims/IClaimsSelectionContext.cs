using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelectionContext
	{
		#region Properties

		string AuthenticationScheme { get; }
		IEnumerable<IClaimsSelector> Selectors { get; }
		Uri Url { get; }

		#endregion

		#region Methods

		Task<bool> AutomaticSelectionIsPossibleAsync(ClaimsPrincipal claimsPrincipal);
		Task<bool> IsAutomaticallySelectedAsync(ClaimsPrincipal claimsPrincipal);

		#endregion
	}
}