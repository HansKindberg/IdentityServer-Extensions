using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace HansKindberg.IdentityServer.Web.Configuration
{
	public class ExtendedForwardedHeadersOptions : ForwardedHeadersOptions
	{
		#region Properties

		public new virtual IList<KnownNetwork> KnownNetworks { get; } = new List<KnownNetwork>();
		public new virtual IList<string> KnownProxies { get; } = new List<string>();

		#endregion
	}
}