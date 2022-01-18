using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims.Configuration
{
	public class ClaimsSelectorLoaderOptions
	{
		#region Properties

		public virtual IDictionary<string, ClaimsSelectorOptions> ClaimsSelectors { get; } = new Dictionary<string, ClaimsSelectorOptions>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}
}