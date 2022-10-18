using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace HansKindberg.IdentityServer.Web.Authentication.Extensions
{
	public static class AuthenticateResultExtension
	{
		#region Methods

		public static async Task<bool> GetBooleanPropertyItemAsync(this AuthenticateResult authenticateResult, string key, bool defaultValue = false)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			var value = authenticateResult.Properties?.GetString(key);

			if(bool.TryParse(value, out var boolean) && boolean)
				return true;

			return await Task.FromResult(defaultValue).ConfigureAwait(false);
		}

		public static async Task<ISet<string>> GetClaimsSelectionClaimTypesAsync(this AuthenticateResult authenticateResult)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			var claimTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

			var claimTypesValue = authenticateResult.Properties?.GetString(AuthenticationKeys.ClaimsSelectionClaimTypes);

			// ReSharper disable InvertIf
			if(claimTypesValue != null)
			{
				foreach(var claimType in claimTypesValue.Split(','))
				{
					if(string.IsNullOrWhiteSpace(claimType))
						continue;

					claimTypes.Add(claimType.Trim());
				}
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(claimTypes).ConfigureAwait(false);
		}

		public static async Task<bool> GetClaimsSelectionHandledAsync(this AuthenticateResult authenticateResult)
		{
			return await authenticateResult.GetBooleanPropertyItemAsync(AuthenticationKeys.ClaimsSelectionHandled);
		}

		#endregion
	}
}