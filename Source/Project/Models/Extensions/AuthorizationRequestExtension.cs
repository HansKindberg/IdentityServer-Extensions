using System;
using Duende.IdentityServer.Models;

namespace HansKindberg.IdentityServer.Models.Extensions
{
	public static class AuthorizationRequestExtension
	{
		#region Methods

		public static bool IsNativeClient(this AuthorizationRequest authorizationRequest)
		{
			return !(authorizationRequest?.RedirectUri ?? string.Empty).StartsWith("http", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}