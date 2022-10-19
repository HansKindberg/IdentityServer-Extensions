using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public class ClaimsSelectionInformation
	{
		#region Properties

		public virtual ISet<string> ClaimTypes { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
		public virtual IClaimsSelectionContext Context { get; set; }
		public virtual bool InProgress { get; set; }

		#endregion
	}
}