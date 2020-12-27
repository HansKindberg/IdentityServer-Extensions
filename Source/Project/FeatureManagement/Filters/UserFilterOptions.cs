using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.FeatureManagement.Filters
{
	public class UserFilterOptions
	{
		#region Properties

		/// <summary>
		/// User-name-patterns to use for evaluating the user-filter. Wildcards are allowed.
		/// </summary>
		public virtual ISet<string> Names { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Provider-patterns to use for evaluating the identity-provider for the user-filter. Wildcards are allowed.
		/// </summary>
		public virtual ISet<string> Providers { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}
}