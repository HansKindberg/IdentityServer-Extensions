using System;
using System.Linq;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Security.Claims.Extensions
{
	public static class ClaimsSelectionResultExtension
	{
		#region Methods

		public static async Task<bool> AnySelectableClaimsAsync(this IClaimsSelectionResult claimsSelectionResult)
		{
			var numberOfSelectableClaims = await claimsSelectionResult.NumberOfSelectableClaimsAsync().ConfigureAwait(false);

			return numberOfSelectableClaims > 0;
		}

		public static async Task<int> NumberOfSelectableClaimsAsync(this IClaimsSelectionResult claimsSelectionResult)
		{
			if(claimsSelectionResult == null)
				throw new ArgumentNullException(nameof(claimsSelectionResult));

			var numberOfSelectableClaims = claimsSelectionResult.Selectables.SelectMany(selectable => selectable.Value).Count();

			return await Task.FromResult(numberOfSelectableClaims).ConfigureAwait(false);
		}

		public static bool Selected(this IClaimsSelectionResult claimsSelectionResult)
		{
			if(claimsSelectionResult == null)
				throw new ArgumentNullException(nameof(claimsSelectionResult));

			return claimsSelectionResult.Selectables.SelectMany(selectable => selectable.Value).Any(selectableClaim => selectableClaim.Selected);
		}

		#endregion
	}
}