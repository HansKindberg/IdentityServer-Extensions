using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace HansKindberg.IdentityServer.Security.Claims.Configuration
{
	public class ClaimsSelectorOptions
	{
		#region Properties

		/// <summary>
		/// Map to authentication-schemes. A dictionary where the key is a name for an authentication-scheme or a name-pattern for authentication-schemes. The key support wildcards. The value is the index used to decide the order of the entry.
		/// </summary>
		public virtual IDictionary<string, int> AuthenticationSchemes { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		public virtual bool Enabled { get; set; } = true;
		public virtual IConfigurationSection Options { get; set; }
		public virtual Type Type { get; set; }

		#endregion
	}
}