using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelectionContext
	{
		#region Properties

		string AuthenticationScheme { get; }
		IEnumerable<IClaimsSelector> Selectors { get; }
		Uri Url { get; }

		#endregion
	}
}