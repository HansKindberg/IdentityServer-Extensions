using System;
using Rsk.WsFederation.Configuration;

namespace HansKindberg.IdentityServer.WsFederation.Configuration.Extensions
{
	public static class WsFederationOptionsExtension
	{
		#region Methods

		public static void SetDefaults(this WsFederationOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));
		}

		#endregion
	}
}