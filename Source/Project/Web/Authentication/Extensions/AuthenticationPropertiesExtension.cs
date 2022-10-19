using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace HansKindberg.IdentityServer.Web.Authentication.Extensions
{
	public static class AuthenticationPropertiesExtension
	{
		#region Methods

		public static async Task<bool> GetBooleanPropertyItemAsync(this AuthenticationProperties authenticationProperties, string key, bool defaultValue = false)
		{
			if(authenticationProperties == null)
				throw new ArgumentNullException(nameof(authenticationProperties));

			var value = authenticationProperties.GetString(key);

			if(bool.TryParse(value, out var boolean) && boolean)
				return true;

			return await Task.FromResult(defaultValue).ConfigureAwait(false);
		}

		public static async Task<ISet<string>> GetClaimsSelectionClaimTypesAsync(this AuthenticationProperties authenticationProperties)
		{
			if(authenticationProperties == null)
				throw new ArgumentNullException(nameof(authenticationProperties));

			var claimTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

			var claimTypesValue = authenticationProperties.GetString(AuthenticationKeys.ClaimsSelectionClaimTypes);

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

		public static async Task<bool> GetClaimsSelectionInProgressAsync(this AuthenticationProperties authenticationProperties)
		{
			return await authenticationProperties.GetBooleanPropertyItemAsync(AuthenticationKeys.ClaimsSelectionInProgress).ConfigureAwait(false);
		}

		public static async Task SetClaimsSelectionClaimTypesAsync(this AuthenticationProperties authenticationProperties, ISet<string> claimTypes)
		{
			if(authenticationProperties == null)
				throw new ArgumentNullException(nameof(authenticationProperties));

			if(claimTypes == null)
				throw new ArgumentNullException(nameof(claimTypes));

			await Task.CompletedTask.ConfigureAwait(false);

			var sortedClaimTypes = new SortedSet<string>(claimTypes, StringComparer.OrdinalIgnoreCase);

			if(!sortedClaimTypes.Any())
				sortedClaimTypes.Add(Guid.NewGuid().ToString());

			authenticationProperties.SetString(AuthenticationKeys.ClaimsSelectionClaimTypes, string.Join(',', sortedClaimTypes));
		}

		public static async Task SetClaimsSelectionInProgressAsync(this AuthenticationProperties authenticationProperties, bool value)
		{
			if(authenticationProperties == null)
				throw new ArgumentNullException(nameof(authenticationProperties));

			await Task.CompletedTask.ConfigureAwait(false);

			var itemValue = value ? true.ToString() : null;

			authenticationProperties.SetString(AuthenticationKeys.ClaimsSelectionInProgress, itemValue);
		}

		#endregion
	}
}