using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Security.Claims.Extensions
{
	public static class ClaimsSelectionContextExtension
	{
		#region Methods

		public static async Task<bool> AnySelectableClaimsAsync(this IClaimsSelectionContext claimsSelectionContext, ClaimsPrincipal claimsPrincipal)
		{
			var numberOfSelectableClaims = await claimsSelectionContext.NumberOfSelectableClaimsAsync(claimsPrincipal).ConfigureAwait(false);

			return numberOfSelectableClaims > 0;
		}

		public static async Task<int> NumberOfSelectableClaimsAsync(this IClaimsSelectionContext claimsSelectionContext, ClaimsPrincipal claimsPrincipal)
		{
			if(claimsSelectionContext == null)
				throw new ArgumentNullException(nameof(claimsSelectionContext));

			var numberOfSelectableClaims = 0;

			foreach(var selector in claimsSelectionContext.Selectors)
			{
				var claimsSelectionResult = await selector.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

				numberOfSelectableClaims += await claimsSelectionResult.NumberOfSelectableClaimsAsync().ConfigureAwait(false);
			}

			return numberOfSelectableClaims;
		}

		#endregion
	}
}