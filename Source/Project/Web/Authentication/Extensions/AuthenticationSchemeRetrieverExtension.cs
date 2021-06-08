using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Extensions;

namespace HansKindberg.IdentityServer.Web.Authentication.Extensions
{
	public static class AuthenticationSchemeRetrieverExtension
	{
		#region Methods

		public static async Task<IDictionary<IAuthenticationScheme, AuthenticationSchemeOptions>> GetDiagnosticsAsync(this IAuthenticationSchemeRetriever authenticationSchemeRetriever, IServiceProvider serviceProvider)
		{
			if(authenticationSchemeRetriever == null)
				throw new ArgumentNullException(nameof(authenticationSchemeRetriever));

			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var diagnostics = new Dictionary<IAuthenticationScheme, AuthenticationSchemeOptions>();

			var schemes = await authenticationSchemeRetriever.ListAsync().ConfigureAwait(false);

			foreach(var scheme in schemes)
			{
				var options = await scheme.GetOptionsDiagnosticsAsync(serviceProvider).ConfigureAwait(false);

				diagnostics.Add(scheme, options);
			}

			return diagnostics;
		}

		#endregion
	}
}