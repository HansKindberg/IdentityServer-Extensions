using System;
using System.Linq;

namespace HansKindberg.IdentityServer.Security.Claims.Extensions
{
	public static class ClaimsSelectionResultExtension
	{
		#region Methods

		public static bool Selected(this IClaimsSelectionResult claimsSelectionResult)
		{
			if(claimsSelectionResult == null)
				throw new ArgumentNullException(nameof(claimsSelectionResult));

			return claimsSelectionResult.Selectables.SelectMany(selectable => selectable.Value).Any(selectableClaim => selectableClaim.Selected);
		}

		#endregion
	}
}